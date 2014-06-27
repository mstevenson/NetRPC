using System;
using System.Net;
using NetMQ;
using NetMQ.zmq;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace NetRPC
{
	public class NetworkContext : IDisposable
	{
		public enum ContextType {
			Client,
			Server
		}

		ContextType type;

		public event Action<NetworkMessage> ReceivedMessage;

		public bool IsRunning { get; protected set; }

		NetMQContext mqContext;
		Thread sendReceiveThread;
		Queue<NetMQMessage> _sendMessages = new Queue<NetMQMessage> ();
		AutoResetEvent _sendSignal;
		IPAddress ip;
		ISerializer argumentSerializer;
		int port;

		public NetworkContext (ContextType type, ISerializer argumentSerializer)
		{
			this.type = type;
			this.argumentSerializer = argumentSerializer;
			mqContext = NetMQContext.Create ();
			_sendSignal = new AutoResetEvent (false);
		}

		public void Connect (IPAddress ip, int port)
		{
			if (IsRunning) {
				return;
			}
			this.ip = ip;
			this.port = port;
			IsRunning = true;

			sendReceiveThread = new Thread (SendWorker);
			sendReceiveThread.IsBackground = true;
			sendReceiveThread.Start ();
		}

		public void Close ()
		{
			if (!IsRunning) {
				return;
			}
			IsRunning = false;
		}


		#region Network Threads

		void SendWorker ()
		{
			using (var frontend = mqContext.CreateDealerSocket ())
			using (var backend = mqContext.CreatePullSocket ())
			{
				var address = string.Format ("tcp://{0}:{1}", ip, port);
				if (type == ContextType.Client) {
					frontend.Connect (address);
				} else {
					frontend.Bind (address);
				}
				if (type == ContextType.Client) {
					backend.Bind ("inproc://ClientSendBackend");
				} else {
					backend.Bind ("inproc://ServerSendBackend");
				}

				StartNetworkThread (SendBackendWorker);

				NetMQ.Poller poller = new NetMQ.Poller ();
				poller.AddSocket (frontend);
				poller.AddSocket (backend);

				frontend.ReceiveReady += (sender, e) => {
					var m = e.Socket.ReceiveMessage ();
					if (type == ContextType.Client) {
						Console.WriteLine ("<-- Client received message: " + m.First.ConvertToString ());
					} else {
						Console.WriteLine ("<-- Server received message: " + m.First.ConvertToString ());
					}

					string methodIdentifier = m.Pop ().ConvertToString ();
					var args = new object[m.FrameCount];
					if (m.FrameCount > 0) {
						for (int i = 0; i < args.Length; i++) {
							var argBytes = m.Pop ().ToByteArray ();
							args[i] = argumentSerializer.Deserialize (argBytes);
						}
					}
					var networkMessage = new NetworkMessage (methodIdentifier, args);

					if (ReceivedMessage != null) {
						ReceivedMessage (networkMessage);
					}
				};

				// forward the message from the backend to the frontend socket
				backend.ReceiveReady += (sender, e) => {
					frontend.SendMessage (backend.ReceiveMessage ());
				};

				poller.Start ();
			}
		}

		// Watch for client-side messages to send, push them to a frontend socket
		void SendBackendWorker ()
		{
			using (var socket = mqContext.CreatePushSocket ()) {
				if (type == ContextType.Client) {
					socket.Connect ("inproc://ClientSendBackend");
				} else {
					socket.Connect ("inproc://ServerSendBackend");
				}
				while (IsRunning) {
					_sendSignal.WaitOne ();
					lock (_sendSignal) {
						while (_sendMessages.Count > 0) {
							var m = _sendMessages.Dequeue ();
							socket.SendMessage (m);
							if (type == ContextType.Client) {
								Console.WriteLine ("--> Client message sent: " + m.First.ConvertToString ());
							} else {
								Console.WriteLine ("--> Server message sent: " + m.First.ConvertToString ());
							}
						}
					}
				}
			}
		}

		void StartNetworkThread (ThreadStart thread)
		{
			var t = new Thread (thread);
			t.IsBackground = true;
			t.Start ();
		}

		#endregion

		public void SendMessage (NetworkMessage message)
		{
			if (!IsRunning) {
				return;
			}
			lock (_sendMessages) {
				var netmqMessage = new NetMQMessage ();
				netmqMessage.Append (message.message);
				var serializedArgs = GetSerializedArgs (message);
				if (message.args != null) {
					foreach (var arg in serializedArgs) {
						netmqMessage.Append (arg);
					}
				}
				_sendMessages.Enqueue (netmqMessage);
				_sendSignal.Set ();
			}
		}

		byte[][] GetSerializedArgs (NetworkMessage message)
		{
			var serializedArgs = new byte[message.args.Length][];
			for (int i = 0; i < message.args.Length; i++) {
				serializedArgs[i] = argumentSerializer.Serialize (message.args [i]);
			}
			return serializedArgs;
		}

		public void Dispose ()
		{
			Close ();
			// Can't dispose of the context until all sockets have been disposed
			//mqContext.Dispose ();
		}
	}
}


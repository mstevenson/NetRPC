using System;
using NetRPC;
using System.Net;
using System.Threading;
using NetMQ;

namespace NetRPC
{
	class MainClass
	{
		public class MessageResponderExample
		{
			[Message ("String/Print")]
			public void PrintAString (string thingToPrint)
			{
				Console.WriteLine (thingToPrint);
			}
		}

		public static void Main (string[] args)
		{
			BinarySerializer serializer = new BinarySerializer ();

			using (var server = new NetworkContext (NetworkContext.ContextType.Server, serializer))
			using (var client = new NetworkContext (NetworkContext.ContextType.Client, serializer))
			{
				// Set up client and server
				server.Connect (IPAddress.Loopback, 1701);
				client.Connect (IPAddress.Loopback, 1701);

				// Set up RPC method dispatcher and register an object with it
				var rpcDispatcher = new RPCDispatcher (client);
				var exampleObj = new MessageResponderExample ();
				rpcDispatcher.RegisterMessageBindings (exampleObj);

				// Wait for both client and server to become available
				Thread.Sleep (100);

				// Send a message from server to client to invoke an RPC
				server.SendMessage (new NetworkMessage ("String/Print", "hello"));
			}
		}
	}
}

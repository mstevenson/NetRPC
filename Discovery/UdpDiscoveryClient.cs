﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace NetRPC
{
	/// <summary>
	/// Listens for UDP beacon packets from UdpDiscoveryServer.
	/// </summary>
	public class UdpDiscoveryClient : IDisposable
	{
		const ushort multicastPort = 6767;
		const string multicastAddress = "239.17.0.1";

		UdpClient udpClient;
		IPEndPoint endPoint;
		Thread networkThread;

		List<ServerBeacon> servers = new List<ServerBeacon> ();

		public bool IsRunning { get; protected set; }

		public event Action<ServerBeacon> ServerFound;
		public event Action<ServerBeacon> ServerLost;

		void OnServerFound (ServerBeacon server)
		{
			if (ServerFound != null) {
				ServerFound (server);
			}
		}

		void OnServerLost (ServerBeacon server)
		{
			if (ServerLost != null) {
				ServerLost (server);
			}
		}

		public ServerBeacon[] AvailableServers
		{
			get {
				if (servers.Count == 0) {
					return new ServerBeacon[0];
				} else {
					return servers.ToArray ();
				}
			}
		}

		public UdpDiscoveryClient ()
		{
			udpClient = new UdpClient ();
			var localEndPoint = new IPEndPoint (IPAddress.Any, multicastPort);
			udpClient.Client.Bind (localEndPoint);
			var ip = IPAddress.Parse (multicastAddress);
			udpClient.MulticastLoopback = true;
			udpClient.JoinMulticastGroup (ip);
		}

		public void Start ()
		{
			Console.WriteLine ("Server Discovery Started: " + multicastAddress + ":" + multicastPort);
			IsRunning = true;
			networkThread = new Thread (new ThreadStart (NetworkThread));
			networkThread.IsBackground = true;
			networkThread.Start ();
		}

		public void Stop ()
		{
			Console.WriteLine ("Server Discovery Stopped");
			IsRunning = false;
			if (udpClient != null) {
				udpClient.Close ();
			}
		}

		void NetworkThread ()
		{
			//			try {
			while (IsRunning) {
				byte[] data = udpClient.Receive (ref endPoint);
				ServerBeacon serverInfo = ServerBeacon.FromBytes (data);
				serverInfo.address = endPoint.Address;
				if (serverInfo != null) {
					bool serverExists = false;
					foreach (var s in servers) {
						if (s == serverInfo) {
							s.lastSeen = DateTime.Now;
							serverExists = true;
						}
					}
					if (!serverExists) {
						servers.Add (serverInfo);
						OnServerFound (serverInfo);
					}
					servers = PruneLostServers (servers);
				}
			}
			//			} catch {}
			IsRunning = false;
		}

		List<ServerBeacon> PruneLostServers (List<ServerBeacon> servers, int timeout = 3500)
		{
			List<ServerBeacon> pruned = new List<ServerBeacon> ();
			var now = DateTime.Now;
			foreach (var serverInfo in servers) {
				var span = serverInfo.lastSeen.Subtract (now);
				if (span.Milliseconds < timeout) {
					pruned.Add (serverInfo);
				} else {
					OnServerLost (serverInfo);
				}
			}
			return pruned;
		}

		public void Dispose ()
		{
			IsRunning = false;
			udpClient.Close ();
		}

	}
}


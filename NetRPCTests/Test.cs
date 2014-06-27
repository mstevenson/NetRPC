using NUnit.Framework;
using System;
using System.Threading;
using System.Net;
using NetRPC;

[TestFixture]
public class CommunicationTests
{
	[Test]
	public void StartServer ()
	{
		var serializer = new BinarySerializer ();
		var server = new NetworkContext (NetworkContext.ContextType.Server, serializer);
		server.Connect (IPAddress.Loopback, 1702);
		Thread.Sleep (100);
		Assert.IsTrue (server.IsRunning);
		server.Dispose ();
		Thread.Sleep (100);
	}

	[Test]
	public void StartClient ()
	{
		var serializer = new BinarySerializer ();
		var client = new NetworkContext (NetworkContext.ContextType.Client, serializer);
		client.Connect (IPAddress.Loopback, 1701);
		Thread.Sleep (100);
		Assert.IsTrue (client.IsRunning);
		client.Dispose ();
		Thread.Sleep (100);
	}
//
//	[Test]
//	public void CreateMessage ()
//	{
//		var message = new NetworkMessage ("TestMessage");
//		Assert.AreEqual ("TestMessage", message.message);
//	}

	[Test]
	public void SendMessageFromServerToClient ()
	{
		var serializer = new BinarySerializer ();
		using (var server = new NetworkContext (NetworkContext.ContextType.Server, serializer))
		using (var client = new NetworkContext (NetworkContext.ContextType.Client, serializer))
		{
			server.Connect (IPAddress.Loopback, 1701);
			Thread.Sleep (100);

			client.Connect (IPAddress.Loopback, 1701);
			NetworkMessage receivedMessage = null;
			client.ReceivedMessage += m => receivedMessage = m;
			Thread.Sleep (100);

			server.SendMessage (new NetworkMessage ("TestMessage"));
			Thread.Sleep (100);

			Assert.IsNotNull (receivedMessage);

//			var message = new NetworkMessage ("TestMessage");
//			server.SendMessage (message);
		}
	}
}

//[TestFixture]
//public class UdpServerTests
//{
//	ServerBeacon beacon;
//	UdpDiscoveryServer server;
//
//	[SetUp]
//	public void Init ()
//	{
//		beacon = new ServerBeacon ();
//		server = new UdpDiscoveryServer (beacon);
//	}
//
//	[Test]
//	public void TestStartStopServer ()
//	{
//		Assert.IsFalse (server.IsRunning);
//		server.Start ();
//		Assert.IsTrue (server.IsRunning);
//		server.Stop ();
//		Assert.IsFalse (server.IsRunning);
//	}
//}

//[TestFixture]
//public class UdpClientTests
//{
//	ServerBeacon beacon;
//	UdpDiscoveryServer server;
//
//	[SetUp]
//	public void Init ()
//	{
//		beacon = new ServerBeacon ();
//		server = new UdpDiscoveryServer (beacon);
//		server.Start ();
//		Thread.Sleep (100);
//	}
//
////	[Test]
////	public void TestInitializeUdpClient ()
////	{
////		var client = new UdpDiscoveryClient ();
////		Assert.IsFalse (client.IsRunning);
////		client.Start ();
////		Thread.Sleep (20);
////		Assert.IsTrue (client.IsRunning);
////		client.Stop ();
////		Thread.Sleep (20);
////		Assert.IsFalse (client.IsRunning);
////		client.Dispose ();
////	}
//
////	[Test]
////	public void TestFindServer ()
////	{
////		Assert.Fail ("boo");
////		var client = new UdpDiscoveryClient ();
////		client.Start ();
////		Thread.Sleep (200);
////		var available = client.AvailableServers;
////		Assert.AreEqual (1, client.AvailableServers.Length, "Expected 1 available server, but found " + available);
////		client.Dispose ();
////	}
//}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using NetMQ;
using NetMQ.zmq;
using System.Threading;

namespace NetRPC
{
	public class RPCDispatcher
	{
		/// <summary>
		/// Indicates whether the RPCDispatcher has been initialized with a NetworkContext.
		/// </summary>
		public bool IsInitialized { get; private set; }

		List<NetworkContext> contexts;

		public RPCDispatcher (NetworkContext context)
		{
			contexts = new List<NetworkContext> ();

			AddContext (context);
			IsInitialized = true;
		}

		/// <summary>
		/// Add a network context through which this dispatcher will send and receive messages.
		/// </summary>
		/// <param name="context">Context.</param>
		public void AddContext (NetworkContext context)
		{
			contexts.Add (context);
			context.ReceivedMessage += HandleContextReceivedMessage;
		}

		public void RemoveContext (NetworkContext context)
		{
			context.ReceivedMessage -= HandleContextReceivedMessage;
			contexts.Remove (context);
		}

		void HandleContextReceivedMessage (NetworkMessage networkMessage)
		{
			// Invoke specific method on target system
			var bindings = messageBindings.Where (b => b.messageAttribute == networkMessage.message);
			foreach (var b in bindings) {
				b.Invoke (networkMessage.args);
			}
		}


		MethodInvocationBinding[] messageBindings;
//		List<MethodInvocationBinding> outletBindings = new List<MethodInvocationBinding> ();

		/// <summary>
		/// Discover the given responder's methods marked with a MessageAttribute.
		/// When a matching message is received by this dispatcher, it will invoke the method.
		/// </summary>
		public void RegisterMessageBindings (object obj)
		{
			var type = obj.GetType ();
			var allMethods = type.GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreReturn);
			messageBindings = allMethods
				.Where (m => m.GetCustomAttributes (typeof(MessageAttribute), true).Length > 0)
				.Select (method => new MethodInvocationBinding (obj, method))
				.ToArray ();
		}

//		public void RegisterOutletBindings (object responder)
//		{
////			outletBindings.Add (responder.NetworkName, GetOutletBindings (responder));
//		}

//		/// <summary>
//		/// Binds string names to a type's methods that are marked with OutletAttribute.
//		/// </summary>
//		MethodInvocationBinding[] GetOutletBindings (object obj)
//		{
////			var type = obj.GetType ();
////			var allProperties = type.GetProperties (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
////			var outlets = allProperties.Where (f => f.GetCustomAttributes (typeof(OutletAttribute), true).Length > 0).ToArray ();
////			return outlets.Select (property => new MethodInvocationBinding (obj, property.GetSetMethod ())).ToArray ();
//		}


//		/// <summary>
//		/// Send a message
//		/// </summary>
//		public void SendMessage (string message, params object[] args)
//		{
//			if (!IsInitialized) {
//				throw new Exception ("Network context is not initialized");
//			}
//			foreach (var context in contexts) {
//				var m = new NetworkMessage (message, args);
//				context.SendMessage (m);
//			}
//		}
//
	}
}


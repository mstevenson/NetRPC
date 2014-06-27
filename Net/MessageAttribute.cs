using System;

namespace NetRPC
{
	/// <summary>
	/// A method that may be invoked by the given string name via an RPC call.
	/// </summary>
	[AttributeUsage (AttributeTargets.Method)]
	public class MessageAttribute : Attribute
	{
		public readonly string name;

		public MessageAttribute (string messageName)
		{
			this.name = messageName;
		}
	}
}


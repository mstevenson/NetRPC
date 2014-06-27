using System;

namespace NetRPC
{
	public class NetworkMessage
	{
		public readonly string message;
		public readonly object[] args;

		public NetworkMessage (string message, params object[] args)
		{
			this.message = message;
			this.args = args;
		}
	}
}


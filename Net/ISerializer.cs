using System;

namespace NetRPC
{
	public interface ISerializer
	{
		byte[] Serialize (object arg);
		object Deserialize (byte[] bytes);
	}
}


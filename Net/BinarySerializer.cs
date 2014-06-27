using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetRPC
{
	public class BinarySerializer : ISerializer
	{
		public byte[] Serialize (object arg)
		{
			var formatter = new BinaryFormatter ();
			using (var ms = new MemoryStream ()) {
				formatter.Serialize (ms, arg);
				return ms.ToArray ();
			}
		}

		public object Deserialize (byte[] bytes)
		{
			var formatter = new BinaryFormatter ();
			using (var ms = new MemoryStream (bytes)) {
				var obj = formatter.Deserialize (ms);
				return obj;
			}
		}
	}
}


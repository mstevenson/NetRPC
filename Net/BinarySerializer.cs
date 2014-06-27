using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetRPC
{
	/// <summary>
	/// Example object serializer. It's recommended not to use binary serialization
	/// outside of the same process since it is unlikely to be compabible across
	/// platforms or CLR versions.
	/// </summary>
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


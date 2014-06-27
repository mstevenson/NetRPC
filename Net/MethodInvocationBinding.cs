using System;
using System.Reflection;

namespace NetRPC
{
	/// <summary>
	/// Stores a reference to an object and one its instance methods. The method can be easily
	/// invoked at a later time with newly supplied arguments by calling Invoke.
	/// </summary>
	public class MethodInvocationBinding
	{
		public readonly object obj;
		public readonly MethodInfo method;
		public readonly string messageAttribute; // MessageAttribute string that the associated method should respond to

		public MethodInvocationBinding (object obj, MethodInfo method)
		{
			this.obj = obj;
			this.method = method;
			var attrib = method.GetCustomAttributes (typeof(MessageAttribute), true)[0] as MessageAttribute;
			this.messageAttribute = attrib.name;
		}

		public void Invoke (params object[] args)
		{
			try {
				method.Invoke (obj, args);
			} catch (System.Reflection.TargetParameterCountException e) {
				throw new Exception ("Message parameters do not match the message '" + method.Name + "'");
			}
		}
	}
}

using System;
using System.Reflection;

namespace V8.AddIn
{
	internal enum MessageTypes : ushort
	{
		None = 1000,
		Ordinary,
		Attention,
		Important,
		VeryImportant,
		Info,
		Fail,
		MsgboxAttention,
		MsgboxInfo,
		MsgboxFail
	}
	
	internal sealed class V8Context
	{
		private static object m_Context;
		public IAsyncEvent AsyncEvent
		{
			get
			{
				IAsyncEvent result = null;
				if (V8Context.m_Context != null)
				{
					result = (IAsyncEvent)V8Context.m_Context;
				}
				return result;
			}
		}
		public object Context
		{
			get
			{
				return V8Context.m_Context;
			}
		}
		private V8Context()
		{
		}
		internal V8Context(object context)
		{
			V8Context.m_Context = context;
		}
		public static V8Context CreateV8Context()
		{
			return new V8Context();
		}
		public void V8Message(MessageTypes MsgType, string MsgText, string Source)
		{
			if (string.IsNullOrEmpty(Source))
			{
				Source = typeof(V8Context).Assembly.GetName().Name;
			}
			if (V8Context.m_Context != null)
			{
				object[] args = new object[]
				{
					(ushort)MsgType, 
					Source, 
					MsgText, 
					0
				};
				V8Context.m_Context.GetType().InvokeMember("AddError", BindingFlags.InvokeMethod, null, V8Context.m_Context, args);
				return;
			}
			throw new Exception(MsgText);
		}
		public void V8Message(MessageTypes MsgType, string MsgText)
		{
			this.V8Message(MsgType, MsgText, null);
		}
	}

}

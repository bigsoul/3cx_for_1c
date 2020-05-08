using System;
using System.Runtime.InteropServices;

namespace V8.AddIn
{
	/// <summary>
	/// Служебный класс, поддерживающий функционирование приложения как внешней компоненты 1С:Предприятия. Не предназначен для прямого использования.
	/// </summary>
	[ComVisible(true), Guid("bc631c98-2f0b-49b9-b722-b7e223e46059")]
	public abstract class InitAddIn : IInitDone
	{
		private int m_Version;
		protected InitAddIn(int Version)
		{
			this.m_Version = Version;
		}
		void IInitDone.Init([MarshalAs(UnmanagedType.IDispatch)] object pConnection)
		{
			new V8Context(pConnection);
		}
		void IInitDone.Done()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
		void IInitDone.GetInfo([MarshalAs(UnmanagedType.SafeArray)] ref object[] pInfo)
		{
			pInfo.SetValue(this.m_Version, 0);
		}
	}
}

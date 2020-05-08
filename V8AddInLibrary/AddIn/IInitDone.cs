using System;
using System.Runtime.InteropServices;

namespace V8.AddIn
{
	/// <summary>
	/// Служебный интерфейс, поддерживающий функционирование приложения как внешней компоненты 1С:Предприятия. Не предназначен для прямого использования.
	/// </summary>
	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("AB634001-F13D-11d0-A459-004095E1DAEA")]
	public interface IInitDone
	{
		void Init([MarshalAs(UnmanagedType.IDispatch)] object pConnection);
		void Done();
		void GetInfo([MarshalAs(UnmanagedType.SafeArray)] ref object[] pInfo);
	}
}

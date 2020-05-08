using System;
using System.Runtime.InteropServices;

namespace V8.AddIn
{
	/// <summary>
	/// Служебный интерфейс, поддерживающий функционирование приложения как внешней компоненты 1С:Предприятия. Не предназначен для прямого использования.
	/// </summary>
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("ab634004-f13d-11d0-a459-004095e1daea"), ComVisible(true)]
	public interface IAsyncEvent
	{
		void SetEventBufferDepth(int lDepth);
		void GetEventBufferDepth(ref int plDepth);
		void ExternalEvent([MarshalAs(UnmanagedType.BStr)] string bstrSource, [MarshalAs(UnmanagedType.BStr)] string bstrMessage, [MarshalAs(UnmanagedType.BStr)] string bstrData);
		void CleanBuffer();
	}
}

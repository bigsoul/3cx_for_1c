using System;
using System.Runtime.InteropServices;

namespace V8.AddIn
{
	/// <summary>
	/// Служебный интерфейс, поддерживающий функционирование приложения как внешней компоненты 1С:Предприятия. Не предназначен для прямого использования.
	/// </summary>
	[Guid("AB634003-F13D-11d0-A459-004095E1DAEA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
	public interface ILanguageExtender
	{
		void RegisterExtensionAs([MarshalAs(UnmanagedType.BStr)] ref string bstrExtensionName);
		void GetNProps(ref int plProps);
		void FindProp([MarshalAs(UnmanagedType.BStr)] string bstrPropName, ref int plPropNum);
		void GetPropName(int lPropNum, int lPropAlias, [MarshalAs(UnmanagedType.BStr)] ref string pbstrPropName);
		void GetPropVal(int lPropNum, ref object pvarPropVal);
		void SetPropVal(int lPropNum, ref object varPropVal);
		void IsPropReadable(int lPropNum, [MarshalAs(UnmanagedType.Bool)] ref bool pboolPropRead);
		void IsPropWritable(int lPropNum, [MarshalAs(UnmanagedType.Bool)] ref bool pboolPropWrite);
		void GetNMethods(ref int plMethods);
		void FindMethod([MarshalAs(UnmanagedType.BStr)] string bstrMethodName, ref int plMethodNum);
		void GetMethodName(int lMethodNum, int lMethodAlias, [MarshalAs(UnmanagedType.BStr)] ref string pbstrMethodName);
		void GetNParams(int lMethodNum, ref int plParams);
		void GetParamDefValue(int lMethodNum, int lParamNum, ref object pvarParamDefValue);
		void HasRetVal(int lMethodNum, [MarshalAs(UnmanagedType.Bool)] ref bool pboolRetValue);
		void CallAsProc(int lMethodNum, [MarshalAs(UnmanagedType.SafeArray)] ref Array paParams);
		void CallAsFunc(int lMethodNum, ref object pvarRetValue, [MarshalAs(UnmanagedType.SafeArray)] ref Array paParams);
	}
}

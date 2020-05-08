using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace V8.AddIn
{
	/// <summary>
	/// Служебный класс, поддерживающий функционирование приложения как внешней компоненты 1С:Предприятия. Не предназначен для прямого использования.
	/// </summary>
	[Guid("43295454-83da-49a0-beca-58a9f6ac1ef0"), ComVisible(true)]
	public abstract class LanguageExtenderAddIn : InitAddIn, ILanguageExtender
	{
		private string m_Name;
		private PropertyInfo[] m_Properties;
		private MethodInfo[] m_Methods;
		private object m_Wrapper;
		private void InitWrapperInfo(Type WrapperType, BindingFlags flags)
		{
			this.m_Name = WrapperType.Name;
			this.m_Properties = WrapperType.GetProperties(flags);
			this.m_Methods = WrapperType.GetMethods(flags);
		}
		protected LanguageExtenderAddIn(Type WrapperType, int Version) : base(Version)
		{
			ConstructorInfo constructor = WrapperType.GetConstructor(Type.EmptyTypes);
			if (constructor == null)
			{
				this.InitWrapperInfo(WrapperType, BindingFlags.Static | BindingFlags.Public);
				return;
			}
			this.m_Wrapper = constructor.Invoke(null);
			this.InitWrapperInfo(WrapperType, BindingFlags.Instance | BindingFlags.Public);
		}
		void ILanguageExtender.RegisterExtensionAs([MarshalAs(UnmanagedType.BStr)] ref string bstrExtensionName)
		{
			bstrExtensionName = this.m_Name;
		}
		void ILanguageExtender.GetNProps(ref int plProps)
		{
			plProps = this.m_Properties.GetLength(0);
		}
		void ILanguageExtender.FindProp([MarshalAs(UnmanagedType.BStr)] string bstrPropName, ref int plPropNum)
		{
			plPropNum = 0;
			Type typeFromHandle = typeof(AliasAttribute);
			for (int i = 0; i <= this.m_Properties.GetUpperBound(0); i++)
			{
				AliasAttribute aliasAttribute = (AliasAttribute)Attribute.GetCustomAttribute(this.m_Properties[i], typeFromHandle);
				if (this.m_Properties[i].Name.ToUpper() == bstrPropName.ToUpper() || (aliasAttribute != null && aliasAttribute.AliasName.ToUpper() == bstrPropName.ToUpper()))
				{
					plPropNum = i + 1;
					return;
				}
			}
		}
		void ILanguageExtender.GetPropName(int lPropNum, int lPropAlias, [MarshalAs(UnmanagedType.BStr)] ref string pbstrPropName)
		{
			PropertyInfo propertyInfo = this.m_Properties[lPropNum - 1];
			pbstrPropName = propertyInfo.Name;
			if (lPropAlias == 1)
			{
				AliasAttribute aliasAttribute = (AliasAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(AliasAttribute));
				if (aliasAttribute != null)
				{
					pbstrPropName = aliasAttribute.AliasName;
				}
			}
		}
		void ILanguageExtender.GetPropVal(int lPropNum, ref object pvarPropVal)
		{
			PropertyInfo propertyInfo = this.m_Properties[lPropNum - 1];
			try
			{
				pvarPropVal = propertyInfo.GetValue(this.m_Wrapper, null);
			}
			catch //(Exception ex)
			{
				//V8Context v8Context = V8Context.CreateV8Context();
        //        if (ex.InnerException!=null)
				    //v8Context.V8Message(MessageTypes.Fail, ex.InnerException.Message, ex.InnerException.Source);
        //        else
        //            v8Context.V8Message(MessageTypes.Fail, ex.Message, ex.Source);
			}
		}
		void ILanguageExtender.SetPropVal(int lPropNum, ref object varPropVal)
		{
			PropertyInfo propertyInfo = this.m_Properties[lPropNum - 1];
			try
			{
				propertyInfo.SetValue(this.m_Wrapper, varPropVal, null);
			}
			catch //(Exception ex)
			{
				//V8Context v8Context = V8Context.CreateV8Context();
    //            if (ex.InnerException != null)
    //              v8Context.V8Message(MessageTypes.Fail, ex.InnerException.Message, ex.InnerException.Source);
    //            else
    //              v8Context.V8Message(MessageTypes.Fail, ex.Message, ex.Source);
			}
		}
		void ILanguageExtender.IsPropReadable(int lPropNum, [MarshalAs(UnmanagedType.Bool)] ref bool pboolPropRead)
		{
			pboolPropRead = this.m_Properties[lPropNum - 1].CanRead;
		}
		void ILanguageExtender.IsPropWritable(int lPropNum, [MarshalAs(UnmanagedType.Bool)] ref bool pboolPropWrite)
		{
			pboolPropWrite = this.m_Properties[lPropNum - 1].CanWrite;
		}
		void ILanguageExtender.GetNMethods(ref int plMethods)
		{
			plMethods = this.m_Methods.GetLength(0);
		}
		void ILanguageExtender.FindMethod([MarshalAs(UnmanagedType.BStr)] string bstrMethodName, ref int plMethodNum)
		{
			plMethodNum = 0;
			for (int i = 0; i <= this.m_Methods.GetUpperBound(0); i++)
			{
				AliasAttribute aliasAttribute = (AliasAttribute)Attribute.GetCustomAttribute(this.m_Methods[i], typeof(AliasAttribute));
				if (this.m_Methods[i].Name.ToUpper() == bstrMethodName.ToUpper() || (aliasAttribute != null && aliasAttribute.AliasName.ToUpper() == bstrMethodName.ToUpper()))
				{
					plMethodNum = i + 1;
					return;
				}
			}
		}
		void ILanguageExtender.GetMethodName(int lMethodNum, int lMethodAlias, [MarshalAs(UnmanagedType.BStr)] ref string pbstrMethodName)
		{
			MethodInfo methodInfo = this.m_Methods[lMethodNum - 1];
			pbstrMethodName = methodInfo.Name;
			if (lMethodAlias == 1)
			{
				AliasAttribute aliasAttribute = (AliasAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(AliasAttribute));
				if (aliasAttribute != null)
				{
					pbstrMethodName = aliasAttribute.AliasName;
				}
			}
		}
		void ILanguageExtender.GetNParams(int lMethodNum, ref int plParams)
		{
			MethodInfo methodInfo = this.m_Methods[lMethodNum - 1];
			plParams = methodInfo.GetParameters().GetLength(0);
		}
		void ILanguageExtender.GetParamDefValue(int lMethodNum, int lParamNum, ref object pvarParamDefValue)
		{
			ParameterInfo element = this.m_Methods[lMethodNum - 1].GetParameters()[lParamNum];
			HasDefaultValueAttribute hasDefaultValueAttribute = (HasDefaultValueAttribute)Attribute.GetCustomAttribute(element, typeof(HasDefaultValueAttribute));
			if (hasDefaultValueAttribute != null)
			{
				pvarParamDefValue = hasDefaultValueAttribute.DefaultValue;
			}
		}
		void ILanguageExtender.HasRetVal(int lMethodNum, [MarshalAs(UnmanagedType.Bool)] ref bool pboolRetValue)
		{
			MethodInfo methodInfo = this.m_Methods[lMethodNum - 1];
			pboolRetValue = (methodInfo.ReturnParameter.ParameterType != typeof(void));
		}
		void ILanguageExtender.CallAsProc(int lMethodNum, [MarshalAs(UnmanagedType.SafeArray)] ref Array paParams)
		{
			MethodInfo methodInfo = this.m_Methods[lMethodNum - 1];
			try
			{
				methodInfo.Invoke(this.m_Wrapper, (object[])paParams);
			}
			catch //(Exception ex)
			{
				//V8Context v8Context = V8Context.CreateV8Context();
    //            if (ex.InnerException != null)
    //              v8Context.V8Message(MessageTypes.Fail, ex.InnerException.Message, ex.InnerException.Source);
    //            else
    //              v8Context.V8Message(MessageTypes.Fail, ex.Message, ex.Source);
			}
		}
		void ILanguageExtender.CallAsFunc(int lMethodNum, ref object pvarRetValue, [MarshalAs(UnmanagedType.SafeArray)] ref Array paParams)
		{
			MethodInfo methodInfo = this.m_Methods[lMethodNum - 1];
			try
			{
				pvarRetValue = methodInfo.Invoke(this.m_Wrapper, (object[])paParams);
			}
			catch //(Exception ex)
			{
				//V8Context v8Context = V8Context.CreateV8Context();
    //            if (ex.InnerException != null)
    //              v8Context.V8Message(MessageTypes.Fail, ex.InnerException.Message, ex.InnerException.Source);
    //            else
    //              v8Context.V8Message(MessageTypes.Fail, ex.Message, ex.Source);
			}
		}
	}
}

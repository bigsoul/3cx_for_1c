using System;

namespace V8.AddIn
{
	/// <summary>
	/// Служебный класс, поддерживающий функционирование приложения как внешней компоненты 1С:Предприятия. Не предназначен для прямого использования.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class AliasAttribute : Attribute
	{
		public readonly string AliasName;
		public AliasAttribute(string value)
		{
			this.AliasName = value;
		}
	}
}

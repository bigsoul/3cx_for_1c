using System;

namespace V8.AddIn
{
	/// <summary>
	/// Служебный класс, поддерживающий функционирование приложения как внешней компоненты 1С:Предприятия. Не предназначен для прямого использования.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class HasDefaultValueAttribute : Attribute
	{
		public readonly object DefaultValue;
		public HasDefaultValueAttribute(object value)
		{
			this.DefaultValue = value;
		}
	}
}

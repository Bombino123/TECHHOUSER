using System;
using System.ComponentModel;

namespace Microsoft.Win32.TaskScheduler;

internal class DefaultValueExAttribute : DefaultValueAttribute
{
	public DefaultValueExAttribute(Type type, string value)
		: base(null)
	{
		try
		{
			if (type == typeof(Version))
			{
				SetValue(new Version(value));
			}
			else
			{
				SetValue(TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value));
			}
		}
		catch
		{
		}
	}
}

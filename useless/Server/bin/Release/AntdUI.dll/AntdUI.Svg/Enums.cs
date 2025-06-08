using System;

namespace AntdUI.Svg;

public static class Enums
{
	public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, IConvertible
	{
		try
		{
			result = (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase: true);
			return true;
		}
		catch
		{
			result = default(TEnum);
			return false;
		}
	}
}

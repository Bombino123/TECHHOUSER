using System.Data.Entity.SqlServer.Resources;

namespace System.Data.Entity.SqlServer.Utilities;

internal class Check
{
	public static T NotNull<T>(T value, string parameterName) where T : class
	{
		if (value == null)
		{
			throw new ArgumentNullException(parameterName);
		}
		return value;
	}

	public static T? NotNull<T>(T? value, string parameterName) where T : struct
	{
		if (!value.HasValue)
		{
			throw new ArgumentNullException(parameterName);
		}
		return value;
	}

	public static string NotEmpty(string value, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException(Strings.ArgumentIsNullOrWhitespace(parameterName));
		}
		return value;
	}
}

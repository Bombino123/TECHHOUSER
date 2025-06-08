using System.Diagnostics;

namespace System.Data.Entity.SqlServer.Utilities;

internal class DebugCheck
{
	[Conditional("DEBUG")]
	public static void NotNull<T>(T value) where T : class
	{
	}

	[Conditional("DEBUG")]
	public static void NotNull<T>(T? value) where T : struct
	{
	}

	[Conditional("DEBUG")]
	public static void NotEmpty(string value)
	{
	}
}

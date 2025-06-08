using System;

namespace AntdUI.Svg.ExtensionMethods;

public static class UriExtensions
{
	public static Uri? ReplaceWithNullIfNone(this Uri uri)
	{
		if (uri == null)
		{
			return null;
		}
		if (!string.Equals(uri.ToString(), "none", StringComparison.OrdinalIgnoreCase))
		{
			return uri;
		}
		return null;
	}
}

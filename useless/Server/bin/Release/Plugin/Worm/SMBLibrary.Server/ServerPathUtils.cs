using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class ServerPathUtils
{
	public static string GetRelativeServerPath(string path)
	{
		if (path.StartsWith("\\\\"))
		{
			int num = path.IndexOf('\\', 2);
			if (num > 0)
			{
				return path.Substring(num);
			}
			return string.Empty;
		}
		return path;
	}

	public static string GetRelativeSharePath(string path)
	{
		int num = GetRelativeServerPath(path).IndexOf('\\', 1);
		if (num > 0)
		{
			return path.Substring(num);
		}
		return "\\";
	}

	public static string GetShareName(string path)
	{
		string text = GetRelativeServerPath(path);
		if (text.StartsWith("\\"))
		{
			text = text.Substring(1);
		}
		int num = text.IndexOf("\\");
		if (num >= 0)
		{
			text = text.Substring(0, num);
		}
		return text;
	}
}

using System.IO;

namespace Worm2.Helper;

internal class Methods
{
	public static long CheckLeghtFile(string path)
	{
		return new FileInfo(path).Length;
	}
}

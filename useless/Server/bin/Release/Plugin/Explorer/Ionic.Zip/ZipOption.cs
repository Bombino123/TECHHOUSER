using System.Runtime.InteropServices;

namespace Ionic.Zip;

[ComVisible(true)]
public enum ZipOption
{
	Default = 0,
	Never = 0,
	AsNecessary = 1,
	Always = 2
}

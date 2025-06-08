using System.Runtime.InteropServices;

namespace Toolbelt.Drawing.Win32;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ICONFILEHEAD
{
	public ushort Reserved;

	public ushort Type;

	public ushort Count;
}

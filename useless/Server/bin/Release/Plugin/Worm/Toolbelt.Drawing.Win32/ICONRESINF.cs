using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Toolbelt.Drawing.Win32;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[DebuggerDisplay("{Cx} x {Cy}, {BitCount}bit, {Size}bytes")]
internal struct ICONRESINF
{
	public byte Cx;

	public byte Cy;

	public byte ColorCount;

	public byte Reserved;

	public ushort Planes;

	public ushort BitCount;

	public uint Size;

	public ushort ID;
}

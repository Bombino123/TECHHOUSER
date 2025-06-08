using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct AuthenticatedProtectionFlagsMidlMidlItfD3d11000000340001Inner
{
	[FieldOffset(0)]
	internal int _ProtectionEnabled;

	[FieldOffset(0)]
	internal int _OverlayOrFullscreenRequired;

	[FieldOffset(0)]
	internal int _Reserved;

	public bool ProtectionEnabled
	{
		get
		{
			return (_ProtectionEnabled & 1) != 0;
		}
		set
		{
			_ProtectionEnabled = (_ProtectionEnabled & -2) | ((value ? 1 : 0) & 1);
		}
	}

	public bool OverlayOrFullscreenRequired
	{
		get
		{
			return ((_OverlayOrFullscreenRequired >> 1) & 1) != 0;
		}
		set
		{
			_OverlayOrFullscreenRequired = (_OverlayOrFullscreenRequired & -3) | (((value ? 1 : 0) & 1) << 1);
		}
	}

	public int Reserved
	{
		get
		{
			return (_Reserved >> 2) & 0x3FFFFFFF;
		}
		set
		{
			_Reserved = (_Reserved & 3) | ((value & 0x3FFFFFFF) << 2);
		}
	}
}

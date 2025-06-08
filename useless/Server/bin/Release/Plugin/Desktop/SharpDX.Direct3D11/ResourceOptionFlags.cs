using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum ResourceOptionFlags
{
	GenerateMipMaps = 1,
	Shared = 2,
	TextureCube = 4,
	DrawIndirectArguments = 0x10,
	BufferAllowRawViews = 0x20,
	BufferStructured = 0x40,
	ResourceClamp = 0x80,
	SharedKeyedmutex = 0x100,
	GdiCompatible = 0x200,
	SharedNthandle = 0x800,
	RestrictedContent = 0x1000,
	RestrictSharedResource = 0x2000,
	RestrictSharedResourceDriver = 0x4000,
	Guarded = 0x8000,
	TilePool = 0x20000,
	Tiled = 0x40000,
	HwProtected = 0x80000,
	None = 0
}

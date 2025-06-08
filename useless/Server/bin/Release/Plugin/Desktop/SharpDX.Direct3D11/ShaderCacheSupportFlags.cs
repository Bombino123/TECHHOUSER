using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum ShaderCacheSupportFlags
{
	None = 0,
	AutomaticInprocCache = 1,
	AutomaticDiskCache = 2
}

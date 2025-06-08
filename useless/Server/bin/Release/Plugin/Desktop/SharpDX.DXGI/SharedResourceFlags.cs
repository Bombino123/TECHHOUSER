using System;

namespace SharpDX.DXGI;

[Flags]
public enum SharedResourceFlags
{
	Read = int.MinValue,
	Write = 1,
	None = 0
}

using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum ReportingLevel
{
	Summary = 1,
	Detail = 2,
	IgnoreInternal = 4,
	None = 0
}

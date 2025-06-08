using System;

namespace dnlib.DotNet.Writer;

[Flags]
public enum PdbWriterOptions
{
	None = 0,
	NoDiaSymReader = 1,
	NoOldDiaSymReader = 2,
	Deterministic = 4,
	PdbChecksum = 8
}

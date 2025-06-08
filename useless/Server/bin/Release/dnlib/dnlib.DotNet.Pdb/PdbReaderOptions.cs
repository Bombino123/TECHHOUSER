using System;

namespace dnlib.DotNet.Pdb;

[Flags]
public enum PdbReaderOptions
{
	None = 0,
	MicrosoftComReader = 1,
	NoDiaSymReader = 2,
	NoOldDiaSymReader = 4
}

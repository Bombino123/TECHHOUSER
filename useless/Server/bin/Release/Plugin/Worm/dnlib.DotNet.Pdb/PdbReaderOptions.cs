using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[Flags]
[ComVisible(true)]
public enum PdbReaderOptions
{
	None = 0,
	MicrosoftComReader = 1,
	NoDiaSymReader = 2,
	NoOldDiaSymReader = 4
}

using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[Flags]
[ComVisible(true)]
public enum ImporterOptions
{
	TryToUseTypeDefs = 1,
	TryToUseMethodDefs = 2,
	TryToUseFieldDefs = 4,
	TryToUseDefs = 7,
	TryToUseExistingAssemblyRefs = 8,
	FixSignature = int.MinValue
}

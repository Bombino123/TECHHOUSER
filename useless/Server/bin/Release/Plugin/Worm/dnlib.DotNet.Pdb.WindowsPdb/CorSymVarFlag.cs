using System;

namespace dnlib.DotNet.Pdb.WindowsPdb;

[Flags]
internal enum CorSymVarFlag : uint
{
	VAR_IS_COMP_GEN = 1u
}

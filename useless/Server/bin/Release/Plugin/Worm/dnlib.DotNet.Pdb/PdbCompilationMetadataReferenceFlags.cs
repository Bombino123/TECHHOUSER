using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[Flags]
[ComVisible(true)]
public enum PdbCompilationMetadataReferenceFlags : byte
{
	None = 0,
	Assembly = 1,
	EmbedInteropTypes = 2
}

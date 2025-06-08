using System;

namespace dnlib.DotNet.Pdb;

[Flags]
public enum PdbCompilationMetadataReferenceFlags : byte
{
	None = 0,
	Assembly = 1,
	EmbedInteropTypes = 2
}

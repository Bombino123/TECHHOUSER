using System;

namespace dnlib.DotNet.Pdb;

public sealed class PdbEmbeddedSourceCustomDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.EmbeddedSource;

	public override Guid Guid => CustomDebugInfoGuids.EmbeddedSource;

	public byte[] SourceCodeBlob { get; set; }

	public PdbEmbeddedSourceCustomDebugInfo()
	{
	}

	public PdbEmbeddedSourceCustomDebugInfo(byte[] sourceCodeBlob)
	{
		SourceCodeBlob = sourceCodeBlob;
	}
}

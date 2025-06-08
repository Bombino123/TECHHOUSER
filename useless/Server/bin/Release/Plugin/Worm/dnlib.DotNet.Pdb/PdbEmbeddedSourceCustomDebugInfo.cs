using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
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

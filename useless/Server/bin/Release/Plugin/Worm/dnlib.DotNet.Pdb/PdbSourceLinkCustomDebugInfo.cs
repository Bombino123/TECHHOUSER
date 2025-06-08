using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbSourceLinkCustomDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.SourceLink;

	public override Guid Guid => CustomDebugInfoGuids.SourceLink;

	public byte[] FileBlob { get; set; }

	public PdbSourceLinkCustomDebugInfo()
	{
	}

	public PdbSourceLinkCustomDebugInfo(byte[] fileBlob)
	{
		FileBlob = fileBlob;
	}
}

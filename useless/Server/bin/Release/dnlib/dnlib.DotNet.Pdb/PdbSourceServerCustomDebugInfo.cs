using System;

namespace dnlib.DotNet.Pdb;

public sealed class PdbSourceServerCustomDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.SourceServer;

	public override Guid Guid => Guid.Empty;

	public byte[] FileBlob { get; set; }

	public PdbSourceServerCustomDebugInfo()
	{
	}

	public PdbSourceServerCustomDebugInfo(byte[] fileBlob)
	{
		FileBlob = fileBlob;
	}
}

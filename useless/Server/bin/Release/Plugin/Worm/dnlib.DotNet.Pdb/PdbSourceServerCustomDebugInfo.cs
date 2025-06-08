using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
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

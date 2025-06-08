using System;

namespace dnlib.DotNet.Pdb;

public sealed class PdbEditAndContinueLocalSlotMapCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly byte[] data;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.EditAndContinueLocalSlotMap;

	public override Guid Guid => CustomDebugInfoGuids.EncLocalSlotMap;

	public byte[] Data => data;

	public PdbEditAndContinueLocalSlotMapCustomDebugInfo(byte[] data)
	{
		this.data = data ?? throw new ArgumentNullException("data");
	}
}

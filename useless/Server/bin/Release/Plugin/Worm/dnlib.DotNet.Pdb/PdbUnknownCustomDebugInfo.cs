using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbUnknownCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly PdbCustomDebugInfoKind kind;

	private readonly Guid guid;

	private readonly byte[] data;

	public override PdbCustomDebugInfoKind Kind => kind;

	public override Guid Guid => guid;

	public byte[] Data => data;

	public PdbUnknownCustomDebugInfo(PdbCustomDebugInfoKind kind, byte[] data)
	{
		this.kind = kind;
		this.data = data ?? throw new ArgumentNullException("data");
		guid = Guid.Empty;
	}

	public PdbUnknownCustomDebugInfo(Guid guid, byte[] data)
	{
		kind = PdbCustomDebugInfoKind.Unknown;
		this.data = data ?? throw new ArgumentNullException("data");
		this.guid = guid;
	}
}

using System;

namespace dnlib.DotNet.Pdb;

public sealed class PdbEditAndContinueLambdaMapCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly byte[] data;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.EditAndContinueLambdaMap;

	public override Guid Guid => CustomDebugInfoGuids.EncLambdaAndClosureMap;

	public byte[] Data => data;

	public PdbEditAndContinueLambdaMapCustomDebugInfo(byte[] data)
	{
		this.data = data ?? throw new ArgumentNullException("data");
	}
}

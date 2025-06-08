using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb;

internal sealed class PdbAsyncMethodSteppingInformationCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly IList<PdbAsyncStepInfo> asyncStepInfos;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.Unknown;

	public override Guid Guid => CustomDebugInfoGuids.AsyncMethodSteppingInformationBlob;

	public Instruction CatchHandler { get; set; }

	public IList<PdbAsyncStepInfo> AsyncStepInfos => asyncStepInfos;

	public PdbAsyncMethodSteppingInformationCustomDebugInfo()
	{
		asyncStepInfos = new List<PdbAsyncStepInfo>();
	}
}

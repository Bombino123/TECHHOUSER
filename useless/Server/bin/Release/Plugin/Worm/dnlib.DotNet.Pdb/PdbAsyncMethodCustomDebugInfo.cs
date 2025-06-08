using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbAsyncMethodCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly IList<PdbAsyncStepInfo> asyncStepInfos;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.AsyncMethod;

	public override Guid Guid => Guid.Empty;

	public MethodDef KickoffMethod { get; set; }

	public Instruction CatchHandlerInstruction { get; set; }

	public IList<PdbAsyncStepInfo> StepInfos => asyncStepInfos;

	public PdbAsyncMethodCustomDebugInfo()
	{
		asyncStepInfos = new List<PdbAsyncStepInfo>();
	}

	public PdbAsyncMethodCustomDebugInfo(int stepInfosCapacity)
	{
		asyncStepInfos = new List<PdbAsyncStepInfo>(stepInfosCapacity);
	}
}

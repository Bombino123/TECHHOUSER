using System;
using System.Collections.Generic;

namespace dnlib.DotNet.Pdb;

public sealed class PdbStateMachineHoistedLocalScopesCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly IList<StateMachineHoistedLocalScope> scopes;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.StateMachineHoistedLocalScopes;

	public override Guid Guid => CustomDebugInfoGuids.StateMachineHoistedLocalScopes;

	public IList<StateMachineHoistedLocalScope> Scopes => scopes;

	public PdbStateMachineHoistedLocalScopesCustomDebugInfo()
	{
		scopes = new List<StateMachineHoistedLocalScope>();
	}

	public PdbStateMachineHoistedLocalScopesCustomDebugInfo(int capacity)
	{
		scopes = new List<StateMachineHoistedLocalScope>(capacity);
	}
}

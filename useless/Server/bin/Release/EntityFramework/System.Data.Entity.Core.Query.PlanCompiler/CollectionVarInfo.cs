using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class CollectionVarInfo : VarInfo
{
	private readonly List<Var> m_newVars;

	internal Var NewVar => m_newVars[0];

	internal override VarInfoKind Kind => VarInfoKind.CollectionVarInfo;

	internal override List<Var> NewVars => m_newVars;

	internal CollectionVarInfo(Var newVar)
	{
		m_newVars = new List<Var>();
		m_newVars.Add(newVar);
	}
}

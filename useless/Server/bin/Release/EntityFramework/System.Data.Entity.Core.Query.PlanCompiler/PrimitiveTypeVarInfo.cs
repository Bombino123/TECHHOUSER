using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class PrimitiveTypeVarInfo : VarInfo
{
	private readonly List<Var> m_newVars;

	internal Var NewVar => m_newVars[0];

	internal override VarInfoKind Kind => VarInfoKind.PrimitiveTypeVarInfo;

	internal override List<Var> NewVars => m_newVars;

	internal PrimitiveTypeVarInfo(Var newVar)
	{
		m_newVars = new List<Var> { newVar };
	}
}

using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal abstract class VarInfo
{
	internal abstract VarInfoKind Kind { get; }

	internal virtual List<Var> NewVars => null;
}

using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ComputedVar : Var
{
	internal ComputedVar(int id, TypeUsage type)
		: base(id, VarType.Computed, type)
	{
	}
}

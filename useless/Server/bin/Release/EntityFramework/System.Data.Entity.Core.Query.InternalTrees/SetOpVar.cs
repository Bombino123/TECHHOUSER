using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class SetOpVar : Var
{
	internal SetOpVar(int id, TypeUsage type)
		: base(id, VarType.SetOp, type)
	{
	}
}

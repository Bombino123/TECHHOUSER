using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class Var
{
	private readonly int _id;

	private readonly VarType _varType;

	private readonly TypeUsage _type;

	internal int Id => _id;

	internal VarType VarType => _varType;

	internal TypeUsage Type => _type;

	internal Var(int id, VarType varType, TypeUsage type)
	{
		_id = id;
		_varType = varType;
		_type = type;
	}

	internal virtual bool TryGetName(out string name)
	{
		name = null;
		return false;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { Id });
	}
}

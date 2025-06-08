using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ParameterVar : Var
{
	private readonly string m_paramName;

	internal string ParameterName => m_paramName;

	internal ParameterVar(int id, TypeUsage type, string paramName)
		: base(id, VarType.Parameter, type)
	{
		m_paramName = paramName;
	}

	internal override bool TryGetName(out string name)
	{
		name = ParameterName;
		return true;
	}
}

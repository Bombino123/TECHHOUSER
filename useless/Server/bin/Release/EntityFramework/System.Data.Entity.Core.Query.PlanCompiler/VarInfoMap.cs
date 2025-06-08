using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class VarInfoMap
{
	private readonly Dictionary<Var, VarInfo> m_map;

	internal VarInfoMap()
	{
		m_map = new Dictionary<Var, VarInfo>();
	}

	internal VarInfo CreateStructuredVarInfo(Var v, RowType newType, List<Var> newVars, List<EdmProperty> newProperties, bool newVarsIncludeNullSentinelVar)
	{
		VarInfo varInfo = new StructuredVarInfo(newType, newVars, newProperties, newVarsIncludeNullSentinelVar);
		m_map.Add(v, varInfo);
		return varInfo;
	}

	internal VarInfo CreateStructuredVarInfo(Var v, RowType newType, List<Var> newVars, List<EdmProperty> newProperties)
	{
		return CreateStructuredVarInfo(v, newType, newVars, newProperties, newVarsIncludeNullSentinelVar: false);
	}

	internal VarInfo CreateCollectionVarInfo(Var v, Var newVar)
	{
		VarInfo varInfo = new CollectionVarInfo(newVar);
		m_map.Add(v, varInfo);
		return varInfo;
	}

	internal VarInfo CreatePrimitiveTypeVarInfo(Var v, Var newVar)
	{
		PlanCompiler.Assert(TypeSemantics.IsScalarType(v.Type), "The current variable should be of primitive or enum type.");
		PlanCompiler.Assert(TypeSemantics.IsScalarType(newVar.Type), "The new variable should be of primitive or enum type.");
		VarInfo varInfo = new PrimitiveTypeVarInfo(newVar);
		m_map.Add(v, varInfo);
		return varInfo;
	}

	internal bool TryGetVarInfo(Var v, out VarInfo varInfo)
	{
		return m_map.TryGetValue(v, out varInfo);
	}
}

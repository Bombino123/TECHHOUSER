using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class StructuredVarInfo : VarInfo
{
	private Dictionary<EdmProperty, Var> m_propertyToVarMap;

	private readonly List<Var> m_newVars;

	private readonly bool m_newVarsIncludeNullSentinelVar;

	private readonly List<EdmProperty> m_newProperties;

	private readonly RowType m_newType;

	private readonly TypeUsage m_newTypeUsage;

	internal override VarInfoKind Kind => VarInfoKind.StructuredTypeVarInfo;

	internal override List<Var> NewVars => m_newVars;

	internal List<EdmProperty> Fields => m_newProperties;

	internal bool NewVarsIncludeNullSentinelVar => m_newVarsIncludeNullSentinelVar;

	internal RowType NewType => m_newType;

	internal TypeUsage NewTypeUsage => m_newTypeUsage;

	internal StructuredVarInfo(RowType newType, List<Var> newVars, List<EdmProperty> newTypeProperties, bool newVarsIncludeNullSentinelVar)
	{
		PlanCompiler.Assert(newVars.Count == newTypeProperties.Count, "count mismatch");
		m_newVars = newVars;
		m_newProperties = newTypeProperties;
		m_newType = newType;
		m_newVarsIncludeNullSentinelVar = newVarsIncludeNullSentinelVar;
		m_newTypeUsage = TypeUsage.Create(newType);
	}

	internal bool TryGetVar(EdmProperty p, out Var v)
	{
		if (m_propertyToVarMap == null)
		{
			InitPropertyToVarMap();
		}
		return m_propertyToVarMap.TryGetValue(p, out v);
	}

	private void InitPropertyToVarMap()
	{
		if (m_propertyToVarMap != null)
		{
			return;
		}
		m_propertyToVarMap = new Dictionary<EdmProperty, Var>();
		IEnumerator<Var> enumerator = m_newVars.GetEnumerator();
		foreach (EdmProperty newProperty in m_newProperties)
		{
			enumerator.MoveNext();
			m_propertyToVarMap.Add(newProperty, enumerator.Current);
		}
		enumerator.Dispose();
	}
}

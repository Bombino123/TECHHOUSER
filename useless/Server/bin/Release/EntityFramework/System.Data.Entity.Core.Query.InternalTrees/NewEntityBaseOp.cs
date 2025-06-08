using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class NewEntityBaseOp : ScalarOp
{
	private readonly bool m_scoped;

	private readonly EntitySet m_entitySet;

	private readonly List<RelProperty> m_relProperties;

	internal bool Scoped => m_scoped;

	internal EntitySet EntitySet => m_entitySet;

	internal List<RelProperty> RelationshipProperties => m_relProperties;

	internal NewEntityBaseOp(OpType opType, TypeUsage type, bool scoped, EntitySet entitySet, List<RelProperty> relProperties)
		: base(opType, type)
	{
		m_scoped = scoped;
		m_entitySet = entitySet;
		m_relProperties = relProperties;
	}

	protected NewEntityBaseOp(OpType opType)
		: base(opType)
	{
	}
}

using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class ChangeNode
{
	private readonly TypeUsage m_elementType;

	private readonly List<PropagatorResult> m_inserted = new List<PropagatorResult>();

	private readonly List<PropagatorResult> m_deleted = new List<PropagatorResult>();

	internal TypeUsage ElementType => m_elementType;

	internal List<PropagatorResult> Inserted => m_inserted;

	internal List<PropagatorResult> Deleted => m_deleted;

	internal PropagatorResult Placeholder { get; set; }

	internal ChangeNode(TypeUsage elementType)
	{
		m_elementType = elementType;
	}
}

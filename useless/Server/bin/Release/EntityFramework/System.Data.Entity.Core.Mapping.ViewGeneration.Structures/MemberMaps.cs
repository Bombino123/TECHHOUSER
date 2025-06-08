namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class MemberMaps
{
	private readonly MemberProjectionIndex m_projectedSlotMap;

	private readonly MemberDomainMap m_queryDomainMap;

	private readonly MemberDomainMap m_updateDomainMap;

	private readonly ViewTarget m_viewTarget;

	internal MemberProjectionIndex ProjectedSlotMap => m_projectedSlotMap;

	internal MemberDomainMap QueryDomainMap => m_queryDomainMap;

	internal MemberDomainMap UpdateDomainMap => m_updateDomainMap;

	internal MemberDomainMap RightDomainMap
	{
		get
		{
			if (m_viewTarget != 0)
			{
				return m_queryDomainMap;
			}
			return m_updateDomainMap;
		}
	}

	internal MemberDomainMap LeftDomainMap
	{
		get
		{
			if (m_viewTarget != 0)
			{
				return m_updateDomainMap;
			}
			return m_queryDomainMap;
		}
	}

	internal MemberMaps(ViewTarget viewTarget, MemberProjectionIndex projectedSlotMap, MemberDomainMap queryDomainMap, MemberDomainMap updateDomainMap)
	{
		m_projectedSlotMap = projectedSlotMap;
		m_queryDomainMap = queryDomainMap;
		m_updateDomainMap = updateDomainMap;
		m_viewTarget = viewTarget;
	}
}

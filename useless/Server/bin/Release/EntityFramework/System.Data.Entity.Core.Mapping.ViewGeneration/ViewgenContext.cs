using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal class ViewgenContext : InternalBase
{
	internal class OneToOneFkAssociationsForEntitiesFilter
	{
		public virtual IEnumerable<AssociationSet> Filter(IList<EntityType> entityTypes, IEnumerable<AssociationSet> associationSets)
		{
			return associationSets.Where((AssociationSet a) => a.ElementType.IsForeignKey && a.ElementType.AssociationEndMembers.All((AssociationEndMember aem) => aem.RelationshipMultiplicity == RelationshipMultiplicity.One && entityTypes.Contains(aem.GetEntityType())));
		}
	}

	private readonly ConfigViewGenerator m_config;

	private readonly ViewTarget m_viewTarget;

	private readonly EntitySetBase m_extent;

	private readonly MemberMaps m_memberMaps;

	private readonly EdmItemCollection m_edmItemCollection;

	private readonly EntityContainerMapping m_entityContainerMapping;

	private List<LeftCellWrapper> m_cellWrappers;

	private readonly FragmentQueryProcessor m_leftFragmentQP;

	private readonly FragmentQueryProcessor m_rightFragmentQP;

	private readonly CqlIdentifiers m_identifiers;

	private readonly Dictionary<FragmentQuery, Tile<FragmentQuery>> m_rewritingCache;

	internal ViewTarget ViewTarget => m_viewTarget;

	internal MemberMaps MemberMaps => m_memberMaps;

	internal EntitySetBase Extent => m_extent;

	internal ConfigViewGenerator Config => m_config;

	internal CqlIdentifiers CqlIdentifiers => m_identifiers;

	internal EdmItemCollection EdmItemCollection => m_edmItemCollection;

	internal FragmentQueryProcessor LeftFragmentQP => m_leftFragmentQP;

	internal FragmentQueryProcessor RightFragmentQP => m_rightFragmentQP;

	internal List<LeftCellWrapper> AllWrappersForExtent => m_cellWrappers;

	internal EntityContainerMapping EntityContainerMapping => m_entityContainerMapping;

	internal ViewgenContext(ViewTarget viewTarget, EntitySetBase extent, IList<Cell> extentCells, CqlIdentifiers identifiers, ConfigViewGenerator config, MemberDomainMap queryDomainMap, MemberDomainMap updateDomainMap, EntityContainerMapping entityContainerMapping)
	{
		foreach (Cell extentCell in extentCells)
		{
			_ = extentCell;
		}
		m_extent = extent;
		m_viewTarget = viewTarget;
		m_config = config;
		m_edmItemCollection = entityContainerMapping.StorageMappingItemCollection.EdmItemCollection;
		m_entityContainerMapping = entityContainerMapping;
		m_identifiers = identifiers;
		updateDomainMap = updateDomainMap.MakeCopy();
		MemberDomainMap domainMap = ((viewTarget == ViewTarget.QueryView) ? queryDomainMap : updateDomainMap);
		m_memberMaps = new MemberMaps(viewTarget, MemberProjectionIndex.Create(extent, m_edmItemCollection), queryDomainMap, updateDomainMap);
		FragmentQueryKBChaseSupport fragmentQueryKBChaseSupport = new FragmentQueryKBChaseSupport();
		fragmentQueryKBChaseSupport.CreateVariableConstraints(extent, domainMap, m_edmItemCollection);
		m_leftFragmentQP = new FragmentQueryProcessor(fragmentQueryKBChaseSupport);
		m_rewritingCache = new Dictionary<FragmentQuery, Tile<FragmentQuery>>(FragmentQuery.GetEqualityComparer(m_leftFragmentQP));
		if (!CreateLeftCellWrappers(extentCells, viewTarget))
		{
			return;
		}
		FragmentQueryKBChaseSupport fragmentQueryKBChaseSupport2 = new FragmentQueryKBChaseSupport();
		MemberDomainMap memberDomainMap = ((viewTarget == ViewTarget.QueryView) ? updateDomainMap : queryDomainMap);
		foreach (LeftCellWrapper cellWrapper in m_cellWrappers)
		{
			EntitySetBase rightExtent = cellWrapper.RightExtent;
			fragmentQueryKBChaseSupport2.CreateVariableConstraints(rightExtent, memberDomainMap, m_edmItemCollection);
			fragmentQueryKBChaseSupport2.CreateAssociationConstraints(rightExtent, memberDomainMap, m_edmItemCollection);
		}
		if (m_viewTarget == ViewTarget.UpdateView)
		{
			CreateConstraintsForForeignKeyAssociationsAffectingThisWrapper(fragmentQueryKBChaseSupport2, memberDomainMap);
		}
		m_rightFragmentQP = new FragmentQueryProcessor(fragmentQueryKBChaseSupport2);
		if (m_viewTarget == ViewTarget.QueryView)
		{
			CheckConcurrencyControlTokens();
		}
		m_cellWrappers.Sort(LeftCellWrapper.Comparer);
	}

	private void CreateConstraintsForForeignKeyAssociationsAffectingThisWrapper(FragmentQueryKB rightKB, MemberDomainMap rightDomainMap)
	{
		foreach (AssociationSet item in new OneToOneFkAssociationsForEntitiesFilter().Filter((from it in m_cellWrappers.Select((LeftCellWrapper it) => it.RightExtent).OfType<EntitySet>()
			select it.ElementType).ToList(), m_entityContainerMapping.EdmEntityContainer.BaseEntitySets.OfType<AssociationSet>()))
		{
			rightKB.CreateEquivalenceConstraintForOneToOneForeignKeyAssociation(item, rightDomainMap);
		}
	}

	internal bool TryGetCachedRewriting(FragmentQuery query, out Tile<FragmentQuery> rewriting)
	{
		return m_rewritingCache.TryGetValue(query, out rewriting);
	}

	internal void SetCachedRewriting(FragmentQuery query, Tile<FragmentQuery> rewriting)
	{
		m_rewritingCache[query] = rewriting;
	}

	private void CheckConcurrencyControlTokens()
	{
		EntityTypeBase elementType = m_extent.ElementType;
		Set<EdmMember> concurrencyMembersForTypeHierarchy = MetadataHelper.GetConcurrencyMembersForTypeHierarchy(elementType, m_edmItemCollection);
		Set<MemberPath> set = new Set<MemberPath>(MemberPath.EqualityComparer);
		foreach (EdmMember item in concurrencyMembersForTypeHierarchy)
		{
			if (!item.DeclaringType.IsAssignableFrom(elementType))
			{
				string message = Strings.ViewGen_Concurrency_Derived_Class(item.Name, item.DeclaringType.Name, m_extent);
				ExceptionHelpers.ThrowMappingException(new ErrorLog.Record(ViewGenErrorCode.ConcurrencyDerivedClass, message, m_cellWrappers, string.Empty), m_config);
			}
			set.Add(new MemberPath(m_extent, item));
		}
		if (concurrencyMembersForTypeHierarchy.Count <= 0)
		{
			return;
		}
		foreach (LeftCellWrapper cellWrapper in m_cellWrappers)
		{
			Set<MemberPath> set2 = new Set<MemberPath>(cellWrapper.OnlyInputCell.CQuery.WhereClause.MemberRestrictions.Select((MemberRestriction oneOf) => oneOf.RestrictedMemberSlot.MemberPath), MemberPath.EqualityComparer);
			set2.Intersect(set);
			if (set2.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(Strings.ViewGen_Concurrency_Invalid_Condition(MemberPath.PropertiesToUserString(set2, fullPath: false), m_extent.Name));
				ExceptionHelpers.ThrowMappingException(new ErrorLog.Record(ViewGenErrorCode.ConcurrencyTokenHasCondition, stringBuilder.ToString(), new LeftCellWrapper[1] { cellWrapper }, string.Empty), m_config);
			}
		}
	}

	private bool CreateLeftCellWrappers(IList<Cell> extentCells, ViewTarget viewTarget)
	{
		List<Cell> list = AlignFields(extentCells, m_memberMaps.ProjectedSlotMap, viewTarget);
		m_cellWrappers = new List<LeftCellWrapper>();
		for (int i = 0; i < list.Count; i++)
		{
			Cell cell = list[i];
			CellQuery leftQuery = cell.GetLeftQuery(viewTarget);
			CellQuery rightQuery = cell.GetRightQuery(viewTarget);
			Set<MemberPath> nonNullSlots = leftQuery.GetNonNullSlots();
			FragmentQuery fragmentQuery = FragmentQuery.Create(BoolExpression.CreateLiteral(new CellIdBoolean(m_identifiers, extentCells[i].CellNumber), m_memberMaps.LeftDomainMap), leftQuery);
			if (viewTarget == ViewTarget.UpdateView)
			{
				fragmentQuery = m_leftFragmentQP.CreateDerivedViewBySelectingConstantAttributes(fragmentQuery) ?? fragmentQuery;
			}
			LeftCellWrapper item = new LeftCellWrapper(m_viewTarget, nonNullSlots, fragmentQuery, leftQuery, rightQuery, m_memberMaps, extentCells[i]);
			m_cellWrappers.Add(item);
		}
		return true;
	}

	private static List<Cell> AlignFields(IEnumerable<Cell> cells, MemberProjectionIndex projectedSlotMap, ViewTarget viewTarget)
	{
		List<Cell> list = new List<Cell>();
		foreach (Cell cell in cells)
		{
			CellQuery leftQuery = cell.GetLeftQuery(viewTarget);
			CellQuery rightQuery = cell.GetRightQuery(viewTarget);
			leftQuery.CreateFieldAlignedCellQueries(rightQuery, projectedSlotMap, out var newMainQuery, out var newOtherQuery);
			Cell item = ((viewTarget == ViewTarget.QueryView) ? Cell.CreateCS(newMainQuery, newOtherQuery, cell.CellLabel, cell.CellNumber) : Cell.CreateCS(newOtherQuery, newMainQuery, cell.CellLabel, cell.CellNumber));
			list.Add(item);
		}
		return list;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		LeftCellWrapper.WrappersToStringBuilder(builder, m_cellWrappers, "Left Cell Wrappers");
	}
}

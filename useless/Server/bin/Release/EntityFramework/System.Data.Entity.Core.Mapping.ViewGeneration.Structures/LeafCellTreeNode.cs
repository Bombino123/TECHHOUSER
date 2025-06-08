using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class LeafCellTreeNode : CellTreeNode
{
	private class LeafCellTreeNodeComparer : IEqualityComparer<LeafCellTreeNode>
	{
		public bool Equals(LeafCellTreeNode left, LeafCellTreeNode right)
		{
			if (left == right)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			return left.m_cellWrapper.Equals(right.m_cellWrapper);
		}

		public int GetHashCode(LeafCellTreeNode node)
		{
			return node.m_cellWrapper.GetHashCode();
		}
	}

	internal static readonly IEqualityComparer<LeafCellTreeNode> EqualityComparer = new LeafCellTreeNodeComparer();

	private readonly LeftCellWrapper m_cellWrapper;

	private readonly FragmentQuery m_rightFragmentQuery;

	internal LeftCellWrapper LeftCellWrapper => m_cellWrapper;

	internal override MemberDomainMap RightDomainMap => m_cellWrapper.RightDomainMap;

	internal override FragmentQuery LeftFragmentQuery => m_cellWrapper.FragmentQuery;

	internal override FragmentQuery RightFragmentQuery => m_rightFragmentQuery;

	internal override Set<MemberPath> Attributes => m_cellWrapper.Attributes;

	internal override List<CellTreeNode> Children => new List<CellTreeNode>();

	internal override CellTreeOpType OpType => CellTreeOpType.Leaf;

	internal override int NumProjectedSlots => LeftCellWrapper.RightCellQuery.NumProjectedSlots;

	internal override int NumBoolSlots => LeftCellWrapper.RightCellQuery.NumBoolVars;

	internal LeafCellTreeNode(ViewgenContext context, LeftCellWrapper cellWrapper)
		: base(context)
	{
		m_cellWrapper = cellWrapper;
		m_rightFragmentQuery = FragmentQuery.Create(cellWrapper.OriginalCellNumberString, cellWrapper.CreateRoleBoolean(), cellWrapper.RightCellQuery);
	}

	internal LeafCellTreeNode(ViewgenContext context, LeftCellWrapper cellWrapper, FragmentQuery rightFragmentQuery)
		: base(context)
	{
		m_cellWrapper = cellWrapper;
		m_rightFragmentQuery = rightFragmentQuery;
	}

	internal override TOutput Accept<TInput, TOutput>(CellTreeVisitor<TInput, TOutput> visitor, TInput param)
	{
		return visitor.VisitLeaf(this, param);
	}

	internal override TOutput Accept<TInput, TOutput>(SimpleCellTreeVisitor<TInput, TOutput> visitor, TInput param)
	{
		return visitor.VisitLeaf(this, param);
	}

	internal override bool IsProjectedSlot(int slot)
	{
		CellQuery rightCellQuery = LeftCellWrapper.RightCellQuery;
		if (IsBoolSlot(slot))
		{
			return rightCellQuery.GetBoolVar(SlotToBoolIndex(slot)) != null;
		}
		return rightCellQuery.ProjectedSlotAt(slot) != null;
	}

	internal override CqlBlock ToCqlBlock(bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum, ref List<WithRelationship> withRelationships)
	{
		int num = requiredSlots.Length;
		CellQuery rightCellQuery = LeftCellWrapper.RightCellQuery;
		SlotInfo[] array = new SlotInfo[num];
		for (int i = 0; i < rightCellQuery.NumProjectedSlots; i++)
		{
			ProjectedSlot projectedSlot = rightCellQuery.ProjectedSlotAt(i);
			if (requiredSlots[i] && projectedSlot == null)
			{
				ConstantProjectedSlot constantProjectedSlot = new ConstantProjectedSlot(Domain.GetDefaultValueForMemberPath(base.ProjectedSlotMap[i], GetLeaves(), base.ViewgenContext.Config));
				rightCellQuery.FixMissingSlotAsDefaultConstant(i, constantProjectedSlot);
				projectedSlot = constantProjectedSlot;
			}
			SlotInfo slotInfo = new SlotInfo(requiredSlots[i], projectedSlot != null, projectedSlot, base.ProjectedSlotMap[i]);
			array[i] = slotInfo;
		}
		for (int j = 0; j < rightCellQuery.NumBoolVars; j++)
		{
			BoolExpression boolVar = rightCellQuery.GetBoolVar(j);
			BooleanProjectedSlot slotValue = ((boolVar == null) ? new BooleanProjectedSlot(BoolExpression.False, identifiers, j) : new BooleanProjectedSlot(boolVar, identifiers, j));
			int num2 = BoolIndexToSlot(j);
			SlotInfo slotInfo2 = new SlotInfo(requiredSlots[num2], boolVar != null, slotValue, null);
			array[num2] = slotInfo2;
		}
		IEnumerable<SlotInfo> source = array;
		if (rightCellQuery.Extent.EntityContainer.DataSpace == DataSpace.SSpace && m_cellWrapper.LeftExtent.BuiltInTypeKind == BuiltInTypeKind.EntitySet)
		{
			IEnumerable<AssociationSetMapping> relationshipSetMappingsFor = base.ViewgenContext.EntityContainerMapping.GetRelationshipSetMappingsFor(m_cellWrapper.LeftExtent, rightCellQuery.Extent);
			List<SlotInfo> foreignKeySlots = new List<SlotInfo>();
			foreach (AssociationSetMapping item in relationshipSetMappingsFor)
			{
				if (TryGetWithRelationship(item, m_cellWrapper.LeftExtent, rightCellQuery.SourceExtentMemberPath, ref foreignKeySlots, out var withRelationship))
				{
					withRelationships.Add(withRelationship);
					source = array.Concat(foreignKeySlots);
				}
			}
		}
		return new ExtentCqlBlock(rightCellQuery.Extent, rightCellQuery.SelectDistinctFlag, source.ToArray(), rightCellQuery.WhereClause, identifiers, ++blockAliasNum);
	}

	private static bool TryGetWithRelationship(AssociationSetMapping collocatedAssociationSetMap, EntitySetBase thisExtent, MemberPath sRootNode, ref List<SlotInfo> foreignKeySlots, out WithRelationship withRelationship)
	{
		withRelationship = null;
		EndPropertyMapping foreignKeyEndMapFromAssociationMap = GetForeignKeyEndMapFromAssociationMap(collocatedAssociationSetMap);
		if (foreignKeyEndMapFromAssociationMap == null || foreignKeyEndMapFromAssociationMap.AssociationEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
		{
			return false;
		}
		AssociationEndMember associationEnd = foreignKeyEndMapFromAssociationMap.AssociationEnd;
		AssociationEndMember otherAssociationEnd = MetadataHelper.GetOtherAssociationEnd(associationEnd);
		EntityType entityType = (EntityType)((RefType)associationEnd.TypeUsage.EdmType).ElementType;
		EntityType entityType2 = (EntityType)((RefType)otherAssociationEnd.TypeUsage.EdmType).ElementType;
		AssociationSet associationSet = (AssociationSet)collocatedAssociationSetMap.Set;
		MemberPath prefix = new MemberPath(associationSet, associationEnd);
		IEnumerable<ScalarPropertyMapping> source = foreignKeyEndMapFromAssociationMap.PropertyMappings.Cast<ScalarPropertyMapping>();
		List<MemberPath> list = new List<MemberPath>();
		foreach (EdmProperty edmProperty in entityType.KeyMembers)
		{
			ScalarPropertyMapping scalarPropertyMapping = source.Where((ScalarPropertyMapping propMap) => propMap.Property.Equals(edmProperty)).First();
			MemberProjectedSlot slotValue = new MemberProjectedSlot(new MemberPath(sRootNode, scalarPropertyMapping.Column));
			MemberPath memberPath = new MemberPath(prefix, edmProperty);
			list.Add(memberPath);
			foreignKeySlots.Add(new SlotInfo(isRequiredByParent: true, isProjected: true, slotValue, memberPath));
		}
		if (thisExtent.ElementType.IsAssignableFrom(entityType2))
		{
			withRelationship = new WithRelationship(associationSet, otherAssociationEnd, entityType2, associationEnd, entityType, list);
			return true;
		}
		return false;
	}

	private static EndPropertyMapping GetForeignKeyEndMapFromAssociationMap(AssociationSetMapping collocatedAssociationSetMap)
	{
		MappingFragment mappingFragment = collocatedAssociationSetMap.TypeMappings.First().MappingFragments.First();
		IEnumerable<EdmMember> keyMembers = collocatedAssociationSetMap.StoreEntitySet.ElementType.KeyMembers;
		foreach (EndPropertyMapping endMap in mappingFragment.PropertyMappings)
		{
			if (endMap.StoreProperties.SequenceEqual(keyMembers, EqualityComparer<EdmMember>.Default))
			{
				return (from eMap in mappingFragment.PropertyMappings.OfType<EndPropertyMapping>()
					where !eMap.Equals(endMap)
					select eMap).First();
			}
		}
		return null;
	}

	internal override void ToCompactString(StringBuilder stringBuilder)
	{
		m_cellWrapper.ToCompactString(stringBuilder);
	}
}

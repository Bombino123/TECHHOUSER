using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal class CellCreator : InternalBase
{
	private readonly EntityContainerMapping m_containerMapping;

	private int m_currentCellNumber;

	private readonly CqlIdentifiers m_identifiers;

	internal CqlIdentifiers Identifiers => m_identifiers;

	internal CellCreator(EntityContainerMapping containerMapping)
	{
		m_containerMapping = containerMapping;
		m_identifiers = new CqlIdentifiers();
	}

	internal List<Cell> GenerateCells()
	{
		List<Cell> list = new List<Cell>();
		ExtractCells(list);
		ExpandCells(list);
		m_identifiers.AddIdentifier(m_containerMapping.EdmEntityContainer.Name);
		m_identifiers.AddIdentifier(m_containerMapping.StorageEntityContainer.Name);
		foreach (Cell item in list)
		{
			item.GetIdentifiers(m_identifiers);
		}
		return list;
	}

	private void ExpandCells(List<Cell> cells)
	{
		Set<MemberPath> set = new Set<MemberPath>();
		foreach (Cell cell2 in cells)
		{
			foreach (MemberPath item in from member in cell2.SQuery.GetProjectedMembers()
				where IsBooleanMember(member)
				select member into boolMember
				where (from restriction in cell2.SQuery.GetConjunctsFromWhereClause()
					where restriction.Domain.Values.Contains(Constant.NotNull)
					select restriction.RestrictedMemberSlot.MemberPath).Contains(boolMember)
				select boolMember)
			{
				set.Add(item);
			}
		}
		Dictionary<MemberPath, Set<MemberPath>> dictionary = new Dictionary<MemberPath, Set<MemberPath>>();
		foreach (Cell cell in cells)
		{
			foreach (MemberPath item2 in set)
			{
				IEnumerable<MemberPath> elements = from pos in cell.SQuery.GetProjectedPositions(item2)
					select ((MemberProjectedSlot)cell.CQuery.ProjectedSlotAt(pos)).MemberPath;
				Set<MemberPath> value = null;
				if (!dictionary.TryGetValue(item2, out value))
				{
					value = (dictionary[item2] = new Set<MemberPath>());
				}
				value.AddRange(elements);
			}
		}
		Cell[] array = cells.ToArray();
		foreach (Cell cell3 in array)
		{
			foreach (MemberPath item3 in set)
			{
				Set<MemberPath> second = dictionary[item3];
				if (cell3.SQuery.GetProjectedMembers().Contains(item3))
				{
					Cell result = null;
					if (TryCreateAdditionalCellWithCondition(cell3, item3, conditionValue: true, ViewTarget.UpdateView, out result))
					{
						cells.Add(result);
					}
					if (TryCreateAdditionalCellWithCondition(cell3, item3, conditionValue: false, ViewTarget.UpdateView, out result))
					{
						cells.Add(result);
					}
					continue;
				}
				foreach (MemberPath item4 in cell3.CQuery.GetProjectedMembers().Intersect(second))
				{
					Cell result2 = null;
					if (TryCreateAdditionalCellWithCondition(cell3, item4, conditionValue: true, ViewTarget.QueryView, out result2))
					{
						cells.Add(result2);
					}
					if (TryCreateAdditionalCellWithCondition(cell3, item4, conditionValue: false, ViewTarget.QueryView, out result2))
					{
						cells.Add(result2);
					}
				}
			}
		}
	}

	private bool TryCreateAdditionalCellWithCondition(Cell originalCell, MemberPath memberToExpand, bool conditionValue, ViewTarget viewTarget, out Cell result)
	{
		result = null;
		MemberPath sourceExtentMemberPath = originalCell.GetLeftQuery(viewTarget).SourceExtentMemberPath;
		MemberPath sourceExtentMemberPath2 = originalCell.GetRightQuery(viewTarget).SourceExtentMemberPath;
		int slotNum = originalCell.GetLeftQuery(viewTarget).GetProjectedMembers().TakeWhile((MemberPath path) => !path.Equals(memberToExpand))
			.Count();
		MemberProjectedSlot memberProjectedSlot = (MemberProjectedSlot)originalCell.GetRightQuery(viewTarget).ProjectedSlotAt(slotNum);
		MemberPath rightSidePath = memberProjectedSlot.MemberPath;
		List<ProjectedSlot> list = new List<ProjectedSlot>();
		List<ProjectedSlot> list2 = new List<ProjectedSlot>();
		ScalarConstant negatedCondition = new ScalarConstant(!conditionValue);
		if ((from restriction in originalCell.GetLeftQuery(viewTarget).Conditions
			where restriction.RestrictedMemberSlot.MemberPath.Equals(memberToExpand)
			where restriction.Domain.Values.Contains(negatedCondition)
			select restriction).Any() || (from restriction in originalCell.GetRightQuery(viewTarget).Conditions
			where restriction.RestrictedMemberSlot.MemberPath.Equals(rightSidePath)
			where restriction.Domain.Values.Contains(negatedCondition)
			select restriction).Any())
		{
			return false;
		}
		for (int i = 0; i < originalCell.GetLeftQuery(viewTarget).NumProjectedSlots; i++)
		{
			list.Add(originalCell.GetLeftQuery(viewTarget).ProjectedSlotAt(i));
		}
		for (int j = 0; j < originalCell.GetRightQuery(viewTarget).NumProjectedSlots; j++)
		{
			list2.Add(originalCell.GetRightQuery(viewTarget).ProjectedSlotAt(j));
		}
		BoolExpression boolExpression = BoolExpression.CreateLiteral(new ScalarRestriction(memberToExpand, new ScalarConstant(conditionValue)), null);
		boolExpression = BoolExpression.CreateAnd(originalCell.GetLeftQuery(viewTarget).WhereClause, boolExpression);
		BoolExpression boolExpression2 = BoolExpression.CreateLiteral(new ScalarRestriction(rightSidePath, new ScalarConstant(conditionValue)), null);
		boolExpression2 = BoolExpression.CreateAnd(originalCell.GetRightQuery(viewTarget).WhereClause, boolExpression2);
		CellQuery cellQuery = new CellQuery(list2, boolExpression2, sourceExtentMemberPath2, originalCell.GetRightQuery(viewTarget).SelectDistinctFlag);
		CellQuery cellQuery2 = new CellQuery(list, boolExpression, sourceExtentMemberPath, originalCell.GetLeftQuery(viewTarget).SelectDistinctFlag);
		Cell cell = ((viewTarget != ViewTarget.UpdateView) ? Cell.CreateCS(cellQuery2, cellQuery, originalCell.CellLabel, m_currentCellNumber) : Cell.CreateCS(cellQuery, cellQuery2, originalCell.CellLabel, m_currentCellNumber));
		m_currentCellNumber++;
		result = cell;
		return true;
	}

	private void ExtractCells(List<Cell> cells)
	{
		foreach (EntitySetBaseMapping allSetMap in m_containerMapping.AllSetMaps)
		{
			foreach (TypeMapping typeMapping in allSetMap.TypeMappings)
			{
				EntityTypeMapping entityTypeMapping = typeMapping as EntityTypeMapping;
				Set<EdmType> set = new Set<EdmType>();
				if (entityTypeMapping != null)
				{
					set.AddRange(entityTypeMapping.Types);
					foreach (EntityTypeBase isOfType in entityTypeMapping.IsOfTypes)
					{
						IEnumerable<EdmType> typeAndSubtypesOf = MetadataHelper.GetTypeAndSubtypesOf(isOfType, m_containerMapping.StorageMappingItemCollection.EdmItemCollection, includeAbstractTypes: false);
						set.AddRange(typeAndSubtypesOf);
					}
				}
				EntitySetBase set2 = allSetMap.Set;
				foreach (MappingFragment mappingFragment in typeMapping.MappingFragments)
				{
					ExtractCellsFromTableFragment(set2, mappingFragment, set, cells);
				}
			}
		}
	}

	private void ExtractCellsFromTableFragment(EntitySetBase extent, MappingFragment fragmentMap, Set<EdmType> allTypes, List<Cell> cells)
	{
		MemberPath memberPath = new MemberPath(extent);
		BoolExpression cQueryWhereClause = BoolExpression.True;
		List<ProjectedSlot> list = new List<ProjectedSlot>();
		if (allTypes.Count > 0)
		{
			cQueryWhereClause = BoolExpression.CreateLiteral(new TypeRestriction(memberPath, allTypes), null);
		}
		MemberPath memberPath2 = new MemberPath(fragmentMap.TableSet);
		BoolExpression sQueryWhereClause = BoolExpression.True;
		List<ProjectedSlot> list2 = new List<ProjectedSlot>();
		ExtractProperties(fragmentMap.AllProperties, memberPath, list, ref cQueryWhereClause, memberPath2, list2, ref sQueryWhereClause);
		CellQuery cQuery = new CellQuery(list, cQueryWhereClause, memberPath, CellQuery.SelectDistinct.No);
		CellQuery sQuery = new CellQuery(list2, sQueryWhereClause, memberPath2, (!fragmentMap.IsSQueryDistinct) ? CellQuery.SelectDistinct.No : CellQuery.SelectDistinct.Yes);
		CellLabel label = new CellLabel(fragmentMap);
		Cell item = Cell.CreateCS(cQuery, sQuery, label, m_currentCellNumber);
		m_currentCellNumber++;
		cells.Add(item);
	}

	private void ExtractProperties(IEnumerable<PropertyMapping> properties, MemberPath cNode, List<ProjectedSlot> cSlots, ref BoolExpression cQueryWhereClause, MemberPath sRootExtent, List<ProjectedSlot> sSlots, ref BoolExpression sQueryWhereClause)
	{
		foreach (PropertyMapping property in properties)
		{
			ScalarPropertyMapping scalarPropertyMapping = property as ScalarPropertyMapping;
			ComplexPropertyMapping complexPropertyMapping = property as ComplexPropertyMapping;
			EndPropertyMapping endPropertyMapping = property as EndPropertyMapping;
			ConditionPropertyMapping conditionPropertyMapping = property as ConditionPropertyMapping;
			if (scalarPropertyMapping != null)
			{
				MemberPath node = new MemberPath(cNode, scalarPropertyMapping.Property);
				MemberPath node2 = new MemberPath(sRootExtent, scalarPropertyMapping.Column);
				cSlots.Add(new MemberProjectedSlot(node));
				sSlots.Add(new MemberProjectedSlot(node2));
			}
			if (complexPropertyMapping != null)
			{
				foreach (ComplexTypeMapping typeMapping in complexPropertyMapping.TypeMappings)
				{
					MemberPath memberPath = new MemberPath(cNode, complexPropertyMapping.Property);
					Set<EdmType> set = new Set<EdmType>();
					IEnumerable<EdmType> elements = Helpers.AsSuperTypeList<ComplexType, EdmType>(typeMapping.Types);
					set.AddRange(elements);
					foreach (ComplexType isOfType in typeMapping.IsOfTypes)
					{
						set.AddRange(MetadataHelper.GetTypeAndSubtypesOf(isOfType, m_containerMapping.StorageMappingItemCollection.EdmItemCollection, includeAbstractTypes: false));
					}
					BoolExpression boolExpression = BoolExpression.CreateLiteral(new TypeRestriction(memberPath, set), null);
					cQueryWhereClause = BoolExpression.CreateAnd(cQueryWhereClause, boolExpression);
					ExtractProperties(typeMapping.AllProperties, memberPath, cSlots, ref cQueryWhereClause, sRootExtent, sSlots, ref sQueryWhereClause);
				}
			}
			if (endPropertyMapping != null)
			{
				MemberPath cNode2 = new MemberPath(cNode, endPropertyMapping.AssociationEnd);
				ExtractProperties(endPropertyMapping.PropertyMappings, cNode2, cSlots, ref cQueryWhereClause, sRootExtent, sSlots, ref sQueryWhereClause);
			}
			if (conditionPropertyMapping != null)
			{
				if (conditionPropertyMapping.Column != null)
				{
					BoolExpression conditionExpression = GetConditionExpression(sRootExtent, conditionPropertyMapping);
					sQueryWhereClause = BoolExpression.CreateAnd(sQueryWhereClause, conditionExpression);
				}
				else
				{
					BoolExpression conditionExpression2 = GetConditionExpression(cNode, conditionPropertyMapping);
					cQueryWhereClause = BoolExpression.CreateAnd(cQueryWhereClause, conditionExpression2);
				}
			}
		}
	}

	private static BoolExpression GetConditionExpression(MemberPath member, ConditionPropertyMapping conditionMap)
	{
		EdmMember edmMember = ((conditionMap.Column != null) ? conditionMap.Column : conditionMap.Property);
		MemberPath member2 = new MemberPath(member, edmMember);
		MemberRestriction memberRestriction = null;
		if (conditionMap.IsNull.HasValue)
		{
			Constant value = (conditionMap.IsNull.Value ? Constant.Null : Constant.NotNull);
			memberRestriction = ((!MetadataHelper.IsNonRefSimpleMember(edmMember)) ? ((MemberRestriction)new TypeRestriction(member2, value)) : ((MemberRestriction)new ScalarRestriction(member2, value)));
		}
		else
		{
			memberRestriction = new ScalarRestriction(member2, new ScalarConstant(conditionMap.Value));
		}
		return BoolExpression.CreateLiteral(memberRestriction, null);
	}

	private static bool IsBooleanMember(MemberPath path)
	{
		if (path.EdmType is PrimitiveType primitiveType)
		{
			return primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Boolean;
		}
		return false;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append("CellCreator");
	}
}

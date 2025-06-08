#define TRACE
using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class ForeignConstraint : InternalBase
{
	private readonly AssociationSet m_fKeySet;

	private readonly EntitySet m_parentTable;

	private readonly EntitySet m_childTable;

	private readonly List<MemberPath> m_parentColumns;

	private readonly List<MemberPath> m_childColumns;

	internal EntitySet ParentTable => m_parentTable;

	internal EntitySet ChildTable => m_childTable;

	internal IEnumerable<MemberPath> ChildColumns => m_childColumns;

	internal IEnumerable<MemberPath> ParentColumns => m_parentColumns;

	internal ForeignConstraint(AssociationSet i_fkeySet, EntitySet i_parentTable, EntitySet i_childTable, ReadOnlyMetadataCollection<EdmProperty> i_parentColumns, ReadOnlyMetadataCollection<EdmProperty> i_childColumns)
	{
		m_fKeySet = i_fkeySet;
		m_parentTable = i_parentTable;
		m_childTable = i_childTable;
		m_childColumns = new List<MemberPath>();
		foreach (EdmProperty i_childColumn in i_childColumns)
		{
			MemberPath item = new MemberPath(m_childTable, i_childColumn);
			m_childColumns.Add(item);
		}
		m_parentColumns = new List<MemberPath>();
		foreach (EdmProperty i_parentColumn in i_parentColumns)
		{
			MemberPath item2 = new MemberPath(m_parentTable, i_parentColumn);
			m_parentColumns.Add(item2);
		}
	}

	internal static List<ForeignConstraint> GetForeignConstraints(EntityContainer container)
	{
		List<ForeignConstraint> list = new List<ForeignConstraint>();
		foreach (EntitySetBase baseEntitySet in container.BaseEntitySets)
		{
			if (!(baseEntitySet is AssociationSet associationSet))
			{
				continue;
			}
			Dictionary<string, EntitySet> dictionary = new Dictionary<string, EntitySet>();
			foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
			{
				dictionary.Add(associationSetEnd.Name, associationSetEnd.EntitySet);
			}
			foreach (ReferentialConstraint referentialConstraint in associationSet.ElementType.ReferentialConstraints)
			{
				EntitySet i_parentTable = dictionary[referentialConstraint.FromRole.Name];
				EntitySet i_childTable = dictionary[referentialConstraint.ToRole.Name];
				ForeignConstraint item = new ForeignConstraint(associationSet, i_parentTable, i_childTable, referentialConstraint.FromProperties, referentialConstraint.ToProperties);
				list.Add(item);
			}
		}
		return list;
	}

	internal void CheckConstraint(Set<Cell> cells, QueryRewriter childRewriter, QueryRewriter parentRewriter, ErrorLog errorLog, ConfigViewGenerator config)
	{
		if (!IsConstraintRelevantForCells(cells))
		{
			return;
		}
		if (config.IsNormalTracing)
		{
			Trace.WriteLine(string.Empty);
			Trace.WriteLine(string.Empty);
			Trace.Write("Checking: ");
			Trace.WriteLine(this);
		}
		if (childRewriter == null && parentRewriter == null)
		{
			return;
		}
		if (childRewriter == null)
		{
			string message = Strings.ViewGen_Foreign_Key_Missing_Table_Mapping(ToUserString(), ChildTable.Name);
			ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.ForeignKeyMissingTableMapping, message, parentRewriter.UsedCells, string.Empty);
			errorLog.AddEntry(record);
		}
		else if (parentRewriter == null)
		{
			string message2 = Strings.ViewGen_Foreign_Key_Missing_Table_Mapping(ToUserString(), ParentTable.Name);
			ErrorLog.Record record2 = new ErrorLog.Record(ViewGenErrorCode.ForeignKeyMissingTableMapping, message2, childRewriter.UsedCells, string.Empty);
			errorLog.AddEntry(record2);
		}
		else if (!CheckIfConstraintMappedToForeignKeyAssociation(childRewriter, cells))
		{
			int count = errorLog.Count;
			if (IsForeignKeySuperSetOfPrimaryKeyInChildTable())
			{
				GuaranteeForeignKeyConstraintInCSpace(childRewriter, parentRewriter, errorLog);
			}
			else
			{
				GuaranteeMappedRelationshipForForeignKey(childRewriter, parentRewriter, cells, errorLog, config);
			}
			if (count == errorLog.Count)
			{
				CheckForeignKeyColumnOrder(cells, errorLog);
			}
		}
	}

	private void GuaranteeForeignKeyConstraintInCSpace(QueryRewriter childRewriter, QueryRewriter parentRewriter, ErrorLog errorLog)
	{
		ViewgenContext viewgenContext = childRewriter.ViewgenContext;
		ViewgenContext viewgenContext2 = parentRewriter.ViewgenContext;
		CellTreeNode basicView = childRewriter.BasicView;
		CellTreeNode basicView2 = parentRewriter.BasicView;
		if (!FragmentQueryProcessor.Merge(viewgenContext.RightFragmentQP, viewgenContext2.RightFragmentQP).IsContainedIn(basicView.RightFragmentQuery, basicView2.RightFragmentQuery))
		{
			string message = Strings.ViewGen_Foreign_Key_Not_Guaranteed_InCSpace(ToUserString());
			Set<LeftCellWrapper> set = new Set<LeftCellWrapper>(basicView2.GetLeaves());
			set.AddRange(basicView.GetLeaves());
			ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.ForeignKeyNotGuaranteedInCSpace, message, set, string.Empty);
			errorLog.AddEntry(record);
		}
	}

	private void GuaranteeMappedRelationshipForForeignKey(QueryRewriter childRewriter, QueryRewriter parentRewriter, IEnumerable<Cell> cells, ErrorLog errorLog, ConfigViewGenerator config)
	{
		ViewgenContext viewgenContext = childRewriter.ViewgenContext;
		ViewgenContext viewgenContext2 = parentRewriter.ViewgenContext;
		IEnumerable<MemberPath> keyFields = ExtentKey.GetPrimaryKeyForEntityType(new MemberPath(ChildTable), ChildTable.ElementType).KeyFields;
		bool flag = false;
		bool flag2 = false;
		List<ErrorLog.Record> errorList = null;
		foreach (Cell cell in cells)
		{
			if (!cell.SQuery.Extent.Equals(ChildTable))
			{
				continue;
			}
			AssociationEndMember relationEndForColumns = GetRelationEndForColumns(cell, ChildColumns);
			if (relationEndForColumns != null && !CheckParentColumnsForForeignKey(cell, cells, relationEndForColumns, ref errorList))
			{
				continue;
			}
			flag2 = true;
			if (GetRelationEndForColumns(cell, keyFields) != null && relationEndForColumns != null && FindEntitySetForColumnsMappedToEntityKeys(cells, keyFields).Count > 0)
			{
				flag = true;
				CheckConstraintWhenParentChildMapped(cell, errorLog, relationEndForColumns, config);
				break;
			}
			if (relationEndForColumns != null)
			{
				flag = CheckConstraintWhenOnlyParentMapped((AssociationSet)cell.CQuery.Extent, relationEndForColumns, childRewriter, parentRewriter);
				if (flag)
				{
					break;
				}
			}
		}
		if (!flag2)
		{
			foreach (ErrorLog.Record item in errorList)
			{
				errorLog.AddEntry(item);
			}
			return;
		}
		if (!flag)
		{
			string message = Strings.ViewGen_Foreign_Key_Missing_Relationship_Mapping(ToUserString());
			List<LeftCellWrapper> wrappersFromContext = GetWrappersFromContext(viewgenContext2, ParentTable);
			IEnumerable<LeftCellWrapper> wrappersFromContext2 = GetWrappersFromContext(viewgenContext, ChildTable);
			Set<LeftCellWrapper> set = new Set<LeftCellWrapper>(wrappersFromContext);
			set.AddRange(wrappersFromContext2);
			ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.ForeignKeyMissingRelationshipMapping, message, set, string.Empty);
			errorLog.AddEntry(record);
		}
	}

	private bool CheckIfConstraintMappedToForeignKeyAssociation(QueryRewriter childRewriter, Set<Cell> cells)
	{
		ViewgenContext viewgenContext = childRewriter.ViewgenContext;
		List<Set<EdmProperty>> list = new List<Set<EdmProperty>>();
		List<Set<EdmProperty>> list2 = new List<Set<EdmProperty>>();
		foreach (Cell cell in cells)
		{
			if (cell.CQuery.Extent.BuiltInTypeKind != BuiltInTypeKind.AssociationSet)
			{
				Set<EdmProperty> cSlotsForTableColumns = cell.GetCSlotsForTableColumns(ChildColumns);
				if (cSlotsForTableColumns != null && cSlotsForTableColumns.Count != 0)
				{
					list.Add(cSlotsForTableColumns);
				}
				Set<EdmProperty> cSlotsForTableColumns2 = cell.GetCSlotsForTableColumns(ParentColumns);
				if (cSlotsForTableColumns2 != null && cSlotsForTableColumns2.Count != 0)
				{
					list2.Add(cSlotsForTableColumns2);
				}
			}
		}
		if (list.Count != 0 && list2.Count != 0)
		{
			foreach (AssociationType item in from it in viewgenContext.EntityContainerMapping.EdmEntityContainer.BaseEntitySets.OfType<AssociationSet>()
				where it.ElementType.IsForeignKey
				select it.ElementType)
			{
				ReferentialConstraint refConstraint = item.ReferentialConstraints.FirstOrDefault();
				IEnumerable<Set<EdmProperty>> enumerable = list.Where((Set<EdmProperty> it) => it.SetEquals(new Set<EdmProperty>(refConstraint.ToProperties)));
				IEnumerable<Set<EdmProperty>> enumerable2 = list2.Where((Set<EdmProperty> it) => it.SetEquals(new Set<EdmProperty>(refConstraint.FromProperties)));
				if (enumerable.Count() == 0 || enumerable2.Count() == 0)
				{
					continue;
				}
				foreach (Set<EdmProperty> item2 in enumerable2)
				{
					Set<int> propertyIndexes = GetPropertyIndexes(item2, refConstraint.FromProperties);
					foreach (Set<EdmProperty> item3 in enumerable)
					{
						if (GetPropertyIndexes(item3, refConstraint.ToProperties).SequenceEqual(propertyIndexes))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private static Set<int> GetPropertyIndexes(IEnumerable<EdmProperty> properties1, ReadOnlyMetadataCollection<EdmProperty> properties2)
	{
		Set<int> set = new Set<int>();
		foreach (EdmProperty item in properties1)
		{
			set.Add(properties2.IndexOf(item));
		}
		return set;
	}

	private static bool CheckConstraintWhenOnlyParentMapped(AssociationSet assocSet, AssociationEndMember endMember, QueryRewriter childRewriter, QueryRewriter parentRewriter)
	{
		ViewgenContext viewgenContext = childRewriter.ViewgenContext;
		ViewgenContext viewgenContext2 = parentRewriter.ViewgenContext;
		CellTreeNode basicView = parentRewriter.BasicView;
		RoleBoolean literal = new RoleBoolean(assocSet.AssociationSetEnds[endMember.Name]);
		FragmentQuery q = FragmentQuery.Create(whereClause: basicView.RightFragmentQuery.Condition.Create(literal), attrs: basicView.RightFragmentQuery.Attributes);
		return FragmentQueryProcessor.Merge(viewgenContext.RightFragmentQP, viewgenContext2.RightFragmentQP).IsContainedIn(q, basicView.RightFragmentQuery);
	}

	private bool CheckConstraintWhenParentChildMapped(Cell cell, ErrorLog errorLog, AssociationEndMember parentEnd, ConfigViewGenerator config)
	{
		bool flag = true;
		if (parentEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
		{
			string message = Strings.ViewGen_Foreign_Key_UpperBound_MustBeOne(ToUserString(), cell.CQuery.Extent.Name, parentEnd.Name);
			ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.ForeignKeyUpperBoundMustBeOne, message, cell, string.Empty);
			errorLog.AddEntry(record);
			flag = false;
		}
		if (!MemberPath.AreAllMembersNullable(ChildColumns) && parentEnd.RelationshipMultiplicity != RelationshipMultiplicity.One)
		{
			string message2 = Strings.ViewGen_Foreign_Key_LowerBound_MustBeOne(ToUserString(), cell.CQuery.Extent.Name, parentEnd.Name);
			ErrorLog.Record record2 = new ErrorLog.Record(ViewGenErrorCode.ForeignKeyLowerBoundMustBeOne, message2, cell, string.Empty);
			errorLog.AddEntry(record2);
			flag = false;
		}
		if (config.IsNormalTracing && flag)
		{
			Trace.WriteLine("Foreign key mapped to relationship " + cell.CQuery.Extent.Name);
		}
		return flag;
	}

	private bool CheckParentColumnsForForeignKey(Cell cell, IEnumerable<Cell> cells, AssociationEndMember parentEnd, ref List<ErrorLog.Record> errorList)
	{
		EntitySet entitySetAtEnd = MetadataHelper.GetEntitySetAtEnd((AssociationSet)cell.CQuery.Extent, parentEnd);
		if (!FindEntitySetForColumnsMappedToEntityKeys(cells, ParentColumns).Contains(entitySetAtEnd))
		{
			if (errorList == null)
			{
				errorList = new List<ErrorLog.Record>();
			}
			string message = Strings.ViewGen_Foreign_Key_ParentTable_NotMappedToEnd(ToUserString(), ChildTable.Name, cell.CQuery.Extent.Name, parentEnd.Name, ParentTable.Name, entitySetAtEnd.Name);
			ErrorLog.Record item = new ErrorLog.Record(ViewGenErrorCode.ForeignKeyParentTableNotMappedToEnd, message, cell, string.Empty);
			errorList.Add(item);
			return false;
		}
		return true;
	}

	private static IList<EntitySet> FindEntitySetForColumnsMappedToEntityKeys(IEnumerable<Cell> cells, IEnumerable<MemberPath> tableColumns)
	{
		List<EntitySet> list = new List<EntitySet>();
		foreach (Cell cell in cells)
		{
			CellQuery cQuery = cell.CQuery;
			if (cQuery.Extent is AssociationSet)
			{
				continue;
			}
			Set<EdmProperty> cSlotsForTableColumns = cell.GetCSlotsForTableColumns(tableColumns);
			if (cSlotsForTableColumns == null)
			{
				continue;
			}
			EntitySet entitySet = (EntitySet)cQuery.Extent;
			List<EdmProperty> list2 = new List<EdmProperty>();
			foreach (EdmProperty keyMember in entitySet.ElementType.KeyMembers)
			{
				list2.Add(keyMember);
			}
			if (new Set<EdmProperty>(list2).MakeReadOnly().SetEquals(cSlotsForTableColumns))
			{
				list.Add(entitySet);
			}
		}
		return list;
	}

	private static AssociationEndMember GetRelationEndForColumns(Cell cell, IEnumerable<MemberPath> columns)
	{
		if (cell.CQuery.Extent is EntitySet)
		{
			return null;
		}
		AssociationSet associationSet = (AssociationSet)cell.CQuery.Extent;
		foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
		{
			AssociationEndMember correspondingAssociationEndMember = associationSetEnd.CorrespondingAssociationEndMember;
			ExtentKey primaryKeyForEntityType = ExtentKey.GetPrimaryKeyForEntityType(new MemberPath(associationSet, correspondingAssociationEndMember), associationSetEnd.EntitySet.ElementType);
			List<int> projectedPositions = cell.CQuery.GetProjectedPositions(primaryKeyForEntityType.KeyFields);
			if (projectedPositions != null)
			{
				List<int> projectedPositions2 = cell.SQuery.GetProjectedPositions(columns, projectedPositions);
				if (projectedPositions2 != null && Helpers.IsSetEqual(projectedPositions2, projectedPositions, EqualityComparer<int>.Default))
				{
					return correspondingAssociationEndMember;
				}
			}
		}
		return null;
	}

	private static List<LeftCellWrapper> GetWrappersFromContext(ViewgenContext context, EntitySetBase extent)
	{
		if (context == null)
		{
			return new List<LeftCellWrapper>();
		}
		return context.AllWrappersForExtent;
	}

	private bool CheckForeignKeyColumnOrder(Set<Cell> cells, ErrorLog errorLog)
	{
		List<Cell> list = new List<Cell>();
		List<Cell> list2 = new List<Cell>();
		foreach (Cell cell2 in cells)
		{
			if (cell2.SQuery.Extent.Equals(ChildTable))
			{
				list2.Add(cell2);
			}
			if (cell2.SQuery.Extent.Equals(ParentTable))
			{
				list.Add(cell2);
			}
		}
		foreach (Cell item in list2)
		{
			List<List<int>> slotNumsForColumns = GetSlotNumsForColumns(item, ChildColumns);
			if (slotNumsForColumns.Count == 0)
			{
				continue;
			}
			List<MemberPath> list3 = null;
			List<MemberPath> list4 = null;
			Cell cell = null;
			foreach (List<int> item2 in slotNumsForColumns)
			{
				list3 = new List<MemberPath>(item2.Count);
				foreach (int item3 in item2)
				{
					MemberProjectedSlot memberProjectedSlot = (MemberProjectedSlot)item.CQuery.ProjectedSlotAt(item3);
					list3.Add(memberProjectedSlot.MemberPath);
				}
				foreach (Cell item4 in list)
				{
					List<List<int>> slotNumsForColumns2 = GetSlotNumsForColumns(item4, ParentColumns);
					if (slotNumsForColumns2.Count == 0)
					{
						continue;
					}
					foreach (List<int> item5 in slotNumsForColumns2)
					{
						list4 = new List<MemberPath>(item5.Count);
						foreach (int item6 in item5)
						{
							MemberProjectedSlot memberProjectedSlot2 = (MemberProjectedSlot)item4.CQuery.ProjectedSlotAt(item6);
							list4.Add(memberProjectedSlot2.MemberPath);
						}
						if (list3.Count != list4.Count)
						{
							continue;
						}
						bool flag = false;
						for (int i = 0; i < list3.Count; i++)
						{
							if (flag)
							{
								break;
							}
							MemberPath memberPath = list4[i];
							MemberPath memberPath2 = list3[i];
							if (!memberPath.LeafEdmMember.Equals(memberPath2.LeafEdmMember))
							{
								if (memberPath.IsEquivalentViaRefConstraint(memberPath2))
								{
									return true;
								}
								flag = true;
							}
						}
						if (!flag)
						{
							return true;
						}
						cell = item4;
					}
				}
			}
			string message = Strings.ViewGen_Foreign_Key_ColumnOrder_Incorrect(ToUserString(), MemberPath.PropertiesToUserString(ChildColumns, fullPath: false), ChildTable.Name, MemberPath.PropertiesToUserString(list3, fullPath: false), item.CQuery.Extent.Name, MemberPath.PropertiesToUserString(ParentColumns, fullPath: false), ParentTable.Name, MemberPath.PropertiesToUserString(list4, fullPath: false), cell.CQuery.Extent.Name);
			ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.ForeignKeyColumnOrderIncorrect, message, new Cell[2] { cell, item }, string.Empty);
			errorLog.AddEntry(record);
			return false;
		}
		return true;
	}

	private static List<List<int>> GetSlotNumsForColumns(Cell cell, IEnumerable<MemberPath> columns)
	{
		List<List<int>> list = new List<List<int>>();
		if (cell.CQuery.Extent is AssociationSet associationSet)
		{
			foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
			{
				List<int> associationEndSlots = cell.CQuery.GetAssociationEndSlots(associationSetEnd.CorrespondingAssociationEndMember);
				List<int> projectedPositions = cell.SQuery.GetProjectedPositions(columns, associationEndSlots);
				if (projectedPositions != null)
				{
					list.Add(projectedPositions);
				}
			}
		}
		else
		{
			List<int> projectedPositions2 = cell.SQuery.GetProjectedPositions(columns);
			if (projectedPositions2 != null)
			{
				list.Add(projectedPositions2);
			}
		}
		return list;
	}

	private bool IsForeignKeySuperSetOfPrimaryKeyInChildTable()
	{
		bool result = true;
		foreach (EdmProperty keyMember in m_childTable.ElementType.KeyMembers)
		{
			bool flag = false;
			foreach (MemberPath childColumn in m_childColumns)
			{
				if (childColumn.LeafEdmMember.Equals(keyMember))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private bool IsConstraintRelevantForCells(IEnumerable<Cell> cells)
	{
		bool result = false;
		foreach (Cell cell in cells)
		{
			EntitySetBase extent = cell.SQuery.Extent;
			if (extent.Equals(m_parentTable) || extent.Equals(m_childTable))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	internal string ToUserString()
	{
		string p = MemberPath.PropertiesToUserString(m_childColumns, fullPath: false);
		string p2 = MemberPath.PropertiesToUserString(m_parentColumns, fullPath: false);
		return Strings.ViewGen_Foreign_Key(m_fKeySet.Name, m_childTable.Name, p, m_parentTable.Name, p2);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append(m_fKeySet.Name + ": ");
		builder.Append(ToUserString());
	}
}

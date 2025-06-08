using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class CellQuery : InternalBase
{
	internal enum SelectDistinct
	{
		Yes,
		No
	}

	private List<BoolExpression> m_boolExprs;

	private readonly ProjectedSlot[] m_projectedSlots;

	private BoolExpression m_whereClause;

	private readonly BoolExpression m_originalWhereClause;

	private readonly SelectDistinct m_selectDistinct;

	private readonly MemberPath m_extentMemberPath;

	private BasicCellRelation m_basicCellRelation;

	internal SelectDistinct SelectDistinctFlag => m_selectDistinct;

	internal EntitySetBase Extent => m_extentMemberPath.Extent;

	internal int NumProjectedSlots => m_projectedSlots.Length;

	internal ProjectedSlot[] ProjectedSlots => m_projectedSlots;

	internal List<BoolExpression> BoolVars => m_boolExprs;

	internal int NumBoolVars => m_boolExprs.Count;

	internal BoolExpression WhereClause => m_whereClause;

	internal MemberPath SourceExtentMemberPath => m_extentMemberPath;

	internal BasicCellRelation BasicCellRelation => m_basicCellRelation;

	internal IEnumerable<MemberRestriction> Conditions => GetConjunctsFromOriginalWhereClause();

	internal CellQuery(List<ProjectedSlot> slots, BoolExpression whereClause, MemberPath rootMember, SelectDistinct eliminateDuplicates)
		: this(slots.ToArray(), whereClause, new List<BoolExpression>(), eliminateDuplicates, rootMember)
	{
	}

	internal CellQuery(ProjectedSlot[] projectedSlots, BoolExpression whereClause, List<BoolExpression> boolExprs, SelectDistinct elimDupl, MemberPath rootMember)
	{
		m_boolExprs = boolExprs;
		m_projectedSlots = projectedSlots;
		m_whereClause = whereClause;
		m_originalWhereClause = whereClause;
		m_selectDistinct = elimDupl;
		m_extentMemberPath = rootMember;
	}

	internal CellQuery(CellQuery source)
	{
		m_basicCellRelation = source.m_basicCellRelation;
		m_boolExprs = source.m_boolExprs;
		m_selectDistinct = source.m_selectDistinct;
		m_extentMemberPath = source.m_extentMemberPath;
		m_originalWhereClause = source.m_originalWhereClause;
		m_projectedSlots = source.m_projectedSlots;
		m_whereClause = source.m_whereClause;
	}

	private CellQuery(CellQuery existing, ProjectedSlot[] newSlots)
		: this(newSlots, existing.m_whereClause, existing.m_boolExprs, existing.m_selectDistinct, existing.m_extentMemberPath)
	{
	}

	internal ProjectedSlot ProjectedSlotAt(int slotNum)
	{
		return m_projectedSlots[slotNum];
	}

	internal ErrorLog.Record CheckForDuplicateFields(CellQuery cQuery, Cell sourceCell)
	{
		KeyToListMap<MemberProjectedSlot, int> keyToListMap = new KeyToListMap<MemberProjectedSlot, int>(ProjectedSlot.EqualityComparer);
		for (int i = 0; i < m_projectedSlots.Length; i++)
		{
			MemberProjectedSlot key = m_projectedSlots[i] as MemberProjectedSlot;
			keyToListMap.Add(key, i);
		}
		StringBuilder stringBuilder = null;
		bool flag = false;
		foreach (MemberProjectedSlot key2 in keyToListMap.Keys)
		{
			ReadOnlyCollection<int> readOnlyCollection = keyToListMap.ListForKey(key2);
			if (readOnlyCollection.Count <= 1 || cQuery.AreSlotsEquivalentViaRefConstraints(readOnlyCollection))
			{
				continue;
			}
			flag = true;
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(Strings.ViewGen_Duplicate_CProperties(Extent.Name));
				stringBuilder.AppendLine();
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			for (int j = 0; j < readOnlyCollection.Count; j++)
			{
				int num = readOnlyCollection[j];
				if (j != 0)
				{
					stringBuilder2.Append(", ");
				}
				MemberProjectedSlot memberProjectedSlot = (MemberProjectedSlot)cQuery.m_projectedSlots[num];
				stringBuilder2.Append(memberProjectedSlot.ToUserString());
			}
			stringBuilder.AppendLine(Strings.ViewGen_Duplicate_CProperties_IsMapped(key2.ToUserString(), stringBuilder2.ToString()));
		}
		if (!flag)
		{
			return null;
		}
		return new ErrorLog.Record(ViewGenErrorCode.DuplicateCPropertiesMapped, stringBuilder.ToString(), sourceCell, string.Empty);
	}

	private bool AreSlotsEquivalentViaRefConstraints(ReadOnlyCollection<int> cSideSlotIndexes)
	{
		if (!(Extent is AssociationSet))
		{
			return false;
		}
		if (cSideSlotIndexes.Count > 2)
		{
			return false;
		}
		MemberProjectedSlot obj = (MemberProjectedSlot)m_projectedSlots[cSideSlotIndexes[0]];
		MemberProjectedSlot memberProjectedSlot = (MemberProjectedSlot)m_projectedSlots[cSideSlotIndexes[1]];
		return obj.MemberPath.IsEquivalentViaRefConstraint(memberProjectedSlot.MemberPath);
	}

	internal ErrorLog.Record CheckForProjectedNotNullSlots(Cell sourceCell, IEnumerable<Cell> associationSets)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		foreach (MemberRestriction condition in Conditions)
		{
			if (!condition.Domain.ContainsNotNull() || MemberProjectedSlot.GetSlotForMember(m_projectedSlots, condition.RestrictedMemberSlot.MemberPath) != null)
			{
				continue;
			}
			bool flag2 = true;
			if (Extent is EntitySet)
			{
				bool flag3 = sourceCell.CQuery == this;
				ViewTarget target = ((!flag3) ? ViewTarget.UpdateView : ViewTarget.QueryView);
				CellQuery cellQuery = (flag3 ? sourceCell.SQuery : sourceCell.CQuery);
				EntitySet rightExtent = cellQuery.Extent as EntitySet;
				if (rightExtent != null)
				{
					foreach (AssociationSet association2 in (cellQuery.Extent as EntitySet).AssociationSets.Where((AssociationSet association) => association.AssociationSetEnds.Any((AssociationSetEnd end) => end.CorrespondingAssociationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One && MetadataHelper.GetOppositeEnd(end).EntitySet.EdmEquals(rightExtent))))
					{
						foreach (Cell item in associationSets.Where((Cell c) => c.GetRightQuery(target).Extent.EdmEquals(association2)))
						{
							if (MemberProjectedSlot.GetSlotForMember(item.GetLeftQuery(target).ProjectedSlots, condition.RestrictedMemberSlot.MemberPath) != null)
							{
								flag2 = false;
							}
						}
					}
				}
			}
			if (flag2)
			{
				stringBuilder.AppendLine(Strings.ViewGen_NotNull_No_Projected_Slot(condition.RestrictedMemberSlot.MemberPath.PathToString(false)));
				flag = true;
			}
		}
		if (!flag)
		{
			return null;
		}
		return new ErrorLog.Record(ViewGenErrorCode.NotNullNoProjectedSlot, stringBuilder.ToString(), sourceCell, string.Empty);
	}

	internal void FixMissingSlotAsDefaultConstant(int slotNumber, ConstantProjectedSlot slot)
	{
		m_projectedSlots[slotNumber] = slot;
	}

	internal void CreateFieldAlignedCellQueries(CellQuery otherQuery, MemberProjectionIndex projectedSlotMap, out CellQuery newMainQuery, out CellQuery newOtherQuery)
	{
		int count = projectedSlotMap.Count;
		ProjectedSlot[] array = new ProjectedSlot[count];
		ProjectedSlot[] array2 = new ProjectedSlot[count];
		for (int i = 0; i < m_projectedSlots.Length; i++)
		{
			MemberProjectedSlot memberProjectedSlot = m_projectedSlots[i] as MemberProjectedSlot;
			int num = projectedSlotMap.IndexOf(memberProjectedSlot.MemberPath);
			array[num] = m_projectedSlots[i];
			array2[num] = otherQuery.m_projectedSlots[i];
		}
		newMainQuery = new CellQuery(this, array);
		newOtherQuery = new CellQuery(otherQuery, array2);
	}

	internal Set<MemberPath> GetNonNullSlots()
	{
		Set<MemberPath> set = new Set<MemberPath>(MemberPath.EqualityComparer);
		ProjectedSlot[] projectedSlots = m_projectedSlots;
		foreach (ProjectedSlot projectedSlot in projectedSlots)
		{
			if (projectedSlot != null)
			{
				MemberProjectedSlot memberProjectedSlot = projectedSlot as MemberProjectedSlot;
				set.Add(memberProjectedSlot.MemberPath);
			}
		}
		return set;
	}

	internal ErrorLog.Record VerifyKeysPresent(Cell ownerCell, Func<object, object, string> formatEntitySetMessage, Func<object, object, object, string> formatAssociationSetMessage, ViewGenErrorCode errorCode)
	{
		List<MemberPath> list = new List<MemberPath>(1);
		List<ExtentKey> list2 = new List<ExtentKey>(1);
		if (Extent is EntitySet)
		{
			MemberPath memberPath = new MemberPath(Extent);
			list.Add(memberPath);
			EntityType entityType = (EntityType)Extent.ElementType;
			List<ExtentKey> keysForEntityType = ExtentKey.GetKeysForEntityType(memberPath, entityType);
			list2.Add(keysForEntityType[0]);
		}
		else
		{
			AssociationSet associationSet = (AssociationSet)Extent;
			foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
			{
				AssociationEndMember correspondingAssociationEndMember = associationSetEnd.CorrespondingAssociationEndMember;
				MemberPath memberPath2 = new MemberPath(associationSet, correspondingAssociationEndMember);
				list.Add(memberPath2);
				List<ExtentKey> keysForEntityType2 = ExtentKey.GetKeysForEntityType(memberPath2, MetadataHelper.GetEntityTypeForEnd(correspondingAssociationEndMember));
				list2.Add(keysForEntityType2[0]);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			MemberPath memberPath3 = list[i];
			if (MemberProjectedSlot.GetKeySlots(GetMemberProjectedSlots(), memberPath3) == null)
			{
				ExtentKey extentKey = list2[i];
				string message;
				if (Extent is EntitySet)
				{
					string arg = MemberPath.PropertiesToUserString(extentKey.KeyFields, fullPath: true);
					message = formatEntitySetMessage(arg, Extent.Name);
				}
				else
				{
					string name = memberPath3.RootEdmMember.Name;
					string arg2 = MemberPath.PropertiesToUserString(extentKey.KeyFields, fullPath: false);
					message = formatAssociationSetMessage(arg2, name, Extent.Name);
				}
				return new ErrorLog.Record(errorCode, message, ownerCell, string.Empty);
			}
		}
		return null;
	}

	internal IEnumerable<MemberPath> GetProjectedMembers()
	{
		foreach (MemberProjectedSlot memberProjectedSlot in GetMemberProjectedSlots())
		{
			yield return memberProjectedSlot.MemberPath;
		}
	}

	private IEnumerable<MemberProjectedSlot> GetMemberProjectedSlots()
	{
		ProjectedSlot[] projectedSlots = m_projectedSlots;
		for (int i = 0; i < projectedSlots.Length; i++)
		{
			if (projectedSlots[i] is MemberProjectedSlot memberProjectedSlot)
			{
				yield return memberProjectedSlot;
			}
		}
	}

	internal List<MemberProjectedSlot> GetAllQuerySlots()
	{
		HashSet<MemberProjectedSlot> hashSet = new HashSet<MemberProjectedSlot>(GetMemberProjectedSlots());
		hashSet.Add(new MemberProjectedSlot(SourceExtentMemberPath));
		foreach (MemberRestriction condition in Conditions)
		{
			hashSet.Add(condition.RestrictedMemberSlot);
		}
		return new List<MemberProjectedSlot>(hashSet);
	}

	internal int GetProjectedPosition(MemberProjectedSlot slot)
	{
		for (int i = 0; i < m_projectedSlots.Length; i++)
		{
			if (ProjectedSlot.EqualityComparer.Equals(slot, m_projectedSlots[i]))
			{
				return i;
			}
		}
		return -1;
	}

	internal List<int> GetProjectedPositions(MemberPath member)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < m_projectedSlots.Length; i++)
		{
			if (m_projectedSlots[i] is MemberProjectedSlot memberProjectedSlot && MemberPath.EqualityComparer.Equals(member, memberProjectedSlot.MemberPath))
			{
				list.Add(i);
			}
		}
		return list;
	}

	internal List<int> GetProjectedPositions(IEnumerable<MemberPath> paths)
	{
		List<int> list = new List<int>();
		foreach (MemberPath path in paths)
		{
			List<int> projectedPositions = GetProjectedPositions(path);
			if (projectedPositions.Count == 0)
			{
				return null;
			}
			list.Add(projectedPositions[0]);
		}
		return list;
	}

	internal List<int> GetAssociationEndSlots(AssociationEndMember endMember)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < m_projectedSlots.Length; i++)
		{
			if (m_projectedSlots[i] is MemberProjectedSlot memberProjectedSlot && memberProjectedSlot.MemberPath.RootEdmMember.Equals(endMember))
			{
				list.Add(i);
			}
		}
		return list;
	}

	internal List<int> GetProjectedPositions(IEnumerable<MemberPath> paths, List<int> slotsToSearchFrom)
	{
		List<int> list = new List<int>();
		foreach (MemberPath path in paths)
		{
			List<int> projectedPositions = GetProjectedPositions(path);
			if (projectedPositions.Count == 0)
			{
				return null;
			}
			int num = -1;
			if (projectedPositions.Count > 1)
			{
				for (int i = 0; i < projectedPositions.Count; i++)
				{
					if (slotsToSearchFrom.Contains(projectedPositions[i]))
					{
						num = projectedPositions[i];
					}
				}
				if (num == -1)
				{
					return null;
				}
			}
			else
			{
				num = projectedPositions[0];
			}
			list.Add(num);
		}
		return list;
	}

	internal void UpdateWhereClause(MemberDomainMap domainMap)
	{
		List<BoolExpression> list = new List<BoolExpression>();
		foreach (BoolExpression atom in WhereClause.Atoms)
		{
			MemberRestriction memberRestriction = atom.AsLiteral as MemberRestriction;
			IEnumerable<Constant> domain = domainMap.GetDomain(memberRestriction.RestrictedMemberSlot.MemberPath);
			MemberRestriction memberRestriction2 = memberRestriction.CreateCompleteMemberRestriction(domain);
			int num;
			if (memberRestriction is ScalarRestriction scalarRestriction && !scalarRestriction.Domain.Contains(Constant.Null) && !scalarRestriction.Domain.Contains(Constant.NotNull))
			{
				num = ((!scalarRestriction.Domain.Contains(Constant.Undefined)) ? 1 : 0);
				if (num != 0)
				{
					domainMap.AddSentinel(memberRestriction2.RestrictedMemberSlot.MemberPath);
				}
			}
			else
			{
				num = 0;
			}
			list.Add(BoolExpression.CreateLiteral(memberRestriction2, domainMap));
			if (num != 0)
			{
				domainMap.RemoveSentinel(memberRestriction2.RestrictedMemberSlot.MemberPath);
			}
		}
		if (list.Count > 0)
		{
			m_whereClause = BoolExpression.CreateAnd(list.ToArray());
		}
	}

	internal BoolExpression GetBoolVar(int varNum)
	{
		return m_boolExprs[varNum];
	}

	internal void InitializeBoolExpressions(int numBoolVars, int cellNum)
	{
		m_boolExprs = new List<BoolExpression>(numBoolVars);
		for (int i = 0; i < numBoolVars; i++)
		{
			m_boolExprs.Add(null);
		}
		m_boolExprs[cellNum] = BoolExpression.True;
	}

	internal IEnumerable<MemberRestriction> GetConjunctsFromWhereClause()
	{
		return GetConjunctsFromWhereClause(m_whereClause);
	}

	internal IEnumerable<MemberRestriction> GetConjunctsFromOriginalWhereClause()
	{
		return GetConjunctsFromWhereClause(m_originalWhereClause);
	}

	private static IEnumerable<MemberRestriction> GetConjunctsFromWhereClause(BoolExpression whereClause)
	{
		foreach (BoolExpression atom in whereClause.Atoms)
		{
			if (!atom.IsTrue)
			{
				yield return atom.AsLiteral as MemberRestriction;
			}
		}
	}

	internal void GetIdentifiers(CqlIdentifiers identifiers)
	{
		ProjectedSlot[] projectedSlots = m_projectedSlots;
		for (int i = 0; i < projectedSlots.Length; i++)
		{
			if (projectedSlots[i] is MemberProjectedSlot memberProjectedSlot)
			{
				memberProjectedSlot.MemberPath.GetIdentifiers(identifiers);
			}
		}
		m_extentMemberPath.GetIdentifiers(identifiers);
	}

	internal void CreateBasicCellRelation(ViewCellRelation viewCellRelation)
	{
		List<MemberProjectedSlot> allQuerySlots = GetAllQuerySlots();
		m_basicCellRelation = new BasicCellRelation(this, viewCellRelation, allQuerySlots);
	}

	internal override void ToCompactString(StringBuilder stringBuilder)
	{
		List<BoolExpression> boolExprs = m_boolExprs;
		int num = 0;
		bool flag = true;
		foreach (BoolExpression item in boolExprs)
		{
			if (item != null)
			{
				if (!flag)
				{
					stringBuilder.Append(",");
				}
				else
				{
					stringBuilder.Append("[");
				}
				StringUtil.FormatStringBuilder(stringBuilder, "C{0}", num);
				flag = false;
			}
			num++;
		}
		if (flag)
		{
			ToFullString(stringBuilder);
		}
		else
		{
			stringBuilder.Append("]");
		}
	}

	internal override void ToFullString(StringBuilder builder)
	{
		builder.Append("SELECT ");
		if (m_selectDistinct == SelectDistinct.Yes)
		{
			builder.Append("DISTINCT ");
		}
		StringUtil.ToSeparatedString(builder, m_projectedSlots, ", ", "_");
		if (m_boolExprs.Count > 0)
		{
			builder.Append(", Bool[");
			StringUtil.ToSeparatedString(builder, m_boolExprs, ", ", "_");
			builder.Append("]");
		}
		builder.Append(" FROM ");
		m_extentMemberPath.ToFullString(builder);
		if (!m_whereClause.IsTrue)
		{
			builder.Append(" WHERE ");
			m_whereClause.ToFullString(builder);
		}
	}

	public override string ToString()
	{
		return ToFullString();
	}
}

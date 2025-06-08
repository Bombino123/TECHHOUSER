using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class ErrorPatternMatcher
{
	private enum ComparisonOP
	{
		IsContainedIn,
		IsDisjointFrom
	}

	private readonly ViewgenContext m_viewgenContext;

	private readonly MemberDomainMap m_domainMap;

	private readonly ErrorLog m_errorLog;

	private readonly int m_originalErrorCount;

	private const int NUM_PARTITION_ERR_TO_FIND = 5;

	private ErrorPatternMatcher(ViewgenContext context, MemberDomainMap domainMap, ErrorLog errorLog)
	{
		m_viewgenContext = context;
		m_domainMap = domainMap;
		MemberPath.GetKeyMembers(context.Extent, domainMap);
		m_errorLog = errorLog;
		m_originalErrorCount = m_errorLog.Count;
	}

	public static bool FindMappingErrors(ViewgenContext context, MemberDomainMap domainMap, ErrorLog errorLog)
	{
		if (context.ViewTarget == ViewTarget.QueryView && !context.Config.IsValidationEnabled)
		{
			return false;
		}
		ErrorPatternMatcher errorPatternMatcher = new ErrorPatternMatcher(context, domainMap, errorLog);
		errorPatternMatcher.MatchMissingMappingErrors();
		errorPatternMatcher.MatchConditionErrors();
		errorPatternMatcher.MatchSplitErrors();
		if (errorPatternMatcher.m_errorLog.Count == errorPatternMatcher.m_originalErrorCount)
		{
			errorPatternMatcher.MatchPartitionErrors();
		}
		if (errorPatternMatcher.m_errorLog.Count > errorPatternMatcher.m_originalErrorCount)
		{
			ExceptionHelpers.ThrowMappingException(errorPatternMatcher.m_errorLog, errorPatternMatcher.m_viewgenContext.Config);
		}
		return false;
	}

	private void MatchMissingMappingErrors()
	{
		if (m_viewgenContext.ViewTarget != 0)
		{
			return;
		}
		Set<EdmType> set = new Set<EdmType>(MetadataHelper.GetTypeAndSubtypesOf(m_viewgenContext.Extent.ElementType, m_viewgenContext.EdmItemCollection, includeAbstractTypes: false));
		foreach (LeftCellWrapper item in m_viewgenContext.AllWrappersForExtent)
		{
			foreach (Cell cell in item.Cells)
			{
				foreach (MemberRestriction condition in cell.CQuery.Conditions)
				{
					foreach (Constant value in condition.Domain.Values)
					{
						if (value is TypeConstant typeConstant)
						{
							set.Remove(typeConstant.EdmType);
						}
					}
				}
			}
		}
		if (set.Count > 0)
		{
			m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.ErrorPatternMissingMappingError, Strings.ViewGen_Missing_Type_Mapping(BuildCommaSeparatedErrorString(set)), m_viewgenContext.AllWrappersForExtent, ""));
		}
	}

	private static bool HasNotNullCondition(CellQuery cellQuery, MemberPath member)
	{
		foreach (MemberRestriction item in cellQuery.GetConjunctsFromWhereClause())
		{
			if (!item.RestrictedMemberSlot.MemberPath.Equals(member))
			{
				continue;
			}
			if (item.Domain.Values.Contains(Constant.NotNull))
			{
				return true;
			}
			foreach (NegatedConstant item2 in from cellConstant in item.Domain.Values
				select cellConstant as NegatedConstant into negated
				where negated != null
				select negated)
			{
				if (item2.Elements.Contains(Constant.Null))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsMemberPartOfNotNullCondition(IEnumerable<LeftCellWrapper> wrappers, MemberPath leftMember, ViewTarget viewTarget)
	{
		foreach (LeftCellWrapper wrapper in wrappers)
		{
			CellQuery leftQuery = wrapper.OnlyInputCell.GetLeftQuery(viewTarget);
			if (HasNotNullCondition(leftQuery, leftMember))
			{
				return true;
			}
			CellQuery rightQuery = wrapper.OnlyInputCell.GetRightQuery(viewTarget);
			int num = leftQuery.GetProjectedMembers().TakeWhile((MemberPath path) => !path.Equals(leftMember)).Count();
			if (num < leftQuery.GetProjectedMembers().Count())
			{
				MemberPath memberPath = ((MemberProjectedSlot)rightQuery.ProjectedSlotAt(num)).MemberPath;
				if (HasNotNullCondition(rightQuery, memberPath))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void MatchConditionErrors()
	{
		List<LeftCellWrapper> allWrappersForExtent = m_viewgenContext.AllWrappersForExtent;
		Set<MemberPath> set = new Set<MemberPath>();
		Set<Dictionary<MemberPath, Set<Constant>>> set2 = new Set<Dictionary<MemberPath, Set<Constant>>>(new ConditionComparer());
		Dictionary<Dictionary<MemberPath, Set<Constant>>, LeftCellWrapper> dictionary = new Dictionary<Dictionary<MemberPath, Set<Constant>>, LeftCellWrapper>(new ConditionComparer());
		foreach (LeftCellWrapper item in allWrappersForExtent)
		{
			Dictionary<MemberPath, Set<Constant>> dictionary2 = new Dictionary<MemberPath, Set<Constant>>();
			foreach (MemberRestriction item2 in item.OnlyInputCell.GetLeftQuery(m_viewgenContext.ViewTarget).GetConjunctsFromWhereClause())
			{
				MemberPath memberPath = item2.RestrictedMemberSlot.MemberPath;
				if (!m_domainMap.IsConditionMember(memberPath))
				{
					continue;
				}
				ScalarRestriction scalarRestriction = item2 as ScalarRestriction;
				if (scalarRestriction != null && !set.Contains(memberPath) && !item.OnlyInputCell.CQuery.WhereClause.Equals(item.OnlyInputCell.SQuery.WhereClause) && !IsMemberPartOfNotNullCondition(allWrappersForExtent, memberPath, m_viewgenContext.ViewTarget))
				{
					CheckThatConditionMemberIsNotMapped(memberPath, allWrappersForExtent, set);
				}
				if (m_viewgenContext.ViewTarget == ViewTarget.UpdateView && scalarRestriction != null && memberPath.IsNullable && IsMemberPartOfNotNullCondition(new LeftCellWrapper[1] { item }, memberPath, m_viewgenContext.ViewTarget))
				{
					MemberPath rightMemberPath = GetRightMemberPath(memberPath, item);
					if (rightMemberPath != null && rightMemberPath.IsNullable && !IsMemberPartOfNotNullCondition(new LeftCellWrapper[1] { item }, rightMemberPath, m_viewgenContext.ViewTarget))
					{
						m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.ErrorPatternConditionError, Strings.Viewgen_ErrorPattern_NotNullConditionMappedToNullableMember(memberPath, rightMemberPath), item.OnlyInputCell, ""));
					}
				}
				foreach (Constant value2 in item2.Domain.Values)
				{
					if (!dictionary2.TryGetValue(memberPath, out var value))
					{
						value = new Set<Constant>(Constant.EqualityComparer);
						dictionary2.Add(memberPath, value);
					}
					value.Add(value2);
				}
			}
			if (dictionary2.Count <= 0)
			{
				continue;
			}
			if (set2.Contains(dictionary2))
			{
				if (!RightSideEqual(dictionary[dictionary2], item))
				{
					m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.ErrorPatternConditionError, Strings.Viewgen_ErrorPattern_DuplicateConditionValue(BuildCommaSeparatedErrorString(dictionary2.Keys)), ToIEnum(dictionary[dictionary2].OnlyInputCell, item.OnlyInputCell), ""));
				}
			}
			else
			{
				set2.Add(dictionary2);
				dictionary.Add(dictionary2, item);
			}
		}
	}

	private static MemberPath GetRightMemberPath(MemberPath conditionMember, LeftCellWrapper leftCellWrapper)
	{
		List<int> projectedPositions = leftCellWrapper.OnlyInputCell.GetRightQuery(ViewTarget.QueryView).GetProjectedPositions(conditionMember);
		if (projectedPositions.Count != 1)
		{
			return null;
		}
		int slotNum = projectedPositions.First();
		return ((MemberProjectedSlot)leftCellWrapper.OnlyInputCell.GetLeftQuery(ViewTarget.QueryView).ProjectedSlotAt(slotNum)).MemberPath;
	}

	private void MatchSplitErrors()
	{
		IEnumerable<LeftCellWrapper> enumerable = m_viewgenContext.AllWrappersForExtent.Where((LeftCellWrapper r) => !(r.LeftExtent is AssociationSet) && !(r.RightCellQuery.Extent is AssociationSet));
		if (m_viewgenContext.ViewTarget != ViewTarget.UpdateView || !enumerable.Any())
		{
			return;
		}
		LeftCellWrapper leftCellWrapper = enumerable.First();
		EntitySetBase extent = leftCellWrapper.RightCellQuery.Extent;
		foreach (LeftCellWrapper item in enumerable)
		{
			if (!item.RightCellQuery.Extent.EdmEquals(extent) && !RightSideEqual(item, leftCellWrapper))
			{
				m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.ErrorPatternSplittingError, Strings.Viewgen_ErrorPattern_TableMappedToMultipleES(item.LeftExtent.ToString(), item.RightCellQuery.Extent.ToString(), extent.ToString()), item.Cells.First(), ""));
			}
		}
	}

	private void MatchPartitionErrors()
	{
		List<LeftCellWrapper> allWrappersForExtent = m_viewgenContext.AllWrappersForExtent;
		int num = 0;
		foreach (LeftCellWrapper item in allWrappersForExtent)
		{
			foreach (LeftCellWrapper item2 in allWrappersForExtent.Skip(++num))
			{
				FragmentQuery fragmentQuery = CreateRightFragmentQuery(item);
				FragmentQuery fragmentQuery2 = CreateRightFragmentQuery(item2);
				bool num2 = CompareS(ComparisonOP.IsDisjointFrom, m_viewgenContext, item, item2, fragmentQuery, fragmentQuery2);
				bool flag = CompareC(ComparisonOP.IsDisjointFrom, m_viewgenContext, item, item2, fragmentQuery, fragmentQuery2);
				bool flag2;
				bool flag3;
				bool flag4;
				if (num2)
				{
					if (flag)
					{
						continue;
					}
					flag2 = CompareC(ComparisonOP.IsContainedIn, m_viewgenContext, item, item2, fragmentQuery, fragmentQuery2);
					flag3 = CompareC(ComparisonOP.IsContainedIn, m_viewgenContext, item2, item, fragmentQuery2, fragmentQuery);
					flag4 = flag2 && flag3;
					StringBuilder stringBuilder = new StringBuilder();
					if (flag4)
					{
						stringBuilder.Append(Strings.Viewgen_ErrorPattern_Partition_Disj_Eq);
					}
					else if (flag2 || flag3)
					{
						if (CSideHasDifferentEntitySets(item, item2))
						{
							stringBuilder.Append(Strings.Viewgen_ErrorPattern_Partition_Disj_Subs_Ref);
						}
						else
						{
							stringBuilder.Append(Strings.Viewgen_ErrorPattern_Partition_Disj_Subs);
						}
					}
					else
					{
						stringBuilder.Append(Strings.Viewgen_ErrorPattern_Partition_Disj_Unk);
					}
					m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.ErrorPatternInvalidPartitionError, stringBuilder.ToString(), ToIEnum(item.OnlyInputCell, item2.OnlyInputCell), ""));
					if (FoundTooManyErrors())
					{
						return;
					}
				}
				else
				{
					flag2 = CompareC(ComparisonOP.IsContainedIn, m_viewgenContext, item, item2, fragmentQuery, fragmentQuery2);
					flag3 = CompareC(ComparisonOP.IsContainedIn, m_viewgenContext, item2, item, fragmentQuery2, fragmentQuery);
				}
				bool flag5 = CompareS(ComparisonOP.IsContainedIn, m_viewgenContext, item, item2, fragmentQuery, fragmentQuery2);
				bool flag6 = CompareS(ComparisonOP.IsContainedIn, m_viewgenContext, item2, item, fragmentQuery2, fragmentQuery);
				flag4 = flag2 && flag3;
				if (flag5 && flag6)
				{
					if (flag4)
					{
						continue;
					}
					StringBuilder stringBuilder2 = new StringBuilder();
					if (flag)
					{
						stringBuilder2.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Disj);
					}
					else if (flag2 || flag3)
					{
						if (CSideHasDifferentEntitySets(item, item2))
						{
							stringBuilder2.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Subs_Ref);
						}
						else
						{
							if (item.LeftExtent.Equals(item2.LeftExtent))
							{
								GetTypesAndConditionForWrapper(item, out var hasCondition, out var edmTypes);
								GetTypesAndConditionForWrapper(item2, out var hasCondition2, out var edmTypes2);
								if (!hasCondition && !hasCondition2 && (edmTypes.Except(edmTypes2).Count() != 0 || edmTypes2.Except(edmTypes).Count() != 0) && (!CheckForStoreConditions(item) || !CheckForStoreConditions(item2)))
								{
									IEnumerable<string> list = edmTypes.Select((EdmType it) => it.FullName).Union(edmTypes2.Select((EdmType it) => it.FullName));
									m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.ErrorPatternConditionError, Strings.Viewgen_ErrorPattern_Partition_MultipleTypesMappedToSameTable_WithoutCondition(StringUtil.ToCommaSeparatedString(list), item.LeftExtent), ToIEnum(item.OnlyInputCell, item2.OnlyInputCell), ""));
									return;
								}
							}
							stringBuilder2.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Subs);
						}
					}
					else if (!IsQueryView() && (item.OnlyInputCell.CQuery.Extent is AssociationSet || item2.OnlyInputCell.CQuery.Extent is AssociationSet))
					{
						stringBuilder2.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Unk_Association);
					}
					else
					{
						stringBuilder2.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Unk);
					}
					m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.ErrorPatternInvalidPartitionError, stringBuilder2.ToString(), ToIEnum(item.OnlyInputCell, item2.OnlyInputCell), ""));
					if (FoundTooManyErrors())
					{
						return;
					}
				}
				else
				{
					if (!(flag5 || flag6) || (flag5 && flag2 && !flag3) || (flag6 && flag3 && !flag2))
					{
						continue;
					}
					StringBuilder stringBuilder3 = new StringBuilder();
					if (flag)
					{
						stringBuilder3.Append(Strings.Viewgen_ErrorPattern_Partition_Sub_Disj);
					}
					else if (flag4)
					{
						if (CSideHasDifferentEntitySets(item, item2))
						{
							stringBuilder3.Append(" " + Strings.Viewgen_ErrorPattern_Partition_Sub_Eq_Ref);
						}
						else
						{
							stringBuilder3.Append(Strings.Viewgen_ErrorPattern_Partition_Sub_Eq);
						}
					}
					else
					{
						stringBuilder3.Append(Strings.Viewgen_ErrorPattern_Partition_Sub_Unk);
					}
					m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.ErrorPatternInvalidPartitionError, stringBuilder3.ToString(), ToIEnum(item.OnlyInputCell, item2.OnlyInputCell), ""));
					if (FoundTooManyErrors())
					{
						return;
					}
				}
			}
		}
	}

	private static void GetTypesAndConditionForWrapper(LeftCellWrapper wrapper, out bool hasCondition, out List<EdmType> edmTypes)
	{
		hasCondition = false;
		edmTypes = new List<EdmType>();
		foreach (Cell cell in wrapper.Cells)
		{
			foreach (MemberRestriction condition in cell.CQuery.Conditions)
			{
				foreach (Constant value in condition.Domain.Values)
				{
					if (value is TypeConstant typeConstant)
					{
						edmTypes.Add(typeConstant.EdmType);
					}
					else
					{
						hasCondition = true;
					}
				}
			}
		}
	}

	private static bool CheckForStoreConditions(LeftCellWrapper wrapper)
	{
		return wrapper.Cells.SelectMany((Cell c) => c.SQuery.Conditions).Any();
	}

	private void CheckThatConditionMemberIsNotMapped(MemberPath conditionMember, List<LeftCellWrapper> mappingFragments, Set<MemberPath> mappedConditionMembers)
	{
		foreach (LeftCellWrapper mappingFragment in mappingFragments)
		{
			foreach (Cell cell in mappingFragment.Cells)
			{
				if (cell.GetLeftQuery(m_viewgenContext.ViewTarget).GetProjectedMembers().Contains(conditionMember))
				{
					mappedConditionMembers.Add(conditionMember);
					m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.ErrorPatternConditionError, Strings.Viewgen_ErrorPattern_ConditionMemberIsMapped(conditionMember.ToString()), cell, ""));
				}
			}
		}
	}

	private bool FoundTooManyErrors()
	{
		return m_errorLog.Count > m_originalErrorCount + 5;
	}

	private static string BuildCommaSeparatedErrorString<T>(IEnumerable<T> members)
	{
		StringBuilder stringBuilder = new StringBuilder();
		T val = members.First();
		foreach (T member in members)
		{
			if (!member.Equals(val))
			{
				stringBuilder.Append(", ");
			}
			T val2 = member;
			stringBuilder.Append("'" + val2?.ToString() + "'");
		}
		return stringBuilder.ToString();
	}

	private bool CSideHasDifferentEntitySets(LeftCellWrapper a, LeftCellWrapper b)
	{
		if (IsQueryView())
		{
			return a.LeftExtent == b.LeftExtent;
		}
		return a.RightCellQuery == b.RightCellQuery;
	}

	private bool CompareC(ComparisonOP op, ViewgenContext context, LeftCellWrapper leftWrapper1, LeftCellWrapper leftWrapper2, FragmentQuery rightQuery1, FragmentQuery rightQuery2)
	{
		return Compare(lookingForC: true, op, context, leftWrapper1, leftWrapper2, rightQuery1, rightQuery2);
	}

	private bool CompareS(ComparisonOP op, ViewgenContext context, LeftCellWrapper leftWrapper1, LeftCellWrapper leftWrapper2, FragmentQuery rightQuery1, FragmentQuery rightQuery2)
	{
		return Compare(lookingForC: false, op, context, leftWrapper1, leftWrapper2, rightQuery1, rightQuery2);
	}

	private bool Compare(bool lookingForC, ComparisonOP op, ViewgenContext context, LeftCellWrapper leftWrapper1, LeftCellWrapper leftWrapper2, FragmentQuery rightQuery1, FragmentQuery rightQuery2)
	{
		LCWComparer lCWComparer;
		if ((lookingForC && IsQueryView()) || (!lookingForC && !IsQueryView()))
		{
			switch (op)
			{
			case ComparisonOP.IsContainedIn:
				lCWComparer = context.LeftFragmentQP.IsContainedIn;
				break;
			case ComparisonOP.IsDisjointFrom:
				lCWComparer = context.LeftFragmentQP.IsDisjointFrom;
				break;
			default:
				return false;
			}
			return lCWComparer(leftWrapper1.FragmentQuery, leftWrapper2.FragmentQuery);
		}
		switch (op)
		{
		case ComparisonOP.IsContainedIn:
			lCWComparer = context.RightFragmentQP.IsContainedIn;
			break;
		case ComparisonOP.IsDisjointFrom:
			lCWComparer = context.RightFragmentQP.IsDisjointFrom;
			break;
		default:
			return false;
		}
		return lCWComparer(rightQuery1, rightQuery2);
	}

	private bool RightSideEqual(LeftCellWrapper wrapper1, LeftCellWrapper wrapper2)
	{
		FragmentQuery q = CreateRightFragmentQuery(wrapper1);
		FragmentQuery q2 = CreateRightFragmentQuery(wrapper2);
		return m_viewgenContext.RightFragmentQP.IsEquivalentTo(q, q2);
	}

	private FragmentQuery CreateRightFragmentQuery(LeftCellWrapper wrapper)
	{
		return FragmentQuery.Create(wrapper.OnlyInputCell.CellLabel.ToString(), wrapper.CreateRoleBoolean(), wrapper.OnlyInputCell.GetRightQuery(m_viewgenContext.ViewTarget));
	}

	private static IEnumerable<Cell> ToIEnum(Cell one, Cell two)
	{
		return new List<Cell> { one, two };
	}

	private bool IsQueryView()
	{
		return m_viewgenContext.ViewTarget == ViewTarget.QueryView;
	}
}

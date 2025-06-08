using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class LeftCellWrapper : InternalBase
{
	private class BoolWrapperComparer : IEqualityComparer<LeftCellWrapper>
	{
		public bool Equals(LeftCellWrapper left, LeftCellWrapper right)
		{
			if (left == right)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			bool flag = BoolExpression.EqualityComparer.Equals(left.RightCellQuery.WhereClause, right.RightCellQuery.WhereClause);
			return left.RightExtent.Equals(right.RightExtent) && flag;
		}

		public int GetHashCode(LeftCellWrapper wrapper)
		{
			return BoolExpression.EqualityComparer.GetHashCode(wrapper.RightCellQuery.WhereClause) ^ wrapper.RightExtent.GetHashCode();
		}
	}

	private class LeftCellWrapperComparer : IComparer<LeftCellWrapper>
	{
		public int Compare(LeftCellWrapper x, LeftCellWrapper y)
		{
			if (x.FragmentQuery.Attributes.Count > y.FragmentQuery.Attributes.Count)
			{
				return -1;
			}
			if (x.FragmentQuery.Attributes.Count < y.FragmentQuery.Attributes.Count)
			{
				return 1;
			}
			return string.CompareOrdinal(x.OriginalCellNumberString, y.OriginalCellNumberString);
		}
	}

	internal class CellIdComparer : IComparer<LeftCellWrapper>
	{
		public int Compare(LeftCellWrapper x, LeftCellWrapper y)
		{
			return StringComparer.Ordinal.Compare(x.OriginalCellNumberString, y.OriginalCellNumberString);
		}
	}

	internal static readonly IEqualityComparer<LeftCellWrapper> BoolEqualityComparer = new BoolWrapperComparer();

	private readonly Set<MemberPath> m_attributes;

	private readonly MemberMaps m_memberMaps;

	private readonly CellQuery m_leftCellQuery;

	private readonly CellQuery m_rightCellQuery;

	private readonly HashSet<Cell> m_mergedCells;

	private readonly ViewTarget m_viewTarget;

	private readonly FragmentQuery m_leftFragmentQuery;

	internal static readonly IComparer<LeftCellWrapper> Comparer = new LeftCellWrapperComparer();

	internal static readonly IComparer<LeftCellWrapper> OriginalCellIdComparer = new CellIdComparer();

	internal FragmentQuery FragmentQuery => m_leftFragmentQuery;

	internal Set<MemberPath> Attributes => m_attributes;

	internal string OriginalCellNumberString => StringUtil.ToSeparatedString(m_mergedCells.Select((Cell cell) => cell.CellNumberAsString), "+", "");

	internal MemberDomainMap RightDomainMap => m_memberMaps.RightDomainMap;

	internal IEnumerable<Cell> Cells => m_mergedCells;

	internal Cell OnlyInputCell => m_mergedCells.First();

	internal CellQuery RightCellQuery => m_rightCellQuery;

	internal CellQuery LeftCellQuery => m_leftCellQuery;

	internal EntitySetBase LeftExtent => m_mergedCells.First().GetLeftQuery(m_viewTarget).Extent;

	internal EntitySetBase RightExtent => m_rightCellQuery.Extent;

	internal LeftCellWrapper(ViewTarget viewTarget, Set<MemberPath> attrs, FragmentQuery fragmentQuery, CellQuery leftCellQuery, CellQuery rightCellQuery, MemberMaps memberMaps, IEnumerable<Cell> inputCells)
	{
		m_leftFragmentQuery = fragmentQuery;
		m_rightCellQuery = rightCellQuery;
		m_leftCellQuery = leftCellQuery;
		m_attributes = attrs;
		m_viewTarget = viewTarget;
		m_memberMaps = memberMaps;
		m_mergedCells = new HashSet<Cell>(inputCells);
	}

	internal LeftCellWrapper(ViewTarget viewTarget, Set<MemberPath> attrs, FragmentQuery fragmentQuery, CellQuery leftCellQuery, CellQuery rightCellQuery, MemberMaps memberMaps, Cell inputCell)
		: this(viewTarget, attrs, fragmentQuery, leftCellQuery, rightCellQuery, memberMaps, Enumerable.Repeat(inputCell, 1))
	{
	}

	[Conditional("DEBUG")]
	internal void AssertHasUniqueCell()
	{
	}

	internal static IEnumerable<Cell> GetInputCellsForWrappers(IEnumerable<LeftCellWrapper> wrappers)
	{
		foreach (LeftCellWrapper wrapper in wrappers)
		{
			foreach (Cell mergedCell in wrapper.m_mergedCells)
			{
				yield return mergedCell;
			}
		}
	}

	internal RoleBoolean CreateRoleBoolean()
	{
		if (RightExtent is AssociationSet)
		{
			Set<AssociationEndMember> endsForTablePrimaryKey = GetEndsForTablePrimaryKey();
			if (endsForTablePrimaryKey.Count == 1)
			{
				return new RoleBoolean(((AssociationSet)RightExtent).AssociationSetEnds[endsForTablePrimaryKey.First().Name]);
			}
		}
		return new RoleBoolean(RightExtent);
	}

	internal static string GetExtentListAsUserString(IEnumerable<LeftCellWrapper> wrappers)
	{
		Set<EntitySetBase> set = new Set<EntitySetBase>(EqualityComparer<EntitySetBase>.Default);
		foreach (LeftCellWrapper wrapper in wrappers)
		{
			set.Add(wrapper.RightExtent);
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (EntitySetBase item in set)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			flag = false;
			stringBuilder.Append(item.Name);
		}
		return stringBuilder.ToString();
	}

	internal override void ToFullString(StringBuilder builder)
	{
		builder.Append("P[");
		StringUtil.ToSeparatedString(builder, m_attributes, ",");
		builder.Append("] = ");
		m_rightCellQuery.ToFullString(builder);
	}

	internal override void ToCompactString(StringBuilder stringBuilder)
	{
		stringBuilder.Append(OriginalCellNumberString);
	}

	internal static void WrappersToStringBuilder(StringBuilder builder, List<LeftCellWrapper> wrappers, string header)
	{
		builder.AppendLine().Append(header).AppendLine();
		LeftCellWrapper[] array = wrappers.ToArray();
		Array.Sort(array, OriginalCellIdComparer);
		LeftCellWrapper[] array2 = array;
		foreach (LeftCellWrapper obj in array2)
		{
			obj.ToCompactString(builder);
			builder.Append(" = ");
			obj.ToFullString(builder);
			builder.AppendLine();
		}
	}

	private Set<AssociationEndMember> GetEndsForTablePrimaryKey()
	{
		CellQuery rightCellQuery = RightCellQuery;
		Set<AssociationEndMember> set = new Set<AssociationEndMember>(EqualityComparer<AssociationEndMember>.Default);
		foreach (int keySlot in m_memberMaps.ProjectedSlotMap.KeySlots)
		{
			AssociationEndMember element = (AssociationEndMember)((MemberProjectedSlot)rightCellQuery.ProjectedSlotAt(keySlot)).MemberPath.RootEdmMember;
			set.Add(element);
		}
		return set;
	}

	internal MemberProjectedSlot GetLeftSideMappedSlotForRightSideMember(MemberPath member)
	{
		int projectedPosition = RightCellQuery.GetProjectedPosition(new MemberProjectedSlot(member));
		if (projectedPosition == -1)
		{
			return null;
		}
		ProjectedSlot projectedSlot = LeftCellQuery.ProjectedSlotAt(projectedPosition);
		if (projectedSlot == null || projectedSlot is ConstantProjectedSlot)
		{
			return null;
		}
		return projectedSlot as MemberProjectedSlot;
	}

	internal MemberProjectedSlot GetRightSideMappedSlotForLeftSideMember(MemberPath member)
	{
		int projectedPosition = LeftCellQuery.GetProjectedPosition(new MemberProjectedSlot(member));
		if (projectedPosition == -1)
		{
			return null;
		}
		ProjectedSlot projectedSlot = RightCellQuery.ProjectedSlotAt(projectedPosition);
		if (projectedSlot == null || projectedSlot is ConstantProjectedSlot)
		{
			return null;
		}
		return projectedSlot as MemberProjectedSlot;
	}

	internal MemberProjectedSlot GetCSideMappedSlotForSMember(MemberPath member)
	{
		if (m_viewTarget == ViewTarget.QueryView)
		{
			return GetLeftSideMappedSlotForRightSideMember(member);
		}
		return GetRightSideMappedSlotForLeftSideMember(member);
	}
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class OpCellTreeNode : CellTreeNode
{
	private readonly Set<MemberPath> m_attrs;

	private readonly List<CellTreeNode> m_children;

	private readonly CellTreeOpType m_opType;

	private FragmentQuery m_leftFragmentQuery;

	private FragmentQuery m_rightFragmentQuery;

	internal override CellTreeOpType OpType => m_opType;

	internal override FragmentQuery LeftFragmentQuery
	{
		get
		{
			if (m_leftFragmentQuery == null)
			{
				m_leftFragmentQuery = GenerateFragmentQuery(Children, isLeft: true, base.ViewgenContext, OpType);
			}
			return m_leftFragmentQuery;
		}
	}

	internal override FragmentQuery RightFragmentQuery
	{
		get
		{
			if (m_rightFragmentQuery == null)
			{
				m_rightFragmentQuery = GenerateFragmentQuery(Children, isLeft: false, base.ViewgenContext, OpType);
			}
			return m_rightFragmentQuery;
		}
	}

	internal override MemberDomainMap RightDomainMap => m_children[0].RightDomainMap;

	internal override Set<MemberPath> Attributes => m_attrs;

	internal override List<CellTreeNode> Children => m_children;

	internal override int NumProjectedSlots => m_children[0].NumProjectedSlots;

	internal override int NumBoolSlots => m_children[0].NumBoolSlots;

	internal OpCellTreeNode(ViewgenContext context, CellTreeOpType opType)
		: base(context)
	{
		m_opType = opType;
		m_attrs = new Set<MemberPath>(MemberPath.EqualityComparer);
		m_children = new List<CellTreeNode>();
	}

	internal OpCellTreeNode(ViewgenContext context, CellTreeOpType opType, params CellTreeNode[] children)
		: this(context, opType, (IEnumerable<CellTreeNode>)children)
	{
	}

	internal OpCellTreeNode(ViewgenContext context, CellTreeOpType opType, IEnumerable<CellTreeNode> children)
		: this(context, opType)
	{
		foreach (CellTreeNode child in children)
		{
			Add(child);
		}
	}

	internal override TOutput Accept<TInput, TOutput>(SimpleCellTreeVisitor<TInput, TOutput> visitor, TInput param)
	{
		return visitor.VisitOpNode(this, param);
	}

	internal override TOutput Accept<TInput, TOutput>(CellTreeVisitor<TInput, TOutput> visitor, TInput param)
	{
		return OpType switch
		{
			CellTreeOpType.IJ => visitor.VisitInnerJoin(this, param), 
			CellTreeOpType.LOJ => visitor.VisitLeftOuterJoin(this, param), 
			CellTreeOpType.Union => visitor.VisitUnion(this, param), 
			CellTreeOpType.FOJ => visitor.VisitFullOuterJoin(this, param), 
			CellTreeOpType.LASJ => visitor.VisitLeftAntiSemiJoin(this, param), 
			_ => visitor.VisitInnerJoin(this, param), 
		};
	}

	internal void Add(CellTreeNode child)
	{
		Insert(m_children.Count, child);
	}

	internal void AddFirst(CellTreeNode child)
	{
		Insert(0, child);
	}

	private void Insert(int index, CellTreeNode child)
	{
		m_attrs.Unite(child.Attributes);
		m_children.Insert(index, child);
		m_leftFragmentQuery = null;
		m_rightFragmentQuery = null;
	}

	internal override CqlBlock ToCqlBlock(bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum, ref List<WithRelationship> withRelationships)
	{
		if (OpType == CellTreeOpType.Union)
		{
			return UnionToCqlBlock(requiredSlots, identifiers, ref blockAliasNum, ref withRelationships);
		}
		return JoinToCqlBlock(requiredSlots, identifiers, ref blockAliasNum, ref withRelationships);
	}

	internal override bool IsProjectedSlot(int slot)
	{
		foreach (CellTreeNode child in Children)
		{
			if (child.IsProjectedSlot(slot))
			{
				return true;
			}
		}
		return false;
	}

	private CqlBlock UnionToCqlBlock(bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum, ref List<WithRelationship> withRelationships)
	{
		List<CqlBlock> list = new List<CqlBlock>();
		List<Tuple<CqlBlock, SlotInfo>> list2 = new List<Tuple<CqlBlock, SlotInfo>>();
		int num = requiredSlots.Length;
		foreach (CellTreeNode child in Children)
		{
			bool[] projectedSlots = child.GetProjectedSlots();
			AndWith(projectedSlots, requiredSlots);
			CqlBlock cqlBlock = child.ToCqlBlock(projectedSlots, identifiers, ref blockAliasNum, ref withRelationships);
			for (int i = projectedSlots.Length; i < cqlBlock.Slots.Count; i++)
			{
				list2.Add(Tuple.Create(cqlBlock, cqlBlock.Slots[i]));
			}
			SlotInfo[] array = new SlotInfo[cqlBlock.Slots.Count];
			for (int j = 0; j < num; j++)
			{
				if (requiredSlots[j] && !projectedSlots[j])
				{
					if (IsBoolSlot(j))
					{
						array[j] = new SlotInfo(isRequiredByParent: true, isProjected: true, new BooleanProjectedSlot(BoolExpression.False, identifiers, SlotToBoolIndex(j)), null);
						continue;
					}
					MemberPath outputMember = cqlBlock.MemberPath(j);
					array[j] = new SlotInfo(isRequiredByParent: true, isProjected: true, new ConstantProjectedSlot(Constant.Null), outputMember);
				}
				else
				{
					array[j] = cqlBlock.Slots[j];
				}
			}
			cqlBlock.Slots = new ReadOnlyCollection<SlotInfo>(array);
			list.Add(cqlBlock);
		}
		if (list2.Count != 0)
		{
			foreach (CqlBlock item2 in list)
			{
				SlotInfo[] array2 = new SlotInfo[num + list2.Count];
				item2.Slots.CopyTo(array2, 0);
				int num2 = num;
				foreach (Tuple<CqlBlock, SlotInfo> item3 in list2)
				{
					SlotInfo item = item3.Item2;
					if (item3.Item1.Equals(item2))
					{
						array2[num2] = new SlotInfo(isRequiredByParent: true, isProjected: true, item.SlotValue, item.OutputMember);
					}
					else
					{
						array2[num2] = new SlotInfo(isRequiredByParent: true, isProjected: true, new ConstantProjectedSlot(Constant.Null), item.OutputMember);
					}
					num2++;
				}
				item2.Slots = new ReadOnlyCollection<SlotInfo>(array2);
			}
		}
		SlotInfo[] array3 = new SlotInfo[num + list2.Count];
		CqlBlock cqlBlock2 = list[0];
		for (int k = 0; k < num; k++)
		{
			SlotInfo slotInfo = cqlBlock2.Slots[k];
			bool flag = requiredSlots[k];
			array3[k] = new SlotInfo(flag, flag, slotInfo.SlotValue, slotInfo.OutputMember);
		}
		for (int l = num; l < num + list2.Count; l++)
		{
			SlotInfo slotInfo2 = cqlBlock2.Slots[l];
			array3[l] = new SlotInfo(isRequiredByParent: true, isProjected: true, slotInfo2.SlotValue, slotInfo2.OutputMember);
		}
		return new UnionCqlBlock(array3, list, identifiers, ++blockAliasNum);
	}

	private static void AndWith(bool[] boolArray, bool[] another)
	{
		for (int i = 0; i < boolArray.Length; i++)
		{
			ref bool reference = ref boolArray[i];
			reference &= another[i];
		}
	}

	private CqlBlock JoinToCqlBlock(bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum, ref List<WithRelationship> withRelationships)
	{
		int num = requiredSlots.Length;
		List<CqlBlock> list = new List<CqlBlock>();
		List<Tuple<QualifiedSlot, MemberPath>> list2 = new List<Tuple<QualifiedSlot, MemberPath>>();
		foreach (CellTreeNode child in Children)
		{
			bool[] projectedSlots = child.GetProjectedSlots();
			AndWith(projectedSlots, requiredSlots);
			CqlBlock cqlBlock = child.ToCqlBlock(projectedSlots, identifiers, ref blockAliasNum, ref withRelationships);
			list.Add(cqlBlock);
			for (int i = projectedSlots.Length; i < cqlBlock.Slots.Count; i++)
			{
				list2.Add(Tuple.Create(cqlBlock.QualifySlotWithBlockAlias(i), cqlBlock.MemberPath(i)));
			}
		}
		SlotInfo[] array = new SlotInfo[num + list2.Count];
		for (int j = 0; j < num; j++)
		{
			SlotInfo joinSlotInfo = GetJoinSlotInfo(OpType, requiredSlots[j], list, j, identifiers);
			array[j] = joinSlotInfo;
		}
		int num2 = 0;
		int num3 = num;
		while (num3 < num + list2.Count)
		{
			array[num3] = new SlotInfo(isRequiredByParent: true, isProjected: true, list2[num2].Item1, list2[num2].Item2);
			num3++;
			num2++;
		}
		List<JoinCqlBlock.OnClause> list3 = new List<JoinCqlBlock.OnClause>();
		for (int k = 1; k < list.Count; k++)
		{
			CqlBlock cqlBlock2 = list[k];
			JoinCqlBlock.OnClause onClause = new JoinCqlBlock.OnClause();
			foreach (int keySlot in base.KeySlots)
			{
				if (!base.ViewgenContext.Config.IsValidationEnabled && (!cqlBlock2.IsProjected(keySlot) || !list[0].IsProjected(keySlot)))
				{
					ErrorLog errorLog = new ErrorLog();
					errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.NoJoinKeyOrFKProvidedInMapping, Strings.Viewgen_NoJoinKeyOrFK, base.ViewgenContext.AllWrappersForExtent, string.Empty));
					ExceptionHelpers.ThrowMappingException(errorLog, base.ViewgenContext.Config);
				}
				QualifiedSlot leftSlot = list[0].QualifySlotWithBlockAlias(keySlot);
				QualifiedSlot rightSlot = cqlBlock2.QualifySlotWithBlockAlias(keySlot);
				MemberPath outputMember = array[keySlot].OutputMember;
				onClause.Add(leftSlot, outputMember, rightSlot, outputMember);
			}
			list3.Add(onClause);
		}
		return new JoinCqlBlock(OpType, array, list, list3, identifiers, ++blockAliasNum);
	}

	private SlotInfo GetJoinSlotInfo(CellTreeOpType opType, bool isRequiredSlot, List<CqlBlock> children, int slotNum, CqlIdentifiers identifiers)
	{
		if (!isRequiredSlot)
		{
			return new SlotInfo(isRequiredByParent: false, isProjected: false, null, GetMemberPath(slotNum));
		}
		int num = -1;
		CaseStatement caseStatement = null;
		for (int i = 0; i < children.Count; i++)
		{
			CqlBlock cqlBlock = children[i];
			if (!cqlBlock.IsProjected(slotNum))
			{
				continue;
			}
			if (IsKeySlot(slotNum))
			{
				num = i;
				break;
			}
			if (opType == CellTreeOpType.IJ)
			{
				num = GetInnerJoinChildForSlot(children, slotNum);
				break;
			}
			if (num != -1)
			{
				if (caseStatement == null)
				{
					caseStatement = new CaseStatement(GetMemberPath(slotNum));
					AddCaseForOuterJoins(caseStatement, children[num], slotNum, identifiers);
				}
				AddCaseForOuterJoins(caseStatement, cqlBlock, slotNum, identifiers);
			}
			num = i;
		}
		MemberPath memberPath = GetMemberPath(slotNum);
		ProjectedSlot projectedSlot = null;
		if (caseStatement == null || (caseStatement.Clauses.Count <= 0 && caseStatement.ElseValue == null))
		{
			projectedSlot = ((num >= 0) ? children[num].QualifySlotWithBlockAlias(slotNum) : ((!IsBoolSlot(slotNum)) ? ((ProjectedSlot)new ConstantProjectedSlot(Domain.GetDefaultValueForMemberPath(memberPath, GetLeaves(), base.ViewgenContext.Config))) : ((ProjectedSlot)new BooleanProjectedSlot(BoolExpression.False, identifiers, SlotToBoolIndex(slotNum)))));
		}
		else
		{
			caseStatement.Simplify();
			projectedSlot = new CaseStatementProjectedSlot(caseStatement, null);
		}
		bool enforceNotNull = IsBoolSlot(slotNum) && ((opType == CellTreeOpType.LOJ && num > 0) || opType == CellTreeOpType.FOJ);
		return new SlotInfo(isRequiredByParent: true, isProjected: true, projectedSlot, memberPath, enforceNotNull);
	}

	private static int GetInnerJoinChildForSlot(List<CqlBlock> children, int slotNum)
	{
		int num = -1;
		for (int i = 0; i < children.Count; i++)
		{
			CqlBlock cqlBlock = children[i];
			if (!cqlBlock.IsProjected(slotNum))
			{
				continue;
			}
			ProjectedSlot projectedSlot = cqlBlock.SlotValue(slotNum);
			ConstantProjectedSlot constantProjectedSlot = projectedSlot as ConstantProjectedSlot;
			if (projectedSlot is MemberProjectedSlot)
			{
				num = i;
			}
			else if (constantProjectedSlot != null && constantProjectedSlot.CellConstant.IsNull())
			{
				if (num == -1)
				{
					num = i;
				}
			}
			else
			{
				num = i;
			}
		}
		return num;
	}

	private void AddCaseForOuterJoins(CaseStatement caseForOuterJoins, CqlBlock child, int slotNum, CqlIdentifiers identifiers)
	{
		if (child.SlotValue(slotNum) is ConstantProjectedSlot constantProjectedSlot && constantProjectedSlot.CellConstant.IsNull())
		{
			return;
		}
		BoolExpression boolExpression = BoolExpression.False;
		for (int i = 0; i < NumBoolSlots; i++)
		{
			int slotNum2 = BoolIndexToSlot(i);
			if (child.IsProjected(slotNum2))
			{
				QualifiedCellIdBoolean literal = new QualifiedCellIdBoolean(child, identifiers, i);
				boolExpression = BoolExpression.CreateOr(boolExpression, BoolExpression.CreateLiteral(literal, RightDomainMap));
			}
		}
		QualifiedSlot value = child.QualifySlotWithBlockAlias(slotNum);
		caseForOuterJoins.AddWhenThen(boolExpression, value);
	}

	private static FragmentQuery GenerateFragmentQuery(IEnumerable<CellTreeNode> children, bool isLeft, ViewgenContext context, CellTreeOpType OpType)
	{
		FragmentQuery fragmentQuery = (isLeft ? children.First().LeftFragmentQuery : children.First().RightFragmentQuery);
		FragmentQueryProcessor fragmentQueryProcessor = (isLeft ? context.LeftFragmentQP : context.RightFragmentQP);
		foreach (CellTreeNode item in children.Skip(1))
		{
			FragmentQuery arg = (isLeft ? item.LeftFragmentQuery : item.RightFragmentQuery);
			switch (OpType)
			{
			case CellTreeOpType.IJ:
				fragmentQuery = fragmentQueryProcessor.Intersect(fragmentQuery, arg);
				break;
			case CellTreeOpType.LASJ:
				fragmentQuery = fragmentQueryProcessor.Difference(fragmentQuery, arg);
				break;
			default:
				fragmentQuery = fragmentQueryProcessor.Union(fragmentQuery, arg);
				break;
			case CellTreeOpType.LOJ:
				break;
			}
		}
		return fragmentQuery;
	}

	internal static string OpToEsql(CellTreeOpType opType)
	{
		return opType switch
		{
			CellTreeOpType.FOJ => "FULL OUTER JOIN", 
			CellTreeOpType.IJ => "INNER JOIN", 
			CellTreeOpType.LOJ => "LEFT OUTER JOIN", 
			CellTreeOpType.Union => "UNION ALL", 
			_ => null, 
		};
	}

	internal override void ToCompactString(StringBuilder stringBuilder)
	{
		stringBuilder.Append("(");
		for (int i = 0; i < m_children.Count; i++)
		{
			m_children[i].ToCompactString(stringBuilder);
			if (i != m_children.Count - 1)
			{
				StringUtil.FormatStringBuilder(stringBuilder, " {0} ", OpType);
			}
		}
		stringBuilder.Append(")");
	}
}

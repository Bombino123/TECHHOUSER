using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal abstract class CellTreeNode : InternalBase
{
	internal abstract class CellTreeVisitor<TInput, TOutput>
	{
		internal abstract TOutput VisitLeaf(LeafCellTreeNode node, TInput param);

		internal abstract TOutput VisitUnion(OpCellTreeNode node, TInput param);

		internal abstract TOutput VisitInnerJoin(OpCellTreeNode node, TInput param);

		internal abstract TOutput VisitLeftOuterJoin(OpCellTreeNode node, TInput param);

		internal abstract TOutput VisitFullOuterJoin(OpCellTreeNode node, TInput param);

		internal abstract TOutput VisitLeftAntiSemiJoin(OpCellTreeNode node, TInput param);
	}

	internal abstract class SimpleCellTreeVisitor<TInput, TOutput>
	{
		internal abstract TOutput VisitLeaf(LeafCellTreeNode node, TInput param);

		internal abstract TOutput VisitOpNode(OpCellTreeNode node, TInput param);
	}

	private class DefaultCellTreeVisitor<TInput> : CellTreeVisitor<TInput, CellTreeNode>
	{
		internal override CellTreeNode VisitLeaf(LeafCellTreeNode node, TInput param)
		{
			return node;
		}

		internal override CellTreeNode VisitUnion(OpCellTreeNode node, TInput param)
		{
			return AcceptChildren(node, param);
		}

		internal override CellTreeNode VisitInnerJoin(OpCellTreeNode node, TInput param)
		{
			return AcceptChildren(node, param);
		}

		internal override CellTreeNode VisitLeftOuterJoin(OpCellTreeNode node, TInput param)
		{
			return AcceptChildren(node, param);
		}

		internal override CellTreeNode VisitFullOuterJoin(OpCellTreeNode node, TInput param)
		{
			return AcceptChildren(node, param);
		}

		internal override CellTreeNode VisitLeftAntiSemiJoin(OpCellTreeNode node, TInput param)
		{
			return AcceptChildren(node, param);
		}

		private OpCellTreeNode AcceptChildren(OpCellTreeNode node, TInput param)
		{
			List<CellTreeNode> list = new List<CellTreeNode>();
			foreach (CellTreeNode child in node.Children)
			{
				list.Add(child.Accept(this, param));
			}
			return new OpCellTreeNode(node.ViewgenContext, node.OpType, list);
		}
	}

	private class FlatteningVisitor : SimpleCellTreeVisitor<bool, CellTreeNode>
	{
		protected FlatteningVisitor()
		{
		}

		internal static CellTreeNode Flatten(CellTreeNode node)
		{
			FlatteningVisitor visitor = new FlatteningVisitor();
			return node.Accept(visitor, param: true);
		}

		internal override CellTreeNode VisitLeaf(LeafCellTreeNode node, bool dummy)
		{
			return node;
		}

		internal override CellTreeNode VisitOpNode(OpCellTreeNode node, bool dummy)
		{
			List<CellTreeNode> list = new List<CellTreeNode>();
			foreach (CellTreeNode child in node.Children)
			{
				CellTreeNode item = child.Accept(this, dummy);
				list.Add(item);
			}
			if (list.Count == 1)
			{
				return list[0];
			}
			return new OpCellTreeNode(node.ViewgenContext, node.OpType, list);
		}
	}

	private class AssociativeOpFlatteningVisitor : SimpleCellTreeVisitor<bool, CellTreeNode>
	{
		private AssociativeOpFlatteningVisitor()
		{
		}

		internal static CellTreeNode Flatten(CellTreeNode node)
		{
			CellTreeNode cellTreeNode = FlatteningVisitor.Flatten(node);
			AssociativeOpFlatteningVisitor visitor = new AssociativeOpFlatteningVisitor();
			return cellTreeNode.Accept(visitor, param: true);
		}

		internal override CellTreeNode VisitLeaf(LeafCellTreeNode node, bool dummy)
		{
			return node;
		}

		internal override CellTreeNode VisitOpNode(OpCellTreeNode node, bool dummy)
		{
			List<CellTreeNode> list = new List<CellTreeNode>();
			foreach (CellTreeNode child in node.Children)
			{
				CellTreeNode item = child.Accept(this, dummy);
				list.Add(item);
			}
			List<CellTreeNode> list2 = list;
			if (IsAssociativeOp(node.OpType))
			{
				list2 = new List<CellTreeNode>();
				foreach (CellTreeNode item2 in list)
				{
					if (item2.OpType == node.OpType)
					{
						list2.AddRange(item2.Children);
					}
					else
					{
						list2.Add(item2);
					}
				}
			}
			return new OpCellTreeNode(node.ViewgenContext, node.OpType, list2);
		}
	}

	private class LeafVisitor : SimpleCellTreeVisitor<bool, IEnumerable<LeafCellTreeNode>>
	{
		private LeafVisitor()
		{
		}

		internal static IEnumerable<LeafCellTreeNode> GetLeaves(CellTreeNode node)
		{
			LeafVisitor visitor = new LeafVisitor();
			return node.Accept(visitor, param: true);
		}

		internal override IEnumerable<LeafCellTreeNode> VisitLeaf(LeafCellTreeNode node, bool dummy)
		{
			yield return node;
		}

		internal override IEnumerable<LeafCellTreeNode> VisitOpNode(OpCellTreeNode node, bool dummy)
		{
			foreach (CellTreeNode child in node.Children)
			{
				IEnumerable<LeafCellTreeNode> enumerable = child.Accept(this, dummy);
				foreach (LeafCellTreeNode item in enumerable)
				{
					yield return item;
				}
			}
		}
	}

	private readonly ViewgenContext m_viewgenContext;

	internal abstract CellTreeOpType OpType { get; }

	internal abstract MemberDomainMap RightDomainMap { get; }

	internal abstract FragmentQuery LeftFragmentQuery { get; }

	internal abstract FragmentQuery RightFragmentQuery { get; }

	internal bool IsEmptyRightFragmentQuery => !m_viewgenContext.RightFragmentQP.IsSatisfiable(RightFragmentQuery);

	internal abstract Set<MemberPath> Attributes { get; }

	internal abstract List<CellTreeNode> Children { get; }

	internal abstract int NumProjectedSlots { get; }

	internal abstract int NumBoolSlots { get; }

	internal MemberProjectionIndex ProjectedSlotMap => m_viewgenContext.MemberMaps.ProjectedSlotMap;

	internal ViewgenContext ViewgenContext => m_viewgenContext;

	protected IEnumerable<int> KeySlots
	{
		get
		{
			int numMembers = ProjectedSlotMap.Count;
			for (int slotNum = 0; slotNum < numMembers; slotNum++)
			{
				if (IsKeySlot(slotNum))
				{
					yield return slotNum;
				}
			}
		}
	}

	protected CellTreeNode(ViewgenContext context)
	{
		m_viewgenContext = context;
	}

	internal CellTreeNode MakeCopy()
	{
		DefaultCellTreeVisitor<bool> visitor = new DefaultCellTreeVisitor<bool>();
		return Accept(visitor, param: true);
	}

	internal abstract CqlBlock ToCqlBlock(bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum, ref List<WithRelationship> withRelationships);

	internal abstract bool IsProjectedSlot(int slot);

	internal abstract TOutput Accept<TInput, TOutput>(CellTreeVisitor<TInput, TOutput> visitor, TInput param);

	internal abstract TOutput Accept<TInput, TOutput>(SimpleCellTreeVisitor<TInput, TOutput> visitor, TInput param);

	internal CellTreeNode Flatten()
	{
		return FlatteningVisitor.Flatten(this);
	}

	internal List<LeftCellWrapper> GetLeaves()
	{
		return (from leafNode in GetLeafNodes()
			select leafNode.LeftCellWrapper).ToList();
	}

	internal IEnumerable<LeafCellTreeNode> GetLeafNodes()
	{
		return LeafVisitor.GetLeaves(this);
	}

	internal CellTreeNode AssociativeFlatten()
	{
		return AssociativeOpFlatteningVisitor.Flatten(this);
	}

	internal static bool IsAssociativeOp(CellTreeOpType opType)
	{
		if (opType != CellTreeOpType.IJ && opType != CellTreeOpType.Union)
		{
			return opType == CellTreeOpType.FOJ;
		}
		return true;
	}

	internal bool[] GetProjectedSlots()
	{
		int num = ProjectedSlotMap.Count + NumBoolSlots;
		bool[] array = new bool[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = IsProjectedSlot(i);
		}
		return array;
	}

	protected MemberPath GetMemberPath(int slotNum)
	{
		return ProjectedSlotMap.GetMemberPath(slotNum, NumBoolSlots);
	}

	protected int BoolIndexToSlot(int boolIndex)
	{
		return ProjectedSlotMap.BoolIndexToSlot(boolIndex, NumBoolSlots);
	}

	protected int SlotToBoolIndex(int slotNum)
	{
		return ProjectedSlotMap.SlotToBoolIndex(slotNum, NumBoolSlots);
	}

	protected bool IsKeySlot(int slotNum)
	{
		return ProjectedSlotMap.IsKeySlot(slotNum, NumBoolSlots);
	}

	protected bool IsBoolSlot(int slotNum)
	{
		return ProjectedSlotMap.IsBoolSlot(slotNum, NumBoolSlots);
	}

	internal override void ToFullString(StringBuilder builder)
	{
		int blockAliasNum = 0;
		bool[] projectedSlots = GetProjectedSlots();
		CqlIdentifiers identifiers = new CqlIdentifiers();
		List<WithRelationship> withRelationships = new List<WithRelationship>();
		ToCqlBlock(projectedSlots, identifiers, ref blockAliasNum, ref withRelationships).AsEsql(builder, isTopLevel: false, 1);
	}
}

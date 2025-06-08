using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal class CellTreeSimplifier : InternalBase
{
	private readonly ViewgenContext m_viewgenContext;

	private CellTreeSimplifier(ViewgenContext context)
	{
		m_viewgenContext = context;
	}

	internal static CellTreeNode MergeNodes(CellTreeNode rootNode)
	{
		return new CellTreeSimplifier(rootNode.ViewgenContext).SimplifyTreeByMergingNodes(rootNode);
	}

	private CellTreeNode SimplifyTreeByMergingNodes(CellTreeNode rootNode)
	{
		if (rootNode is LeafCellTreeNode)
		{
			return rootNode;
		}
		rootNode = RestructureTreeForMerges(rootNode);
		List<CellTreeNode> children = rootNode.Children;
		for (int i = 0; i < children.Count; i++)
		{
			children[i] = SimplifyTreeByMergingNodes(children[i]);
		}
		bool flag = CellTreeNode.IsAssociativeOp(rootNode.OpType);
		children = ((!flag) ? GroupNonAssociativeLeafChildren(children) : GroupLeafChildrenByExtent(children));
		OpCellTreeNode opCellTreeNode = new OpCellTreeNode(m_viewgenContext, rootNode.OpType);
		CellTreeNode node = null;
		bool flag2 = false;
		foreach (CellTreeNode item in children)
		{
			if (node == null)
			{
				node = item;
				continue;
			}
			bool flag3 = false;
			if (!flag2 && node.OpType == CellTreeOpType.Leaf && item.OpType == CellTreeOpType.Leaf)
			{
				flag3 = TryMergeCellQueries(rootNode.OpType, ref node, item);
			}
			if (!flag3)
			{
				opCellTreeNode.Add(node);
				node = item;
				if (!flag)
				{
					flag2 = true;
				}
			}
		}
		opCellTreeNode.Add(node);
		return opCellTreeNode.AssociativeFlatten();
	}

	private CellTreeNode RestructureTreeForMerges(CellTreeNode rootNode)
	{
		List<CellTreeNode> children = rootNode.Children;
		if (!CellTreeNode.IsAssociativeOp(rootNode.OpType) || children.Count <= 1)
		{
			return rootNode;
		}
		Set<LeafCellTreeNode> commonGrandChildren = GetCommonGrandChildren(children);
		if (commonGrandChildren == null)
		{
			return rootNode;
		}
		CellTreeOpType opType = children[0].OpType;
		List<OpCellTreeNode> list = new List<OpCellTreeNode>(children.Count);
		foreach (OpCellTreeNode item2 in children)
		{
			List<LeafCellTreeNode> list2 = new List<LeafCellTreeNode>(item2.Children.Count);
			foreach (LeafCellTreeNode child in item2.Children)
			{
				if (!commonGrandChildren.Contains(child))
				{
					list2.Add(child);
				}
			}
			OpCellTreeNode item = new OpCellTreeNode(m_viewgenContext, item2.OpType, Helpers.AsSuperTypeList<LeafCellTreeNode, CellTreeNode>(list2));
			list.Add(item);
		}
		CellTreeNode cellTreeNode = new OpCellTreeNode(m_viewgenContext, rootNode.OpType, Helpers.AsSuperTypeList<OpCellTreeNode, CellTreeNode>(list));
		CellTreeNode cellTreeNode2 = new OpCellTreeNode(m_viewgenContext, opType, Helpers.AsSuperTypeList<LeafCellTreeNode, CellTreeNode>(commonGrandChildren));
		return new OpCellTreeNode(m_viewgenContext, opType, cellTreeNode2, cellTreeNode).AssociativeFlatten();
	}

	private static Set<LeafCellTreeNode> GetCommonGrandChildren(List<CellTreeNode> nodes)
	{
		Set<LeafCellTreeNode> set = null;
		CellTreeOpType cellTreeOpType = CellTreeOpType.Leaf;
		foreach (CellTreeNode node in nodes)
		{
			if (!(node is OpCellTreeNode opCellTreeNode))
			{
				return null;
			}
			if (cellTreeOpType == CellTreeOpType.Leaf)
			{
				cellTreeOpType = opCellTreeNode.OpType;
			}
			else if (!CellTreeNode.IsAssociativeOp(opCellTreeNode.OpType) || cellTreeOpType != opCellTreeNode.OpType)
			{
				return null;
			}
			Set<LeafCellTreeNode> set2 = new Set<LeafCellTreeNode>(LeafCellTreeNode.EqualityComparer);
			foreach (CellTreeNode child in opCellTreeNode.Children)
			{
				if (!(child is LeafCellTreeNode element))
				{
					return null;
				}
				set2.Add(element);
			}
			if (set == null)
			{
				set = set2;
			}
			else
			{
				set.Intersect(set2);
			}
		}
		if (set.Count == 0)
		{
			return null;
		}
		return set;
	}

	private static List<CellTreeNode> GroupLeafChildrenByExtent(List<CellTreeNode> nodes)
	{
		KeyToListMap<EntitySetBase, CellTreeNode> keyToListMap = new KeyToListMap<EntitySetBase, CellTreeNode>(EqualityComparer<EntitySetBase>.Default);
		List<CellTreeNode> list = new List<CellTreeNode>();
		foreach (CellTreeNode node in nodes)
		{
			if (node is LeafCellTreeNode leafCellTreeNode)
			{
				keyToListMap.Add(leafCellTreeNode.LeftCellWrapper.RightCellQuery.Extent, leafCellTreeNode);
			}
			else
			{
				list.Add(node);
			}
		}
		list.AddRange(keyToListMap.AllValues);
		return list;
	}

	private static List<CellTreeNode> GroupNonAssociativeLeafChildren(List<CellTreeNode> nodes)
	{
		KeyToListMap<EntitySetBase, CellTreeNode> keyToListMap = new KeyToListMap<EntitySetBase, CellTreeNode>(EqualityComparer<EntitySetBase>.Default);
		List<CellTreeNode> list = new List<CellTreeNode>();
		List<CellTreeNode> list2 = new List<CellTreeNode>();
		list.Add(nodes[0]);
		for (int i = 1; i < nodes.Count; i++)
		{
			CellTreeNode cellTreeNode = nodes[i];
			if (cellTreeNode is LeafCellTreeNode leafCellTreeNode)
			{
				keyToListMap.Add(leafCellTreeNode.LeftCellWrapper.RightCellQuery.Extent, leafCellTreeNode);
			}
			else
			{
				list2.Add(cellTreeNode);
			}
		}
		if (nodes[0] is LeafCellTreeNode leafCellTreeNode2)
		{
			EntitySetBase extent = leafCellTreeNode2.LeftCellWrapper.RightCellQuery.Extent;
			if (keyToListMap.ContainsKey(extent))
			{
				list.AddRange(keyToListMap.ListForKey(extent));
				keyToListMap.RemoveKey(extent);
			}
		}
		list.AddRange(keyToListMap.AllValues);
		list.AddRange(list2);
		return list;
	}

	private bool TryMergeCellQueries(CellTreeOpType opType, ref CellTreeNode node1, CellTreeNode node2)
	{
		LeafCellTreeNode leafCellTreeNode = node1 as LeafCellTreeNode;
		LeafCellTreeNode leafCellTreeNode2 = node2 as LeafCellTreeNode;
		if (!TryMergeTwoCellQueries(leafCellTreeNode.LeftCellWrapper.RightCellQuery, leafCellTreeNode2.LeftCellWrapper.RightCellQuery, opType, out var mergedQuery))
		{
			return false;
		}
		if (!TryMergeTwoCellQueries(leafCellTreeNode.LeftCellWrapper.LeftCellQuery, leafCellTreeNode2.LeftCellWrapper.LeftCellQuery, opType, out var mergedQuery2))
		{
			return false;
		}
		OpCellTreeNode opCellTreeNode = new OpCellTreeNode(m_viewgenContext, opType);
		opCellTreeNode.Add(node1);
		opCellTreeNode.Add(node2);
		LeftCellWrapper cellWrapper = new LeftCellWrapper(m_viewgenContext.ViewTarget, opCellTreeNode.Attributes, opCellTreeNode.LeftFragmentQuery, mergedQuery2, mergedQuery, m_viewgenContext.MemberMaps, leafCellTreeNode.LeftCellWrapper.Cells.Concat(leafCellTreeNode2.LeftCellWrapper.Cells));
		node1 = new LeafCellTreeNode(m_viewgenContext, cellWrapper, opCellTreeNode.RightFragmentQuery);
		return true;
	}

	internal static bool TryMergeTwoCellQueries(CellQuery query1, CellQuery query2, CellTreeOpType opType, out CellQuery mergedQuery)
	{
		mergedQuery = null;
		BoolExpression boolExpression = null;
		BoolExpression boolExpression2 = null;
		switch (opType)
		{
		case CellTreeOpType.LOJ:
		case CellTreeOpType.LASJ:
			boolExpression2 = BoolExpression.True;
			break;
		case CellTreeOpType.Union:
		case CellTreeOpType.FOJ:
			boolExpression = BoolExpression.True;
			boolExpression2 = BoolExpression.True;
			break;
		}
		Dictionary<MemberPath, MemberPath> remap = new Dictionary<MemberPath, MemberPath>(MemberPath.EqualityComparer);
		if (!query1.Extent.Equals(query2.Extent))
		{
			return false;
		}
		MemberPath sourceExtentMemberPath = query1.SourceExtentMemberPath;
		BoolExpression conjunct = BoolExpression.True;
		BoolExpression boolExpression3 = BoolExpression.True;
		BoolExpression boolExpression4 = null;
		switch (opType)
		{
		case CellTreeOpType.IJ:
			boolExpression4 = BoolExpression.CreateAnd(query1.WhereClause, query2.WhereClause);
			break;
		case CellTreeOpType.LOJ:
			boolExpression3 = BoolExpression.CreateAnd(query2.WhereClause, boolExpression2);
			boolExpression4 = query1.WhereClause;
			break;
		case CellTreeOpType.Union:
		case CellTreeOpType.FOJ:
			conjunct = BoolExpression.CreateAnd(query1.WhereClause, boolExpression);
			boolExpression3 = BoolExpression.CreateAnd(query2.WhereClause, boolExpression2);
			boolExpression4 = BoolExpression.CreateOr(BoolExpression.CreateAnd(query1.WhereClause, boolExpression), BoolExpression.CreateAnd(query2.WhereClause, boolExpression2));
			break;
		case CellTreeOpType.LASJ:
			boolExpression3 = BoolExpression.CreateAnd(query2.WhereClause, boolExpression2);
			boolExpression4 = BoolExpression.CreateAnd(query1.WhereClause, BoolExpression.CreateNot(boolExpression3));
			break;
		}
		List<BoolExpression> boolExprs = MergeBoolExpressions(query1, query2, conjunct, boolExpression3, opType);
		if (!ProjectedSlot.TryMergeRemapSlots(query1.ProjectedSlots, query2.ProjectedSlots, out var result))
		{
			return false;
		}
		boolExpression4 = boolExpression4.RemapBool(remap);
		CellQuery.SelectDistinct elimDupl = MergeDupl(query1.SelectDistinctFlag, query2.SelectDistinctFlag);
		boolExpression4.ExpensiveSimplify();
		mergedQuery = new CellQuery(result, boolExpression4, boolExprs, elimDupl, sourceExtentMemberPath);
		return true;
	}

	private static CellQuery.SelectDistinct MergeDupl(CellQuery.SelectDistinct d1, CellQuery.SelectDistinct d2)
	{
		if (d1 == CellQuery.SelectDistinct.Yes || d2 == CellQuery.SelectDistinct.Yes)
		{
			return CellQuery.SelectDistinct.Yes;
		}
		return CellQuery.SelectDistinct.No;
	}

	private static List<BoolExpression> MergeBoolExpressions(CellQuery query1, CellQuery query2, BoolExpression conjunct1, BoolExpression conjunct2, CellTreeOpType opType)
	{
		List<BoolExpression> list = query1.BoolVars;
		List<BoolExpression> list2 = query2.BoolVars;
		if (!conjunct1.IsTrue)
		{
			list = BoolExpression.AddConjunctionToBools(list, conjunct1);
		}
		if (!conjunct2.IsTrue)
		{
			list2 = BoolExpression.AddConjunctionToBools(list2, conjunct2);
		}
		List<BoolExpression> list3 = new List<BoolExpression>();
		for (int i = 0; i < list.Count; i++)
		{
			BoolExpression boolExpression = null;
			if (list[i] == null)
			{
				boolExpression = list2[i];
			}
			else if (list2[i] == null)
			{
				boolExpression = list[i];
			}
			else
			{
				switch (opType)
				{
				case CellTreeOpType.IJ:
					boolExpression = BoolExpression.CreateAnd(list[i], list2[i]);
					break;
				case CellTreeOpType.Union:
					boolExpression = BoolExpression.CreateOr(list[i], list2[i]);
					break;
				case CellTreeOpType.LASJ:
					boolExpression = BoolExpression.CreateAnd(list[i], BoolExpression.CreateNot(list2[i]));
					break;
				}
			}
			boolExpression?.ExpensiveSimplify();
			list3.Add(boolExpression);
		}
		return list3;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		m_viewgenContext.MemberMaps.ProjectedSlotMap.ToCompactString(builder);
	}
}

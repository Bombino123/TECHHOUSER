using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Linq;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class JoinGraph
{
	private readonly Command m_command;

	private readonly AugmentedJoinNode m_root;

	private readonly List<AugmentedNode> m_vertexes;

	private readonly Dictionary<Table, AugmentedTableNode> m_tableVertexMap;

	private VarMap m_varMap;

	private readonly Dictionary<Var, VarVec> m_reverseVarMap;

	private readonly Dictionary<Var, AugmentedTableNode> m_varToDefiningNodeMap;

	private readonly Dictionary<Node, Node> m_processedNodes;

	private bool m_modifiedGraph;

	private readonly ConstraintManager m_constraintManager;

	private readonly VarRefManager m_varRefManager;

	internal JoinGraph(Command command, ConstraintManager constraintManager, VarRefManager varRefManager, Node joinNode)
	{
		m_command = command;
		m_constraintManager = constraintManager;
		m_varRefManager = varRefManager;
		m_vertexes = new List<AugmentedNode>();
		m_tableVertexMap = new Dictionary<Table, AugmentedTableNode>();
		m_varMap = new VarMap();
		m_reverseVarMap = new Dictionary<Var, VarVec>();
		m_varToDefiningNodeMap = new Dictionary<Var, AugmentedTableNode>();
		m_processedNodes = new Dictionary<Node, Node>();
		m_root = BuildAugmentedNodeTree(joinNode) as AugmentedJoinNode;
		PlanCompiler.Assert(m_root != null, "The root isn't a join?");
		BuildJoinEdges(m_root, m_root.Id);
	}

	internal Node DoJoinElimination(out VarMap varMap, out Dictionary<Node, Node> processedNodes)
	{
		TryTurnLeftOuterJoinsIntoInnerJoins();
		GenerateTransitiveEdges();
		EliminateSelfJoins();
		EliminateParentChildJoins();
		Node result = BuildNodeTree();
		varMap = m_varMap;
		processedNodes = m_processedNodes;
		return result;
	}

	private VarVec GetColumnVars(VarVec varVec)
	{
		VarVec varVec2 = m_command.CreateVarVec();
		foreach (Var item in varVec)
		{
			if (item.VarType == VarType.Column)
			{
				varVec2.Set(item);
			}
		}
		return varVec2;
	}

	private static void GetColumnVars(List<ColumnVar> columnVars, IEnumerable<Var> vec)
	{
		foreach (Var item in vec)
		{
			PlanCompiler.Assert(item.VarType == VarType.Column, "Expected a columnVar. Found " + item.VarType);
			columnVars.Add((ColumnVar)item);
		}
	}

	private void SplitPredicate(Node joinNode, out List<ColumnVar> leftVars, out List<ColumnVar> rightVars, out Node otherPredicateNode)
	{
		leftVars = new List<ColumnVar>();
		rightVars = new List<ColumnVar>();
		otherPredicateNode = joinNode.Child2;
		if (joinNode.Op.OpType != OpType.FullOuterJoin)
		{
			Predicate predicate = new Predicate(m_command, joinNode.Child2);
			ExtendedNodeInfo extendedNodeInfo = m_command.GetExtendedNodeInfo(joinNode.Child0);
			ExtendedNodeInfo extendedNodeInfo2 = m_command.GetExtendedNodeInfo(joinNode.Child1);
			VarVec columnVars = GetColumnVars(extendedNodeInfo.Definitions);
			VarVec columnVars2 = GetColumnVars(extendedNodeInfo2.Definitions);
			predicate.GetEquiJoinPredicates(columnVars, columnVars2, out var leftTableEquiJoinColumns, out var rightTableEquiJoinColumns, out var otherPredicates);
			otherPredicateNode = otherPredicates.BuildAndTree();
			GetColumnVars(leftVars, leftTableEquiJoinColumns);
			GetColumnVars(rightVars, rightTableEquiJoinColumns);
		}
	}

	private AugmentedNode BuildAugmentedNodeTree(Node node)
	{
		AugmentedNode augmentedNode;
		switch (node.Op.OpType)
		{
		case OpType.ScanTable:
		{
			m_processedNodes[node] = node;
			ScanTableOp scanTableOp = (ScanTableOp)node.Op;
			augmentedNode = new AugmentedTableNode(m_vertexes.Count, node);
			m_tableVertexMap[scanTableOp.Table] = (AugmentedTableNode)augmentedNode;
			break;
		}
		case OpType.InnerJoin:
		case OpType.LeftOuterJoin:
		case OpType.FullOuterJoin:
		{
			m_processedNodes[node] = node;
			AugmentedNode leftChild = BuildAugmentedNodeTree(node.Child0);
			AugmentedNode rightChild = BuildAugmentedNodeTree(node.Child1);
			SplitPredicate(node, out var leftVars, out var rightVars, out var otherPredicateNode);
			m_varRefManager.AddChildren(node);
			augmentedNode = new AugmentedJoinNode(m_vertexes.Count, node, leftChild, rightChild, leftVars, rightVars, otherPredicateNode);
			break;
		}
		case OpType.CrossJoin:
		{
			m_processedNodes[node] = node;
			List<AugmentedNode> list = new List<AugmentedNode>();
			foreach (Node child in node.Children)
			{
				list.Add(BuildAugmentedNodeTree(child));
			}
			augmentedNode = new AugmentedJoinNode(m_vertexes.Count, node, list);
			m_varRefManager.AddChildren(node);
			break;
		}
		default:
			augmentedNode = new AugmentedNode(m_vertexes.Count, node);
			break;
		}
		m_vertexes.Add(augmentedNode);
		return augmentedNode;
	}

	private bool AddJoinEdge(AugmentedJoinNode joinNode, ColumnVar leftVar, ColumnVar rightVar)
	{
		if (!m_tableVertexMap.TryGetValue(leftVar.Table, out var value))
		{
			return false;
		}
		if (!m_tableVertexMap.TryGetValue(rightVar.Table, out var value2))
		{
			return false;
		}
		foreach (JoinEdge joinEdge in value.JoinEdges)
		{
			if (joinEdge.Right.Table.Equals(rightVar.Table))
			{
				return joinEdge.AddCondition(joinNode, leftVar, rightVar);
			}
		}
		JoinEdge item = JoinEdge.CreateJoinEdge(value, value2, joinNode, leftVar, rightVar);
		value.JoinEdges.Add(item);
		joinNode.JoinEdges.Add(item);
		return true;
	}

	private static bool SingleTableVars(IEnumerable<ColumnVar> varList)
	{
		Table table = null;
		foreach (ColumnVar var in varList)
		{
			if (table == null)
			{
				table = var.Table;
			}
			else if (var.Table != table)
			{
				return false;
			}
		}
		return true;
	}

	private void BuildJoinEdges(AugmentedJoinNode joinNode, int maxVisibility)
	{
		OpType opType = joinNode.Node.Op.OpType;
		int maxVisibility2;
		int maxVisibility3;
		switch (opType)
		{
		case OpType.CrossJoin:
		{
			foreach (AugmentedNode child in joinNode.Children)
			{
				BuildJoinEdges(child, maxVisibility);
			}
			return;
		}
		case OpType.FullOuterJoin:
			maxVisibility2 = joinNode.Id;
			maxVisibility3 = joinNode.Id;
			break;
		case OpType.LeftOuterJoin:
			maxVisibility2 = maxVisibility;
			maxVisibility3 = joinNode.Id;
			break;
		default:
			maxVisibility2 = maxVisibility;
			maxVisibility3 = maxVisibility;
			break;
		}
		BuildJoinEdges(joinNode.Children[0], maxVisibility2);
		BuildJoinEdges(joinNode.Children[1], maxVisibility3);
		if (joinNode.Node.Op.OpType == OpType.FullOuterJoin || joinNode.LeftVars.Count == 0 || (opType == OpType.LeftOuterJoin && (!SingleTableVars(joinNode.RightVars) || !SingleTableVars(joinNode.LeftVars))))
		{
			return;
		}
		JoinKind joinKind = ((opType == OpType.LeftOuterJoin) ? JoinKind.LeftOuter : JoinKind.Inner);
		for (int i = 0; i < joinNode.LeftVars.Count; i++)
		{
			if (AddJoinEdge(joinNode, joinNode.LeftVars[i], joinNode.RightVars[i]) && joinKind == JoinKind.Inner)
			{
				AddJoinEdge(joinNode, joinNode.RightVars[i], joinNode.LeftVars[i]);
			}
		}
	}

	private void BuildJoinEdges(AugmentedNode node, int maxVisibility)
	{
		switch (node.Node.Op.OpType)
		{
		case OpType.InnerJoin:
		case OpType.LeftOuterJoin:
		case OpType.FullOuterJoin:
		case OpType.CrossJoin:
			BuildJoinEdges(node as AugmentedJoinNode, maxVisibility);
			break;
		case OpType.ScanTable:
			((AugmentedTableNode)node).LastVisibleId = maxVisibility;
			break;
		}
	}

	private static bool GenerateTransitiveEdge(JoinEdge edge1, JoinEdge edge2)
	{
		PlanCompiler.Assert(edge1.Right == edge2.Left, "need a common table for transitive predicate generation");
		if (edge1.RestrictedElimination || edge2.RestrictedElimination)
		{
			return false;
		}
		if (edge2.Right == edge1.Left)
		{
			return false;
		}
		if (edge1.JoinKind != edge2.JoinKind)
		{
			return false;
		}
		if (edge1.JoinKind == JoinKind.LeftOuter && (edge1.Left != edge1.Right || edge2.Left != edge2.Right))
		{
			return false;
		}
		if (edge1.JoinKind == JoinKind.LeftOuter && edge1.RightVars.Count != edge2.LeftVars.Count)
		{
			return false;
		}
		foreach (JoinEdge joinEdge in edge1.Left.JoinEdges)
		{
			if (joinEdge.Right == edge2.Right)
			{
				return false;
			}
		}
		IEnumerable<KeyValuePair<ColumnVar, ColumnVar>> enumerable = CreateOrderedKeyValueList(edge1.RightVars, edge1.LeftVars);
		IEnumerable<KeyValuePair<ColumnVar, ColumnVar>> enumerable2 = CreateOrderedKeyValueList(edge2.LeftVars, edge2.RightVars);
		IEnumerator<KeyValuePair<ColumnVar, ColumnVar>> enumerator2 = enumerable.GetEnumerator();
		IEnumerator<KeyValuePair<ColumnVar, ColumnVar>> enumerator3 = enumerable2.GetEnumerator();
		List<ColumnVar> list = new List<ColumnVar>();
		List<ColumnVar> list2 = new List<ColumnVar>();
		bool flag = enumerator2.MoveNext() && enumerator3.MoveNext();
		while (flag)
		{
			if (enumerator2.Current.Key == enumerator3.Current.Key)
			{
				list.Add(enumerator2.Current.Value);
				list2.Add(enumerator3.Current.Value);
				flag = enumerator2.MoveNext() && enumerator3.MoveNext();
				continue;
			}
			if (edge1.JoinKind == JoinKind.LeftOuter)
			{
				return false;
			}
			flag = ((enumerator2.Current.Key.Id <= enumerator3.Current.Key.Id) ? enumerator2.MoveNext() : enumerator3.MoveNext());
		}
		JoinEdge item = JoinEdge.CreateTransitiveJoinEdge(edge1.Left, edge2.Right, edge1.JoinKind, list, list2);
		edge1.Left.JoinEdges.Add(item);
		if (edge1.JoinKind == JoinKind.Inner)
		{
			JoinEdge item2 = JoinEdge.CreateTransitiveJoinEdge(edge2.Right, edge1.Left, edge1.JoinKind, list2, list);
			edge2.Right.JoinEdges.Add(item2);
		}
		return true;
	}

	private static IEnumerable<KeyValuePair<ColumnVar, ColumnVar>> CreateOrderedKeyValueList(List<ColumnVar> keyVars, List<ColumnVar> valueVars)
	{
		List<KeyValuePair<ColumnVar, ColumnVar>> list = new List<KeyValuePair<ColumnVar, ColumnVar>>(keyVars.Count);
		for (int i = 0; i < keyVars.Count; i++)
		{
			list.Add(new KeyValuePair<ColumnVar, ColumnVar>(keyVars[i], valueVars[i]));
		}
		return list.OrderBy((KeyValuePair<ColumnVar, ColumnVar> kv) => kv.Key.Id);
	}

	private void TryTurnLeftOuterJoinsIntoInnerJoins()
	{
		foreach (AugmentedJoinNode item in from j in m_vertexes.OfType<AugmentedJoinNode>()
			where j.Node.Op.OpType == OpType.LeftOuterJoin && j.JoinEdges.Count > 0
			select j)
		{
			if (!CanAllJoinEdgesBeTurnedIntoInnerJoins(item.Children[1], item.JoinEdges))
			{
				continue;
			}
			item.Node.Op = m_command.CreateInnerJoinOp();
			m_modifiedGraph = true;
			List<JoinEdge> list = new List<JoinEdge>(item.JoinEdges.Count);
			foreach (JoinEdge joinEdge2 in item.JoinEdges)
			{
				joinEdge2.JoinKind = JoinKind.Inner;
				if (!ContainsJoinEdgeForTable(joinEdge2.Right.JoinEdges, joinEdge2.Left.Table))
				{
					JoinEdge joinEdge = JoinEdge.CreateJoinEdge(joinEdge2.Right, joinEdge2.Left, item, joinEdge2.RightVars[0], joinEdge2.LeftVars[0]);
					joinEdge2.Right.JoinEdges.Add(joinEdge);
					list.Add(joinEdge);
					for (int i = 1; i < joinEdge2.LeftVars.Count; i++)
					{
						joinEdge.AddCondition(item, joinEdge2.RightVars[i], joinEdge2.LeftVars[i]);
					}
				}
			}
			item.JoinEdges.AddRange(list);
		}
	}

	private static bool AreAllTableRowsPreserved(AugmentedNode root, AugmentedTableNode table)
	{
		if (root is AugmentedTableNode)
		{
			return true;
		}
		AugmentedNode augmentedNode = table;
		do
		{
			AugmentedJoinNode augmentedJoinNode = (AugmentedJoinNode)augmentedNode.Parent;
			if (augmentedJoinNode.Node.Op.OpType != OpType.LeftOuterJoin || augmentedJoinNode.Children[0] != augmentedNode)
			{
				return false;
			}
			augmentedNode = augmentedJoinNode;
		}
		while (augmentedNode != root);
		return true;
	}

	private static bool ContainsJoinEdgeForTable(IEnumerable<JoinEdge> joinEdges, Table table)
	{
		foreach (JoinEdge joinEdge in joinEdges)
		{
			if (joinEdge.Right.Table.Equals(table))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanAllJoinEdgesBeTurnedIntoInnerJoins(AugmentedNode rightNode, IEnumerable<JoinEdge> joinEdges)
	{
		foreach (JoinEdge joinEdge in joinEdges)
		{
			if (!CanJoinEdgeBeTurnedIntoInnerJoin(rightNode, joinEdge))
			{
				return false;
			}
		}
		return true;
	}

	private bool CanJoinEdgeBeTurnedIntoInnerJoin(AugmentedNode rightNode, JoinEdge joinEdge)
	{
		if (!joinEdge.RestrictedElimination && AreAllTableRowsPreserved(rightNode, joinEdge.Right))
		{
			return IsConstraintPresentForTurningIntoInnerJoin(joinEdge);
		}
		return false;
	}

	private bool IsConstraintPresentForTurningIntoInnerJoin(JoinEdge joinEdge)
	{
		if (m_constraintManager.IsParentChildRelationship(joinEdge.Right.Table.TableMetadata.Extent, joinEdge.Left.Table.TableMetadata.Extent, out var constraints))
		{
			PlanCompiler.Assert(constraints != null && constraints.Count > 0, "Invalid foreign key constraints");
			foreach (ForeignKeyConstraint item in constraints)
			{
				if (IsJoinOnFkConstraint(item, joinEdge.RightVars, joinEdge.LeftVars, out var childForeignKeyVars) && item.ParentKeys.Count == joinEdge.RightVars.Count && childForeignKeyVars.Where((ColumnVar v) => v.ColumnMetadata.IsNullable).Count() == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void GenerateTransitiveEdges()
	{
		foreach (AugmentedNode vertex in m_vertexes)
		{
			if (!(vertex is AugmentedTableNode augmentedTableNode))
			{
				continue;
			}
			for (int i = 0; i < augmentedTableNode.JoinEdges.Count; i++)
			{
				JoinEdge joinEdge = augmentedTableNode.JoinEdges[i];
				int j = 0;
				for (AugmentedTableNode right = joinEdge.Right; j < right.JoinEdges.Count; j++)
				{
					JoinEdge edge = right.JoinEdges[j];
					GenerateTransitiveEdge(joinEdge, edge);
				}
			}
		}
	}

	private static bool CanBeEliminatedBasedOnLojParticipation(AugmentedTableNode table, AugmentedTableNode replacingTable)
	{
		if (replacingTable.Id < table.NewLocationId)
		{
			return CanBeMovedBasedOnLojParticipation(table, replacingTable);
		}
		return CanBeMovedBasedOnLojParticipation(replacingTable, table);
	}

	private static bool CanBeEliminatedViaStarJoinBasedOnOtherJoinParticipation(JoinEdge tableJoinEdge, JoinEdge replacingTableJoinEdge)
	{
		if (tableJoinEdge.JoinNode == null || replacingTableJoinEdge.JoinNode == null)
		{
			return false;
		}
		AugmentedNode leastCommonAncestor = GetLeastCommonAncestor(tableJoinEdge.Right, replacingTableJoinEdge.Right);
		if (!CanGetFileredByJoins(tableJoinEdge, leastCommonAncestor, disallowAnyJoin: true))
		{
			return !CanGetFileredByJoins(replacingTableJoinEdge, leastCommonAncestor, disallowAnyJoin: false);
		}
		return false;
	}

	private static bool CanGetFileredByJoins(JoinEdge joinEdge, AugmentedNode leastCommonAncestor, bool disallowAnyJoin)
	{
		AugmentedNode augmentedNode = joinEdge.Right;
		AugmentedNode parent = augmentedNode.Parent;
		while (parent != null && augmentedNode != leastCommonAncestor)
		{
			if (parent.Node != joinEdge.JoinNode.Node && (disallowAnyJoin || parent.Node.Op.OpType != OpType.LeftOuterJoin || parent.Children[0] != augmentedNode))
			{
				return true;
			}
			augmentedNode = augmentedNode.Parent;
			parent = augmentedNode.Parent;
		}
		return false;
	}

	private static bool CanBeMovedBasedOnLojParticipation(AugmentedTableNode table, AugmentedTableNode replacingTable)
	{
		AugmentedNode leastCommonAncestor = GetLeastCommonAncestor(table, replacingTable);
		AugmentedNode augmentedNode = table;
		while (augmentedNode.Parent != null && augmentedNode != leastCommonAncestor)
		{
			if (augmentedNode.Parent.Node.Op.OpType == OpType.LeftOuterJoin && augmentedNode.Parent.Children[0] == augmentedNode)
			{
				return false;
			}
			augmentedNode = augmentedNode.Parent;
		}
		return true;
	}

	private static AugmentedNode GetLeastCommonAncestor(AugmentedNode node1, AugmentedNode node2)
	{
		if (node1.Id == node2.Id)
		{
			return node1;
		}
		AugmentedNode augmentedNode;
		AugmentedNode augmentedNode2;
		if (node1.Id < node2.Id)
		{
			augmentedNode = node1;
			augmentedNode2 = node2;
		}
		else
		{
			augmentedNode = node2;
			augmentedNode2 = node1;
		}
		while (augmentedNode.Id < augmentedNode2.Id)
		{
			augmentedNode = augmentedNode.Parent;
		}
		return augmentedNode;
	}

	private void MarkTableAsEliminated<T>(AugmentedTableNode tableNode, AugmentedTableNode replacementNode, List<T> tableVars, List<T> replacementVars) where T : Var
	{
		PlanCompiler.Assert(tableVars != null && replacementVars != null, "null vars");
		PlanCompiler.Assert(tableVars.Count == replacementVars.Count, "var count mismatch");
		PlanCompiler.Assert(tableVars.Count > 0, "no vars in the table ?");
		m_modifiedGraph = true;
		if (tableNode.Id < replacementNode.NewLocationId)
		{
			tableNode.ReplacementTable = replacementNode;
			replacementNode.NewLocationId = tableNode.Id;
		}
		else
		{
			tableNode.ReplacementTable = null;
		}
		for (int i = 0; i < tableVars.Count; i++)
		{
			if (tableNode.Table.ReferencedColumns.IsSet(tableVars[i]))
			{
				m_varMap[tableVars[i]] = replacementVars[i];
				AddReverseMapping(replacementVars[i], tableVars[i]);
				replacementNode.Table.ReferencedColumns.Set(replacementVars[i]);
			}
		}
		foreach (Var referencedColumn in replacementNode.Table.ReferencedColumns)
		{
			m_varToDefiningNodeMap[referencedColumn] = replacementNode;
		}
	}

	private void AddReverseMapping(Var replacingVar, Var replacedVar)
	{
		if (m_reverseVarMap.TryGetValue(replacedVar, out var value))
		{
			m_reverseVarMap.Remove(replacedVar);
		}
		if (!m_reverseVarMap.TryGetValue(replacingVar, out var value2))
		{
			value2 = ((value == null) ? m_command.CreateVarVec() : value);
			m_reverseVarMap[replacingVar] = value2;
		}
		else if (value != null)
		{
			value2.Or(value);
		}
		value2.Set(replacedVar);
	}

	private void EliminateSelfJoinedTable(AugmentedTableNode tableNode, AugmentedTableNode replacementNode)
	{
		MarkTableAsEliminated(tableNode, replacementNode, tableNode.Table.Columns, replacementNode.Table.Columns);
	}

	private void EliminateStarSelfJoin(List<JoinEdge> joinEdges)
	{
		List<List<JoinEdge>> list = new List<List<JoinEdge>>();
		foreach (JoinEdge joinEdge2 in joinEdges)
		{
			bool flag = false;
			foreach (List<JoinEdge> item in list)
			{
				if (AreMatchingForStarSelfJoinElimination(item[0], joinEdge2))
				{
					item.Add(joinEdge2);
					flag = true;
					break;
				}
			}
			if (!flag && QualifiesForStarSelfJoinGroup(joinEdge2))
			{
				List<JoinEdge> list2 = new List<JoinEdge>();
				list2.Add(joinEdge2);
				list.Add(list2);
			}
		}
		foreach (List<JoinEdge> item2 in list.Where((List<JoinEdge> l) => l.Count > 1))
		{
			JoinEdge joinEdge = item2[0];
			foreach (JoinEdge item3 in item2)
			{
				if (joinEdge.Right.Id > item3.Right.Id)
				{
					joinEdge = item3;
				}
			}
			foreach (JoinEdge item4 in item2)
			{
				if (item4 != joinEdge && CanBeEliminatedViaStarJoinBasedOnOtherJoinParticipation(item4, joinEdge))
				{
					EliminateSelfJoinedTable(item4.Right, joinEdge.Right);
				}
			}
		}
	}

	private static bool AreMatchingForStarSelfJoinElimination(JoinEdge edge1, JoinEdge edge2)
	{
		if (edge2.LeftVars.Count != edge1.LeftVars.Count || edge2.JoinKind != edge1.JoinKind)
		{
			return false;
		}
		for (int i = 0; i < edge2.LeftVars.Count; i++)
		{
			if (!edge2.LeftVars[i].Equals(edge1.LeftVars[i]) || !edge2.RightVars[i].ColumnMetadata.Name.Equals(edge1.RightVars[i].ColumnMetadata.Name))
			{
				return false;
			}
		}
		return MatchOtherPredicates(edge1, edge2);
	}

	private static bool MatchOtherPredicates(JoinEdge edge1, JoinEdge edge2)
	{
		if (edge1.JoinNode == null)
		{
			return edge2.JoinNode == null;
		}
		if (edge2.JoinNode == null)
		{
			return false;
		}
		if (edge1.JoinNode.OtherPredicate == null)
		{
			return edge2.JoinNode.OtherPredicate == null;
		}
		if (edge2.JoinNode.OtherPredicate == null)
		{
			return false;
		}
		return MatchOtherPredicates(edge1.JoinNode.OtherPredicate, edge2.JoinNode.OtherPredicate);
	}

	private static bool MatchOtherPredicates(Node x, Node y)
	{
		if (x.Children.Count != y.Children.Count)
		{
			return false;
		}
		if (x.Op.IsEquivalent(y.Op))
		{
			return !x.Children.Where((Node t, int i) => !MatchOtherPredicates(t, y.Children[i])).Any();
		}
		if (!(x.Op is VarRefOp varRefOp))
		{
			return false;
		}
		if (!(y.Op is VarRefOp varRefOp2))
		{
			return false;
		}
		if (!(varRefOp.Var is ColumnVar columnVar))
		{
			return false;
		}
		if (!(varRefOp2.Var is ColumnVar columnVar2))
		{
			return false;
		}
		return columnVar.ColumnMetadata.Name.Equals(columnVar2.ColumnMetadata.Name);
	}

	private bool QualifiesForStarSelfJoinGroup(JoinEdge joinEdge)
	{
		VarVec varVec = m_command.CreateVarVec(joinEdge.Right.Table.Keys);
		foreach (ColumnVar rightVar in joinEdge.RightVars)
		{
			if (joinEdge.JoinKind == JoinKind.LeftOuter && !varVec.IsSet(rightVar))
			{
				return false;
			}
			varVec.Clear(rightVar);
		}
		if (!varVec.IsEmpty)
		{
			return false;
		}
		if (joinEdge.JoinNode != null && joinEdge.JoinNode.OtherPredicate != null)
		{
			return QualifiesForStarSelfJoinGroup(joinEdge.JoinNode.OtherPredicate, m_command.GetExtendedNodeInfo(joinEdge.Right.Node).Definitions);
		}
		return true;
	}

	private static bool QualifiesForStarSelfJoinGroup(Node otherPredicateNode, VarVec rightTableColumnVars)
	{
		if (!(otherPredicateNode.Op is VarRefOp varRefOp))
		{
			return true;
		}
		if (!(varRefOp.Var is ColumnVar v))
		{
			return true;
		}
		if (rightTableColumnVars.IsSet(v))
		{
			return otherPredicateNode.Children.All((Node node) => QualifiesForStarSelfJoinGroup(node, rightTableColumnVars));
		}
		return false;
	}

	private void EliminateStarSelfJoins(AugmentedTableNode tableNode)
	{
		Dictionary<EntitySetBase, List<JoinEdge>> dictionary = new Dictionary<EntitySetBase, List<JoinEdge>>();
		foreach (JoinEdge joinEdge in tableNode.JoinEdges)
		{
			if (!joinEdge.IsEliminated)
			{
				if (!dictionary.TryGetValue(joinEdge.Right.Table.TableMetadata.Extent, out var value))
				{
					value = new List<JoinEdge>();
					dictionary[joinEdge.Right.Table.TableMetadata.Extent] = value;
				}
				value.Add(joinEdge);
			}
		}
		foreach (KeyValuePair<EntitySetBase, List<JoinEdge>> item in dictionary)
		{
			if (item.Value.Count > 1)
			{
				EliminateStarSelfJoin(item.Value);
			}
		}
	}

	private bool EliminateSelfJoin(JoinEdge joinEdge)
	{
		if (joinEdge.RestrictedElimination)
		{
			return false;
		}
		if (joinEdge.IsEliminated)
		{
			return false;
		}
		if (!joinEdge.Left.Table.TableMetadata.Extent.Equals(joinEdge.Right.Table.TableMetadata.Extent))
		{
			return false;
		}
		for (int i = 0; i < joinEdge.LeftVars.Count; i++)
		{
			if (!joinEdge.LeftVars[i].ColumnMetadata.Name.Equals(joinEdge.RightVars[i].ColumnMetadata.Name))
			{
				return false;
			}
		}
		VarVec varVec = m_command.CreateVarVec(joinEdge.Left.Table.Keys);
		foreach (ColumnVar leftVar in joinEdge.LeftVars)
		{
			if (joinEdge.JoinKind == JoinKind.LeftOuter && !varVec.IsSet(leftVar))
			{
				return false;
			}
			varVec.Clear(leftVar);
		}
		if (!varVec.IsEmpty)
		{
			return false;
		}
		if (!CanBeEliminatedBasedOnLojParticipation(joinEdge.Right, joinEdge.Left))
		{
			return false;
		}
		EliminateSelfJoinedTable(joinEdge.Right, joinEdge.Left);
		return true;
	}

	private void EliminateSelfJoins(AugmentedTableNode tableNode)
	{
		if (tableNode.IsEliminated)
		{
			return;
		}
		foreach (JoinEdge joinEdge in tableNode.JoinEdges)
		{
			EliminateSelfJoin(joinEdge);
		}
	}

	private void EliminateSelfJoins()
	{
		foreach (AugmentedNode vertex in m_vertexes)
		{
			if (vertex is AugmentedTableNode tableNode)
			{
				EliminateSelfJoins(tableNode);
				EliminateStarSelfJoins(tableNode);
			}
		}
	}

	private void EliminateLeftTable(JoinEdge joinEdge)
	{
		PlanCompiler.Assert(joinEdge.JoinKind == JoinKind.Inner, "Expected inner join");
		MarkTableAsEliminated(joinEdge.Left, joinEdge.Right, joinEdge.LeftVars, joinEdge.RightVars);
		if (joinEdge.Right.NullableColumns == null)
		{
			joinEdge.Right.NullableColumns = m_command.CreateVarVec();
		}
		foreach (ColumnVar rightVar in joinEdge.RightVars)
		{
			if (rightVar.ColumnMetadata.IsNullable)
			{
				joinEdge.Right.NullableColumns.Set(rightVar);
			}
		}
	}

	private void EliminateRightTable(JoinEdge joinEdge)
	{
		PlanCompiler.Assert(joinEdge.JoinKind == JoinKind.LeftOuter, "Expected left-outer-join");
		PlanCompiler.Assert(joinEdge.Left.Id < joinEdge.Right.Id, "(left-id, right-id) = (" + joinEdge.Left.Id + "," + joinEdge.Right.Id + ")");
		MarkTableAsEliminated(joinEdge.Right, joinEdge.Left, joinEdge.RightVars, joinEdge.LeftVars);
	}

	private static bool HasNonKeyReferences(Table table)
	{
		return !table.Keys.Subsumes(table.ReferencedColumns);
	}

	private bool RightTableHasKeyReferences(JoinEdge joinEdge)
	{
		if (joinEdge.JoinNode == null)
		{
			return true;
		}
		VarVec varVec = null;
		foreach (Var key in joinEdge.Right.Table.Keys)
		{
			if (m_reverseVarMap.TryGetValue(key, out var value))
			{
				if (varVec == null)
				{
					varVec = joinEdge.Right.Table.Keys.Clone();
				}
				varVec.Or(value);
			}
		}
		if (varVec == null)
		{
			varVec = joinEdge.Right.Table.Keys;
		}
		return m_varRefManager.HasKeyReferences(varVec, joinEdge.Right.Node, joinEdge.JoinNode.Node);
	}

	private bool TryEliminateParentChildJoin(JoinEdge joinEdge, ForeignKeyConstraint fkConstraint)
	{
		if (joinEdge.JoinKind == JoinKind.LeftOuter && fkConstraint.ChildMultiplicity == RelationshipMultiplicity.Many)
		{
			return false;
		}
		if (!IsJoinOnFkConstraint(fkConstraint, joinEdge.LeftVars, joinEdge.RightVars, out var _))
		{
			return false;
		}
		if (joinEdge.JoinKind == JoinKind.Inner)
		{
			if (HasNonKeyReferences(joinEdge.Left.Table))
			{
				return false;
			}
			if (!CanBeEliminatedBasedOnLojParticipation(joinEdge.Right, joinEdge.Left))
			{
				return false;
			}
			EliminateLeftTable(joinEdge);
			return true;
		}
		return TryEliminateRightTable(joinEdge, fkConstraint.ChildKeys.Count, fkConstraint.ChildMultiplicity == RelationshipMultiplicity.One);
	}

	private static bool IsJoinOnFkConstraint(ForeignKeyConstraint fkConstraint, IList<ColumnVar> parentVars, IList<ColumnVar> childVars, out IList<ColumnVar> childForeignKeyVars)
	{
		childForeignKeyVars = new List<ColumnVar>(fkConstraint.ChildKeys.Count);
		foreach (string parentKey in fkConstraint.ParentKeys)
		{
			bool flag = false;
			foreach (ColumnVar parentVar in parentVars)
			{
				if (parentVar.ColumnMetadata.Name.Equals(parentKey))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		foreach (string childKey in fkConstraint.ChildKeys)
		{
			bool flag2 = false;
			for (int i = 0; i < parentVars.Count; i++)
			{
				ColumnVar columnVar = childVars[i];
				if (columnVar.ColumnMetadata.Name.Equals(childKey))
				{
					childForeignKeyVars.Add(columnVar);
					flag2 = true;
					ColumnVar columnVar2 = parentVars[i];
					if (fkConstraint.GetParentProperty(columnVar.ColumnMetadata.Name, out var parentPropertyName) && parentPropertyName.Equals(columnVar2.ColumnMetadata.Name))
					{
						break;
					}
					return false;
				}
			}
			if (!flag2)
			{
				return false;
			}
		}
		return true;
	}

	private bool TryEliminateChildParentJoin(JoinEdge joinEdge, ForeignKeyConstraint fkConstraint)
	{
		if (!IsJoinOnFkConstraint(fkConstraint, joinEdge.RightVars, joinEdge.LeftVars, out var childForeignKeyVars))
		{
			return false;
		}
		if (childForeignKeyVars.Count > 1 && childForeignKeyVars.Where((ColumnVar v) => v.ColumnMetadata.IsNullable).Count() > 0)
		{
			return false;
		}
		return TryEliminateRightTable(joinEdge, fkConstraint.ParentKeys.Count, allowRefsForJoinedOnFkOnly: true);
	}

	private bool TryEliminateRightTable(JoinEdge joinEdge, int fkConstraintKeyCount, bool allowRefsForJoinedOnFkOnly)
	{
		if (HasNonKeyReferences(joinEdge.Right.Table))
		{
			return false;
		}
		if ((!allowRefsForJoinedOnFkOnly || joinEdge.RightVars.Count != fkConstraintKeyCount) && RightTableHasKeyReferences(joinEdge))
		{
			return false;
		}
		if (!CanBeEliminatedBasedOnLojParticipation(joinEdge.Right, joinEdge.Left))
		{
			return false;
		}
		EliminateRightTable(joinEdge);
		return true;
	}

	private void EliminateParentChildJoin(JoinEdge joinEdge)
	{
		if (joinEdge.RestrictedElimination)
		{
			return;
		}
		if (m_constraintManager.IsParentChildRelationship(joinEdge.Left.Table.TableMetadata.Extent, joinEdge.Right.Table.TableMetadata.Extent, out var constraints))
		{
			PlanCompiler.Assert(constraints != null && constraints.Count > 0, "Invalid foreign key constraints");
			foreach (ForeignKeyConstraint item in constraints)
			{
				if (TryEliminateParentChildJoin(joinEdge, item))
				{
					return;
				}
			}
		}
		if (joinEdge.JoinKind != JoinKind.LeftOuter || !m_constraintManager.IsParentChildRelationship(joinEdge.Right.Table.TableMetadata.Extent, joinEdge.Left.Table.TableMetadata.Extent, out constraints))
		{
			return;
		}
		PlanCompiler.Assert(constraints != null && constraints.Count > 0, "Invalid foreign key constraints");
		foreach (ForeignKeyConstraint item2 in constraints)
		{
			if (TryEliminateChildParentJoin(joinEdge, item2))
			{
				break;
			}
		}
	}

	private void EliminateParentChildJoins(AugmentedTableNode tableNode)
	{
		foreach (JoinEdge joinEdge in tableNode.JoinEdges)
		{
			EliminateParentChildJoin(joinEdge);
			if (tableNode.IsEliminated)
			{
				break;
			}
		}
	}

	private void EliminateParentChildJoins()
	{
		foreach (AugmentedNode vertex in m_vertexes)
		{
			if (vertex is AugmentedTableNode { IsEliminated: false } augmentedTableNode)
			{
				EliminateParentChildJoins(augmentedTableNode);
			}
		}
	}

	private Node BuildNodeTree()
	{
		if (!m_modifiedGraph)
		{
			return m_root.Node;
		}
		VarMap varMap = new VarMap();
		foreach (KeyValuePair<Var, Var> item in m_varMap)
		{
			Var var = item.Value;
			Var value;
			while (m_varMap.TryGetValue(var, out value))
			{
				PlanCompiler.Assert(value != null, "null var mapping?");
				var = value;
			}
			varMap[item.Key] = var;
		}
		m_varMap = varMap;
		Dictionary<Node, int> predicates;
		Node node = RebuildNodeTree(m_root, out predicates);
		PlanCompiler.Assert(node != null, "Resulting node tree is null");
		PlanCompiler.Assert(predicates == null || predicates.Count == 0, "Leaking predicates?");
		return node;
	}

	private Node BuildFilterForNullableColumns(Node inputNode, VarVec nonNullableColumns)
	{
		if (nonNullableColumns == null)
		{
			return inputNode;
		}
		VarVec varVec = nonNullableColumns.Remap(m_varMap);
		if (varVec.IsEmpty)
		{
			return inputNode;
		}
		Node node = null;
		foreach (Var item in varVec)
		{
			Node arg = m_command.CreateNode(m_command.CreateVarRefOp(item));
			Node arg2 = m_command.CreateNode(m_command.CreateConditionalOp(OpType.IsNull), arg);
			arg2 = m_command.CreateNode(m_command.CreateConditionalOp(OpType.Not), arg2);
			node = ((node != null) ? m_command.CreateNode(m_command.CreateConditionalOp(OpType.And), node, arg2) : arg2);
		}
		PlanCompiler.Assert(node != null, "Null predicate?");
		return m_command.CreateNode(m_command.CreateFilterOp(), inputNode, node);
	}

	private Node BuildFilterNode(Node inputNode, Node predicateNode)
	{
		if (predicateNode == null)
		{
			return inputNode;
		}
		return m_command.CreateNode(m_command.CreateFilterOp(), inputNode, predicateNode);
	}

	private Node RebuildPredicate(AugmentedJoinNode joinNode, out int minLocationId)
	{
		minLocationId = joinNode.Id;
		if (joinNode.OtherPredicate != null)
		{
			foreach (Var externalReference in joinNode.OtherPredicate.GetNodeInfo(m_command).ExternalReferences)
			{
				if (!m_varMap.TryGetValue(externalReference, out var value))
				{
					value = externalReference;
				}
				minLocationId = GetLeastCommonAncestor(minLocationId, GetLocationId(value, minLocationId));
			}
		}
		Node node = joinNode.OtherPredicate;
		for (int i = 0; i < joinNode.LeftVars.Count; i++)
		{
			if (!m_varMap.TryGetValue(joinNode.LeftVars[i], out var value2))
			{
				value2 = joinNode.LeftVars[i];
			}
			if (!m_varMap.TryGetValue(joinNode.RightVars[i], out var value3))
			{
				value3 = joinNode.RightVars[i];
			}
			if (!value2.Equals(value3))
			{
				minLocationId = GetLeastCommonAncestor(minLocationId, GetLocationId(value2, minLocationId));
				minLocationId = GetLeastCommonAncestor(minLocationId, GetLocationId(value3, minLocationId));
				Node arg = m_command.CreateNode(m_command.CreateVarRefOp(value2));
				Node arg2 = m_command.CreateNode(m_command.CreateVarRefOp(value3));
				Node node2 = m_command.CreateNode(m_command.CreateComparisonOp(OpType.EQ), arg, arg2);
				node = ((node == null) ? node2 : PlanCompilerUtil.CombinePredicates(node2, node, m_command));
			}
		}
		return node;
	}

	private Node RebuildNodeTreeForCrossJoins(AugmentedJoinNode joinNode)
	{
		List<Node> list = new List<Node>();
		foreach (AugmentedNode child in joinNode.Children)
		{
			list.Add(RebuildNodeTree(child, out var predicates));
			PlanCompiler.Assert(predicates == null || predicates.Count == 0, "Leaking predicates");
		}
		if (list.Count == 0)
		{
			return null;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		Node node = m_command.CreateNode(m_command.CreateCrossJoinOp(), list);
		m_processedNodes[node] = node;
		return node;
	}

	private Node RebuildNodeTree(AugmentedJoinNode joinNode, out Dictionary<Node, int> predicates)
	{
		if (joinNode.Node.Op.OpType == OpType.CrossJoin)
		{
			predicates = null;
			return RebuildNodeTreeForCrossJoins(joinNode);
		}
		Dictionary<Node, int> predicates2;
		Node node = RebuildNodeTree(joinNode.Children[0], out predicates2);
		Dictionary<Node, int> predicates3;
		Node node2 = RebuildNodeTree(joinNode.Children[1], out predicates3);
		int minLocationId;
		Node localPredicateNode;
		if (node != null && node2 == null && joinNode.Node.Op.OpType == OpType.LeftOuterJoin)
		{
			minLocationId = joinNode.Id;
			localPredicateNode = null;
		}
		else
		{
			localPredicateNode = RebuildPredicate(joinNode, out minLocationId);
		}
		localPredicateNode = CombinePredicateNodes(joinNode.Id, localPredicateNode, minLocationId, predicates2, predicates3, out predicates);
		if (node == null && node2 == null)
		{
			if (localPredicateNode == null)
			{
				return null;
			}
			Node inputNode = m_command.CreateNode(m_command.CreateSingleRowTableOp());
			return BuildFilterNode(inputNode, localPredicateNode);
		}
		if (node == null)
		{
			return BuildFilterNode(node2, localPredicateNode);
		}
		if (node2 == null)
		{
			return BuildFilterNode(node, localPredicateNode);
		}
		if (localPredicateNode == null)
		{
			localPredicateNode = m_command.CreateNode(m_command.CreateTrueOp());
		}
		Node node3 = m_command.CreateNode(joinNode.Node.Op, node, node2, localPredicateNode);
		m_processedNodes[node3] = node3;
		return node3;
	}

	private Node RebuildNodeTree(AugmentedTableNode tableNode)
	{
		AugmentedTableNode augmentedTableNode = tableNode;
		if (tableNode.IsMoved)
		{
			return null;
		}
		while (augmentedTableNode.IsEliminated)
		{
			augmentedTableNode = augmentedTableNode.ReplacementTable;
			if (augmentedTableNode == null)
			{
				return null;
			}
		}
		if (augmentedTableNode.NewLocationId < tableNode.Id)
		{
			return null;
		}
		return BuildFilterForNullableColumns(augmentedTableNode.Node, augmentedTableNode.NullableColumns);
	}

	private Node RebuildNodeTree(AugmentedNode augmentedNode, out Dictionary<Node, int> predicates)
	{
		switch (augmentedNode.Node.Op.OpType)
		{
		case OpType.ScanTable:
			predicates = null;
			return RebuildNodeTree((AugmentedTableNode)augmentedNode);
		case OpType.InnerJoin:
		case OpType.LeftOuterJoin:
		case OpType.FullOuterJoin:
		case OpType.CrossJoin:
			return RebuildNodeTree((AugmentedJoinNode)augmentedNode, out predicates);
		default:
			predicates = null;
			return augmentedNode.Node;
		}
	}

	private Node CombinePredicateNodes(int targetNodeId, Node localPredicateNode, int localPredicateMinLocationId, Dictionary<Node, int> leftPredicates, Dictionary<Node, int> rightPredicates, out Dictionary<Node, int> outPredicates)
	{
		Node result = null;
		outPredicates = new Dictionary<Node, int>();
		if (localPredicateNode != null)
		{
			result = ClassifyPredicate(targetNodeId, localPredicateNode, localPredicateMinLocationId, result, outPredicates);
		}
		if (leftPredicates != null)
		{
			foreach (KeyValuePair<Node, int> leftPredicate in leftPredicates)
			{
				result = ClassifyPredicate(targetNodeId, leftPredicate.Key, leftPredicate.Value, result, outPredicates);
			}
		}
		if (rightPredicates != null)
		{
			foreach (KeyValuePair<Node, int> rightPredicate in rightPredicates)
			{
				result = ClassifyPredicate(targetNodeId, rightPredicate.Key, rightPredicate.Value, result, outPredicates);
			}
		}
		return result;
	}

	private Node ClassifyPredicate(int targetNodeId, Node predicateNode, int predicateMinLocationId, Node result, Dictionary<Node, int> outPredicates)
	{
		if (targetNodeId >= predicateMinLocationId)
		{
			result = CombinePredicates(result, predicateNode);
		}
		else
		{
			outPredicates.Add(predicateNode, predicateMinLocationId);
		}
		return result;
	}

	private Node CombinePredicates(Node node1, Node node2)
	{
		if (node1 == null)
		{
			return node2;
		}
		if (node2 == null)
		{
			return node1;
		}
		return PlanCompilerUtil.CombinePredicates(node1, node2, m_command);
	}

	private int GetLocationId(Var var, int defaultLocationId)
	{
		if (m_varToDefiningNodeMap.TryGetValue(var, out var value))
		{
			if (value.IsMoved)
			{
				return value.NewLocationId;
			}
			return value.Id;
		}
		return defaultLocationId;
	}

	private int GetLeastCommonAncestor(int nodeId1, int nodeId2)
	{
		if (nodeId1 == nodeId2)
		{
			return nodeId1;
		}
		AugmentedNode augmentedNode = m_root;
		AugmentedNode augmentedNode2 = augmentedNode;
		AugmentedNode augmentedNode3 = augmentedNode;
		while (augmentedNode2 == augmentedNode3)
		{
			augmentedNode = augmentedNode2;
			if (augmentedNode.Id == nodeId1 || augmentedNode.Id == nodeId2)
			{
				return augmentedNode.Id;
			}
			augmentedNode2 = PickSubtree(nodeId1, augmentedNode);
			augmentedNode3 = PickSubtree(nodeId2, augmentedNode);
		}
		return augmentedNode.Id;
	}

	private static AugmentedNode PickSubtree(int nodeId, AugmentedNode root)
	{
		AugmentedNode augmentedNode = root.Children[0];
		int num = 1;
		while (augmentedNode.Id < nodeId && num < root.Children.Count)
		{
			augmentedNode = root.Children[num];
			num++;
		}
		return augmentedNode;
	}
}

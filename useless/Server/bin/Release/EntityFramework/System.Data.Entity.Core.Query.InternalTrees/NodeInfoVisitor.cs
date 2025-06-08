using System.Collections.Generic;
using System.Data.Entity.Core.Common;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class NodeInfoVisitor : BasicOpVisitorOfT<NodeInfo>
{
	private readonly Command m_command;

	internal void RecomputeNodeInfo(Node n)
	{
		if (n.IsNodeInfoInitialized)
		{
			VisitNode(n).ComputeHashValue(m_command, n);
		}
	}

	internal NodeInfoVisitor(Command command)
	{
		m_command = command;
	}

	private NodeInfo GetNodeInfo(Node n)
	{
		return n.GetNodeInfo(m_command);
	}

	private ExtendedNodeInfo GetExtendedNodeInfo(Node n)
	{
		return n.GetExtendedNodeInfo(m_command);
	}

	private NodeInfo InitNodeInfo(Node n)
	{
		NodeInfo nodeInfo = GetNodeInfo(n);
		nodeInfo.Clear();
		return nodeInfo;
	}

	private ExtendedNodeInfo InitExtendedNodeInfo(Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = GetExtendedNodeInfo(n);
		extendedNodeInfo.Clear();
		return extendedNodeInfo;
	}

	protected override NodeInfo VisitDefault(Node n)
	{
		NodeInfo nodeInfo = InitNodeInfo(n);
		foreach (Node child in n.Children)
		{
			NodeInfo nodeInfo2 = GetNodeInfo(child);
			nodeInfo.ExternalReferences.Or(nodeInfo2.ExternalReferences);
		}
		return nodeInfo;
	}

	private static bool IsDefinitionNonNullable(Node definition, VarVec nonNullableInputs)
	{
		if (definition.Op.OpType != 0 && definition.Op.OpType != OpType.InternalConstant && definition.Op.OpType != OpType.NullSentinel)
		{
			if (definition.Op.OpType == OpType.VarRef)
			{
				return nonNullableInputs.IsSet(((VarRefOp)definition.Op).Var);
			}
			return false;
		}
		return true;
	}

	public override NodeInfo Visit(VarRefOp op, Node n)
	{
		NodeInfo nodeInfo = InitNodeInfo(n);
		nodeInfo.ExternalReferences.Set(op.Var);
		return nodeInfo;
	}

	protected override NodeInfo VisitRelOpDefault(RelOp op, Node n)
	{
		return Unimplemented(n);
	}

	protected override NodeInfo VisitTableOp(ScanTableBaseOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		extendedNodeInfo.LocalDefinitions.Or(op.Table.ReferencedColumns);
		extendedNodeInfo.Definitions.Or(op.Table.ReferencedColumns);
		if (op.Table.ReferencedColumns.Subsumes(op.Table.Keys))
		{
			extendedNodeInfo.Keys.InitFrom(op.Table.Keys);
		}
		extendedNodeInfo.NonNullableDefinitions.Or(op.Table.NonNullableColumns);
		extendedNodeInfo.NonNullableDefinitions.And(extendedNodeInfo.Definitions);
		return extendedNodeInfo;
	}

	public override NodeInfo Visit(UnnestOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		foreach (Var column in op.Table.Columns)
		{
			extendedNodeInfo.LocalDefinitions.Set(column);
			extendedNodeInfo.Definitions.Set(column);
		}
		if (n.Child0.Op.OpType == OpType.VarDef && n.Child0.Child0.Op.OpType == OpType.Function && op.Table.Keys.Count > 0 && op.Table.ReferencedColumns.Subsumes(op.Table.Keys))
		{
			extendedNodeInfo.Keys.InitFrom(op.Table.Keys);
		}
		if (n.HasChild0)
		{
			NodeInfo nodeInfo = GetNodeInfo(n.Child0);
			extendedNodeInfo.ExternalReferences.Or(nodeInfo.ExternalReferences);
		}
		else
		{
			extendedNodeInfo.ExternalReferences.Set(op.Var);
		}
		return extendedNodeInfo;
	}

	internal static Dictionary<Var, Var> ComputeVarRemappings(Node varDefListNode)
	{
		Dictionary<Var, Var> dictionary = new Dictionary<Var, Var>();
		foreach (Node child in varDefListNode.Children)
		{
			if (child.Child0.Op is VarRefOp varRefOp)
			{
				VarDefOp varDefOp = child.Op as VarDefOp;
				dictionary[varRefOp.Var] = varDefOp.Var;
			}
		}
		return dictionary;
	}

	public override NodeInfo Visit(ProjectOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		foreach (Var output in op.Outputs)
		{
			if (extendedNodeInfo2.Definitions.IsSet(output))
			{
				extendedNodeInfo.Definitions.Set(output);
			}
			else
			{
				extendedNodeInfo.ExternalReferences.Set(output);
			}
		}
		extendedNodeInfo.NonNullableDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.NonNullableDefinitions.And(op.Outputs);
		extendedNodeInfo.NonNullableVisibleDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		foreach (Node child in n.Child1.Children)
		{
			VarDefOp varDefOp = child.Op as VarDefOp;
			NodeInfo nodeInfo = GetNodeInfo(child.Child0);
			extendedNodeInfo.LocalDefinitions.Set(varDefOp.Var);
			extendedNodeInfo.ExternalReferences.Clear(varDefOp.Var);
			extendedNodeInfo.Definitions.Set(varDefOp.Var);
			extendedNodeInfo.ExternalReferences.Or(nodeInfo.ExternalReferences);
			if (IsDefinitionNonNullable(child.Child0, extendedNodeInfo.NonNullableVisibleDefinitions))
			{
				extendedNodeInfo.NonNullableDefinitions.Set(varDefOp.Var);
			}
		}
		extendedNodeInfo.ExternalReferences.Minus(extendedNodeInfo2.Definitions);
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo2.ExternalReferences);
		extendedNodeInfo.Keys.NoKeys = true;
		if (!extendedNodeInfo2.Keys.NoKeys)
		{
			VarVec varVec = m_command.CreateVarVec(extendedNodeInfo2.Keys.KeyVars);
			Dictionary<Var, Var> varMap = ComputeVarRemappings(n.Child1);
			VarVec varVec2 = varVec.Remap(varMap);
			VarVec varSet = varVec2.Clone();
			VarVec other = m_command.CreateVarVec(op.Outputs);
			varVec2.Minus(other);
			if (varVec2.IsEmpty)
			{
				extendedNodeInfo.Keys.InitFrom(varSet);
			}
		}
		extendedNodeInfo.InitRowCountFrom(extendedNodeInfo2);
		return extendedNodeInfo;
	}

	public override NodeInfo Visit(FilterOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		NodeInfo nodeInfo = GetNodeInfo(n.Child1);
		extendedNodeInfo.Definitions.Or(extendedNodeInfo2.Definitions);
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo2.ExternalReferences);
		extendedNodeInfo.ExternalReferences.Or(nodeInfo.ExternalReferences);
		extendedNodeInfo.ExternalReferences.Minus(extendedNodeInfo2.Definitions);
		extendedNodeInfo.Keys.InitFrom(extendedNodeInfo2.Keys);
		extendedNodeInfo.NonNullableDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.NonNullableVisibleDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.MinRows = RowCount.Zero;
		if (n.Child1.Op is ConstantPredicateOp { IsFalse: not false })
		{
			extendedNodeInfo.MaxRows = RowCount.Zero;
		}
		else
		{
			extendedNodeInfo.MaxRows = extendedNodeInfo2.MaxRows;
		}
		return extendedNodeInfo;
	}

	protected override NodeInfo VisitGroupByOp(GroupByBaseOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		extendedNodeInfo.Definitions.InitFrom(op.Outputs);
		extendedNodeInfo.LocalDefinitions.InitFrom(extendedNodeInfo.Definitions);
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo2.ExternalReferences);
		foreach (Node child in n.Child1.Children)
		{
			NodeInfo nodeInfo = GetNodeInfo(child.Child0);
			extendedNodeInfo.ExternalReferences.Or(nodeInfo.ExternalReferences);
			if (IsDefinitionNonNullable(child.Child0, extendedNodeInfo2.NonNullableDefinitions))
			{
				extendedNodeInfo.NonNullableDefinitions.Set(((VarDefOp)child.Op).Var);
			}
		}
		extendedNodeInfo.NonNullableDefinitions.Or(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.NonNullableDefinitions.And(op.Keys);
		for (int i = 2; i < n.Children.Count; i++)
		{
			foreach (Node child2 in n.Children[i].Children)
			{
				NodeInfo nodeInfo2 = GetNodeInfo(child2.Child0);
				extendedNodeInfo.ExternalReferences.Or(nodeInfo2.ExternalReferences);
			}
		}
		extendedNodeInfo.ExternalReferences.Minus(extendedNodeInfo2.Definitions);
		extendedNodeInfo.Keys.InitFrom(op.Keys);
		extendedNodeInfo.MinRows = ((op.Keys.IsEmpty || extendedNodeInfo2.MinRows == RowCount.One) ? RowCount.One : RowCount.Zero);
		extendedNodeInfo.MaxRows = (op.Keys.IsEmpty ? RowCount.One : extendedNodeInfo2.MaxRows);
		return extendedNodeInfo;
	}

	public override NodeInfo Visit(CrossJoinOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		List<KeyVec> list = new List<KeyVec>();
		RowCount rowCount = RowCount.Zero;
		RowCount rowCount2 = RowCount.One;
		foreach (Node child in n.Children)
		{
			ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(child);
			extendedNodeInfo.Definitions.Or(extendedNodeInfo2.Definitions);
			extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo2.ExternalReferences);
			list.Add(extendedNodeInfo2.Keys);
			extendedNodeInfo.NonNullableDefinitions.Or(extendedNodeInfo2.NonNullableDefinitions);
			if ((int)extendedNodeInfo2.MaxRows > (int)rowCount)
			{
				rowCount = extendedNodeInfo2.MaxRows;
			}
			if ((int)extendedNodeInfo2.MinRows < (int)rowCount2)
			{
				rowCount2 = extendedNodeInfo2.MinRows;
			}
		}
		extendedNodeInfo.Keys.InitFrom(list);
		extendedNodeInfo.SetRowCount(rowCount2, rowCount);
		return extendedNodeInfo;
	}

	protected override NodeInfo VisitJoinOp(JoinBaseOp op, Node n)
	{
		if (op.OpType != OpType.InnerJoin && op.OpType != OpType.LeftOuterJoin && op.OpType != OpType.FullOuterJoin)
		{
			return Unimplemented(n);
		}
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		ExtendedNodeInfo extendedNodeInfo3 = GetExtendedNodeInfo(n.Child1);
		NodeInfo nodeInfo = GetNodeInfo(n.Child2);
		extendedNodeInfo.Definitions.Or(extendedNodeInfo2.Definitions);
		extendedNodeInfo.Definitions.Or(extendedNodeInfo3.Definitions);
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo2.ExternalReferences);
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo3.ExternalReferences);
		extendedNodeInfo.ExternalReferences.Or(nodeInfo.ExternalReferences);
		extendedNodeInfo.ExternalReferences.Minus(extendedNodeInfo.Definitions);
		extendedNodeInfo.Keys.InitFrom(extendedNodeInfo2.Keys, extendedNodeInfo3.Keys);
		if (op.OpType == OpType.InnerJoin || op.OpType == OpType.LeftOuterJoin)
		{
			extendedNodeInfo.NonNullableDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		}
		if (op.OpType == OpType.InnerJoin)
		{
			extendedNodeInfo.NonNullableDefinitions.Or(extendedNodeInfo3.NonNullableDefinitions);
		}
		extendedNodeInfo.NonNullableVisibleDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.NonNullableVisibleDefinitions.Or(extendedNodeInfo3.NonNullableDefinitions);
		RowCount maxRows;
		RowCount minRows;
		if (op.OpType != OpType.FullOuterJoin)
		{
			maxRows = (((int)extendedNodeInfo2.MaxRows <= 1 && (int)extendedNodeInfo3.MaxRows <= 1) ? RowCount.One : RowCount.Unbounded);
			minRows = ((op.OpType == OpType.LeftOuterJoin) ? extendedNodeInfo2.MinRows : RowCount.Zero);
		}
		else
		{
			minRows = RowCount.Zero;
			maxRows = RowCount.Unbounded;
		}
		extendedNodeInfo.SetRowCount(minRows, maxRows);
		return extendedNodeInfo;
	}

	protected override NodeInfo VisitApplyOp(ApplyBaseOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		ExtendedNodeInfo extendedNodeInfo3 = GetExtendedNodeInfo(n.Child1);
		extendedNodeInfo.Definitions.Or(extendedNodeInfo2.Definitions);
		extendedNodeInfo.Definitions.Or(extendedNodeInfo3.Definitions);
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo2.ExternalReferences);
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo3.ExternalReferences);
		extendedNodeInfo.ExternalReferences.Minus(extendedNodeInfo.Definitions);
		extendedNodeInfo.Keys.InitFrom(extendedNodeInfo2.Keys, extendedNodeInfo3.Keys);
		extendedNodeInfo.NonNullableDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		if (op.OpType == OpType.CrossApply)
		{
			extendedNodeInfo.NonNullableDefinitions.Or(extendedNodeInfo3.NonNullableDefinitions);
		}
		extendedNodeInfo.NonNullableVisibleDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.NonNullableVisibleDefinitions.Or(extendedNodeInfo3.NonNullableDefinitions);
		RowCount maxRows = (((int)extendedNodeInfo2.MaxRows <= 1 && (int)extendedNodeInfo3.MaxRows <= 1) ? RowCount.One : RowCount.Unbounded);
		RowCount minRows = ((op.OpType != OpType.CrossApply) ? extendedNodeInfo2.MinRows : RowCount.Zero);
		extendedNodeInfo.SetRowCount(minRows, maxRows);
		return extendedNodeInfo;
	}

	protected override NodeInfo VisitSetOp(SetOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		extendedNodeInfo.Definitions.InitFrom(op.Outputs);
		extendedNodeInfo.LocalDefinitions.InitFrom(op.Outputs);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		ExtendedNodeInfo extendedNodeInfo3 = GetExtendedNodeInfo(n.Child1);
		RowCount minRows = RowCount.Zero;
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo2.ExternalReferences);
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo3.ExternalReferences);
		if (op.OpType == OpType.UnionAll)
		{
			minRows = (((int)extendedNodeInfo2.MinRows > (int)extendedNodeInfo3.MinRows) ? extendedNodeInfo2.MinRows : extendedNodeInfo3.MinRows);
		}
		if (op.OpType == OpType.Intersect || op.OpType == OpType.Except)
		{
			extendedNodeInfo.Keys.InitFrom(op.Outputs);
		}
		else
		{
			UnionAllOp unionAllOp = (UnionAllOp)op;
			if (unionAllOp.BranchDiscriminator == null)
			{
				extendedNodeInfo.Keys.NoKeys = true;
			}
			else
			{
				VarVec varVec = m_command.CreateVarVec();
				for (int i = 0; i < n.Children.Count; i++)
				{
					ExtendedNodeInfo extendedNodeInfo4 = n.Children[i].GetExtendedNodeInfo(m_command);
					if (!extendedNodeInfo4.Keys.NoKeys && !extendedNodeInfo4.Keys.KeyVars.IsEmpty)
					{
						VarVec other = extendedNodeInfo4.Keys.KeyVars.Remap(unionAllOp.VarMap[i].GetReverseMap());
						varVec.Or(other);
						continue;
					}
					varVec.Clear();
					break;
				}
				if (varVec.IsEmpty)
				{
					extendedNodeInfo.Keys.NoKeys = true;
				}
				else
				{
					extendedNodeInfo.Keys.InitFrom(varVec);
				}
			}
		}
		VarVec other2 = extendedNodeInfo2.NonNullableDefinitions.Remap(op.VarMap[0].GetReverseMap());
		extendedNodeInfo.NonNullableDefinitions.InitFrom(other2);
		if (op.OpType != OpType.Except)
		{
			VarVec other3 = extendedNodeInfo3.NonNullableDefinitions.Remap(op.VarMap[1].GetReverseMap());
			if (op.OpType == OpType.Intersect)
			{
				extendedNodeInfo.NonNullableDefinitions.Or(other3);
			}
			else
			{
				extendedNodeInfo.NonNullableDefinitions.And(other3);
			}
		}
		extendedNodeInfo.NonNullableDefinitions.And(op.Outputs);
		extendedNodeInfo.MinRows = minRows;
		return extendedNodeInfo;
	}

	protected override NodeInfo VisitSortOp(SortBaseOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		extendedNodeInfo.Definitions.Or(extendedNodeInfo2.Definitions);
		extendedNodeInfo.ExternalReferences.Or(extendedNodeInfo2.ExternalReferences);
		extendedNodeInfo.ExternalReferences.Minus(extendedNodeInfo2.Definitions);
		extendedNodeInfo.Keys.InitFrom(extendedNodeInfo2.Keys);
		extendedNodeInfo.NonNullableDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.NonNullableVisibleDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.InitRowCountFrom(extendedNodeInfo2);
		if (OpType.ConstrainedSort == op.OpType && n.Child2.Op.OpType == OpType.Constant && !((ConstrainedSortOp)op).WithTies)
		{
			ConstantBaseOp constantBaseOp = (ConstantBaseOp)n.Child2.Op;
			if (TypeHelpers.IsIntegerConstant(constantBaseOp.Type, constantBaseOp.Value, 1L))
			{
				extendedNodeInfo.SetRowCount(RowCount.Zero, RowCount.One);
			}
		}
		return extendedNodeInfo;
	}

	public override NodeInfo Visit(DistinctOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		extendedNodeInfo.Keys.InitFrom(op.Keys, ignoreParameters: true);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		extendedNodeInfo.ExternalReferences.InitFrom(extendedNodeInfo2.ExternalReferences);
		foreach (Var key in op.Keys)
		{
			if (extendedNodeInfo2.Definitions.IsSet(key))
			{
				extendedNodeInfo.Definitions.Set(key);
			}
			else
			{
				extendedNodeInfo.ExternalReferences.Set(key);
			}
		}
		extendedNodeInfo.NonNullableDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.NonNullableDefinitions.And(op.Keys);
		extendedNodeInfo.InitRowCountFrom(extendedNodeInfo2);
		return extendedNodeInfo;
	}

	public override NodeInfo Visit(SingleRowOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		extendedNodeInfo.Definitions.InitFrom(extendedNodeInfo2.Definitions);
		extendedNodeInfo.Keys.InitFrom(extendedNodeInfo2.Keys);
		extendedNodeInfo.ExternalReferences.InitFrom(extendedNodeInfo2.ExternalReferences);
		extendedNodeInfo.NonNullableDefinitions.InitFrom(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.SetRowCount(RowCount.Zero, RowCount.One);
		return extendedNodeInfo;
	}

	public override NodeInfo Visit(SingleRowTableOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		extendedNodeInfo.Keys.NoKeys = false;
		extendedNodeInfo.SetRowCount(RowCount.One, RowCount.One);
		return extendedNodeInfo;
	}

	public override NodeInfo Visit(PhysicalProjectOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		foreach (Node child in n.Children)
		{
			NodeInfo nodeInfo = GetNodeInfo(child);
			extendedNodeInfo.ExternalReferences.Or(nodeInfo.ExternalReferences);
		}
		extendedNodeInfo.Definitions.InitFrom(op.Outputs);
		extendedNodeInfo.LocalDefinitions.InitFrom(extendedNodeInfo.Definitions);
		ExtendedNodeInfo extendedNodeInfo2 = GetExtendedNodeInfo(n.Child0);
		if (!extendedNodeInfo2.Keys.NoKeys)
		{
			VarVec varVec = m_command.CreateVarVec(extendedNodeInfo2.Keys.KeyVars);
			varVec.Minus(extendedNodeInfo.Definitions);
			if (varVec.IsEmpty)
			{
				extendedNodeInfo.Keys.InitFrom(extendedNodeInfo2.Keys);
			}
		}
		extendedNodeInfo.NonNullableDefinitions.Or(extendedNodeInfo2.NonNullableDefinitions);
		extendedNodeInfo.NonNullableDefinitions.And(extendedNodeInfo.Definitions);
		extendedNodeInfo.NonNullableVisibleDefinitions.Or(extendedNodeInfo2.NonNullableVisibleDefinitions);
		return extendedNodeInfo;
	}

	protected override NodeInfo VisitNestOp(NestBaseOp op, Node n)
	{
		SingleStreamNestOp singleStreamNestOp = op as SingleStreamNestOp;
		ExtendedNodeInfo extendedNodeInfo = InitExtendedNodeInfo(n);
		foreach (CollectionInfo item in op.CollectionInfo)
		{
			extendedNodeInfo.LocalDefinitions.Set(item.CollectionVar);
		}
		extendedNodeInfo.Definitions.InitFrom(op.Outputs);
		foreach (Node child in n.Children)
		{
			extendedNodeInfo.ExternalReferences.Or(GetExtendedNodeInfo(child).ExternalReferences);
		}
		extendedNodeInfo.ExternalReferences.Minus(extendedNodeInfo.Definitions);
		if (singleStreamNestOp == null)
		{
			extendedNodeInfo.Keys.InitFrom(GetExtendedNodeInfo(n.Child0).Keys);
		}
		else
		{
			extendedNodeInfo.Keys.InitFrom(singleStreamNestOp.Keys);
		}
		return extendedNodeInfo;
	}
}

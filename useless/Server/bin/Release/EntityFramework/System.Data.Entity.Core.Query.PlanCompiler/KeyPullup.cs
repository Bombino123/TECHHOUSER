using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class KeyPullup : BasicOpVisitor
{
	private readonly Command m_command;

	internal KeyPullup(Command command)
	{
		m_command = command;
	}

	internal KeyVec GetKeys(Node node)
	{
		ExtendedNodeInfo extendedNodeInfo = node.GetExtendedNodeInfo(m_command);
		if (extendedNodeInfo.Keys.NoKeys)
		{
			VisitNode(node);
		}
		return extendedNodeInfo.Keys;
	}

	protected override void VisitChildren(Node n)
	{
		foreach (Node child in n.Children)
		{
			if (child.Op.IsRelOp || child.Op.IsPhysicalOp)
			{
				GetKeys(child);
			}
		}
	}

	protected override void VisitRelOpDefault(RelOp op, Node n)
	{
		VisitChildren(n);
		m_command.RecomputeNodeInfo(n);
	}

	public override void Visit(ScanTableOp op, Node n)
	{
		op.Table.ReferencedColumns.Or(op.Table.Keys);
		m_command.RecomputeNodeInfo(n);
	}

	public override void Visit(ProjectOp op, Node n)
	{
		VisitChildren(n);
		ExtendedNodeInfo extendedNodeInfo = n.Child0.GetExtendedNodeInfo(m_command);
		if (!extendedNodeInfo.Keys.NoKeys)
		{
			VarVec varVec = m_command.CreateVarVec(op.Outputs);
			Dictionary<Var, Var> varMap = NodeInfoVisitor.ComputeVarRemappings(n.Child1);
			VarVec other = extendedNodeInfo.Keys.KeyVars.Remap(varMap);
			varVec.Or(other);
			op.Outputs.InitFrom(varVec);
		}
		m_command.RecomputeNodeInfo(n);
	}

	public override void Visit(UnionAllOp op, Node n)
	{
		VisitChildren(n);
		Var var = m_command.CreateSetOpVar(m_command.IntegerType);
		VarList varList = Command.CreateVarList();
		VarVec[] array = new VarVec[n.Children.Count];
		for (int i = 0; i < n.Children.Count; i++)
		{
			Node node = n.Children[i];
			VarVec v = m_command.GetExtendedNodeInfo(node).Keys.KeyVars.Remap(op.VarMap[i]);
			array[i] = m_command.CreateVarVec(v);
			array[i].Minus(op.Outputs);
			if (OpType.UnionAll == node.Op.OpType)
			{
				UnionAllOp unionAllOp = (UnionAllOp)node.Op;
				array[i].Clear(unionAllOp.BranchDiscriminator);
			}
			varList.AddRange(array[i]);
		}
		VarList varList2 = Command.CreateVarList();
		foreach (Var item2 in varList)
		{
			Var item = m_command.CreateSetOpVar(item2.Type);
			varList2.Add(item);
		}
		for (int j = 0; j < n.Children.Count; j++)
		{
			Node node2 = n.Children[j];
			ExtendedNodeInfo extendedNodeInfo = m_command.GetExtendedNodeInfo(node2);
			VarVec varVec = m_command.CreateVarVec();
			List<Node> list = new List<Node>();
			Var computedVar;
			if (OpType.UnionAll == node2.Op.OpType && ((UnionAllOp)node2.Op).BranchDiscriminator != null)
			{
				computedVar = ((UnionAllOp)node2.Op).BranchDiscriminator;
				if (!op.VarMap[j].ContainsValue(computedVar))
				{
					op.VarMap[j].Add(var, computedVar);
				}
				else
				{
					PlanCompiler.Assert(j == 0, "right branch has a discriminator var that the left branch doesn't have?");
					var = op.VarMap[j].GetReverseMap()[computedVar];
				}
			}
			else
			{
				list.Add(m_command.CreateVarDefNode(m_command.CreateNode(m_command.CreateConstantOp(m_command.IntegerType, m_command.NextBranchDiscriminatorValue)), out computedVar));
				varVec.Set(computedVar);
				op.VarMap[j].Add(var, computedVar);
			}
			for (int k = 0; k < varList.Count; k++)
			{
				Var computedVar2 = varList[k];
				if (!array[j].IsSet(computedVar2))
				{
					list.Add(m_command.CreateVarDefNode(m_command.CreateNode(m_command.CreateNullOp(computedVar2.Type)), out computedVar2));
					varVec.Set(computedVar2);
				}
				op.VarMap[j].Add(varList2[k], computedVar2);
			}
			if (varVec.IsEmpty)
			{
				extendedNodeInfo.Keys.KeyVars.Set(computedVar);
				continue;
			}
			PlanCompiler.Assert(list.Count != 0, "no new nodes?");
			foreach (Var value in op.VarMap[j].Values)
			{
				varVec.Set(value);
			}
			n.Children[j] = m_command.CreateNode(m_command.CreateProjectOp(varVec), node2, m_command.CreateNode(m_command.CreateVarDefListOp(), list));
			m_command.RecomputeNodeInfo(n.Children[j]);
			ExtendedNodeInfo extendedNodeInfo2 = m_command.GetExtendedNodeInfo(n.Children[j]);
			extendedNodeInfo2.Keys.KeyVars.InitFrom(extendedNodeInfo.Keys.KeyVars);
			extendedNodeInfo2.Keys.KeyVars.Set(computedVar);
		}
		n.Op = m_command.CreateUnionAllOp(op.VarMap[0], op.VarMap[1], var);
		m_command.RecomputeNodeInfo(n);
	}
}

using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class JoinOpRules
{
	internal static readonly PatternMatchRule Rule_CrossJoinOverProject1 = new PatternMatchRule(new Node(CrossJoinOp.Pattern, new Node(LeafOp.Pattern), new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))), ProcessJoinOverProject);

	internal static readonly PatternMatchRule Rule_CrossJoinOverProject2 = new PatternMatchRule(new Node(CrossJoinOp.Pattern, new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessJoinOverProject);

	internal static readonly PatternMatchRule Rule_InnerJoinOverProject1 = new PatternMatchRule(new Node(InnerJoinOp.Pattern, new Node(LeafOp.Pattern), new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessJoinOverProject);

	internal static readonly PatternMatchRule Rule_InnerJoinOverProject2 = new PatternMatchRule(new Node(InnerJoinOp.Pattern, new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), ProcessJoinOverProject);

	internal static readonly PatternMatchRule Rule_OuterJoinOverProject2 = new PatternMatchRule(new Node(LeftOuterJoinOp.Pattern, new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), ProcessJoinOverProject);

	internal static readonly PatternMatchRule Rule_CrossJoinOverFilter1 = new PatternMatchRule(new Node(CrossJoinOp.Pattern, new Node(LeafOp.Pattern), new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))), ProcessJoinOverFilter);

	internal static readonly PatternMatchRule Rule_CrossJoinOverFilter2 = new PatternMatchRule(new Node(CrossJoinOp.Pattern, new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessJoinOverFilter);

	internal static readonly PatternMatchRule Rule_InnerJoinOverFilter1 = new PatternMatchRule(new Node(InnerJoinOp.Pattern, new Node(LeafOp.Pattern), new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessJoinOverFilter);

	internal static readonly PatternMatchRule Rule_InnerJoinOverFilter2 = new PatternMatchRule(new Node(InnerJoinOp.Pattern, new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), ProcessJoinOverFilter);

	internal static readonly PatternMatchRule Rule_OuterJoinOverFilter2 = new PatternMatchRule(new Node(LeftOuterJoinOp.Pattern, new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), ProcessJoinOverFilter);

	internal static readonly PatternMatchRule Rule_CrossJoinOverSingleRowTable1 = new PatternMatchRule(new Node(CrossJoinOp.Pattern, new Node(SingleRowTableOp.Pattern), new Node(LeafOp.Pattern)), ProcessJoinOverSingleRowTable);

	internal static readonly PatternMatchRule Rule_CrossJoinOverSingleRowTable2 = new PatternMatchRule(new Node(CrossJoinOp.Pattern, new Node(LeafOp.Pattern), new Node(SingleRowTableOp.Pattern)), ProcessJoinOverSingleRowTable);

	internal static readonly PatternMatchRule Rule_LeftOuterJoinOverSingleRowTable = new PatternMatchRule(new Node(LeftOuterJoinOp.Pattern, new Node(LeafOp.Pattern), new Node(SingleRowTableOp.Pattern), new Node(LeafOp.Pattern)), ProcessJoinOverSingleRowTable);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[13]
	{
		Rule_CrossJoinOverProject1, Rule_CrossJoinOverProject2, Rule_InnerJoinOverProject1, Rule_InnerJoinOverProject2, Rule_OuterJoinOverProject2, Rule_CrossJoinOverFilter1, Rule_CrossJoinOverFilter2, Rule_InnerJoinOverFilter1, Rule_InnerJoinOverFilter2, Rule_OuterJoinOverFilter2,
		Rule_CrossJoinOverSingleRowTable1, Rule_CrossJoinOverSingleRowTable2, Rule_LeftOuterJoinOverSingleRowTable
	};

	private static bool ProcessJoinOverProject(RuleProcessingContext context, Node joinNode, out Node newNode)
	{
		newNode = joinNode;
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Command command = transformationRulesContext.Command;
		Node node = (joinNode.HasChild2 ? joinNode.Child2 : null);
		Dictionary<Var, int> varRefMap = new Dictionary<Var, int>();
		if (node != null && !transformationRulesContext.IsScalarOpTree(node, varRefMap))
		{
			return false;
		}
		VarVec varVec = command.CreateVarVec();
		List<Node> list = new List<Node>();
		if (joinNode.Op.OpType != OpType.LeftOuterJoin && joinNode.Child0.Op.OpType == OpType.Project && joinNode.Child1.Op.OpType == OpType.Project)
		{
			ProjectOp projectOp = (ProjectOp)joinNode.Child0.Op;
			ProjectOp projectOp2 = (ProjectOp)joinNode.Child1.Op;
			Dictionary<Var, Node> varMap = transformationRulesContext.GetVarMap(joinNode.Child0.Child1, varRefMap);
			Dictionary<Var, Node> varMap2 = transformationRulesContext.GetVarMap(joinNode.Child1.Child1, varRefMap);
			if (varMap == null || varMap2 == null)
			{
				return false;
			}
			Node arg;
			if (node != null)
			{
				node = transformationRulesContext.ReMap(node, varMap);
				node = transformationRulesContext.ReMap(node, varMap2);
				arg = context.Command.CreateNode(joinNode.Op, joinNode.Child0.Child0, joinNode.Child1.Child0, node);
			}
			else
			{
				arg = context.Command.CreateNode(joinNode.Op, joinNode.Child0.Child0, joinNode.Child1.Child0);
			}
			varVec.InitFrom(projectOp.Outputs);
			foreach (Var output in projectOp2.Outputs)
			{
				varVec.Set(output);
			}
			ProjectOp op = command.CreateProjectOp(varVec);
			list.AddRange(joinNode.Child0.Child1.Children);
			list.AddRange(joinNode.Child1.Child1.Children);
			Node arg2 = command.CreateNode(command.CreateVarDefListOp(), list);
			Node node2 = command.CreateNode(op, arg, arg2);
			newNode = node2;
			return true;
		}
		int num = -1;
		int num2 = -1;
		if (joinNode.Child0.Op.OpType == OpType.Project)
		{
			num = 0;
			num2 = 1;
		}
		else
		{
			PlanCompiler.Assert(joinNode.Op.OpType != OpType.LeftOuterJoin, "unexpected non-LeftOuterJoin");
			num = 1;
			num2 = 0;
		}
		Node node3 = joinNode.Children[num];
		ProjectOp projectOp3 = node3.Op as ProjectOp;
		Dictionary<Var, Node> varMap3 = transformationRulesContext.GetVarMap(node3.Child1, varRefMap);
		if (varMap3 == null)
		{
			return false;
		}
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(joinNode.Children[num2]);
		VarVec varVec2 = command.CreateVarVec(projectOp3.Outputs);
		varVec2.Or(extendedNodeInfo.Definitions);
		projectOp3.Outputs.InitFrom(varVec2);
		if (node != null)
		{
			node = transformationRulesContext.ReMap(node, varMap3);
			joinNode.Child2 = node;
		}
		joinNode.Children[num] = node3.Child0;
		context.Command.RecomputeNodeInfo(joinNode);
		newNode = context.Command.CreateNode(projectOp3, joinNode, node3.Child1);
		return true;
	}

	private static bool ProcessJoinOverFilter(RuleProcessingContext context, Node joinNode, out Node newNode)
	{
		newNode = joinNode;
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Command command = transformationRulesContext.Command;
		Node node = null;
		Node child = joinNode.Child0;
		if (joinNode.Child0.Op.OpType == OpType.Filter)
		{
			node = joinNode.Child0.Child1;
			child = joinNode.Child0.Child0;
		}
		Node arg = joinNode.Child1;
		if (joinNode.Child1.Op.OpType == OpType.Filter && joinNode.Op.OpType != OpType.LeftOuterJoin)
		{
			node = ((node != null) ? command.CreateNode(command.CreateConditionalOp(OpType.And), node, joinNode.Child1.Child1) : joinNode.Child1.Child1);
			arg = joinNode.Child1.Child0;
		}
		if (node == null)
		{
			return false;
		}
		Node arg2 = ((joinNode.Op.OpType != OpType.CrossJoin) ? command.CreateNode(joinNode.Op, child, arg, joinNode.Child2) : command.CreateNode(joinNode.Op, child, arg));
		FilterOp op = command.CreateFilterOp();
		newNode = command.CreateNode(op, arg2, node);
		transformationRulesContext.SuppressFilterPushdown(newNode);
		return true;
	}

	private static bool ProcessJoinOverSingleRowTable(RuleProcessingContext context, Node joinNode, out Node newNode)
	{
		newNode = joinNode;
		if (joinNode.Child0.Op.OpType == OpType.SingleRowTable)
		{
			newNode = joinNode.Child1;
		}
		else
		{
			newNode = joinNode.Child0;
		}
		return true;
	}
}

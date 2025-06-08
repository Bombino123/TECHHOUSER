using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class ScalarOpRules
{
	internal static readonly SimpleRule Rule_SimplifyCase = new SimpleRule(OpType.Case, ProcessSimplifyCase);

	internal static readonly SimpleRule Rule_FlattenCase = new SimpleRule(OpType.Case, ProcessFlattenCase);

	internal static readonly PatternMatchRule Rule_IsNullOverCase = new PatternMatchRule(new Node(ConditionalOp.PatternIsNull, new Node(CaseOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))), ProcessIsNullOverCase);

	internal static readonly PatternMatchRule Rule_EqualsOverConstant = new PatternMatchRule(new Node(ComparisonOp.PatternEq, new Node(InternalConstantOp.Pattern), new Node(InternalConstantOp.Pattern)), ProcessComparisonsOverConstant);

	internal static readonly PatternMatchRule Rule_LikeOverConstants = new PatternMatchRule(new Node(LikeOp.Pattern, new Node(InternalConstantOp.Pattern), new Node(InternalConstantOp.Pattern), new Node(NullOp.Pattern)), ProcessLikeOverConstant);

	internal static readonly PatternMatchRule Rule_AndOverConstantPred1 = new PatternMatchRule(new Node(ConditionalOp.PatternAnd, new Node(LeafOp.Pattern), new Node(ConstantPredicateOp.Pattern)), ProcessAndOverConstantPredicate1);

	internal static readonly PatternMatchRule Rule_AndOverConstantPred2 = new PatternMatchRule(new Node(ConditionalOp.PatternAnd, new Node(ConstantPredicateOp.Pattern), new Node(LeafOp.Pattern)), ProcessAndOverConstantPredicate2);

	internal static readonly PatternMatchRule Rule_OrOverConstantPred1 = new PatternMatchRule(new Node(ConditionalOp.PatternOr, new Node(LeafOp.Pattern), new Node(ConstantPredicateOp.Pattern)), ProcessOrOverConstantPredicate1);

	internal static readonly PatternMatchRule Rule_OrOverConstantPred2 = new PatternMatchRule(new Node(ConditionalOp.PatternOr, new Node(ConstantPredicateOp.Pattern), new Node(LeafOp.Pattern)), ProcessOrOverConstantPredicate2);

	internal static readonly PatternMatchRule Rule_NotOverConstantPred = new PatternMatchRule(new Node(ConditionalOp.PatternNot, new Node(ConstantPredicateOp.Pattern)), ProcessNotOverConstantPredicate);

	internal static readonly PatternMatchRule Rule_IsNullOverConstant = new PatternMatchRule(new Node(ConditionalOp.PatternIsNull, new Node(InternalConstantOp.Pattern)), ProcessIsNullOverConstant);

	internal static readonly PatternMatchRule Rule_IsNullOverNullSentinel = new PatternMatchRule(new Node(ConditionalOp.PatternIsNull, new Node(NullSentinelOp.Pattern)), ProcessIsNullOverConstant);

	internal static readonly PatternMatchRule Rule_IsNullOverNull = new PatternMatchRule(new Node(ConditionalOp.PatternIsNull, new Node(NullOp.Pattern)), ProcessIsNullOverNull);

	internal static readonly PatternMatchRule Rule_NullCast = new PatternMatchRule(new Node(CastOp.Pattern, new Node(NullOp.Pattern)), ProcessNullCast);

	internal static readonly PatternMatchRule Rule_IsNullOverVarRef = new PatternMatchRule(new Node(ConditionalOp.PatternIsNull, new Node(VarRefOp.Pattern)), ProcessIsNullOverVarRef);

	internal static readonly PatternMatchRule Rule_IsNullOverAnything = new PatternMatchRule(new Node(ConditionalOp.PatternIsNull, new Node(LeafOp.Pattern)), ProcessIsNullOverAnything);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[15]
	{
		Rule_IsNullOverCase, Rule_SimplifyCase, Rule_FlattenCase, Rule_LikeOverConstants, Rule_EqualsOverConstant, Rule_AndOverConstantPred1, Rule_AndOverConstantPred2, Rule_OrOverConstantPred1, Rule_OrOverConstantPred2, Rule_NotOverConstantPred,
		Rule_IsNullOverConstant, Rule_IsNullOverNullSentinel, Rule_IsNullOverNull, Rule_NullCast, Rule_IsNullOverVarRef
	};

	private static bool ProcessSimplifyCase(RuleProcessingContext context, Node caseOpNode, out Node newNode)
	{
		CaseOp caseOp = (CaseOp)caseOpNode.Op;
		newNode = caseOpNode;
		if (ProcessSimplifyCase_Collapse(caseOpNode, out newNode))
		{
			return true;
		}
		if (ProcessSimplifyCase_EliminateWhenClauses(context, caseOp, caseOpNode, out newNode))
		{
			return true;
		}
		return false;
	}

	private static bool ProcessSimplifyCase_Collapse(Node caseOpNode, out Node newNode)
	{
		newNode = caseOpNode;
		Node child = caseOpNode.Child1;
		Node other = caseOpNode.Children[caseOpNode.Children.Count - 1];
		if (!child.IsEquivalent(other))
		{
			return false;
		}
		for (int i = 3; i < caseOpNode.Children.Count - 1; i += 2)
		{
			if (!caseOpNode.Children[i].IsEquivalent(child))
			{
				return false;
			}
		}
		newNode = child;
		return true;
	}

	private static bool ProcessSimplifyCase_EliminateWhenClauses(RuleProcessingContext context, CaseOp caseOp, Node caseOpNode, out Node newNode)
	{
		List<Node> list = null;
		newNode = caseOpNode;
		int num = 0;
		while (num < caseOpNode.Children.Count)
		{
			if (num == caseOpNode.Children.Count - 1)
			{
				if (OpType.SoftCast == caseOpNode.Children[num].Op.OpType)
				{
					return false;
				}
				list?.Add(caseOpNode.Children[num]);
				break;
			}
			if (OpType.SoftCast == caseOpNode.Children[num + 1].Op.OpType)
			{
				return false;
			}
			if (caseOpNode.Children[num].Op.OpType != OpType.ConstantPredicate)
			{
				if (list != null)
				{
					list.Add(caseOpNode.Children[num]);
					list.Add(caseOpNode.Children[num + 1]);
				}
				num += 2;
				continue;
			}
			ConstantPredicateOp constantPredicateOp = (ConstantPredicateOp)caseOpNode.Children[num].Op;
			if (list == null)
			{
				list = new List<Node>();
				for (int i = 0; i < num; i++)
				{
					list.Add(caseOpNode.Children[i]);
				}
			}
			if (constantPredicateOp.IsTrue)
			{
				list.Add(caseOpNode.Children[num + 1]);
				break;
			}
			PlanCompiler.Assert(constantPredicateOp.IsFalse, "constant predicate must be either true or false");
			num += 2;
		}
		if (list == null)
		{
			return false;
		}
		PlanCompiler.Assert(list.Count > 0, "new args list must not be empty");
		if (list.Count == 1)
		{
			newNode = list[0];
		}
		else
		{
			newNode = context.Command.CreateNode(caseOp, list);
		}
		return true;
	}

	private static bool ProcessFlattenCase(RuleProcessingContext context, Node caseOpNode, out Node newNode)
	{
		newNode = caseOpNode;
		Node node = caseOpNode.Children[caseOpNode.Children.Count - 1];
		if (node.Op.OpType != OpType.Case)
		{
			return false;
		}
		caseOpNode.Children.RemoveAt(caseOpNode.Children.Count - 1);
		caseOpNode.Children.AddRange(node.Children);
		return true;
	}

	private static bool ProcessIsNullOverCase(RuleProcessingContext context, Node isNullOpNode, out Node newNode)
	{
		Node child = isNullOpNode.Child0;
		if (child.Children.Count != 3)
		{
			newNode = isNullOpNode;
			return false;
		}
		Node child2 = child.Child0;
		Node child3 = child.Child1;
		Node child4 = child.Child2;
		switch (child3.Op.OpType)
		{
		case OpType.Null:
		{
			OpType opType = child4.Op.OpType;
			if ((uint)opType <= 2u)
			{
				newNode = child2;
				return true;
			}
			break;
		}
		case OpType.Constant:
		case OpType.InternalConstant:
		case OpType.NullSentinel:
			if (child4.Op.OpType == OpType.Null)
			{
				newNode = context.Command.CreateNode(context.Command.CreateConditionalOp(OpType.Not), child2);
				return true;
			}
			break;
		}
		newNode = isNullOpNode;
		return false;
	}

	private static bool ProcessComparisonsOverConstant(RuleProcessingContext context, Node node, out Node newNode)
	{
		newNode = node;
		PlanCompiler.Assert(node.Op.OpType == OpType.EQ || node.Op.OpType == OpType.NE, "unexpected comparison op type?");
		bool? flag = node.Child0.Op.IsEquivalent(node.Child1.Op);
		if (!flag.HasValue)
		{
			return false;
		}
		bool value = ((node.Op.OpType == OpType.EQ) ? flag.Value : (!flag.Value));
		ConstantPredicateOp op = context.Command.CreateConstantPredicateOp(value);
		newNode = context.Command.CreateNode(op);
		return true;
	}

	private static bool? MatchesPattern(string str, string pattern)
	{
		int num = pattern.IndexOf('%');
		if (num == -1 || num != pattern.Length - 1 || pattern.Length > str.Length + 1)
		{
			return null;
		}
		bool value = true;
		int num2 = 0;
		for (num2 = 0; num2 < str.Length && num2 < pattern.Length - 1; num2++)
		{
			if (pattern[num2] != str[num2])
			{
				value = false;
				break;
			}
		}
		return value;
	}

	private static bool ProcessLikeOverConstant(RuleProcessingContext context, Node n, out Node newNode)
	{
		newNode = n;
		InternalConstantOp internalConstantOp = (InternalConstantOp)n.Child1.Op;
		bool? flag = MatchesPattern((string)((InternalConstantOp)n.Child0.Op).Value, (string)internalConstantOp.Value);
		if (!flag.HasValue)
		{
			return false;
		}
		ConstantPredicateOp op = context.Command.CreateConstantPredicateOp(flag.Value);
		newNode = context.Command.CreateNode(op);
		return true;
	}

	private static bool ProcessLogOpOverConstant(RuleProcessingContext context, Node node, Node constantPredicateNode, Node otherNode, out Node newNode)
	{
		PlanCompiler.Assert(constantPredicateNode != null, "null constantPredicateOp?");
		ConstantPredicateOp constantPredicateOp = (ConstantPredicateOp)constantPredicateNode.Op;
		switch (node.Op.OpType)
		{
		case OpType.And:
			newNode = (constantPredicateOp.IsTrue ? otherNode : constantPredicateNode);
			break;
		case OpType.Or:
			newNode = (constantPredicateOp.IsTrue ? constantPredicateNode : otherNode);
			break;
		case OpType.Not:
			PlanCompiler.Assert(otherNode == null, "Not Op with more than 1 child. Gasp!");
			newNode = context.Command.CreateNode(context.Command.CreateConstantPredicateOp(!constantPredicateOp.Value));
			break;
		default:
			PlanCompiler.Assert(condition: false, "Unexpected OpType - " + node.Op.OpType);
			newNode = null;
			break;
		}
		return true;
	}

	private static bool ProcessAndOverConstantPredicate1(RuleProcessingContext context, Node node, out Node newNode)
	{
		return ProcessLogOpOverConstant(context, node, node.Child1, node.Child0, out newNode);
	}

	private static bool ProcessAndOverConstantPredicate2(RuleProcessingContext context, Node node, out Node newNode)
	{
		return ProcessLogOpOverConstant(context, node, node.Child0, node.Child1, out newNode);
	}

	private static bool ProcessOrOverConstantPredicate1(RuleProcessingContext context, Node node, out Node newNode)
	{
		return ProcessLogOpOverConstant(context, node, node.Child1, node.Child0, out newNode);
	}

	private static bool ProcessOrOverConstantPredicate2(RuleProcessingContext context, Node node, out Node newNode)
	{
		return ProcessLogOpOverConstant(context, node, node.Child0, node.Child1, out newNode);
	}

	private static bool ProcessNotOverConstantPredicate(RuleProcessingContext context, Node node, out Node newNode)
	{
		return ProcessLogOpOverConstant(context, node, node.Child0, null, out newNode);
	}

	private static bool ProcessIsNullOverConstant(RuleProcessingContext context, Node isNullNode, out Node newNode)
	{
		newNode = context.Command.CreateNode(context.Command.CreateFalseOp());
		return true;
	}

	private static bool ProcessIsNullOverNull(RuleProcessingContext context, Node isNullNode, out Node newNode)
	{
		newNode = context.Command.CreateNode(context.Command.CreateTrueOp());
		return true;
	}

	private static bool ProcessNullCast(RuleProcessingContext context, Node castNullOp, out Node newNode)
	{
		newNode = context.Command.CreateNode(context.Command.CreateNullOp(castNullOp.Op.Type));
		return true;
	}

	private static bool ProcessIsNullOverVarRef(RuleProcessingContext context, Node isNullNode, out Node newNode)
	{
		Command command = context.Command;
		TransformationRulesContext obj = (TransformationRulesContext)context;
		Var var = ((VarRefOp)isNullNode.Child0.Op).Var;
		if (obj.IsNonNullable(var))
		{
			newNode = command.CreateNode(context.Command.CreateFalseOp());
			return true;
		}
		newNode = isNullNode;
		return false;
	}

	private static bool ProcessIsNullOverAnything(RuleProcessingContext context, Node isNullNode, out Node newNode)
	{
		Command command = context.Command;
		switch (isNullNode.Child0.Op.OpType)
		{
		case OpType.Cast:
			newNode = command.CreateNode(command.CreateConditionalOp(OpType.IsNull), isNullNode.Child0.Child0);
			break;
		case OpType.Function:
		{
			EdmFunction function = ((FunctionOp)isNullNode.Child0.Op).Function;
			newNode = (PreservesNulls(function) ? command.CreateNode(command.CreateConditionalOp(OpType.IsNull), isNullNode.Child0.Child0) : isNullNode);
			break;
		}
		default:
			newNode = isNullNode;
			break;
		}
		switch (isNullNode.Child0.Op.OpType)
		{
		case OpType.Constant:
		case OpType.InternalConstant:
		case OpType.NullSentinel:
			return ProcessIsNullOverConstant(context, newNode, out newNode);
		case OpType.Null:
			return ProcessIsNullOverNull(context, newNode, out newNode);
		case OpType.VarRef:
			return ProcessIsNullOverVarRef(context, newNode, out newNode);
		default:
			return isNullNode != newNode;
		}
	}

	private static bool PreservesNulls(EdmFunction function)
	{
		return function.FullName == "Edm.Length";
	}
}

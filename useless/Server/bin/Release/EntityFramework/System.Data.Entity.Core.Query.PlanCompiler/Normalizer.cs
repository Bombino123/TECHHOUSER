using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class Normalizer : SubqueryTrackingVisitor
{
	private Normalizer(PlanCompiler planCompilerState)
		: base(planCompilerState)
	{
	}

	internal static void Process(PlanCompiler planCompilerState)
	{
		new Normalizer(planCompilerState).Process();
	}

	private void Process()
	{
		base.m_command.Root = VisitNode(base.m_command.Root);
	}

	public override Node Visit(ExistsOp op, Node n)
	{
		VisitChildren(n);
		n.Child0 = BuildDummyProjectForExists(n.Child0);
		return n;
	}

	private Node BuildDummyProjectForExists(Node child)
	{
		Var projectVar;
		return base.m_command.BuildProject(child, base.m_command.CreateNode(base.m_command.CreateInternalConstantOp(base.m_command.IntegerType, 1)), out projectVar);
	}

	private Node BuildUnnest(Node collectionNode)
	{
		PlanCompiler.Assert(collectionNode.Op.IsScalarOp, "non-scalar usage of Un-nest?");
		PlanCompiler.Assert(TypeSemantics.IsCollectionType(collectionNode.Op.Type), "non-collection usage for Un-nest?");
		Var computedVar;
		Node arg = base.m_command.CreateVarDefNode(collectionNode, out computedVar);
		UnnestOp op = base.m_command.CreateUnnestOp(computedVar);
		return base.m_command.CreateNode(op, arg);
	}

	private Node VisitCollectionFunction(FunctionOp op, Node n)
	{
		PlanCompiler.Assert(TypeSemantics.IsCollectionType(op.Type), "non-TVF function?");
		Node node = BuildUnnest(n);
		UnnestOp unnestOp = node.Op as UnnestOp;
		PhysicalProjectOp op2 = base.m_command.CreatePhysicalProjectOp(unnestOp.Table.Columns[0]);
		Node arg = base.m_command.CreateNode(op2, node);
		CollectOp op3 = base.m_command.CreateCollectOp(n.Op.Type);
		return base.m_command.CreateNode(op3, arg);
	}

	private Node VisitCollectionAggregateFunction(FunctionOp op, Node n)
	{
		TypeUsage typeUsage = null;
		Node child = n.Child0;
		if (OpType.SoftCast == child.Op.OpType)
		{
			typeUsage = TypeHelpers.GetEdmType<CollectionType>(child.Op.Type).TypeUsage;
			child = child.Child0;
			while (OpType.SoftCast == child.Op.OpType)
			{
				child = child.Child0;
			}
		}
		Node node = BuildUnnest(child);
		Var v = (node.Op as UnnestOp).Table.Columns[0];
		AggregateOp op2 = base.m_command.CreateAggregateOp(op.Function, distinctAgg: false);
		VarRefOp op3 = base.m_command.CreateVarRefOp(v);
		Node arg = base.m_command.CreateNode(op3);
		if (typeUsage != null)
		{
			arg = base.m_command.CreateNode(base.m_command.CreateSoftCastOp(typeUsage), arg);
		}
		Node definingExpr = base.m_command.CreateNode(op2, arg);
		VarVec gbyKeys = base.m_command.CreateVarVec();
		Node arg2 = base.m_command.CreateNode(base.m_command.CreateVarDefListOp());
		VarVec varVec = base.m_command.CreateVarVec();
		Var computedVar;
		Node arg3 = base.m_command.CreateVarDefListNode(definingExpr, out computedVar);
		varVec.Set(computedVar);
		GroupByOp op4 = base.m_command.CreateGroupByOp(gbyKeys, varVec);
		Node subquery = base.m_command.CreateNode(op4, node, arg2, arg3);
		return AddSubqueryToParentRelOp(computedVar, subquery);
	}

	public override Node Visit(FunctionOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
		Node node = null;
		node = (TypeSemantics.IsCollectionType(op.Type) ? VisitCollectionFunction(op, n) : ((!PlanCompilerUtil.IsCollectionAggregateFunction(op, n)) ? n : VisitCollectionAggregateFunction(op, n)));
		PlanCompiler.Assert(node != null, "failure to construct a functionOp?");
		return node;
	}

	protected override Node VisitJoinOp(JoinBaseOp op, Node n)
	{
		if (ProcessJoinOp(n))
		{
			n.Child2.Child0 = BuildDummyProjectForExists(n.Child2.Child0);
		}
		return n;
	}
}

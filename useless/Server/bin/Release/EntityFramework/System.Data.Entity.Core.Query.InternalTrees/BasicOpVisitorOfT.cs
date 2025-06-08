using System.Data.Entity.Core.Query.PlanCompiler;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class BasicOpVisitorOfT<TResultType>
{
	protected virtual void VisitChildren(Node n)
	{
		for (int i = 0; i < n.Children.Count; i++)
		{
			VisitNode(n.Children[i]);
		}
	}

	protected virtual void VisitChildrenReverse(Node n)
	{
		for (int num = n.Children.Count - 1; num >= 0; num--)
		{
			VisitNode(n.Children[num]);
		}
	}

	internal TResultType VisitNode(Node n)
	{
		return n.Op.Accept(this, n);
	}

	protected virtual TResultType VisitDefault(Node n)
	{
		VisitChildren(n);
		return default(TResultType);
	}

	internal virtual TResultType Unimplemented(Node n)
	{
		System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(condition: false, "Not implemented op type");
		return default(TResultType);
	}

	public virtual TResultType Visit(Op op, Node n)
	{
		return Unimplemented(n);
	}

	protected virtual TResultType VisitAncillaryOpDefault(AncillaryOp op, Node n)
	{
		return VisitDefault(n);
	}

	public virtual TResultType Visit(VarDefOp op, Node n)
	{
		return VisitAncillaryOpDefault(op, n);
	}

	public virtual TResultType Visit(VarDefListOp op, Node n)
	{
		return VisitAncillaryOpDefault(op, n);
	}

	protected virtual TResultType VisitPhysicalOpDefault(PhysicalOp op, Node n)
	{
		return VisitDefault(n);
	}

	public virtual TResultType Visit(PhysicalProjectOp op, Node n)
	{
		return VisitPhysicalOpDefault(op, n);
	}

	protected virtual TResultType VisitNestOp(NestBaseOp op, Node n)
	{
		return VisitPhysicalOpDefault(op, n);
	}

	public virtual TResultType Visit(SingleStreamNestOp op, Node n)
	{
		return VisitNestOp(op, n);
	}

	public virtual TResultType Visit(MultiStreamNestOp op, Node n)
	{
		return VisitNestOp(op, n);
	}

	protected virtual TResultType VisitRelOpDefault(RelOp op, Node n)
	{
		return VisitDefault(n);
	}

	protected virtual TResultType VisitApplyOp(ApplyBaseOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	public virtual TResultType Visit(CrossApplyOp op, Node n)
	{
		return VisitApplyOp(op, n);
	}

	public virtual TResultType Visit(OuterApplyOp op, Node n)
	{
		return VisitApplyOp(op, n);
	}

	protected virtual TResultType VisitJoinOp(JoinBaseOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	public virtual TResultType Visit(CrossJoinOp op, Node n)
	{
		return VisitJoinOp(op, n);
	}

	public virtual TResultType Visit(FullOuterJoinOp op, Node n)
	{
		return VisitJoinOp(op, n);
	}

	public virtual TResultType Visit(LeftOuterJoinOp op, Node n)
	{
		return VisitJoinOp(op, n);
	}

	public virtual TResultType Visit(InnerJoinOp op, Node n)
	{
		return VisitJoinOp(op, n);
	}

	protected virtual TResultType VisitSetOp(SetOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	public virtual TResultType Visit(ExceptOp op, Node n)
	{
		return VisitSetOp(op, n);
	}

	public virtual TResultType Visit(IntersectOp op, Node n)
	{
		return VisitSetOp(op, n);
	}

	public virtual TResultType Visit(UnionAllOp op, Node n)
	{
		return VisitSetOp(op, n);
	}

	public virtual TResultType Visit(DistinctOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	public virtual TResultType Visit(FilterOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	protected virtual TResultType VisitGroupByOp(GroupByBaseOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	public virtual TResultType Visit(GroupByOp op, Node n)
	{
		return VisitGroupByOp(op, n);
	}

	public virtual TResultType Visit(GroupByIntoOp op, Node n)
	{
		return VisitGroupByOp(op, n);
	}

	public virtual TResultType Visit(ProjectOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	protected virtual TResultType VisitTableOp(ScanTableBaseOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	public virtual TResultType Visit(ScanTableOp op, Node n)
	{
		return VisitTableOp(op, n);
	}

	public virtual TResultType Visit(ScanViewOp op, Node n)
	{
		return VisitTableOp(op, n);
	}

	public virtual TResultType Visit(SingleRowOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	public virtual TResultType Visit(SingleRowTableOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	protected virtual TResultType VisitSortOp(SortBaseOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	public virtual TResultType Visit(SortOp op, Node n)
	{
		return VisitSortOp(op, n);
	}

	public virtual TResultType Visit(ConstrainedSortOp op, Node n)
	{
		return VisitSortOp(op, n);
	}

	public virtual TResultType Visit(UnnestOp op, Node n)
	{
		return VisitRelOpDefault(op, n);
	}

	protected virtual TResultType VisitScalarOpDefault(ScalarOp op, Node n)
	{
		return VisitDefault(n);
	}

	protected virtual TResultType VisitConstantOp(ConstantBaseOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(AggregateOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(ArithmeticOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(CaseOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(CastOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(SoftCastOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(CollectOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(ComparisonOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(ConditionalOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(ConstantOp op, Node n)
	{
		return VisitConstantOp(op, n);
	}

	public virtual TResultType Visit(ConstantPredicateOp op, Node n)
	{
		return VisitConstantOp(op, n);
	}

	public virtual TResultType Visit(ElementOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(ExistsOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(FunctionOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(GetEntityRefOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(GetRefKeyOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(InternalConstantOp op, Node n)
	{
		return VisitConstantOp(op, n);
	}

	public virtual TResultType Visit(IsOfOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(LikeOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(NewEntityOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(NewInstanceOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(DiscriminatedNewEntityOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(NewMultisetOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(NewRecordOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(NullOp op, Node n)
	{
		return VisitConstantOp(op, n);
	}

	public virtual TResultType Visit(NullSentinelOp op, Node n)
	{
		return VisitConstantOp(op, n);
	}

	public virtual TResultType Visit(PropertyOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(RelPropertyOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(RefOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(TreatOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(VarRefOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(DerefOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}

	public virtual TResultType Visit(NavigateOp op, Node n)
	{
		return VisitScalarOpDefault(op, n);
	}
}

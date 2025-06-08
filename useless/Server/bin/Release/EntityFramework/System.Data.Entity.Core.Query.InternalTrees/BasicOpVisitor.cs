using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class BasicOpVisitor
{
	protected virtual void VisitChildren(Node n)
	{
		foreach (Node child in n.Children)
		{
			VisitNode(child);
		}
	}

	protected virtual void VisitChildrenReverse(Node n)
	{
		for (int num = n.Children.Count - 1; num >= 0; num--)
		{
			VisitNode(n.Children[num]);
		}
	}

	internal virtual void VisitNode(Node n)
	{
		n.Op.Accept(this, n);
	}

	protected virtual void VisitDefault(Node n)
	{
		VisitChildren(n);
	}

	protected virtual void VisitConstantOp(ConstantBaseOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	protected virtual void VisitTableOp(ScanTableBaseOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	protected virtual void VisitJoinOp(JoinBaseOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	protected virtual void VisitApplyOp(ApplyBaseOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	protected virtual void VisitSetOp(SetOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	protected virtual void VisitSortOp(SortBaseOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	protected virtual void VisitGroupByOp(GroupByBaseOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	public virtual void Visit(Op op, Node n)
	{
		throw new NotSupportedException(Strings.Iqt_General_UnsupportedOp(op.GetType().FullName));
	}

	protected virtual void VisitScalarOpDefault(ScalarOp op, Node n)
	{
		VisitDefault(n);
	}

	public virtual void Visit(ConstantOp op, Node n)
	{
		VisitConstantOp(op, n);
	}

	public virtual void Visit(NullOp op, Node n)
	{
		VisitConstantOp(op, n);
	}

	public virtual void Visit(NullSentinelOp op, Node n)
	{
		VisitConstantOp(op, n);
	}

	public virtual void Visit(InternalConstantOp op, Node n)
	{
		VisitConstantOp(op, n);
	}

	public virtual void Visit(ConstantPredicateOp op, Node n)
	{
		VisitConstantOp(op, n);
	}

	public virtual void Visit(FunctionOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(PropertyOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(RelPropertyOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(CaseOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(ComparisonOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(LikeOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(AggregateOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(NewInstanceOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(NewEntityOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(DiscriminatedNewEntityOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(NewMultisetOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(NewRecordOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(RefOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(VarRefOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(ConditionalOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(ArithmeticOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(TreatOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(CastOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(SoftCastOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(IsOfOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(ExistsOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(ElementOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(GetEntityRefOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(GetRefKeyOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(CollectOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(DerefOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	public virtual void Visit(NavigateOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
	}

	protected virtual void VisitAncillaryOpDefault(AncillaryOp op, Node n)
	{
		VisitDefault(n);
	}

	public virtual void Visit(VarDefOp op, Node n)
	{
		VisitAncillaryOpDefault(op, n);
	}

	public virtual void Visit(VarDefListOp op, Node n)
	{
		VisitAncillaryOpDefault(op, n);
	}

	protected virtual void VisitRelOpDefault(RelOp op, Node n)
	{
		VisitDefault(n);
	}

	public virtual void Visit(ScanTableOp op, Node n)
	{
		VisitTableOp(op, n);
	}

	public virtual void Visit(ScanViewOp op, Node n)
	{
		VisitTableOp(op, n);
	}

	public virtual void Visit(UnnestOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	public virtual void Visit(ProjectOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	public virtual void Visit(FilterOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	public virtual void Visit(SortOp op, Node n)
	{
		VisitSortOp(op, n);
	}

	public virtual void Visit(ConstrainedSortOp op, Node n)
	{
		VisitSortOp(op, n);
	}

	public virtual void Visit(GroupByOp op, Node n)
	{
		VisitGroupByOp(op, n);
	}

	public virtual void Visit(GroupByIntoOp op, Node n)
	{
		VisitGroupByOp(op, n);
	}

	public virtual void Visit(CrossJoinOp op, Node n)
	{
		VisitJoinOp(op, n);
	}

	public virtual void Visit(InnerJoinOp op, Node n)
	{
		VisitJoinOp(op, n);
	}

	public virtual void Visit(LeftOuterJoinOp op, Node n)
	{
		VisitJoinOp(op, n);
	}

	public virtual void Visit(FullOuterJoinOp op, Node n)
	{
		VisitJoinOp(op, n);
	}

	public virtual void Visit(CrossApplyOp op, Node n)
	{
		VisitApplyOp(op, n);
	}

	public virtual void Visit(OuterApplyOp op, Node n)
	{
		VisitApplyOp(op, n);
	}

	public virtual void Visit(UnionAllOp op, Node n)
	{
		VisitSetOp(op, n);
	}

	public virtual void Visit(IntersectOp op, Node n)
	{
		VisitSetOp(op, n);
	}

	public virtual void Visit(ExceptOp op, Node n)
	{
		VisitSetOp(op, n);
	}

	public virtual void Visit(DistinctOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	public virtual void Visit(SingleRowOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	public virtual void Visit(SingleRowTableOp op, Node n)
	{
		VisitRelOpDefault(op, n);
	}

	protected virtual void VisitPhysicalOpDefault(PhysicalOp op, Node n)
	{
		VisitDefault(n);
	}

	public virtual void Visit(PhysicalProjectOp op, Node n)
	{
		VisitPhysicalOpDefault(op, n);
	}

	protected virtual void VisitNestOp(NestBaseOp op, Node n)
	{
		VisitPhysicalOpDefault(op, n);
	}

	public virtual void Visit(SingleStreamNestOp op, Node n)
	{
		VisitNestOp(op, n);
	}

	public virtual void Visit(MultiStreamNestOp op, Node n)
	{
		VisitNestOp(op, n);
	}
}

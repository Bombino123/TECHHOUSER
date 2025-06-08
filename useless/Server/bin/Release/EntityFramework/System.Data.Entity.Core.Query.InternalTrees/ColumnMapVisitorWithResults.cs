namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class ColumnMapVisitorWithResults<TResultType, TArgType>
{
	protected EntityIdentity VisitEntityIdentity(EntityIdentity entityIdentity, TArgType arg)
	{
		if (entityIdentity is DiscriminatedEntityIdentity entityIdentity2)
		{
			return VisitEntityIdentity(entityIdentity2, arg);
		}
		return VisitEntityIdentity((SimpleEntityIdentity)entityIdentity, arg);
	}

	protected virtual EntityIdentity VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, TArgType arg)
	{
		return entityIdentity;
	}

	protected virtual EntityIdentity VisitEntityIdentity(SimpleEntityIdentity entityIdentity, TArgType arg)
	{
		return entityIdentity;
	}

	internal abstract TResultType Visit(ComplexTypeColumnMap columnMap, TArgType arg);

	internal abstract TResultType Visit(DiscriminatedCollectionColumnMap columnMap, TArgType arg);

	internal abstract TResultType Visit(EntityColumnMap columnMap, TArgType arg);

	internal abstract TResultType Visit(SimplePolymorphicColumnMap columnMap, TArgType arg);

	internal abstract TResultType Visit(RecordColumnMap columnMap, TArgType arg);

	internal abstract TResultType Visit(RefColumnMap columnMap, TArgType arg);

	internal abstract TResultType Visit(ScalarColumnMap columnMap, TArgType arg);

	internal abstract TResultType Visit(SimpleCollectionColumnMap columnMap, TArgType arg);

	internal abstract TResultType Visit(VarRefColumnMap columnMap, TArgType arg);

	internal abstract TResultType Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, TArgType arg);
}

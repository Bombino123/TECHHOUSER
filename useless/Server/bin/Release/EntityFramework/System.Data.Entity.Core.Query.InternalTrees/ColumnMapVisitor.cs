namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class ColumnMapVisitor<TArgType>
{
	protected void VisitList<TListType>(TListType[] columnMaps, TArgType arg) where TListType : ColumnMap
	{
		for (int i = 0; i < columnMaps.Length; i++)
		{
			columnMaps[i].Accept(this, arg);
		}
	}

	protected void VisitEntityIdentity(EntityIdentity entityIdentity, TArgType arg)
	{
		if (entityIdentity is DiscriminatedEntityIdentity entityIdentity2)
		{
			VisitEntityIdentity(entityIdentity2, arg);
		}
		else
		{
			VisitEntityIdentity((SimpleEntityIdentity)entityIdentity, arg);
		}
	}

	protected virtual void VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, TArgType arg)
	{
		entityIdentity.EntitySetColumnMap.Accept(this, arg);
		SimpleColumnMap[] keys = entityIdentity.Keys;
		for (int i = 0; i < keys.Length; i++)
		{
			keys[i].Accept(this, arg);
		}
	}

	protected virtual void VisitEntityIdentity(SimpleEntityIdentity entityIdentity, TArgType arg)
	{
		SimpleColumnMap[] keys = entityIdentity.Keys;
		for (int i = 0; i < keys.Length; i++)
		{
			keys[i].Accept(this, arg);
		}
	}

	internal virtual void Visit(ComplexTypeColumnMap columnMap, TArgType arg)
	{
		columnMap.NullSentinel?.Accept(this, arg);
		ColumnMap[] properties = columnMap.Properties;
		for (int i = 0; i < properties.Length; i++)
		{
			properties[i].Accept(this, arg);
		}
	}

	internal virtual void Visit(DiscriminatedCollectionColumnMap columnMap, TArgType arg)
	{
		columnMap.Discriminator.Accept(this, arg);
		SimpleColumnMap[] foreignKeys = columnMap.ForeignKeys;
		for (int i = 0; i < foreignKeys.Length; i++)
		{
			foreignKeys[i].Accept(this, arg);
		}
		foreignKeys = columnMap.Keys;
		for (int i = 0; i < foreignKeys.Length; i++)
		{
			foreignKeys[i].Accept(this, arg);
		}
		columnMap.Element.Accept(this, arg);
	}

	internal virtual void Visit(EntityColumnMap columnMap, TArgType arg)
	{
		VisitEntityIdentity(columnMap.EntityIdentity, arg);
		ColumnMap[] properties = columnMap.Properties;
		for (int i = 0; i < properties.Length; i++)
		{
			properties[i].Accept(this, arg);
		}
	}

	internal virtual void Visit(SimplePolymorphicColumnMap columnMap, TArgType arg)
	{
		columnMap.TypeDiscriminator.Accept(this, arg);
		foreach (TypedColumnMap value in columnMap.TypeChoices.Values)
		{
			value.Accept(this, arg);
		}
		ColumnMap[] properties = columnMap.Properties;
		for (int i = 0; i < properties.Length; i++)
		{
			properties[i].Accept(this, arg);
		}
	}

	internal virtual void Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, TArgType arg)
	{
		SimpleColumnMap[] typeDiscriminators = columnMap.TypeDiscriminators;
		for (int i = 0; i < typeDiscriminators.Length; i++)
		{
			typeDiscriminators[i].Accept(this, arg);
		}
		foreach (TypedColumnMap value in columnMap.TypeChoices.Values)
		{
			value.Accept(this, arg);
		}
		ColumnMap[] properties = columnMap.Properties;
		for (int i = 0; i < properties.Length; i++)
		{
			properties[i].Accept(this, arg);
		}
	}

	internal virtual void Visit(RecordColumnMap columnMap, TArgType arg)
	{
		columnMap.NullSentinel?.Accept(this, arg);
		ColumnMap[] properties = columnMap.Properties;
		for (int i = 0; i < properties.Length; i++)
		{
			properties[i].Accept(this, arg);
		}
	}

	internal virtual void Visit(RefColumnMap columnMap, TArgType arg)
	{
		VisitEntityIdentity(columnMap.EntityIdentity, arg);
	}

	internal virtual void Visit(ScalarColumnMap columnMap, TArgType arg)
	{
	}

	internal virtual void Visit(SimpleCollectionColumnMap columnMap, TArgType arg)
	{
		SimpleColumnMap[] foreignKeys = columnMap.ForeignKeys;
		for (int i = 0; i < foreignKeys.Length; i++)
		{
			foreignKeys[i].Accept(this, arg);
		}
		foreignKeys = columnMap.Keys;
		for (int i = 0; i < foreignKeys.Length; i++)
		{
			foreignKeys[i].Accept(this, arg);
		}
		columnMap.Element.Accept(this, arg);
	}

	internal virtual void Visit(VarRefColumnMap columnMap, TArgType arg)
	{
	}
}

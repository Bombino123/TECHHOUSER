using System.Collections.Generic;
using System.Data.Entity.Core.Query.PlanCompiler;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class ColumnMapCopier : ColumnMapVisitorWithResults<ColumnMap, VarMap>
{
	private static readonly ColumnMapCopier _instance = new ColumnMapCopier();

	private ColumnMapCopier()
	{
	}

	internal static ColumnMap Copy(ColumnMap columnMap, VarMap replacementVarMap)
	{
		return columnMap.Accept(_instance, replacementVarMap);
	}

	private static Var GetReplacementVar(Var originalVar, VarMap replacementVarMap)
	{
		Var var = originalVar;
		while (replacementVarMap.TryGetValue(var, out originalVar) && originalVar != var)
		{
			var = originalVar;
		}
		return var;
	}

	internal TListType[] VisitList<TListType>(TListType[] tList, VarMap replacementVarMap) where TListType : ColumnMap
	{
		TListType[] array = new TListType[tList.Length];
		for (int i = 0; i < tList.Length; i++)
		{
			array[i] = (TListType)tList[i].Accept(this, replacementVarMap);
		}
		return array;
	}

	protected override EntityIdentity VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, VarMap replacementVarMap)
	{
		return new DiscriminatedEntityIdentity((SimpleColumnMap)entityIdentity.EntitySetColumnMap.Accept(this, replacementVarMap), keyColumns: VisitList(entityIdentity.Keys, replacementVarMap), entitySetMap: entityIdentity.EntitySetMap);
	}

	protected override EntityIdentity VisitEntityIdentity(SimpleEntityIdentity entityIdentity, VarMap replacementVarMap)
	{
		SimpleColumnMap[] keyColumns = VisitList(entityIdentity.Keys, replacementVarMap);
		return new SimpleEntityIdentity(entityIdentity.EntitySet, keyColumns);
	}

	internal override ColumnMap Visit(ComplexTypeColumnMap columnMap, VarMap replacementVarMap)
	{
		SimpleColumnMap simpleColumnMap = columnMap.NullSentinel;
		if (simpleColumnMap != null)
		{
			simpleColumnMap = (SimpleColumnMap)simpleColumnMap.Accept(this, replacementVarMap);
		}
		ColumnMap[] properties = VisitList(columnMap.Properties, replacementVarMap);
		return new ComplexTypeColumnMap(columnMap.Type, columnMap.Name, properties, simpleColumnMap);
	}

	internal override ColumnMap Visit(DiscriminatedCollectionColumnMap columnMap, VarMap replacementVarMap)
	{
		ColumnMap elementMap = columnMap.Element.Accept(this, replacementVarMap);
		SimpleColumnMap discriminator = (SimpleColumnMap)columnMap.Discriminator.Accept(this, replacementVarMap);
		SimpleColumnMap[] keys = VisitList(columnMap.Keys, replacementVarMap);
		SimpleColumnMap[] foreignKeys = VisitList(columnMap.ForeignKeys, replacementVarMap);
		return new DiscriminatedCollectionColumnMap(columnMap.Type, columnMap.Name, elementMap, keys, foreignKeys, discriminator, columnMap.DiscriminatorValue);
	}

	internal override ColumnMap Visit(EntityColumnMap columnMap, VarMap replacementVarMap)
	{
		EntityIdentity entityIdentity = VisitEntityIdentity(columnMap.EntityIdentity, replacementVarMap);
		ColumnMap[] properties = VisitList(columnMap.Properties, replacementVarMap);
		return new EntityColumnMap(columnMap.Type, columnMap.Name, properties, entityIdentity);
	}

	internal override ColumnMap Visit(SimplePolymorphicColumnMap columnMap, VarMap replacementVarMap)
	{
		SimpleColumnMap typeDiscriminator = (SimpleColumnMap)columnMap.TypeDiscriminator.Accept(this, replacementVarMap);
		Dictionary<object, TypedColumnMap> dictionary = new Dictionary<object, TypedColumnMap>(columnMap.TypeChoices.Comparer);
		foreach (KeyValuePair<object, TypedColumnMap> typeChoice in columnMap.TypeChoices)
		{
			TypedColumnMap value = (TypedColumnMap)typeChoice.Value.Accept(this, replacementVarMap);
			dictionary[typeChoice.Key] = value;
		}
		ColumnMap[] baseTypeColumns = VisitList(columnMap.Properties, replacementVarMap);
		return new SimplePolymorphicColumnMap(columnMap.Type, columnMap.Name, baseTypeColumns, typeDiscriminator, dictionary);
	}

	internal override ColumnMap Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, VarMap replacementVarMap)
	{
		System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(condition: false, "unexpected MultipleDiscriminatorPolymorphicColumnMap in ColumnMapCopier");
		return null;
	}

	internal override ColumnMap Visit(RecordColumnMap columnMap, VarMap replacementVarMap)
	{
		SimpleColumnMap simpleColumnMap = columnMap.NullSentinel;
		if (simpleColumnMap != null)
		{
			simpleColumnMap = (SimpleColumnMap)simpleColumnMap.Accept(this, replacementVarMap);
		}
		ColumnMap[] properties = VisitList(columnMap.Properties, replacementVarMap);
		return new RecordColumnMap(columnMap.Type, columnMap.Name, properties, simpleColumnMap);
	}

	internal override ColumnMap Visit(RefColumnMap columnMap, VarMap replacementVarMap)
	{
		EntityIdentity entityIdentity = VisitEntityIdentity(columnMap.EntityIdentity, replacementVarMap);
		return new RefColumnMap(columnMap.Type, columnMap.Name, entityIdentity);
	}

	internal override ColumnMap Visit(ScalarColumnMap columnMap, VarMap replacementVarMap)
	{
		return new ScalarColumnMap(columnMap.Type, columnMap.Name, columnMap.CommandId, columnMap.ColumnPos);
	}

	internal override ColumnMap Visit(SimpleCollectionColumnMap columnMap, VarMap replacementVarMap)
	{
		ColumnMap elementMap = columnMap.Element.Accept(this, replacementVarMap);
		SimpleColumnMap[] keys = VisitList(columnMap.Keys, replacementVarMap);
		SimpleColumnMap[] foreignKeys = VisitList(columnMap.ForeignKeys, replacementVarMap);
		return new SimpleCollectionColumnMap(columnMap.Type, columnMap.Name, elementMap, keys, foreignKeys);
	}

	internal override ColumnMap Visit(VarRefColumnMap columnMap, VarMap replacementVarMap)
	{
		Var replacementVar = GetReplacementVar(columnMap.Var, replacementVarMap);
		return new VarRefColumnMap(columnMap.Type, columnMap.Name, replacementVar);
	}
}

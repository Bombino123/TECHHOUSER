using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class ColumnMapTranslator : ColumnMapVisitorWithResults<ColumnMap, ColumnMapTranslatorTranslationDelegate>
{
	private static readonly ColumnMapTranslator _instance = new ColumnMapTranslator();

	private ColumnMapTranslator()
	{
	}

	private static Var GetReplacementVar(Var originalVar, IDictionary<Var, Var> replacementVarMap)
	{
		Var var = originalVar;
		while (replacementVarMap.TryGetValue(var, out originalVar) && originalVar != var)
		{
			var = originalVar;
		}
		return var;
	}

	internal static ColumnMap Translate(ColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		return columnMap.Accept(_instance, translationDelegate);
	}

	internal static ColumnMap Translate(ColumnMap columnMapToTranslate, Dictionary<Var, ColumnMap> varToColumnMap)
	{
		return Translate(columnMapToTranslate, delegate(ColumnMap columnMap)
		{
			if (columnMap is VarRefColumnMap varRefColumnMap)
			{
				if (varToColumnMap.TryGetValue(varRefColumnMap.Var, out columnMap))
				{
					if (!columnMap.IsNamed && varRefColumnMap.IsNamed)
					{
						columnMap.Name = varRefColumnMap.Name;
					}
					if (Helper.IsEnumType(varRefColumnMap.Type.EdmType) && varRefColumnMap.Type.EdmType != columnMap.Type.EdmType)
					{
						columnMap.Type = varRefColumnMap.Type;
					}
				}
				else
				{
					columnMap = varRefColumnMap;
				}
			}
			return columnMap;
		});
	}

	internal static ColumnMap Translate(ColumnMap columnMapToTranslate, IDictionary<Var, Var> varToVarMap)
	{
		return Translate(columnMapToTranslate, delegate(ColumnMap columnMap)
		{
			if (columnMap is VarRefColumnMap varRefColumnMap)
			{
				Var replacementVar = GetReplacementVar(varRefColumnMap.Var, varToVarMap);
				if (varRefColumnMap.Var != replacementVar)
				{
					columnMap = new VarRefColumnMap(varRefColumnMap.Type, varRefColumnMap.Name, replacementVar);
				}
			}
			return columnMap;
		});
	}

	internal static ColumnMap Translate(ColumnMap columnMapToTranslate, Dictionary<Var, KeyValuePair<int, int>> varToCommandColumnMap)
	{
		return Translate(columnMapToTranslate, delegate(ColumnMap columnMap)
		{
			if (columnMap is VarRefColumnMap varRefColumnMap)
			{
				if (!varToCommandColumnMap.TryGetValue(varRefColumnMap.Var, out var value))
				{
					throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UnknownVar, 1, varRefColumnMap.Var.Id);
				}
				columnMap = new ScalarColumnMap(varRefColumnMap.Type, varRefColumnMap.Name, value.Key, value.Value);
			}
			if (!columnMap.IsNamed)
			{
				columnMap.Name = "Value";
			}
			return columnMap;
		});
	}

	private void VisitList<TResultType>(TResultType[] tList, ColumnMapTranslatorTranslationDelegate translationDelegate) where TResultType : ColumnMap
	{
		for (int i = 0; i < tList.Length; i++)
		{
			tList[i] = (TResultType)tList[i].Accept(this, translationDelegate);
		}
	}

	protected override EntityIdentity VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		ColumnMap columnMap = entityIdentity.EntitySetColumnMap.Accept(this, translationDelegate);
		VisitList(entityIdentity.Keys, translationDelegate);
		if (columnMap != entityIdentity.EntitySetColumnMap)
		{
			entityIdentity = new DiscriminatedEntityIdentity((SimpleColumnMap)columnMap, entityIdentity.EntitySetMap, entityIdentity.Keys);
		}
		return entityIdentity;
	}

	protected override EntityIdentity VisitEntityIdentity(SimpleEntityIdentity entityIdentity, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		VisitList(entityIdentity.Keys, translationDelegate);
		return entityIdentity;
	}

	internal override ColumnMap Visit(ComplexTypeColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		SimpleColumnMap simpleColumnMap = columnMap.NullSentinel;
		if (simpleColumnMap != null)
		{
			simpleColumnMap = (SimpleColumnMap)translationDelegate(simpleColumnMap);
		}
		VisitList(columnMap.Properties, translationDelegate);
		if (columnMap.NullSentinel != simpleColumnMap)
		{
			columnMap = new ComplexTypeColumnMap(columnMap.Type, columnMap.Name, columnMap.Properties, simpleColumnMap);
		}
		return translationDelegate(columnMap);
	}

	internal override ColumnMap Visit(DiscriminatedCollectionColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		ColumnMap columnMap2 = columnMap.Discriminator.Accept(this, translationDelegate);
		VisitList(columnMap.ForeignKeys, translationDelegate);
		VisitList(columnMap.Keys, translationDelegate);
		ColumnMap columnMap3 = columnMap.Element.Accept(this, translationDelegate);
		if (columnMap2 != columnMap.Discriminator || columnMap3 != columnMap.Element)
		{
			columnMap = new DiscriminatedCollectionColumnMap(columnMap.Type, columnMap.Name, columnMap3, columnMap.Keys, columnMap.ForeignKeys, (SimpleColumnMap)columnMap2, columnMap.DiscriminatorValue);
		}
		return translationDelegate(columnMap);
	}

	internal override ColumnMap Visit(EntityColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		EntityIdentity entityIdentity = VisitEntityIdentity(columnMap.EntityIdentity, translationDelegate);
		VisitList(columnMap.Properties, translationDelegate);
		if (entityIdentity != columnMap.EntityIdentity)
		{
			columnMap = new EntityColumnMap(columnMap.Type, columnMap.Name, columnMap.Properties, entityIdentity);
		}
		return translationDelegate(columnMap);
	}

	internal override ColumnMap Visit(SimplePolymorphicColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		ColumnMap columnMap2 = columnMap.TypeDiscriminator.Accept(this, translationDelegate);
		Dictionary<object, TypedColumnMap> dictionary = columnMap.TypeChoices;
		foreach (KeyValuePair<object, TypedColumnMap> typeChoice in columnMap.TypeChoices)
		{
			TypedColumnMap typedColumnMap = (TypedColumnMap)typeChoice.Value.Accept(this, translationDelegate);
			if (typedColumnMap != typeChoice.Value)
			{
				if (dictionary == columnMap.TypeChoices)
				{
					dictionary = new Dictionary<object, TypedColumnMap>(columnMap.TypeChoices);
				}
				dictionary[typeChoice.Key] = typedColumnMap;
			}
		}
		VisitList(columnMap.Properties, translationDelegate);
		if (columnMap2 != columnMap.TypeDiscriminator || dictionary != columnMap.TypeChoices)
		{
			columnMap = new SimplePolymorphicColumnMap(columnMap.Type, columnMap.Name, columnMap.Properties, (SimpleColumnMap)columnMap2, dictionary);
		}
		return translationDelegate(columnMap);
	}

	internal override ColumnMap Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		PlanCompiler.Assert(condition: false, "unexpected MultipleDiscriminatorPolymorphicColumnMap in ColumnMapTranslator");
		return null;
	}

	internal override ColumnMap Visit(RecordColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		SimpleColumnMap simpleColumnMap = columnMap.NullSentinel;
		if (simpleColumnMap != null)
		{
			simpleColumnMap = (SimpleColumnMap)translationDelegate(simpleColumnMap);
		}
		VisitList(columnMap.Properties, translationDelegate);
		if (columnMap.NullSentinel != simpleColumnMap)
		{
			columnMap = new RecordColumnMap(columnMap.Type, columnMap.Name, columnMap.Properties, simpleColumnMap);
		}
		return translationDelegate(columnMap);
	}

	internal override ColumnMap Visit(RefColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		EntityIdentity entityIdentity = VisitEntityIdentity(columnMap.EntityIdentity, translationDelegate);
		if (entityIdentity != columnMap.EntityIdentity)
		{
			columnMap = new RefColumnMap(columnMap.Type, columnMap.Name, entityIdentity);
		}
		return translationDelegate(columnMap);
	}

	internal override ColumnMap Visit(ScalarColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		return translationDelegate(columnMap);
	}

	internal override ColumnMap Visit(SimpleCollectionColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		VisitList(columnMap.ForeignKeys, translationDelegate);
		VisitList(columnMap.Keys, translationDelegate);
		ColumnMap columnMap2 = columnMap.Element.Accept(this, translationDelegate);
		if (columnMap2 != columnMap.Element)
		{
			columnMap = new SimpleCollectionColumnMap(columnMap.Type, columnMap.Name, columnMap2, columnMap.Keys, columnMap.ForeignKeys);
		}
		return translationDelegate(columnMap);
	}

	internal override ColumnMap Visit(VarRefColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
	{
		return translationDelegate(columnMap);
	}
}

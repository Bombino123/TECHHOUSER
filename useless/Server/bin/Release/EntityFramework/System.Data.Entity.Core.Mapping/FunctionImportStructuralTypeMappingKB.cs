using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

internal sealed class FunctionImportStructuralTypeMappingKB
{
	private readonly ItemCollection m_itemCollection;

	private readonly KeyToListMap<EntityType, LineInfo> m_entityTypeLineInfos;

	private readonly KeyToListMap<EntityType, LineInfo> m_isTypeOfLineInfos;

	internal readonly ReadOnlyCollection<EntityType> MappedEntityTypes;

	internal readonly ReadOnlyCollection<string> DiscriminatorColumns;

	internal readonly ReadOnlyCollection<FunctionImportNormalizedEntityTypeMapping> NormalizedEntityTypeMappings;

	internal readonly Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> ReturnTypeColumnsRenameMapping;

	internal FunctionImportStructuralTypeMappingKB(IEnumerable<FunctionImportStructuralTypeMapping> structuralTypeMappings, ItemCollection itemCollection)
	{
		m_itemCollection = itemCollection;
		if (structuralTypeMappings.Count() == 0)
		{
			ReturnTypeColumnsRenameMapping = new Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping>();
			NormalizedEntityTypeMappings = new ReadOnlyCollection<FunctionImportNormalizedEntityTypeMapping>(new List<FunctionImportNormalizedEntityTypeMapping>());
			DiscriminatorColumns = new ReadOnlyCollection<string>(new List<string>());
			MappedEntityTypes = new ReadOnlyCollection<EntityType>(new List<EntityType>());
			return;
		}
		IEnumerable<FunctionImportEntityTypeMapping> enumerable = structuralTypeMappings.OfType<FunctionImportEntityTypeMapping>();
		if (enumerable != null && enumerable.FirstOrDefault() != null)
		{
			Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> dictionary = new Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>>();
			Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> dictionary2 = new Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>>();
			List<FunctionImportNormalizedEntityTypeMapping> list = new List<FunctionImportNormalizedEntityTypeMapping>();
			MappedEntityTypes = new ReadOnlyCollection<EntityType>(enumerable.SelectMany((FunctionImportEntityTypeMapping mapping) => mapping.GetMappedEntityTypes(m_itemCollection)).Distinct().ToList());
			DiscriminatorColumns = new ReadOnlyCollection<string>(enumerable.SelectMany((FunctionImportEntityTypeMapping mapping) => mapping.GetDiscriminatorColumns()).Distinct().ToList());
			m_entityTypeLineInfos = new KeyToListMap<EntityType, LineInfo>(EqualityComparer<EntityType>.Default);
			m_isTypeOfLineInfos = new KeyToListMap<EntityType, LineInfo>(EqualityComparer<EntityType>.Default);
			foreach (FunctionImportEntityTypeMapping item in enumerable)
			{
				foreach (EntityType entityType in item.EntityTypes)
				{
					m_entityTypeLineInfos.Add(entityType, item.LineInfo);
				}
				foreach (EntityType isOfTypeEntityType in item.IsOfTypeEntityTypes)
				{
					m_isTypeOfLineInfos.Add(isOfTypeEntityType, item.LineInfo);
				}
				Dictionary<string, FunctionImportEntityTypeMappingCondition> dictionary3 = item.Conditions.ToDictionary((FunctionImportEntityTypeMappingCondition condition) => condition.ColumnName, (FunctionImportEntityTypeMappingCondition condition) => condition);
				List<FunctionImportEntityTypeMappingCondition> list2 = new List<FunctionImportEntityTypeMappingCondition>(DiscriminatorColumns.Count);
				for (int i = 0; i < DiscriminatorColumns.Count; i++)
				{
					string key = DiscriminatorColumns[i];
					if (dictionary3.TryGetValue(key, out var value))
					{
						list2.Add(value);
					}
					else
					{
						list2.Add(null);
					}
				}
				bool[] array = new bool[MappedEntityTypes.Count];
				Set<EntityType> set = new Set<EntityType>(item.GetMappedEntityTypes(m_itemCollection));
				for (int j = 0; j < MappedEntityTypes.Count; j++)
				{
					array[j] = set.Contains(MappedEntityTypes[j]);
				}
				list.Add(new FunctionImportNormalizedEntityTypeMapping(this, list2, new BitArray(array)));
				foreach (EntityType isOfTypeEntityType2 in item.IsOfTypeEntityTypes)
				{
					if (!dictionary.Keys.Contains(isOfTypeEntityType2))
					{
						dictionary.Add(isOfTypeEntityType2, new Collection<FunctionImportReturnTypePropertyMapping>());
					}
					foreach (FunctionImportReturnTypePropertyMapping columnsRename in item.ColumnsRenameList)
					{
						dictionary[isOfTypeEntityType2].Add(columnsRename);
					}
				}
				foreach (EntityType entityType2 in item.EntityTypes)
				{
					if (!dictionary2.Keys.Contains(entityType2))
					{
						dictionary2.Add(entityType2, new Collection<FunctionImportReturnTypePropertyMapping>());
					}
					foreach (FunctionImportReturnTypePropertyMapping columnsRename2 in item.ColumnsRenameList)
					{
						dictionary2[entityType2].Add(columnsRename2);
					}
				}
			}
			ReturnTypeColumnsRenameMapping = new FunctionImportReturnTypeEntityTypeColumnsRenameBuilder(dictionary, dictionary2).ColumnRenameMapping;
			NormalizedEntityTypeMappings = new ReadOnlyCollection<FunctionImportNormalizedEntityTypeMapping>(list);
			return;
		}
		IEnumerable<FunctionImportComplexTypeMapping> source = structuralTypeMappings.Cast<FunctionImportComplexTypeMapping>();
		ReturnTypeColumnsRenameMapping = new Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping>();
		foreach (FunctionImportReturnTypePropertyMapping columnsRename3 in source.First().ColumnsRenameList)
		{
			FunctionImportReturnTypeStructuralTypeColumnRenameMapping functionImportReturnTypeStructuralTypeColumnRenameMapping = new FunctionImportReturnTypeStructuralTypeColumnRenameMapping(columnsRename3.CMember);
			functionImportReturnTypeStructuralTypeColumnRenameMapping.AddRename(new FunctionImportReturnTypeStructuralTypeColumn(columnsRename3.SColumn, source.First().ReturnType, isTypeOf: false, columnsRename3.LineInfo));
			ReturnTypeColumnsRenameMapping.Add(columnsRename3.CMember, functionImportReturnTypeStructuralTypeColumnRenameMapping);
		}
		NormalizedEntityTypeMappings = new ReadOnlyCollection<FunctionImportNormalizedEntityTypeMapping>(new List<FunctionImportNormalizedEntityTypeMapping>());
		DiscriminatorColumns = new ReadOnlyCollection<string>(new List<string>());
		MappedEntityTypes = new ReadOnlyCollection<EntityType>(new List<EntityType>());
	}

	internal bool ValidateTypeConditions(bool validateAmbiguity, IList<EdmSchemaError> errors, string sourceLocation)
	{
		GetUnreachableTypes(validateAmbiguity, out var unreachableEntityTypes, out var unreachableIsTypeOfs);
		bool result = true;
		foreach (KeyValuePair<EntityType, List<LineInfo>> keyValuePair in unreachableEntityTypes.KeyValuePairs)
		{
			LineInfo lineInfo = keyValuePair.Value.First();
			string p = StringUtil.ToCommaSeparatedString(keyValuePair.Value.Select((LineInfo li) => li.LineNumber));
			EdmSchemaError item = new EdmSchemaError(Strings.Mapping_FunctionImport_UnreachableType(keyValuePair.Key.FullName, p), 2076, EdmSchemaErrorSeverity.Error, sourceLocation, lineInfo.LineNumber, lineInfo.LinePosition);
			errors.Add(item);
			result = false;
		}
		foreach (KeyValuePair<EntityType, List<LineInfo>> keyValuePair2 in unreachableIsTypeOfs.KeyValuePairs)
		{
			LineInfo lineInfo2 = keyValuePair2.Value.First();
			string p2 = StringUtil.ToCommaSeparatedString(keyValuePair2.Value.Select((LineInfo li) => li.LineNumber));
			EdmSchemaError item2 = new EdmSchemaError(Strings.Mapping_FunctionImport_UnreachableIsTypeOf("IsTypeOf(" + keyValuePair2.Key.FullName + ")", p2), 2076, EdmSchemaErrorSeverity.Error, sourceLocation, lineInfo2.LineNumber, lineInfo2.LinePosition);
			errors.Add(item2);
			result = false;
		}
		return result;
	}

	private void GetUnreachableTypes(bool validateAmbiguity, out KeyToListMap<EntityType, LineInfo> unreachableEntityTypes, out KeyToListMap<EntityType, LineInfo> unreachableIsTypeOfs)
	{
		DomainVariable<string, ValueCondition>[] variables = ConstructDomainVariables();
		DomainConstraintConversionContext<string, ValueCondition> converter = new DomainConstraintConversionContext<string, ValueCondition>();
		Vertex[] mappingConditions = ConvertMappingConditionsToVertices(converter, variables);
		Set<EntityType> reachableTypes = (validateAmbiguity ? FindUnambiguouslyReachableTypes(converter, mappingConditions) : FindReachableTypes(converter, mappingConditions));
		CollectUnreachableTypes(reachableTypes, out unreachableEntityTypes, out unreachableIsTypeOfs);
	}

	private DomainVariable<string, ValueCondition>[] ConstructDomainVariables()
	{
		Set<ValueCondition>[] array = new Set<ValueCondition>[DiscriminatorColumns.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Set<ValueCondition>();
			array[i].Add(ValueCondition.IsOther);
			array[i].Add(ValueCondition.IsNull);
		}
		foreach (FunctionImportNormalizedEntityTypeMapping normalizedEntityTypeMapping in NormalizedEntityTypeMappings)
		{
			for (int j = 0; j < DiscriminatorColumns.Count; j++)
			{
				FunctionImportEntityTypeMappingCondition functionImportEntityTypeMappingCondition = normalizedEntityTypeMapping.ColumnConditions[j];
				if (functionImportEntityTypeMappingCondition != null && !functionImportEntityTypeMappingCondition.ConditionValue.IsNotNullCondition)
				{
					array[j].Add(functionImportEntityTypeMappingCondition.ConditionValue);
				}
			}
		}
		DomainVariable<string, ValueCondition>[] array2 = new DomainVariable<string, ValueCondition>[array.Length];
		for (int k = 0; k < array2.Length; k++)
		{
			array2[k] = new DomainVariable<string, ValueCondition>(DiscriminatorColumns[k], array[k].MakeReadOnly());
		}
		return array2;
	}

	private Vertex[] ConvertMappingConditionsToVertices(ConversionContext<DomainConstraint<string, ValueCondition>> converter, DomainVariable<string, ValueCondition>[] variables)
	{
		Vertex[] array = new Vertex[NormalizedEntityTypeMappings.Count];
		for (int i = 0; i < array.Length; i++)
		{
			FunctionImportNormalizedEntityTypeMapping functionImportNormalizedEntityTypeMapping = NormalizedEntityTypeMappings[i];
			Vertex vertex = Vertex.One;
			for (int j = 0; j < DiscriminatorColumns.Count; j++)
			{
				FunctionImportEntityTypeMappingCondition functionImportEntityTypeMappingCondition = functionImportNormalizedEntityTypeMapping.ColumnConditions[j];
				if (functionImportEntityTypeMappingCondition != null)
				{
					ValueCondition conditionValue = functionImportEntityTypeMappingCondition.ConditionValue;
					if (conditionValue.IsNotNullCondition)
					{
						TermExpr<DomainConstraint<string, ValueCondition>> term = new TermExpr<DomainConstraint<string, ValueCondition>>(new DomainConstraint<string, ValueCondition>(variables[j], ValueCondition.IsNull));
						Vertex vertex2 = converter.TranslateTermToVertex(term);
						vertex = converter.Solver.And(vertex, converter.Solver.Not(vertex2));
					}
					else
					{
						TermExpr<DomainConstraint<string, ValueCondition>> term2 = new TermExpr<DomainConstraint<string, ValueCondition>>(new DomainConstraint<string, ValueCondition>(variables[j], conditionValue));
						vertex = converter.Solver.And(vertex, converter.TranslateTermToVertex(term2));
					}
				}
			}
			array[i] = vertex;
		}
		return array;
	}

	private Set<EntityType> FindReachableTypes(DomainConstraintConversionContext<string, ValueCondition> converter, Vertex[] mappingConditions)
	{
		Vertex[] array = new Vertex[MappedEntityTypes.Count];
		for (int j = 0; j < array.Length; j++)
		{
			Vertex vertex = Vertex.One;
			for (int k = 0; k < NormalizedEntityTypeMappings.Count; k++)
			{
				vertex = ((!NormalizedEntityTypeMappings[k].ImpliedEntityTypes[j]) ? converter.Solver.And(vertex, converter.Solver.Not(mappingConditions[k])) : converter.Solver.And(vertex, mappingConditions[k]));
			}
			array[j] = vertex;
		}
		Set<EntityType> set = new Set<EntityType>();
		int i;
		for (i = 0; i < array.Length; i++)
		{
			if (!converter.Solver.And(array.Select((Vertex typeCondition, int ordinal) => (ordinal != i) ? converter.Solver.Not(typeCondition) : typeCondition)).IsZero())
			{
				set.Add(MappedEntityTypes[i]);
			}
		}
		return set;
	}

	private Set<EntityType> FindUnambiguouslyReachableTypes(DomainConstraintConversionContext<string, ValueCondition> converter, Vertex[] mappingConditions)
	{
		Vertex[] array = new Vertex[MappedEntityTypes.Count];
		for (int i = 0; i < array.Length; i++)
		{
			Vertex vertex = Vertex.One;
			for (int j = 0; j < NormalizedEntityTypeMappings.Count; j++)
			{
				if (NormalizedEntityTypeMappings[j].ImpliedEntityTypes[i])
				{
					vertex = converter.Solver.And(vertex, mappingConditions[j]);
				}
			}
			array[i] = vertex;
		}
		BitArray bitArray = new BitArray(array.Length, defaultValue: true);
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k].IsZero())
			{
				bitArray[k] = false;
				continue;
			}
			for (int l = k + 1; l < array.Length; l++)
			{
				if (!converter.Solver.And(array[k], array[l]).IsZero())
				{
					bitArray[k] = false;
					bitArray[l] = false;
				}
			}
		}
		Set<EntityType> set = new Set<EntityType>();
		for (int m = 0; m < array.Length; m++)
		{
			if (bitArray[m])
			{
				set.Add(MappedEntityTypes[m]);
			}
		}
		return set;
	}

	private void CollectUnreachableTypes(Set<EntityType> reachableTypes, out KeyToListMap<EntityType, LineInfo> entityTypes, out KeyToListMap<EntityType, LineInfo> isTypeOfEntityTypes)
	{
		entityTypes = new KeyToListMap<EntityType, LineInfo>(EqualityComparer<EntityType>.Default);
		isTypeOfEntityTypes = new KeyToListMap<EntityType, LineInfo>(EqualityComparer<EntityType>.Default);
		if (reachableTypes.Count == MappedEntityTypes.Count)
		{
			return;
		}
		foreach (EntityType key in m_isTypeOfLineInfos.Keys)
		{
			if (!MetadataHelper.GetTypeAndSubtypesOf(key, m_itemCollection, includeAbstractTypes: false).Cast<EntityType>().Intersect(reachableTypes)
				.Any())
			{
				isTypeOfEntityTypes.AddRange(key, m_isTypeOfLineInfos.EnumerateValues(key));
			}
		}
		foreach (EntityType key2 in m_entityTypeLineInfos.Keys)
		{
			if (!reachableTypes.Contains(key2))
			{
				entityTypes.AddRange(key2, m_entityTypeLineInfos.EnumerateValues(key2));
			}
		}
	}
}

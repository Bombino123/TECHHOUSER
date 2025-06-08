using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public sealed class FunctionImportMappingNonComposable : FunctionImportMapping
{
	private readonly ReadOnlyCollection<FunctionImportResultMapping> _resultMappings;

	private readonly bool noExplicitResultMappings;

	private readonly ReadOnlyCollection<FunctionImportStructuralTypeMappingKB> _internalResultMappings;

	internal ReadOnlyCollection<FunctionImportStructuralTypeMappingKB> InternalResultMappings => _internalResultMappings;

	public ReadOnlyCollection<FunctionImportResultMapping> ResultMappings => _resultMappings;

	public FunctionImportMappingNonComposable(EdmFunction functionImport, EdmFunction targetFunction, IEnumerable<FunctionImportResultMapping> resultMappings, EntityContainerMapping containerMapping)
		: base(Check.NotNull(functionImport, "functionImport"), Check.NotNull(targetFunction, "targetFunction"))
	{
		Check.NotNull(resultMappings, "resultMappings");
		Check.NotNull(containerMapping, "containerMapping");
		if (!resultMappings.Any())
		{
			EdmItemCollection itemCollection = ((containerMapping.StorageMappingItemCollection != null) ? containerMapping.StorageMappingItemCollection.EdmItemCollection : new EdmItemCollection(new EdmModel(DataSpace.CSpace)));
			_internalResultMappings = new ReadOnlyCollection<FunctionImportStructuralTypeMappingKB>(new FunctionImportStructuralTypeMappingKB[1]
			{
				new FunctionImportStructuralTypeMappingKB(new List<FunctionImportStructuralTypeMapping>(), itemCollection)
			});
			noExplicitResultMappings = true;
		}
		else
		{
			_internalResultMappings = new ReadOnlyCollection<FunctionImportStructuralTypeMappingKB>(resultMappings.Select((FunctionImportResultMapping resultMapping) => new FunctionImportStructuralTypeMappingKB(resultMapping.TypeMappings, containerMapping.StorageMappingItemCollection.EdmItemCollection)).ToArray());
			noExplicitResultMappings = false;
		}
		_resultMappings = new ReadOnlyCollection<FunctionImportResultMapping>(resultMappings.ToList());
	}

	internal FunctionImportMappingNonComposable(EdmFunction functionImport, EdmFunction targetFunction, List<List<FunctionImportStructuralTypeMapping>> structuralTypeMappingsList, ItemCollection itemCollection)
		: base(functionImport, targetFunction)
	{
		if (structuralTypeMappingsList.Count == 0)
		{
			_internalResultMappings = new ReadOnlyCollection<FunctionImportStructuralTypeMappingKB>(new FunctionImportStructuralTypeMappingKB[1]
			{
				new FunctionImportStructuralTypeMappingKB(new List<FunctionImportStructuralTypeMapping>(), itemCollection)
			});
			noExplicitResultMappings = true;
		}
		else
		{
			_internalResultMappings = new ReadOnlyCollection<FunctionImportStructuralTypeMappingKB>(structuralTypeMappingsList.Select((List<FunctionImportStructuralTypeMapping> structuralTypeMappings) => new FunctionImportStructuralTypeMappingKB(structuralTypeMappings, itemCollection)).ToArray());
			noExplicitResultMappings = false;
		}
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(_resultMappings);
		base.SetReadOnly();
	}

	internal FunctionImportStructuralTypeMappingKB GetResultMapping(int resultSetIndex)
	{
		if (noExplicitResultMappings)
		{
			return InternalResultMappings[0];
		}
		if (InternalResultMappings.Count <= resultSetIndex)
		{
			throw new ArgumentOutOfRangeException("resultSetIndex");
		}
		return InternalResultMappings[resultSetIndex];
	}

	internal IList<string> GetDiscriminatorColumns(int resultSetIndex)
	{
		return GetResultMapping(resultSetIndex).DiscriminatorColumns;
	}

	internal EntityType Discriminate(object[] discriminatorValues, int resultSetIndex)
	{
		FunctionImportStructuralTypeMappingKB resultMapping = GetResultMapping(resultSetIndex);
		BitArray bitArray = new BitArray(resultMapping.MappedEntityTypes.Count, defaultValue: true);
		foreach (FunctionImportNormalizedEntityTypeMapping normalizedEntityTypeMapping in resultMapping.NormalizedEntityTypeMappings)
		{
			bool flag = true;
			ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> columnConditions = normalizedEntityTypeMapping.ColumnConditions;
			for (int i = 0; i < columnConditions.Count; i++)
			{
				if (columnConditions[i] != null && !columnConditions[i].ColumnValueMatchesCondition(discriminatorValues[i]))
				{
					flag = false;
					break;
				}
			}
			bitArray = ((!flag) ? bitArray.And(normalizedEntityTypeMapping.ComplementImpliedEntityTypes) : bitArray.And(normalizedEntityTypeMapping.ImpliedEntityTypes));
		}
		EntityType entityType = null;
		for (int j = 0; j < bitArray.Length; j++)
		{
			if (bitArray[j])
			{
				if (entityType != null)
				{
					throw new EntityCommandExecutionException(Strings.ADP_InvalidDataReaderUnableToDetermineType);
				}
				entityType = resultMapping.MappedEntityTypes[j];
			}
		}
		if (entityType == null)
		{
			throw new EntityCommandExecutionException(Strings.ADP_InvalidDataReaderUnableToDetermineType);
		}
		return entityType;
	}

	internal TypeUsage GetExpectedTargetResultType(int resultSetIndex)
	{
		FunctionImportStructuralTypeMappingKB resultMapping = GetResultMapping(resultSetIndex);
		Dictionary<string, TypeUsage> dictionary = new Dictionary<string, TypeUsage>();
		IEnumerable<StructuralType> enumerable;
		if (resultMapping.NormalizedEntityTypeMappings.Count == 0)
		{
			MetadataHelper.TryGetFunctionImportReturnType<StructuralType>(base.FunctionImport, resultSetIndex, out var returnType);
			enumerable = new StructuralType[1] { returnType };
		}
		else
		{
			enumerable = resultMapping.MappedEntityTypes.Cast<StructuralType>();
		}
		foreach (StructuralType item in enumerable)
		{
			foreach (EdmProperty allStructuralMember in TypeHelpers.GetAllStructuralMembers(item))
			{
				dictionary[allStructuralMember.Name] = allStructuralMember.TypeUsage;
			}
		}
		foreach (string discriminatorColumn in GetDiscriminatorColumns(resultSetIndex))
		{
			if (!dictionary.ContainsKey(discriminatorColumn))
			{
				TypeUsage value = TypeUsage.CreateStringTypeUsage(MetadataWorkspace.GetModelPrimitiveType(PrimitiveTypeKind.String), isUnicode: true, isFixedLength: false);
				dictionary.Add(discriminatorColumn, value);
			}
		}
		return TypeUsage.Create(new CollectionType(TypeUsage.Create(new RowType(dictionary.Select((KeyValuePair<string, TypeUsage> c) => new EdmProperty(c.Key, c.Value))))));
	}
}

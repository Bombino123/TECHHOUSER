using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class ColumnMapFactory
{
	internal virtual CollectionColumnMap CreateFunctionImportStructuralTypeColumnMap(DbDataReader storeDataReader, FunctionImportMappingNonComposable mapping, int resultSetIndex, EntitySet entitySet, StructuralType baseStructuralType)
	{
		FunctionImportStructuralTypeMappingKB resultMapping = mapping.GetResultMapping(resultSetIndex);
		if (resultMapping.NormalizedEntityTypeMappings.Count == 0)
		{
			return CreateColumnMapFromReaderAndType(storeDataReader, baseStructuralType, entitySet, resultMapping.ReturnTypeColumnsRenameMapping);
		}
		EntityType item = baseStructuralType as EntityType;
		ScalarColumnMap[] array = CreateDiscriminatorColumnMaps(storeDataReader, mapping, resultSetIndex);
		HashSet<EntityType> obj = new HashSet<EntityType>(resultMapping.MappedEntityTypes) { item };
		Dictionary<EntityType, TypedColumnMap> dictionary = new Dictionary<EntityType, TypedColumnMap>(obj.Count);
		ColumnMap[] array2 = null;
		foreach (EntityType item2 in obj)
		{
			ColumnMap[] columnMapsForType = GetColumnMapsForType(storeDataReader, item2, resultMapping.ReturnTypeColumnsRenameMapping);
			EntityColumnMap value = CreateEntityTypeElementColumnMap(storeDataReader, item2, entitySet, columnMapsForType, resultMapping.ReturnTypeColumnsRenameMapping);
			if (!item2.Abstract)
			{
				dictionary.Add(item2, value);
			}
			if (item2 == baseStructuralType)
			{
				array2 = columnMapsForType;
			}
		}
		TypeUsage type = TypeUsage.Create(baseStructuralType);
		string name = baseStructuralType.Name;
		ColumnMap[] baseTypeColumns = array2;
		SimpleColumnMap[] typeDiscriminators = array;
		MultipleDiscriminatorPolymorphicColumnMap elementMap = new MultipleDiscriminatorPolymorphicColumnMap(type, name, baseTypeColumns, typeDiscriminators, dictionary, (object[] discriminatorValues) => mapping.Discriminate(discriminatorValues, resultSetIndex));
		return new SimpleCollectionColumnMap(baseStructuralType.GetCollectionType().TypeUsage, baseStructuralType.Name, elementMap, null, null);
	}

	internal virtual CollectionColumnMap CreateColumnMapFromReaderAndType(DbDataReader storeDataReader, EdmType edmType, EntitySet entitySet, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
	{
		ColumnMap[] columnMapsForType = GetColumnMapsForType(storeDataReader, edmType, renameList);
		ColumnMap elementMap = null;
		if (Helper.IsRowType(edmType))
		{
			elementMap = new RecordColumnMap(TypeUsage.Create(edmType), edmType.Name, columnMapsForType, null);
		}
		else if (Helper.IsComplexType(edmType))
		{
			elementMap = new ComplexTypeColumnMap(TypeUsage.Create(edmType), edmType.Name, columnMapsForType, null);
		}
		else if (Helper.IsScalarType(edmType))
		{
			if (storeDataReader.FieldCount != 1)
			{
				throw new EntityCommandExecutionException(Strings.ADP_InvalidDataReaderFieldCountForScalarType);
			}
			elementMap = new ScalarColumnMap(TypeUsage.Create(edmType), edmType.Name, 0, 0);
		}
		else if (Helper.IsEntityType(edmType))
		{
			elementMap = CreateEntityTypeElementColumnMap(storeDataReader, edmType, entitySet, columnMapsForType, null);
		}
		return new SimpleCollectionColumnMap(edmType.GetCollectionType().TypeUsage, edmType.Name, elementMap, null, null);
	}

	internal virtual CollectionColumnMap CreateColumnMapFromReaderAndClrType(DbDataReader reader, Type type, MetadataWorkspace workspace)
	{
		ConstructorInfo declaredConstructor = type.GetDeclaredConstructor();
		if (type.IsAbstract() || (null == declaredConstructor && !type.IsValueType()))
		{
			throw new InvalidOperationException(Strings.ObjectContext_InvalidTypeForStoreQuery(type));
		}
		List<Tuple<MemberAssignment, int, EdmProperty>> list = new List<Tuple<MemberAssignment, int, EdmProperty>>();
		foreach (PropertyInfo item4 in from p in type.GetInstanceProperties()
			select p.GetPropertyInfoForSet())
		{
			Type type2 = Nullable.GetUnderlyingType(item4.PropertyType) ?? item4.PropertyType;
			Type type3 = (type2.IsEnum() ? type2.GetEnumUnderlyingType() : item4.PropertyType);
			if (TryGetColumnOrdinalFromReader(reader, item4.Name, out var ordinal) && workspace.TryDetermineCSpaceModelType(type3, out var modelEdmType) && Helper.IsScalarType(modelEdmType) && item4.CanWriteExtended() && item4.GetIndexParameters().Length == 0 && null != item4.Setter())
			{
				list.Add(Tuple.Create(Expression.Bind(item4, Expression.Parameter(item4.PropertyType, "placeholder")), ordinal, new EdmProperty(item4.Name, TypeUsage.Create(modelEdmType))));
			}
		}
		MemberInfo[] array = new MemberInfo[list.Count];
		MemberBinding[] array2 = new MemberBinding[list.Count];
		ColumnMap[] array3 = new ColumnMap[list.Count];
		EdmProperty[] array4 = new EdmProperty[list.Count];
		int num = 0;
		foreach (IGrouping<int, Tuple<MemberAssignment, int, EdmProperty>> item5 in from tuple in list
			group tuple by tuple.Item2 into tuple
			orderby tuple.Key
			select tuple)
		{
			if (item5.Count() != 1)
			{
				throw new InvalidOperationException(Strings.ObjectContext_TwoPropertiesMappedToSameColumn(reader.GetName(item5.Key), string.Join(", ", item5.Select((Tuple<MemberAssignment, int, EdmProperty> tuple) => tuple.Item3.Name).ToArray())));
			}
			Tuple<MemberAssignment, int, EdmProperty> tuple2 = item5.Single();
			MemberAssignment item = tuple2.Item1;
			int item2 = tuple2.Item2;
			EdmProperty item3 = tuple2.Item3;
			array[num] = item.Member;
			array2[num] = item;
			array3[num] = new ScalarColumnMap(item3.TypeUsage, item3.Name, 0, item2);
			array4[num] = item3;
			num++;
		}
		MemberInitExpression initExpression = Expression.MemberInit((null == declaredConstructor) ? Expression.New(type) : Expression.New(declaredConstructor), array2);
		InitializerMetadata initializerMetadata = InitializerMetadata.CreateProjectionInitializer((EdmItemCollection)workspace.GetItemCollection(DataSpace.CSpace), initExpression);
		RowType rowType = new RowType(array4, initializerMetadata);
		RecordColumnMap elementMap = new RecordColumnMap(TypeUsage.Create(rowType), "DefaultTypeProjection", array3, null);
		return new SimpleCollectionColumnMap(rowType.GetCollectionType().TypeUsage, rowType.Name, elementMap, null, null);
	}

	private static EntityColumnMap CreateEntityTypeElementColumnMap(DbDataReader storeDataReader, EdmType edmType, EntitySet entitySet, ColumnMap[] propertyColumnMaps, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
	{
		EntityType entityType = (EntityType)edmType;
		ColumnMap[] array = new ColumnMap[storeDataReader.FieldCount];
		foreach (ColumnMap columnMap in propertyColumnMaps)
		{
			int columnPos = ((ScalarColumnMap)columnMap).ColumnPos;
			array[columnPos] = columnMap;
		}
		ReadOnlyMetadataCollection<EdmMember> keyMembers = entityType.KeyMembers;
		SimpleColumnMap[] array2 = new SimpleColumnMap[((ICollection<EdmMember>)keyMembers).Count];
		int num = 0;
		foreach (EdmMember item in (IEnumerable<EdmMember>)keyMembers)
		{
			int memberOrdinalFromReader = GetMemberOrdinalFromReader(storeDataReader, item, edmType, renameList);
			ColumnMap columnMap2 = array[memberOrdinalFromReader];
			array2[num] = (SimpleColumnMap)columnMap2;
			num++;
		}
		SimpleEntityIdentity entityIdentity = new SimpleEntityIdentity(entitySet, array2);
		return new EntityColumnMap(TypeUsage.Create(edmType), edmType.Name, propertyColumnMaps, entityIdentity);
	}

	private static ColumnMap[] GetColumnMapsForType(DbDataReader storeDataReader, EdmType edmType, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
	{
		IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(edmType);
		ColumnMap[] array = new ColumnMap[allStructuralMembers.Count];
		int num = 0;
		foreach (EdmMember item in allStructuralMembers)
		{
			if (!Helper.IsScalarType(item.TypeUsage.EdmType))
			{
				throw new InvalidOperationException(Strings.ADP_InvalidDataReaderUnableToMaterializeNonScalarType(item.Name, item.TypeUsage.EdmType.FullName));
			}
			int memberOrdinalFromReader = GetMemberOrdinalFromReader(storeDataReader, item, edmType, renameList);
			array[num] = new ScalarColumnMap(item.TypeUsage, item.Name, 0, memberOrdinalFromReader);
			num++;
		}
		return array;
	}

	private static ScalarColumnMap[] CreateDiscriminatorColumnMaps(DbDataReader storeDataReader, FunctionImportMappingNonComposable mapping, int resultIndex)
	{
		TypeUsage type = TypeUsage.Create(MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.String));
		IList<string> discriminatorColumns = mapping.GetDiscriminatorColumns(resultIndex);
		ScalarColumnMap[] array = new ScalarColumnMap[discriminatorColumns.Count];
		for (int i = 0; i < array.Length; i++)
		{
			string text = discriminatorColumns[i];
			ScalarColumnMap scalarColumnMap = new ScalarColumnMap(type, text, 0, GetDiscriminatorOrdinalFromReader(storeDataReader, text, mapping.FunctionImport));
			array[i] = scalarColumnMap;
		}
		return array;
	}

	private static int GetMemberOrdinalFromReader(DbDataReader storeDataReader, EdmMember member, EdmType currentType, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
	{
		string renameForMember = GetRenameForMember(member, currentType, renameList);
		if (!TryGetColumnOrdinalFromReader(storeDataReader, renameForMember, out var ordinal))
		{
			throw new EntityCommandExecutionException(Strings.ADP_InvalidDataReaderMissingColumnForType(currentType.FullName, member.Name));
		}
		return ordinal;
	}

	private static string GetRenameForMember(EdmMember member, EdmType currentType, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
	{
		if (renameList != null && renameList.Count != 0 && renameList.Any((KeyValuePair<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> m) => m.Key == member.Name))
		{
			return renameList[member.Name].GetRename(currentType);
		}
		return member.Name;
	}

	private static int GetDiscriminatorOrdinalFromReader(DbDataReader storeDataReader, string columnName, EdmFunction functionImport)
	{
		if (!TryGetColumnOrdinalFromReader(storeDataReader, columnName, out var ordinal))
		{
			throw new EntityCommandExecutionException(Strings.ADP_InvalidDataReaderMissingDiscriminatorColumn(columnName, functionImport.FullName));
		}
		return ordinal;
	}

	private static bool TryGetColumnOrdinalFromReader(DbDataReader storeDataReader, string columnName, out int ordinal)
	{
		if (storeDataReader.FieldCount == 0)
		{
			ordinal = 0;
			return false;
		}
		try
		{
			ordinal = storeDataReader.GetOrdinal(columnName);
			return true;
		}
		catch (IndexOutOfRangeException)
		{
			ordinal = 0;
			return false;
		}
	}
}

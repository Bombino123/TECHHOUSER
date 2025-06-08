using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Core.Mapping;

internal class FunctionImportMappingComposableHelper
{
	private readonly EntityContainerMapping _entityContainerMapping;

	private readonly string m_sourceLocation;

	private readonly List<EdmSchemaError> m_parsingErrors;

	internal FunctionImportMappingComposableHelper(EntityContainerMapping entityContainerMapping, string sourceLocation, List<EdmSchemaError> parsingErrors)
	{
		_entityContainerMapping = entityContainerMapping;
		m_sourceLocation = sourceLocation;
		m_parsingErrors = parsingErrors;
	}

	internal bool TryCreateFunctionImportMappingComposableWithStructuralResult(EdmFunction functionImport, EdmFunction cTypeTargetFunction, List<FunctionImportStructuralTypeMapping> typeMappings, RowType cTypeTvfElementType, RowType sTypeTvfElementType, IXmlLineInfo lineInfo, out FunctionImportMappingComposable mapping)
	{
		mapping = null;
		if (typeMappings.Count == 0 && MetadataHelper.TryGetFunctionImportReturnType<StructuralType>(functionImport, 0, out var returnType))
		{
			if (returnType.Abstract)
			{
				AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_FunctionImport_ImplicitMappingForAbstractReturnType, returnType.FullName, functionImport.Identity, MappingErrorCode.MappingOfAbstractType, m_sourceLocation, lineInfo, m_parsingErrors);
				return false;
			}
			if (returnType.BuiltInTypeKind == BuiltInTypeKind.EntityType)
			{
				typeMappings.Add(new FunctionImportEntityTypeMapping(Enumerable.Empty<EntityType>(), new EntityType[1] { (EntityType)returnType }, Enumerable.Empty<FunctionImportEntityTypeMappingCondition>(), new Collection<FunctionImportReturnTypePropertyMapping>(), new LineInfo(lineInfo)));
			}
			else
			{
				typeMappings.Add(new FunctionImportComplexTypeMapping((ComplexType)returnType, new Collection<FunctionImportReturnTypePropertyMapping>(), new LineInfo(lineInfo)));
			}
		}
		EdmItemCollection itemCollection = ((_entityContainerMapping.StorageMappingItemCollection != null) ? _entityContainerMapping.StorageMappingItemCollection.EdmItemCollection : new EdmItemCollection(new EdmModel(DataSpace.CSpace)));
		FunctionImportStructuralTypeMappingKB functionImportStructuralTypeMappingKB = new FunctionImportStructuralTypeMappingKB(typeMappings, itemCollection);
		List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>> list = new List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>>();
		EdmProperty[] keys = null;
		ComplexType returnType2;
		if (functionImportStructuralTypeMappingKB.MappedEntityTypes.Count > 0)
		{
			if (!functionImportStructuralTypeMappingKB.ValidateTypeConditions(validateAmbiguity: true, m_parsingErrors, m_sourceLocation))
			{
				return false;
			}
			for (int i = 0; i < functionImportStructuralTypeMappingKB.MappedEntityTypes.Count; i++)
			{
				if (TryConvertToEntityTypeConditionsAndPropertyMappings(functionImport, functionImportStructuralTypeMappingKB, i, cTypeTvfElementType, sTypeTvfElementType, lineInfo, out var typeConditions, out var propertyMappings))
				{
					list.Add(Tuple.Create((StructuralType)functionImportStructuralTypeMappingKB.MappedEntityTypes[i], typeConditions, propertyMappings));
				}
			}
			if (list.Count < functionImportStructuralTypeMappingKB.MappedEntityTypes.Count)
			{
				return false;
			}
			if (!TryInferTVFKeys(list, out keys))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_FunctionImport_CannotInferTargetFunctionKeys, functionImport.Identity, MappingErrorCode.MappingFunctionImportCannotInferTargetFunctionKeys, m_sourceLocation, lineInfo, m_parsingErrors);
				return false;
			}
		}
		else if (MetadataHelper.TryGetFunctionImportReturnType<ComplexType>(functionImport, 0, out returnType2))
		{
			if (!TryConvertToPropertyMappings(returnType2, cTypeTvfElementType, sTypeTvfElementType, functionImport, functionImportStructuralTypeMappingKB, lineInfo, out var propertyMappings2))
			{
				return false;
			}
			list.Add(Tuple.Create((StructuralType)returnType2, new List<ConditionPropertyMapping>(), propertyMappings2));
		}
		mapping = new FunctionImportMappingComposable(functionImport, cTypeTargetFunction, list, keys, _entityContainerMapping);
		return true;
	}

	internal bool TryCreateFunctionImportMappingComposableWithScalarResult(EdmFunction functionImport, EdmFunction cTypeTargetFunction, EdmFunction sTypeTargetFunction, EdmType scalarResultType, RowType cTypeTvfElementType, IXmlLineInfo lineInfo, out FunctionImportMappingComposable mapping)
	{
		mapping = null;
		if (cTypeTvfElementType.Properties.Count > 1)
		{
			AddToSchemaErrors(Strings.Mapping_FunctionImport_ScalarMappingToMulticolumnTVF(functionImport.Identity, sTypeTargetFunction.Identity), MappingErrorCode.MappingFunctionImportScalarMappingToMulticolumnTVF, m_sourceLocation, lineInfo, m_parsingErrors);
			return false;
		}
		if (!ValidateFunctionImportMappingResultTypeCompatibility(TypeUsage.Create(scalarResultType), cTypeTvfElementType.Properties[0].TypeUsage))
		{
			AddToSchemaErrors(Strings.Mapping_FunctionImport_ScalarMappingTypeMismatch(functionImport.ReturnParameter.TypeUsage.EdmType.FullName, functionImport.Identity, sTypeTargetFunction.ReturnParameter.TypeUsage.EdmType.FullName, sTypeTargetFunction.Identity), MappingErrorCode.MappingFunctionImportScalarMappingTypeMismatch, m_sourceLocation, lineInfo, m_parsingErrors);
			return false;
		}
		mapping = new FunctionImportMappingComposable(functionImport, cTypeTargetFunction, null, null, _entityContainerMapping);
		return true;
	}

	private bool TryConvertToEntityTypeConditionsAndPropertyMappings(EdmFunction functionImport, FunctionImportStructuralTypeMappingKB functionImportKB, int typeID, RowType cTypeTvfElementType, RowType sTypeTvfElementType, IXmlLineInfo navLineInfo, out List<ConditionPropertyMapping> typeConditions, out List<PropertyMapping> propertyMappings)
	{
		EntityType structuralType = functionImportKB.MappedEntityTypes[typeID];
		typeConditions = new List<ConditionPropertyMapping>();
		bool flag = false;
		foreach (FunctionImportNormalizedEntityTypeMapping item in functionImportKB.NormalizedEntityTypeMappings.Where((FunctionImportNormalizedEntityTypeMapping f) => f.ImpliedEntityTypes[typeID]))
		{
			foreach (FunctionImportEntityTypeMappingCondition condition in item.ColumnConditions.Where((FunctionImportEntityTypeMappingCondition c) => c != null))
			{
				if (sTypeTvfElementType.Properties.TryGetValue(condition.ColumnName, ignoreCase: false, out var column))
				{
					object obj;
					bool? flag2;
					if (condition.ConditionValue.IsSentinel)
					{
						obj = null;
						flag2 = ((condition.ConditionValue != ValueCondition.IsNull) ? new bool?(false) : new bool?(true));
					}
					else
					{
						PrimitiveType primitiveType = (PrimitiveType)cTypeTvfElementType.Properties[column.Name].TypeUsage.EdmType;
						obj = ((FunctionImportEntityTypeMappingConditionValue)condition).GetConditionValue(primitiveType.ClrEquivalentType, delegate
						{
							AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_InvalidContent_ConditionMapping_InvalidPrimitiveTypeKind, column.Name, column.TypeUsage.EdmType.FullName, MappingErrorCode.ConditionError, m_sourceLocation, condition.LineInfo, m_parsingErrors);
						}, delegate
						{
							AddToSchemaErrors(Strings.Mapping_ConditionValueTypeMismatch, MappingErrorCode.ConditionError, m_sourceLocation, condition.LineInfo, m_parsingErrors);
						});
						if (obj == null)
						{
							flag = true;
							continue;
						}
						flag2 = null;
					}
					typeConditions.Add((obj != null) ? ((ConditionPropertyMapping)new ValueConditionMapping(column, obj)) : ((ConditionPropertyMapping)new IsNullConditionMapping(column, flag2.Value)));
				}
				else
				{
					AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Column, condition.ColumnName, MappingErrorCode.InvalidStorageMember, m_sourceLocation, condition.LineInfo, m_parsingErrors);
				}
			}
		}
		flag |= !TryConvertToPropertyMappings(structuralType, cTypeTvfElementType, sTypeTvfElementType, functionImport, functionImportKB, navLineInfo, out propertyMappings);
		return !flag;
	}

	private bool TryConvertToPropertyMappings(StructuralType structuralType, RowType cTypeTvfElementType, RowType sTypeTvfElementType, EdmFunction functionImport, FunctionImportStructuralTypeMappingKB functionImportKB, IXmlLineInfo navLineInfo, out List<PropertyMapping> propertyMappings)
	{
		propertyMappings = new List<PropertyMapping>();
		bool flag = false;
		foreach (EdmProperty allStructuralMember in TypeHelpers.GetAllStructuralMembers(structuralType))
		{
			if (!Helper.IsScalarType(allStructuralMember.TypeUsage.EdmType))
			{
				EdmSchemaError item = new EdmSchemaError(Strings.Mapping_Invalid_CSide_ScalarProperty(allStructuralMember.Name), 2085, EdmSchemaErrorSeverity.Error, m_sourceLocation, navLineInfo.LineNumber, navLineInfo.LinePosition);
				m_parsingErrors.Add(item);
				flag = true;
				continue;
			}
			string text = null;
			IXmlLineInfo lineInfo = null;
			bool flag2;
			if (functionImportKB.ReturnTypeColumnsRenameMapping.TryGetValue(allStructuralMember.Name, out var value))
			{
				flag2 = true;
				text = value.GetRename(structuralType, out lineInfo);
			}
			else
			{
				flag2 = false;
				text = allStructuralMember.Name;
			}
			lineInfo = ((lineInfo != null && lineInfo.HasLineInfo()) ? lineInfo : navLineInfo);
			if (sTypeTvfElementType.Properties.TryGetValue(text, ignoreCase: false, out var item2))
			{
				EdmProperty edmProperty2 = cTypeTvfElementType.Properties[text];
				if (ValidateFunctionImportMappingResultTypeCompatibility(allStructuralMember.TypeUsage, edmProperty2.TypeUsage))
				{
					propertyMappings.Add(new ScalarPropertyMapping(allStructuralMember, item2));
					continue;
				}
				EdmSchemaError item3 = new EdmSchemaError(GetInvalidMemberMappingErrorMessage(allStructuralMember, item2), 2019, EdmSchemaErrorSeverity.Error, m_sourceLocation, lineInfo.LineNumber, lineInfo.LinePosition);
				m_parsingErrors.Add(item3);
				flag = true;
			}
			else if (flag2)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Column, text, MappingErrorCode.InvalidStorageMember, m_sourceLocation, lineInfo, m_parsingErrors);
				flag = true;
			}
			else
			{
				EdmSchemaError item4 = new EdmSchemaError(Strings.Mapping_FunctionImport_PropertyNotMapped(allStructuralMember.Name, structuralType.FullName, functionImport.Identity), 2104, EdmSchemaErrorSeverity.Error, m_sourceLocation, lineInfo.LineNumber, lineInfo.LinePosition);
				m_parsingErrors.Add(item4);
				flag = true;
			}
		}
		return !flag;
	}

	private static bool TryInferTVFKeys(List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>> structuralTypeMappings, out EdmProperty[] keys)
	{
		keys = null;
		foreach (Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>> structuralTypeMapping in structuralTypeMappings)
		{
			if (!TryInferTVFKeysForEntityType((EntityType)structuralTypeMapping.Item1, structuralTypeMapping.Item3, out var keys2))
			{
				keys = null;
				return false;
			}
			if (keys == null)
			{
				keys = keys2;
				continue;
			}
			for (int i = 0; i < keys.Length; i++)
			{
				if (!keys[i].EdmEquals(keys2[i]))
				{
					keys = null;
					return false;
				}
			}
		}
		for (int j = 0; j < keys.Length; j++)
		{
			if (keys[j].Nullable)
			{
				keys = null;
				return false;
			}
		}
		return true;
	}

	private static bool TryInferTVFKeysForEntityType(EntityType entityType, List<PropertyMapping> propertyMappings, out EdmProperty[] keys)
	{
		keys = new EdmProperty[entityType.KeyMembers.Count];
		for (int i = 0; i < keys.Length; i++)
		{
			if (!(propertyMappings[entityType.Properties.IndexOf((EdmProperty)entityType.KeyMembers[i])] is ScalarPropertyMapping scalarPropertyMapping))
			{
				keys = null;
				return false;
			}
			keys[i] = scalarPropertyMapping.Column;
		}
		return true;
	}

	private static bool ValidateFunctionImportMappingResultTypeCompatibility(TypeUsage cSpaceMemberType, TypeUsage sSpaceMemberType)
	{
		TypeUsage typeUsage = ResolveTypeUsageForEnums(cSpaceMemberType);
		bool num = TypeSemantics.IsStructurallyEqualOrPromotableTo(sSpaceMemberType, typeUsage);
		bool flag = TypeSemantics.IsStructurallyEqualOrPromotableTo(typeUsage, sSpaceMemberType);
		return num || flag;
	}

	private static TypeUsage ResolveTypeUsageForEnums(TypeUsage typeUsage)
	{
		return MappingItemLoader.ResolveTypeUsageForEnums(typeUsage);
	}

	private static void AddToSchemaErrors(string message, MappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
	{
		MappingItemLoader.AddToSchemaErrors(message, errorCode, location, lineInfo, parsingErrors);
	}

	private static void AddToSchemaErrorsWithMemberInfo(Func<object, string> messageFormat, string errorMember, MappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
	{
		MappingItemLoader.AddToSchemaErrorsWithMemberInfo(messageFormat, errorMember, errorCode, location, lineInfo, parsingErrors);
	}

	private static void AddToSchemaErrorWithMemberAndStructure(Func<object, object, string> messageFormat, string errorMember, string errorStructure, MappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
	{
		MappingItemLoader.AddToSchemaErrorWithMemberAndStructure(messageFormat, errorMember, errorStructure, errorCode, location, lineInfo, parsingErrors);
	}

	private static string GetInvalidMemberMappingErrorMessage(EdmMember cSpaceMember, EdmMember sSpaceMember)
	{
		return MappingItemLoader.GetInvalidMemberMappingErrorMessage(cSpaceMember, sSpaceMember);
	}
}

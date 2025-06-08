using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MslXmlSchemaWriter : XmlSchemaWriter
{
	private string _entityTypeNamespace;

	private string _dbSchemaName;

	internal MslXmlSchemaWriter(XmlWriter xmlWriter, double version)
	{
		_xmlWriter = xmlWriter;
		_version = version;
	}

	internal void WriteSchema(DbDatabaseMapping databaseMapping)
	{
		WriteSchemaElementHeader();
		WriteDbModelElement(databaseMapping);
		WriteEndElement();
	}

	private void WriteSchemaElementHeader()
	{
		string mslNamespace = MslConstructs.GetMslNamespace(_version);
		_xmlWriter.WriteStartElement("Mapping", mslNamespace);
		_xmlWriter.WriteAttributeString("Space", "C-S");
	}

	private void WriteDbModelElement(DbDatabaseMapping databaseMapping)
	{
		_entityTypeNamespace = databaseMapping.Model.NamespaceNames.SingleOrDefault();
		_dbSchemaName = databaseMapping.Database.Containers.Single().Name;
		WriteEntityContainerMappingElement(databaseMapping.EntityContainerMappings.First());
	}

	internal void WriteEntityContainerMappingElement(EntityContainerMapping containerMapping)
	{
		_xmlWriter.WriteStartElement("EntityContainerMapping");
		_xmlWriter.WriteAttributeString("StorageEntityContainer", _dbSchemaName);
		_xmlWriter.WriteAttributeString("CdmEntityContainer", containerMapping.EdmEntityContainer.Name);
		foreach (EntitySetMapping entitySetMapping in containerMapping.EntitySetMappings)
		{
			WriteEntitySetMappingElement(entitySetMapping);
		}
		foreach (AssociationSetMapping associationSetMapping in containerMapping.AssociationSetMappings)
		{
			WriteAssociationSetMappingElement(associationSetMapping);
		}
		foreach (FunctionImportMappingComposable item in containerMapping.FunctionImportMappings.OfType<FunctionImportMappingComposable>())
		{
			WriteFunctionImportMappingElement(item);
		}
		foreach (FunctionImportMappingNonComposable item2 in containerMapping.FunctionImportMappings.OfType<FunctionImportMappingNonComposable>())
		{
			WriteFunctionImportMappingElement(item2);
		}
		_xmlWriter.WriteEndElement();
	}

	public void WriteEntitySetMappingElement(EntitySetMapping entitySetMapping)
	{
		_xmlWriter.WriteStartElement("EntitySetMapping");
		_xmlWriter.WriteAttributeString("Name", entitySetMapping.EntitySet.Name);
		foreach (EntityTypeMapping entityTypeMapping in entitySetMapping.EntityTypeMappings)
		{
			WriteEntityTypeMappingElement(entityTypeMapping);
		}
		foreach (EntityTypeModificationFunctionMapping modificationFunctionMapping in entitySetMapping.ModificationFunctionMappings)
		{
			_xmlWriter.WriteStartElement("EntityTypeMapping");
			_xmlWriter.WriteAttributeString("TypeName", GetEntityTypeName(_entityTypeNamespace + "." + modificationFunctionMapping.EntityType.Name, isHierarchyMapping: false));
			WriteModificationFunctionMapping(modificationFunctionMapping);
			_xmlWriter.WriteEndElement();
		}
		_xmlWriter.WriteEndElement();
	}

	public void WriteAssociationSetMappingElement(AssociationSetMapping associationSetMapping)
	{
		_xmlWriter.WriteStartElement("AssociationSetMapping");
		_xmlWriter.WriteAttributeString("Name", associationSetMapping.AssociationSet.Name);
		_xmlWriter.WriteAttributeString("TypeName", _entityTypeNamespace + "." + associationSetMapping.AssociationSet.ElementType.Name);
		_xmlWriter.WriteAttributeString("StoreEntitySet", associationSetMapping.Table.Name);
		WriteAssociationEndMappingElement(associationSetMapping.SourceEndMapping);
		WriteAssociationEndMappingElement(associationSetMapping.TargetEndMapping);
		if (associationSetMapping.ModificationFunctionMapping != null)
		{
			WriteModificationFunctionMapping(associationSetMapping.ModificationFunctionMapping);
		}
		foreach (ConditionPropertyMapping condition in associationSetMapping.Conditions)
		{
			WriteConditionElement(condition);
		}
		_xmlWriter.WriteEndElement();
	}

	private void WriteAssociationEndMappingElement(EndPropertyMapping endMapping)
	{
		_xmlWriter.WriteStartElement("EndProperty");
		_xmlWriter.WriteAttributeString("Name", endMapping.AssociationEnd.Name);
		foreach (ScalarPropertyMapping propertyMapping in endMapping.PropertyMappings)
		{
			WriteScalarPropertyElement(propertyMapping.Property.Name, propertyMapping.Column.Name);
		}
		_xmlWriter.WriteEndElement();
	}

	private void WriteEntityTypeMappingElement(EntityTypeMapping entityTypeMapping)
	{
		_xmlWriter.WriteStartElement("EntityTypeMapping");
		_xmlWriter.WriteAttributeString("TypeName", GetEntityTypeName(_entityTypeNamespace + "." + entityTypeMapping.EntityType.Name, entityTypeMapping.IsHierarchyMapping));
		foreach (MappingFragment mappingFragment in entityTypeMapping.MappingFragments)
		{
			WriteMappingFragmentElement(mappingFragment);
		}
		_xmlWriter.WriteEndElement();
	}

	internal void WriteMappingFragmentElement(MappingFragment mappingFragment)
	{
		_xmlWriter.WriteStartElement("MappingFragment");
		_xmlWriter.WriteAttributeString("StoreEntitySet", mappingFragment.TableSet.Name);
		foreach (PropertyMapping propertyMapping in mappingFragment.PropertyMappings)
		{
			WritePropertyMapping(propertyMapping);
		}
		foreach (ConditionPropertyMapping columnCondition in mappingFragment.ColumnConditions)
		{
			WriteConditionElement(columnCondition);
		}
		_xmlWriter.WriteEndElement();
	}

	public void WriteFunctionImportMappingElement(FunctionImportMappingComposable functionImportMapping)
	{
		WriteFunctionImportMappingStartElement(functionImportMapping);
		if (functionImportMapping.StructuralTypeMappings != null)
		{
			_xmlWriter.WriteStartElement("ResultMapping");
			Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>> tuple = functionImportMapping.StructuralTypeMappings.Single();
			if (tuple.Item1.BuiltInTypeKind == BuiltInTypeKind.ComplexType)
			{
				_xmlWriter.WriteStartElement("ComplexTypeMapping");
				_xmlWriter.WriteAttributeString("TypeName", tuple.Item1.FullName);
			}
			else
			{
				_xmlWriter.WriteStartElement("EntityTypeMapping");
				_xmlWriter.WriteAttributeString("TypeName", tuple.Item1.FullName);
				foreach (ConditionPropertyMapping item in tuple.Item2)
				{
					WriteConditionElement(item);
				}
			}
			foreach (PropertyMapping item2 in tuple.Item3)
			{
				WritePropertyMapping(item2);
			}
			_xmlWriter.WriteEndElement();
			_xmlWriter.WriteEndElement();
		}
		WriteFunctionImportEndElement();
	}

	public void WriteFunctionImportMappingElement(FunctionImportMappingNonComposable functionImportMapping)
	{
		WriteFunctionImportMappingStartElement(functionImportMapping);
		foreach (FunctionImportResultMapping resultMapping in functionImportMapping.ResultMappings)
		{
			WriteFunctionImportResultMappingElement(resultMapping);
		}
		WriteFunctionImportEndElement();
	}

	private void WriteFunctionImportMappingStartElement(FunctionImportMapping functionImportMapping)
	{
		_xmlWriter.WriteStartElement("FunctionImportMapping");
		_xmlWriter.WriteAttributeString("FunctionName", functionImportMapping.TargetFunction.FullName);
		_xmlWriter.WriteAttributeString("FunctionImportName", functionImportMapping.FunctionImport.Name);
	}

	private void WriteFunctionImportResultMappingElement(FunctionImportResultMapping resultMapping)
	{
		_xmlWriter.WriteStartElement("ResultMapping");
		foreach (FunctionImportStructuralTypeMapping typeMapping in resultMapping.TypeMappings)
		{
			if (typeMapping is FunctionImportEntityTypeMapping entityTypeMapping)
			{
				WriteFunctionImportEntityTypeMappingElement(entityTypeMapping);
			}
			else
			{
				WriteFunctionImportComplexTypeMappingElement((FunctionImportComplexTypeMapping)typeMapping);
			}
		}
		_xmlWriter.WriteEndElement();
	}

	private void WriteFunctionImportEntityTypeMappingElement(FunctionImportEntityTypeMapping entityTypeMapping)
	{
		_xmlWriter.WriteStartElement("EntityTypeMapping");
		string value = CreateFunctionImportEntityTypeMappingTypeName(entityTypeMapping);
		_xmlWriter.WriteAttributeString("TypeName", value);
		WriteFunctionImportPropertyMappingElements(entityTypeMapping.PropertyMappings.Cast<FunctionImportReturnTypeScalarPropertyMapping>());
		foreach (FunctionImportEntityTypeMappingCondition condition in entityTypeMapping.Conditions)
		{
			WriteFunctionImportConditionElement(condition);
		}
		_xmlWriter.WriteEndElement();
	}

	internal static string CreateFunctionImportEntityTypeMappingTypeName(FunctionImportEntityTypeMapping entityTypeMapping)
	{
		return string.Join(";", entityTypeMapping.EntityTypes.Select((EntityType e) => GetEntityTypeName(e.FullName, isHierarchyMapping: false)).Concat(entityTypeMapping.IsOfTypeEntityTypes.Select((EntityType e) => GetEntityTypeName(e.FullName, isHierarchyMapping: true))));
	}

	private void WriteFunctionImportComplexTypeMappingElement(FunctionImportComplexTypeMapping complexTypeMapping)
	{
		_xmlWriter.WriteStartElement("ComplexTypeMapping");
		_xmlWriter.WriteAttributeString("TypeName", complexTypeMapping.ReturnType.FullName);
		WriteFunctionImportPropertyMappingElements(complexTypeMapping.PropertyMappings.Cast<FunctionImportReturnTypeScalarPropertyMapping>());
		_xmlWriter.WriteEndElement();
	}

	private void WriteFunctionImportPropertyMappingElements(IEnumerable<FunctionImportReturnTypeScalarPropertyMapping> propertyMappings)
	{
		foreach (FunctionImportReturnTypeScalarPropertyMapping propertyMapping in propertyMappings)
		{
			WriteScalarPropertyElement(propertyMapping.PropertyName, propertyMapping.ColumnName);
		}
	}

	private void WriteFunctionImportConditionElement(FunctionImportEntityTypeMappingCondition condition)
	{
		_xmlWriter.WriteStartElement("Condition");
		_xmlWriter.WriteAttributeString("ColumnName", condition.ColumnName);
		if (condition is FunctionImportEntityTypeMappingConditionIsNull functionImportEntityTypeMappingConditionIsNull)
		{
			WriteIsNullConditionAttribute(functionImportEntityTypeMappingConditionIsNull.IsNull);
		}
		else
		{
			WriteConditionValue(((FunctionImportEntityTypeMappingConditionValue)condition).Value);
		}
		_xmlWriter.WriteEndElement();
	}

	private void WriteFunctionImportEndElement()
	{
		_xmlWriter.WriteEndElement();
	}

	private void WriteModificationFunctionMapping(EntityTypeModificationFunctionMapping modificationFunctionMapping)
	{
		_xmlWriter.WriteStartElement("ModificationFunctionMapping");
		WriteFunctionMapping("InsertFunction", modificationFunctionMapping.InsertFunctionMapping);
		WriteFunctionMapping("UpdateFunction", modificationFunctionMapping.UpdateFunctionMapping);
		WriteFunctionMapping("DeleteFunction", modificationFunctionMapping.DeleteFunctionMapping);
		_xmlWriter.WriteEndElement();
	}

	private void WriteModificationFunctionMapping(AssociationSetModificationFunctionMapping modificationFunctionMapping)
	{
		_xmlWriter.WriteStartElement("ModificationFunctionMapping");
		WriteFunctionMapping("InsertFunction", modificationFunctionMapping.InsertFunctionMapping, associationSetMapping: true);
		WriteFunctionMapping("DeleteFunction", modificationFunctionMapping.DeleteFunctionMapping, associationSetMapping: true);
		_xmlWriter.WriteEndElement();
	}

	public void WriteFunctionMapping(string functionElement, ModificationFunctionMapping functionMapping, bool associationSetMapping = false)
	{
		_xmlWriter.WriteStartElement(functionElement);
		_xmlWriter.WriteAttributeString("FunctionName", functionMapping.Function.FullName);
		if (functionMapping.RowsAffectedParameter != null)
		{
			_xmlWriter.WriteAttributeString("RowsAffectedParameter", functionMapping.RowsAffectedParameter.Name);
		}
		if (!associationSetMapping)
		{
			WritePropertyParameterBindings(functionMapping.ParameterBindings);
			WriteAssociationParameterBindings(functionMapping.ParameterBindings);
			if (functionMapping.ResultBindings != null)
			{
				WriteResultBindings(functionMapping.ResultBindings);
			}
		}
		else
		{
			WriteAssociationSetMappingParameterBindings(functionMapping.ParameterBindings);
		}
		_xmlWriter.WriteEndElement();
	}

	private void WriteAssociationSetMappingParameterBindings(IEnumerable<ModificationFunctionParameterBinding> parameterBindings)
	{
		foreach (IGrouping<AssociationSetEnd, ModificationFunctionParameterBinding> item in from pm in parameterBindings
			where pm.MemberPath.AssociationSetEnd != null
			group pm by pm.MemberPath.AssociationSetEnd)
		{
			_xmlWriter.WriteStartElement("EndProperty");
			_xmlWriter.WriteAttributeString("Name", item.Key.Name);
			foreach (ModificationFunctionParameterBinding item2 in item)
			{
				WriteScalarParameterElement(item2.MemberPath.Members.First(), item2);
			}
			_xmlWriter.WriteEndElement();
		}
	}

	private void WritePropertyParameterBindings(IEnumerable<ModificationFunctionParameterBinding> parameterBindings, int level = 0)
	{
		foreach (IGrouping<EdmMember, ModificationFunctionParameterBinding> item in from pm in parameterBindings
			where pm.MemberPath.AssociationSetEnd == null && pm.MemberPath.Members.Count() > level
			group pm by pm.MemberPath.Members.ElementAt(level))
		{
			EdmProperty edmProperty = (EdmProperty)item.Key;
			if (edmProperty.IsComplexType)
			{
				_xmlWriter.WriteStartElement("ComplexProperty");
				_xmlWriter.WriteAttributeString("Name", edmProperty.Name);
				_xmlWriter.WriteAttributeString("TypeName", _entityTypeNamespace + "." + edmProperty.ComplexType.Name);
				WritePropertyParameterBindings(item, level + 1);
				_xmlWriter.WriteEndElement();
				continue;
			}
			foreach (ModificationFunctionParameterBinding item2 in item)
			{
				WriteScalarParameterElement(edmProperty, item2);
			}
		}
	}

	private void WriteAssociationParameterBindings(IEnumerable<ModificationFunctionParameterBinding> parameterBindings)
	{
		foreach (IGrouping<AssociationSetEnd, ModificationFunctionParameterBinding> group in from pm in parameterBindings
			where pm.MemberPath.AssociationSetEnd != null
			group pm by pm.MemberPath.AssociationSetEnd)
		{
			_xmlWriter.WriteStartElement("AssociationEnd");
			AssociationSet parentAssociationSet = group.Key.ParentAssociationSet;
			_xmlWriter.WriteAttributeString("AssociationSet", parentAssociationSet.Name);
			_xmlWriter.WriteAttributeString("From", group.Key.Name);
			_xmlWriter.WriteAttributeString("To", parentAssociationSet.AssociationSetEnds.Single((AssociationSetEnd ae) => ae != group.Key).Name);
			foreach (ModificationFunctionParameterBinding item in group)
			{
				WriteScalarParameterElement(item.MemberPath.Members.First(), item);
			}
			_xmlWriter.WriteEndElement();
		}
	}

	private void WriteResultBindings(IEnumerable<ModificationFunctionResultBinding> resultBindings)
	{
		foreach (ModificationFunctionResultBinding resultBinding in resultBindings)
		{
			_xmlWriter.WriteStartElement("ResultBinding");
			_xmlWriter.WriteAttributeString("Name", resultBinding.Property.Name);
			_xmlWriter.WriteAttributeString("ColumnName", resultBinding.ColumnName);
			_xmlWriter.WriteEndElement();
		}
	}

	private void WriteScalarParameterElement(EdmMember member, ModificationFunctionParameterBinding parameterBinding)
	{
		_xmlWriter.WriteStartElement("ScalarProperty");
		_xmlWriter.WriteAttributeString("Name", member.Name);
		_xmlWriter.WriteAttributeString("ParameterName", parameterBinding.Parameter.Name);
		_xmlWriter.WriteAttributeString("Version", parameterBinding.IsCurrent ? "Current" : "Original");
		_xmlWriter.WriteEndElement();
	}

	private void WritePropertyMapping(PropertyMapping propertyMapping)
	{
		if (propertyMapping is ScalarPropertyMapping scalarPropertyMapping)
		{
			WritePropertyMapping(scalarPropertyMapping);
		}
		else if (propertyMapping is ComplexPropertyMapping complexPropertyMapping)
		{
			WritePropertyMapping(complexPropertyMapping);
		}
	}

	private void WritePropertyMapping(ScalarPropertyMapping scalarPropertyMapping)
	{
		WriteScalarPropertyElement(scalarPropertyMapping.Property.Name, scalarPropertyMapping.Column.Name);
	}

	private void WritePropertyMapping(ComplexPropertyMapping complexPropertyMapping)
	{
		_xmlWriter.WriteStartElement("ComplexProperty");
		_xmlWriter.WriteAttributeString("Name", complexPropertyMapping.Property.Name);
		_xmlWriter.WriteAttributeString("TypeName", _entityTypeNamespace + "." + complexPropertyMapping.Property.ComplexType.Name);
		foreach (PropertyMapping propertyMapping in complexPropertyMapping.TypeMappings.Single().PropertyMappings)
		{
			WritePropertyMapping(propertyMapping);
		}
		_xmlWriter.WriteEndElement();
	}

	private static string GetEntityTypeName(string fullyQualifiedEntityTypeName, bool isHierarchyMapping)
	{
		if (isHierarchyMapping)
		{
			return "IsTypeOf(" + fullyQualifiedEntityTypeName + ")";
		}
		return fullyQualifiedEntityTypeName;
	}

	private void WriteConditionElement(ConditionPropertyMapping condition)
	{
		_xmlWriter.WriteStartElement("Condition");
		if (condition.IsNull.HasValue)
		{
			WriteIsNullConditionAttribute(condition.IsNull.Value);
		}
		else
		{
			WriteConditionValue(condition.Value);
		}
		_xmlWriter.WriteAttributeString("ColumnName", condition.Column.Name);
		_xmlWriter.WriteEndElement();
	}

	private void WriteIsNullConditionAttribute(bool isNullValue)
	{
		_xmlWriter.WriteAttributeString("IsNull", XmlSchemaWriter.GetLowerCaseStringFromBoolValue(isNullValue));
	}

	private void WriteConditionValue(object conditionValue)
	{
		if (conditionValue is bool)
		{
			_xmlWriter.WriteAttributeString("Value", ((bool)conditionValue) ? "1" : "0");
		}
		else
		{
			_xmlWriter.WriteAttributeString("Value", conditionValue.ToString());
		}
	}

	private void WriteScalarPropertyElement(string propertyName, string columnName)
	{
		_xmlWriter.WriteStartElement("ScalarProperty");
		_xmlWriter.WriteAttributeString("Name", propertyName);
		_xmlWriter.WriteAttributeString("ColumnName", columnName);
		_xmlWriter.WriteEndElement();
	}
}

using System.Collections.Generic;
using System.Data.Entity.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class EdmSerializationVisitor : EdmModelVisitor
{
	private readonly EdmXmlSchemaWriter _schemaWriter;

	public EdmSerializationVisitor(XmlWriter xmlWriter, double edmVersion, bool serializeDefaultNullability = false)
		: this(new EdmXmlSchemaWriter(xmlWriter, edmVersion, serializeDefaultNullability))
	{
	}

	public EdmSerializationVisitor(EdmXmlSchemaWriter schemaWriter)
	{
		_schemaWriter = schemaWriter;
	}

	public void Visit(EdmModel edmModel, string modelNamespace)
	{
		string schemaNamespace = modelNamespace ?? edmModel.NamespaceNames.DefaultIfEmpty("Empty").Single();
		_schemaWriter.WriteSchemaElementHeader(schemaNamespace);
		VisitEdmModel(edmModel);
		_schemaWriter.WriteEndElement();
	}

	public void Visit(EdmModel edmModel, string provider, string providerManifestToken)
	{
		Visit(edmModel, edmModel.Containers.Single().Name + "Schema", provider, providerManifestToken);
	}

	public void Visit(EdmModel edmModel, string namespaceName, string provider, string providerManifestToken)
	{
		bool writeStoreSchemaGenNamespace = edmModel.Container.BaseEntitySets.Any((EntitySetBase e) => e.MetadataProperties.Any((MetadataProperty p) => p.Name.StartsWith("http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator", StringComparison.Ordinal)));
		_schemaWriter.WriteSchemaElementHeader(namespaceName, provider, providerManifestToken, writeStoreSchemaGenNamespace);
		VisitEdmModel(edmModel);
		_schemaWriter.WriteEndElement();
	}

	protected override void VisitEdmEntityContainer(EntityContainer item)
	{
		_schemaWriter.WriteEntityContainerElementHeader(item);
		base.VisitEdmEntityContainer(item);
		_schemaWriter.WriteEndElement();
	}

	protected internal override void VisitEdmFunction(EdmFunction item)
	{
		_schemaWriter.WriteFunctionElementHeader(item);
		base.VisitEdmFunction(item);
		_schemaWriter.WriteEndElement();
	}

	protected internal override void VisitFunctionParameter(FunctionParameter functionParameter)
	{
		_schemaWriter.WriteFunctionParameterHeader(functionParameter);
		base.VisitFunctionParameter(functionParameter);
		_schemaWriter.WriteEndElement();
	}

	protected internal override void VisitFunctionReturnParameter(FunctionParameter returnParameter)
	{
		if (returnParameter.TypeUsage.EdmType.BuiltInTypeKind != BuiltInTypeKind.PrimitiveType)
		{
			_schemaWriter.WriteFunctionReturnTypeElementHeader();
			base.VisitFunctionReturnParameter(returnParameter);
			_schemaWriter.WriteEndElement();
		}
		else
		{
			base.VisitFunctionReturnParameter(returnParameter);
		}
	}

	protected internal override void VisitCollectionType(CollectionType collectionType)
	{
		_schemaWriter.WriteCollectionTypeElementHeader();
		base.VisitCollectionType(collectionType);
		_schemaWriter.WriteEndElement();
	}

	protected override void VisitEdmAssociationSet(AssociationSet item)
	{
		_schemaWriter.WriteAssociationSetElementHeader(item);
		base.VisitEdmAssociationSet(item);
		if (item.SourceSet != null)
		{
			_schemaWriter.WriteAssociationSetEndElement(item.SourceSet, item.SourceEnd.Name);
		}
		if (item.TargetSet != null)
		{
			_schemaWriter.WriteAssociationSetEndElement(item.TargetSet, item.TargetEnd.Name);
		}
		_schemaWriter.WriteEndElement();
	}

	protected internal override void VisitEdmEntitySet(EntitySet item)
	{
		_schemaWriter.WriteEntitySetElementHeader(item);
		_schemaWriter.WriteDefiningQuery(item);
		base.VisitEdmEntitySet(item);
		_schemaWriter.WriteEndElement();
	}

	protected internal override void VisitFunctionImport(EdmFunction functionImport)
	{
		_schemaWriter.WriteFunctionImportElementHeader(functionImport);
		if (functionImport.ReturnParameters.Count == 1)
		{
			_schemaWriter.WriteFunctionImportReturnTypeAttributes(functionImport.ReturnParameter, functionImport.EntitySet, inline: true);
			VisitFunctionImportReturnParameter(functionImport.ReturnParameter);
		}
		base.VisitFunctionImport(functionImport);
		if (functionImport.ReturnParameters.Count > 1)
		{
			VisitFunctionImportReturnParameters(functionImport);
		}
		_schemaWriter.WriteEndElement();
	}

	protected internal override void VisitFunctionImportParameter(FunctionParameter parameter)
	{
		_schemaWriter.WriteFunctionImportParameterElementHeader(parameter);
		base.VisitFunctionImportParameter(parameter);
		_schemaWriter.WriteEndElement();
	}

	private void VisitFunctionImportReturnParameters(EdmFunction functionImport)
	{
		for (int i = 0; i < functionImport.ReturnParameters.Count; i++)
		{
			_schemaWriter.WriteFunctionReturnTypeElementHeader();
			_schemaWriter.WriteFunctionImportReturnTypeAttributes(functionImport.ReturnParameters[i], functionImport.EntitySets[i], inline: false);
			VisitFunctionImportReturnParameter(functionImport.ReturnParameter);
			_schemaWriter.WriteEndElement();
		}
	}

	protected internal override void VisitRowType(RowType rowType)
	{
		_schemaWriter.WriteRowTypeElementHeader();
		base.VisitRowType(rowType);
		_schemaWriter.WriteEndElement();
	}

	protected internal override void VisitEdmEntityType(EntityType item)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendSchemaErrors(stringBuilder, item);
		if (MetadataItemHelper.IsInvalid(item))
		{
			AppendMetadataItem(stringBuilder, item, delegate(EdmSerializationVisitor v, EntityType i)
			{
				v.InternalVisitEdmEntityType(i);
			});
			WriteComment(stringBuilder.ToString());
		}
		else
		{
			WriteComment(stringBuilder.ToString());
			InternalVisitEdmEntityType(item);
		}
	}

	protected override void VisitEdmEnumType(EnumType item)
	{
		_schemaWriter.WriteEnumTypeElementHeader(item);
		base.VisitEdmEnumType(item);
		_schemaWriter.WriteEndElement();
	}

	protected override void VisitEdmEnumTypeMember(EnumMember item)
	{
		_schemaWriter.WriteEnumTypeMemberElementHeader(item);
		base.VisitEdmEnumTypeMember(item);
		_schemaWriter.WriteEndElement();
	}

	protected override void VisitKeyProperties(EntityType entityType, IList<EdmProperty> properties)
	{
		if (!properties.Any())
		{
			return;
		}
		_schemaWriter.WriteDeclaredKeyPropertiesElementHeader();
		foreach (EdmProperty property in properties)
		{
			_schemaWriter.WriteDeclaredKeyPropertyRefElement(property);
		}
		_schemaWriter.WriteEndElement();
	}

	protected internal override void VisitEdmProperty(EdmProperty item)
	{
		_schemaWriter.WritePropertyElementHeader(item);
		base.VisitEdmProperty(item);
		_schemaWriter.WriteEndElement();
	}

	protected override void VisitEdmNavigationProperty(NavigationProperty item)
	{
		_schemaWriter.WriteNavigationPropertyElementHeader(item);
		base.VisitEdmNavigationProperty(item);
		_schemaWriter.WriteEndElement();
	}

	protected override void VisitComplexType(ComplexType item)
	{
		_schemaWriter.WriteComplexTypeElementHeader(item);
		base.VisitComplexType(item);
		_schemaWriter.WriteEndElement();
	}

	protected internal override void VisitEdmAssociationType(AssociationType item)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendSchemaErrors(stringBuilder, item);
		if (MetadataItemHelper.IsInvalid(item))
		{
			AppendMetadataItem(stringBuilder, item, delegate(EdmSerializationVisitor v, AssociationType i)
			{
				v.InternalVisitEdmAssociationType(i);
			});
			WriteComment(stringBuilder.ToString());
		}
		else
		{
			WriteComment(stringBuilder.ToString());
			InternalVisitEdmAssociationType(item);
		}
	}

	protected override void VisitEdmAssociationEnd(RelationshipEndMember item)
	{
		_schemaWriter.WriteAssociationEndElementHeader(item);
		if (item.DeleteBehavior != 0)
		{
			_schemaWriter.WriteOperationActionElement("OnDelete", item.DeleteBehavior);
		}
		VisitMetadataItem(item);
		_schemaWriter.WriteEndElement();
	}

	protected override void VisitEdmAssociationConstraint(ReferentialConstraint item)
	{
		_schemaWriter.WriteReferentialConstraintElementHeader();
		_schemaWriter.WriteReferentialConstraintRoleElement("Principal", item.FromRole, item.FromProperties);
		_schemaWriter.WriteReferentialConstraintRoleElement("Dependent", item.ToRole, item.ToProperties);
		VisitMetadataItem(item);
		_schemaWriter.WriteEndElement();
	}

	private void InternalVisitEdmEntityType(EntityType item)
	{
		_schemaWriter.WriteEntityTypeElementHeader(item);
		base.VisitEdmEntityType(item);
		_schemaWriter.WriteEndElement();
	}

	private void InternalVisitEdmAssociationType(AssociationType item)
	{
		_schemaWriter.WriteAssociationTypeElementHeader(item);
		base.VisitEdmAssociationType(item);
		_schemaWriter.WriteEndElement();
	}

	private static void AppendSchemaErrors(StringBuilder builder, MetadataItem item)
	{
		if (!MetadataItemHelper.HasSchemaErrors(item))
		{
			return;
		}
		builder.Append(Strings.MetadataItemErrorsFoundDuringGeneration);
		foreach (EdmSchemaError schemaError in MetadataItemHelper.GetSchemaErrors(item))
		{
			builder.AppendLine();
			builder.Append(schemaError.ToString());
		}
	}

	private void AppendMetadataItem<T>(StringBuilder builder, T item, Action<EdmSerializationVisitor, T> visitAction) where T : MetadataItem
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
		{
			ConformanceLevel = ConformanceLevel.Fragment,
			Indent = true
		};
		xmlWriterSettings.NewLineChars += "        ";
		builder.Append(xmlWriterSettings.NewLineChars);
		using XmlWriter xmlWriter = XmlWriter.Create(builder, xmlWriterSettings);
		EdmSerializationVisitor arg = new EdmSerializationVisitor(_schemaWriter.Replicate(xmlWriter));
		visitAction(arg, item);
	}

	private void WriteComment(string comment)
	{
		_schemaWriter.WriteComment(comment.Replace("--", "- -"));
	}
}

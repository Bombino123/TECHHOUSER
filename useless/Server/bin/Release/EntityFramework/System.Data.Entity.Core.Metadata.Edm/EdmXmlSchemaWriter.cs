using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class EdmXmlSchemaWriter : XmlSchemaWriter
{
	internal static class SyndicationXmlConstants
	{
		internal const string SyndAuthorEmail = "SyndicationAuthorEmail";

		internal const string SyndAuthorName = "SyndicationAuthorName";

		internal const string SyndAuthorUri = "SyndicationAuthorUri";

		internal const string SyndPublished = "SyndicationPublished";

		internal const string SyndRights = "SyndicationRights";

		internal const string SyndSummary = "SyndicationSummary";

		internal const string SyndTitle = "SyndicationTitle";

		internal const string SyndContributorEmail = "SyndicationContributorEmail";

		internal const string SyndContributorName = "SyndicationContributorName";

		internal const string SyndContributorUri = "SyndicationContributorUri";

		internal const string SyndCategoryLabel = "SyndicationCategoryLabel";

		internal const string SyndContentKindPlaintext = "text";

		internal const string SyndContentKindHtml = "html";

		internal const string SyndContentKindXHtml = "xhtml";

		internal const string SyndUpdated = "SyndicationUpdated";

		internal const string SyndLinkHref = "SyndicationLinkHref";

		internal const string SyndLinkRel = "SyndicationLinkRel";

		internal const string SyndLinkType = "SyndicationLinkType";

		internal const string SyndLinkHrefLang = "SyndicationLinkHrefLang";

		internal const string SyndLinkTitle = "SyndicationLinkTitle";

		internal const string SyndLinkLength = "SyndicationLinkLength";

		internal const string SyndCategoryTerm = "SyndicationCategoryTerm";

		internal const string SyndCategoryScheme = "SyndicationCategoryScheme";
	}

	private readonly bool _serializeDefaultNullability;

	private readonly IDbDependencyResolver _resolver;

	private const string AnnotationNamespacePrefix = "annotation";

	private const string CustomAnnotationNamespacePrefix = "customannotation";

	private const string StoreSchemaGenNamespacePrefix = "store";

	private const string DataServicesPrefix = "m";

	private const string DataServicesNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

	private const string DataServicesMimeTypeAttribute = "System.Data.Services.MimeTypeAttribute";

	private const string DataServicesHasStreamAttribute = "System.Data.Services.Common.HasStreamAttribute";

	private const string DataServicesEntityPropertyMappingAttribute = "System.Data.Services.Common.EntityPropertyMappingAttribute";

	private static readonly string[] _syndicationItemToTargetPath = new string[21]
	{
		string.Empty,
		"SyndicationAuthorEmail",
		"SyndicationAuthorName",
		"SyndicationAuthorUri",
		"SyndicationContributorEmail",
		"SyndicationContributorName",
		"SyndicationContributorUri",
		"SyndicationUpdated",
		"SyndicationPublished",
		"SyndicationRights",
		"SyndicationSummary",
		"SyndicationTitle",
		"SyndicationCategoryLabel",
		"SyndicationCategoryScheme",
		"SyndicationCategoryTerm",
		"SyndicationLinkHref",
		"SyndicationLinkHrefLang",
		"SyndicationLinkLength",
		"SyndicationLinkRel",
		"SyndicationLinkTitle",
		"SyndicationLinkType"
	};

	private static readonly string[] _syndicationTextContentKindToString = new string[3] { "text", "html", "xhtml" };

	private static string SyndicationItemPropertyToString(object value)
	{
		return _syndicationItemToTargetPath[(int)value];
	}

	private static string SyndicationTextContentKindToString(object value)
	{
		return _syndicationTextContentKindToString[(int)value];
	}

	public EdmXmlSchemaWriter()
	{
		_resolver = DbConfiguration.DependencyResolver;
	}

	internal EdmXmlSchemaWriter(XmlWriter xmlWriter, double edmVersion, bool serializeDefaultNullability, IDbDependencyResolver resolver = null)
	{
		_resolver = resolver ?? DbConfiguration.DependencyResolver;
		_serializeDefaultNullability = serializeDefaultNullability;
		_xmlWriter = xmlWriter;
		_version = edmVersion;
	}

	internal virtual void WriteSchemaElementHeader(string schemaNamespace)
	{
		string csdlNamespace = XmlConstants.GetCsdlNamespace(_version);
		_xmlWriter.WriteStartElement("Schema", csdlNamespace);
		_xmlWriter.WriteAttributeString("Namespace", schemaNamespace);
		_xmlWriter.WriteAttributeString("Alias", "Self");
		if (_version == 3.0)
		{
			_xmlWriter.WriteAttributeString("annotation", "UseStrongSpatialTypes", "http://schemas.microsoft.com/ado/2009/02/edm/annotation", "false");
		}
		_xmlWriter.WriteAttributeString("xmlns", "annotation", null, "http://schemas.microsoft.com/ado/2009/02/edm/annotation");
		_xmlWriter.WriteAttributeString("xmlns", "customannotation", null, "http://schemas.microsoft.com/ado/2013/11/edm/customannotation");
	}

	internal virtual void WriteSchemaElementHeader(string schemaNamespace, string provider, string providerManifestToken, bool writeStoreSchemaGenNamespace)
	{
		string ssdlNamespace = XmlConstants.GetSsdlNamespace(_version);
		_xmlWriter.WriteStartElement("Schema", ssdlNamespace);
		_xmlWriter.WriteAttributeString("Namespace", schemaNamespace);
		_xmlWriter.WriteAttributeString("Provider", provider);
		_xmlWriter.WriteAttributeString("ProviderManifestToken", providerManifestToken);
		_xmlWriter.WriteAttributeString("Alias", "Self");
		if (writeStoreSchemaGenNamespace)
		{
			_xmlWriter.WriteAttributeString("xmlns", "store", null, "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator");
		}
		_xmlWriter.WriteAttributeString("xmlns", "customannotation", null, "http://schemas.microsoft.com/ado/2013/11/edm/customannotation");
	}

	private void WritePolymorphicTypeAttributes(EdmType edmType)
	{
		if (edmType.BaseType != null)
		{
			_xmlWriter.WriteAttributeString("BaseType", XmlSchemaWriter.GetQualifiedTypeName("Self", edmType.BaseType.Name));
		}
		if (edmType.Abstract)
		{
			_xmlWriter.WriteAttributeString("Abstract", "true");
		}
	}

	public virtual void WriteFunctionElementHeader(EdmFunction function)
	{
		_xmlWriter.WriteStartElement("Function");
		_xmlWriter.WriteAttributeString("Name", function.Name);
		_xmlWriter.WriteAttributeString("Aggregate", XmlSchemaWriter.GetLowerCaseStringFromBoolValue(function.AggregateAttribute));
		_xmlWriter.WriteAttributeString("BuiltIn", XmlSchemaWriter.GetLowerCaseStringFromBoolValue(function.BuiltInAttribute));
		_xmlWriter.WriteAttributeString("NiladicFunction", XmlSchemaWriter.GetLowerCaseStringFromBoolValue(function.NiladicFunctionAttribute));
		_xmlWriter.WriteAttributeString("IsComposable", XmlSchemaWriter.GetLowerCaseStringFromBoolValue(function.IsComposableAttribute));
		_xmlWriter.WriteAttributeString("ParameterTypeSemantics", function.ParameterTypeSemanticsAttribute.ToString());
		_xmlWriter.WriteAttributeString("Schema", function.Schema);
		if (function.StoreFunctionNameAttribute != null && function.StoreFunctionNameAttribute != function.Name)
		{
			_xmlWriter.WriteAttributeString("StoreFunctionName", function.StoreFunctionNameAttribute);
		}
		if (function.ReturnParameters != null && function.ReturnParameters.Any())
		{
			EdmType edmType = function.ReturnParameters.First().TypeUsage.EdmType;
			if (edmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
			{
				_xmlWriter.WriteAttributeString("ReturnType", GetTypeName(edmType));
			}
		}
	}

	public virtual void WriteFunctionParameterHeader(FunctionParameter functionParameter)
	{
		_xmlWriter.WriteStartElement("Parameter");
		_xmlWriter.WriteAttributeString("Name", functionParameter.Name);
		_xmlWriter.WriteAttributeString("Type", functionParameter.TypeName);
		_xmlWriter.WriteAttributeString("Mode", functionParameter.Mode.ToString());
		if (functionParameter.IsMaxLength)
		{
			_xmlWriter.WriteAttributeString("MaxLength", "Max");
		}
		else if (!functionParameter.IsMaxLengthConstant && functionParameter.MaxLength.HasValue)
		{
			_xmlWriter.WriteAttributeString("MaxLength", functionParameter.MaxLength.Value.ToString(CultureInfo.InvariantCulture));
		}
		if (!functionParameter.IsPrecisionConstant && functionParameter.Precision.HasValue)
		{
			_xmlWriter.WriteAttributeString("Precision", functionParameter.Precision.Value.ToString(CultureInfo.InvariantCulture));
		}
		if (!functionParameter.IsScaleConstant && functionParameter.Scale.HasValue)
		{
			_xmlWriter.WriteAttributeString("Scale", functionParameter.Scale.Value.ToString(CultureInfo.InvariantCulture));
		}
	}

	internal virtual void WriteFunctionReturnTypeElementHeader()
	{
		_xmlWriter.WriteStartElement("ReturnType");
	}

	internal void WriteEntityTypeElementHeader(EntityType entityType)
	{
		_xmlWriter.WriteStartElement("EntityType");
		_xmlWriter.WriteAttributeString("Name", entityType.Name);
		WriteExtendedProperties(entityType);
		if (entityType.Annotations.GetClrAttributes() != null)
		{
			foreach (Attribute clrAttribute in entityType.Annotations.GetClrAttributes())
			{
				if (clrAttribute.GetType().FullName.Equals("System.Data.Services.Common.HasStreamAttribute", StringComparison.Ordinal))
				{
					_xmlWriter.WriteAttributeString("m", "HasStream", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "true");
				}
				else if (clrAttribute.GetType().FullName.Equals("System.Data.Services.MimeTypeAttribute", StringComparison.Ordinal))
				{
					string propertyName = clrAttribute.GetType().GetDeclaredProperty("MemberName").GetValue(clrAttribute, null) as string;
					AddAttributeAnnotation(entityType.Properties.SingleOrDefault((EdmProperty p) => p.Name.Equals(propertyName, StringComparison.Ordinal)), clrAttribute);
				}
				else if (clrAttribute.GetType().FullName.Equals("System.Data.Services.Common.EntityPropertyMappingAttribute", StringComparison.Ordinal))
				{
					string text = clrAttribute.GetType().GetDeclaredProperty("SourcePath").GetValue(clrAttribute, null) as string;
					int num = text.IndexOf("/", StringComparison.Ordinal);
					string propertyName2;
					if (num == -1)
					{
						propertyName2 = text;
					}
					else
					{
						propertyName2 = text.Substring(0, num);
					}
					AddAttributeAnnotation(entityType.Properties.SingleOrDefault((EdmProperty p) => p.Name.Equals(propertyName2, StringComparison.Ordinal)), clrAttribute);
				}
			}
		}
		WritePolymorphicTypeAttributes(entityType);
	}

	internal void WriteEnumTypeElementHeader(EnumType enumType)
	{
		_xmlWriter.WriteStartElement("EnumType");
		_xmlWriter.WriteAttributeString("Name", enumType.Name);
		_xmlWriter.WriteAttributeString("IsFlags", XmlSchemaWriter.GetLowerCaseStringFromBoolValue(enumType.IsFlags));
		WriteExtendedProperties(enumType);
		if (enumType.UnderlyingType != null)
		{
			_xmlWriter.WriteAttributeString("UnderlyingType", enumType.UnderlyingType.PrimitiveTypeKind.ToString());
		}
	}

	internal void WriteEnumTypeMemberElementHeader(EnumMember enumTypeMember)
	{
		_xmlWriter.WriteStartElement("Member");
		_xmlWriter.WriteAttributeString("Name", enumTypeMember.Name);
		_xmlWriter.WriteAttributeString("Value", enumTypeMember.Value.ToString());
	}

	private static void AddAttributeAnnotation(EdmProperty property, Attribute a)
	{
		if (property == null)
		{
			return;
		}
		IList<Attribute> clrAttributes = property.Annotations.GetClrAttributes();
		if (clrAttributes != null)
		{
			if (!clrAttributes.Contains(a))
			{
				clrAttributes.Add(a);
			}
		}
		else
		{
			property.GetMetadataProperties().SetClrAttributes(new List<Attribute> { a });
		}
	}

	internal void WriteComplexTypeElementHeader(ComplexType complexType)
	{
		_xmlWriter.WriteStartElement("ComplexType");
		_xmlWriter.WriteAttributeString("Name", complexType.Name);
		WriteExtendedProperties(complexType);
		WritePolymorphicTypeAttributes(complexType);
	}

	internal virtual void WriteCollectionTypeElementHeader()
	{
		_xmlWriter.WriteStartElement("CollectionType");
	}

	internal virtual void WriteRowTypeElementHeader()
	{
		_xmlWriter.WriteStartElement("RowType");
	}

	internal void WriteAssociationTypeElementHeader(AssociationType associationType)
	{
		_xmlWriter.WriteStartElement("Association");
		_xmlWriter.WriteAttributeString("Name", associationType.Name);
	}

	internal void WriteAssociationEndElementHeader(RelationshipEndMember associationEnd)
	{
		_xmlWriter.WriteStartElement("End");
		_xmlWriter.WriteAttributeString("Role", associationEnd.Name);
		string name = associationEnd.GetEntityType().Name;
		_xmlWriter.WriteAttributeString("Type", XmlSchemaWriter.GetQualifiedTypeName("Self", name));
		_xmlWriter.WriteAttributeString("Multiplicity", RelationshipMultiplicityConverter.MultiplicityToString(associationEnd.RelationshipMultiplicity));
	}

	internal void WriteOperationActionElement(string elementName, OperationAction operationAction)
	{
		_xmlWriter.WriteStartElement(elementName);
		_xmlWriter.WriteAttributeString("Action", operationAction.ToString());
		_xmlWriter.WriteEndElement();
	}

	internal void WriteReferentialConstraintElementHeader()
	{
		_xmlWriter.WriteStartElement("ReferentialConstraint");
	}

	internal void WriteDeclaredKeyPropertiesElementHeader()
	{
		_xmlWriter.WriteStartElement("Key");
	}

	internal void WriteDeclaredKeyPropertyRefElement(EdmProperty property)
	{
		_xmlWriter.WriteStartElement("PropertyRef");
		_xmlWriter.WriteAttributeString("Name", property.Name);
		_xmlWriter.WriteEndElement();
	}

	internal void WritePropertyElementHeader(EdmProperty property)
	{
		_xmlWriter.WriteStartElement("Property");
		_xmlWriter.WriteAttributeString("Name", property.Name);
		_xmlWriter.WriteAttributeString("Type", GetTypeReferenceName(property));
		if (property.CollectionKind != 0)
		{
			_xmlWriter.WriteAttributeString("CollectionKind", property.CollectionKind.ToString());
		}
		if (property.ConcurrencyMode == ConcurrencyMode.Fixed)
		{
			_xmlWriter.WriteAttributeString("ConcurrencyMode", "Fixed");
		}
		WriteExtendedProperties(property);
		if (property.Annotations.GetClrAttributes() != null)
		{
			int num = 0;
			foreach (Attribute clrAttribute in property.Annotations.GetClrAttributes())
			{
				if (clrAttribute.GetType().FullName.Equals("System.Data.Services.MimeTypeAttribute", StringComparison.Ordinal))
				{
					string value = clrAttribute.GetType().GetDeclaredProperty("MimeType").GetValue(clrAttribute, null) as string;
					_xmlWriter.WriteAttributeString("m", "MimeType", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", value);
				}
				else if (clrAttribute.GetType().FullName.Equals("System.Data.Services.Common.EntityPropertyMappingAttribute", StringComparison.Ordinal))
				{
					string text = ((num == 0) ? string.Empty : string.Format(CultureInfo.InvariantCulture, "_{0}", new object[1] { num }));
					string text2 = clrAttribute.GetType().GetDeclaredProperty("SourcePath").GetValue(clrAttribute, null) as string;
					int num2 = text2.IndexOf("/", StringComparison.Ordinal);
					if (num2 != -1 && num2 + 1 < text2.Length)
					{
						_xmlWriter.WriteAttributeString("m", "FC_SourcePath" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", text2.Substring(num2 + 1));
					}
					object value2 = clrAttribute.GetType().GetDeclaredProperty("TargetSyndicationItem").GetValue(clrAttribute, null);
					string value3 = clrAttribute.GetType().GetDeclaredProperty("KeepInContent").GetValue(clrAttribute, null)
						.ToString();
					PropertyInfo declaredProperty = clrAttribute.GetType().GetDeclaredProperty("CriteriaValue");
					string text3 = null;
					if (declaredProperty != null)
					{
						text3 = declaredProperty.GetValue(clrAttribute, null) as string;
					}
					if (text3 != null)
					{
						_xmlWriter.WriteAttributeString("m", "FC_TargetPath" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", SyndicationItemPropertyToString(value2));
						_xmlWriter.WriteAttributeString("m", "FC_KeepInContent" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", value3);
						_xmlWriter.WriteAttributeString("m", "FC_CriteriaValue" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", text3);
					}
					else if (string.Equals(value2.ToString(), "CustomProperty", StringComparison.Ordinal))
					{
						string value4 = clrAttribute.GetType().GetDeclaredProperty("TargetPath").GetValue(clrAttribute, null)
							.ToString();
						string value5 = clrAttribute.GetType().GetDeclaredProperty("TargetNamespacePrefix").GetValue(clrAttribute, null)
							.ToString();
						string value6 = clrAttribute.GetType().GetDeclaredProperty("TargetNamespaceUri").GetValue(clrAttribute, null)
							.ToString();
						_xmlWriter.WriteAttributeString("m", "FC_TargetPath" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", value4);
						_xmlWriter.WriteAttributeString("m", "FC_NsUri" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", value6);
						_xmlWriter.WriteAttributeString("m", "FC_NsPrefix" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", value5);
						_xmlWriter.WriteAttributeString("m", "FC_KeepInContent" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", value3);
					}
					else
					{
						object value7 = clrAttribute.GetType().GetDeclaredProperty("TargetTextContentKind").GetValue(clrAttribute, null);
						_xmlWriter.WriteAttributeString("m", "FC_TargetPath" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", SyndicationItemPropertyToString(value2));
						_xmlWriter.WriteAttributeString("m", "FC_ContentKind" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", SyndicationTextContentKindToString(value7));
						_xmlWriter.WriteAttributeString("m", "FC_KeepInContent" + text, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", value3);
					}
					num++;
				}
			}
		}
		if (property.IsMaxLength)
		{
			_xmlWriter.WriteAttributeString("MaxLength", "Max");
		}
		else if (!property.IsMaxLengthConstant && property.MaxLength.HasValue)
		{
			_xmlWriter.WriteAttributeString("MaxLength", property.MaxLength.Value.ToString(CultureInfo.InvariantCulture));
		}
		if (!property.IsFixedLengthConstant && property.IsFixedLength.HasValue)
		{
			_xmlWriter.WriteAttributeString("FixedLength", XmlSchemaWriter.GetLowerCaseStringFromBoolValue(property.IsFixedLength.Value));
		}
		if (!property.IsUnicodeConstant && property.IsUnicode.HasValue)
		{
			_xmlWriter.WriteAttributeString("Unicode", XmlSchemaWriter.GetLowerCaseStringFromBoolValue(property.IsUnicode.Value));
		}
		if (!property.IsPrecisionConstant && property.Precision.HasValue)
		{
			_xmlWriter.WriteAttributeString("Precision", property.Precision.Value.ToString(CultureInfo.InvariantCulture));
		}
		if (!property.IsScaleConstant && property.Scale.HasValue)
		{
			_xmlWriter.WriteAttributeString("Scale", property.Scale.Value.ToString(CultureInfo.InvariantCulture));
		}
		if (property.StoreGeneratedPattern != 0)
		{
			_xmlWriter.WriteAttributeString("StoreGeneratedPattern", (property.StoreGeneratedPattern == StoreGeneratedPattern.Computed) ? "Computed" : "Identity");
		}
		if (_serializeDefaultNullability || !property.Nullable)
		{
			_xmlWriter.WriteAttributeString("Nullable", XmlSchemaWriter.GetLowerCaseStringFromBoolValue(property.Nullable));
		}
		if (property.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2009/02/edm/annotation:StoreGeneratedPattern", ignoreCase: false, out var item))
		{
			_xmlWriter.WriteAttributeString("StoreGeneratedPattern", "http://schemas.microsoft.com/ado/2009/02/edm/annotation", item.Value.ToString());
		}
	}

	private static string GetTypeReferenceName(EdmProperty property)
	{
		if (property.IsPrimitiveType)
		{
			return property.TypeName;
		}
		if (property.IsComplexType)
		{
			return XmlSchemaWriter.GetQualifiedTypeName("Self", property.ComplexType.Name);
		}
		return XmlSchemaWriter.GetQualifiedTypeName("Self", property.EnumType.Name);
	}

	internal void WriteNavigationPropertyElementHeader(NavigationProperty member)
	{
		_xmlWriter.WriteStartElement("NavigationProperty");
		_xmlWriter.WriteAttributeString("Name", member.Name);
		_xmlWriter.WriteAttributeString("Relationship", XmlSchemaWriter.GetQualifiedTypeName("Self", member.Association.Name));
		_xmlWriter.WriteAttributeString("FromRole", member.GetFromEnd().Name);
		_xmlWriter.WriteAttributeString("ToRole", member.ToEndMember.Name);
	}

	internal void WriteReferentialConstraintRoleElement(string roleName, RelationshipEndMember edmAssociationEnd, IEnumerable<EdmProperty> properties)
	{
		_xmlWriter.WriteStartElement(roleName);
		_xmlWriter.WriteAttributeString("Role", edmAssociationEnd.Name);
		foreach (EdmProperty property in properties)
		{
			_xmlWriter.WriteStartElement("PropertyRef");
			_xmlWriter.WriteAttributeString("Name", property.Name);
			_xmlWriter.WriteEndElement();
		}
		_xmlWriter.WriteEndElement();
	}

	internal virtual void WriteEntityContainerElementHeader(EntityContainer container)
	{
		_xmlWriter.WriteStartElement("EntityContainer");
		_xmlWriter.WriteAttributeString("Name", container.Name);
		WriteExtendedProperties(container);
	}

	internal void WriteAssociationSetElementHeader(AssociationSet associationSet)
	{
		_xmlWriter.WriteStartElement("AssociationSet");
		_xmlWriter.WriteAttributeString("Name", associationSet.Name);
		_xmlWriter.WriteAttributeString("Association", XmlSchemaWriter.GetQualifiedTypeName("Self", associationSet.ElementType.Name));
	}

	internal void WriteAssociationSetEndElement(EntitySet end, string roleName)
	{
		_xmlWriter.WriteStartElement("End");
		_xmlWriter.WriteAttributeString("Role", roleName);
		_xmlWriter.WriteAttributeString("EntitySet", end.Name);
		_xmlWriter.WriteEndElement();
	}

	internal virtual void WriteEntitySetElementHeader(EntitySet entitySet)
	{
		_xmlWriter.WriteStartElement("EntitySet");
		_xmlWriter.WriteAttributeString("Name", entitySet.Name);
		_xmlWriter.WriteAttributeString("EntityType", XmlSchemaWriter.GetQualifiedTypeName("Self", entitySet.ElementType.Name));
		if (!string.IsNullOrWhiteSpace(entitySet.Schema))
		{
			_xmlWriter.WriteAttributeString("Schema", entitySet.Schema);
		}
		if (!string.IsNullOrWhiteSpace(entitySet.Table))
		{
			_xmlWriter.WriteAttributeString("Table", entitySet.Table);
		}
		WriteExtendedProperties(entitySet);
	}

	internal virtual void WriteFunctionImportElementHeader(EdmFunction functionImport)
	{
		_xmlWriter.WriteStartElement("FunctionImport");
		_xmlWriter.WriteAttributeString("Name", functionImport.Name);
		if (functionImport.IsComposableAttribute)
		{
			_xmlWriter.WriteAttributeString("IsComposable", "true");
		}
	}

	internal virtual void WriteFunctionImportReturnTypeAttributes(FunctionParameter returnParameter, EntitySet entitySet, bool inline)
	{
		_xmlWriter.WriteAttributeString(inline ? "ReturnType" : "Type", GetTypeName(returnParameter.TypeUsage.EdmType));
		if (entitySet != null)
		{
			_xmlWriter.WriteAttributeString("EntitySet", entitySet.Name);
		}
	}

	internal virtual void WriteFunctionImportParameterElementHeader(FunctionParameter parameter)
	{
		_xmlWriter.WriteStartElement("Parameter");
		_xmlWriter.WriteAttributeString("Name", parameter.Name);
		_xmlWriter.WriteAttributeString("Mode", parameter.Mode.ToString());
		_xmlWriter.WriteAttributeString("Type", GetTypeName(parameter.TypeUsage.EdmType));
	}

	internal void WriteDefiningQuery(EntitySet entitySet)
	{
		if (!string.IsNullOrWhiteSpace(entitySet.DefiningQuery))
		{
			_xmlWriter.WriteElementString("DefiningQuery", entitySet.DefiningQuery);
		}
	}

	internal EdmXmlSchemaWriter Replicate(XmlWriter xmlWriter)
	{
		return new EdmXmlSchemaWriter(xmlWriter, _version, _serializeDefaultNullability);
	}

	internal void WriteExtendedProperties(MetadataItem item)
	{
		foreach (MetadataProperty item2 in item.MetadataProperties.Where((MetadataProperty p) => p.PropertyKind == PropertyKind.Extended))
		{
			if (TrySplitExtendedMetadataPropertyName(item2.Name, out var xmlNamespaceUri, out var attributeName) && item2.Name != "http://schemas.microsoft.com/ado/2009/02/edm/annotation:StoreGeneratedPattern")
			{
				Func<IMetadataAnnotationSerializer> service = _resolver.GetService<Func<IMetadataAnnotationSerializer>>(attributeName);
				string value = ((service == null) ? item2.Value.ToString() : service().Serialize(attributeName, item2.Value));
				_xmlWriter.WriteAttributeString(attributeName, xmlNamespaceUri, value);
			}
		}
	}

	private static bool TrySplitExtendedMetadataPropertyName(string name, out string xmlNamespaceUri, out string attributeName)
	{
		int num = name.LastIndexOf(':');
		if (num < 1 || name.Length <= num + 1)
		{
			xmlNamespaceUri = null;
			attributeName = null;
			return false;
		}
		xmlNamespaceUri = name.Substring(0, num);
		attributeName = name.Substring(num + 1, name.Length - 1 - num);
		return true;
	}

	private static string GetTypeName(EdmType type)
	{
		if (type.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
		{
			return string.Format(CultureInfo.InvariantCulture, "Collection({0})", new object[1] { GetTypeName(((CollectionType)type).TypeUsage.EdmType) });
		}
		if (type.BuiltInTypeKind != BuiltInTypeKind.PrimitiveType)
		{
			return type.FullName;
		}
		return type.Name;
	}
}

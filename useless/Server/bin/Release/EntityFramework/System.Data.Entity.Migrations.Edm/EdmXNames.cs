using System.Collections.Generic;
using System.Xml.Linq;

namespace System.Data.Entity.Migrations.Edm;

internal static class EdmXNames
{
	public static class Csdl
	{
		public static readonly IEnumerable<XName> AssociationNames = Names("Association");

		public static readonly IEnumerable<XName> ComplexTypeNames = Names("ComplexType");

		public static readonly IEnumerable<XName> EndNames = Names("End");

		public static readonly IEnumerable<XName> EntityContainerNames = Names("EntityContainer");

		public static readonly IEnumerable<XName> EntitySetNames = Names("EntitySet");

		public static readonly IEnumerable<XName> EntityTypeNames = Names("EntityType");

		public static readonly IEnumerable<XName> NavigationPropertyNames = Names("NavigationProperty");

		public static readonly IEnumerable<XName> PropertyNames = Names("Property");

		public static readonly IEnumerable<XName> SchemaNames = Names("Schema");

		private static IEnumerable<XName> Names(string elementName)
		{
			return new List<XName>
			{
				_csdlNamespaceV3 + elementName,
				_csdlNamespaceV2 + elementName
			};
		}
	}

	public static class Msl
	{
		public static readonly IEnumerable<XName> AssociationSetMappingNames = Names("AssociationSetMapping");

		public static readonly IEnumerable<XName> ComplexPropertyNames = Names("ComplexProperty");

		public static readonly IEnumerable<XName> ConditionNames = Names("Condition");

		public static readonly IEnumerable<XName> EntityContainerMappingNames = Names("EntityContainerMapping");

		public static readonly IEnumerable<XName> EntitySetMappingNames = Names("EntitySetMapping");

		public static readonly IEnumerable<XName> EntityTypeMappingNames = Names("EntityTypeMapping");

		public static readonly IEnumerable<XName> MappingNames = Names("Mapping");

		public static readonly IEnumerable<XName> MappingFragmentNames = Names("MappingFragment");

		public static readonly IEnumerable<XName> ScalarPropertyNames = Names("ScalarProperty");

		private static IEnumerable<XName> Names(string elementName)
		{
			return new List<XName>
			{
				_mslNamespaceV3 + elementName,
				_mslNamespaceV2 + elementName
			};
		}
	}

	public static class Ssdl
	{
		public static readonly IEnumerable<XName> AssociationNames = Names("Association");

		public static readonly IEnumerable<XName> DependentNames = Names("Dependent");

		public static readonly IEnumerable<XName> EndNames = Names("End");

		public static readonly IEnumerable<XName> EntityContainerNames = Names("EntityContainer");

		public static readonly IEnumerable<XName> EntitySetNames = Names("EntitySet");

		public static readonly IEnumerable<XName> EntityTypeNames = Names("EntityType");

		public static readonly IEnumerable<XName> KeyNames = Names("Key");

		public static readonly IEnumerable<XName> OnDeleteNames = Names("OnDelete");

		public static readonly IEnumerable<XName> PrincipalNames = Names("Principal");

		public static readonly IEnumerable<XName> PropertyNames = Names("Property");

		public static readonly IEnumerable<XName> PropertyRefNames = Names("PropertyRef");

		public static readonly IEnumerable<XName> SchemaNames = Names("Schema");

		private static IEnumerable<XName> Names(string elementName)
		{
			return new List<XName>
			{
				_ssdlNamespaceV3 + elementName,
				_ssdlNamespaceV2 + elementName
			};
		}
	}

	private static readonly XNamespace _csdlNamespaceV2 = XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/edm");

	private static readonly XNamespace _mslNamespaceV2 = XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/mapping/cs");

	private static readonly XNamespace _ssdlNamespaceV2 = XNamespace.Get("http://schemas.microsoft.com/ado/2009/02/edm/ssdl");

	private static readonly XNamespace _csdlNamespaceV3 = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm");

	private static readonly XNamespace _mslNamespaceV3 = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/mapping/cs");

	private static readonly XNamespace _ssdlNamespaceV3 = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm/ssdl");

	public static string ActionAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Action"));
	}

	public static string ColumnNameAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("ColumnName"));
	}

	public static string EntitySetAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("EntitySet"));
	}

	public static string NameAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Name"));
	}

	public static string NamespaceAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Namespace"));
	}

	public static string EntityTypeAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("EntityType"));
	}

	public static string FromRoleAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("FromRole"));
	}

	public static string ToRoleAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("ToRole"));
	}

	public static string NullableAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Nullable"));
	}

	public static string MaxLengthAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("MaxLength"));
	}

	public static string MultiplicityAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Multiplicity"));
	}

	public static string FixedLengthAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("FixedLength"));
	}

	public static string PrecisionAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Precision"));
	}

	public static string ProviderAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Provider"));
	}

	public static string ProviderManifestTokenAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("ProviderManifestToken"));
	}

	public static string RelationshipAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Relationship"));
	}

	public static string ScaleAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Scale"));
	}

	public static string StoreGeneratedPatternAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("StoreGeneratedPattern"));
	}

	public static string UnicodeAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Unicode"));
	}

	public static string RoleAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Role"));
	}

	public static string SchemaAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Schema"));
	}

	public static string StoreEntitySetAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("StoreEntitySet"));
	}

	public static string TableAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Table"));
	}

	public static string TypeAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Type"));
	}

	public static string TypeNameAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("TypeName"));
	}

	public static string ValueAttribute(this XElement element)
	{
		return (string)element.Attribute(XName.op_Implicit("Value"));
	}
}

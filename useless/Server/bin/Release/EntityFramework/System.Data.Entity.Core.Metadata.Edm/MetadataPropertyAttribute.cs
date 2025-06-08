using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class MetadataPropertyAttribute : Attribute
{
	private readonly EdmType _type;

	private readonly bool _isCollectionType;

	internal EdmType Type => _type;

	internal bool IsCollectionType => _isCollectionType;

	internal MetadataPropertyAttribute(BuiltInTypeKind builtInTypeKind, bool isCollectionType)
		: this(MetadataItem.GetBuiltInType(builtInTypeKind), isCollectionType)
	{
	}

	internal MetadataPropertyAttribute(PrimitiveTypeKind primitiveTypeKind, bool isCollectionType)
		: this(MetadataItem.EdmProviderManifest.GetPrimitiveType(primitiveTypeKind), isCollectionType)
	{
	}

	internal MetadataPropertyAttribute(Type type, bool isCollection)
		: this(ClrComplexType.CreateReadonlyClrComplexType(type, type.NestingNamespace() ?? string.Empty, type.Name), isCollection)
	{
	}

	private MetadataPropertyAttribute(EdmType type, bool isCollectionType)
	{
		_type = type;
		_isCollectionType = isCollectionType;
	}
}

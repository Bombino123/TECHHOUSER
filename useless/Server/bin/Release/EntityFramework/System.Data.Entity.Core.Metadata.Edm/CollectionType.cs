using System.Data.Entity.Utilities;
using System.Text;

namespace System.Data.Entity.Core.Metadata.Edm;

public class CollectionType : EdmType
{
	private readonly TypeUsage _typeUsage;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.CollectionType;

	[MetadataProperty(BuiltInTypeKind.TypeUsage, false)]
	public virtual TypeUsage TypeUsage => _typeUsage;

	internal CollectionType()
	{
	}

	internal CollectionType(EdmType elementType)
		: this(TypeUsage.Create(elementType))
	{
		DataSpace = elementType.DataSpace;
	}

	internal CollectionType(TypeUsage elementType)
		: base(GetIdentity(Check.NotNull(elementType, "elementType")), "Transient", elementType.EdmType.DataSpace)
	{
		_typeUsage = elementType;
		SetReadOnly();
	}

	private static string GetIdentity(TypeUsage typeUsage)
	{
		StringBuilder stringBuilder = new StringBuilder(50);
		stringBuilder.Append("collection[");
		typeUsage.BuildIdentity(stringBuilder);
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	internal override bool EdmEquals(MetadataItem item)
	{
		if (this == item)
		{
			return true;
		}
		if (item == null || BuiltInTypeKind.CollectionType != item.BuiltInTypeKind)
		{
			return false;
		}
		CollectionType collectionType = (CollectionType)item;
		return TypeUsage.EdmEquals(collectionType.TypeUsage);
	}
}

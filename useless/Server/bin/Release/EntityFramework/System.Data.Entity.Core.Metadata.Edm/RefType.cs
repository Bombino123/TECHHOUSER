using System.Data.Entity.Utilities;
using System.Text;

namespace System.Data.Entity.Core.Metadata.Edm;

public class RefType : EdmType
{
	private readonly EntityTypeBase _elementType;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.RefType;

	[MetadataProperty(BuiltInTypeKind.EntityTypeBase, false)]
	public virtual EntityTypeBase ElementType => _elementType;

	internal RefType()
	{
	}

	internal RefType(EntityType entityType)
		: base(GetIdentity(Check.NotNull(entityType, "entityType")), "Transient", entityType.DataSpace)
	{
		_elementType = entityType;
		SetReadOnly();
	}

	private static string GetIdentity(EntityTypeBase entityTypeBase)
	{
		StringBuilder stringBuilder = new StringBuilder(50);
		stringBuilder.Append("reference[");
		entityTypeBase.BuildIdentity(stringBuilder);
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public override int GetHashCode()
	{
		return (_elementType.GetHashCode() * 397) ^ typeof(RefType).GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is RefType refType)
		{
			return refType._elementType == _elementType;
		}
		return false;
	}
}

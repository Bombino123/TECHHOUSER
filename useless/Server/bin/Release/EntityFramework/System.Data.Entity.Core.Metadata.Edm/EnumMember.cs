using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

public sealed class EnumMember : MetadataItem
{
	private readonly string _name;

	private readonly object _value;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.EnumMember;

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public string Name => _name;

	[MetadataProperty(BuiltInTypeKind.PrimitiveType, false)]
	public object Value => _value;

	internal override string Identity => Name;

	internal EnumMember(string name, object value)
		: base(MetadataFlags.Readonly)
	{
		Check.NotEmpty(name, "name");
		_name = name;
		_value = value;
	}

	public override string ToString()
	{
		return Name;
	}

	[CLSCompliant(false)]
	public static EnumMember Create(string name, sbyte value, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		return CreateInternal(name, value, metadataProperties);
	}

	public static EnumMember Create(string name, byte value, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		return CreateInternal(name, value, metadataProperties);
	}

	public static EnumMember Create(string name, short value, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		return CreateInternal(name, value, metadataProperties);
	}

	public static EnumMember Create(string name, int value, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		return CreateInternal(name, value, metadataProperties);
	}

	public static EnumMember Create(string name, long value, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		return CreateInternal(name, value, metadataProperties);
	}

	private static EnumMember CreateInternal(string name, object value, IEnumerable<MetadataProperty> metadataProperties)
	{
		EnumMember enumMember = new EnumMember(name, value);
		if (metadataProperties != null)
		{
			enumMember.AddMetadataProperties(metadataProperties);
		}
		enumMember.SetReadOnly();
		return enumMember;
	}
}

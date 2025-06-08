using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

public class EnumType : SimpleType
{
	private readonly ReadOnlyMetadataCollection<EnumMember> _members = new ReadOnlyMetadataCollection<EnumMember>(new MetadataCollection<EnumMember>());

	private PrimitiveType _underlyingType;

	private bool _isFlags;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.EnumType;

	[MetadataProperty(BuiltInTypeKind.EnumMember, true)]
	public ReadOnlyMetadataCollection<EnumMember> Members => _members;

	[MetadataProperty(PrimitiveTypeKind.Boolean, false)]
	public bool IsFlags
	{
		get
		{
			return _isFlags;
		}
		internal set
		{
			Util.ThrowIfReadOnly(this);
			_isFlags = value;
		}
	}

	[MetadataProperty(BuiltInTypeKind.PrimitiveType, false)]
	public PrimitiveType UnderlyingType
	{
		get
		{
			return _underlyingType;
		}
		internal set
		{
			Util.ThrowIfReadOnly(this);
			_underlyingType = value;
		}
	}

	internal EnumType()
	{
		_underlyingType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);
		_isFlags = false;
	}

	internal EnumType(string name, string namespaceName, PrimitiveType underlyingType, bool isFlags, DataSpace dataSpace)
		: base(name, namespaceName, dataSpace)
	{
		_isFlags = isFlags;
		_underlyingType = underlyingType;
	}

	internal EnumType(Type clrType)
		: base(clrType.Name, clrType.NestingNamespace() ?? string.Empty, DataSpace.OSpace)
	{
		ClrProviderManifest.Instance.TryGetPrimitiveType(clrType.GetEnumUnderlyingType(), out _underlyingType);
		_isFlags = clrType.GetCustomAttributes<FlagsAttribute>(inherit: false).Any();
		string[] names = Enum.GetNames(clrType);
		foreach (string text in names)
		{
			AddMember(new EnumMember(text, Convert.ChangeType(Enum.Parse(clrType, text), clrType.GetEnumUnderlyingType(), CultureInfo.InvariantCulture)));
		}
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
			Members.Source.SetReadOnly();
		}
	}

	internal void AddMember(EnumMember enumMember)
	{
		Members.Source.Add(enumMember);
	}

	public static EnumType Create(string name, string namespaceName, PrimitiveType underlyingType, bool isFlags, IEnumerable<EnumMember> members, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(namespaceName, "namespaceName");
		Check.NotNull(underlyingType, "underlyingType");
		if (!Helper.IsSupportedEnumUnderlyingType(underlyingType.PrimitiveTypeKind))
		{
			throw new ArgumentException(Strings.InvalidEnumUnderlyingType, "underlyingType");
		}
		EnumType enumType = new EnumType(name, namespaceName, underlyingType, isFlags, DataSpace.CSpace);
		if (members != null)
		{
			foreach (EnumMember member in members)
			{
				if (!Helper.IsEnumMemberValueInRange(underlyingType.PrimitiveTypeKind, Convert.ToInt64(member.Value, CultureInfo.InvariantCulture)))
				{
					throw new ArgumentException(Strings.EnumMemberValueOutOfItsUnderylingTypeRange(member.Value, member.Name, underlyingType.Name), "members");
				}
				enumType.AddMember(member);
			}
		}
		if (metadataProperties != null)
		{
			enumType.AddMetadataProperties(metadataProperties);
		}
		enumType.SetReadOnly();
		return enumType;
	}
}

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class MetadataEnumMember : MetadataMember
{
	internal readonly TypeUsage EnumType;

	internal readonly EnumMember EnumMember;

	internal override string MetadataMemberClassName => EnumMemberClassName;

	internal static string EnumMemberClassName => Strings.LocalizedEnumMember;

	internal MetadataEnumMember(string name, TypeUsage enumType, EnumMember enumMember)
		: base(MetadataMemberClass.EnumMember, name)
	{
		EnumType = enumType;
		EnumMember = enumMember;
	}
}

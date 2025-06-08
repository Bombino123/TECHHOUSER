using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class MetadataType : MetadataMember
{
	internal readonly TypeUsage TypeUsage;

	internal override string MetadataMemberClassName => TypeClassName;

	internal static string TypeClassName => Strings.LocalizedType;

	internal MetadataType(string name, TypeUsage typeUsage)
		: base(MetadataMemberClass.Type, name)
	{
		TypeUsage = typeUsage;
	}
}

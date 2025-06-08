using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class MetadataNamespace : MetadataMember
{
	internal override string MetadataMemberClassName => NamespaceClassName;

	internal static string NamespaceClassName => Strings.LocalizedNamespace;

	internal MetadataNamespace(string name)
		: base(MetadataMemberClass.Namespace, name)
	{
	}
}

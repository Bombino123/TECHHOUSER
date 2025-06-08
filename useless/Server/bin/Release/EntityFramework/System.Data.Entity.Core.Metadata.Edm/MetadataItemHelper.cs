using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class MetadataItemHelper
{
	internal const string SchemaErrorsMetadataPropertyName = "EdmSchemaErrors";

	internal const string SchemaInvalidMetadataPropertyName = "EdmSchemaInvalid";

	public static bool IsInvalid(MetadataItem instance)
	{
		if (!instance.MetadataProperties.TryGetValue("EdmSchemaInvalid", ignoreCase: false, out var item) || item == null)
		{
			return false;
		}
		return (bool)item.Value;
	}

	public static bool HasSchemaErrors(MetadataItem instance)
	{
		return instance.MetadataProperties.Contains("EdmSchemaErrors");
	}

	public static IEnumerable<EdmSchemaError> GetSchemaErrors(MetadataItem instance)
	{
		if (!instance.MetadataProperties.TryGetValue("EdmSchemaErrors", ignoreCase: false, out var item) || item == null)
		{
			return Enumerable.Empty<EdmSchemaError>();
		}
		return (IEnumerable<EdmSchemaError>)item.Value;
	}
}

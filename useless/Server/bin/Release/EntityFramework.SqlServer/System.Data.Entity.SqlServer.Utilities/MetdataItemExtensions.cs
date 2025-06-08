using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.SqlServer.Utilities;

internal static class MetdataItemExtensions
{
	public static T GetMetadataPropertyValue<T>(this MetadataItem item, string propertyName)
	{
		MetadataProperty val = ((IEnumerable<MetadataProperty>)item.MetadataProperties).FirstOrDefault((Func<MetadataProperty, bool>)((MetadataProperty p) => p.Name == propertyName));
		if (val != null)
		{
			return (T)val.Value;
		}
		return default(T);
	}
}

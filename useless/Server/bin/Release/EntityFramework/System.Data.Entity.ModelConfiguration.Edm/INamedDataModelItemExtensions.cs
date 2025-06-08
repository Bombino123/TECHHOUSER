using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class INamedDataModelItemExtensions
{
	public static string UniquifyName(this IEnumerable<INamedDataModelItem> namedDataModelItems, string name)
	{
		return namedDataModelItems.Select((INamedDataModelItem i) => i.Name).Uniquify(name);
	}
}

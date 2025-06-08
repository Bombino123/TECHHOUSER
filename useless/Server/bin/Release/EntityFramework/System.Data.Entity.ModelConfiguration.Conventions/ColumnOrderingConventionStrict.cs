using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ColumnOrderingConventionStrict : ColumnOrderingConvention
{
	protected override void ValidateColumns(EntityType table, string tableName)
	{
		if ((from c in table.Properties
			select c.GetOrder() into o
			where o.HasValue
			group o by o).Any((IGrouping<int?, int?> g) => g.Count() > 1))
		{
			throw Error.DuplicateConfiguredColumnOrder(tableName);
		}
	}
}

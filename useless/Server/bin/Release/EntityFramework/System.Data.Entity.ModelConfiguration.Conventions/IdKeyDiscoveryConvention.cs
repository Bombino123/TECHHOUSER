using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class IdKeyDiscoveryConvention : KeyDiscoveryConvention
{
	private const string Id = "Id";

	protected override IEnumerable<EdmProperty> MatchKeyProperty(EntityType entityType, IEnumerable<EdmProperty> primitiveProperties)
	{
		Check.NotNull(entityType, "entityType");
		Check.NotNull(primitiveProperties, "primitiveProperties");
		IEnumerable<EdmProperty> enumerable = primitiveProperties.Where((EdmProperty p) => "Id".Equals(p.Name, StringComparison.OrdinalIgnoreCase));
		if (!enumerable.Any())
		{
			enumerable = primitiveProperties.Where((EdmProperty p) => (entityType.Name + "Id").Equals(p.Name, StringComparison.OrdinalIgnoreCase));
		}
		if (enumerable.Count() > 1)
		{
			throw Error.MultiplePropertiesMatchedAsKeys(enumerable.First().Name, entityType.Name);
		}
		return enumerable;
	}
}

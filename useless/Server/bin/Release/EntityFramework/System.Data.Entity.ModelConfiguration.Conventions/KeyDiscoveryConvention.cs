using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public abstract class KeyDiscoveryConvention : IConceptualModelConvention<EntityType>, IConvention
{
	public virtual void Apply(EntityType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		if (item.KeyProperties.Count > 0 || item.BaseType != null)
		{
			return;
		}
		foreach (EdmProperty item2 in MatchKeyProperty(item, item.GetDeclaredPrimitiveProperties()))
		{
			item2.Nullable = false;
			item.AddKeyMember(item2);
		}
	}

	protected abstract IEnumerable<EdmProperty> MatchKeyProperty(EntityType entityType, IEnumerable<EdmProperty> primitiveProperties);
}

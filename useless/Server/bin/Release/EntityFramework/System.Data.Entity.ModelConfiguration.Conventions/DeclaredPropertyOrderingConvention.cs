using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Mappers;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class DeclaredPropertyOrderingConvention : IConceptualModelConvention<EntityType>, IConvention
{
	public virtual void Apply(EntityType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		if (item.BaseType != null)
		{
			return;
		}
		foreach (EdmProperty keyProperty in item.KeyProperties)
		{
			item.RemoveMember(keyProperty);
			item.AddKeyMember(keyProperty);
		}
		foreach (PropertyInfo p in new PropertyFilter().GetProperties(item.GetClrType(), declaredOnly: false, null, null, includePrivate: true))
		{
			EdmProperty edmProperty = item.DeclaredProperties.SingleOrDefault((EdmProperty ep) => ep.Name == p.Name);
			if (edmProperty != null && !item.KeyProperties.Contains(edmProperty))
			{
				item.RemoveMember(edmProperty);
				item.AddMember(edmProperty);
			}
		}
	}
}

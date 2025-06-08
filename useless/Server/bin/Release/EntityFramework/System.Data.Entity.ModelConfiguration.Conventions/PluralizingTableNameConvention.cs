using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class PluralizingTableNameConvention : IStoreModelConvention<EntityType>, IConvention
{
	private IPluralizationService _pluralizationService = DbConfiguration.DependencyResolver.GetService<IPluralizationService>();

	public virtual void Apply(EntityType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		_pluralizationService = DbConfiguration.DependencyResolver.GetService<IPluralizationService>();
		if (item.GetTableName() == null)
		{
			EntitySet entitySet = model.StoreModel.GetEntitySet(item);
			entitySet.Table = (from n in (from es in model.StoreModel.GetEntitySets()
					where es.Schema == entitySet.Schema
					select es).Except(new EntitySet[1] { entitySet })
				select n.Table).Uniquify(_pluralizationService.Pluralize(entitySet.Table));
		}
	}
}

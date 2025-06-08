using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class PluralizingEntitySetNameConvention : IConceptualModelConvention<EntitySet>, IConvention
{
	private static readonly IPluralizationService _pluralizationService = DbConfiguration.DependencyResolver.GetService<IPluralizationService>();

	public virtual void Apply(EntitySet item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		if (item.GetConfiguration() == null)
		{
			item.Name = model.ConceptualModel.GetEntitySets().Except(new EntitySet[1] { item }).UniquifyName(_pluralizationService.Pluralize(item.Name));
		}
	}
}

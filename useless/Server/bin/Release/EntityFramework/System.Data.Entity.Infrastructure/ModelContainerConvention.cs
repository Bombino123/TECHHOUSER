using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure;

public class ModelContainerConvention : IConceptualModelConvention<EntityContainer>, IConvention
{
	private readonly string _containerName;

	internal ModelContainerConvention(string containerName)
	{
		_containerName = containerName;
	}

	public virtual void Apply(EntityContainer item, DbModel model)
	{
		Check.NotNull(model, "model");
		item.Name = _containerName;
	}
}

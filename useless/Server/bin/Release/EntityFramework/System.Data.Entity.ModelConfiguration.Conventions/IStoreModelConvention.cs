using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public interface IStoreModelConvention<T> : IConvention where T : MetadataItem
{
	void Apply(T item, DbModel model);
}

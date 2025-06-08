using System.Data.Entity.Internal;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure;

[Obsolete("The IncludeMetadataConvention is no longer used. EdmMetadata is not included in the model. <see cref=\"EdmModelDiffer\" /> is now used to detect changes in the model.")]
public class IncludeMetadataConvention : Convention
{
	internal virtual void Apply(System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		Check.NotNull(modelConfiguration, "modelConfiguration");
		EdmMetadataContext.ConfigureEdmMetadata(modelConfiguration);
	}
}

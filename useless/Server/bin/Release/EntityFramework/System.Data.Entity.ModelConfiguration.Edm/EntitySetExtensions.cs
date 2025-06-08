using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class EntitySetExtensions
{
	public static object GetConfiguration(this EntitySet entitySet)
	{
		return entitySet.Annotations.GetConfiguration();
	}

	public static void SetConfiguration(this EntitySet entitySet, object configuration)
	{
		entitySet.GetMetadataProperties().SetConfiguration(configuration);
	}
}

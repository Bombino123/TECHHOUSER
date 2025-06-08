using System.Data.Entity.Core.Mapping;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class StorageAssociationSetMappingExtensions
{
	public static AssociationSetMapping Initialize(this AssociationSetMapping associationSetMapping)
	{
		associationSetMapping.SourceEndMapping = new EndPropertyMapping();
		associationSetMapping.TargetEndMapping = new EndPropertyMapping();
		return associationSetMapping;
	}

	public static object GetConfiguration(this AssociationSetMapping associationSetMapping)
	{
		return associationSetMapping.Annotations.GetConfiguration();
	}

	public static void SetConfiguration(this AssociationSetMapping associationSetMapping, object configuration)
	{
		associationSetMapping.Annotations.SetConfiguration(configuration);
	}
}

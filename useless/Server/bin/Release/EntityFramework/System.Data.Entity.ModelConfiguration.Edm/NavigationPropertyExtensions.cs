using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class NavigationPropertyExtensions
{
	public static object GetConfiguration(this NavigationProperty navigationProperty)
	{
		return navigationProperty.Annotations.GetConfiguration();
	}

	public static void SetConfiguration(this NavigationProperty navigationProperty, object configuration)
	{
		navigationProperty.GetMetadataProperties().SetConfiguration(configuration);
	}

	public static AssociationEndMember GetFromEnd(this NavigationProperty navProp)
	{
		if (navProp.Association.SourceEnd != navProp.ResultEnd)
		{
			return navProp.Association.SourceEnd;
		}
		return navProp.Association.TargetEnd;
	}
}

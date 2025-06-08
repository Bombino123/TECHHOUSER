using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class NavigationPropertyNameForeignKeyDiscoveryConvention : ForeignKeyDiscoveryConvention
{
	protected override bool SupportsMultipleAssociations => true;

	protected override bool MatchDependentKeyProperty(AssociationType associationType, AssociationEndMember dependentAssociationEnd, EdmProperty dependentProperty, EntityType principalEntityType, EdmProperty principalKeyProperty)
	{
		Check.NotNull(associationType, "associationType");
		Check.NotNull(dependentAssociationEnd, "dependentAssociationEnd");
		Check.NotNull(dependentProperty, "dependentProperty");
		Check.NotNull(principalEntityType, "principalEntityType");
		Check.NotNull(principalKeyProperty, "principalKeyProperty");
		AssociationEndMember otherEnd = associationType.GetOtherEnd(dependentAssociationEnd);
		NavigationProperty navigationProperty = dependentAssociationEnd.GetEntityType().NavigationProperties.SingleOrDefault((NavigationProperty n) => n.ResultEnd == otherEnd);
		if (navigationProperty == null)
		{
			return false;
		}
		return string.Equals(dependentProperty.Name, navigationProperty.Name + principalKeyProperty.Name, StringComparison.OrdinalIgnoreCase);
	}
}

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class PrimaryKeyNameForeignKeyDiscoveryConvention : ForeignKeyDiscoveryConvention
{
	protected override bool MatchDependentKeyProperty(AssociationType associationType, AssociationEndMember dependentAssociationEnd, EdmProperty dependentProperty, EntityType principalEntityType, EdmProperty principalKeyProperty)
	{
		Check.NotNull(associationType, "associationType");
		Check.NotNull(dependentAssociationEnd, "dependentAssociationEnd");
		Check.NotNull(dependentProperty, "dependentProperty");
		Check.NotNull(principalEntityType, "principalEntityType");
		Check.NotNull(principalKeyProperty, "principalKeyProperty");
		return string.Equals(dependentProperty.Name, principalKeyProperty.Name, StringComparison.OrdinalIgnoreCase);
	}
}

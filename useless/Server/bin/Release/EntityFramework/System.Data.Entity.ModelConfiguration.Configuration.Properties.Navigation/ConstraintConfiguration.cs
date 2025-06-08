using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Types;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;

internal abstract class ConstraintConfiguration
{
	public virtual bool IsFullySpecified => true;

	internal abstract ConstraintConfiguration Clone();

	internal abstract void Configure(AssociationType associationType, AssociationEndMember dependentEnd, EntityTypeConfiguration entityTypeConfiguration);
}

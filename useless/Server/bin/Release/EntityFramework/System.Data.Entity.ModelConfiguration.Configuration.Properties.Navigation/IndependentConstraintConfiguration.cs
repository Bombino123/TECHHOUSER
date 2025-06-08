using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;

internal class IndependentConstraintConfiguration : ConstraintConfiguration
{
	private static readonly ConstraintConfiguration _instance = new IndependentConstraintConfiguration();

	public static ConstraintConfiguration Instance => _instance;

	private IndependentConstraintConfiguration()
	{
	}

	internal override ConstraintConfiguration Clone()
	{
		return _instance;
	}

	internal override void Configure(AssociationType associationType, AssociationEndMember dependentEnd, EntityTypeConfiguration entityTypeConfiguration)
	{
		associationType.MarkIndependent();
	}
}

using System.Data.Entity.ModelConfiguration.Conventions;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class ConventionsTypeActivator
{
	public virtual IConvention Activate(Type conventionType)
	{
		return (IConvention)Activator.CreateInstance(conventionType, nonPublic: true);
	}
}

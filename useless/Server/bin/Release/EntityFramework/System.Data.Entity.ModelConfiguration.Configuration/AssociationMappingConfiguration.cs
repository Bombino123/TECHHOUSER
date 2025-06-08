using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public abstract class AssociationMappingConfiguration
{
	internal abstract void Configure(AssociationSetMapping associationSetMapping, EdmModel database, PropertyInfo navigationProperty);

	internal abstract AssociationMappingConfiguration Clone();
}

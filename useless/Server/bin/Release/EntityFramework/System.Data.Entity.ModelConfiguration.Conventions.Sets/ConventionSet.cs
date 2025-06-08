using System.Collections.Generic;

namespace System.Data.Entity.ModelConfiguration.Conventions.Sets;

internal class ConventionSet
{
	public IEnumerable<IConvention> ConfigurationConventions { get; private set; }

	public IEnumerable<IConvention> ConceptualModelConventions { get; private set; }

	public IEnumerable<IConvention> ConceptualToStoreMappingConventions { get; private set; }

	public IEnumerable<IConvention> StoreModelConventions { get; private set; }

	public ConventionSet()
	{
		ConfigurationConventions = new IConvention[0];
		ConceptualModelConventions = new IConvention[0];
		ConceptualToStoreMappingConventions = new IConvention[0];
		StoreModelConventions = new IConvention[0];
	}

	public ConventionSet(IEnumerable<IConvention> configurationConventions, IEnumerable<IConvention> entityModelConventions, IEnumerable<IConvention> dbMappingConventions, IEnumerable<IConvention> dbModelConventions)
	{
		ConfigurationConventions = configurationConventions;
		ConceptualModelConventions = entityModelConventions;
		ConceptualToStoreMappingConventions = dbMappingConventions;
		StoreModelConventions = dbModelConventions;
	}
}

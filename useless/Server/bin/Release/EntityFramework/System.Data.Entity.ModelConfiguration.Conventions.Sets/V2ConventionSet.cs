using System.Collections.Generic;

namespace System.Data.Entity.ModelConfiguration.Conventions.Sets;

internal static class V2ConventionSet
{
	private static readonly ConventionSet _conventions;

	public static ConventionSet Conventions => _conventions;

	static V2ConventionSet()
	{
		List<IConvention> list = new List<IConvention>(V1ConventionSet.Conventions.StoreModelConventions);
		int index = list.FindIndex((IConvention c) => c.GetType() == typeof(ColumnOrderingConvention));
		list[index] = new ColumnOrderingConventionStrict();
		_conventions = new ConventionSet(V1ConventionSet.Conventions.ConfigurationConventions, V1ConventionSet.Conventions.ConceptualModelConventions, V1ConventionSet.Conventions.ConceptualToStoreMappingConventions, list);
	}
}

using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class ConventionsTypeFinder
{
	private readonly ConventionsTypeFilter _conventionsTypeFilter;

	private readonly ConventionsTypeActivator _conventionsTypeActivator;

	public ConventionsTypeFinder()
		: this(new ConventionsTypeFilter(), new ConventionsTypeActivator())
	{
	}

	public ConventionsTypeFinder(ConventionsTypeFilter conventionsTypeFilter, ConventionsTypeActivator conventionsTypeActivator)
	{
		_conventionsTypeFilter = conventionsTypeFilter;
		_conventionsTypeActivator = conventionsTypeActivator;
	}

	public void AddConventions(IEnumerable<Type> types, Action<IConvention> addFunction)
	{
		foreach (Type type in types)
		{
			if (_conventionsTypeFilter.IsConvention(type))
			{
				addFunction(_conventionsTypeActivator.Activate(type));
			}
		}
	}
}

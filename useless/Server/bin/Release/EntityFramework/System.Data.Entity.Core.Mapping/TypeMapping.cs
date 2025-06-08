using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping;

public abstract class TypeMapping : MappingItem
{
	internal abstract EntitySetBaseMapping SetMapping { get; }

	internal abstract ReadOnlyCollection<EntityTypeBase> Types { get; }

	internal abstract ReadOnlyCollection<EntityTypeBase> IsOfTypes { get; }

	internal abstract ReadOnlyCollection<MappingFragment> MappingFragments { get; }

	internal TypeMapping()
	{
	}
}

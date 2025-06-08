using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Internal;

internal class EntitySetTypePair : Tuple<EntitySet, Type>
{
	public EntitySet EntitySet => base.Item1;

	public Type BaseType => base.Item2;

	public EntitySetTypePair(EntitySet entitySet, Type type)
		: base(entitySet, type)
	{
	}
}

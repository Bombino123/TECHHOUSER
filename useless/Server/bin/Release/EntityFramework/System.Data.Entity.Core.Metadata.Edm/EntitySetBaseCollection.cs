using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class EntitySetBaseCollection : MetadataCollection<EntitySetBase>
{
	private readonly EntityContainer _entityContainer;

	public override EntitySetBase this[int index]
	{
		get
		{
			return base[index];
		}
		set
		{
			throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
		}
	}

	public override EntitySetBase this[string identity]
	{
		get
		{
			return base[identity];
		}
		set
		{
			throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
		}
	}

	internal EntitySetBaseCollection(EntityContainer entityContainer)
		: this(entityContainer, null)
	{
	}

	internal EntitySetBaseCollection(EntityContainer entityContainer, IEnumerable<EntitySetBase> items)
		: base(items)
	{
		Check.NotNull(entityContainer, "entityContainer");
		_entityContainer = entityContainer;
	}

	public override void Add(EntitySetBase item)
	{
		Check.NotNull(item, "item");
		ThrowIfItHasEntityContainer(item, "item");
		base.Add(item);
		item.ChangeEntityContainerWithoutCollectionFixup(_entityContainer);
	}

	private static void ThrowIfItHasEntityContainer(EntitySetBase entitySet, string argumentName)
	{
		Check.NotNull(entitySet, argumentName);
		if (entitySet.EntityContainer != null)
		{
			throw new ArgumentException(Strings.EntitySetInAnotherContainer, argumentName);
		}
	}
}

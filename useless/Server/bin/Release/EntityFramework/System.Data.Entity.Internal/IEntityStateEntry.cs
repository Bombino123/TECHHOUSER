using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Internal;

internal interface IEntityStateEntry
{
	object Entity { get; }

	EntityState State { get; }

	DbUpdatableDataRecord CurrentValues { get; }

	EntitySetBase EntitySet { get; }

	EntityKey EntityKey { get; }

	void ChangeState(EntityState state);

	DbUpdatableDataRecord GetUpdatableOriginalValues();

	IEnumerable<string> GetModifiedProperties();

	void SetModifiedProperty(string propertyName);

	bool IsPropertyChanged(string propertyName);

	void RejectPropertyChanges(string propertyName);
}

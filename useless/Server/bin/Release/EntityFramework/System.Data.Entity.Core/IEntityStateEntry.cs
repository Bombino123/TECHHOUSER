using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Core;

internal interface IEntityStateEntry
{
	IEntityStateManager StateManager { get; }

	EntityKey EntityKey { get; }

	EntitySetBase EntitySet { get; }

	bool IsRelationship { get; }

	bool IsKeyEntry { get; }

	EntityState State { get; }

	DbDataRecord OriginalValues { get; }

	CurrentValueRecord CurrentValues { get; }

	BitArray ModifiedProperties { get; }

	void AcceptChanges();

	void Delete();

	void SetModified();

	void SetModifiedProperty(string propertyName);

	IEnumerable<string> GetModifiedProperties();
}

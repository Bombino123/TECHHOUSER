using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core;

internal interface IEntityStateManager
{
	IEnumerable<IEntityStateEntry> GetEntityStateEntries(EntityState state);

	IEnumerable<IEntityStateEntry> FindRelationshipsByKey(EntityKey key);

	IEntityStateEntry GetEntityStateEntry(EntityKey key);

	bool TryGetEntityStateEntry(EntityKey key, out IEntityStateEntry stateEntry);

	bool TryGetReferenceKey(EntityKey dependentKey, AssociationEndMember principalRole, out EntityKey principalKey);
}

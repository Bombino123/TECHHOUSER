using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal abstract class UpdateCommand : IComparable<UpdateCommand>, IEquatable<UpdateCommand>
{
	private static int OrderingIdentifierCounter;

	private int _orderingIdentifier;

	internal abstract IEnumerable<int> OutputIdentifiers { get; }

	internal abstract IEnumerable<int> InputIdentifiers { get; }

	internal virtual EntitySet Table => null;

	internal abstract UpdateCommandKind Kind { get; }

	internal PropagatorResult OriginalValues { get; private set; }

	internal PropagatorResult CurrentValues { get; private set; }

	protected UpdateTranslator Translator { get; private set; }

	protected UpdateCommand(UpdateTranslator translator, PropagatorResult originalValues, PropagatorResult currentValues)
	{
		OriginalValues = originalValues;
		CurrentValues = currentValues;
		Translator = translator;
	}

	internal abstract IList<IEntityStateEntry> GetStateEntries(UpdateTranslator translator);

	internal void GetRequiredAndProducedEntities(UpdateTranslator translator, KeyToListMap<EntityKey, UpdateCommand> addedEntities, KeyToListMap<EntityKey, UpdateCommand> deletedEntities, KeyToListMap<EntityKey, UpdateCommand> addedRelationships, KeyToListMap<EntityKey, UpdateCommand> deletedRelationships)
	{
		IList<IEntityStateEntry> stateEntries = GetStateEntries(translator);
		foreach (IEntityStateEntry item in stateEntries)
		{
			if (!item.IsRelationship)
			{
				if (item.State == EntityState.Added)
				{
					addedEntities.Add(item.EntityKey, this);
				}
				else if (item.State == EntityState.Deleted)
				{
					deletedEntities.Add(item.EntityKey, this);
				}
			}
		}
		if (OriginalValues != null)
		{
			AddReferencedEntities(translator, OriginalValues, deletedRelationships);
		}
		if (CurrentValues != null)
		{
			AddReferencedEntities(translator, CurrentValues, addedRelationships);
		}
		foreach (IEntityStateEntry item2 in stateEntries)
		{
			if (item2.IsRelationship)
			{
				bool flag = item2.State == EntityState.Added;
				if (flag || item2.State == EntityState.Deleted)
				{
					DbDataRecord obj = (flag ? item2.CurrentValues : item2.OriginalValues);
					EntityKey key = (EntityKey)obj[0];
					EntityKey key2 = (EntityKey)obj[1];
					KeyToListMap<EntityKey, UpdateCommand> obj2 = (flag ? addedRelationships : deletedRelationships);
					obj2.Add(key, this);
					obj2.Add(key2, this);
				}
			}
		}
	}

	private void AddReferencedEntities(UpdateTranslator translator, PropagatorResult result, KeyToListMap<EntityKey, UpdateCommand> referencedEntities)
	{
		PropagatorResult[] memberValues = result.GetMemberValues();
		foreach (PropagatorResult propagatorResult in memberValues)
		{
			if (!propagatorResult.IsSimple || propagatorResult.Identifier == -1 || PropagatorFlags.ForeignKey != (propagatorResult.PropagatorFlags & PropagatorFlags.ForeignKey))
			{
				continue;
			}
			foreach (int directReference in translator.KeyManager.GetDirectReferences(propagatorResult.Identifier))
			{
				if (translator.KeyManager.TryGetIdentifierOwner(directReference, out var owner) && owner.StateEntry != null)
				{
					referencedEntities.Add(owner.StateEntry.EntityKey, this);
				}
			}
		}
	}

	internal abstract long Execute(Dictionary<int, object> identifierValues, List<KeyValuePair<PropagatorResult, object>> generatedValues);

	internal abstract Task<long> ExecuteAsync(Dictionary<int, object> identifierValues, List<KeyValuePair<PropagatorResult, object>> generatedValues, CancellationToken cancellationToken);

	internal abstract int CompareToType(UpdateCommand other);

	public int CompareTo(UpdateCommand other)
	{
		if (Equals(other))
		{
			return 0;
		}
		int num = Kind - other.Kind;
		if (num != 0)
		{
			return num;
		}
		num = CompareToType(other);
		if (num != 0)
		{
			return num;
		}
		if (_orderingIdentifier == 0)
		{
			_orderingIdentifier = Interlocked.Increment(ref OrderingIdentifierCounter);
		}
		if (other._orderingIdentifier == 0)
		{
			other._orderingIdentifier = Interlocked.Increment(ref OrderingIdentifierCounter);
		}
		return _orderingIdentifier - other._orderingIdentifier;
	}

	public bool Equals(UpdateCommand other)
	{
		return base.Equals(other);
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}

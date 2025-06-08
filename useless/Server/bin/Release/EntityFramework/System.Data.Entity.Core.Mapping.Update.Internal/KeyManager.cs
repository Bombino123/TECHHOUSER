using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class KeyManager
{
	private sealed class Partition
	{
		internal readonly int PartitionId;

		private readonly List<int> _nodeIds;

		private Partition(int partitionId)
		{
			_nodeIds = new List<int>(2);
			PartitionId = partitionId;
		}

		internal static void CreatePartition(KeyManager manager, int firstId, int secondId)
		{
			Partition partition = new Partition(firstId);
			partition.AddNode(manager, firstId);
			partition.AddNode(manager, secondId);
		}

		internal void AddNode(KeyManager manager, int nodeId)
		{
			_nodeIds.Add(nodeId);
			manager._identifiers[nodeId].Partition = this;
		}

		internal void Merge(KeyManager manager, Partition other)
		{
			if (other.PartitionId == PartitionId)
			{
				return;
			}
			foreach (int nodeId in other._nodeIds)
			{
				AddNode(manager, nodeId);
			}
		}
	}

	private sealed class LinkedList<T>
	{
		private readonly T _value;

		private readonly LinkedList<T> _previous;

		private LinkedList(T value, LinkedList<T> previous)
		{
			_value = value;
			_previous = previous;
		}

		internal static IEnumerable<T> Enumerate(LinkedList<T> current)
		{
			while (current != null)
			{
				yield return current._value;
				current = current._previous;
			}
		}

		internal static void Add(ref LinkedList<T> list, T value)
		{
			list = new LinkedList<T>(value, list);
		}
	}

	private sealed class IdentifierInfo
	{
		internal Partition Partition;

		internal PropagatorResult Owner;

		internal LinkedList<IEntityStateEntry> DependentStateEntries;

		internal LinkedList<int> References;

		internal LinkedList<int> ReferencedBy;
	}

	private readonly Dictionary<Tuple<EntityKey, string, bool>, int> _foreignKeyIdentifiers = new Dictionary<Tuple<EntityKey, string, bool>, int>();

	private readonly Dictionary<EntityKey, EntityKey> _valueKeyToTempKey = new Dictionary<EntityKey, EntityKey>();

	private readonly Dictionary<EntityKey, int> _keyIdentifiers = new Dictionary<EntityKey, int>();

	private readonly List<IdentifierInfo> _identifiers = new List<IdentifierInfo>
	{
		new IdentifierInfo()
	};

	private const byte White = 0;

	private const byte Black = 1;

	private const byte Gray = 2;

	internal int GetCliqueIdentifier(int identifier)
	{
		return _identifiers[identifier].Partition?.PartitionId ?? identifier;
	}

	internal void AddReferentialConstraint(IEntityStateEntry dependentStateEntry, int dependentIdentifier, int principalIdentifier)
	{
		IdentifierInfo identifierInfo = _identifiers[dependentIdentifier];
		if (dependentIdentifier != principalIdentifier)
		{
			AssociateNodes(dependentIdentifier, principalIdentifier);
			LinkedList<int>.Add(ref identifierInfo.References, principalIdentifier);
			LinkedList<int>.Add(ref _identifiers[principalIdentifier].ReferencedBy, dependentIdentifier);
		}
		LinkedList<IEntityStateEntry>.Add(ref identifierInfo.DependentStateEntries, dependentStateEntry);
	}

	internal void RegisterIdentifierOwner(PropagatorResult owner)
	{
		_identifiers[owner.Identifier].Owner = owner;
	}

	internal bool TryGetIdentifierOwner(int identifier, out PropagatorResult owner)
	{
		owner = _identifiers[identifier].Owner;
		return owner != null;
	}

	internal int GetKeyIdentifierForMemberOffset(EntityKey entityKey, int memberOffset, int keyMemberCount)
	{
		if (!_keyIdentifiers.TryGetValue(entityKey, out var value))
		{
			value = _identifiers.Count;
			for (int i = 0; i < keyMemberCount; i++)
			{
				_identifiers.Add(new IdentifierInfo());
			}
			_keyIdentifiers.Add(entityKey, value);
		}
		return value + memberOffset;
	}

	internal int GetKeyIdentifierForMember(EntityKey entityKey, string member, bool currentValues)
	{
		Tuple<EntityKey, string, bool> key = Tuple.Create(entityKey, member, currentValues);
		if (!_foreignKeyIdentifiers.TryGetValue(key, out var value))
		{
			value = _identifiers.Count;
			_identifiers.Add(new IdentifierInfo());
			_foreignKeyIdentifiers.Add(key, value);
		}
		return value;
	}

	internal IEnumerable<IEntityStateEntry> GetDependentStateEntries(int identifier)
	{
		return LinkedList<IEntityStateEntry>.Enumerate(_identifiers[identifier].DependentStateEntries);
	}

	internal object GetPrincipalValue(PropagatorResult result)
	{
		int identifier = result.Identifier;
		if (-1 == identifier)
		{
			return result.GetSimpleValue();
		}
		bool flag = true;
		object obj = null;
		foreach (int principal in GetPrincipals(identifier))
		{
			PropagatorResult owner = _identifiers[principal].Owner;
			if (owner != null)
			{
				if (flag)
				{
					obj = owner.GetSimpleValue();
					flag = false;
				}
				else if (!ByValueEqualityComparer.Default.Equals(obj, owner.GetSimpleValue()))
				{
					throw new ConstraintException(Strings.Update_ReferentialConstraintIntegrityViolation);
				}
			}
		}
		if (flag)
		{
			obj = result.GetSimpleValue();
		}
		return obj;
	}

	internal IEnumerable<int> GetPrincipals(int identifier)
	{
		return WalkGraph(identifier, (IdentifierInfo info) => info.References, leavesOnly: true);
	}

	internal IEnumerable<int> GetDirectReferences(int identifier)
	{
		LinkedList<int> references = _identifiers[identifier].References;
		foreach (int item in LinkedList<int>.Enumerate(references))
		{
			yield return item;
		}
	}

	internal IEnumerable<int> GetDependents(int identifier)
	{
		return WalkGraph(identifier, (IdentifierInfo info) => info.ReferencedBy, leavesOnly: false);
	}

	private IEnumerable<int> WalkGraph(int identifier, Func<IdentifierInfo, LinkedList<int>> successorFunction, bool leavesOnly)
	{
		Stack<int> stack = new Stack<int>();
		stack.Push(identifier);
		while (stack.Count > 0)
		{
			int num = stack.Pop();
			LinkedList<int> linkedList = successorFunction(_identifiers[num]);
			if (linkedList != null)
			{
				foreach (int item in LinkedList<int>.Enumerate(linkedList))
				{
					stack.Push(item);
				}
				if (!leavesOnly)
				{
					yield return num;
				}
			}
			else
			{
				yield return num;
			}
		}
	}

	internal bool HasPrincipals(int identifier)
	{
		return _identifiers[identifier].References != null;
	}

	internal void ValidateReferentialIntegrityGraphAcyclic()
	{
		byte[] array = new byte[_identifiers.Count];
		int i = 0;
		for (int count = _identifiers.Count; i < count; i++)
		{
			if (array[i] == 0)
			{
				ValidateReferentialIntegrityGraphAcyclic(i, array, null);
			}
		}
	}

	internal void RegisterKeyValueForAddedEntity(IEntityStateEntry addedEntry)
	{
		EntityKey entityKey = addedEntry.EntityKey;
		ReadOnlyMetadataCollection<EdmMember> keyMembers = addedEntry.EntitySet.ElementType.KeyMembers;
		CurrentValueRecord currentValues = addedEntry.CurrentValues;
		object[] array = new object[keyMembers.Count];
		bool flag = false;
		int i = 0;
		for (int count = keyMembers.Count; i < count; i++)
		{
			int ordinal = currentValues.GetOrdinal(keyMembers[i].Name);
			if (currentValues.IsDBNull(ordinal))
			{
				flag = true;
				break;
			}
			array[i] = currentValues.GetValue(ordinal);
		}
		if (!flag)
		{
			EntityKey key = ((array.Length == 1) ? new EntityKey(addedEntry.EntitySet, array[0]) : new EntityKey(addedEntry.EntitySet, array));
			if (_valueKeyToTempKey.ContainsKey(key))
			{
				_valueKeyToTempKey[key] = null;
			}
			else
			{
				_valueKeyToTempKey.Add(key, entityKey);
			}
		}
	}

	internal bool TryGetTempKey(EntityKey valueKey, out EntityKey tempKey)
	{
		return _valueKeyToTempKey.TryGetValue(valueKey, out tempKey);
	}

	private void ValidateReferentialIntegrityGraphAcyclic(int node, byte[] color, LinkedList<int> parent)
	{
		color[node] = 2;
		LinkedList<int>.Add(ref parent, node);
		foreach (int item in LinkedList<int>.Enumerate(_identifiers[node].References))
		{
			switch (color[item])
			{
			case 0:
				ValidateReferentialIntegrityGraphAcyclic(item, color, parent);
				break;
			case 2:
			{
				List<IEntityStateEntry> list = new List<IEntityStateEntry>();
				foreach (int item2 in LinkedList<int>.Enumerate(parent))
				{
					PropagatorResult owner = _identifiers[item2].Owner;
					if (owner != null)
					{
						list.Add(owner.StateEntry);
					}
					if (item2 == item)
					{
						break;
					}
				}
				throw new UpdateException(Strings.Update_CircularRelationships, null, list.Cast<ObjectStateEntry>().Distinct());
			}
			}
		}
		color[node] = 1;
	}

	internal void AssociateNodes(int firstId, int secondId)
	{
		if (firstId == secondId)
		{
			return;
		}
		Partition partition = _identifiers[firstId].Partition;
		if (partition != null)
		{
			Partition partition2 = _identifiers[secondId].Partition;
			if (partition2 != null)
			{
				partition.Merge(this, partition2);
			}
			else
			{
				partition.AddNode(this, secondId);
			}
		}
		else
		{
			Partition partition3 = _identifiers[secondId].Partition;
			if (partition3 != null)
			{
				partition3.AddNode(this, firstId);
			}
			else
			{
				Partition.CreatePartition(this, firstId, secondId);
			}
		}
	}
}

using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class UpdateCommandOrderer : Graph<UpdateCommand>
{
	private struct ForeignKeyValue
	{
		internal readonly ReferentialConstraint Metadata;

		internal readonly CompositeKey Key;

		internal readonly bool IsInsert;

		private ForeignKeyValue(ReferentialConstraint metadata, PropagatorResult record, bool isTarget, bool isInsert)
		{
			Metadata = metadata;
			IList<EdmProperty> list = (isTarget ? metadata.FromProperties : metadata.ToProperties);
			PropagatorResult[] array = new PropagatorResult[list.Count];
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = record.GetMemberValue(list[i]);
				if (array[i].IsNull)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				Key = null;
			}
			else
			{
				Key = new CompositeKey(array);
			}
			IsInsert = isInsert;
		}

		internal static bool TryCreateTargetKey(ReferentialConstraint metadata, PropagatorResult record, bool isInsert, out ForeignKeyValue key)
		{
			key = new ForeignKeyValue(metadata, record, isTarget: true, isInsert);
			if (key.Key == null)
			{
				return false;
			}
			return true;
		}

		internal static bool TryCreateSourceKey(ReferentialConstraint metadata, PropagatorResult record, bool isInsert, out ForeignKeyValue key)
		{
			key = new ForeignKeyValue(metadata, record, isTarget: false, isInsert);
			if (key.Key == null)
			{
				return false;
			}
			return true;
		}
	}

	private class ForeignKeyValueComparer : IEqualityComparer<ForeignKeyValue>
	{
		private readonly IEqualityComparer<CompositeKey> _baseComparer;

		internal ForeignKeyValueComparer(IEqualityComparer<CompositeKey> baseComparer)
		{
			_baseComparer = baseComparer;
		}

		public bool Equals(ForeignKeyValue x, ForeignKeyValue y)
		{
			if (x.IsInsert == y.IsInsert && x.Metadata == y.Metadata)
			{
				return _baseComparer.Equals(x.Key, y.Key);
			}
			return false;
		}

		public int GetHashCode(ForeignKeyValue obj)
		{
			return _baseComparer.GetHashCode(obj.Key);
		}
	}

	private readonly ForeignKeyValueComparer _keyComparer;

	private readonly KeyToListMap<EntitySetBase, ReferentialConstraint> _sourceMap;

	private readonly KeyToListMap<EntitySetBase, ReferentialConstraint> _targetMap;

	private readonly bool _hasFunctionCommands;

	private readonly UpdateTranslator _translator;

	internal UpdateCommandOrderer(IEnumerable<UpdateCommand> commands, UpdateTranslator translator)
		: base((IEqualityComparer<UpdateCommand>)EqualityComparer<UpdateCommand>.Default)
	{
		_translator = translator;
		_keyComparer = new ForeignKeyValueComparer(_translator.KeyComparer);
		HashSet<EntitySet> hashSet = new HashSet<EntitySet>();
		HashSet<EntityContainer> hashSet2 = new HashSet<EntityContainer>();
		foreach (UpdateCommand command in commands)
		{
			if (command.Table != null)
			{
				hashSet.Add(command.Table);
				hashSet2.Add(command.Table.EntityContainer);
			}
			AddVertex(command);
			if (command.Kind == UpdateCommandKind.Function)
			{
				_hasFunctionCommands = true;
			}
		}
		InitializeForeignKeyMaps(hashSet2, hashSet, out _sourceMap, out _targetMap);
		AddServerGenDependencies();
		AddForeignKeyDependencies();
		if (_hasFunctionCommands)
		{
			AddModelDependencies();
		}
	}

	private static void InitializeForeignKeyMaps(HashSet<EntityContainer> containers, HashSet<EntitySet> tables, out KeyToListMap<EntitySetBase, ReferentialConstraint> sourceMap, out KeyToListMap<EntitySetBase, ReferentialConstraint> targetMap)
	{
		sourceMap = new KeyToListMap<EntitySetBase, ReferentialConstraint>(EqualityComparer<EntitySetBase>.Default);
		targetMap = new KeyToListMap<EntitySetBase, ReferentialConstraint>(EqualityComparer<EntitySetBase>.Default);
		foreach (EntityContainer container in containers)
		{
			foreach (EntitySetBase baseEntitySet in container.BaseEntitySets)
			{
				if (!(baseEntitySet is AssociationSet associationSet))
				{
					continue;
				}
				AssociationSetEnd associationSetEnd = null;
				AssociationSetEnd associationSetEnd2 = null;
				ReadOnlyMetadataCollection<AssociationSetEnd> associationSetEnds = associationSet.AssociationSetEnds;
				if (2 != associationSetEnds.Count)
				{
					continue;
				}
				AssociationType elementType = associationSet.ElementType;
				bool flag = false;
				ReferentialConstraint value = null;
				foreach (ReferentialConstraint referentialConstraint in elementType.ReferentialConstraints)
				{
					if (!flag)
					{
						flag = true;
					}
					associationSetEnd = associationSet.AssociationSetEnds[referentialConstraint.ToRole.Name];
					associationSetEnd2 = associationSet.AssociationSetEnds[referentialConstraint.FromRole.Name];
					value = referentialConstraint;
				}
				if (associationSetEnd2 != null && associationSetEnd != null && tables.Contains(associationSetEnd2.EntitySet) && tables.Contains(associationSetEnd.EntitySet))
				{
					sourceMap.Add(associationSetEnd.EntitySet, value);
					targetMap.Add(associationSetEnd2.EntitySet, value);
				}
			}
		}
	}

	private void AddServerGenDependencies()
	{
		Dictionary<int, UpdateCommand> dictionary = new Dictionary<int, UpdateCommand>();
		foreach (UpdateCommand vertex in base.Vertices)
		{
			foreach (int outputIdentifier in vertex.OutputIdentifiers)
			{
				try
				{
					dictionary.Add(outputIdentifier, vertex);
				}
				catch (ArgumentException innerException)
				{
					throw new UpdateException(Strings.Update_AmbiguousServerGenIdentifier, innerException, vertex.GetStateEntries(_translator).Cast<ObjectStateEntry>().Distinct());
				}
			}
		}
		foreach (UpdateCommand vertex2 in base.Vertices)
		{
			foreach (int inputIdentifier in vertex2.InputIdentifiers)
			{
				if (dictionary.TryGetValue(inputIdentifier, out var value))
				{
					AddEdge(value, vertex2);
				}
			}
		}
	}

	private void AddForeignKeyDependencies()
	{
		KeyToListMap<ForeignKeyValue, UpdateCommand> predecessors = DetermineForeignKeyPredecessors();
		AddForeignKeyEdges(predecessors);
	}

	private void AddForeignKeyEdges(KeyToListMap<ForeignKeyValue, UpdateCommand> predecessors)
	{
		foreach (DynamicUpdateCommand item in base.Vertices.OfType<DynamicUpdateCommand>())
		{
			if (item.Operator == ModificationOperator.Update || ModificationOperator.Insert == item.Operator)
			{
				foreach (ReferentialConstraint item2 in _sourceMap.EnumerateValues(item.Table))
				{
					if (!ForeignKeyValue.TryCreateSourceKey(item2, item.CurrentValues, isInsert: true, out var key) || (item.Operator == ModificationOperator.Update && ForeignKeyValue.TryCreateSourceKey(item2, item.OriginalValues, isInsert: true, out var key2) && _keyComparer.Equals(key2, key)))
					{
						continue;
					}
					foreach (UpdateCommand item3 in predecessors.EnumerateValues(key))
					{
						if (item3 != item)
						{
							AddEdge(item3, item);
						}
					}
				}
			}
			if (item.Operator != 0 && ModificationOperator.Delete != item.Operator)
			{
				continue;
			}
			foreach (ReferentialConstraint item4 in _targetMap.EnumerateValues(item.Table))
			{
				if (!ForeignKeyValue.TryCreateTargetKey(item4, item.OriginalValues, isInsert: false, out var key3) || (item.Operator == ModificationOperator.Update && ForeignKeyValue.TryCreateTargetKey(item4, item.CurrentValues, isInsert: false, out var key4) && _keyComparer.Equals(key4, key3)))
				{
					continue;
				}
				foreach (UpdateCommand item5 in predecessors.EnumerateValues(key3))
				{
					if (item5 != item)
					{
						AddEdge(item5, item);
					}
				}
			}
		}
	}

	private KeyToListMap<ForeignKeyValue, UpdateCommand> DetermineForeignKeyPredecessors()
	{
		KeyToListMap<ForeignKeyValue, UpdateCommand> keyToListMap = new KeyToListMap<ForeignKeyValue, UpdateCommand>(_keyComparer);
		foreach (DynamicUpdateCommand item in base.Vertices.OfType<DynamicUpdateCommand>())
		{
			if (item.Operator == ModificationOperator.Update || ModificationOperator.Insert == item.Operator)
			{
				foreach (ReferentialConstraint item2 in _targetMap.EnumerateValues(item.Table))
				{
					if (ForeignKeyValue.TryCreateTargetKey(item2, item.CurrentValues, isInsert: true, out var key) && (item.Operator != 0 || !ForeignKeyValue.TryCreateTargetKey(item2, item.OriginalValues, isInsert: true, out var key2) || !_keyComparer.Equals(key2, key)))
					{
						keyToListMap.Add(key, item);
					}
				}
			}
			if (item.Operator != 0 && ModificationOperator.Delete != item.Operator)
			{
				continue;
			}
			foreach (ReferentialConstraint item3 in _sourceMap.EnumerateValues(item.Table))
			{
				if (ForeignKeyValue.TryCreateSourceKey(item3, item.OriginalValues, isInsert: false, out var key3) && (item.Operator != 0 || !ForeignKeyValue.TryCreateSourceKey(item3, item.CurrentValues, isInsert: false, out var key4) || !_keyComparer.Equals(key4, key3)))
				{
					keyToListMap.Add(key3, item);
				}
			}
		}
		return keyToListMap;
	}

	private void AddModelDependencies()
	{
		KeyToListMap<EntityKey, UpdateCommand> keyToListMap = new KeyToListMap<EntityKey, UpdateCommand>(EqualityComparer<EntityKey>.Default);
		KeyToListMap<EntityKey, UpdateCommand> keyToListMap2 = new KeyToListMap<EntityKey, UpdateCommand>(EqualityComparer<EntityKey>.Default);
		KeyToListMap<EntityKey, UpdateCommand> keyToListMap3 = new KeyToListMap<EntityKey, UpdateCommand>(EqualityComparer<EntityKey>.Default);
		KeyToListMap<EntityKey, UpdateCommand> keyToListMap4 = new KeyToListMap<EntityKey, UpdateCommand>(EqualityComparer<EntityKey>.Default);
		foreach (UpdateCommand vertex in base.Vertices)
		{
			vertex.GetRequiredAndProducedEntities(_translator, keyToListMap, keyToListMap2, keyToListMap3, keyToListMap4);
		}
		AddModelDependencies(keyToListMap, keyToListMap3);
		AddModelDependencies(keyToListMap4, keyToListMap2);
	}

	private void AddModelDependencies(KeyToListMap<EntityKey, UpdateCommand> producedMap, KeyToListMap<EntityKey, UpdateCommand> requiredMap)
	{
		foreach (KeyValuePair<EntityKey, List<UpdateCommand>> keyValuePair in requiredMap.KeyValuePairs)
		{
			EntityKey key = keyValuePair.Key;
			List<UpdateCommand> value = keyValuePair.Value;
			foreach (UpdateCommand item in producedMap.EnumerateValues(key))
			{
				foreach (UpdateCommand item2 in value)
				{
					if (item != item2 && (item.Kind == UpdateCommandKind.Function || item2.Kind == UpdateCommandKind.Function))
					{
						AddEdge(item, item2);
					}
				}
			}
		}
	}
}

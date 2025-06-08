using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MetadataOptimization
{
	private readonly MetadataWorkspace _workspace;

	private readonly IDictionary<Type, EntitySetTypePair> _entitySetMappingsCache = new Dictionary<Type, EntitySetTypePair>();

	private object _entitySetMappingsUpdateLock = new object();

	private volatile AssociationType[] _csAssociationTypes;

	private volatile AssociationType[] _osAssociationTypes;

	private volatile object[] _csAssociationTypeToSets;

	internal IDictionary<Type, EntitySetTypePair> EntitySetMappingCache => _entitySetMappingsCache;

	internal MetadataOptimization(MetadataWorkspace workspace)
	{
		_workspace = workspace;
	}

	private void UpdateEntitySetMappings()
	{
		ObjectItemCollection objectItemCollection = (ObjectItemCollection)_workspace.GetItemCollection(DataSpace.OSpace);
		ReadOnlyCollection<EntityType> items = _workspace.GetItems<EntityType>(DataSpace.OSpace);
		Stack<EntityType> stack = new Stack<EntityType>();
		foreach (EntityType item in items)
		{
			stack.Clear();
			EntityType cspaceType = (EntityType)_workspace.GetEdmSpaceType(item);
			do
			{
				stack.Push(cspaceType);
				cspaceType = (EntityType)cspaceType.BaseType;
			}
			while (cspaceType != null);
			EntitySet entitySet = null;
			while (entitySet == null && stack.Count > 0)
			{
				cspaceType = stack.Pop();
				foreach (EntityContainer item2 in _workspace.GetItems<EntityContainer>(DataSpace.CSpace))
				{
					List<EntitySetBase> list = item2.BaseEntitySets.Where((EntitySetBase s) => s.ElementType == cspaceType).ToList();
					int count = list.Count;
					if (count > 1 || (count == 1 && entitySet != null))
					{
						throw Error.DbContext_MESTNotSupported();
					}
					if (count == 1)
					{
						entitySet = (EntitySet)list[0];
					}
				}
			}
			if (entitySet != null)
			{
				EntityType objectSpaceType = (EntityType)_workspace.GetObjectSpaceType(cspaceType);
				Type clrType = objectItemCollection.GetClrType(item);
				Type clrType2 = objectItemCollection.GetClrType(objectSpaceType);
				_entitySetMappingsCache[clrType] = new EntitySetTypePair(entitySet, clrType2);
			}
		}
	}

	internal bool TryUpdateEntitySetMappingsForType(Type entityType)
	{
		if (_entitySetMappingsCache.ContainsKey(entityType))
		{
			return true;
		}
		Type type = entityType;
		do
		{
			_workspace.LoadFromAssembly(type.Assembly());
			type = type.BaseType();
		}
		while (type != null && type != typeof(object));
		lock (_entitySetMappingsUpdateLock)
		{
			if (_entitySetMappingsCache.ContainsKey(entityType))
			{
				return true;
			}
			UpdateEntitySetMappings();
		}
		return _entitySetMappingsCache.ContainsKey(entityType);
	}

	internal AssociationType GetCSpaceAssociationType(AssociationType osAssociationType)
	{
		return _csAssociationTypes[osAssociationType.Index];
	}

	internal AssociationSet FindCSpaceAssociationSet(AssociationType associationType, string endName, EntitySet endEntitySet)
	{
		object[] cSpaceAssociationTypeToSetsMap = GetCSpaceAssociationTypeToSetsMap();
		int index = associationType.Index;
		object obj = cSpaceAssociationTypeToSetsMap[index];
		if (obj == null)
		{
			return null;
		}
		if (obj is AssociationSet associationSet)
		{
			if (associationSet.AssociationSetEnds[endName].EntitySet != endEntitySet)
			{
				return null;
			}
			return associationSet;
		}
		AssociationSet[] array = (AssociationSet[])obj;
		foreach (AssociationSet associationSet2 in array)
		{
			if (associationSet2.AssociationSetEnds[endName].EntitySet == endEntitySet)
			{
				return associationSet2;
			}
		}
		return null;
	}

	internal AssociationSet FindCSpaceAssociationSet(AssociationType associationType, string endName, string entitySetName, string entityContainerName, out EntitySet endEntitySet)
	{
		object[] cSpaceAssociationTypeToSetsMap = GetCSpaceAssociationTypeToSetsMap();
		int index = associationType.Index;
		object obj = cSpaceAssociationTypeToSetsMap[index];
		if (obj == null)
		{
			endEntitySet = null;
			return null;
		}
		if (obj is AssociationSet associationSet)
		{
			EntitySet entitySet = associationSet.AssociationSetEnds[endName].EntitySet;
			if (entitySet.Name == entitySetName && entitySet.EntityContainer.Name == entityContainerName)
			{
				endEntitySet = entitySet;
				return associationSet;
			}
			endEntitySet = null;
			return null;
		}
		AssociationSet[] array = (AssociationSet[])obj;
		foreach (AssociationSet associationSet2 in array)
		{
			EntitySet entitySet2 = associationSet2.AssociationSetEnds[endName].EntitySet;
			if (entitySet2.Name == entitySetName && entitySet2.EntityContainer.Name == entityContainerName)
			{
				endEntitySet = entitySet2;
				return associationSet2;
			}
		}
		endEntitySet = null;
		return null;
	}

	internal AssociationType[] GetCSpaceAssociationTypes()
	{
		if (_csAssociationTypes == null)
		{
			_csAssociationTypes = IndexCSpaceAssociationTypes(_workspace.GetItemCollection(DataSpace.CSpace));
		}
		return _csAssociationTypes;
	}

	private static AssociationType[] IndexCSpaceAssociationTypes(ItemCollection itemCollection)
	{
		List<AssociationType> list = new List<AssociationType>();
		int num = 0;
		foreach (AssociationType item in itemCollection.GetItems<AssociationType>())
		{
			list.Add(item);
			item.Index = num++;
		}
		return list.ToArray();
	}

	internal object[] GetCSpaceAssociationTypeToSetsMap()
	{
		if (_csAssociationTypeToSets == null)
		{
			_csAssociationTypeToSets = MapCSpaceAssociationTypeToSets(_workspace.GetItemCollection(DataSpace.CSpace), GetCSpaceAssociationTypes().Length);
		}
		return _csAssociationTypeToSets;
	}

	private static object[] MapCSpaceAssociationTypeToSets(ItemCollection itemCollection, int associationTypeCount)
	{
		object[] array = new object[associationTypeCount];
		foreach (EntityContainer item in itemCollection.GetItems<EntityContainer>())
		{
			foreach (EntitySetBase baseEntitySet in item.BaseEntitySets)
			{
				if (baseEntitySet is AssociationSet associationSet)
				{
					int index = associationSet.ElementType.Index;
					AddItemAtIndex(array, index, associationSet);
				}
			}
		}
		return array;
	}

	internal AssociationType GetOSpaceAssociationType(AssociationType cSpaceAssociationType, Func<AssociationType> initializer)
	{
		AssociationType[] oSpaceAssociationTypes = GetOSpaceAssociationTypes();
		int index = cSpaceAssociationType.Index;
		Thread.MemoryBarrier();
		AssociationType associationType = oSpaceAssociationTypes[index];
		if (associationType == null)
		{
			associationType = initializer();
			associationType.Index = index;
			oSpaceAssociationTypes[index] = associationType;
			Thread.MemoryBarrier();
		}
		return associationType;
	}

	internal AssociationType[] GetOSpaceAssociationTypes()
	{
		if (_osAssociationTypes == null)
		{
			_osAssociationTypes = new AssociationType[GetCSpaceAssociationTypes().Length];
		}
		return _osAssociationTypes;
	}

	private static void AddItemAtIndex<T>(object[] array, int index, T newItem) where T : class
	{
		object obj = array[index];
		if (obj == null)
		{
			array[index] = newItem;
		}
		else if (obj is T val)
		{
			array[index] = new T[2] { val, newItem };
		}
		else
		{
			T[] array2 = (T[])obj;
			int num = array2.Length;
			Array.Resize(ref array2, num + 1);
			array2[num] = newItem;
			array[index] = array2;
		}
	}
}

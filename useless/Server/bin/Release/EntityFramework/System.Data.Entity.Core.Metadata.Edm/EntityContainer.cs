using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

public class EntityContainer : GlobalItem
{
	private string _name;

	private readonly ReadOnlyMetadataCollection<EntitySetBase> _baseEntitySets;

	private readonly ReadOnlyMetadataCollection<EdmFunction> _functionImports;

	private readonly object _baseEntitySetsLock = new object();

	private ReadOnlyMetadataCollection<AssociationSet> _associationSetsCache;

	private ReadOnlyMetadataCollection<EntitySet> _entitySetsCache;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.EntityContainer;

	internal override string Identity => Name;

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public virtual string Name
	{
		get
		{
			return _name;
		}
		set
		{
			Check.NotEmpty(value, "value");
			Util.ThrowIfReadOnly(this);
			_name = value;
		}
	}

	[MetadataProperty(BuiltInTypeKind.EntitySetBase, true)]
	public ReadOnlyMetadataCollection<EntitySetBase> BaseEntitySets => _baseEntitySets;

	public ReadOnlyMetadataCollection<AssociationSet> AssociationSets
	{
		get
		{
			ReadOnlyMetadataCollection<AssociationSet> associationSetsCache = _associationSetsCache;
			if (associationSetsCache == null)
			{
				lock (_baseEntitySetsLock)
				{
					if (_associationSetsCache == null)
					{
						_baseEntitySets.SourceAccessed += ResetAssociationSetsCache;
						_associationSetsCache = new FilteredReadOnlyMetadataCollection<AssociationSet, EntitySetBase>(_baseEntitySets, Helper.IsAssociationSet);
					}
					associationSetsCache = _associationSetsCache;
				}
			}
			return associationSetsCache;
		}
	}

	public ReadOnlyMetadataCollection<EntitySet> EntitySets
	{
		get
		{
			ReadOnlyMetadataCollection<EntitySet> entitySetsCache = _entitySetsCache;
			if (entitySetsCache == null)
			{
				lock (_baseEntitySetsLock)
				{
					if (_entitySetsCache == null)
					{
						_baseEntitySets.SourceAccessed += ResetEntitySetsCache;
						_entitySetsCache = new FilteredReadOnlyMetadataCollection<EntitySet, EntitySetBase>(_baseEntitySets, Helper.IsEntitySet);
					}
					entitySetsCache = _entitySetsCache;
				}
			}
			return entitySetsCache;
		}
	}

	[MetadataProperty(BuiltInTypeKind.EdmFunction, true)]
	public ReadOnlyMetadataCollection<EdmFunction> FunctionImports => _functionImports;

	internal EntityContainer()
	{
	}

	public EntityContainer(string name, DataSpace dataSpace)
	{
		Check.NotEmpty(name, "name");
		_name = name;
		DataSpace = dataSpace;
		_baseEntitySets = new ReadOnlyMetadataCollection<EntitySetBase>(new EntitySetBaseCollection(this));
		_functionImports = new ReadOnlyMetadataCollection<EdmFunction>(new MetadataCollection<EdmFunction>());
	}

	private void ResetAssociationSetsCache(object sender, EventArgs e)
	{
		if (_associationSetsCache == null)
		{
			return;
		}
		lock (_baseEntitySetsLock)
		{
			if (_associationSetsCache != null)
			{
				_associationSetsCache = null;
				_baseEntitySets.SourceAccessed -= ResetAssociationSetsCache;
			}
		}
	}

	private void ResetEntitySetsCache(object sender, EventArgs e)
	{
		if (_entitySetsCache == null)
		{
			return;
		}
		lock (_baseEntitySetsLock)
		{
			if (_entitySetsCache != null)
			{
				_entitySetsCache = null;
				_baseEntitySets.SourceAccessed -= ResetEntitySetsCache;
			}
		}
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
			BaseEntitySets.Source.SetReadOnly();
			FunctionImports.Source.SetReadOnly();
		}
	}

	public EntitySet GetEntitySetByName(string name, bool ignoreCase)
	{
		if (BaseEntitySets.GetValue(name, ignoreCase) is EntitySet result)
		{
			return result;
		}
		throw new ArgumentException(Strings.InvalidEntitySetName(name));
	}

	public bool TryGetEntitySetByName(string name, bool ignoreCase, out EntitySet entitySet)
	{
		Check.NotNull(name, "name");
		EntitySetBase item = null;
		entitySet = null;
		if (BaseEntitySets.TryGetValue(name, ignoreCase, out item) && Helper.IsEntitySet(item))
		{
			entitySet = (EntitySet)item;
			return true;
		}
		return false;
	}

	public RelationshipSet GetRelationshipSetByName(string name, bool ignoreCase)
	{
		if (!TryGetRelationshipSetByName(name, ignoreCase, out var relationshipSet))
		{
			throw new ArgumentException(Strings.InvalidRelationshipSetName(name));
		}
		return relationshipSet;
	}

	public bool TryGetRelationshipSetByName(string name, bool ignoreCase, out RelationshipSet relationshipSet)
	{
		Check.NotNull(name, "name");
		EntitySetBase item = null;
		relationshipSet = null;
		if (BaseEntitySets.TryGetValue(name, ignoreCase, out item) && Helper.IsRelationshipSet(item))
		{
			relationshipSet = (RelationshipSet)item;
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return Name;
	}

	public void AddEntitySetBase(EntitySetBase entitySetBase)
	{
		Check.NotNull(entitySetBase, "entitySetBase");
		Util.ThrowIfReadOnly(this);
		_baseEntitySets.Source.Add(entitySetBase);
		entitySetBase.ChangeEntityContainerWithoutCollectionFixup(this);
	}

	public void RemoveEntitySetBase(EntitySetBase entitySetBase)
	{
		Check.NotNull(entitySetBase, "entitySetBase");
		Util.ThrowIfReadOnly(this);
		_baseEntitySets.Source.Remove(entitySetBase);
		entitySetBase.ChangeEntityContainerWithoutCollectionFixup(null);
	}

	public void AddFunctionImport(EdmFunction function)
	{
		Check.NotNull(function, "function");
		Util.ThrowIfReadOnly(this);
		if (!function.IsFunctionImport)
		{
			throw new ArgumentException(Strings.OnlyFunctionImportsCanBeAddedToEntityContainer(function.Name));
		}
		_functionImports.Source.Add(function);
	}

	public static EntityContainer Create(string name, DataSpace dataSpace, IEnumerable<EntitySetBase> entitySets, IEnumerable<EdmFunction> functionImports, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		EntityContainer entityContainer = new EntityContainer(name, dataSpace);
		if (entitySets != null)
		{
			foreach (EntitySetBase entitySet in entitySets)
			{
				entityContainer.AddEntitySetBase(entitySet);
			}
		}
		if (functionImports != null)
		{
			foreach (EdmFunction functionImport in functionImports)
			{
				if (!functionImport.IsFunctionImport)
				{
					throw new ArgumentException(Strings.OnlyFunctionImportsCanBeAddedToEntityContainer(functionImport.Name));
				}
				entityContainer.AddFunctionImport(functionImport);
			}
		}
		if (metadataProperties != null)
		{
			entityContainer.AddMetadataProperties(metadataProperties);
		}
		entityContainer.SetReadOnly();
		return entityContainer;
	}

	internal virtual void NotifyItemIdentityChanged(EntitySetBase item, string initialIdentity)
	{
		_baseEntitySets.Source.HandleIdentityChange(item, initialIdentity);
	}
}

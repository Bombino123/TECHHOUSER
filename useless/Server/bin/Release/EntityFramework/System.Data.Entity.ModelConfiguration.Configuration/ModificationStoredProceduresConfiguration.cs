using System.ComponentModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class ModificationStoredProceduresConfiguration
{
	private ModificationStoredProcedureConfiguration _insertModificationStoredProcedureConfiguration;

	private ModificationStoredProcedureConfiguration _updateModificationStoredProcedureConfiguration;

	private ModificationStoredProcedureConfiguration _deleteModificationStoredProcedureConfiguration;

	public ModificationStoredProcedureConfiguration InsertModificationStoredProcedureConfiguration => _insertModificationStoredProcedureConfiguration;

	public ModificationStoredProcedureConfiguration UpdateModificationStoredProcedureConfiguration => _updateModificationStoredProcedureConfiguration;

	public ModificationStoredProcedureConfiguration DeleteModificationStoredProcedureConfiguration => _deleteModificationStoredProcedureConfiguration;

	public ModificationStoredProceduresConfiguration()
	{
	}

	private ModificationStoredProceduresConfiguration(ModificationStoredProceduresConfiguration source)
	{
		if (source._insertModificationStoredProcedureConfiguration != null)
		{
			_insertModificationStoredProcedureConfiguration = source._insertModificationStoredProcedureConfiguration.Clone();
		}
		if (source._updateModificationStoredProcedureConfiguration != null)
		{
			_updateModificationStoredProcedureConfiguration = source._updateModificationStoredProcedureConfiguration.Clone();
		}
		if (source._deleteModificationStoredProcedureConfiguration != null)
		{
			_deleteModificationStoredProcedureConfiguration = source._deleteModificationStoredProcedureConfiguration.Clone();
		}
	}

	public virtual ModificationStoredProceduresConfiguration Clone()
	{
		return new ModificationStoredProceduresConfiguration(this);
	}

	public virtual void Insert(ModificationStoredProcedureConfiguration modificationStoredProcedureConfiguration)
	{
		_insertModificationStoredProcedureConfiguration = modificationStoredProcedureConfiguration;
	}

	public virtual void Update(ModificationStoredProcedureConfiguration modificationStoredProcedureConfiguration)
	{
		_updateModificationStoredProcedureConfiguration = modificationStoredProcedureConfiguration;
	}

	public virtual void Delete(ModificationStoredProcedureConfiguration modificationStoredProcedureConfiguration)
	{
		_deleteModificationStoredProcedureConfiguration = modificationStoredProcedureConfiguration;
	}

	public virtual void Configure(EntityTypeModificationFunctionMapping modificationStoredProcedureMapping, DbProviderManifest providerManifest)
	{
		if (_insertModificationStoredProcedureConfiguration != null)
		{
			_insertModificationStoredProcedureConfiguration.Configure(modificationStoredProcedureMapping.InsertFunctionMapping, providerManifest);
		}
		if (_updateModificationStoredProcedureConfiguration != null)
		{
			_updateModificationStoredProcedureConfiguration.Configure(modificationStoredProcedureMapping.UpdateFunctionMapping, providerManifest);
		}
		if (_deleteModificationStoredProcedureConfiguration != null)
		{
			_deleteModificationStoredProcedureConfiguration.Configure(modificationStoredProcedureMapping.DeleteFunctionMapping, providerManifest);
		}
	}

	public void Configure(AssociationSetModificationFunctionMapping modificationStoredProcedureMapping, DbProviderManifest providerManifest)
	{
		if (_insertModificationStoredProcedureConfiguration != null)
		{
			_insertModificationStoredProcedureConfiguration.Configure(modificationStoredProcedureMapping.InsertFunctionMapping, providerManifest);
		}
		if (_deleteModificationStoredProcedureConfiguration != null)
		{
			_deleteModificationStoredProcedureConfiguration.Configure(modificationStoredProcedureMapping.DeleteFunctionMapping, providerManifest);
		}
	}

	public bool IsCompatibleWith(ModificationStoredProceduresConfiguration other)
	{
		if (_insertModificationStoredProcedureConfiguration != null && other._insertModificationStoredProcedureConfiguration != null && !_insertModificationStoredProcedureConfiguration.IsCompatibleWith(other._insertModificationStoredProcedureConfiguration))
		{
			return false;
		}
		if (_deleteModificationStoredProcedureConfiguration != null && other._deleteModificationStoredProcedureConfiguration != null && !_deleteModificationStoredProcedureConfiguration.IsCompatibleWith(other._deleteModificationStoredProcedureConfiguration))
		{
			return false;
		}
		return true;
	}

	public void Merge(ModificationStoredProceduresConfiguration modificationStoredProceduresConfiguration, bool allowOverride)
	{
		if (_insertModificationStoredProcedureConfiguration == null)
		{
			_insertModificationStoredProcedureConfiguration = modificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration;
		}
		else if (modificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration != null)
		{
			_insertModificationStoredProcedureConfiguration.Merge(modificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration, allowOverride);
		}
		if (_updateModificationStoredProcedureConfiguration == null)
		{
			_updateModificationStoredProcedureConfiguration = modificationStoredProceduresConfiguration.UpdateModificationStoredProcedureConfiguration;
		}
		else if (modificationStoredProceduresConfiguration.UpdateModificationStoredProcedureConfiguration != null)
		{
			_updateModificationStoredProcedureConfiguration.Merge(modificationStoredProceduresConfiguration.UpdateModificationStoredProcedureConfiguration, allowOverride);
		}
		if (_deleteModificationStoredProcedureConfiguration == null)
		{
			_deleteModificationStoredProcedureConfiguration = modificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration;
		}
		else if (modificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration != null)
		{
			_deleteModificationStoredProcedureConfiguration.Merge(modificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration, allowOverride);
		}
	}
}
public class ModificationStoredProceduresConfiguration<TEntityType> where TEntityType : class
{
	private readonly ModificationStoredProceduresConfiguration _configuration = new ModificationStoredProceduresConfiguration();

	internal ModificationStoredProceduresConfiguration Configuration => _configuration;

	internal ModificationStoredProceduresConfiguration()
	{
	}

	public ModificationStoredProceduresConfiguration<TEntityType> Insert(Action<InsertModificationStoredProcedureConfiguration<TEntityType>> modificationStoredProcedureConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");
		InsertModificationStoredProcedureConfiguration<TEntityType> insertModificationStoredProcedureConfiguration = new InsertModificationStoredProcedureConfiguration<TEntityType>();
		modificationStoredProcedureConfigurationAction(insertModificationStoredProcedureConfiguration);
		_configuration.Insert(insertModificationStoredProcedureConfiguration.Configuration);
		return this;
	}

	public ModificationStoredProceduresConfiguration<TEntityType> Update(Action<UpdateModificationStoredProcedureConfiguration<TEntityType>> modificationStoredProcedureConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");
		UpdateModificationStoredProcedureConfiguration<TEntityType> updateModificationStoredProcedureConfiguration = new UpdateModificationStoredProcedureConfiguration<TEntityType>();
		modificationStoredProcedureConfigurationAction(updateModificationStoredProcedureConfiguration);
		_configuration.Update(updateModificationStoredProcedureConfiguration.Configuration);
		return this;
	}

	public ModificationStoredProceduresConfiguration<TEntityType> Delete(Action<DeleteModificationStoredProcedureConfiguration<TEntityType>> modificationStoredProcedureConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");
		DeleteModificationStoredProcedureConfiguration<TEntityType> deleteModificationStoredProcedureConfiguration = new DeleteModificationStoredProcedureConfiguration<TEntityType>();
		modificationStoredProcedureConfigurationAction(deleteModificationStoredProcedureConfiguration);
		_configuration.Delete(deleteModificationStoredProcedureConfiguration.Configuration);
		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}

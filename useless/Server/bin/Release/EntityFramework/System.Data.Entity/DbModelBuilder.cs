using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Conventions.Sets;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Mappers;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity;

public class DbModelBuilder
{
	private readonly System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration _modelConfiguration;

	private readonly ConventionsConfiguration _conventionsConfiguration;

	private readonly DbModelBuilderVersion _modelBuilderVersion;

	private readonly object _lock = new object();

	public virtual ConventionsConfiguration Conventions => _conventionsConfiguration;

	public virtual ConfigurationRegistrar Configurations => new ConfigurationRegistrar(_modelConfiguration);

	internal DbModelBuilderVersion Version => _modelBuilderVersion;

	internal System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration ModelConfiguration => _modelConfiguration;

	public DbModelBuilder()
		: this(new System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration())
	{
	}

	public DbModelBuilder(DbModelBuilderVersion modelBuilderVersion)
		: this(new System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration(), modelBuilderVersion)
	{
		if (!Enum.IsDefined(typeof(DbModelBuilderVersion), modelBuilderVersion))
		{
			throw new ArgumentOutOfRangeException("modelBuilderVersion");
		}
	}

	internal DbModelBuilder(System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, DbModelBuilderVersion modelBuilderVersion = DbModelBuilderVersion.Latest)
		: this(modelConfiguration, new ConventionsConfiguration(SelectConventionSet(modelBuilderVersion)), modelBuilderVersion)
	{
	}

	private static ConventionSet SelectConventionSet(DbModelBuilderVersion modelBuilderVersion)
	{
		switch (modelBuilderVersion)
		{
		case DbModelBuilderVersion.V4_1:
			return V1ConventionSet.Conventions;
		case DbModelBuilderVersion.Latest:
		case DbModelBuilderVersion.V5_0_Net4:
		case DbModelBuilderVersion.V5_0:
		case DbModelBuilderVersion.V6_0:
			return V2ConventionSet.Conventions;
		default:
			throw new ArgumentOutOfRangeException("modelBuilderVersion");
		}
	}

	private DbModelBuilder(System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, ConventionsConfiguration conventionsConfiguration, DbModelBuilderVersion modelBuilderVersion = DbModelBuilderVersion.Latest)
	{
		if (!Enum.IsDefined(typeof(DbModelBuilderVersion), modelBuilderVersion))
		{
			throw new ArgumentOutOfRangeException("modelBuilderVersion");
		}
		_modelConfiguration = modelConfiguration;
		_conventionsConfiguration = conventionsConfiguration;
		_modelBuilderVersion = modelBuilderVersion;
	}

	private DbModelBuilder(DbModelBuilder source)
	{
		_modelConfiguration = source._modelConfiguration.Clone();
		_conventionsConfiguration = source._conventionsConfiguration.Clone();
		_modelBuilderVersion = source._modelBuilderVersion;
	}

	internal virtual DbModelBuilder Clone()
	{
		lock (_lock)
		{
			return new DbModelBuilder(this);
		}
	}

	internal DbModel BuildDynamicUpdateModel(DbProviderInfo providerInfo)
	{
		DbModel dbModel = Build(providerInfo);
		EntityContainerMapping entityContainerMapping = dbModel.DatabaseMapping.EntityContainerMappings.Single();
		entityContainerMapping.EntitySetMappings.Each(delegate(EntitySetMapping esm)
		{
			esm.ClearModificationFunctionMappings();
		});
		entityContainerMapping.AssociationSetMappings.Each((AssociationSetMapping asm) => asm.ModificationFunctionMapping = null);
		return dbModel;
	}

	public virtual DbModelBuilder Ignore<T>() where T : class
	{
		_modelConfiguration.Ignore(typeof(T));
		return this;
	}

	public virtual DbModelBuilder HasDefaultSchema(string schema)
	{
		_modelConfiguration.DefaultSchema = schema;
		return this;
	}

	public virtual DbModelBuilder Ignore(IEnumerable<Type> types)
	{
		Check.NotNull(types, "types");
		foreach (Type type in types)
		{
			_modelConfiguration.Ignore(type);
		}
		return this;
	}

	public virtual EntityTypeConfiguration<TEntityType> Entity<TEntityType>() where TEntityType : class
	{
		return new EntityTypeConfiguration<TEntityType>(_modelConfiguration.Entity(typeof(TEntityType), explicitEntity: true));
	}

	public virtual void RegisterEntityType(Type entityType)
	{
		Check.NotNull(entityType, "entityType");
		Entity(entityType);
	}

	internal virtual EntityTypeConfiguration Entity(Type entityType)
	{
		EntityTypeConfiguration entityTypeConfiguration = _modelConfiguration.Entity(entityType);
		entityTypeConfiguration.IsReplaceable = true;
		return entityTypeConfiguration;
	}

	public virtual ComplexTypeConfiguration<TComplexType> ComplexType<TComplexType>() where TComplexType : class
	{
		return new ComplexTypeConfiguration<TComplexType>(_modelConfiguration.ComplexType(typeof(TComplexType)));
	}

	public TypeConventionConfiguration Types()
	{
		return new TypeConventionConfiguration(_conventionsConfiguration);
	}

	public TypeConventionConfiguration<T> Types<T>() where T : class
	{
		return new TypeConventionConfiguration<T>(_conventionsConfiguration);
	}

	public PropertyConventionConfiguration Properties()
	{
		return new PropertyConventionConfiguration(_conventionsConfiguration);
	}

	public PropertyConventionConfiguration Properties<T>()
	{
		if (!typeof(T).IsValidEdmScalarType())
		{
			throw Error.ModelBuilder_PropertyFilterTypeMustBePrimitive(typeof(T));
		}
		return new PropertyConventionConfiguration(_conventionsConfiguration).Where(delegate(PropertyInfo p)
		{
			p.PropertyType.TryUnwrapNullableType(out var underlyingType);
			return underlyingType == typeof(T);
		});
	}

	public virtual DbModel Build(DbConnection providerConnection)
	{
		Check.NotNull(providerConnection, "providerConnection");
		DbProviderManifest providerManifest;
		DbProviderInfo providerInfo = providerConnection.GetProviderInfo(out providerManifest);
		return Build(providerManifest, providerInfo);
	}

	public virtual DbModel Build(DbProviderInfo providerInfo)
	{
		Check.NotNull(providerInfo, "providerInfo");
		DbProviderManifest providerManifest = GetProviderManifest(providerInfo);
		return Build(providerManifest, providerInfo);
	}

	private DbModel Build(DbProviderManifest providerManifest, DbProviderInfo providerInfo)
	{
		double edmVersion = _modelBuilderVersion.GetEdmVersion();
		DbModelBuilder modelBuilder = Clone();
		DbModel dbModel = new DbModel(new DbDatabaseMapping
		{
			Model = EdmModel.CreateConceptualModel(edmVersion),
			Database = EdmModel.CreateStoreModel(providerInfo, providerManifest, edmVersion)
		}, modelBuilder);
		dbModel.ConceptualModel.Container.AddAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:UseClrTypes", "true");
		_conventionsConfiguration.ApplyModelConfiguration(_modelConfiguration);
		_modelConfiguration.NormalizeConfigurations();
		MapTypes(dbModel.ConceptualModel);
		_modelConfiguration.Configure(dbModel.ConceptualModel);
		_conventionsConfiguration.ApplyConceptualModel(dbModel);
		dbModel.ConceptualModel.Validate();
		dbModel = new DbModel(dbModel.ConceptualModel.GenerateDatabaseMapping(providerInfo, providerManifest), modelBuilder);
		_conventionsConfiguration.ApplyPluralizingTableNameConvention(dbModel);
		_modelConfiguration.Configure(dbModel.DatabaseMapping, providerManifest);
		_conventionsConfiguration.ApplyStoreModel(dbModel);
		_conventionsConfiguration.ApplyMapping(dbModel.DatabaseMapping);
		dbModel.StoreModel.Validate();
		return dbModel;
	}

	private static DbProviderManifest GetProviderManifest(DbProviderInfo providerInfo)
	{
		return DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(providerInfo.ProviderInvariantName).GetProviderServices().GetProviderManifest(providerInfo.ProviderManifestToken);
	}

	private void MapTypes(EdmModel model)
	{
		TypeMapper typeMapper = new TypeMapper(new MappingContext(_modelConfiguration, _conventionsConfiguration, model, _modelBuilderVersion, DbConfiguration.DependencyResolver.GetService<AttributeProvider>()));
		IList<Type> list = (_modelConfiguration.Entities as IList<Type>) ?? _modelConfiguration.Entities.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Type type = list[i];
			if (typeMapper.MapEntityType(type) == null)
			{
				throw Error.InvalidEntityType(type);
			}
		}
		IList<Type> list2 = (_modelConfiguration.ComplexTypes as IList<Type>) ?? _modelConfiguration.ComplexTypes.ToList();
		for (int j = 0; j < list2.Count; j++)
		{
			Type type2 = list2[j];
			if (typeMapper.MapComplexType(type2) == null)
			{
				throw Error.CodeFirstInvalidComplexType(type2);
			}
		}
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

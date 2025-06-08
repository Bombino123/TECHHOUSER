using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration.Properties;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.ModelConfiguration.Conventions.Sets;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ConventionsConfiguration
{
	private class ModelConventionDispatcher : EdmModelVisitor
	{
		private readonly IConvention _convention;

		private readonly DbModel _model;

		private readonly DataSpace _dataSpace;

		public ModelConventionDispatcher(IConvention convention, DbModel model, DataSpace dataSpace)
		{
			Check.NotNull(convention, "convention");
			Check.NotNull(model, "model");
			_convention = convention;
			_model = model;
			_dataSpace = dataSpace;
		}

		public void Dispatch()
		{
			VisitEdmModel((_dataSpace == DataSpace.CSpace) ? _model.ConceptualModel : _model.StoreModel);
		}

		private void Dispatch<T>(T item) where T : MetadataItem
		{
			if (_dataSpace == DataSpace.CSpace)
			{
				if (_convention is IConceptualModelConvention<T> conceptualModelConvention)
				{
					conceptualModelConvention.Apply(item, _model);
				}
			}
			else if (_convention is IStoreModelConvention<T> storeModelConvention)
			{
				storeModelConvention.Apply(item, _model);
			}
		}

		protected internal override void VisitEdmModel(EdmModel item)
		{
			Dispatch(item);
			base.VisitEdmModel(item);
		}

		protected override void VisitEdmNavigationProperty(NavigationProperty item)
		{
			Dispatch(item);
			base.VisitEdmNavigationProperty(item);
		}

		protected override void VisitEdmAssociationConstraint(ReferentialConstraint item)
		{
			Dispatch(item);
			if (item != null)
			{
				VisitMetadataItem(item);
			}
		}

		protected override void VisitEdmAssociationEnd(RelationshipEndMember item)
		{
			Dispatch(item);
			base.VisitEdmAssociationEnd(item);
		}

		protected internal override void VisitEdmProperty(EdmProperty item)
		{
			Dispatch(item);
			base.VisitEdmProperty(item);
		}

		protected internal override void VisitMetadataItem(MetadataItem item)
		{
			Dispatch(item);
			base.VisitMetadataItem(item);
		}

		protected override void VisitEdmEntityContainer(EntityContainer item)
		{
			Dispatch(item);
			base.VisitEdmEntityContainer(item);
		}

		protected internal override void VisitEdmEntitySet(EntitySet item)
		{
			Dispatch(item);
			base.VisitEdmEntitySet(item);
		}

		protected override void VisitEdmAssociationSet(AssociationSet item)
		{
			Dispatch(item);
			base.VisitEdmAssociationSet(item);
		}

		protected override void VisitEdmAssociationSetEnd(EntitySet item)
		{
			Dispatch(item);
			base.VisitEdmAssociationSetEnd(item);
		}

		protected override void VisitComplexType(ComplexType item)
		{
			Dispatch(item);
			base.VisitComplexType(item);
		}

		protected internal override void VisitEdmEntityType(EntityType item)
		{
			Dispatch(item);
			VisitMetadataItem(item);
			if (item != null)
			{
				VisitDeclaredProperties(item, item.DeclaredProperties);
				VisitDeclaredNavigationProperties(item, item.DeclaredNavigationProperties);
			}
		}

		protected internal override void VisitEdmAssociationType(AssociationType item)
		{
			Dispatch(item);
			base.VisitEdmAssociationType(item);
		}
	}

	private class PropertyConfigurationConventionDispatcher
	{
		private readonly IConvention _convention;

		private readonly Type _propertyConfigurationType;

		private readonly PropertyInfo _propertyInfo;

		private readonly Func<PropertyConfiguration> _propertyConfiguration;

		private readonly ModelConfiguration _modelConfiguration;

		public PropertyConfigurationConventionDispatcher(IConvention convention, Type propertyConfigurationType, PropertyInfo propertyInfo, Func<PropertyConfiguration> propertyConfiguration, ModelConfiguration modelConfiguration)
		{
			Check.NotNull(convention, "convention");
			Check.NotNull(propertyConfigurationType, "propertyConfigurationType");
			Check.NotNull(propertyInfo, "propertyInfo");
			Check.NotNull(propertyConfiguration, "propertyConfiguration");
			_convention = convention;
			_propertyConfigurationType = propertyConfigurationType;
			_propertyInfo = propertyInfo;
			_propertyConfiguration = propertyConfiguration;
			_modelConfiguration = modelConfiguration;
		}

		public void Dispatch()
		{
			Dispatch<PropertyConfiguration>();
			Dispatch<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>();
			Dispatch<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration>();
			Dispatch<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>();
			Dispatch<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration>();
			Dispatch<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration>();
			Dispatch<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration>();
			Dispatch<NavigationPropertyConfiguration>();
		}

		private void Dispatch<TPropertyConfiguration>() where TPropertyConfiguration : PropertyConfiguration
		{
			if (_convention is IConfigurationConvention<PropertyInfo, TPropertyConfiguration> configurationConvention && typeof(TPropertyConfiguration).IsAssignableFrom(_propertyConfigurationType))
			{
				configurationConvention.Apply(_propertyInfo, () => (TPropertyConfiguration)_propertyConfiguration(), _modelConfiguration);
			}
		}
	}

	private readonly List<IConvention> _configurationConventions = new List<IConvention>();

	private readonly List<IConvention> _conceptualModelConventions = new List<IConvention>();

	private readonly List<IConvention> _conceptualToStoreMappingConventions = new List<IConvention>();

	private readonly List<IConvention> _storeModelConventions = new List<IConvention>();

	private readonly ConventionSet _initialConventionSet;

	internal IEnumerable<IConvention> ConfigurationConventions => _configurationConventions;

	internal IEnumerable<IConvention> ConceptualModelConventions => _conceptualModelConventions;

	internal IEnumerable<IConvention> ConceptualToStoreMappingConventions => _conceptualToStoreMappingConventions;

	internal IEnumerable<IConvention> StoreModelConventions => _storeModelConventions;

	internal ConventionsConfiguration()
		: this(V2ConventionSet.Conventions)
	{
	}

	internal ConventionsConfiguration(ConventionSet conventionSet)
	{
		_configurationConventions.AddRange(conventionSet.ConfigurationConventions);
		_conceptualModelConventions.AddRange(conventionSet.ConceptualModelConventions);
		_conceptualToStoreMappingConventions.AddRange(conventionSet.ConceptualToStoreMappingConventions);
		_storeModelConventions.AddRange(conventionSet.StoreModelConventions);
		_initialConventionSet = conventionSet;
	}

	private ConventionsConfiguration(ConventionsConfiguration source)
	{
		_configurationConventions.AddRange(source._configurationConventions);
		_conceptualModelConventions.AddRange(source._conceptualModelConventions);
		_conceptualToStoreMappingConventions.AddRange(source._conceptualToStoreMappingConventions);
		_storeModelConventions.AddRange(source._storeModelConventions);
	}

	internal virtual ConventionsConfiguration Clone()
	{
		return new ConventionsConfiguration(this);
	}

	public void AddFromAssembly(Assembly assembly)
	{
		Check.NotNull(assembly, "assembly");
		IOrderedEnumerable<Type> types = from type in assembly.GetAccessibleTypes()
			orderby type.Name
			select type;
		new ConventionsTypeFinder().AddConventions(types, delegate(IConvention convention)
		{
			Add(convention);
		});
	}

	public void Add(params IConvention[] conventions)
	{
		Check.NotNull(conventions, "conventions");
		foreach (IConvention convention in conventions)
		{
			bool flag = true;
			if (ConventionsTypeFilter.IsConfigurationConvention(convention.GetType()))
			{
				flag = false;
				int num = _configurationConventions.FindIndex((IConvention initialConvention) => _initialConventionSet.ConfigurationConventions.Contains(initialConvention));
				num = ((num == -1) ? _configurationConventions.Count : num);
				_configurationConventions.Insert(num, convention);
			}
			if (ConventionsTypeFilter.IsConceptualModelConvention(convention.GetType()))
			{
				flag = false;
				_conceptualModelConventions.Add(convention);
			}
			if (ConventionsTypeFilter.IsStoreModelConvention(convention.GetType()))
			{
				flag = false;
				_storeModelConventions.Add(convention);
			}
			if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(convention.GetType()))
			{
				flag = false;
				_conceptualToStoreMappingConventions.Add(convention);
			}
			if (flag)
			{
				throw new InvalidOperationException(Strings.ConventionsConfiguration_InvalidConventionType(convention.GetType()));
			}
		}
	}

	public void Add<TConvention>() where TConvention : IConvention, new()
	{
		Add(new TConvention());
	}

	public void AddAfter<TExistingConvention>(IConvention newConvention) where TExistingConvention : IConvention
	{
		Check.NotNull(newConvention, "newConvention");
		bool flag = true;
		if (ConventionsTypeFilter.IsConfigurationConvention(newConvention.GetType()) && ConventionsTypeFilter.IsConfigurationConvention(typeof(TExistingConvention)))
		{
			flag = false;
			Insert(typeof(TExistingConvention), 1, newConvention, _configurationConventions);
		}
		if (ConventionsTypeFilter.IsConceptualModelConvention(newConvention.GetType()) && ConventionsTypeFilter.IsConceptualModelConvention(typeof(TExistingConvention)))
		{
			flag = false;
			Insert(typeof(TExistingConvention), 1, newConvention, _conceptualModelConventions);
		}
		if (ConventionsTypeFilter.IsStoreModelConvention(newConvention.GetType()) && ConventionsTypeFilter.IsStoreModelConvention(typeof(TExistingConvention)))
		{
			flag = false;
			Insert(typeof(TExistingConvention), 1, newConvention, _storeModelConventions);
		}
		if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(newConvention.GetType()) && ConventionsTypeFilter.IsConceptualToStoreMappingConvention(typeof(TExistingConvention)))
		{
			flag = false;
			Insert(typeof(TExistingConvention), 1, newConvention, _conceptualToStoreMappingConventions);
		}
		if (flag)
		{
			throw new InvalidOperationException(Strings.ConventionsConfiguration_ConventionTypeMissmatch(newConvention.GetType(), typeof(TExistingConvention)));
		}
	}

	public void AddBefore<TExistingConvention>(IConvention newConvention) where TExistingConvention : IConvention
	{
		Check.NotNull(newConvention, "newConvention");
		bool flag = true;
		if (ConventionsTypeFilter.IsConfigurationConvention(newConvention.GetType()) && ConventionsTypeFilter.IsConfigurationConvention(typeof(TExistingConvention)))
		{
			flag = false;
			Insert(typeof(TExistingConvention), 0, newConvention, _configurationConventions);
		}
		if (ConventionsTypeFilter.IsConceptualModelConvention(newConvention.GetType()) && ConventionsTypeFilter.IsConceptualModelConvention(typeof(TExistingConvention)))
		{
			flag = false;
			Insert(typeof(TExistingConvention), 0, newConvention, _conceptualModelConventions);
		}
		if (ConventionsTypeFilter.IsStoreModelConvention(newConvention.GetType()) && ConventionsTypeFilter.IsStoreModelConvention(typeof(TExistingConvention)))
		{
			flag = false;
			Insert(typeof(TExistingConvention), 0, newConvention, _storeModelConventions);
		}
		if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(newConvention.GetType()) && ConventionsTypeFilter.IsConceptualToStoreMappingConvention(typeof(TExistingConvention)))
		{
			flag = false;
			Insert(typeof(TExistingConvention), 0, newConvention, _conceptualToStoreMappingConventions);
		}
		if (flag)
		{
			throw new InvalidOperationException(Strings.ConventionsConfiguration_ConventionTypeMissmatch(newConvention.GetType(), typeof(TExistingConvention)));
		}
	}

	private static void Insert(Type existingConventionType, int offset, IConvention newConvention, IList<IConvention> conventions)
	{
		int num = IndexOf(existingConventionType, conventions);
		if (num < 0)
		{
			throw Error.ConventionNotFound(newConvention.GetType(), existingConventionType);
		}
		conventions.Insert(num + offset, newConvention);
	}

	private static int IndexOf(Type existingConventionType, IList<IConvention> conventions)
	{
		int num = 0;
		foreach (IConvention convention in conventions)
		{
			if (convention.GetType() == existingConventionType)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public void Remove(params IConvention[] conventions)
	{
		Check.NotNull(conventions, "conventions");
		Check.NotNull(conventions, "conventions");
		foreach (IConvention convention in conventions)
		{
			if (ConventionsTypeFilter.IsConfigurationConvention(convention.GetType()))
			{
				_configurationConventions.Remove(convention);
			}
			if (ConventionsTypeFilter.IsConceptualModelConvention(convention.GetType()))
			{
				_conceptualModelConventions.Remove(convention);
			}
			if (ConventionsTypeFilter.IsStoreModelConvention(convention.GetType()))
			{
				_storeModelConventions.Remove(convention);
			}
			if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(convention.GetType()))
			{
				_conceptualToStoreMappingConventions.Remove(convention);
			}
		}
	}

	public void Remove<TConvention>() where TConvention : IConvention
	{
		if (ConventionsTypeFilter.IsConfigurationConvention(typeof(TConvention)))
		{
			_configurationConventions.RemoveAll((IConvention c) => c.GetType() == typeof(TConvention));
		}
		if (ConventionsTypeFilter.IsConceptualModelConvention(typeof(TConvention)))
		{
			_conceptualModelConventions.RemoveAll((IConvention c) => c.GetType() == typeof(TConvention));
		}
		if (ConventionsTypeFilter.IsStoreModelConvention(typeof(TConvention)))
		{
			_storeModelConventions.RemoveAll((IConvention c) => c.GetType() == typeof(TConvention));
		}
		if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(typeof(TConvention)))
		{
			_conceptualToStoreMappingConventions.RemoveAll((IConvention c) => c.GetType() == typeof(TConvention));
		}
	}

	internal void ApplyConceptualModel(DbModel model)
	{
		foreach (IConvention conceptualModelConvention in _conceptualModelConventions)
		{
			new ModelConventionDispatcher(conceptualModelConvention, model, DataSpace.CSpace).Dispatch();
		}
	}

	internal void ApplyStoreModel(DbModel model)
	{
		foreach (IConvention storeModelConvention in _storeModelConventions)
		{
			new ModelConventionDispatcher(storeModelConvention, model, DataSpace.SSpace).Dispatch();
		}
	}

	internal void ApplyPluralizingTableNameConvention(DbModel model)
	{
		foreach (IConvention item in _storeModelConventions.Where((IConvention c) => c is PluralizingTableNameConvention))
		{
			new ModelConventionDispatcher(item, model, DataSpace.SSpace).Dispatch();
		}
	}

	internal void ApplyMapping(DbDatabaseMapping databaseMapping)
	{
		foreach (IConvention conceptualToStoreMappingConvention in _conceptualToStoreMappingConventions)
		{
			if (conceptualToStoreMappingConvention is IDbMappingConvention dbMappingConvention)
			{
				dbMappingConvention.Apply(databaseMapping);
			}
		}
	}

	internal virtual void ApplyModelConfiguration(ModelConfiguration modelConfiguration)
	{
		for (int num = _configurationConventions.Count - 1; num >= 0; num--)
		{
			IConvention convention = _configurationConventions[num];
			if (convention is IConfigurationConvention configurationConvention)
			{
				configurationConvention.Apply(modelConfiguration);
			}
			if (convention is Convention convention2)
			{
				convention2.ApplyModelConfiguration(modelConfiguration);
			}
		}
	}

	internal virtual void ApplyModelConfiguration(Type type, ModelConfiguration modelConfiguration)
	{
		for (int num = _configurationConventions.Count - 1; num >= 0; num--)
		{
			IConvention convention = _configurationConventions[num];
			if (convention is IConfigurationConvention<Type> configurationConvention)
			{
				configurationConvention.Apply(type, modelConfiguration);
			}
			if (convention is Convention convention2)
			{
				convention2.ApplyModelConfiguration(type, modelConfiguration);
			}
		}
	}

	internal virtual void ApplyTypeConfiguration<TStructuralTypeConfiguration>(Type type, Func<TStructuralTypeConfiguration> structuralTypeConfiguration, ModelConfiguration modelConfiguration) where TStructuralTypeConfiguration : StructuralTypeConfiguration
	{
		for (int num = _configurationConventions.Count - 1; num >= 0; num--)
		{
			IConvention convention = _configurationConventions[num];
			if (convention is IConfigurationConvention<Type, TStructuralTypeConfiguration> configurationConvention)
			{
				configurationConvention.Apply(type, structuralTypeConfiguration, modelConfiguration);
			}
			if (convention is IConfigurationConvention<Type, StructuralTypeConfiguration> configurationConvention2)
			{
				configurationConvention2.Apply(type, structuralTypeConfiguration, modelConfiguration);
			}
			if (convention is Convention convention2)
			{
				convention2.ApplyTypeConfiguration(type, structuralTypeConfiguration, modelConfiguration);
			}
		}
	}

	internal virtual void ApplyPropertyConfiguration(PropertyInfo propertyInfo, ModelConfiguration modelConfiguration)
	{
		for (int num = _configurationConventions.Count - 1; num >= 0; num--)
		{
			IConvention convention = _configurationConventions[num];
			if (convention is IConfigurationConvention<PropertyInfo> configurationConvention)
			{
				configurationConvention.Apply(propertyInfo, modelConfiguration);
			}
			if (convention is Convention convention2)
			{
				convention2.ApplyPropertyConfiguration(propertyInfo, modelConfiguration);
			}
		}
	}

	internal virtual void ApplyPropertyConfiguration(PropertyInfo propertyInfo, Func<PropertyConfiguration> propertyConfiguration, ModelConfiguration modelConfiguration)
	{
		Type propertyConfigurationType = StructuralTypeConfiguration.GetPropertyConfigurationType(propertyInfo.PropertyType);
		for (int num = _configurationConventions.Count - 1; num >= 0; num--)
		{
			IConvention convention = _configurationConventions[num];
			new PropertyConfigurationConventionDispatcher(convention, propertyConfigurationType, propertyInfo, propertyConfiguration, modelConfiguration).Dispatch();
			if (convention is Convention convention2)
			{
				convention2.ApplyPropertyConfiguration(propertyInfo, propertyConfiguration, modelConfiguration);
			}
		}
	}

	internal virtual void ApplyPropertyTypeConfiguration<TStructuralTypeConfiguration>(PropertyInfo propertyInfo, Func<TStructuralTypeConfiguration> structuralTypeConfiguration, ModelConfiguration modelConfiguration) where TStructuralTypeConfiguration : StructuralTypeConfiguration
	{
		for (int num = _configurationConventions.Count - 1; num >= 0; num--)
		{
			IConvention convention = _configurationConventions[num];
			if (convention is IConfigurationConvention<PropertyInfo, TStructuralTypeConfiguration> configurationConvention)
			{
				configurationConvention.Apply(propertyInfo, structuralTypeConfiguration, modelConfiguration);
			}
			if (convention is IConfigurationConvention<PropertyInfo, StructuralTypeConfiguration> configurationConvention2)
			{
				configurationConvention2.Apply(propertyInfo, structuralTypeConfiguration, modelConfiguration);
			}
			if (convention is Convention convention2)
			{
				convention2.ApplyPropertyTypeConfiguration(propertyInfo, structuralTypeConfiguration, modelConfiguration);
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

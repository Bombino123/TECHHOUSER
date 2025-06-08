using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Edm.Services;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;

internal class NavigationPropertyConfiguration : PropertyConfiguration
{
	private readonly PropertyInfo _navigationProperty;

	private RelationshipMultiplicity? _endKind;

	private PropertyInfo _inverseNavigationProperty;

	private RelationshipMultiplicity? _inverseEndKind;

	private ConstraintConfiguration _constraint;

	private AssociationMappingConfiguration _associationMappingConfiguration;

	private ModificationStoredProceduresConfiguration _modificationStoredProceduresConfiguration;

	public OperationAction? DeleteAction { get; set; }

	internal PropertyInfo NavigationProperty => _navigationProperty;

	public RelationshipMultiplicity? RelationshipMultiplicity
	{
		get
		{
			return _endKind;
		}
		set
		{
			Check.NotNull(value, "value");
			_endKind = value;
		}
	}

	internal PropertyInfo InverseNavigationProperty
	{
		get
		{
			return _inverseNavigationProperty;
		}
		set
		{
			if (value == _navigationProperty)
			{
				throw Error.NavigationInverseItself(value.Name, value.ReflectedType);
			}
			_inverseNavigationProperty = value;
		}
	}

	internal RelationshipMultiplicity? InverseEndKind
	{
		get
		{
			return _inverseEndKind;
		}
		set
		{
			_inverseEndKind = value;
		}
	}

	public ConstraintConfiguration Constraint
	{
		get
		{
			return _constraint;
		}
		set
		{
			Check.NotNull(value, "value");
			_constraint = value;
		}
	}

	internal bool? IsNavigationPropertyDeclaringTypePrincipal { get; set; }

	internal AssociationMappingConfiguration AssociationMappingConfiguration
	{
		get
		{
			return _associationMappingConfiguration;
		}
		set
		{
			_associationMappingConfiguration = value;
		}
	}

	internal ModificationStoredProceduresConfiguration ModificationStoredProceduresConfiguration
	{
		get
		{
			return _modificationStoredProceduresConfiguration;
		}
		set
		{
			_modificationStoredProceduresConfiguration = value;
		}
	}

	internal NavigationPropertyConfiguration(PropertyInfo navigationProperty)
	{
		_navigationProperty = navigationProperty;
	}

	private NavigationPropertyConfiguration(NavigationPropertyConfiguration source)
	{
		_navigationProperty = source._navigationProperty;
		_endKind = source._endKind;
		_inverseNavigationProperty = source._inverseNavigationProperty;
		_inverseEndKind = source._inverseEndKind;
		_constraint = ((source._constraint == null) ? null : source._constraint.Clone());
		_associationMappingConfiguration = ((source._associationMappingConfiguration == null) ? null : source._associationMappingConfiguration.Clone());
		DeleteAction = source.DeleteAction;
		IsNavigationPropertyDeclaringTypePrincipal = source.IsNavigationPropertyDeclaringTypePrincipal;
		_modificationStoredProceduresConfiguration = ((source._modificationStoredProceduresConfiguration == null) ? null : source._modificationStoredProceduresConfiguration.Clone());
	}

	internal virtual NavigationPropertyConfiguration Clone()
	{
		return new NavigationPropertyConfiguration(this);
	}

	internal void Configure(NavigationProperty navigationProperty, EdmModel model, EntityTypeConfiguration entityTypeConfiguration)
	{
		navigationProperty.SetConfiguration(this);
		AssociationType association = navigationProperty.Association;
		NavigationPropertyConfiguration navigationPropertyConfiguration = association.GetConfiguration() as NavigationPropertyConfiguration;
		if (navigationPropertyConfiguration == null)
		{
			association.SetConfiguration(this);
		}
		else
		{
			EnsureConsistency(navigationPropertyConfiguration);
		}
		ConfigureInverse(association, model);
		ConfigureEndKinds(association, navigationPropertyConfiguration);
		ConfigureDependentBehavior(association, model, entityTypeConfiguration);
	}

	internal void Configure(AssociationSetMapping associationSetMapping, DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
	{
		if (AssociationMappingConfiguration != null)
		{
			associationSetMapping.SetConfiguration(this);
			AssociationMappingConfiguration.Configure(associationSetMapping, databaseMapping.Database, _navigationProperty);
		}
		if (_modificationStoredProceduresConfiguration != null)
		{
			if (associationSetMapping.ModificationFunctionMapping == null)
			{
				new ModificationFunctionMappingGenerator(providerManifest).Generate(associationSetMapping, databaseMapping);
			}
			_modificationStoredProceduresConfiguration.Configure(associationSetMapping.ModificationFunctionMapping, providerManifest);
		}
	}

	private void ConfigureInverse(AssociationType associationType, EdmModel model)
	{
		if (_inverseNavigationProperty == null)
		{
			return;
		}
		NavigationProperty navigationProperty = model.GetNavigationProperty(_inverseNavigationProperty);
		if (navigationProperty != null && navigationProperty.Association != associationType)
		{
			associationType.SourceEnd.RelationshipMultiplicity = navigationProperty.Association.TargetEnd.RelationshipMultiplicity;
			if (associationType.Constraint == null && _constraint == null && navigationProperty.Association.Constraint != null)
			{
				associationType.Constraint = navigationProperty.Association.Constraint;
				associationType.Constraint.FromRole = associationType.SourceEnd;
				associationType.Constraint.ToRole = associationType.TargetEnd;
			}
			model.RemoveAssociationType(navigationProperty.Association);
			navigationProperty.RelationshipType = associationType;
			navigationProperty.FromEndMember = associationType.TargetEnd;
			navigationProperty.ToEndMember = associationType.SourceEnd;
		}
	}

	private void ConfigureEndKinds(AssociationType associationType, NavigationPropertyConfiguration configuration)
	{
		AssociationEndMember associationEndMember = associationType.SourceEnd;
		AssociationEndMember associationEndMember2 = associationType.TargetEnd;
		if (configuration != null && configuration.InverseNavigationProperty != null)
		{
			associationEndMember = associationType.TargetEnd;
			associationEndMember2 = associationType.SourceEnd;
		}
		if (_inverseEndKind.HasValue)
		{
			associationEndMember.RelationshipMultiplicity = _inverseEndKind.Value;
		}
		if (_endKind.HasValue)
		{
			associationEndMember2.RelationshipMultiplicity = _endKind.Value;
		}
	}

	private void EnsureConsistency(NavigationPropertyConfiguration navigationPropertyConfiguration)
	{
		if (RelationshipMultiplicity.HasValue)
		{
			if (!navigationPropertyConfiguration.InverseEndKind.HasValue)
			{
				navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity;
			}
			else if (navigationPropertyConfiguration.InverseEndKind != RelationshipMultiplicity)
			{
				throw Error.ConflictingMultiplicities(NavigationProperty.Name, NavigationProperty.ReflectedType);
			}
		}
		if (InverseEndKind.HasValue)
		{
			if (!navigationPropertyConfiguration.RelationshipMultiplicity.HasValue)
			{
				navigationPropertyConfiguration.RelationshipMultiplicity = InverseEndKind;
			}
			else if (navigationPropertyConfiguration.RelationshipMultiplicity != InverseEndKind)
			{
				if (InverseNavigationProperty == null)
				{
					throw Error.ConflictingMultiplicities(NavigationProperty.Name, NavigationProperty.ReflectedType);
				}
				throw Error.ConflictingMultiplicities(InverseNavigationProperty.Name, InverseNavigationProperty.ReflectedType);
			}
		}
		if (DeleteAction.HasValue)
		{
			if (!navigationPropertyConfiguration.DeleteAction.HasValue)
			{
				navigationPropertyConfiguration.DeleteAction = DeleteAction;
			}
			else if (navigationPropertyConfiguration.DeleteAction != DeleteAction)
			{
				throw Error.ConflictingCascadeDeleteOperation(NavigationProperty.Name, NavigationProperty.ReflectedType);
			}
		}
		if (Constraint != null)
		{
			if (navigationPropertyConfiguration.Constraint == null)
			{
				navigationPropertyConfiguration.Constraint = Constraint;
			}
			else if (!object.Equals(navigationPropertyConfiguration.Constraint, Constraint))
			{
				throw Error.ConflictingConstraint(NavigationProperty.Name, NavigationProperty.ReflectedType);
			}
		}
		if (IsNavigationPropertyDeclaringTypePrincipal.HasValue)
		{
			if (!navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal.HasValue)
			{
				navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal = !IsNavigationPropertyDeclaringTypePrincipal;
			}
			else if (navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal == IsNavigationPropertyDeclaringTypePrincipal)
			{
				throw Error.ConflictingConstraint(NavigationProperty.Name, NavigationProperty.ReflectedType);
			}
		}
		if (AssociationMappingConfiguration != null)
		{
			if (navigationPropertyConfiguration.AssociationMappingConfiguration == null)
			{
				navigationPropertyConfiguration.AssociationMappingConfiguration = AssociationMappingConfiguration;
			}
			else if (!object.Equals(navigationPropertyConfiguration.AssociationMappingConfiguration, AssociationMappingConfiguration))
			{
				throw Error.ConflictingMapping(NavigationProperty.Name, NavigationProperty.ReflectedType);
			}
		}
		if (ModificationStoredProceduresConfiguration != null)
		{
			if (navigationPropertyConfiguration.ModificationStoredProceduresConfiguration == null)
			{
				navigationPropertyConfiguration.ModificationStoredProceduresConfiguration = ModificationStoredProceduresConfiguration;
			}
			else if (!navigationPropertyConfiguration.ModificationStoredProceduresConfiguration.IsCompatibleWith(ModificationStoredProceduresConfiguration))
			{
				throw Error.ConflictingFunctionsMapping(NavigationProperty.Name, NavigationProperty.ReflectedType);
			}
		}
	}

	private void ConfigureDependentBehavior(AssociationType associationType, EdmModel model, EntityTypeConfiguration entityTypeConfiguration)
	{
		if (!associationType.TryGuessPrincipalAndDependentEnds(out var principalEnd, out var dependentEnd))
		{
			if (IsNavigationPropertyDeclaringTypePrincipal.HasValue)
			{
				associationType.MarkPrincipalConfigured();
				NavigationProperty navigationProperty = model.EntityTypes.SelectMany((EntityType et) => et.DeclaredNavigationProperties).Single((NavigationProperty np) => np.RelationshipType.Equals(associationType) && np.GetClrPropertyInfo().IsSameAs(NavigationProperty));
				principalEnd = (IsNavigationPropertyDeclaringTypePrincipal.Value ? associationType.GetOtherEnd(navigationProperty.ResultEnd) : navigationProperty.ResultEnd);
				dependentEnd = associationType.GetOtherEnd(principalEnd);
				if (associationType.SourceEnd != principalEnd)
				{
					associationType.SourceEnd = principalEnd;
					associationType.TargetEnd = dependentEnd;
					AssociationSet associationSet = model.Containers.SelectMany((EntityContainer ct) => ct.AssociationSets).Single((AssociationSet aset) => aset.ElementType == associationType);
					EntitySet sourceSet = associationSet.SourceSet;
					associationSet.SourceSet = associationSet.TargetSet;
					associationSet.TargetSet = sourceSet;
				}
			}
			if (principalEnd == null)
			{
				dependentEnd = associationType.TargetEnd;
			}
		}
		ConfigureConstraint(associationType, dependentEnd, entityTypeConfiguration);
		ConfigureDeleteAction(associationType.GetOtherEnd(dependentEnd));
	}

	private void ConfigureConstraint(AssociationType associationType, AssociationEndMember dependentEnd, EntityTypeConfiguration entityTypeConfiguration)
	{
		if (_constraint != null)
		{
			_constraint.Configure(associationType, dependentEnd, entityTypeConfiguration);
			ReferentialConstraint constraint = associationType.Constraint;
			if (constraint != null && constraint.ToProperties.SequenceEqual(constraint.ToRole.GetEntityType().KeyProperties) && !_inverseEndKind.HasValue && associationType.SourceEnd.IsMany())
			{
				associationType.SourceEnd.RelationshipMultiplicity = System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne;
				associationType.TargetEnd.RelationshipMultiplicity = System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One;
			}
		}
	}

	private void ConfigureDeleteAction(AssociationEndMember principalEnd)
	{
		if (DeleteAction.HasValue)
		{
			principalEnd.DeleteBehavior = DeleteAction.Value;
		}
	}

	internal void Reset()
	{
		_endKind = null;
		_inverseNavigationProperty = null;
		_inverseEndKind = null;
		_constraint = null;
		_associationMappingConfiguration = null;
	}
}

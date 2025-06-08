using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;

internal class ConventionNavigationPropertyConfiguration
{
	private readonly NavigationPropertyConfiguration _configuration;

	private readonly ModelConfiguration _modelConfiguration;

	public virtual PropertyInfo ClrPropertyInfo
	{
		get
		{
			if (_configuration == null)
			{
				return null;
			}
			return _configuration.NavigationProperty;
		}
	}

	internal NavigationPropertyConfiguration Configuration => _configuration;

	internal ConventionNavigationPropertyConfiguration(NavigationPropertyConfiguration configuration, ModelConfiguration modelConfiguration)
	{
		_configuration = configuration;
		_modelConfiguration = modelConfiguration;
	}

	public virtual void HasConstraint<T>() where T : ConstraintConfiguration
	{
		HasConstraintInternal<T>(null);
	}

	public virtual void HasConstraint<T>(Action<T> constraintConfigurationAction) where T : ConstraintConfiguration
	{
		Check.NotNull(constraintConfigurationAction, "constraintConfigurationAction");
		HasConstraintInternal(constraintConfigurationAction);
	}

	private void HasConstraintInternal<T>(Action<T> constraintConfigurationAction) where T : ConstraintConfiguration
	{
		if (_configuration == null || HasConfiguredConstraint())
		{
			return;
		}
		Type typeFromHandle = typeof(T);
		if (_configuration.Constraint == null)
		{
			if (typeFromHandle == typeof(IndependentConstraintConfiguration))
			{
				_configuration.Constraint = IndependentConstraintConfiguration.Instance;
			}
			else
			{
				_configuration.Constraint = (ConstraintConfiguration)Activator.CreateInstance(typeFromHandle);
			}
		}
		else if (_configuration.Constraint.GetType() != typeFromHandle)
		{
			return;
		}
		constraintConfigurationAction?.Invoke((T)_configuration.Constraint);
	}

	private bool HasConfiguredConstraint()
	{
		if (_configuration != null && _configuration.Constraint != null && _configuration.Constraint.IsFullySpecified)
		{
			return true;
		}
		if (_configuration != null && _configuration.InverseNavigationProperty != null)
		{
			Type targetType = _configuration.NavigationProperty.PropertyType.GetTargetType();
			if (_modelConfiguration.Entities.Contains(targetType))
			{
				EntityTypeConfiguration entityTypeConfiguration = _modelConfiguration.Entity(targetType);
				if (entityTypeConfiguration.IsNavigationPropertyConfigured(_configuration.InverseNavigationProperty))
				{
					return entityTypeConfiguration.Navigation(_configuration.InverseNavigationProperty).Constraint != null;
				}
			}
		}
		return false;
	}

	public virtual ConventionNavigationPropertyConfiguration HasInverseNavigationProperty(Func<PropertyInfo, PropertyInfo> inverseNavigationPropertyGetter)
	{
		Check.NotNull(inverseNavigationPropertyGetter, "inverseNavigationPropertyGetter");
		if (_configuration != null && _configuration.InverseNavigationProperty == null)
		{
			PropertyInfo propertyInfo = inverseNavigationPropertyGetter(ClrPropertyInfo);
			Check.NotNull(propertyInfo, "inverseNavigationProperty");
			if (!propertyInfo.IsValidEdmNavigationProperty())
			{
				throw new InvalidOperationException(Strings.LightweightEntityConfiguration_InvalidNavigationProperty(propertyInfo.Name));
			}
			if (!propertyInfo.DeclaringType.IsAssignableFrom(_configuration.NavigationProperty.PropertyType.GetTargetType()))
			{
				throw new InvalidOperationException(Strings.LightweightEntityConfiguration_MismatchedInverseNavigationProperty(_configuration.NavigationProperty.PropertyType.GetTargetType().FullName, _configuration.NavigationProperty.Name, propertyInfo.DeclaringType.FullName, propertyInfo.Name));
			}
			if (!_configuration.NavigationProperty.DeclaringType.IsAssignableFrom(propertyInfo.PropertyType.GetTargetType()))
			{
				throw new InvalidOperationException(Strings.LightweightEntityConfiguration_InvalidInverseNavigationProperty(_configuration.NavigationProperty.DeclaringType.FullName, _configuration.NavigationProperty.Name, propertyInfo.PropertyType.GetTargetType().FullName, propertyInfo.Name));
			}
			if (_configuration.InverseEndKind.HasValue)
			{
				VerifyMultiplicityCompatibility(_configuration.InverseEndKind.Value, propertyInfo);
			}
			_modelConfiguration.Entity(_configuration.NavigationProperty.PropertyType.GetTargetType()).Navigation(propertyInfo);
			_configuration.InverseNavigationProperty = propertyInfo;
		}
		return this;
	}

	public virtual ConventionNavigationPropertyConfiguration HasInverseEndMultiplicity(RelationshipMultiplicity multiplicity)
	{
		if (_configuration != null && !_configuration.InverseEndKind.HasValue)
		{
			if (_configuration.InverseNavigationProperty != null)
			{
				VerifyMultiplicityCompatibility(multiplicity, _configuration.InverseNavigationProperty);
			}
			_configuration.InverseEndKind = multiplicity;
		}
		return this;
	}

	public virtual ConventionNavigationPropertyConfiguration IsDeclaringTypePrincipal(bool isPrincipal)
	{
		if (_configuration != null && !_configuration.IsNavigationPropertyDeclaringTypePrincipal.HasValue)
		{
			_configuration.IsNavigationPropertyDeclaringTypePrincipal = isPrincipal;
		}
		return this;
	}

	public virtual ConventionNavigationPropertyConfiguration HasDeleteAction(OperationAction deleteAction)
	{
		if (_configuration != null && !_configuration.DeleteAction.HasValue)
		{
			_configuration.DeleteAction = deleteAction;
		}
		return this;
	}

	public virtual ConventionNavigationPropertyConfiguration HasRelationshipMultiplicity(RelationshipMultiplicity multiplicity)
	{
		if (_configuration != null && !_configuration.RelationshipMultiplicity.HasValue)
		{
			VerifyMultiplicityCompatibility(multiplicity, _configuration.NavigationProperty);
			_configuration.RelationshipMultiplicity = multiplicity;
		}
		return this;
	}

	private static void VerifyMultiplicityCompatibility(RelationshipMultiplicity multiplicity, PropertyInfo propertyInfo)
	{
		bool flag = true;
		switch (multiplicity)
		{
		case RelationshipMultiplicity.Many:
			flag = propertyInfo.PropertyType.IsCollection();
			break;
		case RelationshipMultiplicity.ZeroOrOne:
		case RelationshipMultiplicity.One:
			flag = !propertyInfo.PropertyType.IsCollection();
			break;
		default:
			throw new InvalidOperationException(Strings.LightweightNavigationPropertyConfiguration_InvalidMultiplicity(multiplicity));
		}
		if (!flag)
		{
			throw new InvalidOperationException(Strings.LightweightNavigationPropertyConfiguration_IncompatibleMultiplicity(RelationshipMultiplicityConverter.MultiplicityToString(multiplicity), propertyInfo.DeclaringType.Name + "." + propertyInfo.Name, propertyInfo.PropertyType));
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

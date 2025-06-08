using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;

internal class ForeignKeyConstraintConfiguration : ConstraintConfiguration
{
	private readonly List<PropertyInfo> _dependentProperties = new List<PropertyInfo>();

	private readonly bool _isFullySpecified;

	public override bool IsFullySpecified => _isFullySpecified;

	internal IEnumerable<PropertyInfo> ToProperties => _dependentProperties;

	public ForeignKeyConstraintConfiguration()
	{
	}

	internal ForeignKeyConstraintConfiguration(IEnumerable<PropertyInfo> dependentProperties)
	{
		_dependentProperties.AddRange(dependentProperties);
		_isFullySpecified = true;
	}

	private ForeignKeyConstraintConfiguration(ForeignKeyConstraintConfiguration source)
	{
		_dependentProperties.AddRange(source._dependentProperties);
		_isFullySpecified = source._isFullySpecified;
	}

	internal override ConstraintConfiguration Clone()
	{
		return new ForeignKeyConstraintConfiguration(this);
	}

	public void AddColumn(PropertyInfo propertyInfo)
	{
		Check.NotNull(propertyInfo, "propertyInfo");
		if (!_dependentProperties.ContainsSame(propertyInfo))
		{
			_dependentProperties.Add(propertyInfo);
		}
	}

	internal override void Configure(AssociationType associationType, AssociationEndMember dependentEnd, EntityTypeConfiguration entityTypeConfiguration)
	{
		if (!_dependentProperties.Any())
		{
			return;
		}
		IEnumerable<PropertyInfo> enumerable = _dependentProperties.AsEnumerable();
		if (!IsFullySpecified)
		{
			if (dependentEnd.GetEntityType().GetClrType() != entityTypeConfiguration.ClrType)
			{
				return;
			}
			var source = _dependentProperties.Select((PropertyInfo p) => new
			{
				PropertyInfo = p,
				ColumnOrder = entityTypeConfiguration.Property(new PropertyPath(p)).ColumnOrder
			});
			if (_dependentProperties.Count > 1 && source.Any(p => !p.ColumnOrder.HasValue))
			{
				ReadOnlyMetadataCollection<EdmProperty> dependentKeys = dependentEnd.GetEntityType().KeyProperties;
				if (dependentKeys.Count != _dependentProperties.Count || !source.All(fk => dependentKeys.Any((EdmProperty p) => p.GetClrPropertyInfo().IsSameAs(fk.PropertyInfo))))
				{
					throw Error.ForeignKeyAttributeConvention_OrderRequired(entityTypeConfiguration.ClrType);
				}
				enumerable = dependentKeys.Select((EdmProperty p) => p.GetClrPropertyInfo());
			}
			else
			{
				enumerable = from p in source
					orderby p.ColumnOrder
					select p.PropertyInfo;
			}
		}
		List<EdmProperty> list = new List<EdmProperty>();
		foreach (PropertyInfo item in enumerable)
		{
			EdmProperty declaredPrimitiveProperty = dependentEnd.GetEntityType().GetDeclaredPrimitiveProperty(item);
			if (declaredPrimitiveProperty == null)
			{
				throw Error.ForeignKeyPropertyNotFound(item.Name, dependentEnd.GetEntityType().Name);
			}
			list.Add(declaredPrimitiveProperty);
		}
		AssociationEndMember otherEnd = associationType.GetOtherEnd(dependentEnd);
		ReferentialConstraint referentialConstraint = new ReferentialConstraint(otherEnd, dependentEnd, otherEnd.GetEntityType().KeyProperties, list);
		if (otherEnd.IsRequired())
		{
			referentialConstraint.ToProperties.Each((EdmProperty p) => p.Nullable = false);
		}
		associationType.Constraint = referentialConstraint;
	}

	public bool Equals(ForeignKeyConstraintConfiguration other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return other.ToProperties.SequenceEqual(ToProperties, new DynamicEqualityComparer<PropertyInfo>((PropertyInfo p1, PropertyInfo p2) => p1.IsSameAs(p2)));
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != typeof(ForeignKeyConstraintConfiguration))
		{
			return false;
		}
		return Equals((ForeignKeyConstraintConfiguration)obj);
	}

	public override int GetHashCode()
	{
		return ToProperties.Aggregate(0, (int t, PropertyInfo p) => t + p.GetHashCode());
	}
}

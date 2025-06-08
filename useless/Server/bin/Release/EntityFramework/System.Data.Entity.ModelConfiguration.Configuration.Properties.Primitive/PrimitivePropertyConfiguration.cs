using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Internal;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

internal class PrimitivePropertyConfiguration : PropertyConfiguration
{
	private readonly IDictionary<string, object> _annotations = new Dictionary<string, object>();

	public bool? IsNullable { get; set; }

	public ConcurrencyMode? ConcurrencyMode { get; set; }

	public DatabaseGeneratedOption? DatabaseGeneratedOption { get; set; }

	public string ColumnType { get; set; }

	public string ColumnName { get; set; }

	public IDictionary<string, object> Annotations => _annotations;

	public string ParameterName { get; set; }

	public int? ColumnOrder { get; set; }

	internal OverridableConfigurationParts OverridableConfigurationParts { get; set; }

	internal StructuralTypeConfiguration TypeConfiguration { get; set; }

	public PrimitivePropertyConfiguration()
	{
		OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace;
	}

	protected PrimitivePropertyConfiguration(PrimitivePropertyConfiguration source)
	{
		Check.NotNull(source, "source");
		TypeConfiguration = source.TypeConfiguration;
		IsNullable = source.IsNullable;
		ConcurrencyMode = source.ConcurrencyMode;
		DatabaseGeneratedOption = source.DatabaseGeneratedOption;
		ColumnType = source.ColumnType;
		ColumnName = source.ColumnName;
		ParameterName = source.ParameterName;
		ColumnOrder = source.ColumnOrder;
		OverridableConfigurationParts = source.OverridableConfigurationParts;
		foreach (KeyValuePair<string, object> annotation in source._annotations)
		{
			_annotations.Add(annotation);
		}
	}

	internal virtual PrimitivePropertyConfiguration Clone()
	{
		return new PrimitivePropertyConfiguration(this);
	}

	public virtual void SetAnnotation(string name, object value)
	{
		if (!name.IsValidUndottedName())
		{
			throw new ArgumentException(Strings.BadAnnotationName(name));
		}
		_annotations[name] = value;
	}

	internal virtual void Configure(EdmProperty property)
	{
		Clone().MergeWithExistingConfiguration(property, delegate(string errorMessage)
		{
			PropertyInfo clrPropertyInfo = property.GetClrPropertyInfo();
			string p = ((clrPropertyInfo == null) ? string.Empty : ObjectContextTypeCache.GetObjectType(clrPropertyInfo.DeclaringType).FullNameWithNesting());
			return Error.ConflictingPropertyConfiguration(property.Name, p, errorMessage);
		}, inCSpace: true, fillFromExistingConfiguration: false).ConfigureProperty(property);
	}

	private PrimitivePropertyConfiguration MergeWithExistingConfiguration(EdmProperty property, Func<string, Exception> getConflictException, bool inCSpace, bool fillFromExistingConfiguration)
	{
		if (property.GetConfiguration() is PrimitivePropertyConfiguration primitivePropertyConfiguration)
		{
			OverridableConfigurationParts overridableConfigurationParts = (inCSpace ? OverridableConfigurationParts.OverridableInCSpace : OverridableConfigurationParts.OverridableInSSpace);
			if (primitivePropertyConfiguration.OverridableConfigurationParts.HasFlag(overridableConfigurationParts) || fillFromExistingConfiguration)
			{
				return primitivePropertyConfiguration.OverrideFrom(this, inCSpace);
			}
			if (OverridableConfigurationParts.HasFlag(overridableConfigurationParts) || primitivePropertyConfiguration.IsCompatible(this, inCSpace, out var errorMessage))
			{
				return OverrideFrom(primitivePropertyConfiguration, inCSpace);
			}
			throw getConflictException(errorMessage);
		}
		return this;
	}

	private PrimitivePropertyConfiguration OverrideFrom(PrimitivePropertyConfiguration overridingConfiguration, bool inCSpace)
	{
		if (overridingConfiguration.GetType().IsAssignableFrom(GetType()))
		{
			MakeCompatibleWith(overridingConfiguration, inCSpace);
			FillFrom(overridingConfiguration, inCSpace);
			return this;
		}
		overridingConfiguration.FillFrom(this, inCSpace);
		return overridingConfiguration;
	}

	protected virtual void ConfigureProperty(EdmProperty property)
	{
		if (IsNullable.HasValue)
		{
			property.Nullable = IsNullable.Value;
		}
		if (ConcurrencyMode.HasValue)
		{
			property.ConcurrencyMode = ConcurrencyMode.Value;
		}
		if (DatabaseGeneratedOption.HasValue)
		{
			property.SetStoreGeneratedPattern((StoreGeneratedPattern)DatabaseGeneratedOption.Value);
			if (DatabaseGeneratedOption.Value == System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)
			{
				property.Nullable = false;
			}
		}
		property.SetConfiguration(this);
	}

	internal void Configure(IEnumerable<Tuple<ColumnMappingBuilder, EntityType>> propertyMappings, DbProviderManifest providerManifest, bool allowOverride = false, bool fillFromExistingConfiguration = false)
	{
		propertyMappings.Each(delegate(Tuple<ColumnMappingBuilder, EntityType> pm)
		{
			Configure(pm.Item1.ColumnProperty, pm.Item2, providerManifest, allowOverride, fillFromExistingConfiguration);
		});
	}

	internal void ConfigureFunctionParameters(IEnumerable<FunctionParameter> parameters)
	{
		parameters.Each(ConfigureParameterName);
	}

	private void ConfigureParameterName(FunctionParameter parameter)
	{
		if (!string.IsNullOrWhiteSpace(ParameterName) && !string.Equals(ParameterName, parameter.Name, StringComparison.Ordinal))
		{
			parameter.Name = ParameterName;
			IEnumerable<FunctionParameter> ts = from p in parameter.DeclaringFunction.Parameters
				let configuration = p.GetConfiguration() as PrimitivePropertyConfiguration
				where p != parameter && string.Equals(ParameterName, p.Name, StringComparison.Ordinal) && (configuration == null || configuration.ParameterName == null)
				select p;
			List<FunctionParameter> renamedParameters = new List<FunctionParameter> { parameter };
			ts.Each(delegate(FunctionParameter c)
			{
				c.Name = renamedParameters.UniquifyName(ParameterName);
				renamedParameters.Add(c);
			});
			parameter.SetConfiguration(this);
		}
	}

	internal void Configure(EdmProperty column, EntityType table, DbProviderManifest providerManifest, bool allowOverride = false, bool fillFromExistingConfiguration = false)
	{
		PrimitivePropertyConfiguration primitivePropertyConfiguration = Clone();
		if (allowOverride)
		{
			primitivePropertyConfiguration.OverridableConfigurationParts |= OverridableConfigurationParts.OverridableInSSpace;
		}
		primitivePropertyConfiguration.MergeWithExistingConfiguration(column, (string errorMessage) => Error.ConflictingColumnConfiguration(column.Name, table.Name, errorMessage), inCSpace: false, fillFromExistingConfiguration).ConfigureColumn(column, table, providerManifest);
	}

	protected virtual void ConfigureColumn(EdmProperty column, EntityType table, DbProviderManifest providerManifest)
	{
		ConfigureColumnName(column, table);
		ConfigureAnnotations(column);
		if (!string.IsNullOrWhiteSpace(ColumnType))
		{
			column.PrimitiveType = providerManifest.GetStoreTypeFromName(ColumnType);
		}
		if (ColumnOrder.HasValue)
		{
			column.SetOrder(ColumnOrder.Value);
		}
		providerManifest.GetStoreTypes().SingleOrDefault((PrimitiveType t) => t.Name.Equals(column.TypeName, StringComparison.OrdinalIgnoreCase))?.FacetDescriptions.Each(delegate(FacetDescription f)
		{
			Configure(column, f);
		});
		column.SetConfiguration(this);
	}

	private void ConfigureColumnName(EdmProperty column, EntityType table)
	{
		if (!string.IsNullOrWhiteSpace(ColumnName) && !string.Equals(ColumnName, column.Name, StringComparison.Ordinal))
		{
			column.Name = ColumnName;
			IEnumerable<EdmProperty> ts = from c in table.Properties
				let configuration = c.GetConfiguration() as PrimitivePropertyConfiguration
				where c != column && string.Equals(ColumnName, c.GetPreferredName(), StringComparison.Ordinal) && (configuration == null || configuration.ColumnName == null)
				select c;
			List<EdmProperty> renamedColumns = new List<EdmProperty> { column };
			ts.Each(delegate(EdmProperty c)
			{
				c.Name = renamedColumns.UniquifyName(ColumnName);
				renamedColumns.Add(c);
			});
		}
	}

	private void ConfigureAnnotations(EdmProperty column)
	{
		foreach (KeyValuePair<string, object> annotation in _annotations)
		{
			column.AddAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:" + annotation.Key, annotation.Value);
		}
	}

	internal virtual void Configure(EdmProperty column, FacetDescription facetDescription)
	{
	}

	internal virtual void CopyFrom(PrimitivePropertyConfiguration other)
	{
		if (this == other)
		{
			return;
		}
		ColumnName = other.ColumnName;
		ParameterName = other.ParameterName;
		ColumnOrder = other.ColumnOrder;
		ColumnType = other.ColumnType;
		ConcurrencyMode = other.ConcurrencyMode;
		DatabaseGeneratedOption = other.DatabaseGeneratedOption;
		IsNullable = other.IsNullable;
		OverridableConfigurationParts = other.OverridableConfigurationParts;
		_annotations.Clear();
		foreach (KeyValuePair<string, object> annotation in other._annotations)
		{
			_annotations[annotation.Key] = annotation.Value;
		}
	}

	internal virtual void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		if (this == other)
		{
			return;
		}
		if (inCSpace)
		{
			if (!ConcurrencyMode.HasValue)
			{
				ConcurrencyMode = other.ConcurrencyMode;
			}
			if (!DatabaseGeneratedOption.HasValue)
			{
				DatabaseGeneratedOption = other.DatabaseGeneratedOption;
			}
			if (!IsNullable.HasValue)
			{
				IsNullable = other.IsNullable;
			}
			if (!other.OverridableConfigurationParts.HasFlag(OverridableConfigurationParts.OverridableInCSpace))
			{
				OverridableConfigurationParts &= ~OverridableConfigurationParts.OverridableInCSpace;
			}
			return;
		}
		if (ColumnName == null)
		{
			ColumnName = other.ColumnName;
		}
		if (ParameterName == null)
		{
			ParameterName = other.ParameterName;
		}
		if (!ColumnOrder.HasValue)
		{
			ColumnOrder = other.ColumnOrder;
		}
		if (ColumnType == null)
		{
			ColumnType = other.ColumnType;
		}
		foreach (KeyValuePair<string, object> annotation in other._annotations)
		{
			if (_annotations.ContainsKey(annotation.Key))
			{
				if (_annotations[annotation.Key] is IMergeableAnnotation mergeableAnnotation)
				{
					_annotations[annotation.Key] = mergeableAnnotation.MergeWith(annotation.Value);
				}
			}
			else
			{
				_annotations[annotation.Key] = annotation.Value;
			}
		}
		if (!other.OverridableConfigurationParts.HasFlag(OverridableConfigurationParts.OverridableInSSpace))
		{
			OverridableConfigurationParts &= ~OverridableConfigurationParts.OverridableInSSpace;
		}
	}

	internal virtual void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		if (this == other)
		{
			return;
		}
		if (inCSpace)
		{
			if (other.ConcurrencyMode.HasValue)
			{
				ConcurrencyMode = null;
			}
			if (other.DatabaseGeneratedOption.HasValue)
			{
				DatabaseGeneratedOption = null;
			}
			if (other.IsNullable.HasValue)
			{
				IsNullable = null;
			}
			return;
		}
		if (other.ColumnName != null)
		{
			ColumnName = null;
		}
		if (other.ParameterName != null)
		{
			ParameterName = null;
		}
		if (other.ColumnOrder.HasValue)
		{
			ColumnOrder = null;
		}
		if (other.ColumnType != null)
		{
			ColumnType = null;
		}
		foreach (string key in other._annotations.Keys)
		{
			if (_annotations.ContainsKey(key) && (!(_annotations[key] is IMergeableAnnotation mergeableAnnotation) || !mergeableAnnotation.IsCompatibleWith(other._annotations[key])))
			{
				_annotations.Remove(key);
			}
		}
	}

	internal virtual bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
	{
		errorMessage = string.Empty;
		if (other == null || this == other)
		{
			return true;
		}
		bool flag = !inCSpace || IsCompatible((PrimitivePropertyConfiguration c) => c.IsNullable, other, ref errorMessage);
		bool flag2 = !inCSpace || IsCompatible((PrimitivePropertyConfiguration c) => c.ConcurrencyMode, other, ref errorMessage);
		bool flag3 = !inCSpace || IsCompatible((PrimitivePropertyConfiguration c) => c.DatabaseGeneratedOption, other, ref errorMessage);
		bool flag4 = inCSpace || IsCompatible((PrimitivePropertyConfiguration c) => c.ColumnName, other, ref errorMessage);
		bool flag5 = inCSpace || IsCompatible((PrimitivePropertyConfiguration c) => c.ParameterName, other, ref errorMessage);
		bool flag6 = inCSpace || IsCompatible((PrimitivePropertyConfiguration c) => c.ColumnOrder, other, ref errorMessage);
		bool flag7 = inCSpace || IsCompatible((PrimitivePropertyConfiguration c) => c.ColumnType, other, ref errorMessage);
		bool flag8 = inCSpace || AnnotationsAreCompatible(other, ref errorMessage);
		return flag && flag2 && flag3 && flag4 && flag5 && flag6 && flag7 && flag8;
	}

	private bool AnnotationsAreCompatible(PrimitivePropertyConfiguration other, ref string errorMessage)
	{
		bool result = true;
		foreach (KeyValuePair<string, object> annotation in Annotations)
		{
			if (!other.Annotations.ContainsKey(annotation.Key))
			{
				continue;
			}
			object value = annotation.Value;
			object obj = other.Annotations[annotation.Key];
			if (value is IMergeableAnnotation mergeableAnnotation)
			{
				CompatibilityResult compatibilityResult = mergeableAnnotation.IsCompatibleWith(obj);
				if (!compatibilityResult)
				{
					result = false;
					errorMessage = errorMessage + Environment.NewLine + "\t" + compatibilityResult.ErrorMessage;
				}
			}
			else if (!object.Equals(value, obj))
			{
				result = false;
				errorMessage = errorMessage + Environment.NewLine + "\t" + Strings.ConflictingAnnotationValue(annotation.Key, value.ToString(), obj.ToString());
			}
		}
		return result;
	}

	protected bool IsCompatible<TProperty, TConfiguration>(Expression<Func<TConfiguration, TProperty?>> propertyExpression, TConfiguration other, ref string errorMessage) where TProperty : struct where TConfiguration : PrimitivePropertyConfiguration
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotNull(other, "other");
		PropertyInfo propertyInfo = propertyExpression.GetSimplePropertyAccess().Single();
		TProperty? val = (TProperty?)propertyInfo.GetValue(this, null);
		TProperty? val2 = (TProperty?)propertyInfo.GetValue(other, null);
		if (IsCompatible(val, val2))
		{
			return true;
		}
		errorMessage = errorMessage + Environment.NewLine + "\t" + Strings.ConflictingConfigurationValue(propertyInfo.Name, val, propertyInfo.Name, val2);
		return false;
	}

	protected bool IsCompatible<TConfiguration>(Expression<Func<TConfiguration, string>> propertyExpression, TConfiguration other, ref string errorMessage) where TConfiguration : PrimitivePropertyConfiguration
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotNull(other, "other");
		PropertyInfo propertyInfo = propertyExpression.GetSimplePropertyAccess().Single();
		string text = (string)propertyInfo.GetValue(this, null);
		string text2 = (string)propertyInfo.GetValue(other, null);
		if (IsCompatible(text, text2))
		{
			return true;
		}
		errorMessage = errorMessage + Environment.NewLine + "\t" + Strings.ConflictingConfigurationValue(propertyInfo.Name, text, propertyInfo.Name, text2);
		return false;
	}

	protected static bool IsCompatible<T>(T? thisConfiguration, T? other) where T : struct
	{
		if (thisConfiguration.HasValue)
		{
			if (other.HasValue)
			{
				return object.Equals(thisConfiguration.Value, other.Value);
			}
			return true;
		}
		return true;
	}

	protected static bool IsCompatible(string thisConfiguration, string other)
	{
		if (thisConfiguration != null)
		{
			if (other != null)
			{
				return object.Equals(thisConfiguration, other);
			}
			return true;
		}
		return true;
	}
}

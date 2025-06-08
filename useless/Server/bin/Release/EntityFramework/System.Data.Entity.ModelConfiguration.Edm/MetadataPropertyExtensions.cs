using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class MetadataPropertyExtensions
{
	private const string ClrPropertyInfoAnnotation = "ClrPropertyInfo";

	private const string ClrAttributesAnnotation = "ClrAttributes";

	private const string ConfiguationAnnotation = "Configuration";

	public static IList<Attribute> GetClrAttributes(this IEnumerable<MetadataProperty> metadataProperties)
	{
		return (IList<Attribute>)metadataProperties.GetAnnotation("ClrAttributes");
	}

	public static void SetClrAttributes(this ICollection<MetadataProperty> metadataProperties, IList<Attribute> attributes)
	{
		metadataProperties.SetAnnotation("ClrAttributes", attributes);
	}

	public static PropertyInfo GetClrPropertyInfo(this IEnumerable<MetadataProperty> metadataProperties)
	{
		return (PropertyInfo)metadataProperties.GetAnnotation("ClrPropertyInfo");
	}

	public static void SetClrPropertyInfo(this ICollection<MetadataProperty> metadataProperties, PropertyInfo propertyInfo)
	{
		metadataProperties.SetAnnotation("ClrPropertyInfo", propertyInfo);
	}

	public static Type GetClrType(this IEnumerable<MetadataProperty> metadataProperties)
	{
		return (Type)metadataProperties.GetAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:ClrType");
	}

	public static void SetClrType(this ICollection<MetadataProperty> metadataProperties, Type type)
	{
		metadataProperties.SetAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:ClrType", type);
	}

	public static object GetConfiguration(this IEnumerable<MetadataProperty> metadataProperties)
	{
		return metadataProperties.GetAnnotation("Configuration");
	}

	public static void SetConfiguration(this ICollection<MetadataProperty> metadataProperties, object configuration)
	{
		metadataProperties.SetAnnotation("Configuration", configuration);
	}

	public static object GetAnnotation(this IEnumerable<MetadataProperty> metadataProperties, string name)
	{
		foreach (MetadataProperty metadataProperty in metadataProperties)
		{
			if (metadataProperty.Name.Equals(name, StringComparison.Ordinal))
			{
				return metadataProperty.Value;
			}
		}
		return null;
	}

	public static void SetAnnotation(this ICollection<MetadataProperty> metadataProperties, string name, object value)
	{
		MetadataProperty metadataProperty = metadataProperties.SingleOrDefault((MetadataProperty p) => p.Name.Equals(name, StringComparison.Ordinal));
		if (metadataProperty == null)
		{
			metadataProperty = MetadataProperty.CreateAnnotation(name, value);
			metadataProperties.Add(metadataProperty);
		}
		else
		{
			metadataProperty.Value = value;
		}
	}

	public static void RemoveAnnotation(this ICollection<MetadataProperty> metadataProperties, string name)
	{
		MetadataProperty metadataProperty = metadataProperties.SingleOrDefault((MetadataProperty p) => p.Name.Equals(name, StringComparison.Ordinal));
		if (metadataProperty != null)
		{
			metadataProperties.Remove(metadataProperty);
		}
	}

	public static void Copy(this ICollection<MetadataProperty> sourceAnnotations, ICollection<MetadataProperty> targetAnnotations)
	{
		foreach (MetadataProperty sourceAnnotation in sourceAnnotations)
		{
			targetAnnotations.SetAnnotation(sourceAnnotation.Name, sourceAnnotation.Value);
		}
	}
}

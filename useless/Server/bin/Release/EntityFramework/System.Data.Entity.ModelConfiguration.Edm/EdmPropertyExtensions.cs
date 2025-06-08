using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class EdmPropertyExtensions
{
	private const string OrderAnnotation = "Order";

	private const string PreferredNameAnnotation = "PreferredName";

	private const string UnpreferredUniqueNameAnnotation = "UnpreferredUniqueName";

	public static void CopyFrom(this EdmProperty column, EdmProperty other)
	{
		column.IsFixedLength = other.IsFixedLength;
		column.IsMaxLength = other.IsMaxLength;
		column.IsUnicode = other.IsUnicode;
		column.MaxLength = other.MaxLength;
		column.Precision = other.Precision;
		column.Scale = other.Scale;
	}

	public static EdmProperty Clone(this EdmProperty tableColumn)
	{
		EdmProperty columnMetadata = new EdmProperty(tableColumn.Name, tableColumn.TypeUsage)
		{
			Nullable = tableColumn.Nullable,
			StoreGeneratedPattern = tableColumn.StoreGeneratedPattern,
			IsFixedLength = tableColumn.IsFixedLength,
			IsMaxLength = tableColumn.IsMaxLength,
			IsUnicode = tableColumn.IsUnicode,
			MaxLength = tableColumn.MaxLength,
			Precision = tableColumn.Precision,
			Scale = tableColumn.Scale
		};
		tableColumn.Annotations.Each(delegate(MetadataProperty a)
		{
			columnMetadata.GetMetadataProperties().Add(a);
		});
		return columnMetadata;
	}

	public static int? GetOrder(this EdmProperty tableColumn)
	{
		return (int?)tableColumn.Annotations.GetAnnotation("Order");
	}

	public static void SetOrder(this EdmProperty tableColumn, int order)
	{
		tableColumn.GetMetadataProperties().SetAnnotation("Order", order);
	}

	public static string GetPreferredName(this EdmProperty tableColumn)
	{
		return (string)tableColumn.Annotations.GetAnnotation("PreferredName");
	}

	public static void SetPreferredName(this EdmProperty tableColumn, string name)
	{
		tableColumn.GetMetadataProperties().SetAnnotation("PreferredName", name);
	}

	public static string GetUnpreferredUniqueName(this EdmProperty tableColumn)
	{
		return (string)tableColumn.Annotations.GetAnnotation("UnpreferredUniqueName");
	}

	public static void SetUnpreferredUniqueName(this EdmProperty tableColumn, string name)
	{
		tableColumn.GetMetadataProperties().SetAnnotation("UnpreferredUniqueName", name);
	}

	public static void RemoveStoreGeneratedIdentityPattern(this EdmProperty tableColumn)
	{
		if (tableColumn.StoreGeneratedPattern == StoreGeneratedPattern.Identity)
		{
			tableColumn.StoreGeneratedPattern = StoreGeneratedPattern.None;
		}
	}

	public static bool HasStoreGeneratedPattern(this EdmProperty property)
	{
		StoreGeneratedPattern? storeGeneratedPattern = property.GetStoreGeneratedPattern();
		if (storeGeneratedPattern.HasValue)
		{
			return storeGeneratedPattern != StoreGeneratedPattern.None;
		}
		return false;
	}

	public static StoreGeneratedPattern? GetStoreGeneratedPattern(this EdmProperty property)
	{
		if (property.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2009/02/edm/annotation:StoreGeneratedPattern", ignoreCase: false, out var item))
		{
			return (StoreGeneratedPattern?)Enum.Parse(typeof(StoreGeneratedPattern), (string)item.Value);
		}
		return null;
	}

	public static void SetStoreGeneratedPattern(this EdmProperty property, StoreGeneratedPattern storeGeneratedPattern)
	{
		if (!property.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2009/02/edm/annotation:StoreGeneratedPattern", ignoreCase: false, out var item))
		{
			property.MetadataProperties.Source.Add(new MetadataProperty("http://schemas.microsoft.com/ado/2009/02/edm/annotation:StoreGeneratedPattern", TypeUsage.Create(EdmProviderManifest.Instance.GetPrimitiveType(PrimitiveTypeKind.String)), storeGeneratedPattern.ToString()));
		}
		else
		{
			item.Value = storeGeneratedPattern.ToString();
		}
	}

	public static object GetConfiguration(this EdmProperty property)
	{
		return property.Annotations.GetConfiguration();
	}

	public static void SetConfiguration(this EdmProperty property, object configuration)
	{
		property.GetMetadataProperties().SetConfiguration(configuration);
	}

	public static List<EdmPropertyPath> ToPropertyPathList(this EdmProperty property)
	{
		return property.ToPropertyPathList(new List<EdmProperty>());
	}

	public static List<EdmPropertyPath> ToPropertyPathList(this EdmProperty property, List<EdmProperty> currentPath)
	{
		List<EdmPropertyPath> list = new List<EdmPropertyPath>();
		IncludePropertyPath(list, currentPath, property);
		return list;
	}

	private static void IncludePropertyPath(List<EdmPropertyPath> propertyPaths, List<EdmProperty> currentPath, EdmProperty property)
	{
		currentPath.Add(property);
		if (property.IsUnderlyingPrimitiveType)
		{
			propertyPaths.Add(new EdmPropertyPath(currentPath));
		}
		else if (property.IsComplexType)
		{
			foreach (EdmProperty property2 in property.ComplexType.Properties)
			{
				IncludePropertyPath(propertyPaths, currentPath, property2);
			}
		}
		currentPath.Remove(property);
	}
}

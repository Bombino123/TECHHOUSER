using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Data.Entity.Infrastructure.Annotations;

public class IndexAnnotationSerializer : IMetadataAnnotationSerializer
{
	internal const string FormatExample = "{ Name: MyIndex, Order: 7, IsClustered: True, IsUnique: False } { } { Name: MyOtherIndex }";

	private static readonly Regex _indexesSplitter = new Regex("(?<!\\\\)}\\s*{", RegexOptions.Compiled);

	private static readonly Regex _indexPartsSplitter = new Regex("(?<!\\\\),", RegexOptions.Compiled);

	public virtual string Serialize(string name, object value)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(value, "value");
		IndexAnnotation obj = (value as IndexAnnotation) ?? throw new ArgumentException(Strings.AnnotationSerializeWrongType(value.GetType().Name, typeof(IndexAnnotationSerializer).Name, typeof(IndexAnnotation).Name));
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IndexAttribute index in obj.Indexes)
		{
			stringBuilder.Append(SerializeIndexAttribute(index));
		}
		return stringBuilder.ToString();
	}

	internal static string SerializeIndexAttribute(IndexAttribute indexAttribute)
	{
		StringBuilder stringBuilder = new StringBuilder("{ ");
		if (!string.IsNullOrWhiteSpace(indexAttribute.Name))
		{
			stringBuilder.Append("Name: ").Append(indexAttribute.Name.Replace(",", "\\,").Replace("{", "\\{"));
		}
		if (indexAttribute.Order != -1)
		{
			if (stringBuilder.Length > 2)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append("Order: ").Append(indexAttribute.Order);
		}
		if (indexAttribute.IsClusteredConfigured)
		{
			if (stringBuilder.Length > 2)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append("IsClustered: ").Append(indexAttribute.IsClustered);
		}
		if (indexAttribute.IsUniqueConfigured)
		{
			if (stringBuilder.Length > 2)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append("IsUnique: ").Append(indexAttribute.IsUnique);
		}
		if (stringBuilder.Length > 2)
		{
			stringBuilder.Append(" ");
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	public virtual object Deserialize(string name, string value)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(value, "value");
		value = value.Trim();
		if (!value.StartsWith("{", StringComparison.Ordinal) || !value.EndsWith("}", StringComparison.Ordinal))
		{
			throw BuildFormatException(value);
		}
		List<IndexAttribute> list = new List<IndexAttribute>();
		List<string> list2 = (from s in _indexesSplitter.Split(value)
			select s.Trim()).ToList();
		list2[0] = list2[0].Substring(1);
		int index = list2.Count - 1;
		list2[index] = list2[index].Substring(0, list2[index].Length - 1);
		foreach (string item in list2)
		{
			IndexAttribute indexAttribute = new IndexAttribute();
			if (!string.IsNullOrWhiteSpace(item))
			{
				foreach (string item2 in from s in _indexPartsSplitter.Split(item)
					select s.Trim())
				{
					if (item2.StartsWith("Name:", StringComparison.Ordinal))
					{
						string text = item2.Substring(5).Trim();
						if (string.IsNullOrWhiteSpace(text) || !string.IsNullOrWhiteSpace(indexAttribute.Name))
						{
							throw BuildFormatException(value);
						}
						indexAttribute.Name = text.Replace("\\,", ",").Replace("\\{", "{");
						continue;
					}
					if (item2.StartsWith("Order:", StringComparison.Ordinal))
					{
						if (!int.TryParse(item2.Substring(6).Trim(), out var result) || indexAttribute.Order != -1)
						{
							throw BuildFormatException(value);
						}
						indexAttribute.Order = result;
						continue;
					}
					if (item2.StartsWith("IsClustered:", StringComparison.Ordinal))
					{
						if (!bool.TryParse(item2.Substring(12).Trim(), out var result2) || indexAttribute.IsClusteredConfigured)
						{
							throw BuildFormatException(value);
						}
						indexAttribute.IsClustered = result2;
						continue;
					}
					if (item2.StartsWith("IsUnique:", StringComparison.Ordinal))
					{
						if (!bool.TryParse(item2.Substring(9).Trim(), out var result3) || indexAttribute.IsUniqueConfigured)
						{
							throw BuildFormatException(value);
						}
						indexAttribute.IsUnique = result3;
						continue;
					}
					throw BuildFormatException(value);
				}
			}
			list.Add(indexAttribute);
		}
		return new IndexAnnotation(list);
	}

	private static FormatException BuildFormatException(string value)
	{
		return new FormatException(Strings.AnnotationSerializeBadFormat(value, typeof(IndexAnnotationSerializer).Name, "{ Name: MyIndex, Order: 7, IsClustered: True, IsUnique: False } { } { Name: MyOtherIndex }"));
	}
}

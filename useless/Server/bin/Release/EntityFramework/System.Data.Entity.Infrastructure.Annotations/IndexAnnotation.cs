using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Infrastructure.Annotations;

public class IndexAnnotation : IMergeableAnnotation
{
	public const string AnnotationName = "Index";

	private readonly IList<IndexAttribute> _indexes = new List<IndexAttribute>();

	public virtual IEnumerable<IndexAttribute> Indexes => _indexes;

	public IndexAnnotation(IndexAttribute indexAttribute)
	{
		Check.NotNull(indexAttribute, "indexAttribute");
		_indexes.Add(indexAttribute);
	}

	public IndexAnnotation(IEnumerable<IndexAttribute> indexAttributes)
	{
		Check.NotNull(indexAttributes, "indexAttributes");
		MergeLists(_indexes, indexAttributes, null);
	}

	internal IndexAnnotation(PropertyInfo propertyInfo, IEnumerable<IndexAttribute> indexAttributes)
	{
		Check.NotNull(indexAttributes, "indexAttributes");
		MergeLists(_indexes, indexAttributes, propertyInfo);
	}

	private static void MergeLists(ICollection<IndexAttribute> existingIndexes, IEnumerable<IndexAttribute> newIndexes, PropertyInfo propertyInfo)
	{
		foreach (IndexAttribute index in newIndexes)
		{
			if (index == null)
			{
				throw new ArgumentNullException("indexAttribute");
			}
			IndexAttribute indexAttribute = existingIndexes.SingleOrDefault((IndexAttribute i) => i.Name == index.Name);
			if (indexAttribute == null)
			{
				existingIndexes.Add(index);
				continue;
			}
			CompatibilityResult compatibilityResult = index.IsCompatibleWith(indexAttribute);
			if ((bool)compatibilityResult)
			{
				existingIndexes.Remove(indexAttribute);
				existingIndexes.Add(index.MergeWith(indexAttribute));
				continue;
			}
			string text = Environment.NewLine + "\t" + compatibilityResult.ErrorMessage;
			throw new InvalidOperationException((propertyInfo == null) ? Strings.ConflictingIndexAttribute(indexAttribute.Name, text) : Strings.ConflictingIndexAttributesOnProperty(propertyInfo.Name, propertyInfo.ReflectedType.Name, indexAttribute.Name, text));
		}
	}

	public virtual CompatibilityResult IsCompatibleWith(object other)
	{
		if (this == other || other == null)
		{
			return new CompatibilityResult(isCompatible: true, null);
		}
		if (!(other is IndexAnnotation indexAnnotation))
		{
			return new CompatibilityResult(isCompatible: false, Strings.IncompatibleTypes(other.GetType().Name, typeof(IndexAnnotation).Name));
		}
		foreach (IndexAttribute newIndex in indexAnnotation._indexes)
		{
			IndexAttribute indexAttribute = _indexes.SingleOrDefault((IndexAttribute i) => i.Name == newIndex.Name);
			if (indexAttribute != null)
			{
				CompatibilityResult compatibilityResult = indexAttribute.IsCompatibleWith(newIndex);
				if (!compatibilityResult)
				{
					return compatibilityResult;
				}
			}
		}
		return new CompatibilityResult(isCompatible: true, null);
	}

	public virtual object MergeWith(object other)
	{
		if (this == other || other == null)
		{
			return this;
		}
		if (!(other is IndexAnnotation indexAnnotation))
		{
			throw new ArgumentException(Strings.IncompatibleTypes(other.GetType().Name, typeof(IndexAnnotation).Name));
		}
		List<IndexAttribute> list = _indexes.ToList();
		MergeLists(list, indexAnnotation._indexes, null);
		return new IndexAnnotation(list);
	}

	public override string ToString()
	{
		return "IndexAnnotation: " + new IndexAnnotationSerializer().Serialize("Index", this);
	}
}

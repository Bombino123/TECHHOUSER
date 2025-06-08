using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Index;

internal class IndexConfiguration : PropertyConfiguration
{
	private bool? _isUnique;

	private bool? _isClustered;

	private string _name;

	public bool? IsUnique
	{
		get
		{
			return _isUnique;
		}
		set
		{
			Check.NotNull(value, "value");
			_isUnique = value;
		}
	}

	public bool? IsClustered
	{
		get
		{
			return _isClustered;
		}
		set
		{
			Check.NotNull(value, "value");
			_isClustered = value;
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			Check.NotNull(value, "value");
			_name = value;
		}
	}

	public IndexConfiguration()
	{
	}

	internal IndexConfiguration(IndexConfiguration source)
	{
		_isUnique = source._isUnique;
		_isClustered = source._isClustered;
		_name = source._name;
	}

	internal virtual IndexConfiguration Clone()
	{
		return new IndexConfiguration(this);
	}

	internal void Configure(EdmProperty edmProperty, int indexOrder)
	{
		AddAnnotationWithMerge(edmProperty, new IndexAnnotation(new IndexAttribute(_name, indexOrder, _isClustered, _isUnique)));
	}

	internal void Configure(EntityType entityType)
	{
		AddAnnotationWithMerge(entityType, new IndexAnnotation(new IndexAttribute(_name, _isClustered, _isUnique)));
	}

	private static void AddAnnotationWithMerge(MetadataItem metadataItem, IndexAnnotation newAnnotation)
	{
		object annotation = metadataItem.Annotations.GetAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:Index");
		if (annotation != null)
		{
			newAnnotation = (IndexAnnotation)((IndexAnnotation)annotation).MergeWith(newAnnotation);
		}
		metadataItem.AddAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:Index", newAnnotation);
	}
}

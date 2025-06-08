using System.Collections.Generic;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class FacetValues
{
	private FacetValueContainer<bool?> _nullable;

	private FacetValueContainer<int?> _maxLength;

	private FacetValueContainer<bool?> _unicode;

	private FacetValueContainer<bool?> _fixedLength;

	private FacetValueContainer<byte?> _precision;

	private FacetValueContainer<byte?> _scale;

	private object _defaultValue;

	private FacetValueContainer<string> _collation;

	private FacetValueContainer<int?> _srid;

	private FacetValueContainer<bool?> _isStrict;

	private FacetValueContainer<StoreGeneratedPattern?> _storeGeneratedPattern;

	private FacetValueContainer<ConcurrencyMode?> _concurrencyMode;

	private FacetValueContainer<CollectionKind?> _collectionKind;

	internal FacetValueContainer<bool?> Nullable
	{
		set
		{
			_nullable = value;
		}
	}

	internal FacetValueContainer<int?> MaxLength
	{
		set
		{
			_maxLength = value;
		}
	}

	internal FacetValueContainer<bool?> Unicode
	{
		set
		{
			_unicode = value;
		}
	}

	internal FacetValueContainer<bool?> FixedLength
	{
		set
		{
			_fixedLength = value;
		}
	}

	internal FacetValueContainer<byte?> Precision
	{
		set
		{
			_precision = value;
		}
	}

	internal FacetValueContainer<byte?> Scale
	{
		set
		{
			_scale = value;
		}
	}

	internal object DefaultValue
	{
		set
		{
			_defaultValue = value;
		}
	}

	internal FacetValueContainer<string> Collation
	{
		set
		{
			_collation = value;
		}
	}

	internal FacetValueContainer<int?> Srid
	{
		set
		{
			_srid = value;
		}
	}

	internal FacetValueContainer<bool?> IsStrict
	{
		set
		{
			_isStrict = value;
		}
	}

	internal FacetValueContainer<StoreGeneratedPattern?> StoreGeneratedPattern
	{
		set
		{
			_storeGeneratedPattern = value;
		}
	}

	internal FacetValueContainer<ConcurrencyMode?> ConcurrencyMode
	{
		set
		{
			_concurrencyMode = value;
		}
	}

	internal FacetValueContainer<CollectionKind?> CollectionKind
	{
		set
		{
			_collectionKind = value;
		}
	}

	internal static FacetValues NullFacetValues => new FacetValues
	{
		FixedLength = (bool?)null,
		MaxLength = (int?)null,
		Precision = (byte?)null,
		Scale = (byte?)null,
		Unicode = (bool?)null,
		Collation = (string)null,
		Srid = (int?)null,
		IsStrict = (bool?)null,
		ConcurrencyMode = (ConcurrencyMode?)null,
		StoreGeneratedPattern = (StoreGeneratedPattern?)null,
		CollectionKind = (CollectionKind?)null
	};

	internal bool TryGetFacet(FacetDescription description, out Facet facet)
	{
		switch (description.FacetName)
		{
		case "Nullable":
			if (_nullable.HasValue)
			{
				facet = Facet.Create(description, _nullable.GetValueAsObject());
				return true;
			}
			break;
		case "MaxLength":
			if (_maxLength.HasValue)
			{
				facet = Facet.Create(description, _maxLength.GetValueAsObject());
				return true;
			}
			break;
		case "Unicode":
			if (_unicode.HasValue)
			{
				facet = Facet.Create(description, _unicode.GetValueAsObject());
				return true;
			}
			break;
		case "FixedLength":
			if (_fixedLength.HasValue)
			{
				facet = Facet.Create(description, _fixedLength.GetValueAsObject());
				return true;
			}
			break;
		case "Precision":
			if (_precision.HasValue)
			{
				facet = Facet.Create(description, _precision.GetValueAsObject());
				return true;
			}
			break;
		case "Scale":
			if (_scale.HasValue)
			{
				facet = Facet.Create(description, _scale.GetValueAsObject());
				return true;
			}
			break;
		case "DefaultValue":
			if (_defaultValue != null)
			{
				facet = Facet.Create(description, _defaultValue);
				return true;
			}
			break;
		case "Collation":
			if (_collation.HasValue)
			{
				facet = Facet.Create(description, _collation.GetValueAsObject());
				return true;
			}
			break;
		case "SRID":
			if (_srid.HasValue)
			{
				facet = Facet.Create(description, _srid.GetValueAsObject());
				return true;
			}
			break;
		case "IsStrict":
			if (_isStrict.HasValue)
			{
				facet = Facet.Create(description, _isStrict.GetValueAsObject());
				return true;
			}
			break;
		case "StoreGeneratedPattern":
			if (_storeGeneratedPattern.HasValue)
			{
				facet = Facet.Create(description, _storeGeneratedPattern.GetValueAsObject());
				return true;
			}
			break;
		case "ConcurrencyMode":
			if (_concurrencyMode.HasValue)
			{
				facet = Facet.Create(description, _concurrencyMode.GetValueAsObject());
				return true;
			}
			break;
		case "CollectionKind":
			if (_collectionKind.HasValue)
			{
				facet = Facet.Create(description, _collectionKind.GetValueAsObject());
				return true;
			}
			break;
		}
		facet = null;
		return false;
	}

	public static FacetValues Create(IEnumerable<Facet> facets)
	{
		FacetValues facetValues = new FacetValues();
		foreach (Facet facet in facets)
		{
			switch (facet.Description.FacetName)
			{
			case "Nullable":
				facetValues.Nullable = (bool?)facet.Value;
				break;
			case "MaxLength":
				if (facet.Value is EdmConstants.Unbounded unbounded3)
				{
					facetValues.MaxLength = unbounded3;
				}
				else
				{
					facetValues.MaxLength = (int?)facet.Value;
				}
				break;
			case "Unicode":
				facetValues.Unicode = (bool?)facet.Value;
				break;
			case "FixedLength":
				facetValues.FixedLength = (bool?)facet.Value;
				break;
			case "Precision":
				if (facet.Value is EdmConstants.Unbounded unbounded2)
				{
					facetValues.Precision = unbounded2;
				}
				else
				{
					facetValues.Precision = (byte?)facet.Value;
				}
				break;
			case "Scale":
				if (facet.Value is EdmConstants.Unbounded unbounded)
				{
					facetValues.Scale = unbounded;
				}
				else
				{
					facetValues.Scale = (byte?)facet.Value;
				}
				break;
			case "DefaultValue":
				facetValues.DefaultValue = facet.Value;
				break;
			case "Collation":
				facetValues.Collation = (string)facet.Value;
				break;
			case "SRID":
				facetValues.Srid = (int?)facet.Value;
				break;
			case "IsStrict":
				facetValues.IsStrict = (bool?)facet.Value;
				break;
			case "StoreGeneratedPattern":
				facetValues.StoreGeneratedPattern = (StoreGeneratedPattern?)facet.Value;
				break;
			case "ConcurrencyMode":
				facetValues.ConcurrencyMode = (ConcurrencyMode?)facet.Value;
				break;
			case "CollectionKind":
				facetValues.CollectionKind = (CollectionKind?)facet.Value;
				break;
			}
		}
		return facetValues;
	}
}

using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public class FacetDescription
{
	private readonly string _facetName;

	private readonly EdmType _facetType;

	private readonly int? _minValue;

	private readonly int? _maxValue;

	private readonly object _defaultValue;

	private readonly bool _isConstant;

	private Facet _defaultValueFacet;

	private Facet _nullValueFacet;

	private Facet[] _valueCache;

	private static readonly object _notInitializedSentinel = new object();

	public virtual string FacetName => _facetName;

	public EdmType FacetType => _facetType;

	public int? MinValue => _minValue;

	public int? MaxValue => _maxValue;

	public object DefaultValue
	{
		get
		{
			if (_defaultValue == _notInitializedSentinel)
			{
				return null;
			}
			return _defaultValue;
		}
	}

	public virtual bool IsConstant => _isConstant;

	public bool IsRequired => _defaultValue == _notInitializedSentinel;

	internal Facet DefaultValueFacet
	{
		get
		{
			if (_defaultValueFacet == null)
			{
				Facet value = Facet.Create(this, DefaultValue, bypassKnownValues: true);
				Interlocked.CompareExchange(ref _defaultValueFacet, value, null);
			}
			return _defaultValueFacet;
		}
	}

	internal Facet NullValueFacet
	{
		get
		{
			if (_nullValueFacet == null)
			{
				Facet value = Facet.Create(this, null, bypassKnownValues: true);
				Interlocked.CompareExchange(ref _nullValueFacet, value, null);
			}
			return _nullValueFacet;
		}
	}

	internal FacetDescription()
	{
	}

	internal FacetDescription(string facetName, EdmType facetType, int? minValue, int? maxValue, object defaultValue, bool isConstant, string declaringTypeName)
	{
		_facetName = facetName;
		_facetType = facetType;
		_minValue = minValue;
		_maxValue = maxValue;
		if (defaultValue != null)
		{
			_defaultValue = defaultValue;
		}
		else
		{
			_defaultValue = _notInitializedSentinel;
		}
		_isConstant = isConstant;
		Validate(declaringTypeName);
		if (_isConstant)
		{
			UpdateMinMaxValueForConstant(_facetName, _facetType, ref _minValue, ref _maxValue, _defaultValue);
		}
	}

	internal FacetDescription(string facetName, EdmType facetType, int? minValue, int? maxValue, object defaultValue)
	{
		Check.NotEmpty(facetName, "facetName");
		Check.NotNull(facetType, "facetType");
		if ((minValue.HasValue || maxValue.HasValue) && minValue.HasValue)
		{
			_ = maxValue.HasValue;
		}
		_facetName = facetName;
		_facetType = facetType;
		_minValue = minValue;
		_maxValue = maxValue;
		_defaultValue = defaultValue;
	}

	public override string ToString()
	{
		return FacetName;
	}

	internal Facet GetBooleanFacet(bool value)
	{
		if (_valueCache == null)
		{
			Interlocked.CompareExchange(value: new Facet[2]
			{
				Facet.Create(this, true, bypassKnownValues: true),
				Facet.Create(this, false, bypassKnownValues: true)
			}, location1: ref _valueCache, comparand: null);
		}
		if (!value)
		{
			return _valueCache[1];
		}
		return _valueCache[0];
	}

	internal static bool IsNumericType(EdmType facetType)
	{
		if (Helper.IsPrimitiveType(facetType))
		{
			PrimitiveType primitiveType = (PrimitiveType)facetType;
			if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Byte && primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.SByte && primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Int16)
			{
				return primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Int32;
			}
			return true;
		}
		return false;
	}

	private static void UpdateMinMaxValueForConstant(string facetName, EdmType facetType, ref int? minValue, ref int? maxValue, object defaultValue)
	{
		if (IsNumericType(facetType))
		{
			if (facetName == "Precision" || facetName == "Scale")
			{
				minValue = (byte?)defaultValue;
				maxValue = (byte?)defaultValue;
			}
			else
			{
				minValue = (int?)defaultValue;
				maxValue = (int?)defaultValue;
			}
		}
	}

	private void Validate(string declaringTypeName)
	{
		if (_defaultValue == _notInitializedSentinel)
		{
			if (_isConstant)
			{
				throw new ArgumentException(Strings.MissingDefaultValueForConstantFacet(_facetName, declaringTypeName));
			}
		}
		else
		{
			if (!IsNumericType(_facetType))
			{
				return;
			}
			if (_isConstant)
			{
				if (_minValue.HasValue != _maxValue.HasValue || (_minValue.HasValue && _minValue.Value != _maxValue.Value))
				{
					throw new ArgumentException(Strings.MinAndMaxValueMustBeSameForConstantFacet(_facetName, declaringTypeName));
				}
				return;
			}
			if (!_minValue.HasValue || !_maxValue.HasValue)
			{
				throw new ArgumentException(Strings.BothMinAndMaxValueMustBeSpecifiedForNonConstantFacet(_facetName, declaringTypeName));
			}
			if (_minValue.Value == _maxValue)
			{
				throw new ArgumentException(Strings.MinAndMaxValueMustBeDifferentForNonConstantFacet(_facetName, declaringTypeName));
			}
			if (_minValue < 0 || _maxValue < 0)
			{
				throw new ArgumentException(Strings.MinAndMaxMustBePositive(_facetName, declaringTypeName));
			}
			if (_minValue > _maxValue)
			{
				throw new ArgumentException(Strings.MinMustBeLessThanMax(_minValue.ToString(), _facetName, declaringTypeName));
			}
		}
	}
}

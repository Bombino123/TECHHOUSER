#define TRACE
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class TypeUsageBuilder
{
	private readonly Dictionary<string, object> _facetValues;

	private readonly SchemaElement _element;

	private string _default;

	private object _defaultObject;

	private bool? _nullable;

	private TypeUsage _typeUsage;

	private bool _hasUserDefinedFacets;

	internal TypeUsage TypeUsage => _typeUsage;

	internal bool Nullable
	{
		get
		{
			if (_nullable.HasValue)
			{
				return _nullable.Value;
			}
			return true;
		}
	}

	internal string Default => _default;

	internal object DefaultAsObject => _defaultObject;

	internal bool HasUserDefinedFacets => _hasUserDefinedFacets;

	internal TypeUsageBuilder(SchemaElement element)
	{
		_element = element;
		_facetValues = new Dictionary<string, object>();
	}

	private bool TryGetFacets(EdmType edmType, bool complainOnMissingFacet, out Dictionary<string, Facet> calculatedFacets)
	{
		bool result = true;
		Dictionary<string, Facet> dictionary = edmType.GetAssociatedFacetDescriptions().ToDictionary((FacetDescription f) => f.FacetName, (FacetDescription f) => f.DefaultValueFacet);
		calculatedFacets = new Dictionary<string, Facet>();
		foreach (Facet value2 in dictionary.Values)
		{
			if (_facetValues.TryGetValue(value2.Name, out var value))
			{
				if (value2.Description.IsConstant)
				{
					_element.AddError(ErrorCode.ConstantFacetSpecifiedInSchema, EdmSchemaErrorSeverity.Error, _element, Strings.ConstantFacetSpecifiedInSchema(value2.Name, edmType.Name));
					result = false;
				}
				else
				{
					calculatedFacets.Add(value2.Name, Facet.Create(value2.Description, value));
				}
				_facetValues.Remove(value2.Name);
			}
			else if (complainOnMissingFacet && value2.Description.IsRequired)
			{
				_element.AddError(ErrorCode.RequiredFacetMissing, EdmSchemaErrorSeverity.Error, Strings.RequiredFacetMissing(value2.Name, edmType.Name));
				result = false;
			}
			else
			{
				calculatedFacets.Add(value2.Name, value2);
			}
		}
		foreach (KeyValuePair<string, object> facetValue in _facetValues)
		{
			if (facetValue.Key == "StoreGeneratedPattern")
			{
				Facet facet = Facet.Create(Converter.StoreGeneratedPatternFacet, facetValue.Value);
				calculatedFacets.Add(facet.Name, facet);
			}
			else if (facetValue.Key == "ConcurrencyMode")
			{
				Facet facet2 = Facet.Create(Converter.ConcurrencyModeFacet, facetValue.Value);
				calculatedFacets.Add(facet2.Name, facet2);
			}
			else if (edmType is PrimitiveType && ((PrimitiveType)edmType).PrimitiveTypeKind == PrimitiveTypeKind.String && facetValue.Key == "Collation")
			{
				Facet facet3 = Facet.Create(Converter.CollationFacet, facetValue.Value);
				calculatedFacets.Add(facet3.Name, facet3);
			}
			else
			{
				_element.AddError(ErrorCode.FacetNotAllowedByType, EdmSchemaErrorSeverity.Error, Strings.FacetNotAllowed(facetValue.Key, edmType.Name));
			}
		}
		return result;
	}

	internal void ValidateAndSetTypeUsage(EdmType edmType, bool complainOnMissingFacet)
	{
		TryGetFacets(edmType, complainOnMissingFacet, out var calculatedFacets);
		_typeUsage = TypeUsage.Create(edmType, calculatedFacets.Values);
	}

	internal void ValidateAndSetTypeUsage(ScalarType scalar, bool complainOnMissingFacet)
	{
		Trace.Assert(_element != null);
		Trace.Assert(scalar != null);
		if (Helper.IsSpatialType(scalar.Type) && !_facetValues.ContainsKey("IsStrict") && !_element.Schema.UseStrongSpatialTypes)
		{
			_facetValues.Add("IsStrict", false);
		}
		if (TryGetFacets(scalar.Type, complainOnMissingFacet, out var calculatedFacets))
		{
			switch (scalar.TypeKind)
			{
			case PrimitiveTypeKind.Binary:
				ValidateAndSetBinaryFacets(scalar.Type, calculatedFacets);
				break;
			case PrimitiveTypeKind.String:
				ValidateAndSetStringFacets(scalar.Type, calculatedFacets);
				break;
			case PrimitiveTypeKind.Decimal:
				ValidateAndSetDecimalFacets(scalar.Type, calculatedFacets);
				break;
			case PrimitiveTypeKind.DateTime:
			case PrimitiveTypeKind.Time:
			case PrimitiveTypeKind.DateTimeOffset:
				ValidatePrecisionFacetsForDateTimeFamily(scalar.Type, calculatedFacets);
				break;
			case PrimitiveTypeKind.Geometry:
			case PrimitiveTypeKind.Geography:
			case PrimitiveTypeKind.GeometryPoint:
			case PrimitiveTypeKind.GeometryLineString:
			case PrimitiveTypeKind.GeometryPolygon:
			case PrimitiveTypeKind.GeometryMultiPoint:
			case PrimitiveTypeKind.GeometryMultiLineString:
			case PrimitiveTypeKind.GeometryMultiPolygon:
			case PrimitiveTypeKind.GeometryCollection:
			case PrimitiveTypeKind.GeographyPoint:
			case PrimitiveTypeKind.GeographyLineString:
			case PrimitiveTypeKind.GeographyPolygon:
			case PrimitiveTypeKind.GeographyMultiPoint:
			case PrimitiveTypeKind.GeographyMultiLineString:
			case PrimitiveTypeKind.GeographyMultiPolygon:
			case PrimitiveTypeKind.GeographyCollection:
				ValidateSpatialFacets(scalar.Type, calculatedFacets);
				break;
			}
		}
		_typeUsage = TypeUsage.Create(scalar.Type, calculatedFacets.Values);
	}

	internal void ValidateEnumFacets(SchemaEnumType schemaEnumType)
	{
		foreach (KeyValuePair<string, object> facetValue in _facetValues)
		{
			if (facetValue.Key != "Nullable" && facetValue.Key != "StoreGeneratedPattern" && facetValue.Key != "ConcurrencyMode")
			{
				_element.AddError(ErrorCode.FacetNotAllowedByType, EdmSchemaErrorSeverity.Error, Strings.FacetNotAllowed(facetValue.Key, schemaEnumType.FQName));
			}
		}
	}

	internal bool HandleAttribute(XmlReader reader)
	{
		bool flag = InternalHandleAttribute(reader);
		_hasUserDefinedFacets |= flag;
		return flag;
	}

	private bool InternalHandleAttribute(XmlReader reader)
	{
		if (SchemaElement.CanHandleAttribute(reader, "Nullable"))
		{
			HandleNullableAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "DefaultValue"))
		{
			HandleDefaultAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Precision"))
		{
			HandlePrecisionAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Scale"))
		{
			HandleScaleAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "StoreGeneratedPattern"))
		{
			HandleStoreGeneratedPatternAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "ConcurrencyMode"))
		{
			HandleConcurrencyModeAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "MaxLength"))
		{
			HandleMaxLengthAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Unicode"))
		{
			HandleUnicodeAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Collation"))
		{
			HandleCollationAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "FixedLength"))
		{
			HandleIsFixedLengthAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Nullable"))
		{
			HandleNullableAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "SRID"))
		{
			HandleSridAttribute(reader);
			return true;
		}
		return false;
	}

	private void ValidateAndSetBinaryFacets(EdmType type, Dictionary<string, Facet> facets)
	{
		ValidateLengthFacets(type, facets);
	}

	private void ValidateAndSetDecimalFacets(EdmType type, Dictionary<string, Facet> facets)
	{
		PrimitiveType primitiveType = (PrimitiveType)type;
		byte? b = null;
		if (facets.TryGetValue("Precision", out var value) && value.Value != null)
		{
			b = (byte)value.Value;
			FacetDescription facet = Helper.GetFacet(primitiveType.FacetDescriptions, "Precision");
			if (b < facet.MinValue.Value || b > facet.MaxValue.Value)
			{
				_element.AddError(ErrorCode.PrecisionOutOfRange, EdmSchemaErrorSeverity.Error, Strings.PrecisionOutOfRange(b, facet.MinValue.Value, facet.MaxValue.Value, primitiveType.Name));
			}
		}
		if (facets.TryGetValue("Scale", out var value2) && value2.Value != null)
		{
			byte b2 = (byte)value2.Value;
			FacetDescription facet2 = Helper.GetFacet(primitiveType.FacetDescriptions, "Scale");
			if (b2 < facet2.MinValue.Value || b2 > facet2.MaxValue.Value)
			{
				_element.AddError(ErrorCode.ScaleOutOfRange, EdmSchemaErrorSeverity.Error, Strings.ScaleOutOfRange(b2, facet2.MinValue.Value, facet2.MaxValue.Value, primitiveType.Name));
			}
			else if (b.HasValue && b < b2)
			{
				_element.AddError(ErrorCode.BadPrecisionAndScale, EdmSchemaErrorSeverity.Error, Strings.BadPrecisionAndScale(b, b2));
			}
		}
	}

	private void ValidatePrecisionFacetsForDateTimeFamily(EdmType type, Dictionary<string, Facet> facets)
	{
		PrimitiveType primitiveType = (PrimitiveType)type;
		byte? b = null;
		if (facets.TryGetValue("Precision", out var value) && value.Value != null)
		{
			b = (byte)value.Value;
			FacetDescription facet = Helper.GetFacet(primitiveType.FacetDescriptions, "Precision");
			if (b < facet.MinValue.Value || b > facet.MaxValue.Value)
			{
				_element.AddError(ErrorCode.PrecisionOutOfRange, EdmSchemaErrorSeverity.Error, Strings.PrecisionOutOfRange(b, facet.MinValue.Value, facet.MaxValue.Value, primitiveType.Name));
			}
		}
	}

	private void ValidateAndSetStringFacets(EdmType type, Dictionary<string, Facet> facets)
	{
		ValidateLengthFacets(type, facets);
	}

	private void ValidateLengthFacets(EdmType type, Dictionary<string, Facet> facets)
	{
		PrimitiveType primitiveType = (PrimitiveType)type;
		if (facets.TryGetValue("MaxLength", out var value) && value.Value != null && !Helper.IsUnboundedFacetValue(value))
		{
			int num = (int)value.Value;
			FacetDescription facet = Helper.GetFacet(primitiveType.FacetDescriptions, "MaxLength");
			int value2 = facet.MaxValue.Value;
			int value3 = facet.MinValue.Value;
			if (num < value3 || num > value2)
			{
				_element.AddError(ErrorCode.InvalidSize, EdmSchemaErrorSeverity.Error, Strings.InvalidSize(num, value3, value2, primitiveType.Name));
			}
		}
	}

	private void ValidateSpatialFacets(EdmType type, Dictionary<string, Facet> facets)
	{
		PrimitiveType primitiveType = (PrimitiveType)type;
		if (_facetValues.ContainsKey("ConcurrencyMode"))
		{
			_element.AddError(ErrorCode.FacetNotAllowedByType, EdmSchemaErrorSeverity.Error, Strings.FacetNotAllowed("ConcurrencyMode", type.FullName));
		}
		if (_element.Schema.DataModel == SchemaDataModelOption.EntityDataModel && (!facets.TryGetValue("IsStrict", out var value) || (bool)value.Value))
		{
			_element.AddError(ErrorCode.UnexpectedSpatialType, EdmSchemaErrorSeverity.Error, Strings.SpatialWithUseStrongSpatialTypesFalse);
		}
		if (facets.TryGetValue("SRID", out var value2) && value2.Value != null && !Helper.IsVariableFacetValue(value2))
		{
			int num = (int)value2.Value;
			FacetDescription facet = Helper.GetFacet(primitiveType.FacetDescriptions, "SRID");
			int value3 = facet.MaxValue.Value;
			int value4 = facet.MinValue.Value;
			if (num < value4 || num > value3)
			{
				_element.AddError(ErrorCode.InvalidSystemReferenceId, EdmSchemaErrorSeverity.Error, Strings.InvalidSystemReferenceId(num, value4, value3, primitiveType.Name));
			}
		}
	}

	internal void HandleMaxLengthAttribute(XmlReader reader)
	{
		if (reader.Value.Trim() == "Max")
		{
			_facetValues.Add("MaxLength", EdmConstants.UnboundedValue);
			return;
		}
		int field = 0;
		if (_element.HandleIntAttribute(reader, ref field))
		{
			_facetValues.Add("MaxLength", field);
		}
	}

	internal void HandleSridAttribute(XmlReader reader)
	{
		if (reader.Value.Trim() == "Variable")
		{
			_facetValues.Add("SRID", EdmConstants.VariableValue);
			return;
		}
		int field = 0;
		if (_element.HandleIntAttribute(reader, ref field))
		{
			_facetValues.Add("SRID", field);
		}
	}

	private void HandleNullableAttribute(XmlReader reader)
	{
		bool field = false;
		if (_element.HandleBoolAttribute(reader, ref field))
		{
			_facetValues.Add("Nullable", field);
			_nullable = field;
		}
	}

	internal void HandleStoreGeneratedPatternAttribute(XmlReader reader)
	{
		StoreGeneratedPattern storeGeneratedPattern;
		switch (reader.Value)
		{
		default:
			return;
		case "None":
			storeGeneratedPattern = StoreGeneratedPattern.None;
			break;
		case "Identity":
			storeGeneratedPattern = StoreGeneratedPattern.Identity;
			break;
		case "Computed":
			storeGeneratedPattern = StoreGeneratedPattern.Computed;
			break;
		}
		_facetValues.Add("StoreGeneratedPattern", storeGeneratedPattern);
	}

	internal void HandleConcurrencyModeAttribute(XmlReader reader)
	{
		string value = reader.Value;
		ConcurrencyMode concurrencyMode;
		if (value == "None")
		{
			concurrencyMode = ConcurrencyMode.None;
		}
		else
		{
			if (!(value == "Fixed"))
			{
				return;
			}
			concurrencyMode = ConcurrencyMode.Fixed;
		}
		_facetValues.Add("ConcurrencyMode", concurrencyMode);
	}

	private void HandleDefaultAttribute(XmlReader reader)
	{
		_default = reader.Value;
	}

	private void HandlePrecisionAttribute(XmlReader reader)
	{
		byte field = 0;
		if (_element.HandleByteAttribute(reader, ref field))
		{
			_facetValues.Add("Precision", field);
		}
	}

	private void HandleScaleAttribute(XmlReader reader)
	{
		byte field = 0;
		if (_element.HandleByteAttribute(reader, ref field))
		{
			_facetValues.Add("Scale", field);
		}
	}

	private void HandleUnicodeAttribute(XmlReader reader)
	{
		bool field = false;
		if (_element.HandleBoolAttribute(reader, ref field))
		{
			_facetValues.Add("Unicode", field);
		}
	}

	private void HandleCollationAttribute(XmlReader reader)
	{
		if (!string.IsNullOrEmpty(reader.Value))
		{
			_facetValues.Add("Collation", reader.Value);
		}
	}

	private void HandleIsFixedLengthAttribute(XmlReader reader)
	{
		bool field = false;
		if (_element.HandleBoolAttribute(reader, ref field))
		{
			_facetValues.Add("FixedLength", field);
		}
	}

	internal void ValidateDefaultValue(SchemaType type)
	{
		if (_default != null)
		{
			if (type is ScalarType scalar)
			{
				ValidateScalarMemberDefaultValue(scalar);
			}
			else
			{
				_element.AddError(ErrorCode.DefaultNotAllowed, EdmSchemaErrorSeverity.Error, Strings.DefaultNotAllowed);
			}
		}
	}

	private void ValidateScalarMemberDefaultValue(ScalarType scalar)
	{
		if (scalar != null)
		{
			switch (scalar.TypeKind)
			{
			case PrimitiveTypeKind.Binary:
				ValidateBinaryDefaultValue(scalar);
				break;
			case PrimitiveTypeKind.Boolean:
				ValidateBooleanDefaultValue(scalar);
				break;
			case PrimitiveTypeKind.Byte:
				ValidateIntegralDefaultValue(scalar, 0L, 255L);
				break;
			case PrimitiveTypeKind.DateTime:
				ValidateDateTimeDefaultValue(scalar);
				break;
			case PrimitiveTypeKind.Time:
				ValidateTimeDefaultValue(scalar);
				break;
			case PrimitiveTypeKind.DateTimeOffset:
				ValidateDateTimeOffsetDefaultValue(scalar);
				break;
			case PrimitiveTypeKind.Decimal:
				ValidateDecimalDefaultValue(scalar);
				break;
			case PrimitiveTypeKind.Double:
				ValidateFloatingPointDefaultValue(scalar, double.MinValue, double.MaxValue);
				break;
			case PrimitiveTypeKind.Guid:
				ValidateGuidDefaultValue(scalar);
				break;
			case PrimitiveTypeKind.Int16:
				ValidateIntegralDefaultValue(scalar, -32768L, 32767L);
				break;
			case PrimitiveTypeKind.Int32:
				ValidateIntegralDefaultValue(scalar, -2147483648L, 2147483647L);
				break;
			case PrimitiveTypeKind.Int64:
				ValidateIntegralDefaultValue(scalar, long.MinValue, long.MaxValue);
				break;
			case PrimitiveTypeKind.Single:
				ValidateFloatingPointDefaultValue(scalar, -3.4028234663852886E+38, 3.4028234663852886E+38);
				break;
			case PrimitiveTypeKind.String:
				_defaultObject = _default;
				break;
			default:
				_element.AddError(ErrorCode.DefaultNotAllowed, EdmSchemaErrorSeverity.Error, Strings.DefaultNotAllowed);
				break;
			}
		}
	}

	private void ValidateBinaryDefaultValue(ScalarType scalar)
	{
		if (!scalar.TryParse(_default, out _defaultObject))
		{
			string message = Strings.InvalidDefaultBinaryWithNoMaxLength(_default);
			_element.AddError(ErrorCode.InvalidDefault, EdmSchemaErrorSeverity.Error, message);
		}
	}

	private void ValidateBooleanDefaultValue(ScalarType scalar)
	{
		if (!scalar.TryParse(_default, out _defaultObject))
		{
			_element.AddError(ErrorCode.InvalidDefault, EdmSchemaErrorSeverity.Error, Strings.InvalidDefaultBoolean(_default));
		}
	}

	private void ValidateIntegralDefaultValue(ScalarType scalar, long minValue, long maxValue)
	{
		if (!scalar.TryParse(_default, out _defaultObject))
		{
			_element.AddError(ErrorCode.InvalidDefault, EdmSchemaErrorSeverity.Error, Strings.InvalidDefaultIntegral(_default, minValue, maxValue));
		}
	}

	private void ValidateDateTimeDefaultValue(ScalarType scalar)
	{
		if (!scalar.TryParse(_default, out _defaultObject))
		{
			_element.AddError(ErrorCode.InvalidDefault, EdmSchemaErrorSeverity.Error, Strings.InvalidDefaultDateTime(_default, "yyyy-MM-dd HH\\:mm\\:ss.fffZ".Replace("\\", "")));
		}
	}

	private void ValidateTimeDefaultValue(ScalarType scalar)
	{
		if (!scalar.TryParse(_default, out _defaultObject))
		{
			_element.AddError(ErrorCode.InvalidDefault, EdmSchemaErrorSeverity.Error, Strings.InvalidDefaultTime(_default, "HH\\:mm\\:ss.fffffffZ".Replace("\\", "")));
		}
	}

	private void ValidateDateTimeOffsetDefaultValue(ScalarType scalar)
	{
		if (!scalar.TryParse(_default, out _defaultObject))
		{
			_element.AddError(ErrorCode.InvalidDefault, EdmSchemaErrorSeverity.Error, Strings.InvalidDefaultDateTimeOffset(_default, "yyyy-MM-dd HH\\:mm\\:ss.fffffffz".Replace("\\", "")));
		}
	}

	private void ValidateDecimalDefaultValue(ScalarType scalar)
	{
		if (!scalar.TryParse(_default, out _defaultObject))
		{
			_element.AddError(ErrorCode.InvalidDefault, EdmSchemaErrorSeverity.Error, Strings.InvalidDefaultDecimal(_default, 38, 38));
		}
	}

	private void ValidateFloatingPointDefaultValue(ScalarType scalar, double minValue, double maxValue)
	{
		if (!scalar.TryParse(_default, out _defaultObject))
		{
			_element.AddError(ErrorCode.InvalidDefault, EdmSchemaErrorSeverity.Error, Strings.InvalidDefaultFloatingPoint(_default, minValue, maxValue));
		}
	}

	private void ValidateGuidDefaultValue(ScalarType scalar)
	{
		if (!scalar.TryParse(_default, out _defaultObject))
		{
			_element.AddError(ErrorCode.InvalidDefault, EdmSchemaErrorSeverity.Error, Strings.InvalidDefaultGuid(_default));
		}
	}
}

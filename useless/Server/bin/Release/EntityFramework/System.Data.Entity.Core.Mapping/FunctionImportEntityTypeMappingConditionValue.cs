using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Xml.XPath;

namespace System.Data.Entity.Core.Mapping;

public sealed class FunctionImportEntityTypeMappingConditionValue : FunctionImportEntityTypeMappingCondition
{
	private readonly object _value;

	private readonly XPathNavigator _xPathValue;

	private readonly Memoizer<Type, object> _convertedValues;

	public object Value => _value;

	internal override ValueCondition ConditionValue => new ValueCondition((_value != null) ? _value.ToString() : _xPathValue.Value);

	public FunctionImportEntityTypeMappingConditionValue(string columnName, object value)
		: base(Check.NotNull(columnName, "columnName"), LineInfo.Empty)
	{
		Check.NotNull(value, "value");
		_value = value;
		_convertedValues = new Memoizer<Type, object>(GetConditionValue, null);
	}

	internal FunctionImportEntityTypeMappingConditionValue(string columnName, XPathNavigator columnValue, LineInfo lineInfo)
		: base(columnName, lineInfo)
	{
		_xPathValue = columnValue;
		_convertedValues = new Memoizer<Type, object>(GetConditionValue, null);
	}

	internal override bool ColumnValueMatchesCondition(object columnValue)
	{
		if (columnValue == null || Convert.IsDBNull(columnValue))
		{
			return false;
		}
		Type type = columnValue.GetType();
		object y = _convertedValues.Evaluate(type);
		return ByValueEqualityComparer.Default.Equals(columnValue, y);
	}

	private object GetConditionValue(Type columnValueType)
	{
		return GetConditionValue(columnValueType, delegate
		{
			throw new EntityCommandExecutionException(Strings.Mapping_FunctionImport_UnsupportedType(base.ColumnName, columnValueType.FullName));
		}, delegate
		{
			throw new EntityCommandExecutionException(Strings.Mapping_FunctionImport_ConditionValueTypeMismatch("FunctionImportMapping", base.ColumnName, columnValueType.FullName));
		});
	}

	internal object GetConditionValue(Type columnValueType, Action handleTypeNotComparable, Action handleInvalidConditionValue)
	{
		if (!ClrProviderManifest.Instance.TryGetPrimitiveType(columnValueType, out var primitiveType) || !MappingItemLoader.IsTypeSupportedForCondition(primitiveType.PrimitiveTypeKind))
		{
			handleTypeNotComparable();
			return null;
		}
		if (_value != null)
		{
			if (_value.GetType() == columnValueType)
			{
				return _value;
			}
			handleInvalidConditionValue();
			return null;
		}
		try
		{
			return _xPathValue.ValueAs(columnValueType);
		}
		catch (FormatException)
		{
			handleInvalidConditionValue();
			return null;
		}
	}
}

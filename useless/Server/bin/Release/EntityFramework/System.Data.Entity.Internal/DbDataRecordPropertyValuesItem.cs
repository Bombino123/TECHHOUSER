using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Internal;

internal class DbDataRecordPropertyValuesItem : IPropertyValuesItem
{
	private readonly DbUpdatableDataRecord _dataRecord;

	private readonly int _ordinal;

	private object _value;

	public object Value
	{
		get
		{
			return _value;
		}
		set
		{
			_dataRecord.SetValue(_ordinal, value);
			_value = value;
		}
	}

	public string Name => _dataRecord.GetName(_ordinal);

	public bool IsComplex => _dataRecord.DataRecordInfo.FieldMetadata[_ordinal].FieldType.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType;

	public Type Type => _dataRecord.GetFieldType(_ordinal);

	public DbDataRecordPropertyValuesItem(DbUpdatableDataRecord dataRecord, int ordinal, object value)
	{
		_dataRecord = dataRecord;
		_ordinal = ordinal;
		_value = value;
	}
}

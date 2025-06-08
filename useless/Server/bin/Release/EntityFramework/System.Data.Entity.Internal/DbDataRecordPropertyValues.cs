using System.Collections.Generic;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Internal;

internal class DbDataRecordPropertyValues : InternalPropertyValues
{
	private readonly DbUpdatableDataRecord _dataRecord;

	private ISet<string> _names;

	public override ISet<string> PropertyNames
	{
		get
		{
			if (_names == null)
			{
				HashSet<string> hashSet = new HashSet<string>();
				for (int i = 0; i < _dataRecord.FieldCount; i++)
				{
					hashSet.Add(_dataRecord.GetName(i));
				}
				_names = new ReadOnlySet<string>(hashSet);
			}
			return _names;
		}
	}

	internal DbDataRecordPropertyValues(InternalContext internalContext, Type type, DbUpdatableDataRecord dataRecord, bool isEntity)
		: base(internalContext, type, isEntity)
	{
		_dataRecord = dataRecord;
	}

	protected override IPropertyValuesItem GetItemImpl(string propertyName)
	{
		int ordinal = _dataRecord.GetOrdinal(propertyName);
		object obj = _dataRecord[ordinal];
		if (obj is DbUpdatableDataRecord dataRecord)
		{
			obj = new DbDataRecordPropertyValues(base.InternalContext, _dataRecord.GetFieldType(ordinal), dataRecord, isEntity: false);
		}
		else if (obj == DBNull.Value)
		{
			obj = null;
		}
		return new DbDataRecordPropertyValuesItem(_dataRecord, ordinal, obj);
	}
}

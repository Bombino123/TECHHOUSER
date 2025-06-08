using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal class RecordState
{
	private readonly RecordStateFactory RecordStateFactory;

	internal readonly CoordinatorFactory CoordinatorFactory;

	private bool _pendingIsNull;

	private bool _currentIsNull;

	private EntityRecordInfo _currentEntityRecordInfo;

	private EntityRecordInfo _pendingEntityRecordInfo;

	internal object[] CurrentColumnValues;

	internal object[] PendingColumnValues;

	internal int ColumnCount => RecordStateFactory.ColumnCount;

	internal DataRecordInfo DataRecordInfo
	{
		get
		{
			DataRecordInfo dataRecordInfo = _currentEntityRecordInfo;
			if (dataRecordInfo == null)
			{
				dataRecordInfo = RecordStateFactory.DataRecordInfo;
			}
			return dataRecordInfo;
		}
	}

	internal bool IsNull => _currentIsNull;

	internal RecordState(RecordStateFactory recordStateFactory, CoordinatorFactory coordinatorFactory)
	{
		RecordStateFactory = recordStateFactory;
		CoordinatorFactory = coordinatorFactory;
		CurrentColumnValues = new object[RecordStateFactory.ColumnCount];
		PendingColumnValues = new object[RecordStateFactory.ColumnCount];
	}

	internal void AcceptPendingValues()
	{
		object[] currentColumnValues = CurrentColumnValues;
		CurrentColumnValues = PendingColumnValues;
		PendingColumnValues = currentColumnValues;
		_currentEntityRecordInfo = _pendingEntityRecordInfo;
		_pendingEntityRecordInfo = null;
		_currentIsNull = _pendingIsNull;
		if (!RecordStateFactory.HasNestedColumns)
		{
			return;
		}
		for (int i = 0; i < CurrentColumnValues.Length; i++)
		{
			if (RecordStateFactory.IsColumnNested[i] && CurrentColumnValues[i] is RecordState recordState)
			{
				recordState.AcceptPendingValues();
			}
		}
	}

	internal long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
	{
		byte[] array = (byte[])CurrentColumnValues[ordinal];
		int num = array.Length;
		int num2 = (int)dataOffset;
		int num3 = num - num2;
		if (buffer != null)
		{
			num3 = Math.Min(num3, length);
			if (0 < num3)
			{
				Buffer.BlockCopy(array, num2, buffer, bufferOffset, num3);
			}
		}
		return Math.Max(0, num3);
	}

	internal long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
	{
		char[] array = ((!(CurrentColumnValues[ordinal] is string text)) ? ((char[])CurrentColumnValues[ordinal]) : text.ToCharArray());
		int num = array.Length;
		int num2 = (int)dataOffset;
		int num3 = num - num2;
		if (buffer != null)
		{
			num3 = Math.Min(num3, length);
			if (0 < num3)
			{
				Buffer.BlockCopy(array, num2 * 2, buffer, bufferOffset * 2, num3 * 2);
			}
		}
		return Math.Max(0, num3);
	}

	internal string GetName(int ordinal)
	{
		if (ordinal < 0 || ordinal >= RecordStateFactory.ColumnCount)
		{
			throw new ArgumentOutOfRangeException("ordinal");
		}
		return RecordStateFactory.ColumnNames[ordinal];
	}

	internal int GetOrdinal(string name)
	{
		return RecordStateFactory.FieldNameLookup.GetOrdinal(name);
	}

	internal TypeUsage GetTypeUsage(int ordinal)
	{
		return RecordStateFactory.TypeUsages[ordinal];
	}

	internal bool IsNestedObject(int ordinal)
	{
		return RecordStateFactory.IsColumnNested[ordinal];
	}

	internal void ResetToDefaultState()
	{
		_currentEntityRecordInfo = null;
	}

	internal RecordState GatherData(Shaper shaper)
	{
		RecordStateFactory.GatherData(shaper);
		_pendingIsNull = false;
		return this;
	}

	internal bool SetColumnValue(int ordinal, object value)
	{
		PendingColumnValues[ordinal] = value;
		return true;
	}

	internal bool SetEntityRecordInfo(EntityKey entityKey, EntitySet entitySet)
	{
		_pendingEntityRecordInfo = new EntityRecordInfo(RecordStateFactory.DataRecordInfo, entityKey, entitySet);
		return true;
	}

	internal RecordState SetNullRecord()
	{
		for (int i = 0; i < PendingColumnValues.Length; i++)
		{
			PendingColumnValues[i] = DBNull.Value;
		}
		_pendingEntityRecordInfo = null;
		_pendingIsNull = true;
		return this;
	}
}

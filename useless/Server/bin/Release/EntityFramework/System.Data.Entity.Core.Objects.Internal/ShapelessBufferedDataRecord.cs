using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Objects.Internal;

internal class ShapelessBufferedDataRecord : BufferedDataRecord
{
	private object[] _currentRow;

	private List<object[]> _resultSet;

	private DbSpatialDataReader _spatialDataReader;

	private bool[] _geographyColumns;

	private bool[] _geometryColumns;

	protected ShapelessBufferedDataRecord()
	{
	}

	internal static ShapelessBufferedDataRecord Initialize(string providerManifestToken, DbProviderServices providerServices, DbDataReader reader)
	{
		ShapelessBufferedDataRecord shapelessBufferedDataRecord = new ShapelessBufferedDataRecord();
		shapelessBufferedDataRecord.ReadMetadata(providerManifestToken, providerServices, reader);
		int fieldCount = shapelessBufferedDataRecord.FieldCount;
		List<object[]> list = new List<object[]>();
		if (shapelessBufferedDataRecord._spatialDataReader != null)
		{
			while (reader.Read())
			{
				object[] array = new object[fieldCount];
				for (int i = 0; i < fieldCount; i++)
				{
					if (reader.IsDBNull(i))
					{
						array[i] = DBNull.Value;
					}
					else if (shapelessBufferedDataRecord._geographyColumns[i])
					{
						array[i] = shapelessBufferedDataRecord._spatialDataReader.GetGeography(i);
					}
					else if (shapelessBufferedDataRecord._geometryColumns[i])
					{
						array[i] = shapelessBufferedDataRecord._spatialDataReader.GetGeometry(i);
					}
					else
					{
						array[i] = reader.GetValue(i);
					}
				}
				list.Add(array);
			}
		}
		else
		{
			while (reader.Read())
			{
				object[] array2 = new object[fieldCount];
				reader.GetValues(array2);
				list.Add(array2);
			}
		}
		shapelessBufferedDataRecord._rowCount = list.Count;
		shapelessBufferedDataRecord._resultSet = list;
		return shapelessBufferedDataRecord;
	}

	internal static async Task<ShapelessBufferedDataRecord> InitializeAsync(string providerManifestToken, DbProviderServices providerServices, DbDataReader reader, CancellationToken cancellationToken)
	{
		ShapelessBufferedDataRecord record = new ShapelessBufferedDataRecord();
		record.ReadMetadata(providerManifestToken, providerServices, reader);
		int fieldCount = record.FieldCount;
		List<object[]> resultSet = new List<object[]>();
		while (await reader.ReadAsync(cancellationToken).WithCurrentCulture())
		{
			object[] row = new object[fieldCount];
			for (int i = 0; i < fieldCount; i++)
			{
				if (await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture())
				{
					row[i] = DBNull.Value;
				}
				else if (record._spatialDataReader != null && record._geographyColumns[i])
				{
					row[i] = await record._spatialDataReader.GetGeographyAsync(i, cancellationToken).WithCurrentCulture();
				}
				else if (record._spatialDataReader != null && record._geometryColumns[i])
				{
					row[i] = await record._spatialDataReader.GetGeometryAsync(i, cancellationToken).WithCurrentCulture();
				}
				else
				{
					row[i] = await reader.GetFieldValueAsync<object>(i, cancellationToken).WithCurrentCulture();
				}
			}
			resultSet.Add(row);
		}
		record._rowCount = resultSet.Count;
		record._resultSet = resultSet;
		return record;
	}

	protected override void ReadMetadata(string providerManifestToken, DbProviderServices providerServices, DbDataReader reader)
	{
		base.ReadMetadata(providerManifestToken, providerServices, reader);
		int fieldCount = base.FieldCount;
		bool flag = false;
		DbSpatialDataReader dbSpatialDataReader = null;
		if (fieldCount > 0)
		{
			dbSpatialDataReader = providerServices.GetSpatialDataReader(reader, providerManifestToken);
		}
		if (dbSpatialDataReader != null)
		{
			_geographyColumns = new bool[fieldCount];
			_geometryColumns = new bool[fieldCount];
			for (int i = 0; i < fieldCount; i++)
			{
				_geographyColumns[i] = dbSpatialDataReader.IsGeographyColumn(i);
				_geometryColumns[i] = dbSpatialDataReader.IsGeometryColumn(i);
				flag = flag || _geographyColumns[i] || _geometryColumns[i];
			}
		}
		_spatialDataReader = (flag ? dbSpatialDataReader : null);
	}

	public override bool GetBoolean(int ordinal)
	{
		return GetFieldValue<bool>(ordinal);
	}

	public override byte GetByte(int ordinal)
	{
		return GetFieldValue<byte>(ordinal);
	}

	public override char GetChar(int ordinal)
	{
		return GetFieldValue<char>(ordinal);
	}

	public override DateTime GetDateTime(int ordinal)
	{
		return GetFieldValue<DateTime>(ordinal);
	}

	public override decimal GetDecimal(int ordinal)
	{
		return GetFieldValue<decimal>(ordinal);
	}

	public override double GetDouble(int ordinal)
	{
		return GetFieldValue<double>(ordinal);
	}

	public override float GetFloat(int ordinal)
	{
		return GetFieldValue<float>(ordinal);
	}

	public override Guid GetGuid(int ordinal)
	{
		return GetFieldValue<Guid>(ordinal);
	}

	public override short GetInt16(int ordinal)
	{
		return GetFieldValue<short>(ordinal);
	}

	public override int GetInt32(int ordinal)
	{
		return GetFieldValue<int>(ordinal);
	}

	public override long GetInt64(int ordinal)
	{
		return GetFieldValue<long>(ordinal);
	}

	public override string GetString(int ordinal)
	{
		return GetFieldValue<string>(ordinal);
	}

	public override T GetFieldValue<T>(int ordinal)
	{
		return (T)_currentRow[ordinal];
	}

	public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
	{
		return Task.FromResult((T)_currentRow[ordinal]);
	}

	public override object GetValue(int ordinal)
	{
		return GetFieldValue<object>(ordinal);
	}

	public override int GetValues(object[] values)
	{
		int num = Math.Min(values.Length, base.FieldCount);
		for (int i = 0; i < num; i++)
		{
			values[i] = GetValue(i);
		}
		return num;
	}

	public override bool IsDBNull(int ordinal)
	{
		if (_currentRow.Length == 0)
		{
			return true;
		}
		return DBNull.Value == _currentRow[ordinal];
	}

	public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
	{
		return Task.FromResult(IsDBNull(ordinal));
	}

	public override bool Read()
	{
		if (++_currentRowNumber < _rowCount)
		{
			_currentRow = _resultSet[_currentRowNumber];
			base.IsDataReady = true;
		}
		else
		{
			_currentRow = null;
			base.IsDataReady = false;
		}
		return base.IsDataReady;
	}

	public override Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(Read());
	}
}

using System.Collections;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Objects.Internal;

internal class ShapedBufferedDataRecord : BufferedDataRecord
{
	private enum TypeCase
	{
		Empty,
		Object,
		Bool,
		Byte,
		Char,
		DateTime,
		Decimal,
		Double,
		Float,
		Guid,
		Short,
		Int,
		Long,
		DbGeography,
		DbGeometry
	}

	private int _rowCapacity = 1;

	private BitArray _bools;

	private bool[] _tempBools;

	private int _boolCount;

	private byte[] _bytes;

	private int _byteCount;

	private char[] _chars;

	private int _charCount;

	private DateTime[] _dateTimes;

	private int _dateTimeCount;

	private decimal[] _decimals;

	private int _decimalCount;

	private double[] _doubles;

	private int _doubleCount;

	private float[] _floats;

	private int _floatCount;

	private Guid[] _guids;

	private int _guidCount;

	private short[] _shorts;

	private int _shortCount;

	private int[] _ints;

	private int _intCount;

	private long[] _longs;

	private int _longCount;

	private object[] _objects;

	private int _objectCount;

	private int[] _ordinalToIndexMap;

	private BitArray _nulls;

	private bool[] _tempNulls;

	private int _nullCount;

	private int[] _nullOrdinalToIndexMap;

	private TypeCase[] _columnTypeCases;

	protected ShapedBufferedDataRecord()
	{
	}

	internal static BufferedDataRecord Initialize(string providerManifestToken, DbProviderServices providerServices, DbDataReader reader, Type[] columnTypes, bool[] nullableColumns)
	{
		ShapedBufferedDataRecord shapedBufferedDataRecord = new ShapedBufferedDataRecord();
		shapedBufferedDataRecord.ReadMetadata(providerManifestToken, providerServices, reader);
		DbSpatialDataReader spatialDataReader = null;
		if (columnTypes.Any((Type t) => t == typeof(DbGeography) || t == typeof(DbGeometry)))
		{
			spatialDataReader = providerServices.GetSpatialDataReader(reader, providerManifestToken);
		}
		return shapedBufferedDataRecord.Initialize(reader, spatialDataReader, columnTypes, nullableColumns);
	}

	internal static Task<BufferedDataRecord> InitializeAsync(string providerManifestToken, DbProviderServices providerServices, DbDataReader reader, Type[] columnTypes, bool[] nullableColumns, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ShapedBufferedDataRecord shapedBufferedDataRecord = new ShapedBufferedDataRecord();
		shapedBufferedDataRecord.ReadMetadata(providerManifestToken, providerServices, reader);
		DbSpatialDataReader spatialDataReader = null;
		if (columnTypes.Any((Type t) => t == typeof(DbGeography) || t == typeof(DbGeometry)))
		{
			spatialDataReader = providerServices.GetSpatialDataReader(reader, providerManifestToken);
		}
		return shapedBufferedDataRecord.InitializeAsync(reader, spatialDataReader, columnTypes, nullableColumns, cancellationToken);
	}

	private BufferedDataRecord Initialize(DbDataReader reader, DbSpatialDataReader spatialDataReader, Type[] columnTypes, bool[] nullableColumns)
	{
		InitializeFields(columnTypes, nullableColumns);
		while (reader.Read())
		{
			_currentRowNumber++;
			if (_rowCapacity == _currentRowNumber)
			{
				DoubleBufferCapacity();
			}
			int num = Math.Max(columnTypes.Length, nullableColumns.Length);
			for (int i = 0; i < num; i++)
			{
				if (i < _columnTypeCases.Length)
				{
					switch (_columnTypeCases[i])
					{
					case TypeCase.Bool:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadBool(reader, i);
							}
						}
						else
						{
							ReadBool(reader, i);
						}
						continue;
					case TypeCase.Byte:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadByte(reader, i);
							}
						}
						else
						{
							ReadByte(reader, i);
						}
						continue;
					case TypeCase.Char:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadChar(reader, i);
							}
						}
						else
						{
							ReadChar(reader, i);
						}
						continue;
					case TypeCase.DateTime:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadDateTime(reader, i);
							}
						}
						else
						{
							ReadDateTime(reader, i);
						}
						continue;
					case TypeCase.Decimal:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadDecimal(reader, i);
							}
						}
						else
						{
							ReadDecimal(reader, i);
						}
						continue;
					case TypeCase.Double:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadDouble(reader, i);
							}
						}
						else
						{
							ReadDouble(reader, i);
						}
						continue;
					case TypeCase.Float:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadFloat(reader, i);
							}
						}
						else
						{
							ReadFloat(reader, i);
						}
						continue;
					case TypeCase.Guid:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadGuid(reader, i);
							}
						}
						else
						{
							ReadGuid(reader, i);
						}
						continue;
					case TypeCase.Short:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadShort(reader, i);
							}
						}
						else
						{
							ReadShort(reader, i);
						}
						continue;
					case TypeCase.Int:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadInt(reader, i);
							}
						}
						else
						{
							ReadInt(reader, i);
						}
						continue;
					case TypeCase.Long:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadLong(reader, i);
							}
						}
						else
						{
							ReadLong(reader, i);
						}
						continue;
					case TypeCase.DbGeography:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadGeography(spatialDataReader, i);
							}
						}
						else
						{
							ReadGeography(spatialDataReader, i);
						}
						continue;
					case TypeCase.DbGeometry:
						if (nullableColumns[i])
						{
							bool flag;
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
							if (!flag)
							{
								ReadGeometry(spatialDataReader, i);
							}
						}
						else
						{
							ReadGeometry(spatialDataReader, i);
						}
						continue;
					case TypeCase.Empty:
						if (nullableColumns[i])
						{
							_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i);
						}
						continue;
					}
					if (nullableColumns[i])
					{
						bool flag;
						_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = (flag = reader.IsDBNull(i));
						if (!flag)
						{
							ReadObject(reader, i);
						}
					}
					else
					{
						ReadObject(reader, i);
					}
				}
				else if (nullableColumns[i])
				{
					_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i);
				}
			}
		}
		_bools = new BitArray(_tempBools);
		_tempBools = null;
		_nulls = new BitArray(_tempNulls);
		_tempNulls = null;
		_rowCount = _currentRowNumber + 1;
		_currentRowNumber = -1;
		return this;
	}

	private async Task<BufferedDataRecord> InitializeAsync(DbDataReader reader, DbSpatialDataReader spatialDataReader, Type[] columnTypes, bool[] nullableColumns, CancellationToken cancellationToken)
	{
		InitializeFields(columnTypes, nullableColumns);
		while (await reader.ReadAsync(cancellationToken).WithCurrentCulture())
		{
			cancellationToken.ThrowIfCancellationRequested();
			_currentRowNumber++;
			if (_rowCapacity == _currentRowNumber)
			{
				DoubleBufferCapacity();
			}
			int columnCount = ((columnTypes.Length > nullableColumns.Length) ? columnTypes.Length : nullableColumns.Length);
			for (int i = 0; i < columnCount; i++)
			{
				if (i < _columnTypeCases.Length)
				{
					switch (_columnTypeCases[i])
					{
					case TypeCase.Bool:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls13 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls13[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadBoolAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadBoolAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Byte:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls8 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls8[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadByteAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadByteAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Char:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadCharAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadCharAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.DateTime:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls7 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls7[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadDateTimeAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadDateTimeAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Decimal:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls12 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls12[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadDecimalAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadDecimalAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Double:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls3 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls3[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadDoubleAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadDoubleAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Float:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls9 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls9[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadFloatAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadFloatAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Guid:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls2 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls2[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadGuidAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadGuidAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Short:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls11 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls11[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadShortAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadShortAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Int:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls5 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls5[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadIntAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadIntAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Long:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls14 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls14[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadLongAsync(reader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadLongAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.DbGeography:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls10 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls10[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadGeographyAsync(spatialDataReader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadGeographyAsync(spatialDataReader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.DbGeometry:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls6 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							bool flag2;
							tempNulls6[num] = (flag2 = flag);
							if (!flag2)
							{
								await ReadGeometryAsync(spatialDataReader, i, cancellationToken).WithCurrentCulture();
							}
						}
						else
						{
							await ReadGeometryAsync(spatialDataReader, i, cancellationToken).WithCurrentCulture();
						}
						continue;
					case TypeCase.Empty:
						if (nullableColumns[i])
						{
							bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
							bool[] tempNulls4 = _tempNulls;
							int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
							tempNulls4[num] = flag;
						}
						continue;
					}
					if (nullableColumns[i])
					{
						bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
						bool[] tempNulls15 = _tempNulls;
						int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
						bool flag2;
						tempNulls15[num] = (flag2 = flag);
						if (!flag2)
						{
							await ReadObjectAsync(reader, i, cancellationToken).WithCurrentCulture();
						}
					}
					else
					{
						await ReadObjectAsync(reader, i, cancellationToken).WithCurrentCulture();
					}
				}
				else if (nullableColumns[i])
				{
					bool flag = await reader.IsDBNullAsync(i, cancellationToken).WithCurrentCulture();
					bool[] tempNulls16 = _tempNulls;
					int num = _currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i];
					tempNulls16[num] = flag;
				}
			}
		}
		_bools = new BitArray(_tempBools);
		_tempBools = null;
		_nulls = new BitArray(_tempNulls);
		_tempNulls = null;
		_rowCount = _currentRowNumber + 1;
		_currentRowNumber = -1;
		return this;
	}

	private void InitializeFields(Type[] columnTypes, bool[] nullableColumns)
	{
		_columnTypeCases = Enumerable.Repeat(TypeCase.Empty, columnTypes.Length).ToArray();
		int count = Math.Max(base.FieldCount, Math.Max(columnTypes.Length, nullableColumns.Length));
		_ordinalToIndexMap = Enumerable.Repeat(-1, count).ToArray();
		for (int i = 0; i < columnTypes.Length; i++)
		{
			Type type = columnTypes[i];
			if (type == null)
			{
				continue;
			}
			if (type == typeof(bool))
			{
				_columnTypeCases[i] = TypeCase.Bool;
				_ordinalToIndexMap[i] = _boolCount;
				_boolCount++;
				continue;
			}
			if (type == typeof(byte))
			{
				_columnTypeCases[i] = TypeCase.Byte;
				_ordinalToIndexMap[i] = _byteCount;
				_byteCount++;
				continue;
			}
			if (type == typeof(char))
			{
				_columnTypeCases[i] = TypeCase.Char;
				_ordinalToIndexMap[i] = _charCount;
				_charCount++;
				continue;
			}
			if (type == typeof(DateTime))
			{
				_columnTypeCases[i] = TypeCase.DateTime;
				_ordinalToIndexMap[i] = _dateTimeCount;
				_dateTimeCount++;
				continue;
			}
			if (type == typeof(decimal))
			{
				_columnTypeCases[i] = TypeCase.Decimal;
				_ordinalToIndexMap[i] = _decimalCount;
				_decimalCount++;
				continue;
			}
			if (type == typeof(double))
			{
				_columnTypeCases[i] = TypeCase.Double;
				_ordinalToIndexMap[i] = _doubleCount;
				_doubleCount++;
				continue;
			}
			if (type == typeof(float))
			{
				_columnTypeCases[i] = TypeCase.Float;
				_ordinalToIndexMap[i] = _floatCount;
				_floatCount++;
				continue;
			}
			if (type == typeof(Guid))
			{
				_columnTypeCases[i] = TypeCase.Guid;
				_ordinalToIndexMap[i] = _guidCount;
				_guidCount++;
				continue;
			}
			if (type == typeof(short))
			{
				_columnTypeCases[i] = TypeCase.Short;
				_ordinalToIndexMap[i] = _shortCount;
				_shortCount++;
				continue;
			}
			if (type == typeof(int))
			{
				_columnTypeCases[i] = TypeCase.Int;
				_ordinalToIndexMap[i] = _intCount;
				_intCount++;
				continue;
			}
			if (type == typeof(long))
			{
				_columnTypeCases[i] = TypeCase.Long;
				_ordinalToIndexMap[i] = _longCount;
				_longCount++;
				continue;
			}
			if (type == typeof(DbGeography))
			{
				_columnTypeCases[i] = TypeCase.DbGeography;
			}
			else if (type == typeof(DbGeometry))
			{
				_columnTypeCases[i] = TypeCase.DbGeometry;
			}
			else
			{
				_columnTypeCases[i] = TypeCase.Object;
			}
			_ordinalToIndexMap[i] = _objectCount;
			_objectCount++;
		}
		_tempBools = new bool[_rowCapacity * _boolCount];
		_bytes = new byte[_rowCapacity * _byteCount];
		_chars = new char[_rowCapacity * _charCount];
		_dateTimes = new DateTime[_rowCapacity * _dateTimeCount];
		_decimals = new decimal[_rowCapacity * _decimalCount];
		_doubles = new double[_rowCapacity * _doubleCount];
		_floats = new float[_rowCapacity * _floatCount];
		_guids = new Guid[_rowCapacity * _guidCount];
		_shorts = new short[_rowCapacity * _shortCount];
		_ints = new int[_rowCapacity * _intCount];
		_longs = new long[_rowCapacity * _longCount];
		_objects = new object[_rowCapacity * _objectCount];
		_nullOrdinalToIndexMap = Enumerable.Repeat(-1, count).ToArray();
		for (int j = 0; j < nullableColumns.Length; j++)
		{
			if (nullableColumns[j])
			{
				_nullOrdinalToIndexMap[j] = _nullCount;
				_nullCount++;
			}
		}
		_tempNulls = new bool[_rowCapacity * _nullCount];
	}

	private void DoubleBufferCapacity()
	{
		_rowCapacity <<= 1;
		bool[] array = new bool[_tempBools.Length << 1];
		Array.Copy(_tempBools, array, _tempBools.Length);
		_tempBools = array;
		byte[] array2 = new byte[_bytes.Length << 1];
		Array.Copy(_bytes, array2, _bytes.Length);
		_bytes = array2;
		char[] array3 = new char[_chars.Length << 1];
		Array.Copy(_chars, array3, _chars.Length);
		_chars = array3;
		DateTime[] array4 = new DateTime[_dateTimes.Length << 1];
		Array.Copy(_dateTimes, array4, _dateTimes.Length);
		_dateTimes = array4;
		decimal[] array5 = new decimal[_decimals.Length << 1];
		Array.Copy(_decimals, array5, _decimals.Length);
		_decimals = array5;
		double[] array6 = new double[_doubles.Length << 1];
		Array.Copy(_doubles, array6, _doubles.Length);
		_doubles = array6;
		float[] array7 = new float[_floats.Length << 1];
		Array.Copy(_floats, array7, _floats.Length);
		_floats = array7;
		Guid[] array8 = new Guid[_guids.Length << 1];
		Array.Copy(_guids, array8, _guids.Length);
		_guids = array8;
		short[] array9 = new short[_shorts.Length << 1];
		Array.Copy(_shorts, array9, _shorts.Length);
		_shorts = array9;
		int[] array10 = new int[_ints.Length << 1];
		Array.Copy(_ints, array10, _ints.Length);
		_ints = array10;
		long[] array11 = new long[_longs.Length << 1];
		Array.Copy(_longs, array11, _longs.Length);
		_longs = array11;
		object[] array12 = new object[_objects.Length << 1];
		Array.Copy(_objects, array12, _objects.Length);
		_objects = array12;
		bool[] array13 = new bool[_tempNulls.Length << 1];
		Array.Copy(_tempNulls, array13, _tempNulls.Length);
		_tempNulls = array13;
	}

	private void ReadBool(DbDataReader reader, int ordinal)
	{
		_tempBools[_currentRowNumber * _boolCount + _ordinalToIndexMap[ordinal]] = reader.GetBoolean(ordinal);
	}

	private async Task ReadBoolAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		bool flag = await reader.GetFieldValueAsync<bool>(ordinal, cancellationToken).WithCurrentCulture();
		bool[] tempBools = _tempBools;
		int num = _currentRowNumber * _boolCount + _ordinalToIndexMap[ordinal];
		tempBools[num] = flag;
	}

	private void ReadByte(DbDataReader reader, int ordinal)
	{
		_bytes[_currentRowNumber * _byteCount + _ordinalToIndexMap[ordinal]] = reader.GetByte(ordinal);
	}

	private async Task ReadByteAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		byte b = await reader.GetFieldValueAsync<byte>(ordinal, cancellationToken).WithCurrentCulture();
		byte[] bytes = _bytes;
		int num = _currentRowNumber * _byteCount + _ordinalToIndexMap[ordinal];
		bytes[num] = b;
	}

	private void ReadChar(DbDataReader reader, int ordinal)
	{
		_chars[_currentRowNumber * _charCount + _ordinalToIndexMap[ordinal]] = reader.GetChar(ordinal);
	}

	private async Task ReadCharAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		char c = await reader.GetFieldValueAsync<char>(ordinal, cancellationToken).WithCurrentCulture();
		char[] chars = _chars;
		int num = _currentRowNumber * _charCount + _ordinalToIndexMap[ordinal];
		chars[num] = c;
	}

	private void ReadDateTime(DbDataReader reader, int ordinal)
	{
		_dateTimes[_currentRowNumber * _dateTimeCount + _ordinalToIndexMap[ordinal]] = reader.GetDateTime(ordinal);
	}

	private async Task ReadDateTimeAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		DateTime dateTime = await reader.GetFieldValueAsync<DateTime>(ordinal, cancellationToken).WithCurrentCulture();
		DateTime[] dateTimes = _dateTimes;
		int num = _currentRowNumber * _dateTimeCount + _ordinalToIndexMap[ordinal];
		dateTimes[num] = dateTime;
	}

	private void ReadDecimal(DbDataReader reader, int ordinal)
	{
		_decimals[_currentRowNumber * _decimalCount + _ordinalToIndexMap[ordinal]] = reader.GetDecimal(ordinal);
	}

	private async Task ReadDecimalAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		decimal num = await reader.GetFieldValueAsync<decimal>(ordinal, cancellationToken).WithCurrentCulture();
		decimal[] decimals = _decimals;
		int num2 = _currentRowNumber * _decimalCount + _ordinalToIndexMap[ordinal];
		decimals[num2] = num;
	}

	private void ReadDouble(DbDataReader reader, int ordinal)
	{
		_doubles[_currentRowNumber * _doubleCount + _ordinalToIndexMap[ordinal]] = reader.GetDouble(ordinal);
	}

	private async Task ReadDoubleAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		double num = await reader.GetFieldValueAsync<double>(ordinal, cancellationToken).WithCurrentCulture();
		double[] doubles = _doubles;
		int num2 = _currentRowNumber * _doubleCount + _ordinalToIndexMap[ordinal];
		doubles[num2] = num;
	}

	private void ReadFloat(DbDataReader reader, int ordinal)
	{
		_floats[_currentRowNumber * _floatCount + _ordinalToIndexMap[ordinal]] = reader.GetFloat(ordinal);
	}

	private async Task ReadFloatAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		float num = await reader.GetFieldValueAsync<float>(ordinal, cancellationToken).WithCurrentCulture();
		float[] floats = _floats;
		int num2 = _currentRowNumber * _floatCount + _ordinalToIndexMap[ordinal];
		floats[num2] = num;
	}

	private void ReadGuid(DbDataReader reader, int ordinal)
	{
		_guids[_currentRowNumber * _guidCount + _ordinalToIndexMap[ordinal]] = reader.GetGuid(ordinal);
	}

	private async Task ReadGuidAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		Guid guid = await reader.GetFieldValueAsync<Guid>(ordinal, cancellationToken).WithCurrentCulture();
		Guid[] guids = _guids;
		int num = _currentRowNumber * _guidCount + _ordinalToIndexMap[ordinal];
		guids[num] = guid;
	}

	private void ReadShort(DbDataReader reader, int ordinal)
	{
		_shorts[_currentRowNumber * _shortCount + _ordinalToIndexMap[ordinal]] = reader.GetInt16(ordinal);
	}

	private async Task ReadShortAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		short num = await reader.GetFieldValueAsync<short>(ordinal, cancellationToken).WithCurrentCulture();
		short[] shorts = _shorts;
		int num2 = _currentRowNumber * _shortCount + _ordinalToIndexMap[ordinal];
		shorts[num2] = num;
	}

	private void ReadInt(DbDataReader reader, int ordinal)
	{
		_ints[_currentRowNumber * _intCount + _ordinalToIndexMap[ordinal]] = reader.GetInt32(ordinal);
	}

	private async Task ReadIntAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		int num = await reader.GetFieldValueAsync<int>(ordinal, cancellationToken).WithCurrentCulture();
		int[] ints = _ints;
		int num2 = _currentRowNumber * _intCount + _ordinalToIndexMap[ordinal];
		ints[num2] = num;
	}

	private void ReadLong(DbDataReader reader, int ordinal)
	{
		_longs[_currentRowNumber * _longCount + _ordinalToIndexMap[ordinal]] = reader.GetInt64(ordinal);
	}

	private async Task ReadLongAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		long num = await reader.GetFieldValueAsync<long>(ordinal, cancellationToken).WithCurrentCulture();
		long[] longs = _longs;
		int num2 = _currentRowNumber * _longCount + _ordinalToIndexMap[ordinal];
		longs[num2] = num;
	}

	private void ReadObject(DbDataReader reader, int ordinal)
	{
		_objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] = reader.GetValue(ordinal);
	}

	private async Task ReadObjectAsync(DbDataReader reader, int ordinal, CancellationToken cancellationToken)
	{
		object obj = await reader.GetFieldValueAsync<object>(ordinal, cancellationToken).WithCurrentCulture();
		object[] objects = _objects;
		int num = _currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal];
		objects[num] = obj;
	}

	private void ReadGeography(DbSpatialDataReader spatialReader, int ordinal)
	{
		_objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] = spatialReader.GetGeography(ordinal);
	}

	private async Task ReadGeographyAsync(DbSpatialDataReader spatialReader, int ordinal, CancellationToken cancellationToken)
	{
		DbGeography dbGeography = await spatialReader.GetGeographyAsync(ordinal, cancellationToken).WithCurrentCulture();
		object[] objects = _objects;
		int num = _currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal];
		objects[num] = dbGeography;
	}

	private void ReadGeometry(DbSpatialDataReader spatialReader, int ordinal)
	{
		_objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] = spatialReader.GetGeometry(ordinal);
	}

	private async Task ReadGeometryAsync(DbSpatialDataReader spatialReader, int ordinal, CancellationToken cancellationToken)
	{
		DbGeometry dbGeometry = await spatialReader.GetGeometryAsync(ordinal, cancellationToken).WithCurrentCulture();
		object[] objects = _objects;
		int num = _currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal];
		objects[num] = dbGeometry;
	}

	public override bool GetBoolean(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Bool)
		{
			return _bools[_currentRowNumber * _boolCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<bool>(ordinal);
	}

	public override byte GetByte(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Byte)
		{
			return _bytes[_currentRowNumber * _byteCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<byte>(ordinal);
	}

	public override char GetChar(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Char)
		{
			return _chars[_currentRowNumber * _charCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<char>(ordinal);
	}

	public override DateTime GetDateTime(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.DateTime)
		{
			return _dateTimes[_currentRowNumber * _dateTimeCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<DateTime>(ordinal);
	}

	public override decimal GetDecimal(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Decimal)
		{
			return _decimals[_currentRowNumber * _decimalCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<decimal>(ordinal);
	}

	public override double GetDouble(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Double)
		{
			return _doubles[_currentRowNumber * _doubleCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<double>(ordinal);
	}

	public override float GetFloat(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Float)
		{
			return _floats[_currentRowNumber * _floatCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<float>(ordinal);
	}

	public override Guid GetGuid(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Guid)
		{
			return _guids[_currentRowNumber * _guidCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<Guid>(ordinal);
	}

	public override short GetInt16(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Short)
		{
			return _shorts[_currentRowNumber * _shortCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<short>(ordinal);
	}

	public override int GetInt32(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Int)
		{
			return _ints[_currentRowNumber * _intCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<int>(ordinal);
	}

	public override long GetInt64(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Long)
		{
			return _longs[_currentRowNumber * _longCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<long>(ordinal);
	}

	public override string GetString(int ordinal)
	{
		if (_columnTypeCases[ordinal] == TypeCase.Object)
		{
			return (string)_objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]];
		}
		return GetFieldValue<string>(ordinal);
	}

	public override object GetValue(int ordinal)
	{
		return GetFieldValue<object>(ordinal);
	}

	public override int GetValues(object[] values)
	{
		throw new NotSupportedException();
	}

	public override T GetFieldValue<T>(int ordinal)
	{
		return _columnTypeCases[ordinal] switch
		{
			TypeCase.Bool => (T)(object)GetBoolean(ordinal), 
			TypeCase.Byte => (T)(object)GetByte(ordinal), 
			TypeCase.Char => (T)(object)GetChar(ordinal), 
			TypeCase.DateTime => (T)(object)GetDateTime(ordinal), 
			TypeCase.Decimal => (T)(object)GetDecimal(ordinal), 
			TypeCase.Double => (T)(object)GetDouble(ordinal), 
			TypeCase.Float => (T)(object)GetFloat(ordinal), 
			TypeCase.Guid => (T)(object)GetGuid(ordinal), 
			TypeCase.Short => (T)(object)GetInt16(ordinal), 
			TypeCase.Int => (T)(object)GetInt32(ordinal), 
			TypeCase.Long => (T)(object)GetInt64(ordinal), 
			TypeCase.Empty => default(T), 
			_ => (T)_objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]], 
		};
	}

	public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
	{
		return Task.FromResult(GetFieldValue<T>(ordinal));
	}

	public override bool IsDBNull(int ordinal)
	{
		return _nulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[ordinal]];
	}

	public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
	{
		return Task.FromResult(IsDBNull(ordinal));
	}

	public override bool Read()
	{
		return base.IsDataReady = ++_currentRowNumber < _rowCount;
	}

	public override Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(Read());
	}
}

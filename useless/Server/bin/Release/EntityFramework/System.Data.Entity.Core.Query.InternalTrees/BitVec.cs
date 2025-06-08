using System.Collections.Concurrent;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class BitVec
{
	private class ArrayPool
	{
		private ConcurrentDictionary<int, ConcurrentBag<int[]>> dictionary;

		private static readonly ArrayPool instance = new ArrayPool();

		public static ArrayPool Instance => instance;

		private ArrayPool()
		{
			dictionary = new ConcurrentDictionary<int, ConcurrentBag<int[]>>();
		}

		public int[] GetArray(int length)
		{
			if (GetBag(length).TryTake(out var result))
			{
				return result;
			}
			return new int[length];
		}

		private ConcurrentBag<int[]> GetBag(int length)
		{
			return dictionary.GetOrAdd(length, (int l) => new ConcurrentBag<int[]>());
		}

		public void PutArray(int[] arr)
		{
			ConcurrentBag<int[]> bag = GetBag(arr.Length);
			Array.Clear(arr, 0, arr.Length);
			bag.Add(arr);
		}
	}

	private const int BitsPerInt32 = 32;

	private const int BytesPerInt32 = 4;

	private const int BitsPerByte = 8;

	public int[] m_array;

	private int m_length;

	private int _version;

	private const int _ShrinkThreshold = 1024;

	public bool this[int index]
	{
		get
		{
			return Get(index);
		}
		set
		{
			Set(index, value);
		}
	}

	public int Length
	{
		get
		{
			return m_length;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_NeedNonNegNum");
			}
			int arraySize = GetArraySize(value, 32);
			if (arraySize > m_array.Length || arraySize + 1024 < m_array.Length)
			{
				int[] array = ArrayPool.Instance.GetArray(arraySize);
				Array.Copy(m_array, array, (arraySize > m_array.Length) ? m_array.Length : arraySize);
				ArrayPool.Instance.PutArray(m_array);
				m_array = array;
			}
			if (value > m_length)
			{
				int num = GetArrayLength(m_length, 32) - 1;
				int num2 = m_length % 32;
				if (num2 > 0)
				{
					m_array[num] &= (1 << num2) - 1;
				}
				Array.Clear(m_array, num + 1, arraySize - num - 1);
			}
			m_length = value;
			_version++;
		}
	}

	private BitVec()
	{
	}

	public BitVec(int length)
		: this(length, defaultValue: false)
	{
	}

	public BitVec(int length, bool defaultValue)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_NeedNonNegNum");
		}
		m_array = ArrayPool.Instance.GetArray(GetArrayLength(length, 32));
		m_length = length;
		int num = (defaultValue ? (-1) : 0);
		for (int i = 0; i < m_array.Length; i++)
		{
			m_array[i] = num;
		}
		_version = 0;
	}

	public BitVec(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (bytes.Length > 268435455)
		{
			throw new ArgumentException("Argument_ArrayTooLarge", "bytes");
		}
		m_array = ArrayPool.Instance.GetArray(GetArrayLength(bytes.Length, 4));
		m_length = bytes.Length * 8;
		int num = 0;
		int i;
		for (i = 0; bytes.Length - i >= 4; i += 4)
		{
			m_array[num++] = (bytes[i] & 0xFF) | ((bytes[i + 1] & 0xFF) << 8) | ((bytes[i + 2] & 0xFF) << 16) | ((bytes[i + 3] & 0xFF) << 24);
		}
		switch (bytes.Length - i)
		{
		case 3:
			m_array[num] = (bytes[i + 2] & 0xFF) << 16;
			goto case 2;
		case 2:
			m_array[num] |= (bytes[i + 1] & 0xFF) << 8;
			goto case 1;
		case 1:
			m_array[num] |= bytes[i] & 0xFF;
			break;
		}
		_version = 0;
	}

	public BitVec(bool[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		m_array = ArrayPool.Instance.GetArray(GetArrayLength(values.Length, 32));
		m_length = values.Length;
		for (int i = 0; i < values.Length; i++)
		{
			if (values[i])
			{
				m_array[i / 32] |= 1 << i % 32;
			}
		}
		_version = 0;
	}

	public BitVec(int[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		_ = values.Length;
		_ = 67108863;
		m_array = ArrayPool.Instance.GetArray(values.Length);
		m_length = values.Length * 32;
		Array.Copy(values, m_array, values.Length);
		_version = 0;
	}

	public BitVec(BitVec bits)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		int arrayLength = GetArrayLength(bits.m_length, 32);
		m_array = ArrayPool.Instance.GetArray(arrayLength);
		m_length = bits.m_length;
		Array.Copy(bits.m_array, m_array, arrayLength);
		_version = bits._version;
	}

	public bool Get(int index)
	{
		if (index < 0 || index >= Length)
		{
			throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
		}
		return (m_array[index / 32] & (1 << index % 32)) != 0;
	}

	public void Set(int index, bool value)
	{
		if (index < 0 || index >= Length)
		{
			throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
		}
		if (value)
		{
			m_array[index / 32] |= 1 << index % 32;
		}
		else
		{
			m_array[index / 32] &= ~(1 << index % 32);
		}
		_version++;
	}

	public void SetAll(bool value)
	{
		int num = (value ? (-1) : 0);
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] = num;
		}
		_version++;
	}

	public BitVec And(BitVec value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Length != value.Length)
		{
			throw new ArgumentException("Arg_ArrayLengthsDiffer");
		}
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] &= value.m_array[i];
		}
		_version++;
		return this;
	}

	public BitVec Or(BitVec value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Length != value.Length)
		{
			throw new ArgumentException("Arg_ArrayLengthsDiffer");
		}
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] |= value.m_array[i];
		}
		_version++;
		return this;
	}

	public BitVec Xor(BitVec value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Length != value.Length)
		{
			throw new ArgumentException("Arg_ArrayLengthsDiffer");
		}
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] ^= value.m_array[i];
		}
		_version++;
		return this;
	}

	public BitVec Not()
	{
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] = ~m_array[i];
		}
		_version++;
		return this;
	}

	public static int GetArrayLength(int n, int div)
	{
		if (n <= 0)
		{
			return 0;
		}
		return (n - 1) / div + 1;
	}

	private static int GetArraySize(int n, int div)
	{
		uint num = Convert.ToUInt32(GetArrayLength(n, div)) - 1;
		uint num2 = num | (num >> 1);
		uint num3 = num2 | (num2 >> 2);
		uint num4 = num3 | (num3 >> 4);
		uint num5 = num4 | (num4 >> 8);
		return Convert.ToInt32((num5 | (num5 >> 16)) + 1);
	}
}

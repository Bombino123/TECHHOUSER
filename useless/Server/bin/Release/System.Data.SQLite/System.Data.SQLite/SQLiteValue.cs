namespace System.Data.SQLite;

public sealed class SQLiteValue : ISQLiteNativeHandle
{
	private IntPtr pValue;

	private bool persisted;

	private object value;

	public IntPtr NativeHandle => pValue;

	public bool Persisted => persisted;

	public object Value
	{
		get
		{
			if (!persisted)
			{
				throw new InvalidOperationException("value was not persisted");
			}
			return value;
		}
	}

	private SQLiteValue(IntPtr pValue)
	{
		this.pValue = pValue;
	}

	private void PreventNativeAccess()
	{
		pValue = IntPtr.Zero;
	}

	internal static SQLiteValue FromIntPtr(IntPtr pValue)
	{
		if (pValue == IntPtr.Zero)
		{
			return null;
		}
		return new SQLiteValue(pValue);
	}

	internal static SQLiteValue[] ArrayFromSizeAndIntPtr(int argc, IntPtr argv)
	{
		if (argc < 0)
		{
			return null;
		}
		if (argv == IntPtr.Zero)
		{
			return null;
		}
		SQLiteValue[] array = new SQLiteValue[argc];
		int num = 0;
		int num2 = 0;
		while (num < array.Length)
		{
			IntPtr intPtr = SQLiteMarshal.ReadIntPtr(argv, num2);
			array[num] = ((intPtr != IntPtr.Zero) ? new SQLiteValue(intPtr) : null);
			num++;
			num2 += IntPtr.Size;
		}
		return array;
	}

	public TypeAffinity GetTypeAffinity()
	{
		if (pValue == IntPtr.Zero)
		{
			return TypeAffinity.None;
		}
		return UnsafeNativeMethods.sqlite3_value_type(pValue);
	}

	public int GetBytes()
	{
		if (pValue == IntPtr.Zero)
		{
			return 0;
		}
		return UnsafeNativeMethods.sqlite3_value_bytes(pValue);
	}

	public int GetInt()
	{
		if (pValue == IntPtr.Zero)
		{
			return 0;
		}
		return UnsafeNativeMethods.sqlite3_value_int(pValue);
	}

	public long GetInt64()
	{
		if (pValue == IntPtr.Zero)
		{
			return 0L;
		}
		return UnsafeNativeMethods.sqlite3_value_int64(pValue);
	}

	public double GetDouble()
	{
		if (pValue == IntPtr.Zero)
		{
			return 0.0;
		}
		return UnsafeNativeMethods.sqlite3_value_double(pValue);
	}

	public string GetString()
	{
		if (pValue == IntPtr.Zero)
		{
			return null;
		}
		int len = 0;
		return SQLiteString.StringFromUtf8IntPtr(UnsafeNativeMethods.sqlite3_value_text_interop(pValue, ref len), len);
	}

	public byte[] GetBlob()
	{
		if (pValue == IntPtr.Zero)
		{
			return null;
		}
		return SQLiteBytes.FromIntPtr(UnsafeNativeMethods.sqlite3_value_blob(pValue), GetBytes());
	}

	public object GetObject()
	{
		return GetTypeAffinity() switch
		{
			TypeAffinity.Uninitialized => null, 
			TypeAffinity.Int64 => GetInt64(), 
			TypeAffinity.Double => GetDouble(), 
			TypeAffinity.Text => GetString(), 
			TypeAffinity.Blob => GetBytes(), 
			TypeAffinity.Null => DBNull.Value, 
			_ => null, 
		};
	}

	public bool Persist()
	{
		switch (GetTypeAffinity())
		{
		case TypeAffinity.Uninitialized:
			value = null;
			PreventNativeAccess();
			return persisted = true;
		case TypeAffinity.Int64:
			value = GetInt64();
			PreventNativeAccess();
			return persisted = true;
		case TypeAffinity.Double:
			value = GetDouble();
			PreventNativeAccess();
			return persisted = true;
		case TypeAffinity.Text:
			value = GetString();
			PreventNativeAccess();
			return persisted = true;
		case TypeAffinity.Blob:
			value = GetBytes();
			PreventNativeAccess();
			return persisted = true;
		case TypeAffinity.Null:
			value = DBNull.Value;
			PreventNativeAccess();
			return persisted = true;
		default:
			return false;
		}
	}
}

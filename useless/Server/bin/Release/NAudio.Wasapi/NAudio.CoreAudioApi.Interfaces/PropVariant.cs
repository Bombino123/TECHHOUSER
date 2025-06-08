using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace NAudio.CoreAudioApi.Interfaces;

[StructLayout(LayoutKind.Explicit)]
public struct PropVariant
{
	[FieldOffset(0)]
	public short vt;

	[FieldOffset(2)]
	public short wReserved1;

	[FieldOffset(4)]
	public short wReserved2;

	[FieldOffset(6)]
	public short wReserved3;

	[FieldOffset(8)]
	public sbyte cVal;

	[FieldOffset(8)]
	public byte bVal;

	[FieldOffset(8)]
	public short iVal;

	[FieldOffset(8)]
	public ushort uiVal;

	[FieldOffset(8)]
	public int lVal;

	[FieldOffset(8)]
	public uint ulVal;

	[FieldOffset(8)]
	public int intVal;

	[FieldOffset(8)]
	public uint uintVal;

	[FieldOffset(8)]
	public long hVal;

	[FieldOffset(8)]
	public long uhVal;

	[FieldOffset(8)]
	public float fltVal;

	[FieldOffset(8)]
	public double dblVal;

	[FieldOffset(8)]
	public short boolVal;

	[FieldOffset(8)]
	public int scode;

	[FieldOffset(8)]
	public FILETIME filetime;

	[FieldOffset(8)]
	public Blob blobVal;

	[FieldOffset(8)]
	public IntPtr pointerValue;

	public VarEnum DataType => (VarEnum)vt;

	public object Value
	{
		get
		{
			VarEnum dataType = DataType;
			switch (dataType)
			{
			case VarEnum.VT_I1:
				return bVal;
			case VarEnum.VT_I2:
				return iVal;
			case VarEnum.VT_I4:
				return lVal;
			case VarEnum.VT_I8:
				return hVal;
			case VarEnum.VT_INT:
				return iVal;
			case VarEnum.VT_UI4:
				return ulVal;
			case VarEnum.VT_UI8:
				return uhVal;
			case VarEnum.VT_LPWSTR:
				return Marshal.PtrToStringUni(pointerValue);
			case VarEnum.VT_BLOB:
			case (VarEnum)4113:
				return GetBlob();
			case VarEnum.VT_CLSID:
				return Marshal.PtrToStructure<Guid>(pointerValue);
			case VarEnum.VT_BOOL:
				return boolVal switch
				{
					-1 => true, 
					0 => false, 
					_ => throw new NotSupportedException("PropVariant VT_BOOL must be either -1 or 0"), 
				};
			case VarEnum.VT_FILETIME:
				return DateTime.FromFileTime(((long)filetime.dwHighDateTime << 32) + filetime.dwLowDateTime);
			default:
				throw new NotImplementedException("PropVariant " + dataType);
			}
		}
	}

	public static PropVariant FromLong(long value)
	{
		PropVariant result = default(PropVariant);
		result.vt = 20;
		result.hVal = value;
		return result;
	}

	private byte[] GetBlob()
	{
		byte[] array = new byte[blobVal.Length];
		Marshal.Copy(blobVal.Data, array, 0, array.Length);
		return array;
	}

	public T[] GetBlobAsArrayOf<T>()
	{
		int length = blobVal.Length;
		int num = Marshal.SizeOf((T)Activator.CreateInstance(typeof(T)));
		if (length % num != 0)
		{
			throw new InvalidDataException($"Blob size {length} not a multiple of struct size {num}");
		}
		int num2 = length / num;
		T[] array = new T[num2];
		for (int i = 0; i < num2; i++)
		{
			array[i] = (T)Activator.CreateInstance(typeof(T));
			Marshal.PtrToStructure(new IntPtr((long)blobVal.Data + i * num), array[i]);
		}
		return array;
	}

	public static void Clear(IntPtr ptr)
	{
		PropVariantNative.PropVariantClear(ptr);
	}
}

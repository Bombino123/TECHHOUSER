using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Vanara.Extensions;

public static class StringHelper
{
	public static IntPtr AllocChars(uint count, Func<int, IntPtr> memAllocator, CharSet charSet = CharSet.Auto)
	{
		if (count == 0)
		{
			return IntPtr.Zero;
		}
		int charSize = GetCharSize(charSet);
		IntPtr intPtr = memAllocator((int)count * charSize);
		if (count != 0)
		{
			if (charSize == 1)
			{
				Marshal.WriteByte(intPtr, 0);
			}
			else
			{
				Marshal.WriteInt16(intPtr, 0);
			}
		}
		return intPtr;
	}

	public static IntPtr AllocChars(uint count, CharSet charSet = CharSet.Auto)
	{
		return AllocChars(count, Marshal.AllocCoTaskMem, charSet);
	}

	public static IntPtr AllocSecureString(SecureString s, CharSet charSet = CharSet.Auto)
	{
		if (s == null)
		{
			return IntPtr.Zero;
		}
		if (GetCharSize(charSet) == 2)
		{
			return Marshal.SecureStringToCoTaskMemUnicode(s);
		}
		return Marshal.SecureStringToCoTaskMemAnsi(s);
	}

	public static IntPtr AllocSecureString(SecureString s, CharSet charSet, Func<int, IntPtr> memAllocator)
	{
		int allocatedBytes;
		return AllocSecureString(s, charSet, memAllocator, out allocatedBytes);
	}

	public static IntPtr AllocSecureString(SecureString s, CharSet charSet, Func<int, IntPtr> memAllocator, out int allocatedBytes)
	{
		allocatedBytes = 0;
		if (s == null)
		{
			return IntPtr.Zero;
		}
		int charSize = GetCharSize(charSet);
		Encoding encoding = ((charSize == 2) ? Encoding.Unicode : Encoding.ASCII);
		IntPtr ptr = AllocSecureString(s, charSet);
		string text = ((charSize == 2) ? Marshal.PtrToStringUni(ptr) : Marshal.PtrToStringAnsi(ptr));
		Marshal.FreeCoTaskMem(ptr);
		if (text == null)
		{
			return IntPtr.Zero;
		}
		byte[] bytes = encoding.GetBytes(text);
		IntPtr intPtr = memAllocator(bytes.Length);
		Marshal.Copy(bytes, 0, intPtr, bytes.Length);
		allocatedBytes = bytes.Length;
		return intPtr;
	}

	public static IntPtr AllocString(string s, CharSet charSet = CharSet.Auto)
	{
		return charSet switch
		{
			CharSet.Unicode => Marshal.StringToCoTaskMemUni(s), 
			CharSet.Auto => Marshal.StringToCoTaskMemAuto(s), 
			_ => Marshal.StringToCoTaskMemAnsi(s), 
		};
	}

	public static IntPtr AllocString(string s, CharSet charSet, Func<int, IntPtr> memAllocator)
	{
		int allocatedBytes;
		return AllocString(s, charSet, memAllocator, out allocatedBytes);
	}

	public static IntPtr AllocString(string s, CharSet charSet, Func<int, IntPtr> memAllocator, out int allocatedBytes)
	{
		if (s == null)
		{
			allocatedBytes = 0;
			return IntPtr.Zero;
		}
		byte[] bytes = s.GetBytes(nullTerm: true, charSet);
		IntPtr intPtr = memAllocator(bytes.Length);
		Marshal.Copy(bytes, 0, intPtr, allocatedBytes = bytes.Length);
		return intPtr;
	}

	public static void FreeSecureString(IntPtr ptr, int sizeInBytes, Action<IntPtr> memFreer)
	{
		if (!IsValue(ptr))
		{
			byte[] array = new byte[sizeInBytes];
			Marshal.Copy(array, 0, ptr, array.Length);
			memFreer(ptr);
		}
	}

	public static void FreeString(IntPtr ptr, CharSet charSet = CharSet.Auto)
	{
		if (!IsValue(ptr))
		{
			if (GetCharSize(charSet) == 2)
			{
				Marshal.ZeroFreeCoTaskMemUnicode(ptr);
			}
			else
			{
				Marshal.ZeroFreeCoTaskMemAnsi(ptr);
			}
		}
	}

	public static byte[] GetBytes(this string value, bool nullTerm = true, CharSet charSet = CharSet.Auto)
	{
		int charSize = GetCharSize(charSet);
		Encoding encoding = ((charSize == 1) ? Encoding.ASCII : Encoding.Unicode);
		byte[] array = new byte[encoding.GetByteCount(value) + (nullTerm ? charSize : 0)];
		encoding.GetBytes(value, 0, value.Length, array, 0);
		if (nullTerm)
		{
			encoding.GetBytes(new char[1], 0, 1, array, array.Length - charSize);
		}
		return array;
	}

	public static int GetByteCount(this string value, bool nullTerm = true, CharSet charSet = CharSet.Auto)
	{
		if (value == null)
		{
			return 0;
		}
		int charSize = GetCharSize(charSet);
		return ((charSize == 1) ? Encoding.ASCII : Encoding.Unicode).GetByteCount(value) + (nullTerm ? charSize : 0);
	}

	public static int GetCharSize(CharSet charSet = CharSet.Auto)
	{
		return charSet switch
		{
			CharSet.Unicode => 2, 
			CharSet.Auto => Marshal.SystemDefaultCharSize, 
			_ => 1, 
		};
	}

	public unsafe static string GetString(IntPtr ptr, CharSet charSet = CharSet.Auto, long allocatedBytes = long.MaxValue)
	{
		if (IsValue(ptr))
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		long num = 0L;
		if (GetCharSize(charSet) == 1)
		{
			byte* ptr2 = (byte*)(void*)ptr;
			while (num < allocatedBytes && *ptr2 != 0)
			{
				stringBuilder.Append((char)(*ptr2));
				num++;
				ptr2++;
			}
		}
		else
		{
			ushort* ptr3 = (ushort*)(void*)ptr;
			while (num + 2 <= allocatedBytes && *ptr3 != 0)
			{
				stringBuilder.Append((char)(*ptr3));
				num += 2;
				ptr3++;
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetString(IntPtr ptr, int length, CharSet charSet = CharSet.Auto)
	{
		return GetString(ptr, charSet, length * GetCharSize(charSet));
	}

	public static bool RefreshString(ref IntPtr ptr, out uint charLen, string s, CharSet charSet = CharSet.Auto)
	{
		FreeString(ptr, charSet);
		ptr = AllocString(s, charSet);
		charLen = ((s != null) ? ((uint)(s.Length + 1)) : 0u);
		return s != null;
	}

	public static void Write(string value, IntPtr ptr, out int byteCnt, bool nullTerm = true, CharSet charSet = CharSet.Auto, long allocatedBytes = long.MaxValue)
	{
		if (value == null)
		{
			byteCnt = 0;
			return;
		}
		if (ptr == IntPtr.Zero)
		{
			throw new ArgumentNullException("ptr");
		}
		byte[] bytes = value.GetBytes(nullTerm, charSet);
		if (bytes.Length > allocatedBytes)
		{
			throw new ArgumentOutOfRangeException("allocatedBytes");
		}
		byteCnt = bytes.Length;
		Marshal.Copy(bytes, 0, ptr, byteCnt);
	}

	private static bool IsValue(IntPtr ptr)
	{
		return ptr.ToInt64() >> 16 == 0;
	}
}

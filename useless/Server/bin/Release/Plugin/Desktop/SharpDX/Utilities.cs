using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;

namespace SharpDX;

public static class Utilities
{
	[Flags]
	public enum CLSCTX : uint
	{
		ClsctxInprocServer = 1u,
		ClsctxInprocHandler = 2u,
		ClsctxLocalServer = 4u,
		ClsctxInprocServer16 = 8u,
		ClsctxRemoteServer = 0x10u,
		ClsctxInprocHandler16 = 0x20u,
		ClsctxReserved1 = 0x40u,
		ClsctxReserved2 = 0x80u,
		ClsctxReserved3 = 0x100u,
		ClsctxReserved4 = 0x200u,
		ClsctxNoCodeDownload = 0x400u,
		ClsctxReserved5 = 0x800u,
		ClsctxNoCustomMarshal = 0x1000u,
		ClsctxEnableCodeDownload = 0x2000u,
		ClsctxNoFailureLog = 0x4000u,
		ClsctxDisableAaa = 0x8000u,
		ClsctxEnableAaa = 0x10000u,
		ClsctxFromDefaultContext = 0x20000u,
		ClsctxInproc = 3u,
		ClsctxServer = 0x15u,
		ClsctxAll = 0x17u
	}

	public enum CoInit
	{
		MultiThreaded = 0,
		ApartmentThreaded = 2,
		DisableOle1Dde = 4,
		SpeedOverMemory = 8
	}

	internal struct Buffer<TElement>
	{
		internal TElement[] items;

		internal int count;

		internal Buffer(IEnumerable<TElement> source)
		{
			TElement[] array = null;
			int num = 0;
			if (source is ICollection<TElement> collection)
			{
				num = collection.Count;
				if (num > 0)
				{
					array = new TElement[num];
					collection.CopyTo(array, 0);
				}
			}
			else
			{
				foreach (TElement item in source)
				{
					if (array == null)
					{
						array = new TElement[4];
					}
					else if (array.Length == num)
					{
						TElement[] array2 = new TElement[checked(num * 2)];
						Array.Copy(array, 0, array2, 0, num);
						array = array2;
					}
					array[num] = item;
					num++;
				}
			}
			items = array;
			count = num;
		}

		internal TElement[] ToArray()
		{
			if (count == 0)
			{
				return new TElement[0];
			}
			if (items.Length == count)
			{
				return items;
			}
			TElement[] array = new TElement[count];
			Array.Copy(items, 0, array, 0, count);
			return array;
		}
	}

	public unsafe static void CopyMemory(IntPtr dest, IntPtr src, int sizeInBytesToCopy)
	{
		Interop.memcpy((void*)dest, (void*)src, sizeInBytesToCopy);
	}

	public unsafe static bool CompareMemory(IntPtr from, IntPtr against, int sizeToCompare)
	{
		byte* ptr = (byte*)(void*)from;
		byte* ptr2 = (byte*)(void*)against;
		for (int num = sizeToCompare >> 3; num > 0; num--)
		{
			if (*(long*)ptr != *(long*)ptr2)
			{
				return false;
			}
			ptr += 8;
			ptr2 += 8;
		}
		for (int num = sizeToCompare & 7; num > 0; num--)
		{
			if (*ptr != *ptr2)
			{
				return false;
			}
			ptr++;
			ptr2++;
		}
		return true;
	}

	public unsafe static void ClearMemory(IntPtr dest, byte value, int sizeInBytesToClear)
	{
		Interop.memset((void*)dest, value, sizeInBytesToClear);
	}

	public static int SizeOf<T>() where T : struct
	{
		return System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
	}

	public static int SizeOf<T>(T[] array) where T : struct
	{
		if (array != null)
		{
			return array.Length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
		}
		return 0;
	}

	public unsafe static void Pin<T>(ref T source, Action<IntPtr> pinAction) where T : struct
	{
		fixed (T* ptr = &source)
		{
			pinAction((IntPtr)ptr);
		}
	}

	public unsafe static void Pin<T>(T[] source, Action<IntPtr> pinAction) where T : struct
	{
		//The blocks IL_001f are reachable both inside and outside the pinned region starting at IL_000e. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		IntPtr obj;
		if (source != null)
		{
			fixed (T* ptr = &source[0])
			{
				obj = (IntPtr)ptr;
				pinAction(obj);
				return;
			}
		}
		obj = IntPtr.Zero;
		pinAction(obj);
	}

	public unsafe static byte[] ToByteArray<T>(T[] source) where T : struct
	{
		if (source == null)
		{
			return null;
		}
		byte[] array = new byte[SizeOf<T>() * source.Length];
		if (source.Length == 0)
		{
			return array;
		}
		fixed (byte* ptr = array)
		{
			void* pDest = ptr;
			Interop.Write(pDest, source, 0, source.Length);
		}
		return array;
	}

	public static void Swap<T>(ref T left, ref T right)
	{
		T val = left;
		left = right;
		right = val;
	}

	public unsafe static T Read<T>(IntPtr source) where T : struct
	{
		return System.Runtime.CompilerServices.Unsafe.Read<T>((void*)source);
	}

	public unsafe static void Read<T>(IntPtr source, ref T data) where T : struct
	{
		data = System.Runtime.CompilerServices.Unsafe.Read<T>((void*)source);
	}

	public unsafe static void ReadOut<T>(IntPtr source, out T data) where T : struct
	{
		data = System.Runtime.CompilerServices.Unsafe.Read<T>((void*)source);
	}

	public unsafe static IntPtr ReadAndPosition<T>(IntPtr source, ref T data) where T : struct
	{
		return (IntPtr)Interop.Read((void*)source, ref data);
	}

	public unsafe static IntPtr Read<T>(IntPtr source, T[] data, int offset, int count) where T : struct
	{
		return (IntPtr)Interop.Read((void*)source, data, offset, count);
	}

	public unsafe static void Write<T>(IntPtr destination, ref T data) where T : struct
	{
		System.Runtime.CompilerServices.Unsafe.Write((void*)destination, data);
	}

	public unsafe static IntPtr WriteAndPosition<T>(IntPtr destination, ref T data) where T : struct
	{
		return (IntPtr)Interop.Write((void*)destination, ref data);
	}

	public unsafe static IntPtr Write<T>(IntPtr destination, T[] data, int offset, int count) where T : struct
	{
		return (IntPtr)Interop.Write((void*)destination, data, offset, count);
	}

	public unsafe static void ConvertToIntArray(bool[] array, int* dest)
	{
		for (int i = 0; i < array.Length; i++)
		{
			dest[i] = (array[i] ? 1 : 0);
		}
	}

	public static RawBool[] ConvertToIntArray(bool[] array)
	{
		RawBool[] array2 = new RawBool[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	public unsafe static bool[] ConvertToBoolArray(int* array, int length)
	{
		bool[] array2 = new bool[length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i] != 0;
		}
		return array2;
	}

	public static bool[] ConvertToBoolArray(RawBool[] array)
	{
		bool[] array2 = new bool[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	public static Guid GetGuidFromType(Type type)
	{
		return SharpDXTypeExtensions.GetTypeInfo(type).GUID;
	}

	public static bool IsAssignableToGenericType(Type givenType, Type genericType)
	{
		Type[] interfaces = SharpDXTypeExtensions.GetTypeInfo(givenType).GetInterfaces();
		foreach (Type type in interfaces)
		{
			if (SharpDXTypeExtensions.GetTypeInfo(type).IsGenericType && type.GetGenericTypeDefinition() == genericType)
			{
				return true;
			}
		}
		if (SharpDXTypeExtensions.GetTypeInfo(givenType).IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
		{
			return true;
		}
		Type baseType = SharpDXTypeExtensions.GetTypeInfo(givenType).BaseType;
		if (baseType == null)
		{
			return false;
		}
		return IsAssignableToGenericType(baseType, genericType);
	}

	public unsafe static IntPtr AllocateMemory(int sizeInBytes, int align = 16)
	{
		int num = align - 1;
		IntPtr intPtr = Marshal.AllocHGlobal(sizeInBytes + num + IntPtr.Size);
		long num2 = (long)((byte*)(void*)intPtr + sizeof(void*) + num) & (long)(~num);
		*(IntPtr*)((nint)num2 + (nint)(-1) * (nint)sizeof(IntPtr)) = intPtr;
		return new IntPtr((void*)num2);
	}

	public static IntPtr AllocateClearedMemory(int sizeInBytes, byte clearValue = 0, int align = 16)
	{
		IntPtr intPtr = AllocateMemory(sizeInBytes, align);
		ClearMemory(intPtr, clearValue, sizeInBytes);
		return intPtr;
	}

	public static bool IsMemoryAligned(IntPtr memoryPtr, int align = 16)
	{
		return (memoryPtr.ToInt64() & (align - 1)) == 0;
	}

	public unsafe static void FreeMemory(IntPtr alignedBuffer)
	{
		if (!(alignedBuffer == IntPtr.Zero))
		{
			Marshal.FreeHGlobal(*(IntPtr*)((byte*)(void*)alignedBuffer + (nint)(-1) * (nint)sizeof(IntPtr)));
		}
	}

	public static string PtrToStringAnsi(IntPtr pointer, int maxLength)
	{
		string text = Marshal.PtrToStringAnsi(pointer);
		if (text != null && text.Length > maxLength)
		{
			text = text.Substring(0, maxLength);
		}
		return text;
	}

	public static string PtrToStringUni(IntPtr pointer, int maxLength)
	{
		string text = Marshal.PtrToStringUni(pointer);
		if (text != null && text.Length > maxLength)
		{
			text = text.Substring(0, maxLength);
		}
		return text;
	}

	public static IntPtr StringToHGlobalAnsi(string s)
	{
		return Marshal.StringToHGlobalAnsi(s);
	}

	public static IntPtr StringToHGlobalUni(string s)
	{
		return Marshal.StringToHGlobalUni(s);
	}

	public static IntPtr StringToCoTaskMemUni(string s)
	{
		if (s == null)
		{
			return IntPtr.Zero;
		}
		int num = (s.Length + 1) * 2;
		if (num < s.Length)
		{
			throw new ArgumentOutOfRangeException("s");
		}
		IntPtr intPtr = Marshal.AllocCoTaskMem(num);
		if (intPtr == IntPtr.Zero)
		{
			throw new OutOfMemoryException();
		}
		CopyStringToUnmanaged(intPtr, s);
		return intPtr;
	}

	private unsafe static void CopyStringToUnmanaged(IntPtr ptr, string str)
	{
		fixed (char* value = str)
		{
			CopyMemory(ptr, new IntPtr(value), (str.Length + 1) * 2);
		}
	}

	public static IntPtr GetIUnknownForObject(object obj)
	{
		if (obj != null)
		{
			return Marshal.GetIUnknownForObject(obj);
		}
		return IntPtr.Zero;
	}

	public static object GetObjectForIUnknown(IntPtr iunknownPtr)
	{
		if (!(iunknownPtr == IntPtr.Zero))
		{
			return Marshal.GetObjectForIUnknown(iunknownPtr);
		}
		return null;
	}

	public static string Join<T>(string separator, T[] array)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(separator);
				}
				stringBuilder.Append(array[i]);
			}
		}
		return stringBuilder.ToString();
	}

	public static string Join(string separator, IEnumerable elements)
	{
		List<string> list = new List<string>();
		foreach (object element in elements)
		{
			list.Add(element.ToString());
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			string value = list[i];
			if (i > 0)
			{
				stringBuilder.Append(separator);
			}
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public static string Join(string separator, IEnumerator elements)
	{
		List<string> list = new List<string>();
		while (elements.MoveNext())
		{
			list.Add(elements.Current.ToString());
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			string value = list[i];
			if (i > 0)
			{
				stringBuilder.Append(separator);
			}
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public static string BlobToString(Blob blob)
	{
		if (blob == null)
		{
			return null;
		}
		string? result = Marshal.PtrToStringAnsi(blob.BufferPointer);
		blob.Dispose();
		return result;
	}

	public unsafe static IntPtr IntPtrAdd(IntPtr ptr, int offset)
	{
		return new IntPtr((byte*)(void*)ptr + offset);
	}

	public static byte[] ReadStream(Stream stream)
	{
		int readLength = 0;
		return ReadStream(stream, ref readLength);
	}

	public static byte[] ReadStream(Stream stream, ref int readLength)
	{
		if (readLength == 0)
		{
			readLength = (int)(stream.Length - stream.Position);
		}
		int num = readLength;
		if (num == 0)
		{
			return new byte[0];
		}
		byte[] array = new byte[num];
		int num2 = 0;
		if (num > 0)
		{
			do
			{
				num2 += stream.Read(array, num2, readLength - num2);
			}
			while (num2 < readLength);
		}
		return array;
	}

	public static bool Compare(IEnumerable left, IEnumerable right)
	{
		if (left == right)
		{
			return true;
		}
		if (left == null || right == null)
		{
			return false;
		}
		return Compare(left.GetEnumerator(), right.GetEnumerator());
	}

	public static bool Compare(IEnumerator leftIt, IEnumerator rightIt)
	{
		if (leftIt == rightIt)
		{
			return true;
		}
		if (leftIt == null || rightIt == null)
		{
			return false;
		}
		bool flag;
		bool flag2;
		while (true)
		{
			flag = leftIt.MoveNext();
			flag2 = rightIt.MoveNext();
			if (!flag || !flag2)
			{
				break;
			}
			if (!object.Equals(leftIt.Current, rightIt.Current))
			{
				return false;
			}
		}
		if (flag != flag2)
		{
			return false;
		}
		return true;
	}

	public static bool Compare(ICollection left, ICollection right)
	{
		if (left == right)
		{
			return true;
		}
		if (left == null || right == null)
		{
			return false;
		}
		if (left.Count != right.Count)
		{
			return false;
		}
		int num = 0;
		IEnumerator enumerator = left.GetEnumerator();
		IEnumerator enumerator2 = right.GetEnumerator();
		while (enumerator.MoveNext() && enumerator2.MoveNext())
		{
			if (!object.Equals(enumerator.Current, enumerator2.Current))
			{
				return false;
			}
			num++;
		}
		if (num != left.Count)
		{
			return false;
		}
		return true;
	}

	public static T GetCustomAttribute<T>(MemberInfo memberInfo, bool inherited = false) where T : Attribute
	{
		return SharpDXTypeExtensions.GetCustomAttribute<T>(memberInfo, inherited);
	}

	public static IEnumerable<T> GetCustomAttributes<T>(MemberInfo memberInfo, bool inherited = false) where T : Attribute
	{
		return SharpDXTypeExtensions.GetCustomAttributes<T>(memberInfo, inherited);
	}

	public static bool IsAssignableFrom(Type toType, Type fromType)
	{
		return SharpDXTypeExtensions.GetTypeInfo(toType).IsAssignableFrom(SharpDXTypeExtensions.GetTypeInfo(fromType));
	}

	public static bool IsEnum(Type typeToTest)
	{
		return SharpDXTypeExtensions.GetTypeInfo(typeToTest).IsEnum;
	}

	public static bool IsValueType(Type typeToTest)
	{
		return SharpDXTypeExtensions.GetTypeInfo(typeToTest).IsValueType;
	}

	private static MethodInfo GetMethod(Type type, string name, Type[] typeArgs)
	{
		MethodInfo[] methods = SharpDXTypeExtensions.GetTypeInfo(type).GetMethods(BindingFlags.Instance | BindingFlags.Public);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.Name != name || methodInfo.GetParameters().Length != typeArgs.Length)
			{
				continue;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			bool flag = true;
			for (int j = 0; j < typeArgs.Length; j++)
			{
				if (parameters[j].ParameterType != typeArgs[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return methodInfo;
			}
		}
		return null;
	}

	public static GetValueFastDelegate<T> BuildPropertyGetter<T>(Type customEffectType, PropertyInfo propertyInfo)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(T).MakeByRefType());
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object));
		MemberExpression memberExpression = Expression.Property(Expression.Convert(parameterExpression2, customEffectType), propertyInfo);
		Expression right = ((!(propertyInfo.PropertyType == typeof(bool))) ? ((Expression)Expression.Convert(memberExpression, typeof(T))) : ((Expression)Expression.Condition(memberExpression, Expression.Constant(1), Expression.Constant(0))));
		return Expression.Lambda<GetValueFastDelegate<T>>(Expression.Assign(parameterExpression, right), new ParameterExpression[2] { parameterExpression2, parameterExpression }).Compile();
	}

	public static SetValueFastDelegate<T> BuildPropertySetter<T>(Type customEffectType, PropertyInfo propertyInfo)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(T).MakeByRefType());
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object));
		MemberExpression left = Expression.Property(Expression.Convert(parameterExpression2, customEffectType), propertyInfo);
		Expression right = ((!(propertyInfo.PropertyType == typeof(bool))) ? ((Expression)Expression.Convert(parameterExpression, propertyInfo.PropertyType)) : ((Expression)Expression.NotEqual(parameterExpression, Expression.Constant(0))));
		return Expression.Lambda<SetValueFastDelegate<T>>(Expression.Assign(left, right), new ParameterExpression[2] { parameterExpression2, parameterExpression }).Compile();
	}

	private static MethodInfo FindExplicitConverstion(Type sourceType, Type targetType)
	{
		if (sourceType == targetType)
		{
			return null;
		}
		List<MethodInfo> list = new List<MethodInfo>();
		Type type = sourceType;
		while (type != null)
		{
			list.AddRange(SharpDXTypeExtensions.GetTypeInfo(type).GetMethods(BindingFlags.Public));
			type = SharpDXTypeExtensions.GetTypeInfo(type).BaseType;
		}
		type = targetType;
		while (type != null)
		{
			list.AddRange(SharpDXTypeExtensions.GetTypeInfo(type).GetMethods(BindingFlags.Public));
			type = SharpDXTypeExtensions.GetTypeInfo(type).BaseType;
		}
		foreach (MethodInfo item in list)
		{
			if (item.Name == "op_Explicit" && item.ReturnType == targetType && IsAssignableFrom(item.GetParameters()[0].ParameterType, sourceType))
			{
				return item;
			}
		}
		return null;
	}

	[DllImport("ole32.dll", ExactSpelling = true)]
	private static extern Result CoCreateInstance([In][MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, IntPtr pUnkOuter, CLSCTX dwClsContext, [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr comObject);

	internal static void CreateComInstance(Guid clsid, CLSCTX clsctx, Guid riid, ComObject comObject)
	{
		CoCreateInstance(clsid, IntPtr.Zero, clsctx, riid, out var comObject2).CheckError();
		comObject.NativePointer = comObject2;
	}

	internal static bool TryCreateComInstance(Guid clsid, CLSCTX clsctx, Guid riid, ComObject comObject)
	{
		IntPtr comObject2;
		Result result = CoCreateInstance(clsid, IntPtr.Zero, clsctx, riid, out comObject2);
		comObject.NativePointer = comObject2;
		return result.Success;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern bool CloseHandle(IntPtr handle);

	public static IntPtr GetProcAddress(IntPtr handle, string dllFunctionToImport)
	{
		IntPtr procAddress_ = GetProcAddress_(handle, dllFunctionToImport);
		if (procAddress_ == IntPtr.Zero)
		{
			throw new SharpDXException(dllFunctionToImport);
		}
		return procAddress_;
	}

	[DllImport("kernel32", CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", ExactSpelling = true, SetLastError = true)]
	private static extern IntPtr GetProcAddress_(IntPtr hModule, string procName);

	public static int ComputeHashFNVModified(byte[] data)
	{
		uint num = 2166136261u;
		foreach (byte b in data)
		{
			num = (num ^ b) * 16777619;
		}
		num += num << 13;
		num ^= num >> 7;
		num += num << 3;
		num ^= num >> 17;
		return (int)(num + (num << 5));
	}

	public static void Dispose<T>(ref T comObject) where T : class, IDisposable
	{
		if (comObject != null)
		{
			comObject.Dispose();
			comObject = null;
		}
	}

	public static T[] ToArray<T>(IEnumerable<T> source)
	{
		return new Buffer<T>(source).ToArray();
	}

	public static bool Any<T>(IEnumerable<T> source)
	{
		return source.GetEnumerator().MoveNext();
	}

	public static IEnumerable<TResult> SelectMany<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
	{
		foreach (TSource item in source)
		{
			foreach (TResult item2 in selector(item))
			{
				yield return item2;
			}
		}
	}

	public static IEnumerable<TSource> Distinct<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer = null)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		Dictionary<TSource, object> values = new Dictionary<TSource, object>(comparer);
		foreach (TSource item in source)
		{
			if (!values.ContainsKey(item))
			{
				values.Add(item, null);
				yield return item;
			}
		}
	}

	public static bool IsTypeInheritFrom(Type type, string parentType)
	{
		while (type != null)
		{
			if (type.FullName == parentType)
			{
				return true;
			}
			type = SharpDXTypeExtensions.GetTypeInfo(type).BaseType;
		}
		return false;
	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;

namespace SharpDX;

public class DataBuffer : DisposeBase
{
	private unsafe sbyte* _buffer;

	private GCHandle _gCHandle;

	private readonly bool _ownsBuffer;

	private readonly int _size;

	private Blob _blob;

	public unsafe IntPtr DataPointer => new IntPtr(_buffer);

	public int Size => _size;

	public unsafe static DataBuffer Create<T>(T[] userBuffer, int index = 0, bool pinBuffer = true) where T : struct
	{
		if (userBuffer == null)
		{
			throw new ArgumentNullException("userBuffer");
		}
		if (index < 0 || index > userBuffer.Length)
		{
			throw new ArgumentException("Index is out of range [0, userBuffer.Length-1]", "index");
		}
		int num = Utilities.SizeOf(userBuffer);
		int num2 = index * Utilities.SizeOf<T>();
		if (pinBuffer)
		{
			GCHandle handle = GCHandle.Alloc(userBuffer, GCHandleType.Pinned);
			return new DataBuffer(num2 + (byte*)(void*)handle.AddrOfPinnedObject(), num - num2, handle);
		}
		fixed (T* ptr = &userBuffer[0])
		{
			return new DataBuffer(num2 + (byte*)(void*)(IntPtr)ptr, num - num2, makeCopy: true);
		}
	}

	public unsafe DataBuffer(int sizeInBytes)
	{
		_buffer = (sbyte*)(void*)Utilities.AllocateMemory(sizeInBytes);
		_size = sizeInBytes;
		_ownsBuffer = true;
	}

	public DataBuffer(DataPointer dataPointer)
		: this(dataPointer.Pointer, dataPointer.Size)
	{
	}

	public unsafe DataBuffer(IntPtr userBuffer, int sizeInBytes)
		: this((void*)userBuffer, sizeInBytes, makeCopy: false)
	{
	}

	internal unsafe DataBuffer(void* buffer, int sizeInBytes, GCHandle handle)
	{
		_buffer = (sbyte*)buffer;
		_size = sizeInBytes;
		_gCHandle = handle;
		_ownsBuffer = false;
	}

	internal unsafe DataBuffer(void* buffer, int sizeInBytes, bool makeCopy)
	{
		if (makeCopy)
		{
			_buffer = (sbyte*)(void*)Utilities.AllocateMemory(sizeInBytes);
			Utilities.CopyMemory((IntPtr)_buffer, (IntPtr)buffer, sizeInBytes);
		}
		else
		{
			_buffer = (sbyte*)buffer;
		}
		_size = sizeInBytes;
		_ownsBuffer = makeCopy;
	}

	internal unsafe DataBuffer(Blob buffer)
	{
		_buffer = (sbyte*)(void*)buffer.GetBufferPointer();
		_size = buffer.GetBufferSize();
		_blob = buffer;
	}

	protected unsafe override void Dispose(bool disposing)
	{
		if (disposing && _blob != null)
		{
			_blob.Dispose();
			_blob = null;
		}
		if (_gCHandle.IsAllocated)
		{
			_gCHandle.Free();
		}
		if (_ownsBuffer && _buffer != null)
		{
			Utilities.FreeMemory((IntPtr)_buffer);
			_buffer = null;
		}
	}

	public unsafe void Clear(byte value = 0)
	{
		Utilities.ClearMemory((IntPtr)_buffer, value, Size);
	}

	public unsafe T Get<T>(int positionInBytes) where T : struct
	{
		T data = default(T);
		Utilities.Read((IntPtr)(_buffer + positionInBytes), ref data);
		return data;
	}

	public unsafe void Get<T>(int positionInBytes, out T value) where T : struct
	{
		Utilities.ReadOut<T>((IntPtr)(_buffer + positionInBytes), out value);
	}

	public unsafe T[] GetRange<T>(int positionInBytes, int count) where T : struct
	{
		T[] array = new T[count];
		Utilities.Read((IntPtr)(_buffer + positionInBytes), array, 0, count);
		return array;
	}

	public unsafe void GetRange<T>(int positionInBytes, T[] buffer, int offset, int count) where T : struct
	{
		Utilities.Read((IntPtr)(_buffer + positionInBytes), buffer, offset, count);
	}

	public unsafe void Set<T>(int positionInBytes, ref T value) where T : struct
	{
		System.Runtime.CompilerServices.Unsafe.Write(_buffer + positionInBytes, value);
	}

	public unsafe void Set<T>(int positionInBytes, T value) where T : struct
	{
		System.Runtime.CompilerServices.Unsafe.Write(_buffer + positionInBytes, value);
	}

	public unsafe void Set(int positionInBytes, bool value)
	{
		*(int*)(_buffer + positionInBytes) = (value ? 1 : 0);
	}

	public void Set<T>(int positionInBytes, T[] data) where T : struct
	{
		Set(positionInBytes, data, 0, data.Length);
	}

	public unsafe void Set(int positionInBytes, IntPtr source, long count)
	{
		Utilities.CopyMemory((IntPtr)(_buffer + positionInBytes), source, (int)count);
	}

	public unsafe void Set<T>(int positionInBytes, T[] data, int offset, int count) where T : struct
	{
		Utilities.Write((IntPtr)(_buffer + positionInBytes), data, offset, count);
	}

	public static implicit operator DataPointer(DataBuffer from)
	{
		return new DataPointer(from.DataPointer, from.Size);
	}
}

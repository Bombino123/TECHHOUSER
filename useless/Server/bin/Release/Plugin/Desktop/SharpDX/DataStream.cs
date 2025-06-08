using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;

namespace SharpDX;

public class DataStream : Stream
{
	private unsafe byte* _buffer;

	private readonly bool _canRead;

	private readonly bool _canWrite;

	private GCHandle _gCHandle;

	private Blob _blob;

	private readonly bool _ownsBuffer;

	private long _position;

	private readonly long _size;

	public override bool CanRead => _canRead;

	public override bool CanSeek => true;

	public override bool CanWrite => _canWrite;

	public unsafe IntPtr DataPointer => new IntPtr(_buffer);

	public override long Length => _size;

	public override long Position
	{
		get
		{
			return _position;
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
		}
	}

	public unsafe IntPtr PositionPointer => (IntPtr)(_buffer + _position);

	public long RemainingLength => _size - _position;

	public unsafe DataStream(Blob buffer)
	{
		_buffer = (byte*)(void*)buffer.GetBufferPointer();
		_size = buffer.GetBufferSize();
		_canRead = true;
		_canWrite = true;
		_blob = buffer;
	}

	public unsafe static DataStream Create<T>(T[] userBuffer, bool canRead, bool canWrite, int index = 0, bool pinBuffer = true) where T : struct
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
			return new DataStream(num2 + (byte*)(void*)handle.AddrOfPinnedObject(), num - num2, canRead, canWrite, handle);
		}
		fixed (T* ptr = &userBuffer[0])
		{
			return new DataStream(num2 + (byte*)(void*)(IntPtr)ptr, num - num2, canRead, canWrite, makeCopy: true);
		}
	}

	public unsafe DataStream(int sizeInBytes, bool canRead, bool canWrite)
	{
		_buffer = (byte*)(void*)Utilities.AllocateMemory(sizeInBytes);
		_size = sizeInBytes;
		_ownsBuffer = true;
		_canRead = canRead;
		_canWrite = canWrite;
	}

	public DataStream(DataPointer dataPointer)
		: this(dataPointer.Pointer, dataPointer.Size, canRead: true, canWrite: true)
	{
	}

	public unsafe DataStream(IntPtr userBuffer, long sizeInBytes, bool canRead, bool canWrite)
	{
		_buffer = (byte*)userBuffer.ToPointer();
		_size = sizeInBytes;
		_canRead = canRead;
		_canWrite = canWrite;
	}

	internal unsafe DataStream(void* dataPointer, int sizeInBytes, bool canRead, bool canWrite, GCHandle handle)
	{
		_gCHandle = handle;
		_buffer = (byte*)dataPointer;
		_size = sizeInBytes;
		_canRead = canRead;
		_canWrite = canWrite;
		_ownsBuffer = false;
	}

	internal unsafe DataStream(void* buffer, int sizeInBytes, bool canRead, bool canWrite, bool makeCopy)
	{
		if (makeCopy)
		{
			_buffer = (byte*)(void*)Utilities.AllocateMemory(sizeInBytes);
			Utilities.CopyMemory((IntPtr)_buffer, (IntPtr)buffer, sizeInBytes);
		}
		else
		{
			_buffer = (byte*)buffer;
		}
		_size = sizeInBytes;
		_canRead = canRead;
		_canWrite = canWrite;
		_ownsBuffer = makeCopy;
	}

	~DataStream()
	{
		Dispose(disposing: false);
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

	public override void Flush()
	{
		throw new NotSupportedException("DataStream objects cannot be flushed.");
	}

	public unsafe T Read<T>() where T : struct
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		byte* ptr = _buffer + _position;
		T data = default(T);
		_position = (byte*)(void*)Utilities.ReadAndPosition((IntPtr)ptr, ref data) - _buffer;
		return data;
	}

	public unsafe override int ReadByte()
	{
		if (_position >= Length)
		{
			return -1;
		}
		return _buffer[_position++];
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int count2 = (int)Math.Min(RemainingLength, count);
		return ReadRange(buffer, offset, count2);
	}

	public unsafe void Read(IntPtr buffer, int offset, int count)
	{
		Utilities.CopyMemory(new IntPtr((byte*)(void*)buffer + offset), (IntPtr)(_buffer + _position), count);
		_position += count;
	}

	public unsafe T[] ReadRange<T>(int count) where T : struct
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		byte* ptr = _buffer + _position;
		T[] array = new T[count];
		_position = (byte*)(void*)Utilities.Read((IntPtr)ptr, array, 0, count) - _buffer;
		return array;
	}

	public unsafe int ReadRange<T>(T[] buffer, int offset, int count) where T : struct
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		long position = _position;
		_position = (byte*)(void*)Utilities.Read((IntPtr)(_buffer + _position), buffer, offset, count) - _buffer;
		return (int)(_position - position);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		long num = 0L;
		switch (origin)
		{
		case SeekOrigin.Begin:
			num = offset;
			break;
		case SeekOrigin.Current:
			num = _position + offset;
			break;
		case SeekOrigin.End:
			num = _size - offset;
			break;
		}
		if (num < 0)
		{
			throw new InvalidOperationException("Cannot seek beyond the beginning of the stream.");
		}
		if (num > _size)
		{
			throw new InvalidOperationException("Cannot seek beyond the end of the stream.");
		}
		_position = num;
		return _position;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException("DataStream objects cannot be resized.");
	}

	public unsafe void Write<T>(T value) where T : struct
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		_position = (byte*)(void*)Utilities.WriteAndPosition((IntPtr)(_buffer + _position), ref value) - _buffer;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		WriteRange(buffer, offset, count);
	}

	public unsafe void Write(IntPtr buffer, int offset, int count)
	{
		Utilities.CopyMemory((IntPtr)(_buffer + _position), new IntPtr((byte*)(void*)buffer + offset), count);
		_position += count;
	}

	public void WriteRange<T>(T[] data) where T : struct
	{
		WriteRange(data, 0, data.Length);
	}

	public unsafe void WriteRange(IntPtr source, long count)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		Utilities.CopyMemory((IntPtr)(_buffer + _position), source, (int)count);
		_position += count;
	}

	public unsafe void WriteRange<T>(T[] data, int offset, int count) where T : struct
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		_position = (byte*)(void*)Utilities.Write((IntPtr)(_buffer + _position), data, offset, count) - _buffer;
	}

	public static implicit operator DataPointer(DataStream from)
	{
		return new DataPointer(from.PositionPointer, (int)from.RemainingLength);
	}
}

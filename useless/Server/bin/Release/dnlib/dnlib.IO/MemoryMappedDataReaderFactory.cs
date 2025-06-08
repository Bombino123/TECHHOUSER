using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace dnlib.IO;

internal sealed class MemoryMappedDataReaderFactory : DataReaderFactory
{
	private enum OSType : byte
	{
		Unknown,
		Windows,
		Unix
	}

	[Serializable]
	private sealed class MemoryMappedIONotSupportedException : IOException
	{
		public MemoryMappedIONotSupportedException(string s)
			: base(s)
		{
		}

		public MemoryMappedIONotSupportedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	private static class Windows
	{
		private const uint GENERIC_READ = 2147483648u;

		private const uint FILE_SHARE_READ = 1u;

		private const uint OPEN_EXISTING = 3u;

		private const uint FILE_ATTRIBUTE_NORMAL = 128u;

		private const uint PAGE_READONLY = 2u;

		private const uint SEC_IMAGE = 16777216u;

		private const uint SECTION_MAP_READ = 4u;

		private const uint FILE_MAP_READ = 4u;

		private const uint INVALID_FILE_SIZE = uint.MaxValue;

		private const int NO_ERROR = 0;

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern SafeFileHandle CreateFileMapping(SafeFileHandle hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr MapViewOfFile(SafeFileHandle hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, UIntPtr dwNumberOfBytesToMap);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern uint GetFileSize(SafeFileHandle hFile, out uint lpFileSizeHigh);

		public unsafe static void Mmap(MemoryMappedDataReaderFactory creator, bool mapAsImage)
		{
			using SafeFileHandle safeFileHandle = CreateFile(creator.filename, 2147483648u, 1u, IntPtr.Zero, 3u, 128u, IntPtr.Zero);
			if (safeFileHandle.IsInvalid)
			{
				throw new IOException($"Could not open file {creator.filename} for reading. Error: {Marshal.GetLastWin32Error():X8}");
			}
			uint lpFileSizeHigh;
			uint fileSize = GetFileSize(safeFileHandle, out lpFileSizeHigh);
			int lastWin32Error;
			if (fileSize == uint.MaxValue && (lastWin32Error = Marshal.GetLastWin32Error()) != 0)
			{
				throw new IOException($"Could not get file size. File: {creator.filename}, error: {lastWin32Error:X8}");
			}
			long num = (long)(((ulong)lpFileSizeHigh << 32) | fileSize);
			using SafeFileHandle safeFileHandle2 = CreateFileMapping(safeFileHandle, IntPtr.Zero, 2u | (mapAsImage ? 16777216u : 0u), 0u, 0u, null);
			if (safeFileHandle2.IsInvalid)
			{
				throw new MemoryMappedIONotSupportedException($"Could not create a file mapping object. File: {creator.filename}, error: {Marshal.GetLastWin32Error():X8}");
			}
			creator.data = MapViewOfFile(safeFileHandle2, 4u, 0u, 0u, UIntPtr.Zero);
			if (creator.data == IntPtr.Zero)
			{
				throw new MemoryMappedIONotSupportedException($"Could not map file {creator.filename}. Error: {Marshal.GetLastWin32Error():X8}");
			}
			creator.length = (uint)num;
			creator.osType = OSType.Windows;
			creator.stream = DataStreamFactory.Create((byte*)(void*)creator.data);
		}

		public static void Dispose(IntPtr addr)
		{
			if (addr != IntPtr.Zero)
			{
				UnmapViewOfFile(addr);
			}
		}
	}

	private static class Unix
	{
		private const int O_RDONLY = 0;

		private const int SEEK_END = 2;

		private const int PROT_READ = 1;

		private const int MAP_PRIVATE = 2;

		[DllImport("libc")]
		private static extern int open(string pathname, int flags);

		[DllImport("libc")]
		private static extern int close(int fd);

		[DllImport("libc", EntryPoint = "lseek", SetLastError = true)]
		private static extern int lseek32(int fd, int offset, int whence);

		[DllImport("libc", EntryPoint = "lseek", SetLastError = true)]
		private static extern long lseek64(int fd, long offset, int whence);

		[DllImport("libc", EntryPoint = "mmap", SetLastError = true)]
		private static extern IntPtr mmap32(IntPtr addr, IntPtr length, int prot, int flags, int fd, int offset);

		[DllImport("libc", EntryPoint = "mmap", SetLastError = true)]
		private static extern IntPtr mmap64(IntPtr addr, IntPtr length, int prot, int flags, int fd, long offset);

		[DllImport("libc")]
		private static extern int munmap(IntPtr addr, IntPtr length);

		public unsafe static void Mmap(MemoryMappedDataReaderFactory creator, bool mapAsImage)
		{
			int num = open(creator.filename, 0);
			try
			{
				if (num < 0)
				{
					throw new IOException($"Could not open file {creator.filename} for reading. Error: {num}");
				}
				long num2;
				IntPtr intPtr;
				if (IntPtr.Size == 4)
				{
					num2 = lseek32(num, 0, 2);
					if (num2 == -1)
					{
						throw new MemoryMappedIONotSupportedException($"Could not get length of {creator.filename} (lseek failed): {Marshal.GetLastWin32Error()}");
					}
					intPtr = mmap32(IntPtr.Zero, (IntPtr)num2, 1, 2, num, 0);
					if (intPtr == new IntPtr(-1) || intPtr == IntPtr.Zero)
					{
						throw new MemoryMappedIONotSupportedException($"Could not map file {creator.filename}. Error: {Marshal.GetLastWin32Error()}");
					}
				}
				else
				{
					num2 = lseek64(num, 0L, 2);
					if (num2 == -1)
					{
						throw new MemoryMappedIONotSupportedException($"Could not get length of {creator.filename} (lseek failed): {Marshal.GetLastWin32Error()}");
					}
					intPtr = mmap64(IntPtr.Zero, (IntPtr)num2, 1, 2, num, 0L);
					if (intPtr == new IntPtr(-1) || intPtr == IntPtr.Zero)
					{
						throw new MemoryMappedIONotSupportedException($"Could not map file {creator.filename}. Error: {Marshal.GetLastWin32Error()}");
					}
				}
				creator.data = intPtr;
				creator.length = (uint)num2;
				creator.origDataLength = num2;
				creator.osType = OSType.Unix;
				creator.stream = DataStreamFactory.Create((byte*)(void*)creator.data);
			}
			finally
			{
				if (num >= 0)
				{
					close(num);
				}
			}
		}

		public static void Dispose(IntPtr addr, long size)
		{
			if (addr != IntPtr.Zero)
			{
				munmap(addr, new IntPtr(size));
			}
		}
	}

	private DataStream stream;

	private uint length;

	private string filename;

	private GCHandle gcHandle;

	private byte[] dataAry;

	private IntPtr data;

	private OSType osType;

	private long origDataLength;

	private static volatile bool canTryWindows = true;

	private static volatile bool canTryUnix = true;

	public override string Filename => filename;

	public override uint Length => length;

	internal bool IsMemoryMappedIO => dataAry == null;

	public override event EventHandler DataReaderInvalidated;

	private MemoryMappedDataReaderFactory(string filename)
	{
		osType = OSType.Unknown;
		this.filename = filename;
	}

	~MemoryMappedDataReaderFactory()
	{
		Dispose(disposing: false);
	}

	public override DataReader CreateReader(uint offset, uint length)
	{
		return CreateReader(stream, offset, length);
	}

	public override void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal void SetLength(uint length)
	{
		this.length = length;
	}

	internal static MemoryMappedDataReaderFactory CreateWindows(string filename, bool mapAsImage)
	{
		if (!canTryWindows)
		{
			return null;
		}
		MemoryMappedDataReaderFactory memoryMappedDataReaderFactory = new MemoryMappedDataReaderFactory(GetFullPath(filename));
		try
		{
			Windows.Mmap(memoryMappedDataReaderFactory, mapAsImage);
			return memoryMappedDataReaderFactory;
		}
		catch (EntryPointNotFoundException)
		{
		}
		catch (DllNotFoundException)
		{
		}
		canTryWindows = false;
		return null;
	}

	internal static MemoryMappedDataReaderFactory CreateUnix(string filename, bool mapAsImage)
	{
		if (!canTryUnix)
		{
			return null;
		}
		MemoryMappedDataReaderFactory memoryMappedDataReaderFactory = new MemoryMappedDataReaderFactory(GetFullPath(filename));
		try
		{
			Unix.Mmap(memoryMappedDataReaderFactory, mapAsImage);
			if (mapAsImage)
			{
				memoryMappedDataReaderFactory.Dispose();
				throw new ArgumentException("mapAsImage == true is not supported on this OS");
			}
			return memoryMappedDataReaderFactory;
		}
		catch (MemoryMappedIONotSupportedException)
		{
		}
		catch (EntryPointNotFoundException)
		{
		}
		catch (DllNotFoundException)
		{
		}
		canTryUnix = false;
		return null;
	}

	private static string GetFullPath(string filename)
	{
		try
		{
			return Path.GetFullPath(filename);
		}
		catch
		{
			return filename;
		}
	}

	private void Dispose(bool disposing)
	{
		FreeMemoryMappedIoData();
		if (disposing)
		{
			length = 0u;
			stream = EmptyDataStream.Instance;
			data = IntPtr.Zero;
			filename = null;
		}
	}

	internal unsafe void UnsafeDisableMemoryMappedIO()
	{
		if (dataAry == null)
		{
			byte[] array = new byte[length];
			Marshal.Copy(data, array, 0, array.Length);
			FreeMemoryMappedIoData();
			length = (uint)array.Length;
			dataAry = array;
			gcHandle = GCHandle.Alloc(dataAry, GCHandleType.Pinned);
			data = gcHandle.AddrOfPinnedObject();
			stream = DataStreamFactory.Create((byte*)(void*)data);
			DataReaderInvalidated?.Invoke(this, EventArgs.Empty);
		}
	}

	private void FreeMemoryMappedIoData()
	{
		if (dataAry == null)
		{
			IntPtr intPtr = Interlocked.Exchange(ref data, IntPtr.Zero);
			if (intPtr != IntPtr.Zero)
			{
				length = 0u;
				switch (osType)
				{
				case OSType.Windows:
					Windows.Dispose(intPtr);
					break;
				case OSType.Unix:
					Unix.Dispose(intPtr, origDataLength);
					break;
				default:
					throw new InvalidOperationException("Shouldn't be here");
				}
			}
		}
		if (gcHandle.IsAllocated)
		{
			try
			{
				gcHandle.Free();
			}
			catch (InvalidOperationException)
			{
			}
		}
		dataAry = null;
	}
}

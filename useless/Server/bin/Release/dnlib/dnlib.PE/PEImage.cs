using System;
using System.Collections.Generic;
using System.IO;
using dnlib.IO;
using dnlib.Threading;
using dnlib.Utils;
using dnlib.W32Resources;

namespace dnlib.PE;

public sealed class PEImage : IInternalPEImage, IPEImage, IRvaFileOffsetConverter, IDisposable
{
	private sealed class FilePEType : IPEType
	{
		public RVA ToRVA(PEInfo peInfo, FileOffset offset)
		{
			return peInfo.ToRVA(offset);
		}

		public FileOffset ToFileOffset(PEInfo peInfo, RVA rva)
		{
			return peInfo.ToFileOffset(rva);
		}
	}

	private sealed class MemoryPEType : IPEType
	{
		public RVA ToRVA(PEInfo peInfo, FileOffset offset)
		{
			return (RVA)offset;
		}

		public FileOffset ToFileOffset(PEInfo peInfo, RVA rva)
		{
			return (FileOffset)rva;
		}
	}

	private const bool USE_MEMORY_LAYOUT_WITH_MAPPED_FILES = false;

	private static readonly IPEType MemoryLayout = new MemoryPEType();

	private static readonly IPEType FileLayout = new FilePEType();

	private DataReaderFactory dataReaderFactory;

	private IPEType peType;

	private PEInfo peInfo;

	private UserValue<Win32Resources> win32Resources;

	private readonly Lock theLock = Lock.Create();

	private ImageDebugDirectory[] imageDebugDirectories;

	public bool IsFileImageLayout => peType is FilePEType;

	public bool MayHaveInvalidAddresses => !IsFileImageLayout;

	public string Filename => dataReaderFactory.Filename;

	public ImageDosHeader ImageDosHeader => peInfo.ImageDosHeader;

	public ImageNTHeaders ImageNTHeaders => peInfo.ImageNTHeaders;

	public IList<ImageSectionHeader> ImageSectionHeaders => peInfo.ImageSectionHeaders;

	public IList<ImageDebugDirectory> ImageDebugDirectories
	{
		get
		{
			if (imageDebugDirectories == null)
			{
				imageDebugDirectories = ReadImageDebugDirectories();
			}
			return imageDebugDirectories;
		}
	}

	public DataReaderFactory DataReaderFactory => dataReaderFactory;

	public Win32Resources Win32Resources
	{
		get
		{
			return win32Resources.Value;
		}
		set
		{
			IDisposable disposable = null;
			if (win32Resources.IsValueInitialized)
			{
				disposable = win32Resources.Value;
				if (disposable == value)
				{
					return;
				}
			}
			win32Resources.Value = value;
			disposable?.Dispose();
		}
	}

	bool IInternalPEImage.IsMemoryMappedIO
	{
		get
		{
			if (dataReaderFactory is MemoryMappedDataReaderFactory memoryMappedDataReaderFactory)
			{
				return memoryMappedDataReaderFactory.IsMemoryMappedIO;
			}
			return false;
		}
	}

	public PEImage(DataReaderFactory dataReaderFactory, ImageLayout imageLayout, bool verify)
	{
		try
		{
			this.dataReaderFactory = dataReaderFactory;
			peType = ConvertImageLayout(imageLayout);
			DataReader reader = dataReaderFactory.CreateReader();
			peInfo = new PEInfo(ref reader, verify);
			Initialize();
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private void Initialize()
	{
		win32Resources.ReadOriginalValue = delegate
		{
			ImageDataDirectory imageDataDirectory = peInfo.ImageNTHeaders.OptionalHeader.DataDirectories[2];
			return (imageDataDirectory.VirtualAddress == (RVA)0u || imageDataDirectory.Size == 0) ? null : new Win32ResourcesPE(this);
		};
		win32Resources.Lock = theLock;
	}

	private static IPEType ConvertImageLayout(ImageLayout imageLayout)
	{
		return imageLayout switch
		{
			ImageLayout.File => FileLayout, 
			ImageLayout.Memory => MemoryLayout, 
			_ => throw new ArgumentException("imageLayout"), 
		};
	}

	internal PEImage(string filename, bool mapAsImage, bool verify)
		: this(DataReaderFactoryFactory.Create(filename, mapAsImage), mapAsImage ? ImageLayout.Memory : ImageLayout.File, verify)
	{
		try
		{
			if (mapAsImage && dataReaderFactory is MemoryMappedDataReaderFactory)
			{
				((MemoryMappedDataReaderFactory)dataReaderFactory).SetLength(peInfo.GetImageSize());
			}
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	public PEImage(string filename, bool verify)
		: this(filename, mapAsImage: false, verify)
	{
	}

	public PEImage(string filename)
		: this(filename, verify: true)
	{
	}

	public PEImage(byte[] data, string filename, ImageLayout imageLayout, bool verify)
		: this(ByteArrayDataReaderFactory.Create(data, filename), imageLayout, verify)
	{
	}

	public PEImage(byte[] data, ImageLayout imageLayout, bool verify)
		: this(data, null, imageLayout, verify)
	{
	}

	public PEImage(byte[] data, bool verify)
		: this(data, null, ImageLayout.File, verify)
	{
	}

	public PEImage(byte[] data, string filename, bool verify)
		: this(data, filename, ImageLayout.File, verify)
	{
	}

	public PEImage(byte[] data)
		: this(data, null, verify: true)
	{
	}

	public PEImage(byte[] data, string filename)
		: this(data, filename, verify: true)
	{
	}

	public unsafe PEImage(IntPtr baseAddr, uint length, ImageLayout imageLayout, bool verify)
		: this(NativeMemoryDataReaderFactory.Create((byte*)(void*)baseAddr, length, null), imageLayout, verify)
	{
	}

	public PEImage(IntPtr baseAddr, uint length, bool verify)
		: this(baseAddr, length, ImageLayout.Memory, verify)
	{
	}

	public PEImage(IntPtr baseAddr, uint length)
		: this(baseAddr, length, verify: true)
	{
	}

	public unsafe PEImage(IntPtr baseAddr, ImageLayout imageLayout, bool verify)
		: this(NativeMemoryDataReaderFactory.Create((byte*)(void*)baseAddr, 65536u, null), imageLayout, verify)
	{
		try
		{
			((NativeMemoryDataReaderFactory)dataReaderFactory).SetLength(peInfo.GetImageSize());
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	public PEImage(IntPtr baseAddr, bool verify)
		: this(baseAddr, ImageLayout.Memory, verify)
	{
	}

	public PEImage(IntPtr baseAddr)
		: this(baseAddr, verify: true)
	{
	}

	public RVA ToRVA(FileOffset offset)
	{
		return peType.ToRVA(peInfo, offset);
	}

	public FileOffset ToFileOffset(RVA rva)
	{
		return peType.ToFileOffset(peInfo, rva);
	}

	public void Dispose()
	{
		IDisposable value;
		if (win32Resources.IsValueInitialized && (value = win32Resources.Value) != null)
		{
			value.Dispose();
		}
		dataReaderFactory?.Dispose();
		win32Resources.Value = null;
		dataReaderFactory = null;
		peType = null;
		peInfo = null;
	}

	public DataReader CreateReader(FileOffset offset)
	{
		return DataReaderFactory.CreateReader((uint)offset, (uint)(DataReaderFactory.Length - offset));
	}

	public DataReader CreateReader(FileOffset offset, uint length)
	{
		return DataReaderFactory.CreateReader((uint)offset, length);
	}

	public DataReader CreateReader(RVA rva)
	{
		return CreateReader(ToFileOffset(rva));
	}

	public DataReader CreateReader(RVA rva, uint length)
	{
		return CreateReader(ToFileOffset(rva), length);
	}

	public DataReader CreateReader()
	{
		return DataReaderFactory.CreateReader();
	}

	void IInternalPEImage.UnsafeDisableMemoryMappedIO()
	{
		if (dataReaderFactory is MemoryMappedDataReaderFactory memoryMappedDataReaderFactory)
		{
			memoryMappedDataReaderFactory.UnsafeDisableMemoryMappedIO();
		}
	}

	private ImageDebugDirectory[] ReadImageDebugDirectories()
	{
		try
		{
			ImageDataDirectory imageDataDirectory = ImageNTHeaders.OptionalHeader.DataDirectories[6];
			if (imageDataDirectory.VirtualAddress == (RVA)0u)
			{
				return Array2.Empty<ImageDebugDirectory>();
			}
			DataReader reader = DataReaderFactory.CreateReader();
			if (imageDataDirectory.Size > reader.Length)
			{
				return Array2.Empty<ImageDebugDirectory>();
			}
			int num = (int)(imageDataDirectory.Size / 28);
			if (num == 0)
			{
				return Array2.Empty<ImageDebugDirectory>();
			}
			reader.CurrentOffset = (uint)ToFileOffset(imageDataDirectory.VirtualAddress);
			if ((ulong)((long)reader.CurrentOffset + (long)imageDataDirectory.Size) > (ulong)reader.Length)
			{
				return Array2.Empty<ImageDebugDirectory>();
			}
			ImageDebugDirectory[] array = new ImageDebugDirectory[num];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ImageDebugDirectory(ref reader, verify: true);
			}
			return array;
		}
		catch (IOException)
		{
		}
		return Array2.Empty<ImageDebugDirectory>();
	}
}

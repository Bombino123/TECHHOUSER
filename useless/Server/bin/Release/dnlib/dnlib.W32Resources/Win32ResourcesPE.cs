using System;
using dnlib.IO;
using dnlib.PE;
using dnlib.Threading;
using dnlib.Utils;

namespace dnlib.W32Resources;

public sealed class Win32ResourcesPE : Win32Resources
{
	private readonly IRvaFileOffsetConverter rvaConverter;

	private DataReaderFactory dataReader_factory;

	private uint dataReader_offset;

	private uint dataReader_length;

	private bool owns_dataReader_factory;

	private DataReaderFactory rsrcReader_factory;

	private uint rsrcReader_offset;

	private uint rsrcReader_length;

	private bool owns_rsrcReader_factory;

	private UserValue<ResourceDirectory> root;

	private readonly Lock theLock = Lock.Create();

	public override ResourceDirectory Root
	{
		get
		{
			return root.Value;
		}
		set
		{
			if (!root.IsValueInitialized || root.Value != value)
			{
				root.Value = value;
			}
		}
	}

	internal DataReader GetResourceReader()
	{
		return rsrcReader_factory.CreateReader(rsrcReader_offset, rsrcReader_length);
	}

	public Win32ResourcesPE(IRvaFileOffsetConverter rvaConverter, DataReaderFactory rsrcReader_factory, uint rsrcReader_offset, uint rsrcReader_length, bool owns_rsrcReader_factory, DataReaderFactory dataReader_factory, uint dataReader_offset, uint dataReader_length, bool owns_dataReader_factory)
	{
		this.rvaConverter = rvaConverter ?? throw new ArgumentNullException("rvaConverter");
		this.rsrcReader_factory = rsrcReader_factory ?? throw new ArgumentNullException("rsrcReader_factory");
		this.rsrcReader_offset = rsrcReader_offset;
		this.rsrcReader_length = rsrcReader_length;
		this.owns_rsrcReader_factory = owns_rsrcReader_factory;
		this.dataReader_factory = dataReader_factory ?? throw new ArgumentNullException("dataReader_factory");
		this.dataReader_offset = dataReader_offset;
		this.dataReader_length = dataReader_length;
		this.owns_dataReader_factory = owns_dataReader_factory;
		Initialize();
	}

	public Win32ResourcesPE(IPEImage peImage)
		: this(peImage, null, 0u, 0u, owns_rsrcReader_factory: false)
	{
	}

	public Win32ResourcesPE(IPEImage peImage, DataReaderFactory rsrcReader_factory, uint rsrcReader_offset, uint rsrcReader_length, bool owns_rsrcReader_factory)
	{
		rvaConverter = peImage ?? throw new ArgumentNullException("peImage");
		dataReader_factory = peImage.DataReaderFactory;
		dataReader_offset = 0u;
		dataReader_length = dataReader_factory.Length;
		if (rsrcReader_factory != null)
		{
			this.rsrcReader_factory = rsrcReader_factory;
			this.rsrcReader_offset = rsrcReader_offset;
			this.rsrcReader_length = rsrcReader_length;
			this.owns_rsrcReader_factory = owns_rsrcReader_factory;
		}
		else
		{
			ImageDataDirectory imageDataDirectory = peImage.ImageNTHeaders.OptionalHeader.DataDirectories[2];
			if (imageDataDirectory.VirtualAddress != 0 && imageDataDirectory.Size != 0)
			{
				DataReader dataReader = peImage.CreateReader(imageDataDirectory.VirtualAddress, imageDataDirectory.Size);
				this.rsrcReader_factory = peImage.DataReaderFactory;
				this.rsrcReader_offset = dataReader.StartOffset;
				this.rsrcReader_length = dataReader.Length;
			}
			else
			{
				this.rsrcReader_factory = ByteArrayDataReaderFactory.Create(Array2.Empty<byte>(), null);
				this.rsrcReader_offset = 0u;
				this.rsrcReader_length = 0u;
			}
		}
		Initialize();
	}

	private void Initialize()
	{
		root.ReadOriginalValue = delegate
		{
			DataReaderFactory dataReaderFactory = rsrcReader_factory;
			if (dataReaderFactory == null)
			{
				return (ResourceDirectory)null;
			}
			DataReader reader = dataReaderFactory.CreateReader(rsrcReader_offset, rsrcReader_length);
			return new ResourceDirectoryPE(0u, new ResourceName("root"), this, ref reader);
		};
		root.Lock = theLock;
	}

	public DataReader CreateReader(RVA rva, uint size)
	{
		GetDataReaderInfo(rva, size, out var dataReaderFactory, out var dataOffset, out var dataLength);
		return dataReaderFactory.CreateReader(dataOffset, dataLength);
	}

	internal void GetDataReaderInfo(RVA rva, uint size, out DataReaderFactory dataReaderFactory, out uint dataOffset, out uint dataLength)
	{
		dataOffset = (uint)rvaConverter.ToFileOffset(rva);
		if ((ulong)((long)dataOffset + (long)size) <= (ulong)dataReader_factory.Length)
		{
			dataReaderFactory = dataReader_factory;
			dataLength = size;
		}
		else
		{
			dataReaderFactory = ByteArrayDataReaderFactory.Create(Array2.Empty<byte>(), null);
			dataOffset = 0u;
			dataLength = 0u;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (owns_dataReader_factory)
			{
				dataReader_factory?.Dispose();
			}
			if (owns_rsrcReader_factory)
			{
				rsrcReader_factory?.Dispose();
			}
			dataReader_factory = null;
			rsrcReader_factory = null;
			base.Dispose(disposing);
		}
	}
}

using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class PEHeaders : IChunk
{
	private IList<PESection> sections;

	private readonly PEHeadersOptions options;

	private FileOffset offset;

	private RVA rva;

	private uint length;

	private readonly uint sectionAlignment;

	private readonly uint fileAlignment;

	private ulong imageBase;

	private long startOffset;

	private long checkSumOffset;

	private bool isExeFile;

	private static readonly byte[] dosHeader = new byte[128]
	{
		77, 90, 144, 0, 3, 0, 0, 0, 4, 0,
		0, 0, 255, 255, 0, 0, 184, 0, 0, 0,
		0, 0, 0, 0, 64, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		128, 0, 0, 0, 14, 31, 186, 14, 0, 180,
		9, 205, 33, 184, 1, 76, 205, 33, 84, 104,
		105, 115, 32, 112, 114, 111, 103, 114, 97, 109,
		32, 99, 97, 110, 110, 111, 116, 32, 98, 101,
		32, 114, 117, 110, 32, 105, 110, 32, 68, 79,
		83, 32, 109, 111, 100, 101, 46, 13, 13, 10,
		36, 0, 0, 0, 0, 0, 0, 0
	};

	public StartupStub StartupStub { get; set; }

	public ImageCor20Header ImageCor20Header { get; set; }

	public ImportAddressTable ImportAddressTable { get; set; }

	public ImportDirectory ImportDirectory { get; set; }

	public Win32ResourcesChunk Win32Resources { get; set; }

	public RelocDirectory RelocDirectory { get; set; }

	public DebugDirectory DebugDirectory { get; set; }

	internal IChunk ExportDirectory { get; set; }

	public ulong ImageBase => imageBase;

	public bool IsExeFile
	{
		get
		{
			return isExeFile;
		}
		set
		{
			isExeFile = value;
		}
	}

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public uint SectionAlignment => sectionAlignment;

	public uint FileAlignment => fileAlignment;

	public IList<PESection> PESections
	{
		get
		{
			return sections;
		}
		set
		{
			sections = value;
		}
	}

	private int SectionsCount
	{
		get
		{
			int num = 0;
			foreach (PESection section in sections)
			{
				if (section.GetVirtualSize() != 0)
				{
					num++;
				}
			}
			return num;
		}
	}

	public PEHeaders()
		: this(new PEHeadersOptions())
	{
	}

	public PEHeaders(PEHeadersOptions options)
	{
		this.options = options ?? new PEHeadersOptions();
		sectionAlignment = this.options.SectionAlignment ?? 8192;
		fileAlignment = this.options.FileAlignment ?? 512;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
		length = (uint)dosHeader.Length;
		length += 24u;
		length += (uint)(Use32BitOptionalHeader() ? 224 : 240);
		length += (uint)(sections.Count * 40);
		if (Use32BitOptionalHeader())
		{
			imageBase = (ulong)(((long?)options.ImageBase) ?? ((long)(IsExeFile ? 4194304 : 268435456)));
		}
		else
		{
			imageBase = (ulong)(((long?)options.ImageBase) ?? (IsExeFile ? 5368709120L : 6442450944L));
		}
	}

	public uint GetFileLength()
	{
		return length;
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	private IEnumerable<SectionSizeInfo> GetSectionSizeInfos()
	{
		foreach (PESection section in sections)
		{
			uint virtualSize = section.GetVirtualSize();
			if (virtualSize != 0)
			{
				yield return new SectionSizeInfo(virtualSize, section.Characteristics);
			}
		}
	}

	public void WriteTo(DataWriter writer)
	{
		startOffset = writer.Position;
		writer.WriteBytes(dosHeader);
		writer.WriteInt32(17744);
		writer.WriteUInt16((ushort)GetMachine());
		writer.WriteUInt16((ushort)SectionsCount);
		writer.WriteUInt32(options.TimeDateStamp ?? PEHeadersOptions.CreateNewTimeDateStamp());
		writer.WriteUInt32(options.PointerToSymbolTable.GetValueOrDefault());
		writer.WriteUInt32(options.NumberOfSymbols.GetValueOrDefault());
		writer.WriteUInt16((ushort)(Use32BitOptionalHeader() ? 224u : 240u));
		writer.WriteUInt16((ushort)GetCharacteristics());
		SectionSizes sectionSizes = new SectionSizes(fileAlignment, sectionAlignment, length, () => GetSectionSizeInfos());
		uint value = (uint)((StartupStub != null && StartupStub.Enable) ? StartupStub.EntryPointRVA : ((RVA)0u));
		if (Use32BitOptionalHeader())
		{
			writer.WriteUInt16(267);
			writer.WriteByte(options.MajorLinkerVersion ?? 11);
			writer.WriteByte(options.MinorLinkerVersion.GetValueOrDefault());
			writer.WriteUInt32(sectionSizes.SizeOfCode);
			writer.WriteUInt32(sectionSizes.SizeOfInitdData);
			writer.WriteUInt32(sectionSizes.SizeOfUninitdData);
			writer.WriteUInt32(value);
			writer.WriteUInt32(sectionSizes.BaseOfCode);
			writer.WriteUInt32(sectionSizes.BaseOfData);
			writer.WriteUInt32((uint)imageBase);
			writer.WriteUInt32(sectionAlignment);
			writer.WriteUInt32(fileAlignment);
			writer.WriteUInt16(options.MajorOperatingSystemVersion ?? 4);
			writer.WriteUInt16(options.MinorOperatingSystemVersion.GetValueOrDefault());
			writer.WriteUInt16(options.MajorImageVersion.GetValueOrDefault());
			writer.WriteUInt16(options.MinorImageVersion.GetValueOrDefault());
			writer.WriteUInt16(options.MajorSubsystemVersion ?? 4);
			writer.WriteUInt16(options.MinorSubsystemVersion.GetValueOrDefault());
			writer.WriteUInt32(options.Win32VersionValue.GetValueOrDefault());
			writer.WriteUInt32(sectionSizes.SizeOfImage);
			writer.WriteUInt32(sectionSizes.SizeOfHeaders);
			checkSumOffset = writer.Position;
			writer.WriteInt32(0);
			writer.WriteUInt16((ushort)(options.Subsystem ?? Subsystem.WindowsGui));
			writer.WriteUInt16((ushort)(options.DllCharacteristics ?? (DllCharacteristics.DynamicBase | DllCharacteristics.NxCompat | DllCharacteristics.NoSeh | DllCharacteristics.TerminalServerAware)));
			writer.WriteUInt32((uint)(options.SizeOfStackReserve ?? 1048576));
			writer.WriteUInt32((uint)(options.SizeOfStackCommit ?? 4096));
			writer.WriteUInt32((uint)(options.SizeOfHeapReserve ?? 1048576));
			writer.WriteUInt32((uint)(options.SizeOfHeapCommit ?? 4096));
			writer.WriteUInt32(options.LoaderFlags.GetValueOrDefault());
			writer.WriteUInt32(options.NumberOfRvaAndSizes ?? 16);
		}
		else
		{
			writer.WriteUInt16(523);
			writer.WriteByte(options.MajorLinkerVersion ?? 11);
			writer.WriteByte(options.MinorLinkerVersion.GetValueOrDefault());
			writer.WriteUInt32(sectionSizes.SizeOfCode);
			writer.WriteUInt32(sectionSizes.SizeOfInitdData);
			writer.WriteUInt32(sectionSizes.SizeOfUninitdData);
			writer.WriteUInt32(value);
			writer.WriteUInt32(sectionSizes.BaseOfCode);
			writer.WriteUInt64(imageBase);
			writer.WriteUInt32(sectionAlignment);
			writer.WriteUInt32(fileAlignment);
			writer.WriteUInt16(options.MajorOperatingSystemVersion ?? 4);
			writer.WriteUInt16(options.MinorOperatingSystemVersion.GetValueOrDefault());
			writer.WriteUInt16(options.MajorImageVersion.GetValueOrDefault());
			writer.WriteUInt16(options.MinorImageVersion.GetValueOrDefault());
			writer.WriteUInt16(options.MajorSubsystemVersion ?? 4);
			writer.WriteUInt16(options.MinorSubsystemVersion.GetValueOrDefault());
			writer.WriteUInt32(options.Win32VersionValue.GetValueOrDefault());
			writer.WriteUInt32(sectionSizes.SizeOfImage);
			writer.WriteUInt32(sectionSizes.SizeOfHeaders);
			checkSumOffset = writer.Position;
			writer.WriteInt32(0);
			writer.WriteUInt16((ushort)(options.Subsystem ?? Subsystem.WindowsGui));
			writer.WriteUInt16((ushort)(options.DllCharacteristics ?? (DllCharacteristics.DynamicBase | DllCharacteristics.NxCompat | DllCharacteristics.NoSeh | DllCharacteristics.TerminalServerAware)));
			writer.WriteUInt64(options.SizeOfStackReserve ?? 4194304);
			writer.WriteUInt64(options.SizeOfStackCommit ?? 16384);
			writer.WriteUInt64(options.SizeOfHeapReserve ?? 1048576);
			writer.WriteUInt64(options.SizeOfHeapCommit ?? 8192);
			writer.WriteUInt32(options.LoaderFlags.GetValueOrDefault());
			writer.WriteUInt32(options.NumberOfRvaAndSizes ?? 16);
		}
		writer.WriteDataDirectory(ExportDirectory);
		writer.WriteDataDirectory(ImportDirectory);
		writer.WriteDataDirectory(Win32Resources);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(RelocDirectory);
		writer.WriteDebugDirectory(DebugDirectory);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(ImportAddressTable);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(ImageCor20Header);
		writer.WriteDataDirectory(null);
		uint num = Utils.AlignUp(sectionSizes.SizeOfHeaders, sectionAlignment);
		int num2 = 0;
		foreach (PESection section in sections)
		{
			if (section.GetVirtualSize() != 0)
			{
				num += section.WriteHeaderTo(writer, fileAlignment, sectionAlignment, num);
			}
			else
			{
				num2++;
			}
		}
		if (num2 != 0)
		{
			writer.Position += num2 * 40;
		}
	}

	public void WriteCheckSum(DataWriter writer, long length)
	{
		writer.Position = startOffset;
		uint value = writer.InternalStream.CalculatePECheckSum(length, checkSumOffset);
		writer.Position = checkSumOffset;
		writer.WriteUInt32(value);
	}

	private Machine GetMachine()
	{
		return options.Machine ?? Machine.I386;
	}

	private bool Use32BitOptionalHeader()
	{
		return !GetMachine().Is64Bit();
	}

	private Characteristics GetCharacteristics()
	{
		Characteristics characteristics = options.Characteristics ?? GetDefaultCharacteristics();
		if (IsExeFile)
		{
			return characteristics & ~Characteristics.Dll;
		}
		return characteristics | Characteristics.Dll;
	}

	private Characteristics GetDefaultCharacteristics()
	{
		if (Use32BitOptionalHeader())
		{
			return Characteristics.ExecutableImage | Characteristics.Bit32Machine;
		}
		return Characteristics.ExecutableImage | Characteristics.LargeAddressAware;
	}
}

using System;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class PEHeadersOptions
{
	public const DllCharacteristics DefaultDllCharacteristics = dnlib.PE.DllCharacteristics.DynamicBase | dnlib.PE.DllCharacteristics.NxCompat | dnlib.PE.DllCharacteristics.NoSeh | dnlib.PE.DllCharacteristics.TerminalServerAware;

	public const Subsystem DEFAULT_SUBSYSTEM = dnlib.PE.Subsystem.WindowsGui;

	public const byte DEFAULT_MAJOR_LINKER_VERSION = 11;

	public const byte DEFAULT_MINOR_LINKER_VERSION = 0;

	public Machine? Machine;

	public uint? TimeDateStamp;

	public uint? PointerToSymbolTable;

	public uint? NumberOfSymbols;

	public Characteristics? Characteristics;

	public byte? MajorLinkerVersion;

	public byte? MinorLinkerVersion;

	public ulong? ImageBase;

	public uint? SectionAlignment;

	public uint? FileAlignment;

	public ushort? MajorOperatingSystemVersion;

	public ushort? MinorOperatingSystemVersion;

	public ushort? MajorImageVersion;

	public ushort? MinorImageVersion;

	public ushort? MajorSubsystemVersion;

	public ushort? MinorSubsystemVersion;

	public uint? Win32VersionValue;

	public Subsystem? Subsystem;

	public DllCharacteristics? DllCharacteristics;

	public ulong? SizeOfStackReserve;

	public ulong? SizeOfStackCommit;

	public ulong? SizeOfHeapReserve;

	public ulong? SizeOfHeapCommit;

	public uint? LoaderFlags;

	public uint? NumberOfRvaAndSizes;

	private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static uint CreateNewTimeDateStamp()
	{
		return (uint)(DateTime.UtcNow - Epoch).TotalSeconds;
	}
}

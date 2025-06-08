using dnlib.IO;

namespace dnlib.PE;

public interface IImageOptionalHeader : IFileSection
{
	ushort Magic { get; }

	byte MajorLinkerVersion { get; }

	byte MinorLinkerVersion { get; }

	uint SizeOfCode { get; }

	uint SizeOfInitializedData { get; }

	uint SizeOfUninitializedData { get; }

	RVA AddressOfEntryPoint { get; }

	RVA BaseOfCode { get; }

	RVA BaseOfData { get; }

	ulong ImageBase { get; }

	uint SectionAlignment { get; }

	uint FileAlignment { get; }

	ushort MajorOperatingSystemVersion { get; }

	ushort MinorOperatingSystemVersion { get; }

	ushort MajorImageVersion { get; }

	ushort MinorImageVersion { get; }

	ushort MajorSubsystemVersion { get; }

	ushort MinorSubsystemVersion { get; }

	uint Win32VersionValue { get; }

	uint SizeOfImage { get; }

	uint SizeOfHeaders { get; }

	uint CheckSum { get; }

	Subsystem Subsystem { get; }

	DllCharacteristics DllCharacteristics { get; }

	ulong SizeOfStackReserve { get; }

	ulong SizeOfStackCommit { get; }

	ulong SizeOfHeapReserve { get; }

	ulong SizeOfHeapCommit { get; }

	uint LoaderFlags { get; }

	uint NumberOfRvaAndSizes { get; }

	ImageDataDirectory[] DataDirectories { get; }
}

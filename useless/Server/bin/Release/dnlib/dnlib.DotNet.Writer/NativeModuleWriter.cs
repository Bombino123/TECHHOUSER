using System;
using System.Collections.Generic;
using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.PE;
using dnlib.W32Resources;

namespace dnlib.DotNet.Writer;

public sealed class NativeModuleWriter : ModuleWriterBase
{
	private readonly struct ReusedChunkInfo
	{
		public IReuseChunk Chunk { get; }

		public RVA RVA { get; }

		public ReusedChunkInfo(IReuseChunk chunk, RVA rva)
		{
			Chunk = chunk;
			RVA = rva;
		}
	}

	public sealed class OrigSection : IDisposable
	{
		public ImageSectionHeader PESection;

		public DataReaderChunk Chunk;

		public OrigSection(ImageSectionHeader peSection)
		{
			PESection = peSection;
		}

		public void Dispose()
		{
			Chunk = null;
			PESection = null;
		}

		public override string ToString()
		{
			uint startOffset = Chunk.CreateReader().StartOffset;
			return $"{PESection.DisplayName} FO:{startOffset:X8} L:{Chunk.CreateReader().Length:X8}";
		}
	}

	private readonly ModuleDefMD module;

	private NativeModuleWriterOptions options;

	private DataReaderChunk extraData;

	private List<OrigSection> origSections;

	private List<ReusedChunkInfo> reusedChunks;

	private readonly IPEImage peImage;

	private List<PESection> sections;

	private PESection textSection;

	private ByteArrayChunk imageCor20Header;

	private PESection rsrcSection;

	private long checkSumOffset;

	public ModuleDefMD ModuleDefMD => module;

	public override ModuleDef Module => module;

	public override ModuleWriterOptionsBase TheOptions => Options;

	public NativeModuleWriterOptions Options
	{
		get
		{
			return options ?? (options = new NativeModuleWriterOptions(module, optimizeImageSize: true));
		}
		set
		{
			options = value;
		}
	}

	public override List<PESection> Sections => sections;

	public List<OrigSection> OrigSections => origSections;

	public override PESection TextSection => textSection;

	public override PESection RsrcSection => rsrcSection;

	public NativeModuleWriter(ModuleDefMD module, NativeModuleWriterOptions options)
	{
		this.module = module;
		this.options = options;
		peImage = module.Metadata.PEImage;
		reusedChunks = new List<ReusedChunkInfo>();
	}

	protected override long WriteImpl()
	{
		try
		{
			return Write();
		}
		finally
		{
			if (origSections != null)
			{
				foreach (OrigSection origSection in origSections)
				{
					origSection.Dispose();
				}
			}
		}
	}

	private long Write()
	{
		Initialize();
		metadata.KeepFieldRVA = true;
		metadata.CreateTables();
		return WriteFile();
	}

	private void Initialize()
	{
		CreateSections();
		OnWriterEvent(ModuleWriterEvent.PESectionsCreated);
		CreateChunks();
		OnWriterEvent(ModuleWriterEvent.ChunksCreated);
		AddChunksToSections();
		OnWriterEvent(ModuleWriterEvent.ChunksAddedToSections);
	}

	private void CreateSections()
	{
		CreatePESections();
		CreateRawSections();
		CreateExtraData();
	}

	private void CreateChunks()
	{
		CreateMetadataChunks(module);
		methodBodies.CanReuseOldBodyLocation = Options.OptimizeImageSize;
		CreateDebugDirectory();
		imageCor20Header = new ByteArrayChunk(new byte[72]);
		CreateStrongNameSignature();
	}

	private void AddChunksToSections()
	{
		textSection.Add(imageCor20Header, 4u);
		textSection.Add(strongNameSignature, 4u);
		textSection.Add(constants, 8u);
		textSection.Add(methodBodies, 4u);
		textSection.Add(netResources, 4u);
		textSection.Add(metadata, 4u);
		textSection.Add(debugDirectory, 4u);
		if (rsrcSection != null)
		{
			rsrcSection.Add(win32Resources, 8u);
		}
	}

	protected override Win32Resources GetWin32Resources()
	{
		if (Options.KeepWin32Resources)
		{
			return null;
		}
		if (Options.NoWin32Resources)
		{
			return null;
		}
		return Options.Win32Resources ?? module.Win32Resources;
	}

	private void CreatePESections()
	{
		sections = new List<PESection>();
		sections.Add(textSection = new PESection(".text", 1610612768u));
		if (GetWin32Resources() != null)
		{
			sections.Add(rsrcSection = new PESection(".rsrc", 1073741888u));
		}
	}

	private void CreateRawSections()
	{
		uint fileAlignment = peImage.ImageNTHeaders.OptionalHeader.FileAlignment;
		origSections = new List<OrigSection>(peImage.ImageSectionHeaders.Count);
		foreach (ImageSectionHeader imageSectionHeader in peImage.ImageSectionHeaders)
		{
			OrigSection origSection = new OrigSection(imageSectionHeader);
			origSections.Add(origSection);
			uint length = Utils.AlignUp(imageSectionHeader.SizeOfRawData, fileAlignment);
			origSection.Chunk = new DataReaderChunk(peImage.CreateReader(imageSectionHeader.VirtualAddress, length), imageSectionHeader.VirtualSize);
		}
	}

	private DataReaderChunk CreateHeaderSection(out IChunk extraHeaderData)
	{
		int num = (int)GetOffsetAfterLastSectionHeader() + sections.Count * 40;
		uint num2 = Math.Min(GetFirstRawDataFileOffset(), peImage.ImageNTHeaders.OptionalHeader.SectionAlignment);
		uint num3 = (uint)num;
		if (num2 > num3)
		{
			num3 = num2;
		}
		num3 = Utils.AlignUp(num3, peImage.ImageNTHeaders.OptionalHeader.FileAlignment);
		if (num3 <= peImage.ImageNTHeaders.OptionalHeader.SectionAlignment)
		{
			uint sizeOfHeaders = peImage.ImageNTHeaders.OptionalHeader.SizeOfHeaders;
			uint num4;
			if (num3 <= sizeOfHeaders)
			{
				num4 = 0u;
			}
			else
			{
				num4 = num3 - sizeOfHeaders;
				num3 = sizeOfHeaders;
			}
			if (num4 != 0)
			{
				extraHeaderData = new ByteArrayChunk(new byte[num4]);
			}
			else
			{
				extraHeaderData = null;
			}
			return new DataReaderChunk(peImage.CreateReader((FileOffset)0u, num3));
		}
		throw new ModuleWriterException("Could not create header");
	}

	private uint GetOffsetAfterLastSectionHeader()
	{
		return (uint)peImage.ImageSectionHeaders[peImage.ImageSectionHeaders.Count - 1].EndOffset;
	}

	private uint GetFirstRawDataFileOffset()
	{
		uint num = uint.MaxValue;
		foreach (ImageSectionHeader imageSectionHeader in peImage.ImageSectionHeaders)
		{
			num = Math.Min(num, imageSectionHeader.PointerToRawData);
		}
		return num;
	}

	private void CreateExtraData()
	{
		if (Options.KeepExtraPEData)
		{
			uint lastFileSectionOffset = GetLastFileSectionOffset();
			extraData = new DataReaderChunk(peImage.CreateReader((FileOffset)lastFileSectionOffset));
			if (extraData.CreateReader().Length == 0)
			{
				extraData = null;
			}
		}
	}

	private uint GetLastFileSectionOffset()
	{
		uint num = 0u;
		foreach (OrigSection origSection in origSections)
		{
			num = Math.Max(num, (uint)(origSection.PESection.VirtualAddress + origSection.PESection.SizeOfRawData));
		}
		return (uint)(peImage.ToFileOffset((RVA)(num - 1)) + 1);
	}

	private void ReuseIfPossible(PESection section, IReuseChunk chunk, RVA origRva, uint origSize, uint requiredAlignment)
	{
		if (origRva == (RVA)0u || origSize == 0 || chunk == null || !chunk.CanReuse(origRva, origSize) || ((uint)origRva & (requiredAlignment - 1)) != 0)
		{
			return;
		}
		RVA rVA = origRva + origSize;
		foreach (ReusedChunkInfo reusedChunk in reusedChunks)
		{
			if (origRva < reusedChunk.RVA + reusedChunk.Chunk.GetVirtualSize() && rVA > reusedChunk.RVA)
			{
				return;
			}
		}
		if (!section.Remove(chunk).HasValue)
		{
			throw new InvalidOperationException();
		}
		reusedChunks.Add(new ReusedChunkInfo(chunk, origRva));
	}

	private FileOffset GetNewFileOffset(RVA rva)
	{
		foreach (OrigSection origSection in origSections)
		{
			ImageSectionHeader pESection = origSection.PESection;
			if (pESection.VirtualAddress <= rva && rva < pESection.VirtualAddress + Math.Max(pESection.VirtualSize, pESection.SizeOfRawData))
			{
				return origSection.Chunk.FileOffset + (rva - pESection.VirtualAddress);
			}
		}
		return (FileOffset)rva;
	}

	private long WriteFile()
	{
		uint ep;
		bool entryPoint = GetEntryPoint(out ep);
		OnWriterEvent(ModuleWriterEvent.BeginWritePdb);
		WritePdbFile();
		OnWriterEvent(ModuleWriterEvent.EndWritePdb);
		metadata.OnBeforeSetOffset();
		OnWriterEvent(ModuleWriterEvent.BeginCalculateRvasAndFileOffsets);
		if (Options.OptimizeImageSize)
		{
			ImageDataDirectory imageDataDirectory = module.Metadata.ImageCor20Header.Metadata;
			metadata.SetOffset(peImage.ToFileOffset(imageDataDirectory.VirtualAddress), imageDataDirectory.VirtualAddress);
			ReuseIfPossible(textSection, metadata, imageDataDirectory.VirtualAddress, imageDataDirectory.Size, 4u);
			ImageDataDirectory imageDataDirectory2 = peImage.ImageNTHeaders.OptionalHeader.DataDirectories[2];
			if (win32Resources != null && imageDataDirectory2.VirtualAddress != 0 && imageDataDirectory2.Size != 0)
			{
				FileOffset offset = peImage.ToFileOffset(imageDataDirectory2.VirtualAddress);
				if (win32Resources.CheckValidOffset(offset))
				{
					win32Resources.SetOffset(offset, imageDataDirectory2.VirtualAddress);
					ReuseIfPossible(rsrcSection, win32Resources, imageDataDirectory2.VirtualAddress, imageDataDirectory2.Size, 8u);
				}
			}
			ReuseIfPossible(textSection, imageCor20Header, module.Metadata.PEImage.ImageNTHeaders.OptionalHeader.DataDirectories[14].VirtualAddress, module.Metadata.PEImage.ImageNTHeaders.OptionalHeader.DataDirectories[14].Size, 4u);
			if ((module.Metadata.ImageCor20Header.Flags & ComImageFlags.StrongNameSigned) != 0)
			{
				ReuseIfPossible(textSection, strongNameSignature, module.Metadata.ImageCor20Header.StrongNameSignature.VirtualAddress, module.Metadata.ImageCor20Header.StrongNameSignature.Size, 4u);
			}
			ReuseIfPossible(textSection, netResources, module.Metadata.ImageCor20Header.Resources.VirtualAddress, module.Metadata.ImageCor20Header.Resources.Size, 4u);
			if (methodBodies.ReusedAllMethodBodyLocations)
			{
				textSection.Remove(methodBodies);
			}
			ImageDataDirectory imageDataDirectory3 = peImage.ImageNTHeaders.OptionalHeader.DataDirectories[6];
			if (imageDataDirectory3.VirtualAddress != 0 && imageDataDirectory3.Size != 0 && TryGetRealDebugDirectorySize(peImage, out var realSize))
			{
				ReuseIfPossible(textSection, debugDirectory, imageDataDirectory3.VirtualAddress, realSize, 4u);
			}
		}
		if (constants.IsEmpty)
		{
			textSection.Remove(constants);
		}
		if (netResources.IsEmpty)
		{
			textSection.Remove(netResources);
		}
		if (textSection.IsEmpty)
		{
			sections.Remove(textSection);
		}
		if (rsrcSection != null && rsrcSection.IsEmpty)
		{
			sections.Remove(rsrcSection);
			rsrcSection = null;
		}
		IChunk extraHeaderData;
		DataReaderChunk dataReaderChunk = CreateHeaderSection(out extraHeaderData);
		List<IChunk> list = new List<IChunk>();
		uint headerLen;
		if (extraHeaderData != null)
		{
			ChunkList<IChunk> chunkList = new ChunkList<IChunk>();
			chunkList.Add(dataReaderChunk, 1u);
			chunkList.Add(extraHeaderData, 1u);
			list.Add(chunkList);
			headerLen = dataReaderChunk.GetVirtualSize() + extraHeaderData.GetVirtualSize();
		}
		else
		{
			list.Add(dataReaderChunk);
			headerLen = dataReaderChunk.GetVirtualSize();
		}
		foreach (OrigSection origSection in origSections)
		{
			list.Add(origSection.Chunk);
		}
		foreach (PESection section in sections)
		{
			list.Add(section);
		}
		if (extraData != null)
		{
			list.Add(extraData);
		}
		CalculateRvasAndFileOffsets(list, (FileOffset)0u, (RVA)0u, peImage.ImageNTHeaders.OptionalHeader.FileAlignment, peImage.ImageNTHeaders.OptionalHeader.SectionAlignment);
		if (reusedChunks.Count > 0 || methodBodies.HasReusedMethods)
		{
			methodBodies.InitializeReusedMethodBodies(GetNewFileOffset);
			foreach (ReusedChunkInfo reusedChunk in reusedChunks)
			{
				FileOffset newFileOffset = GetNewFileOffset(reusedChunk.RVA);
				reusedChunk.Chunk.SetOffset(newFileOffset, reusedChunk.RVA);
			}
		}
		metadata.UpdateMethodAndFieldRvas();
		foreach (OrigSection origSection2 in origSections)
		{
			if (origSection2.Chunk.RVA != origSection2.PESection.VirtualAddress)
			{
				throw new ModuleWriterException("Invalid section RVA");
			}
		}
		OnWriterEvent(ModuleWriterEvent.EndCalculateRvasAndFileOffsets);
		OnWriterEvent(ModuleWriterEvent.BeginWriteChunks);
		DataWriter dataWriter = new DataWriter(destStream);
		WriteChunks(dataWriter, list, (FileOffset)0u, peImage.ImageNTHeaders.OptionalHeader.FileAlignment);
		long num = dataWriter.Position - destStreamBaseOffset;
		if (reusedChunks.Count > 0 || methodBodies.HasReusedMethods)
		{
			long position = dataWriter.Position;
			foreach (ReusedChunkInfo reusedChunk2 in reusedChunks)
			{
				if (reusedChunk2.Chunk.RVA != reusedChunk2.RVA)
				{
					throw new InvalidOperationException();
				}
				dataWriter.Position = destStreamBaseOffset + (long)reusedChunk2.Chunk.FileOffset;
				reusedChunk2.Chunk.VerifyWriteTo(dataWriter);
			}
			methodBodies.WriteReusedMethodBodies(dataWriter, destStreamBaseOffset);
			dataWriter.Position = position;
		}
		SectionSizes sectionSizes = new SectionSizes(peImage.ImageNTHeaders.OptionalHeader.FileAlignment, peImage.ImageNTHeaders.OptionalHeader.SectionAlignment, headerLen, GetSectionSizeInfos);
		UpdateHeaderFields(dataWriter, entryPoint, ep, ref sectionSizes);
		OnWriterEvent(ModuleWriterEvent.EndWriteChunks);
		OnWriterEvent(ModuleWriterEvent.BeginStrongNameSign);
		if (Options.StrongNameKey != null)
		{
			StrongNameSign((long)strongNameSignature.FileOffset);
		}
		OnWriterEvent(ModuleWriterEvent.EndStrongNameSign);
		OnWriterEvent(ModuleWriterEvent.BeginWritePEChecksum);
		if (Options.AddCheckSum)
		{
			destStream.Position = destStreamBaseOffset;
			uint value = destStream.CalculatePECheckSum(num, checkSumOffset);
			dataWriter.Position = checkSumOffset;
			dataWriter.WriteUInt32(value);
		}
		OnWriterEvent(ModuleWriterEvent.EndWritePEChecksum);
		return num;
	}

	private static bool TryGetRealDebugDirectorySize(IPEImage peImage, out uint realSize)
	{
		realSize = 0u;
		if (peImage.ImageDebugDirectories.Count == 0)
		{
			return false;
		}
		List<ImageDebugDirectory> list = new List<ImageDebugDirectory>(peImage.ImageDebugDirectories);
		list.Sort((ImageDebugDirectory a, ImageDebugDirectory b) => a.AddressOfRawData.CompareTo(b.AddressOfRawData));
		ImageDataDirectory imageDataDirectory = peImage.ImageNTHeaders.OptionalHeader.DataDirectories[6];
		uint num = (uint)(imageDataDirectory.VirtualAddress + imageDataDirectory.Size);
		for (int i = 0; i < list.Count; i++)
		{
			uint num2 = (num + 3) & 0xFFFFFFFCu;
			ImageDebugDirectory imageDebugDirectory = list[i];
			if (imageDebugDirectory.AddressOfRawData != 0 && imageDebugDirectory.SizeOfData != 0)
			{
				if (num > (uint)imageDebugDirectory.AddressOfRawData || (uint)imageDebugDirectory.AddressOfRawData > num2)
				{
					return false;
				}
				num = (uint)(imageDebugDirectory.AddressOfRawData + imageDebugDirectory.SizeOfData);
			}
		}
		realSize = (uint)(num - imageDataDirectory.VirtualAddress);
		return true;
	}

	private bool Is64Bit()
	{
		return peImage.ImageNTHeaders.OptionalHeader is ImageOptionalHeader64;
	}

	private Characteristics GetCharacteristics()
	{
		Characteristics characteristics = module.Characteristics;
		characteristics = ((!Is64Bit()) ? (characteristics | Characteristics.Bit32Machine) : (characteristics & ~Characteristics.Bit32Machine));
		if (Options.IsExeFile)
		{
			return characteristics & ~Characteristics.Dll;
		}
		return characteristics | Characteristics.Dll;
	}

	private void UpdateHeaderFields(DataWriter writer, bool entryPointIsManagedOrNoEntryPoint, uint entryPointToken, ref SectionSizes sectionSizes)
	{
		long position = destStreamBaseOffset + (long)peImage.ImageNTHeaders.FileHeader.StartOffset;
		long position2 = destStreamBaseOffset + (long)peImage.ImageNTHeaders.OptionalHeader.StartOffset;
		long position3 = destStreamBaseOffset + (long)peImage.ImageSectionHeaders[0].StartOffset;
		long num = destStreamBaseOffset + (long)peImage.ImageNTHeaders.OptionalHeader.EndOffset - 128;
		long position4 = destStreamBaseOffset + (long)imageCor20Header.FileOffset;
		PEHeadersOptions pEHeadersOptions = Options.PEHeadersOptions;
		writer.Position = position;
		writer.WriteUInt16((ushort)(pEHeadersOptions.Machine ?? module.Machine));
		writer.WriteUInt16((ushort)(origSections.Count + sections.Count));
		WriteUInt32(writer, pEHeadersOptions.TimeDateStamp);
		WriteUInt32(writer, pEHeadersOptions.PointerToSymbolTable);
		WriteUInt32(writer, pEHeadersOptions.NumberOfSymbols);
		writer.Position += 2L;
		writer.WriteUInt16((ushort)(pEHeadersOptions.Characteristics ?? GetCharacteristics()));
		writer.Position = position2;
		if (peImage.ImageNTHeaders.OptionalHeader is ImageOptionalHeader32)
		{
			writer.Position += 2L;
			WriteByte(writer, pEHeadersOptions.MajorLinkerVersion);
			WriteByte(writer, pEHeadersOptions.MinorLinkerVersion);
			writer.WriteUInt32(sectionSizes.SizeOfCode);
			writer.WriteUInt32(sectionSizes.SizeOfInitdData);
			writer.WriteUInt32(sectionSizes.SizeOfUninitdData);
			writer.Position += 4L;
			writer.WriteUInt32(sectionSizes.BaseOfCode);
			writer.WriteUInt32(sectionSizes.BaseOfData);
			WriteUInt32(writer, pEHeadersOptions.ImageBase);
			writer.Position += 8L;
			WriteUInt16(writer, pEHeadersOptions.MajorOperatingSystemVersion);
			WriteUInt16(writer, pEHeadersOptions.MinorOperatingSystemVersion);
			WriteUInt16(writer, pEHeadersOptions.MajorImageVersion);
			WriteUInt16(writer, pEHeadersOptions.MinorImageVersion);
			WriteUInt16(writer, pEHeadersOptions.MajorSubsystemVersion);
			WriteUInt16(writer, pEHeadersOptions.MinorSubsystemVersion);
			WriteUInt32(writer, pEHeadersOptions.Win32VersionValue);
			writer.WriteUInt32(sectionSizes.SizeOfImage);
			writer.WriteUInt32(sectionSizes.SizeOfHeaders);
			checkSumOffset = writer.Position;
			writer.WriteInt32(0);
			WriteUInt16(writer, pEHeadersOptions.Subsystem);
			WriteUInt16(writer, pEHeadersOptions.DllCharacteristics);
			WriteUInt32(writer, pEHeadersOptions.SizeOfStackReserve);
			WriteUInt32(writer, pEHeadersOptions.SizeOfStackCommit);
			WriteUInt32(writer, pEHeadersOptions.SizeOfHeapReserve);
			WriteUInt32(writer, pEHeadersOptions.SizeOfHeapCommit);
			WriteUInt32(writer, pEHeadersOptions.LoaderFlags);
			WriteUInt32(writer, pEHeadersOptions.NumberOfRvaAndSizes);
		}
		else
		{
			writer.Position += 2L;
			WriteByte(writer, pEHeadersOptions.MajorLinkerVersion);
			WriteByte(writer, pEHeadersOptions.MinorLinkerVersion);
			writer.WriteUInt32(sectionSizes.SizeOfCode);
			writer.WriteUInt32(sectionSizes.SizeOfInitdData);
			writer.WriteUInt32(sectionSizes.SizeOfUninitdData);
			writer.Position += 4L;
			writer.WriteUInt32(sectionSizes.BaseOfCode);
			WriteUInt64(writer, pEHeadersOptions.ImageBase);
			writer.Position += 8L;
			WriteUInt16(writer, pEHeadersOptions.MajorOperatingSystemVersion);
			WriteUInt16(writer, pEHeadersOptions.MinorOperatingSystemVersion);
			WriteUInt16(writer, pEHeadersOptions.MajorImageVersion);
			WriteUInt16(writer, pEHeadersOptions.MinorImageVersion);
			WriteUInt16(writer, pEHeadersOptions.MajorSubsystemVersion);
			WriteUInt16(writer, pEHeadersOptions.MinorSubsystemVersion);
			WriteUInt32(writer, pEHeadersOptions.Win32VersionValue);
			writer.WriteUInt32(sectionSizes.SizeOfImage);
			writer.WriteUInt32(sectionSizes.SizeOfHeaders);
			checkSumOffset = writer.Position;
			writer.WriteInt32(0);
			WriteUInt16(writer, pEHeadersOptions.Subsystem ?? GetSubsystem());
			WriteUInt16(writer, pEHeadersOptions.DllCharacteristics ?? module.DllCharacteristics);
			WriteUInt64(writer, pEHeadersOptions.SizeOfStackReserve);
			WriteUInt64(writer, pEHeadersOptions.SizeOfStackCommit);
			WriteUInt64(writer, pEHeadersOptions.SizeOfHeapReserve);
			WriteUInt64(writer, pEHeadersOptions.SizeOfHeapCommit);
			WriteUInt32(writer, pEHeadersOptions.LoaderFlags);
			WriteUInt32(writer, pEHeadersOptions.NumberOfRvaAndSizes);
		}
		if (win32Resources != null)
		{
			writer.Position = num + 16;
			writer.WriteDataDirectory(win32Resources);
		}
		writer.Position = num + 32;
		writer.WriteDataDirectory(null);
		writer.Position = num + 48;
		writer.WriteDebugDirectory(debugDirectory);
		writer.Position = num + 112;
		writer.WriteDataDirectory(imageCor20Header);
		writer.Position = position3;
		foreach (OrigSection origSection in origSections)
		{
			writer.Position += 20L;
			writer.WriteUInt32((uint)origSection.Chunk.FileOffset);
			writer.Position += 16L;
		}
		foreach (PESection section in sections)
		{
			section.WriteHeaderTo(writer, peImage.ImageNTHeaders.OptionalHeader.FileAlignment, peImage.ImageNTHeaders.OptionalHeader.SectionAlignment, (uint)section.RVA);
		}
		writer.Position = position4;
		writer.WriteInt32(72);
		WriteUInt16(writer, Options.Cor20HeaderOptions.MajorRuntimeVersion);
		WriteUInt16(writer, Options.Cor20HeaderOptions.MinorRuntimeVersion);
		writer.WriteDataDirectory(metadata);
		writer.WriteUInt32((uint)GetComImageFlags(entryPointIsManagedOrNoEntryPoint));
		writer.WriteUInt32(entryPointToken);
		writer.WriteDataDirectory(netResources);
		writer.WriteDataDirectory(strongNameSignature);
		WriteDataDirectory(writer, module.Metadata.ImageCor20Header.CodeManagerTable);
		WriteDataDirectory(writer, module.Metadata.ImageCor20Header.VTableFixups);
		WriteDataDirectory(writer, module.Metadata.ImageCor20Header.ExportAddressTableJumps);
		WriteDataDirectory(writer, module.Metadata.ImageCor20Header.ManagedNativeHeader);
		UpdateVTableFixups(writer);
	}

	private static void WriteDataDirectory(DataWriter writer, ImageDataDirectory dataDir)
	{
		writer.WriteUInt32((uint)dataDir.VirtualAddress);
		writer.WriteUInt32(dataDir.Size);
	}

	private static void WriteByte(DataWriter writer, byte? value)
	{
		if (!value.HasValue)
		{
			writer.Position++;
		}
		else
		{
			writer.WriteByte(value.Value);
		}
	}

	private static void WriteUInt16(DataWriter writer, ushort? value)
	{
		if (!value.HasValue)
		{
			writer.Position += 2L;
		}
		else
		{
			writer.WriteUInt16(value.Value);
		}
	}

	private static void WriteUInt16(DataWriter writer, Subsystem? value)
	{
		if (!value.HasValue)
		{
			writer.Position += 2L;
		}
		else
		{
			writer.WriteUInt16((ushort)value.Value);
		}
	}

	private static void WriteUInt16(DataWriter writer, DllCharacteristics? value)
	{
		if (!value.HasValue)
		{
			writer.Position += 2L;
		}
		else
		{
			writer.WriteUInt16((ushort)value.Value);
		}
	}

	private static void WriteUInt32(DataWriter writer, uint? value)
	{
		if (!value.HasValue)
		{
			writer.Position += 4L;
		}
		else
		{
			writer.WriteUInt32(value.Value);
		}
	}

	private static void WriteUInt32(DataWriter writer, ulong? value)
	{
		if (!value.HasValue)
		{
			writer.Position += 4L;
		}
		else
		{
			writer.WriteUInt32((uint)value.Value);
		}
	}

	private static void WriteUInt64(DataWriter writer, ulong? value)
	{
		if (!value.HasValue)
		{
			writer.Position += 8L;
		}
		else
		{
			writer.WriteUInt64(value.Value);
		}
	}

	private ComImageFlags GetComImageFlags(bool isManagedEntryPoint)
	{
		ComImageFlags comImageFlags = Options.Cor20HeaderOptions.Flags ?? module.Cor20HeaderFlags;
		uint? entryPoint = Options.Cor20HeaderOptions.EntryPoint;
		if (entryPoint.HasValue)
		{
			return comImageFlags;
		}
		if (isManagedEntryPoint)
		{
			return comImageFlags & ~ComImageFlags.NativeEntryPoint;
		}
		return comImageFlags | ComImageFlags.NativeEntryPoint;
	}

	private Subsystem GetSubsystem()
	{
		if (module.Kind == ModuleKind.Windows)
		{
			return Subsystem.WindowsGui;
		}
		return Subsystem.WindowsCui;
	}

	private long ToWriterOffset(RVA rva)
	{
		if (rva == (RVA)0u)
		{
			return 0L;
		}
		foreach (OrigSection origSection in origSections)
		{
			ImageSectionHeader pESection = origSection.PESection;
			if (pESection.VirtualAddress <= rva && rva < pESection.VirtualAddress + Math.Max(pESection.VirtualSize, pESection.SizeOfRawData))
			{
				return destStreamBaseOffset + (long)origSection.Chunk.FileOffset + (rva - pESection.VirtualAddress);
			}
		}
		return 0L;
	}

	private IEnumerable<SectionSizeInfo> GetSectionSizeInfos()
	{
		foreach (OrigSection origSection in origSections)
		{
			yield return new SectionSizeInfo(origSection.Chunk.GetVirtualSize(), origSection.PESection.Characteristics);
		}
		foreach (PESection section in sections)
		{
			yield return new SectionSizeInfo(section.GetVirtualSize(), section.Characteristics);
		}
	}

	private void UpdateVTableFixups(DataWriter writer)
	{
		VTableFixups vTableFixups = module.VTableFixups;
		if (vTableFixups == null || vTableFixups.VTables.Count == 0)
		{
			return;
		}
		writer.Position = ToWriterOffset(vTableFixups.RVA);
		if (writer.Position == 0L)
		{
			Error("Could not convert RVA to file offset");
			return;
		}
		foreach (VTable item in vTableFixups)
		{
			if (item.Methods.Count > 65535)
			{
				throw new ModuleWriterException("Too many methods in vtable");
			}
			writer.WriteUInt32((uint)item.RVA);
			writer.WriteUInt16((ushort)item.Methods.Count);
			writer.WriteUInt16((ushort)item.Flags);
			long position = writer.Position;
			writer.Position = ToWriterOffset(item.RVA);
			if (writer.Position == 0L)
			{
				if (item.RVA != 0 || item.Methods.Count > 0)
				{
					Error("Could not convert RVA to file offset");
				}
			}
			else
			{
				IList<IMethod> methods = item.Methods;
				int count = methods.Count;
				for (int i = 0; i < count; i++)
				{
					IMethod method = methods[i];
					writer.WriteUInt32(GetMethodToken(method));
					if (item.Is64Bit)
					{
						writer.WriteInt32(0);
					}
				}
			}
			writer.Position = position;
		}
	}

	private uint GetMethodToken(IMethod method)
	{
		if (method is MethodDef md)
		{
			return new MDToken(Table.Method, metadata.GetRid(md)).Raw;
		}
		if (method is MemberRef mr)
		{
			return new MDToken(Table.MemberRef, metadata.GetRid(mr)).Raw;
		}
		if (method is MethodSpec ms)
		{
			return new MDToken(Table.MethodSpec, metadata.GetRid(ms)).Raw;
		}
		if (method == null)
		{
			return 0u;
		}
		Error("Invalid VTable method type: {0}", method.GetType());
		return 0u;
	}

	private bool GetEntryPoint(out uint ep)
	{
		uint? entryPoint = Options.Cor20HeaderOptions.EntryPoint;
		if (entryPoint.HasValue)
		{
			ep = entryPoint.Value;
			if (ep != 0)
			{
				return (Options.Cor20HeaderOptions.Flags.GetValueOrDefault() & ComImageFlags.NativeEntryPoint) == 0;
			}
			return true;
		}
		if (module.ManagedEntryPoint is MethodDef md)
		{
			ep = new MDToken(Table.Method, metadata.GetRid(md)).Raw;
			return true;
		}
		if (module.ManagedEntryPoint is FileDef fd)
		{
			ep = new MDToken(Table.File, metadata.GetRid(fd)).Raw;
			return true;
		}
		ep = (uint)module.NativeEntryPoint;
		return ep == 0;
	}
}

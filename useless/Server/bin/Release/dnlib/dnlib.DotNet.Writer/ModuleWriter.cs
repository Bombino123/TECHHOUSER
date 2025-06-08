using System;
using System.Collections.Generic;
using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.PE;
using dnlib.W32Resources;

namespace dnlib.DotNet.Writer;

public sealed class ModuleWriter : ModuleWriterBase
{
	private const uint DEFAULT_RELOC_ALIGNMENT = 4u;

	private const uint MVID_ALIGNMENT = 1u;

	private readonly ModuleDef module;

	private ModuleWriterOptions options;

	private List<PESection> sections;

	private PESection mvidSection;

	private PESection textSection;

	private PESection sdataSection;

	private PESection rsrcSection;

	private PESection relocSection;

	private PEHeaders peHeaders;

	private ImportAddressTable importAddressTable;

	private ImageCor20Header imageCor20Header;

	private ImportDirectory importDirectory;

	private StartupStub startupStub;

	private RelocDirectory relocDirectory;

	private ManagedExportsWriter managedExportsWriter;

	private bool needStartupStub;

	public override ModuleDef Module => module;

	public override ModuleWriterOptionsBase TheOptions => Options;

	public ModuleWriterOptions Options
	{
		get
		{
			return options ?? (options = new ModuleWriterOptions(module));
		}
		set
		{
			options = value;
		}
	}

	public override List<PESection> Sections => sections;

	public override PESection TextSection => textSection;

	internal PESection SdataSection => sdataSection;

	public override PESection RsrcSection => rsrcSection;

	public PESection RelocSection => relocSection;

	public PEHeaders PEHeaders => peHeaders;

	public ImportAddressTable ImportAddressTable => importAddressTable;

	public ImageCor20Header ImageCor20Header => imageCor20Header;

	public ImportDirectory ImportDirectory => importDirectory;

	public StartupStub StartupStub => startupStub;

	public RelocDirectory RelocDirectory => relocDirectory;

	public override void AddSection(PESection section)
	{
		if (sections.Count > 0 && sections[sections.Count - 1] == relocSection)
		{
			sections.Insert(sections.Count - 1, section);
		}
		else
		{
			sections.Add(section);
		}
	}

	public ModuleWriter(ModuleDef module)
		: this(module, null)
	{
	}

	public ModuleWriter(ModuleDef module, ModuleWriterOptions options)
	{
		this.module = module;
		this.options = options;
	}

	protected override long WriteImpl()
	{
		Initialize();
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

	protected override Win32Resources GetWin32Resources()
	{
		if (Options.NoWin32Resources)
		{
			return null;
		}
		return Options.Win32Resources ?? module.Win32Resources;
	}

	private void CreateSections()
	{
		sections = new List<PESection>();
		if (TheOptions.AddMvidSection)
		{
			sections.Add(mvidSection = new PESection(".mvid", 1107296320u));
		}
		sections.Add(textSection = new PESection(".text", 1610612768u));
		sections.Add(sdataSection = new PESection(".sdata", 3221225536u));
		if (GetWin32Resources() != null)
		{
			sections.Add(rsrcSection = new PESection(".rsrc", 1073741888u));
		}
		sections.Add(relocSection = new PESection(".reloc", 1107296320u));
	}

	private void CreateChunks()
	{
		peHeaders = new PEHeaders(Options.PEHeadersOptions);
		Machine machine = Options.PEHeadersOptions.Machine ?? Machine.I386;
		bool is64bit = machine.Is64Bit();
		relocDirectory = new RelocDirectory(machine);
		if (machine.IsI386())
		{
			needStartupStub = true;
		}
		importAddressTable = new ImportAddressTable(is64bit);
		importDirectory = new ImportDirectory(is64bit);
		startupStub = new StartupStub(relocDirectory, machine, delegate(string format, object[] args)
		{
			Error(format, args);
		});
		CreateStrongNameSignature();
		imageCor20Header = new ImageCor20Header(Options.Cor20HeaderOptions);
		CreateMetadataChunks(module);
		managedExportsWriter = new ManagedExportsWriter(UTF8String.ToSystemStringOrEmpty(module.Name), machine, relocDirectory, metadata, peHeaders, delegate(string format, object[] args)
		{
			Error(format, args);
		});
		CreateDebugDirectory();
		importDirectory.IsExeFile = Options.IsExeFile;
		peHeaders.IsExeFile = Options.IsExeFile;
	}

	private void AddChunksToSections()
	{
		uint alignment = ((Options.PEHeadersOptions.Machine ?? Machine.I386).Is64Bit() ? 8u : 4u);
		if (mvidSection != null)
		{
			mvidSection.Add(new ByteArrayChunk((module.Mvid ?? Guid.Empty).ToByteArray()), 1u);
		}
		textSection.Add(importAddressTable, alignment);
		textSection.Add(imageCor20Header, 4u);
		textSection.Add(strongNameSignature, 4u);
		managedExportsWriter.AddTextChunks(textSection);
		textSection.Add(constants, 8u);
		textSection.Add(methodBodies, 4u);
		textSection.Add(netResources, 4u);
		textSection.Add(metadata, 4u);
		textSection.Add(debugDirectory, 4u);
		textSection.Add(importDirectory, alignment);
		textSection.Add(startupStub, startupStub.Alignment);
		managedExportsWriter.AddSdataChunks(sdataSection);
		if (GetWin32Resources() != null)
		{
			rsrcSection.Add(win32Resources, 8u);
		}
		relocSection.Add(relocDirectory, 4u);
	}

	private long WriteFile()
	{
		managedExportsWriter.AddExportedMethods(metadata.ExportedMethods, GetTimeDateStamp());
		if (managedExportsWriter.HasExports)
		{
			needStartupStub = true;
		}
		OnWriterEvent(ModuleWriterEvent.BeginWritePdb);
		WritePdbFile();
		OnWriterEvent(ModuleWriterEvent.EndWritePdb);
		metadata.OnBeforeSetOffset();
		OnWriterEvent(ModuleWriterEvent.BeginCalculateRvasAndFileOffsets);
		List<IChunk> list = new List<IChunk>();
		list.Add(peHeaders);
		if (!managedExportsWriter.HasExports)
		{
			sections.Remove(sdataSection);
		}
		if (!relocDirectory.NeedsRelocSection && !managedExportsWriter.HasExports && !needStartupStub)
		{
			sections.Remove(relocSection);
		}
		importAddressTable.Enable = needStartupStub;
		importDirectory.Enable = needStartupStub;
		startupStub.Enable = needStartupStub;
		foreach (PESection section in sections)
		{
			list.Add(section);
		}
		peHeaders.PESections = sections;
		int num = sections.IndexOf(relocSection);
		if (num >= 0 && num != sections.Count - 1)
		{
			throw new InvalidOperationException("Reloc section must be the last section, use AddSection() to add a section");
		}
		CalculateRvasAndFileOffsets(list, (FileOffset)0u, (RVA)0u, peHeaders.FileAlignment, peHeaders.SectionAlignment);
		OnWriterEvent(ModuleWriterEvent.EndCalculateRvasAndFileOffsets);
		InitializeChunkProperties();
		OnWriterEvent(ModuleWriterEvent.BeginWriteChunks);
		DataWriter dataWriter = new DataWriter(destStream);
		WriteChunks(dataWriter, list, (FileOffset)0u, peHeaders.FileAlignment);
		long num2 = dataWriter.Position - destStreamBaseOffset;
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
			peHeaders.WriteCheckSum(dataWriter, num2);
		}
		OnWriterEvent(ModuleWriterEvent.EndWritePEChecksum);
		return num2;
	}

	private void InitializeChunkProperties()
	{
		Options.Cor20HeaderOptions.EntryPoint = GetEntryPoint();
		importAddressTable.ImportDirectory = importDirectory;
		importDirectory.ImportAddressTable = importAddressTable;
		startupStub.ImportDirectory = importDirectory;
		startupStub.PEHeaders = peHeaders;
		peHeaders.StartupStub = startupStub;
		peHeaders.ImageCor20Header = imageCor20Header;
		peHeaders.ImportAddressTable = importAddressTable;
		peHeaders.ImportDirectory = importDirectory;
		peHeaders.Win32Resources = win32Resources;
		peHeaders.RelocDirectory = relocDirectory;
		peHeaders.DebugDirectory = debugDirectory;
		imageCor20Header.Metadata = metadata;
		imageCor20Header.NetResources = netResources;
		imageCor20Header.StrongNameSignature = strongNameSignature;
		managedExportsWriter.InitializeChunkProperties();
	}

	private uint GetEntryPoint()
	{
		if (module.ManagedEntryPoint is MethodDef md)
		{
			return new MDToken(Table.Method, metadata.GetRid(md)).Raw;
		}
		if (module.ManagedEntryPoint is FileDef fd)
		{
			return new MDToken(Table.File, metadata.GetRid(fd)).Raw;
		}
		uint nativeEntryPoint = (uint)module.NativeEntryPoint;
		if (nativeEntryPoint != 0)
		{
			return nativeEntryPoint;
		}
		return 0u;
	}
}

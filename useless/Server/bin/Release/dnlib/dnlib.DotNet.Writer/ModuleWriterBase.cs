using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.DotNet.Pdb.Dss;
using dnlib.DotNet.Pdb.WindowsPdb;
using dnlib.IO;
using dnlib.PE;
using dnlib.W32Resources;

namespace dnlib.DotNet.Writer;

public abstract class ModuleWriterBase : ILogger
{
	protected internal const uint DEFAULT_CONSTANTS_ALIGNMENT = 8u;

	protected const uint DEFAULT_METHODBODIES_ALIGNMENT = 4u;

	protected const uint DEFAULT_NETRESOURCES_ALIGNMENT = 4u;

	protected const uint DEFAULT_METADATA_ALIGNMENT = 4u;

	protected internal const uint DEFAULT_WIN32_RESOURCES_ALIGNMENT = 8u;

	protected const uint DEFAULT_STRONGNAMESIG_ALIGNMENT = 4u;

	protected const uint DEFAULT_COR20HEADER_ALIGNMENT = 4u;

	protected Stream destStream;

	protected UniqueChunkList<ByteArrayChunk> constants;

	protected MethodBodyChunks methodBodies;

	protected NetResources netResources;

	protected Metadata metadata;

	protected Win32ResourcesChunk win32Resources;

	protected long destStreamBaseOffset;

	protected DebugDirectory debugDirectory;

	private string createdPdbFileName;

	protected StrongNameSignature strongNameSignature;

	private PdbState pdbState;

	private const uint PdbAge = 1u;

	private static readonly double[] eventToProgress = new double[30]
	{
		0.0, 0.00128048488389907, 0.0524625293056615, 0.0531036610555682, 0.0535679983835939, 0.0547784058004697, 0.0558606342971218, 0.120553993799033, 0.226210300699921, 0.236002648477671,
		0.291089703426468, 0.449919748849947, 0.449919985998736, 0.452716444513587, 0.452716681662375, 0.924922132195272, 0.931410404476231, 0.931425463424305, 0.932072998191503, 0.932175327893773,
		0.932175446468167, 0.954646479929387, 0.95492263969368, 0.980563166714175, 0.980563403862964, 0.980563403862964, 0.980563522437358, 0.999975573674777, 1.0, 1.0
	};

	public abstract ModuleWriterOptionsBase TheOptions { get; }

	public Stream DestinationStream => destStream;

	public UniqueChunkList<ByteArrayChunk> Constants => constants;

	public MethodBodyChunks MethodBodies => methodBodies;

	public NetResources NetResources => netResources;

	public Metadata Metadata => metadata;

	public Win32ResourcesChunk Win32Resources => win32Resources;

	public StrongNameSignature StrongNameSignature => strongNameSignature;

	public abstract List<PESection> Sections { get; }

	public abstract PESection TextSection { get; }

	public abstract PESection RsrcSection { get; }

	public DebugDirectory DebugDirectory => debugDirectory;

	public bool IsNativeWriter => this is NativeModuleWriter;

	public abstract ModuleDef Module { get; }

	public virtual void AddSection(PESection section)
	{
		Sections.Add(section);
	}

	public void Write(string fileName)
	{
		using FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		fileStream.SetLength(0L);
		try
		{
			Write(fileStream);
		}
		catch
		{
			fileStream.Close();
			DeleteFileNoThrow(fileName);
			throw;
		}
	}

	private static void DeleteFileNoThrow(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return;
		}
		try
		{
			File.Delete(fileName);
		}
		catch
		{
		}
	}

	public void Write(Stream dest)
	{
		pdbState = ((TheOptions.WritePdb && Module.PdbState != null) ? Module.PdbState : null);
		if (TheOptions.DelaySign)
		{
			TheOptions.Cor20HeaderOptions.Flags &= ~ComImageFlags.StrongNameSigned;
		}
		else if (TheOptions.StrongNameKey != null || TheOptions.StrongNamePublicKey != null)
		{
			TheOptions.Cor20HeaderOptions.Flags |= ComImageFlags.StrongNameSigned;
		}
		destStream = dest;
		destStreamBaseOffset = destStream.Position;
		OnWriterEvent(ModuleWriterEvent.Begin);
		long num = WriteImpl();
		destStream.Position = destStreamBaseOffset + num;
		OnWriterEvent(ModuleWriterEvent.End);
	}

	protected abstract long WriteImpl();

	protected void CreateStrongNameSignature()
	{
		if (TheOptions.DelaySign && TheOptions.StrongNamePublicKey != null)
		{
			int num = TheOptions.StrongNamePublicKey.CreatePublicKey().Length - 32;
			strongNameSignature = new StrongNameSignature((num > 0) ? num : 128);
		}
		else if (TheOptions.StrongNameKey != null)
		{
			strongNameSignature = new StrongNameSignature(TheOptions.StrongNameKey.SignatureSize);
		}
		else if (Module.Assembly != null && !PublicKeyBase.IsNullOrEmpty2(Module.Assembly.PublicKey))
		{
			int num2 = Module.Assembly.PublicKey.Data.Length - 32;
			strongNameSignature = new StrongNameSignature((num2 > 0) ? num2 : 128);
		}
		else if (((TheOptions.Cor20HeaderOptions.Flags ?? Module.Cor20HeaderFlags) & ComImageFlags.StrongNameSigned) != 0)
		{
			strongNameSignature = new StrongNameSignature(128);
		}
	}

	protected void CreateMetadataChunks(ModuleDef module)
	{
		constants = new UniqueChunkList<ByteArrayChunk>();
		methodBodies = new MethodBodyChunks(TheOptions.ShareMethodBodies);
		netResources = new NetResources(4u);
		metadata = Metadata.Create(debugKind: (pdbState != null && (pdbState.PdbFileKind == PdbFileKind.PortablePDB || pdbState.PdbFileKind == PdbFileKind.EmbeddedPortablePDB)) ? DebugMetadataKind.Standalone : DebugMetadataKind.None, module: module, constants: constants, methodBodies: methodBodies, netResources: netResources, options: TheOptions.MetadataOptions);
		metadata.Logger = TheOptions.MetadataLogger ?? this;
		metadata.MetadataEvent += Metadata_MetadataEvent;
		metadata.ProgressUpdated += Metadata_ProgressUpdated;
		StrongNamePublicKey strongNamePublicKey = TheOptions.StrongNamePublicKey;
		if (strongNamePublicKey != null)
		{
			metadata.AssemblyPublicKey = strongNamePublicKey.CreatePublicKey();
		}
		else if (TheOptions.StrongNameKey != null)
		{
			metadata.AssemblyPublicKey = TheOptions.StrongNameKey.PublicKey;
		}
		Win32Resources win32Resources = GetWin32Resources();
		if (win32Resources != null)
		{
			this.win32Resources = new Win32ResourcesChunk(win32Resources);
		}
	}

	protected abstract Win32Resources GetWin32Resources();

	protected void CalculateRvasAndFileOffsets(List<IChunk> chunks, FileOffset offset, RVA rva, uint fileAlignment, uint sectionAlignment)
	{
		int count = chunks.Count;
		uint num = Math.Min(fileAlignment, sectionAlignment);
		for (int i = 0; i < count; i++)
		{
			IChunk chunk = chunks[i];
			uint num2 = chunk.CalculateAlignment();
			if (num2 > num)
			{
				Error("Chunk alignment is too big. Chunk: {0}, alignment: {1:X4}", chunk, num2);
			}
			chunk.SetOffset(offset, rva);
			if (chunk.GetVirtualSize() != 0)
			{
				offset += chunk.GetFileLength();
				rva += chunk.GetVirtualSize();
				offset = offset.AlignUp(fileAlignment);
				rva = rva.AlignUp(sectionAlignment);
			}
		}
	}

	protected void WriteChunks(DataWriter writer, List<IChunk> chunks, FileOffset offset, uint fileAlignment)
	{
		int count = chunks.Count;
		for (int i = 0; i < count; i++)
		{
			IChunk chunk = chunks[i];
			chunk.VerifyWriteTo(writer);
			if (chunk.GetVirtualSize() != 0)
			{
				offset += chunk.GetFileLength();
				FileOffset fileOffset = offset.AlignUp(fileAlignment);
				writer.WriteZeroes((int)(fileOffset - offset));
				offset = fileOffset;
			}
		}
	}

	protected void StrongNameSign(long snSigOffset)
	{
		new StrongNameSigner(destStream, destStreamBaseOffset).WriteSignature(TheOptions.StrongNameKey, snSigOffset);
	}

	private bool CanWritePdb()
	{
		return pdbState != null;
	}

	protected void CreateDebugDirectory()
	{
		if (CanWritePdb())
		{
			debugDirectory = new DebugDirectory();
		}
	}

	protected void WritePdbFile()
	{
		if (!CanWritePdb())
		{
			return;
		}
		if (debugDirectory == null)
		{
			throw new InvalidOperationException("debugDirectory is null but WritePdb is true");
		}
		if (pdbState == null)
		{
			Error("TheOptions.WritePdb is true but module has no PdbState");
			return;
		}
		try
		{
			switch (pdbState.PdbFileKind)
			{
			case PdbFileKind.WindowsPDB:
				WriteWindowsPdb(pdbState);
				break;
			case PdbFileKind.PortablePDB:
				WritePortablePdb(pdbState, isEmbeddedPortablePdb: false);
				break;
			case PdbFileKind.EmbeddedPortablePDB:
				WritePortablePdb(pdbState, isEmbeddedPortablePdb: true);
				break;
			default:
				Error("Invalid PDB file kind {0}", pdbState.PdbFileKind);
				break;
			}
		}
		catch
		{
			DeleteFileNoThrow(createdPdbFileName);
			throw;
		}
	}

	private void AddReproduciblePdbDebugDirectoryEntry()
	{
		debugDirectory.Add(Array2.Empty<byte>(), ImageDebugType.Reproducible, 0, 0, 0u);
	}

	private void AddPdbChecksumDebugDirectoryEntry(byte[] checksumBytes, ChecksumAlgorithm checksumAlgorithm)
	{
		MemoryStream memoryStream = new MemoryStream();
		DataWriter dataWriter = new DataWriter(memoryStream);
		string checksumName = Hasher.GetChecksumName(checksumAlgorithm);
		dataWriter.WriteBytes(Encoding.UTF8.GetBytes(checksumName));
		dataWriter.WriteByte(0);
		dataWriter.WriteBytes(checksumBytes);
		byte[] data = memoryStream.ToArray();
		debugDirectory.Add(data, ImageDebugType.PdbChecksum, 1, 0, 0u);
	}

	private void WriteWindowsPdb(PdbState pdbState)
	{
		bool flag = (TheOptions.PdbOptions & PdbWriterOptions.PdbChecksum) != 0;
		flag = false;
		string pdbFilename;
		SymbolWriter windowsPdbSymbolWriter = GetWindowsPdbSymbolWriter(TheOptions.PdbOptions, out pdbFilename);
		if (windowsPdbSymbolWriter == null)
		{
			Error("Could not create a PDB symbol writer. A Windows OS might be required.");
			return;
		}
		using WindowsPdbWriter windowsPdbWriter = new WindowsPdbWriter(windowsPdbSymbolWriter, pdbState, metadata);
		windowsPdbWriter.Logger = TheOptions.Logger;
		windowsPdbWriter.Write();
		uint pdbAge = 1u;
		if (windowsPdbWriter.GetDebugInfo(TheOptions.PdbChecksumAlgorithm, ref pdbAge, out var guid, out var stamp, out var idd, out var codeViewData))
		{
			debugDirectory.Add(GetCodeViewData(guid, pdbAge, TheOptions.PdbFileNameInDebugDirectory ?? pdbFilename), ImageDebugType.CodeView, 0, 0, stamp);
		}
		else
		{
			if (codeViewData == null)
			{
				throw new InvalidOperationException();
			}
			DebugDirectoryEntry debugDirectoryEntry = debugDirectory.Add(codeViewData);
			debugDirectoryEntry.DebugDirectory = idd;
			debugDirectoryEntry.DebugDirectory.TimeDateStamp = GetTimeDateStamp();
		}
		if (windowsPdbSymbolWriter.IsDeterministic)
		{
			AddReproduciblePdbDebugDirectoryEntry();
		}
	}

	protected uint GetTimeDateStamp()
	{
		uint? timeDateStamp = TheOptions.PEHeadersOptions.TimeDateStamp;
		if (timeDateStamp.HasValue)
		{
			return timeDateStamp.Value;
		}
		TheOptions.PEHeadersOptions.TimeDateStamp = PEHeadersOptions.CreateNewTimeDateStamp();
		return TheOptions.PEHeadersOptions.TimeDateStamp.Value;
	}

	private SymbolWriter GetWindowsPdbSymbolWriter(PdbWriterOptions options, out string pdbFilename)
	{
		if (TheOptions.PdbStream != null)
		{
			Stream pdbStream = TheOptions.PdbStream;
			string obj = TheOptions.PdbFileName ?? GetStreamName(TheOptions.PdbStream) ?? GetDefaultPdbFileName();
			string pdbFileName = obj;
			pdbFilename = obj;
			return SymbolReaderWriterFactory.Create(options, pdbStream, pdbFileName);
		}
		if (!string.IsNullOrEmpty(TheOptions.PdbFileName))
		{
			createdPdbFileName = (pdbFilename = TheOptions.PdbFileName);
			return SymbolReaderWriterFactory.Create(options, createdPdbFileName);
		}
		createdPdbFileName = (pdbFilename = GetDefaultPdbFileName());
		if (createdPdbFileName == null)
		{
			return null;
		}
		return SymbolReaderWriterFactory.Create(options, createdPdbFileName);
	}

	private static string GetStreamName(Stream stream)
	{
		return (stream as FileStream)?.Name;
	}

	private static string GetModuleName(ModuleDef module)
	{
		UTF8String uTF8String = module.Name ?? ((UTF8String)string.Empty);
		if (string.IsNullOrEmpty(uTF8String))
		{
			return null;
		}
		if (uTF8String.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || uTF8String.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || uTF8String.EndsWith(".netmodule", StringComparison.OrdinalIgnoreCase))
		{
			return uTF8String;
		}
		return string.Concat(uTF8String, ".pdb");
	}

	private string GetDefaultPdbFileName()
	{
		string text = GetStreamName(destStream) ?? GetModuleName(Module);
		if (string.IsNullOrEmpty(text))
		{
			Error("TheOptions.WritePdb is true but it's not possible to guess the default PDB file name. Set PdbFileName to the name of the PDB file.");
			return null;
		}
		return Path.ChangeExtension(text, "pdb");
	}

	private void WritePortablePdb(PdbState pdbState, bool isEmbeddedPortablePdb)
	{
		bool ownsStream = false;
		Stream stream = null;
		try
		{
			MemoryStream portablePdbStream = null;
			if (isEmbeddedPortablePdb)
			{
				stream = (portablePdbStream = new MemoryStream());
				ownsStream = true;
			}
			else
			{
				stream = GetStandalonePortablePdbStream(out ownsStream);
			}
			if (stream == null)
			{
				throw new ModuleWriterException("Couldn't create a PDB stream");
			}
			string text = TheOptions.PdbFileName ?? GetStreamName(stream) ?? GetDefaultPdbFileName();
			if (isEmbeddedPortablePdb)
			{
				text = Path.GetFileName(text);
			}
			uint entryPointToken = ((pdbState.UserEntryPoint != null) ? new MDToken(Table.Method, metadata.GetRid(pdbState.UserEntryPoint)).Raw : 0u);
			metadata.WritePortablePdb(stream, entryPointToken, out var pdbIdOffset);
			byte[] array = new byte[20];
			ArrayWriter arrayWriter = new ArrayWriter(array);
			byte[] array2;
			Guid guid;
			uint timestamp;
			if ((TheOptions.PdbOptions & PdbWriterOptions.Deterministic) != 0 || (TheOptions.PdbOptions & PdbWriterOptions.PdbChecksum) != 0 || TheOptions.GetPdbContentId == null)
			{
				stream.Position = 0L;
				array2 = Hasher.Hash(TheOptions.PdbChecksumAlgorithm, stream, stream.Length);
				if (array2.Length < 20)
				{
					throw new ModuleWriterException("Checksum bytes length < 20");
				}
				RoslynContentIdProvider.GetContentId(array2, out guid, out timestamp);
			}
			else
			{
				ContentId contentId = TheOptions.GetPdbContentId(stream, GetTimeDateStamp());
				timestamp = contentId.Timestamp;
				guid = contentId.Guid;
				array2 = null;
			}
			arrayWriter.WriteBytes(guid.ToByteArray());
			arrayWriter.WriteUInt32(timestamp);
			stream.Position = pdbIdOffset;
			stream.Write(array, 0, array.Length);
			debugDirectory.Add(GetCodeViewData(guid, 1u, TheOptions.PdbFileNameInDebugDirectory ?? text), ImageDebugType.CodeView, 256, 20557, timestamp);
			if (array2 != null)
			{
				AddPdbChecksumDebugDirectoryEntry(array2, TheOptions.PdbChecksumAlgorithm);
			}
			if ((TheOptions.PdbOptions & PdbWriterOptions.Deterministic) != 0)
			{
				AddReproduciblePdbDebugDirectoryEntry();
			}
			if (isEmbeddedPortablePdb)
			{
				debugDirectory.Add(CreateEmbeddedPortablePdbBlob(portablePdbStream), ImageDebugType.EmbeddedPortablePdb, 256, 256, 0u);
			}
		}
		finally
		{
			if (ownsStream)
			{
				stream?.Dispose();
			}
		}
	}

	private static byte[] CreateEmbeddedPortablePdbBlob(MemoryStream portablePdbStream)
	{
		byte[] array = Compress(portablePdbStream);
		byte[] array2 = new byte[8 + array.Length];
		DataWriter dataWriter = new DataWriter(new MemoryStream(array2));
		dataWriter.WriteInt32(1111773261);
		dataWriter.WriteUInt32((uint)portablePdbStream.Length);
		dataWriter.WriteBytes(array);
		return array2;
	}

	private static byte[] Compress(MemoryStream sourceStream)
	{
		sourceStream.Position = 0L;
		MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
		{
			byte[] array = sourceStream.ToArray();
			deflateStream.Write(array, 0, array.Length);
		}
		return memoryStream.ToArray();
	}

	private static byte[] GetCodeViewData(Guid guid, uint age, string filename)
	{
		MemoryStream memoryStream = new MemoryStream();
		DataWriter dataWriter = new DataWriter(memoryStream);
		dataWriter.WriteInt32(1396986706);
		dataWriter.WriteBytes(guid.ToByteArray());
		dataWriter.WriteUInt32(age);
		dataWriter.WriteBytes(Encoding.UTF8.GetBytes(filename));
		dataWriter.WriteByte(0);
		return memoryStream.ToArray();
	}

	private Stream GetStandalonePortablePdbStream(out bool ownsStream)
	{
		if (TheOptions.PdbStream != null)
		{
			ownsStream = false;
			return TheOptions.PdbStream;
		}
		if (!string.IsNullOrEmpty(TheOptions.PdbFileName))
		{
			createdPdbFileName = TheOptions.PdbFileName;
		}
		else
		{
			createdPdbFileName = GetDefaultPdbFileName();
		}
		if (createdPdbFileName == null)
		{
			ownsStream = false;
			return null;
		}
		ownsStream = true;
		return File.Create(createdPdbFileName);
	}

	private void Metadata_MetadataEvent(object sender, MetadataWriterEventArgs e)
	{
		switch (e.Event)
		{
		case MetadataEvent.BeginCreateTables:
			OnWriterEvent(ModuleWriterEvent.MDBeginCreateTables);
			break;
		case MetadataEvent.AllocateTypeDefRids:
			OnWriterEvent(ModuleWriterEvent.MDAllocateTypeDefRids);
			break;
		case MetadataEvent.AllocateMemberDefRids:
			OnWriterEvent(ModuleWriterEvent.MDAllocateMemberDefRids);
			break;
		case MetadataEvent.MemberDefRidsAllocated:
			OnWriterEvent(ModuleWriterEvent.MDMemberDefRidsAllocated);
			break;
		case MetadataEvent.MemberDefsInitialized:
			OnWriterEvent(ModuleWriterEvent.MDMemberDefsInitialized);
			break;
		case MetadataEvent.BeforeSortTables:
			OnWriterEvent(ModuleWriterEvent.MDBeforeSortTables);
			break;
		case MetadataEvent.MostTablesSorted:
			OnWriterEvent(ModuleWriterEvent.MDMostTablesSorted);
			break;
		case MetadataEvent.MemberDefCustomAttributesWritten:
			OnWriterEvent(ModuleWriterEvent.MDMemberDefCustomAttributesWritten);
			break;
		case MetadataEvent.BeginAddResources:
			OnWriterEvent(ModuleWriterEvent.MDBeginAddResources);
			break;
		case MetadataEvent.EndAddResources:
			OnWriterEvent(ModuleWriterEvent.MDEndAddResources);
			break;
		case MetadataEvent.BeginWriteMethodBodies:
			OnWriterEvent(ModuleWriterEvent.MDBeginWriteMethodBodies);
			break;
		case MetadataEvent.EndWriteMethodBodies:
			OnWriterEvent(ModuleWriterEvent.MDEndWriteMethodBodies);
			break;
		case MetadataEvent.OnAllTablesSorted:
			OnWriterEvent(ModuleWriterEvent.MDOnAllTablesSorted);
			break;
		case MetadataEvent.EndCreateTables:
			OnWriterEvent(ModuleWriterEvent.MDEndCreateTables);
			break;
		}
	}

	private void Metadata_ProgressUpdated(object sender, MetadataProgressEventArgs e)
	{
		RaiseProgress(ModuleWriterEvent.MDBeginCreateTables, ModuleWriterEvent.BeginWritePdb, e.Progress);
	}

	protected void OnWriterEvent(ModuleWriterEvent evt)
	{
		RaiseProgress(evt, 0.0);
		TheOptions.RaiseEvent(this, new ModuleWriterEventArgs(this, evt));
	}

	private void RaiseProgress(ModuleWriterEvent evt, double subProgress)
	{
		RaiseProgress(evt, evt + 1, subProgress);
	}

	private void RaiseProgress(ModuleWriterEvent evt, ModuleWriterEvent nextEvt, double subProgress)
	{
		subProgress = Math.Min(1.0, Math.Max(0.0, subProgress));
		double num = eventToProgress[(int)evt];
		double num2 = eventToProgress[(int)nextEvt];
		double val = num + (num2 - num) * subProgress;
		val = Math.Min(1.0, Math.Max(0.0, val));
		TheOptions.RaiseEvent(this, new ModuleWriterProgressEventArgs(this, val));
	}

	private ILogger GetLogger()
	{
		return TheOptions.Logger ?? DummyLogger.ThrowModuleWriterExceptionOnErrorInstance;
	}

	void ILogger.Log(object sender, LoggerEvent loggerEvent, string format, params object[] args)
	{
		GetLogger().Log(this, loggerEvent, format, args);
	}

	bool ILogger.IgnoresEvent(LoggerEvent loggerEvent)
	{
		return GetLogger().IgnoresEvent(loggerEvent);
	}

	protected void Error(string format, params object[] args)
	{
		GetLogger().Log(this, LoggerEvent.Error, format, args);
	}

	protected void Warning(string format, params object[] args)
	{
		GetLogger().Log(this, LoggerEvent.Warning, format, args);
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.PE;
using dnlib.W32Resources;

namespace dnlib.DotNet.Writer;

public class ModuleWriterOptionsBase
{
	private PEHeadersOptions peHeadersOptions;

	private Cor20HeaderOptions cor20HeaderOptions;

	private MetadataOptions metadataOptions;

	private ILogger logger;

	private ILogger metadataLogger;

	private bool noWin32Resources;

	private Win32Resources win32Resources;

	private StrongNameKey strongNameKey;

	private StrongNamePublicKey strongNamePublicKey;

	private bool delaySign;

	private const ChecksumAlgorithm DefaultPdbChecksumAlgorithm = ChecksumAlgorithm.SHA256;

	public ILogger Logger
	{
		get
		{
			return logger;
		}
		set
		{
			logger = value;
		}
	}

	public ILogger MetadataLogger
	{
		get
		{
			return metadataLogger;
		}
		set
		{
			metadataLogger = value;
		}
	}

	public PEHeadersOptions PEHeadersOptions
	{
		get
		{
			return peHeadersOptions ?? (peHeadersOptions = new PEHeadersOptions());
		}
		set
		{
			peHeadersOptions = value;
		}
	}

	public Cor20HeaderOptions Cor20HeaderOptions
	{
		get
		{
			return cor20HeaderOptions ?? (cor20HeaderOptions = new Cor20HeaderOptions());
		}
		set
		{
			cor20HeaderOptions = value;
		}
	}

	public MetadataOptions MetadataOptions
	{
		get
		{
			return metadataOptions ?? (metadataOptions = new MetadataOptions());
		}
		set
		{
			metadataOptions = value;
		}
	}

	public bool NoWin32Resources
	{
		get
		{
			return noWin32Resources;
		}
		set
		{
			noWin32Resources = value;
		}
	}

	public Win32Resources Win32Resources
	{
		get
		{
			return win32Resources;
		}
		set
		{
			win32Resources = value;
		}
	}

	public bool DelaySign
	{
		get
		{
			return delaySign;
		}
		set
		{
			delaySign = value;
		}
	}

	public StrongNameKey StrongNameKey
	{
		get
		{
			return strongNameKey;
		}
		set
		{
			strongNameKey = value;
		}
	}

	public StrongNamePublicKey StrongNamePublicKey
	{
		get
		{
			return strongNamePublicKey;
		}
		set
		{
			strongNamePublicKey = value;
		}
	}

	public bool ShareMethodBodies { get; set; }

	public bool AddCheckSum { get; set; }

	public bool Is64Bit
	{
		get
		{
			if (!PEHeadersOptions.Machine.HasValue)
			{
				return false;
			}
			return PEHeadersOptions.Machine.Value.Is64Bit();
		}
	}

	public ModuleKind ModuleKind { get; set; }

	public bool IsExeFile
	{
		get
		{
			if (ModuleKind != ModuleKind.Dll)
			{
				return ModuleKind != ModuleKind.NetModule;
			}
			return false;
		}
	}

	public bool WritePdb { get; set; }

	public PdbWriterOptions PdbOptions { get; set; }

	public string PdbFileName { get; set; }

	public string PdbFileNameInDebugDirectory { get; set; }

	public Stream PdbStream { get; set; }

	public Func<Stream, uint, ContentId> GetPdbContentId { get; set; }

	public ChecksumAlgorithm PdbChecksumAlgorithm { get; set; } = ChecksumAlgorithm.SHA256;


	public bool AddMvidSection { get; set; }

	public event EventHandler2<ModuleWriterEventArgs> WriterEvent;

	public event EventHandler2<ModuleWriterProgressEventArgs> ProgressUpdated;

	internal void RaiseEvent(object sender, ModuleWriterEventArgs e)
	{
		this.WriterEvent?.Invoke(sender, e);
	}

	internal void RaiseEvent(object sender, ModuleWriterProgressEventArgs e)
	{
		this.ProgressUpdated?.Invoke(sender, e);
	}

	protected ModuleWriterOptionsBase(ModuleDef module)
	{
		ShareMethodBodies = true;
		MetadataOptions.MetadataHeaderOptions.VersionString = module.RuntimeVersion;
		ModuleKind = module.Kind;
		PEHeadersOptions.Machine = module.Machine;
		PEHeadersOptions.Characteristics = module.Characteristics;
		PEHeadersOptions.DllCharacteristics = module.DllCharacteristics;
		if (module.Kind == ModuleKind.Windows)
		{
			PEHeadersOptions.Subsystem = Subsystem.WindowsGui;
		}
		else
		{
			PEHeadersOptions.Subsystem = Subsystem.WindowsCui;
		}
		PEHeadersOptions.NumberOfRvaAndSizes = 16u;
		Cor20HeaderOptions.Flags = module.Cor20HeaderFlags;
		if (module.Assembly != null && !PublicKeyBase.IsNullOrEmpty2(module.Assembly.PublicKey))
		{
			Cor20HeaderOptions.Flags |= ComImageFlags.StrongNameSigned;
		}
		if (module.Cor20HeaderRuntimeVersion.HasValue)
		{
			Cor20HeaderOptions.MajorRuntimeVersion = (ushort)(module.Cor20HeaderRuntimeVersion.Value >> 16);
			Cor20HeaderOptions.MinorRuntimeVersion = (ushort)module.Cor20HeaderRuntimeVersion.Value;
		}
		else if (module.IsClr1x)
		{
			Cor20HeaderOptions.MajorRuntimeVersion = (ushort)2;
			Cor20HeaderOptions.MinorRuntimeVersion = 0;
		}
		else
		{
			Cor20HeaderOptions.MajorRuntimeVersion = (ushort)2;
			Cor20HeaderOptions.MinorRuntimeVersion = (ushort)5;
		}
		if (module.TablesHeaderVersion.HasValue)
		{
			MetadataOptions.TablesHeapOptions.MajorVersion = (byte)(module.TablesHeaderVersion.Value >> 8);
			MetadataOptions.TablesHeapOptions.MinorVersion = (byte)module.TablesHeaderVersion.Value;
		}
		else if (module.IsClr1x)
		{
			MetadataOptions.TablesHeapOptions.MajorVersion = (byte)1;
			MetadataOptions.TablesHeapOptions.MinorVersion = 0;
		}
		else
		{
			MetadataOptions.TablesHeapOptions.MajorVersion = (byte)2;
			MetadataOptions.TablesHeapOptions.MinorVersion = 0;
		}
		MetadataOptions.Flags |= MetadataFlags.AlwaysCreateGuidHeap;
		ModuleDefMD moduleDefMD = module as ModuleDefMD;
		if (moduleDefMD != null)
		{
			ImageNTHeaders imageNTHeaders = moduleDefMD.Metadata.PEImage.ImageNTHeaders;
			PEHeadersOptions.TimeDateStamp = imageNTHeaders.FileHeader.TimeDateStamp;
			PEHeadersOptions.MajorLinkerVersion = imageNTHeaders.OptionalHeader.MajorLinkerVersion;
			PEHeadersOptions.MinorLinkerVersion = imageNTHeaders.OptionalHeader.MinorLinkerVersion;
			PEHeadersOptions.ImageBase = imageNTHeaders.OptionalHeader.ImageBase;
			PEHeadersOptions.MajorOperatingSystemVersion = imageNTHeaders.OptionalHeader.MajorOperatingSystemVersion;
			PEHeadersOptions.MinorOperatingSystemVersion = imageNTHeaders.OptionalHeader.MinorOperatingSystemVersion;
			PEHeadersOptions.MajorImageVersion = imageNTHeaders.OptionalHeader.MajorImageVersion;
			PEHeadersOptions.MinorImageVersion = imageNTHeaders.OptionalHeader.MinorImageVersion;
			PEHeadersOptions.MajorSubsystemVersion = imageNTHeaders.OptionalHeader.MajorSubsystemVersion;
			PEHeadersOptions.MinorSubsystemVersion = imageNTHeaders.OptionalHeader.MinorSubsystemVersion;
			PEHeadersOptions.Win32VersionValue = imageNTHeaders.OptionalHeader.Win32VersionValue;
			AddCheckSum = imageNTHeaders.OptionalHeader.CheckSum != 0;
			AddMvidSection = HasMvidSection(moduleDefMD.Metadata.PEImage.ImageSectionHeaders);
			if (HasDebugDirectoryEntry(moduleDefMD.Metadata.PEImage.ImageDebugDirectories, ImageDebugType.Reproducible))
			{
				PdbOptions |= PdbWriterOptions.Deterministic;
			}
			if (HasDebugDirectoryEntry(moduleDefMD.Metadata.PEImage.ImageDebugDirectories, ImageDebugType.PdbChecksum))
			{
				PdbOptions |= PdbWriterOptions.PdbChecksum;
			}
			if (TryGetPdbChecksumAlgorithm(moduleDefMD.Metadata.PEImage, moduleDefMD.Metadata.PEImage.ImageDebugDirectories, out var pdbChecksumAlgorithm))
			{
				PdbChecksumAlgorithm = pdbChecksumAlgorithm;
			}
			MetadataOptions.TablesHeapOptions.Log2Rid = moduleDefMD.TablesStream.Log2Rid;
		}
		if (Is64Bit)
		{
			PEHeadersOptions.Characteristics &= ~Characteristics.Bit32Machine;
			PEHeadersOptions.Characteristics |= Characteristics.LargeAddressAware;
		}
		else if (moduleDefMD == null)
		{
			PEHeadersOptions.Characteristics |= Characteristics.Bit32Machine;
		}
	}

	private static bool HasMvidSection(IList<ImageSectionHeader> sections)
	{
		int count = sections.Count;
		for (int i = 0; i < count; i++)
		{
			ImageSectionHeader imageSectionHeader = sections[i];
			if (imageSectionHeader.VirtualSize == 16)
			{
				byte[] name = imageSectionHeader.Name;
				if (name[0] == 46 && name[1] == 109 && name[2] == 118 && name[3] == 105 && name[4] == 100 && name[5] == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool HasDebugDirectoryEntry(IList<ImageDebugDirectory> debugDirs, ImageDebugType type)
	{
		int count = debugDirs.Count;
		for (int i = 0; i < count; i++)
		{
			if (debugDirs[i].Type == type)
			{
				return true;
			}
		}
		return false;
	}

	private static bool TryGetPdbChecksumAlgorithm(IPEImage peImage, IList<ImageDebugDirectory> debugDirs, out ChecksumAlgorithm pdbChecksumAlgorithm)
	{
		int count = debugDirs.Count;
		for (int i = 0; i < count; i++)
		{
			ImageDebugDirectory imageDebugDirectory = debugDirs[i];
			if (imageDebugDirectory.Type == ImageDebugType.PdbChecksum)
			{
				DataReader reader = peImage.CreateReader(imageDebugDirectory.AddressOfRawData, imageDebugDirectory.SizeOfData);
				if (TryGetPdbChecksumAlgorithm(ref reader, out pdbChecksumAlgorithm))
				{
					return true;
				}
			}
		}
		pdbChecksumAlgorithm = ChecksumAlgorithm.SHA256;
		return false;
	}

	private static bool TryGetPdbChecksumAlgorithm(ref DataReader reader, out ChecksumAlgorithm pdbChecksumAlgorithm)
	{
		try
		{
			if (Hasher.TryGetChecksumAlgorithm(reader.TryReadZeroTerminatedUtf8String(), out pdbChecksumAlgorithm, out var checksumSize) && checksumSize == (int)reader.BytesLeft)
			{
				return true;
			}
		}
		catch (IOException)
		{
		}
		catch (ArgumentException)
		{
		}
		pdbChecksumAlgorithm = ChecksumAlgorithm.SHA256;
		return false;
	}

	public void InitializeStrongNameSigning(ModuleDef module, StrongNameKey signatureKey)
	{
		StrongNameKey = signatureKey;
		StrongNamePublicKey = null;
		if (module.Assembly != null)
		{
			module.Assembly.CustomAttributes.RemoveAll("System.Reflection.AssemblySignatureKeyAttribute");
		}
	}

	public void InitializeEnhancedStrongNameSigning(ModuleDef module, StrongNameKey signatureKey, StrongNamePublicKey signaturePubKey)
	{
		InitializeStrongNameSigning(module, signatureKey);
		StrongNameKey = StrongNameKey.WithHashAlgorithm(signaturePubKey.HashAlgorithm);
	}

	public void InitializeEnhancedStrongNameSigning(ModuleDef module, StrongNameKey signatureKey, StrongNamePublicKey signaturePubKey, StrongNameKey identityKey, StrongNamePublicKey identityPubKey)
	{
		StrongNameKey = signatureKey.WithHashAlgorithm(signaturePubKey.HashAlgorithm);
		StrongNamePublicKey = identityPubKey;
		if (module.Assembly != null)
		{
			module.Assembly.UpdateOrCreateAssemblySignatureKeyAttribute(identityPubKey, identityKey, signaturePubKey);
		}
	}
}

using System;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.MD;

public sealed class ImageCor20Header : FileSection
{
	private readonly uint cb;

	private readonly ushort majorRuntimeVersion;

	private readonly ushort minorRuntimeVersion;

	private readonly ImageDataDirectory metadata;

	private readonly ComImageFlags flags;

	private readonly uint entryPointToken_or_RVA;

	private readonly ImageDataDirectory resources;

	private readonly ImageDataDirectory strongNameSignature;

	private readonly ImageDataDirectory codeManagerTable;

	private readonly ImageDataDirectory vtableFixups;

	private readonly ImageDataDirectory exportAddressTableJumps;

	private readonly ImageDataDirectory managedNativeHeader;

	public bool HasNativeHeader => (flags & ComImageFlags.ILLibrary) != 0;

	public uint CB => cb;

	public ushort MajorRuntimeVersion => majorRuntimeVersion;

	public ushort MinorRuntimeVersion => minorRuntimeVersion;

	public ImageDataDirectory Metadata => metadata;

	public ComImageFlags Flags => flags;

	public uint EntryPointToken_or_RVA => entryPointToken_or_RVA;

	public ImageDataDirectory Resources => resources;

	public ImageDataDirectory StrongNameSignature => strongNameSignature;

	public ImageDataDirectory CodeManagerTable => codeManagerTable;

	public ImageDataDirectory VTableFixups => vtableFixups;

	public ImageDataDirectory ExportAddressTableJumps => exportAddressTableJumps;

	public ImageDataDirectory ManagedNativeHeader => managedNativeHeader;

	public ImageCor20Header(ref DataReader reader, bool verify)
	{
		SetStartOffset(ref reader);
		cb = reader.ReadUInt32();
		if (verify && cb < 72)
		{
			throw new BadImageFormatException("Invalid IMAGE_COR20_HEADER.cb value");
		}
		majorRuntimeVersion = reader.ReadUInt16();
		minorRuntimeVersion = reader.ReadUInt16();
		metadata = new ImageDataDirectory(ref reader, verify);
		flags = (ComImageFlags)reader.ReadUInt32();
		entryPointToken_or_RVA = reader.ReadUInt32();
		resources = new ImageDataDirectory(ref reader, verify);
		strongNameSignature = new ImageDataDirectory(ref reader, verify);
		codeManagerTable = new ImageDataDirectory(ref reader, verify);
		vtableFixups = new ImageDataDirectory(ref reader, verify);
		exportAddressTableJumps = new ImageDataDirectory(ref reader, verify);
		managedNativeHeader = new ImageDataDirectory(ref reader, verify);
		SetEndoffset(ref reader);
	}
}

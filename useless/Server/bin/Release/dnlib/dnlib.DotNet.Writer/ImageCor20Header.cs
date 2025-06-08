using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class ImageCor20Header : IChunk
{
	private FileOffset offset;

	private RVA rva;

	private Cor20HeaderOptions options;

	public Metadata Metadata { get; set; }

	public NetResources NetResources { get; set; }

	public StrongNameSignature StrongNameSignature { get; set; }

	internal IChunk VtableFixups { get; set; }

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public ImageCor20Header(Cor20HeaderOptions options)
	{
		this.options = options;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
	}

	public uint GetFileLength()
	{
		return 72u;
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	public void WriteTo(DataWriter writer)
	{
		writer.WriteInt32(72);
		writer.WriteUInt16(options.MajorRuntimeVersion ?? 2);
		writer.WriteUInt16(options.MinorRuntimeVersion ?? 5);
		writer.WriteDataDirectory(Metadata);
		writer.WriteUInt32((uint)(options.Flags ?? ComImageFlags.ILOnly));
		writer.WriteUInt32(options.EntryPoint.GetValueOrDefault());
		writer.WriteDataDirectory(NetResources);
		writer.WriteDataDirectory(StrongNameSignature);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(VtableFixups);
		writer.WriteDataDirectory(null);
		writer.WriteDataDirectory(null);
	}
}

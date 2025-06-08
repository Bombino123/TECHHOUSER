using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public abstract class HeapBase : IHeap, IChunk
{
	internal const uint ALIGNMENT = 4u;

	private FileOffset offset;

	private RVA rva;

	protected bool isReadOnly;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public abstract string Name { get; }

	public bool IsEmpty => GetRawLength() <= 1;

	public bool IsBig => GetFileLength() > 65535;

	public void SetReadOnly()
	{
		isReadOnly = true;
	}

	public virtual void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
	}

	public uint GetFileLength()
	{
		return Utils.AlignUp(GetRawLength(), 4u);
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	public abstract uint GetRawLength();

	public void WriteTo(DataWriter writer)
	{
		WriteToImpl(writer);
		writer.WriteZeroes((int)(Utils.AlignUp(GetRawLength(), 4u) - GetRawLength()));
	}

	protected abstract void WriteToImpl(DataWriter writer);

	public override string ToString()
	{
		return Name;
	}
}

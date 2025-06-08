using System.Runtime.InteropServices;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class MethodBody : IChunk
{
	private const uint EXTRA_SECTIONS_ALIGNMENT = 4u;

	private readonly bool isTiny;

	private readonly byte[] code;

	private readonly byte[] extraSections;

	private uint length;

	private FileOffset offset;

	private RVA rva;

	private readonly uint localVarSigTok;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public byte[] Code => code;

	public byte[] ExtraSections => extraSections;

	public uint LocalVarSigTok => localVarSigTok;

	public bool IsFat => !isTiny;

	public bool IsTiny => isTiny;

	public bool HasExtraSections
	{
		get
		{
			if (extraSections != null)
			{
				return extraSections.Length != 0;
			}
			return false;
		}
	}

	public MethodBody(byte[] code)
		: this(code, null, 0u)
	{
	}

	public MethodBody(byte[] code, byte[] extraSections)
		: this(code, extraSections, 0u)
	{
	}

	public MethodBody(byte[] code, byte[] extraSections, uint localVarSigTok)
	{
		isTiny = (code[0] & 3) == 2;
		this.code = code;
		this.extraSections = extraSections;
		this.localVarSigTok = localVarSigTok;
	}

	public int GetApproximateSizeOfMethodBody()
	{
		int num = code.Length;
		if (extraSections != null)
		{
			num = Utils.AlignUp(num, 4u);
			num += extraSections.Length;
			num = Utils.AlignUp(num, 4u);
		}
		return num;
	}

	internal bool CanReuse(RVA origRva, uint origSize)
	{
		uint num = ((!HasExtraSections) ? ((uint)code.Length) : ((uint)((int)((RVA)((uint)origRva + (uint)code.Length)).AlignUp(4u) + extraSections.Length) - (uint)origRva));
		return num <= origSize;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
		if (HasExtraSections)
		{
			RVA rVA = (RVA)((uint)rva + (uint)code.Length);
			rVA = rVA.AlignUp(4u);
			rVA = (RVA)((uint)rVA + (uint)extraSections.Length);
			length = rVA - rva;
		}
		else
		{
			length = (uint)code.Length;
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

	public void WriteTo(DataWriter writer)
	{
		writer.WriteBytes(code);
		if (HasExtraSections)
		{
			RVA rVA = (RVA)((uint)rva + (uint)code.Length);
			writer.WriteZeroes((int)(rVA.AlignUp(4u) - rVA));
			writer.WriteBytes(extraSections);
		}
	}

	public override int GetHashCode()
	{
		return Utils.GetHashCode(code) + Utils.GetHashCode(extraSections);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MethodBody methodBody))
		{
			return false;
		}
		if (Utils.Equals(code, methodBody.code))
		{
			return Utils.Equals(extraSections, methodBody.extraSections);
		}
		return false;
	}
}

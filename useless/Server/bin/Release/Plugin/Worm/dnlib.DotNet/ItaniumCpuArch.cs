using System;
using dnlib.DotNet.Writer;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet;

internal sealed class ItaniumCpuArch : CpuArch
{
	public override uint GetStubAlignment(StubType stubType)
	{
		if ((uint)stubType <= 1u)
		{
			return 16u;
		}
		throw new ArgumentOutOfRangeException();
	}

	public override uint GetStubSize(StubType stubType)
	{
		if ((uint)stubType <= 1u)
		{
			return 48u;
		}
		throw new ArgumentOutOfRangeException();
	}

	public override uint GetStubCodeOffset(StubType stubType)
	{
		if ((uint)stubType <= 1u)
		{
			return 32u;
		}
		throw new ArgumentOutOfRangeException();
	}

	protected override bool TryGetExportedRvaFromStubCore(ref DataReader reader, IPEImage peImage, out uint funcRva)
	{
		funcRva = 0u;
		ulong num = reader.ReadUInt64();
		ulong num2 = reader.ReadUInt64();
		reader.Position = (uint)peImage.ToFileOffset((RVA)(num - peImage.ImageNTHeaders.OptionalHeader.ImageBase));
		if (reader.ReadUInt64() != 4656739709999925259L)
		{
			return false;
		}
		if (reader.ReadUInt64() != 1125899909476388L)
		{
			return false;
		}
		if (reader.ReadUInt64() != 5791646816365709328L)
		{
			return false;
		}
		if (reader.ReadUInt64() != 36029209336053764L)
		{
			return false;
		}
		ulong num3 = num2 - peImage.ImageNTHeaders.OptionalHeader.ImageBase;
		if (num3 > uint.MaxValue)
		{
			return false;
		}
		funcRva = (uint)num3;
		return true;
	}

	public override void WriteStubRelocs(StubType stubType, RelocDirectory relocDirectory, IChunk chunk, uint stubOffset)
	{
		if ((uint)stubType <= 1u)
		{
			relocDirectory.Add(chunk, stubOffset + 32);
			relocDirectory.Add(chunk, stubOffset + 40);
			return;
		}
		throw new ArgumentOutOfRangeException();
	}

	public override void WriteStub(StubType stubType, DataWriter writer, ulong imageBase, uint stubRva, uint managedFuncRva)
	{
		if ((uint)stubType <= 1u)
		{
			writer.WriteUInt64(4656739709999925259uL);
			writer.WriteUInt64(1125899909476388uL);
			writer.WriteUInt64(5791646816365709328uL);
			writer.WriteUInt64(36029209336053764uL);
			writer.WriteUInt64(imageBase + stubRva);
			writer.WriteUInt64(imageBase + managedFuncRva);
			return;
		}
		throw new ArgumentOutOfRangeException();
	}
}

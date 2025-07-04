using System;
using dnlib.DotNet.Writer;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet;

internal sealed class ArmCpuArch : CpuArch
{
	public override uint GetStubAlignment(StubType stubType)
	{
		if ((uint)stubType <= 1u)
		{
			return 4u;
		}
		throw new ArgumentOutOfRangeException();
	}

	public override uint GetStubSize(StubType stubType)
	{
		if ((uint)stubType <= 1u)
		{
			return 8u;
		}
		throw new ArgumentOutOfRangeException();
	}

	public override uint GetStubCodeOffset(StubType stubType)
	{
		if ((uint)stubType <= 1u)
		{
			return 0u;
		}
		throw new ArgumentOutOfRangeException();
	}

	protected override bool TryGetExportedRvaFromStubCore(ref DataReader reader, IPEImage peImage, out uint funcRva)
	{
		funcRva = 0u;
		if (reader.ReadUInt32() != 4026595551u)
		{
			return false;
		}
		funcRva = reader.ReadUInt32() - (uint)(int)peImage.ImageNTHeaders.OptionalHeader.ImageBase;
		return true;
	}

	public override void WriteStubRelocs(StubType stubType, RelocDirectory relocDirectory, IChunk chunk, uint stubOffset)
	{
		if ((uint)stubType <= 1u)
		{
			relocDirectory.Add(chunk, stubOffset + 4);
			return;
		}
		throw new ArgumentOutOfRangeException();
	}

	public override void WriteStub(StubType stubType, DataWriter writer, ulong imageBase, uint stubRva, uint managedFuncRva)
	{
		if ((uint)stubType <= 1u)
		{
			writer.WriteUInt32(4026595551u);
			writer.WriteUInt32((uint)(int)imageBase + managedFuncRva);
			return;
		}
		throw new ArgumentOutOfRangeException();
	}
}

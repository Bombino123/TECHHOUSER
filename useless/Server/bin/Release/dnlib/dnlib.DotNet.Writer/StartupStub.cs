using System;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class StartupStub : IChunk
{
	private const StubType stubType = StubType.EntryPoint;

	private readonly RelocDirectory relocDirectory;

	private readonly Machine machine;

	private readonly CpuArch cpuArch;

	private readonly Action<string, object[]> logError;

	private FileOffset offset;

	private RVA rva;

	public ImportDirectory ImportDirectory { get; set; }

	public PEHeaders PEHeaders { get; set; }

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public RVA EntryPointRVA => rva + ((cpuArch != null) ? cpuArch.GetStubCodeOffset(StubType.EntryPoint) : 0);

	internal bool Enable { get; set; }

	internal uint Alignment
	{
		get
		{
			if (cpuArch != null)
			{
				return cpuArch.GetStubAlignment(StubType.EntryPoint);
			}
			return 1u;
		}
	}

	internal StartupStub(RelocDirectory relocDirectory, Machine machine, Action<string, object[]> logError)
	{
		this.relocDirectory = relocDirectory;
		this.machine = machine;
		this.logError = logError;
		CpuArch.TryGetCpuArch(machine, out cpuArch);
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
		if (Enable)
		{
			if (cpuArch == null)
			{
				logError("The module needs an unmanaged entry point but the CPU architecture isn't supported: {0} (0x{1:X4})", new object[2]
				{
					machine,
					(ushort)machine
				});
			}
			else
			{
				cpuArch.WriteStubRelocs(StubType.EntryPoint, relocDirectory, this, 0u);
			}
		}
	}

	public uint GetFileLength()
	{
		if (!Enable)
		{
			return 0u;
		}
		if (cpuArch == null)
		{
			return 0u;
		}
		return cpuArch.GetStubSize(StubType.EntryPoint);
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
		if (Enable && cpuArch != null)
		{
			cpuArch.WriteStub(StubType.EntryPoint, writer, PEHeaders.ImageBase, (uint)rva, (uint)ImportDirectory.IatCorXxxMainRVA);
		}
	}
}

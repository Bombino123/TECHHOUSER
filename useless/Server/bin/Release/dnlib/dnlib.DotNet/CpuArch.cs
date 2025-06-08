using dnlib.DotNet.Writer;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet;

internal abstract class CpuArch
{
	private static readonly X86CpuArch x86CpuArch = new X86CpuArch();

	private static readonly X64CpuArch x64CpuArch = new X64CpuArch();

	private static readonly ItaniumCpuArch itaniumCpuArch = new ItaniumCpuArch();

	private static readonly ArmCpuArch armCpuArch = new ArmCpuArch();

	public abstract uint GetStubAlignment(StubType stubType);

	public abstract uint GetStubSize(StubType stubType);

	public abstract uint GetStubCodeOffset(StubType stubType);

	public static bool TryGetCpuArch(Machine machine, out CpuArch cpuArch)
	{
		switch (machine)
		{
		case Machine.I386:
		case Machine.I386_Native_Sun:
		case Machine.I386_Native_NetBSD:
		case Machine.I386_Native_Apple:
		case Machine.I386_Native_Linux:
		case Machine.I386_Native_FreeBSD:
			cpuArch = x86CpuArch;
			return true;
		case Machine.AMD64_Native_FreeBSD:
		case Machine.AMD64:
		case Machine.AMD64_Native_Sun:
		case Machine.AMD64_Native_NetBSD:
		case Machine.AMD64_Native_Apple:
		case Machine.AMD64_Native_Linux:
			cpuArch = x64CpuArch;
			return true;
		case Machine.IA64:
			cpuArch = itaniumCpuArch;
			return true;
		case Machine.ARMNT:
		case Machine.ARMNT_Native_Sun:
		case Machine.ARMNT_Native_NetBSD:
		case Machine.ARMNT_Native_Apple:
		case Machine.ARMNT_Native_Linux:
		case Machine.ARMNT_Native_FreeBSD:
			cpuArch = armCpuArch;
			return true;
		default:
			cpuArch = null;
			return false;
		}
	}

	public bool TryGetExportedRvaFromStub(ref DataReader reader, IPEImage peImage, out uint funcRva)
	{
		return TryGetExportedRvaFromStubCore(ref reader, peImage, out funcRva);
	}

	protected abstract bool TryGetExportedRvaFromStubCore(ref DataReader reader, IPEImage peImage, out uint funcRva);

	public abstract void WriteStubRelocs(StubType stubType, RelocDirectory relocDirectory, IChunk chunk, uint stubOffset);

	public abstract void WriteStub(StubType stubType, DataWriter writer, ulong imageBase, uint stubRva, uint managedFuncRva);
}

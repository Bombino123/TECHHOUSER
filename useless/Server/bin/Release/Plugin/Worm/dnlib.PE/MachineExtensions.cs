using System.Runtime.InteropServices;

namespace dnlib.PE;

[ComVisible(true)]
public static class MachineExtensions
{
	public static bool Is64Bit(this Machine machine)
	{
		switch (machine)
		{
		case Machine.IA64:
		case Machine.ARM64_Native_FreeBSD:
		case Machine.AMD64_Native_FreeBSD:
		case Machine.AMD64:
		case Machine.AMD64_Native_Sun:
		case Machine.AMD64_Native_NetBSD:
		case Machine.ARM64:
		case Machine.ARM64_Native_Sun:
		case Machine.ARM64_Native_NetBSD:
		case Machine.AMD64_Native_Apple:
		case Machine.ARM64_Native_Linux:
		case Machine.ARM64_Native_Apple:
		case Machine.AMD64_Native_Linux:
			return true;
		case Machine.S390X_Native_Sun:
		case Machine.S390X_Native_NetBSD:
		case Machine.S390X_Native_Apple:
		case Machine.S390X_Native_Linux:
		case Machine.S390X_Native_FreeBSD:
			return true;
		default:
			return false;
		}
	}

	public static bool IsI386(this Machine machine)
	{
		switch (machine)
		{
		case Machine.I386:
		case Machine.I386_Native_Sun:
		case Machine.I386_Native_NetBSD:
		case Machine.I386_Native_Apple:
		case Machine.I386_Native_Linux:
		case Machine.I386_Native_FreeBSD:
			return true;
		default:
			return false;
		}
	}

	public static bool IsAMD64(this Machine machine)
	{
		switch (machine)
		{
		case Machine.AMD64_Native_FreeBSD:
		case Machine.AMD64:
		case Machine.AMD64_Native_Sun:
		case Machine.AMD64_Native_NetBSD:
		case Machine.AMD64_Native_Apple:
		case Machine.AMD64_Native_Linux:
			return true;
		default:
			return false;
		}
	}

	public static bool IsARMNT(this Machine machine)
	{
		switch (machine)
		{
		case Machine.ARMNT:
		case Machine.ARMNT_Native_Sun:
		case Machine.ARMNT_Native_NetBSD:
		case Machine.ARMNT_Native_Apple:
		case Machine.ARMNT_Native_Linux:
		case Machine.ARMNT_Native_FreeBSD:
			return true;
		default:
			return false;
		}
	}

	public static bool IsARM64(this Machine machine)
	{
		switch (machine)
		{
		case Machine.ARM64_Native_FreeBSD:
		case Machine.ARM64:
		case Machine.ARM64_Native_Sun:
		case Machine.ARM64_Native_NetBSD:
		case Machine.ARM64_Native_Linux:
		case Machine.ARM64_Native_Apple:
			return true;
		default:
			return false;
		}
	}

	public static bool IsS390x(this Machine machine)
	{
		switch (machine)
		{
		case Machine.S390X_Native_Sun:
		case Machine.S390X_Native_NetBSD:
		case Machine.S390X_Native_Apple:
		case Machine.S390X_Native_Linux:
		case Machine.S390X_Native_FreeBSD:
			return true;
		default:
			return false;
		}
	}
}

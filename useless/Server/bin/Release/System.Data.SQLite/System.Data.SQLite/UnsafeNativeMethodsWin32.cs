using System.Runtime.InteropServices;
using System.Security;

namespace System.Data.SQLite;

[SuppressUnmanagedCodeSecurity]
internal static class UnsafeNativeMethodsWin32
{
	internal enum ProcessorArchitecture : ushort
	{
		Intel = 0,
		MIPS = 1,
		Alpha = 2,
		PowerPC = 3,
		SHx = 4,
		ARM = 5,
		IA64 = 6,
		Alpha64 = 7,
		MSIL = 8,
		AMD64 = 9,
		IA32_on_Win64 = 10,
		Neutral = 11,
		ARM64 = 12,
		Unknown = ushort.MaxValue
	}

	internal struct SYSTEM_INFO
	{
		public ProcessorArchitecture wProcessorArchitecture;

		public ushort wReserved;

		public uint dwPageSize;

		public IntPtr lpMinimumApplicationAddress;

		public IntPtr lpMaximumApplicationAddress;

		public IntPtr dwActiveProcessorMask;

		public uint dwNumberOfProcessors;

		public uint dwProcessorType;

		public uint dwAllocationGranularity;

		public ushort wProcessorLevel;

		public ushort wProcessorRevision;
	}

	[DllImport("kernel32", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
	internal static extern IntPtr LoadLibrary(string fileName);

	[DllImport("kernel32")]
	internal static extern void GetSystemInfo(out SYSTEM_INFO systemInfo);
}

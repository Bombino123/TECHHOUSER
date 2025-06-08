using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct QueryVideoMemoryInformation
{
	public long Budget;

	public long CurrentUsage;

	public long AvailableForReservation;

	public long CurrentReservation;
}

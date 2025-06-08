using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct InfoQueueFilterDescription
{
	public int NumCategories;

	public IntPtr PCategoryList;

	public int NumSeverities;

	public IntPtr PSeverityList;

	public int NumIDs;

	public IntPtr PIDList;
}

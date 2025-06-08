using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("B20D55B3-532E-4906-87E7-25BD5734ABD2")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedAsyncMethod
{
	bool IsAsyncMethod();

	uint GetKickoffMethod();

	bool HasCatchHandlerILOffset();

	uint GetCatchHandlerILOffset();

	uint GetAsyncStepInfoCount();

	void GetAsyncStepInfo([In] uint cStepInfo, out uint pcStepInfo, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] yieldOffsets, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] breakpointOffset, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] breakpointMethod);
}

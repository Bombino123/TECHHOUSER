using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("FC073774-1739-4232-BD56-A027294BEC15")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedAsyncMethodPropertiesWriter
{
	void DefineKickoffMethod([In] uint kickoffMethod);

	void DefineCatchHandlerILOffset([In] uint catchHandlerOffset);

	void DefineAsyncStepInfo([In] uint count, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] yieldOffsets, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] breakpointOffset, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] breakpointMethod);
}

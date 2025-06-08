using System;
using System.Runtime.InteropServices;
using NAudio.Wasapi.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi.Interfaces;

[ComImport]
[Guid("AE2DE0E4-5BCA-4F2D-AA46-5D13F8FDB3A9")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPart
{
	int GetName([MarshalAs(UnmanagedType.LPWStr)] out string name);

	int GetLocalId(out uint id);

	int GetGlobalId([MarshalAs(UnmanagedType.LPWStr)] out string id);

	int GetPartType(out PartTypeEnum partType);

	int GetSubType(out Guid subType);

	int GetControlInterfaceCount(out uint count);

	int GetControlInterface([In] uint index, [MarshalAs(UnmanagedType.IUnknown)] out IControlInterface controlInterface);

	[PreserveSig]
	int EnumPartsIncoming(out IPartsList parts);

	[PreserveSig]
	int EnumPartsOutgoing(out IPartsList parts);

	int GetTopologyObject(out object topologyObject);

	[PreserveSig]
	int Activate([In] ClsCtx dwClsContext, [In] ref Guid refiid, [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

	int RegisterControlChangeCallback([In] ref Guid refiid, [In] IControlChangeNotify notify);

	int UnregisterControlChangeCallback([In] IControlChangeNotify notify);
}

using System;
using System.Runtime.InteropServices;

namespace NAudio.CoreAudioApi.Interfaces;

[ComImport]
[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDevice
{
	int Activate(ref Guid id, ClsCtx clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

	int OpenPropertyStore(StorageAccessMode stgmAccess, out IPropertyStore properties);

	int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);

	int GetState(out DeviceState state);
}

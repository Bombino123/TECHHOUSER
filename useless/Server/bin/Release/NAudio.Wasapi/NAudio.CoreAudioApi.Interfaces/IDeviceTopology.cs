using System.Runtime.InteropServices;

namespace NAudio.CoreAudioApi.Interfaces;

[ComImport]
[Guid("2A07407E-6497-4A18-9787-32F79BD0D98F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDeviceTopology
{
	int GetConnectorCount(out uint count);

	int GetConnector(uint index, out IConnector connector);

	int GetSubunitCount(out uint count);

	int GetSubunit(uint index, out ISubunit subunit);

	int GetPartById(uint id, out IPart part);

	int GetDeviceId([MarshalAs(UnmanagedType.LPWStr)] out string id);

	int GetSignalPath(IPart from, IPart to, bool rejectMixedPaths, out IPartsList parts);
}

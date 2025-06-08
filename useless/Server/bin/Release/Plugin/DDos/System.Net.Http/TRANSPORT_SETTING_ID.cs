using System.Runtime.InteropServices;

namespace System.Net.Http;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct TRANSPORT_SETTING_ID
{
	public Guid Guid;
}

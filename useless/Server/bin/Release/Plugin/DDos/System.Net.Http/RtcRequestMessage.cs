using System.Runtime.InteropServices;
using System.Security;

namespace System.Net.Http;

[ComVisible(true)]
internal class RtcRequestMessage : HttpRequestMessage, INetworkTransportSettings, INotificationTransportSync
{
	internal RtcState state;

	internal extern RtcRequestMessage(HttpMethod method, Uri uri);

	[SecuritySafeCritical]
	public extern void ApplySetting([In] ref TRANSPORT_SETTING_ID settingId, [In] int lengthIn, [In] IntPtr valueIn, out int lengthOut, out IntPtr valueOut);

	[SecuritySafeCritical]
	public extern void QuerySetting([In] ref TRANSPORT_SETTING_ID settingId, [In] int lengthIn, [In] IntPtr valueIn, out int lengthOut, out IntPtr valueOut);

	public extern void CompleteDelivery();

	public extern void Flush();
}

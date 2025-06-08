using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Shadow(typeof(VirtualSurfaceUpdatesCallbackNativeShadow))]
[Guid("dbf2e947-8e6c-4254-9eee-7738f71386c9")]
internal interface IVirtualSurfaceUpdatesCallbackNative : IUnknown, ICallbackable, IDisposable
{
	void UpdatesNeeded();
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX;

[Shadow(typeof(InspectableShadow))]
[Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90")]
public interface IInspectable : ICallbackable, IDisposable
{
}

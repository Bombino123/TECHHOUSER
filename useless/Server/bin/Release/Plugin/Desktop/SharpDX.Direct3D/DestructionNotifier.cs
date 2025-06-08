using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D;

[Guid("a06eb39a-50da-425b-8c31-4eecd6c270f3")]
public interface DestructionNotifier : IUnknown, ICallbackable, IDisposable
{
}

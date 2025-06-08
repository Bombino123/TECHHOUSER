using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

internal class VirtualSurfaceUpdatesCallbackNativeShadow : ComObjectShadow
{
	public class VirtualSurfaceUpdatesCallbackNativeVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int UpdatesNeededDelegate(IntPtr thisPtr);

		public VirtualSurfaceUpdatesCallbackNativeVtbl()
			: base(1)
		{
			AddMethod(new UpdatesNeededDelegate(UpdatesNeededImpl));
		}

		private static int UpdatesNeededImpl(IntPtr thisPtr)
		{
			try
			{
				((IVirtualSurfaceUpdatesCallbackNative)CppObjectShadow.ToShadow<VirtualSurfaceUpdatesCallbackNativeShadow>(thisPtr).Callback).UpdatesNeeded();
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly VirtualSurfaceUpdatesCallbackNativeVtbl Vtbl = new VirtualSurfaceUpdatesCallbackNativeVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(IVirtualSurfaceUpdatesCallbackNative virtualSurfaceUpdatesCallbackNative)
	{
		return CppObject.ToCallbackPtr<IVirtualSurfaceUpdatesCallbackNative>(virtualSurfaceUpdatesCallbackNative);
	}
}

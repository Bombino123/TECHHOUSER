using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("e9550983-360b-4f53-b391-afd695078691")]
public class IVirtualSurfaceImageSourceNative : ISurfaceImageSourceNative
{
	private class VirtualSurfaceUpdatesCallbackNativeCallback : CallbackBase, IVirtualSurfaceUpdatesCallbackNative, IUnknown, ICallbackable, IDisposable
	{
		private IVirtualSurfaceImageSourceNative eventCallback;

		public VirtualSurfaceUpdatesCallbackNativeCallback(IVirtualSurfaceImageSourceNative eventCallbackArg)
		{
			eventCallback = eventCallbackArg;
		}

		public void UpdatesNeeded()
		{
			eventCallback.OnUpdatesNeeded();
		}
	}

	private IVirtualSurfaceUpdatesCallbackNative callback;

	private EventHandler<EventArgs> updatesNeeded;

	public RawRectangle[] UpdateRectangles
	{
		get
		{
			int updateRectCount = GetUpdateRectCount();
			RawRectangle[] array = new RawRectangle[updateRectCount];
			GetUpdateRects(array, updateRectCount);
			return array;
		}
	}

	public RawRectangle VisibleBounds
	{
		get
		{
			GetVisibleBounds(out var bounds);
			return bounds;
		}
	}

	public event EventHandler<EventArgs> UpdatesNeeded
	{
		add
		{
			if (callback == null)
			{
				callback = new VirtualSurfaceUpdatesCallbackNativeCallback(this);
				RegisterForUpdatesNeeded(callback);
			}
			updatesNeeded = (EventHandler<EventArgs>)Delegate.Combine(updatesNeeded, value);
		}
		remove
		{
			updatesNeeded = (EventHandler<EventArgs>)Delegate.Remove(updatesNeeded, value);
		}
	}

	private void OnUpdatesNeeded()
	{
		if (updatesNeeded != null)
		{
			updatesNeeded(this, EventArgs.Empty);
		}
	}

	public IVirtualSurfaceImageSourceNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator IVirtualSurfaceImageSourceNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new IVirtualSurfaceImageSourceNative(nativePtr);
		}
		return null;
	}

	public unsafe void Invalidate(RawRectangle updateRect)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, RawRectangle, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, updateRect)).CheckError();
	}

	internal unsafe int GetUpdateRectCount()
	{
		int result = default(int);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &result)).CheckError();
		return result;
	}

	internal unsafe void GetUpdateRects(RawRectangle[] updates, int count)
	{
		Result result;
		fixed (RawRectangle* ptr = updates)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2, count);
		}
		result.CheckError();
	}

	internal unsafe void GetVisibleBounds(out RawRectangle bounds)
	{
		bounds = default(RawRectangle);
		Result result;
		fixed (RawRectangle* ptr = &bounds)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void RegisterForUpdatesNeeded(IVirtualSurfaceUpdatesCallbackNative callback)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IVirtualSurfaceUpdatesCallbackNative>(callback);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void Resize(int newWidth, int newHeight)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, newWidth, newHeight)).CheckError();
	}
}

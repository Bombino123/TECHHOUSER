using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[Guid("dc8e63f3-d12b-4952-b47b-5e45026a862d")]
public class Resource : DeviceChild
{
	public const int MaximumMipLevels = 15;

	public const int ResourceSizeInMegabytes = 128;

	public const int MaximumTexture1DArraySize = 2048;

	public const int MaximumTexture2DArraySize = 2048;

	public const int MaximumTexture1DSize = 16384;

	public const int MaximumTexture2DSize = 16384;

	public const int MaximumTexture3DSize = 2048;

	public const int MaximumTextureCubeSize = 16384;

	public ResourceDimension Dimension
	{
		get
		{
			GetDimension(out var resourceDimensionRef);
			return resourceDimensionRef;
		}
	}

	public int EvictionPriority
	{
		get
		{
			return GetEvictionPriority();
		}
		set
		{
			SetEvictionPriority(value);
		}
	}

	public static T FromSwapChain<T>(SwapChain swapChain, int index) where T : Resource
	{
		return swapChain.GetBackBuffer<T>(index);
	}

	public static int CalculateSubResourceIndex(int mipSlice, int arraySlice, int mipLevel)
	{
		return mipLevel * arraySlice + mipSlice;
	}

	public static int CalculateMipSize(int mipLevel, int baseSize)
	{
		baseSize >>= mipLevel;
		if (baseSize <= 0)
		{
			return 1;
		}
		return baseSize;
	}

	public virtual int CalculateSubResourceIndex(int mipSlice, int arraySlice, out int mipSize)
	{
		throw new NotImplementedException("This method is not implemented for this kind of resource");
	}

	public Resource(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Resource(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Resource(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDimension(out ResourceDimension resourceDimensionRef)
	{
		fixed (ResourceDimension* ptr = &resourceDimensionRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void SetEvictionPriority(int evictionPriority)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, evictionPriority);
	}

	internal unsafe int GetEvictionPriority()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}
}

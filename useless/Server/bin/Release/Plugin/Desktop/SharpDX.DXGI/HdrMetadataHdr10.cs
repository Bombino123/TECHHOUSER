using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

public struct HdrMetadataHdr10
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public short RedPrimary;

		public short __RedPrimary1;

		public short GreenPrimary;

		public short __GreenPrimary1;

		public short BluePrimary;

		public short __BluePrimary1;

		public short WhitePoint;

		public short __WhitePoint1;

		public int MaxMasteringLuminance;

		public int MinMasteringLuminance;

		public short MaxContentLightLevel;

		public short MaxFrameAverageLightLevel;
	}

	internal short[] _RedPrimary;

	internal short[] _GreenPrimary;

	internal short[] _BluePrimary;

	internal short[] _WhitePoint;

	public int MaxMasteringLuminance;

	public int MinMasteringLuminance;

	public short MaxContentLightLevel;

	public short MaxFrameAverageLightLevel;

	public short[] RedPrimary
	{
		get
		{
			return _RedPrimary ?? (_RedPrimary = new short[2]);
		}
		private set
		{
			_RedPrimary = value;
		}
	}

	public short[] GreenPrimary
	{
		get
		{
			return _GreenPrimary ?? (_GreenPrimary = new short[2]);
		}
		private set
		{
			_GreenPrimary = value;
		}
	}

	public short[] BluePrimary
	{
		get
		{
			return _BluePrimary ?? (_BluePrimary = new short[2]);
		}
		private set
		{
			_BluePrimary = value;
		}
	}

	public short[] WhitePoint
	{
		get
		{
			return _WhitePoint ?? (_WhitePoint = new short[2]);
		}
		private set
		{
			_WhitePoint = value;
		}
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (short* ptr = &RedPrimary[0])
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = &@ref.RedPrimary)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr4, 4);
			}
		}
		fixed (short* ptr3 = &GreenPrimary[0])
		{
			void* ptr5 = ptr3;
			fixed (short* ptr = &@ref.GreenPrimary)
			{
				void* ptr6 = ptr;
				Utilities.CopyMemory((IntPtr)ptr5, (IntPtr)ptr6, 4);
			}
		}
		fixed (short* ptr = &BluePrimary[0])
		{
			void* ptr7 = ptr;
			fixed (short* ptr3 = &@ref.BluePrimary)
			{
				void* ptr8 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr7, (IntPtr)ptr8, 4);
			}
		}
		fixed (short* ptr3 = &WhitePoint[0])
		{
			void* ptr9 = ptr3;
			fixed (short* ptr = &@ref.WhitePoint)
			{
				void* ptr10 = ptr;
				Utilities.CopyMemory((IntPtr)ptr9, (IntPtr)ptr10, 4);
			}
		}
		MaxMasteringLuminance = @ref.MaxMasteringLuminance;
		MinMasteringLuminance = @ref.MinMasteringLuminance;
		MaxContentLightLevel = @ref.MaxContentLightLevel;
		MaxFrameAverageLightLevel = @ref.MaxFrameAverageLightLevel;
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (short* ptr = &RedPrimary[0])
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = &@ref.RedPrimary)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr4, (IntPtr)ptr2, 4);
			}
		}
		fixed (short* ptr3 = &GreenPrimary[0])
		{
			void* ptr5 = ptr3;
			fixed (short* ptr = &@ref.GreenPrimary)
			{
				void* ptr6 = ptr;
				Utilities.CopyMemory((IntPtr)ptr6, (IntPtr)ptr5, 4);
			}
		}
		fixed (short* ptr = &BluePrimary[0])
		{
			void* ptr7 = ptr;
			fixed (short* ptr3 = &@ref.BluePrimary)
			{
				void* ptr8 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr8, (IntPtr)ptr7, 4);
			}
		}
		fixed (short* ptr3 = &WhitePoint[0])
		{
			void* ptr9 = ptr3;
			fixed (short* ptr = &@ref.WhitePoint)
			{
				void* ptr10 = ptr;
				Utilities.CopyMemory((IntPtr)ptr10, (IntPtr)ptr9, 4);
			}
		}
		@ref.MaxMasteringLuminance = MaxMasteringLuminance;
		@ref.MinMasteringLuminance = MinMasteringLuminance;
		@ref.MaxContentLightLevel = MaxContentLightLevel;
		@ref.MaxFrameAverageLightLevel = MaxFrameAverageLightLevel;
	}
}

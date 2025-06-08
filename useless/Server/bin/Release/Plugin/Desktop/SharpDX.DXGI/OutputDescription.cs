using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

public struct OutputDescription
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public char DeviceName;

		public char __DeviceName1;

		public char __DeviceName2;

		public char __DeviceName3;

		public char __DeviceName4;

		public char __DeviceName5;

		public char __DeviceName6;

		public char __DeviceName7;

		public char __DeviceName8;

		public char __DeviceName9;

		public char __DeviceName10;

		public char __DeviceName11;

		public char __DeviceName12;

		public char __DeviceName13;

		public char __DeviceName14;

		public char __DeviceName15;

		public char __DeviceName16;

		public char __DeviceName17;

		public char __DeviceName18;

		public char __DeviceName19;

		public char __DeviceName20;

		public char __DeviceName21;

		public char __DeviceName22;

		public char __DeviceName23;

		public char __DeviceName24;

		public char __DeviceName25;

		public char __DeviceName26;

		public char __DeviceName27;

		public char __DeviceName28;

		public char __DeviceName29;

		public char __DeviceName30;

		public char __DeviceName31;

		public RawRectangle DesktopBounds;

		public RawBool IsAttachedToDesktop;

		public DisplayModeRotation Rotation;

		public IntPtr MonitorHandle;
	}

	public string DeviceName;

	public RawRectangle DesktopBounds;

	public RawBool IsAttachedToDesktop;

	public DisplayModeRotation Rotation;

	public IntPtr MonitorHandle;

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (char* ptr = &@ref.DeviceName)
		{
			void* ptr2 = ptr;
			DeviceName = Utilities.PtrToStringUni((IntPtr)ptr2, 31);
		}
		DesktopBounds = @ref.DesktopBounds;
		IsAttachedToDesktop = @ref.IsAttachedToDesktop;
		Rotation = @ref.Rotation;
		MonitorHandle = @ref.MonitorHandle;
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (char* ptr2 = DeviceName)
		{
			fixed (char* ptr = &@ref.DeviceName)
			{
				int num = Math.Min((DeviceName?.Length ?? 0) * 2, 62);
				Utilities.CopyMemory((IntPtr)ptr, (IntPtr)ptr2, num);
				ptr[num] = '\0';
			}
		}
		@ref.DesktopBounds = DesktopBounds;
		@ref.IsAttachedToDesktop = IsAttachedToDesktop;
		@ref.Rotation = Rotation;
		@ref.MonitorHandle = MonitorHandle;
	}
}

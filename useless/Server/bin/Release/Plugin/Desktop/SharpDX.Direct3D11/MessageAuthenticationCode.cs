using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct MessageAuthenticationCode
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public byte Buffer;

		public byte __Buffer1;

		public byte __Buffer2;

		public byte __Buffer3;

		public byte __Buffer4;

		public byte __Buffer5;

		public byte __Buffer6;

		public byte __Buffer7;

		public byte __Buffer8;

		public byte __Buffer9;

		public byte __Buffer10;

		public byte __Buffer11;

		public byte __Buffer12;

		public byte __Buffer13;

		public byte __Buffer14;

		public byte __Buffer15;
	}

	internal byte[] _Buffer;

	public byte[] Buffer
	{
		get
		{
			return _Buffer ?? (_Buffer = new byte[16]);
		}
		private set
		{
			_Buffer = value;
		}
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (byte* ptr = &Buffer[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.Buffer)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr4, 16);
			}
		}
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (byte* ptr = &Buffer[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.Buffer)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr4, (IntPtr)ptr2, 16);
			}
		}
	}
}

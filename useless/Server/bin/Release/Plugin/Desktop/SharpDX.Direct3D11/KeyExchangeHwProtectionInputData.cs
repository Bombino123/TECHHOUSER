using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct KeyExchangeHwProtectionInputData
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public int PrivateDataSize;

		public int HWProtectionDataSize;

		public byte PbInput;

		public byte __PbInput1;

		public byte __PbInput2;

		public byte __PbInput3;
	}

	public int PrivateDataSize;

	public int HWProtectionDataSize;

	internal byte[] _PbInput;

	public byte[] PbInput
	{
		get
		{
			return _PbInput ?? (_PbInput = new byte[4]);
		}
		private set
		{
			_PbInput = value;
		}
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		PrivateDataSize = @ref.PrivateDataSize;
		HWProtectionDataSize = @ref.HWProtectionDataSize;
		fixed (byte* ptr = &PbInput[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.PbInput)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr4, 4);
			}
		}
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		@ref.PrivateDataSize = PrivateDataSize;
		@ref.HWProtectionDataSize = HWProtectionDataSize;
		fixed (byte* ptr = &PbInput[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.PbInput)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr4, (IntPtr)ptr2, 4);
			}
		}
	}
}

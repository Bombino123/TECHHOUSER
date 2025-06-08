using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct KeyExchangeHwProtectionOutputData
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public int PrivateDataSize;

		public int MaxHWProtectionDataSize;

		public int HWProtectionDataSize;

		public long TransportTime;

		public long ExecutionTime;

		public byte PbOutput;

		public byte __PbOutput1;

		public byte __PbOutput2;

		public byte __PbOutput3;
	}

	public int PrivateDataSize;

	public int MaxHWProtectionDataSize;

	public int HWProtectionDataSize;

	public long TransportTime;

	public long ExecutionTime;

	internal byte[] _PbOutput;

	public byte[] PbOutput
	{
		get
		{
			return _PbOutput ?? (_PbOutput = new byte[4]);
		}
		private set
		{
			_PbOutput = value;
		}
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		PrivateDataSize = @ref.PrivateDataSize;
		MaxHWProtectionDataSize = @ref.MaxHWProtectionDataSize;
		HWProtectionDataSize = @ref.HWProtectionDataSize;
		TransportTime = @ref.TransportTime;
		ExecutionTime = @ref.ExecutionTime;
		fixed (byte* ptr = &PbOutput[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.PbOutput)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr4, 4);
			}
		}
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		@ref.PrivateDataSize = PrivateDataSize;
		@ref.MaxHWProtectionDataSize = MaxHWProtectionDataSize;
		@ref.HWProtectionDataSize = HWProtectionDataSize;
		@ref.TransportTime = TransportTime;
		@ref.ExecutionTime = ExecutionTime;
		fixed (byte* ptr = &PbOutput[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.PbOutput)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr4, (IntPtr)ptr2, 4);
			}
		}
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

public struct JpegDeviceContextHuffmanTable
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public byte CodeCounts;

		public byte __CodeCounts1;

		public byte __CodeCounts2;

		public byte __CodeCounts3;

		public byte __CodeCounts4;

		public byte __CodeCounts5;

		public byte __CodeCounts6;

		public byte __CodeCounts7;

		public byte __CodeCounts8;

		public byte __CodeCounts9;

		public byte __CodeCounts10;

		public byte __CodeCounts11;

		public byte CodeValues;

		public byte __CodeValues1;

		public byte __CodeValues2;

		public byte __CodeValues3;

		public byte __CodeValues4;

		public byte __CodeValues5;

		public byte __CodeValues6;

		public byte __CodeValues7;

		public byte __CodeValues8;

		public byte __CodeValues9;

		public byte __CodeValues10;

		public byte __CodeValues11;
	}

	internal byte[] _CodeCounts;

	internal byte[] _CodeValues;

	public byte[] CodeCounts
	{
		get
		{
			return _CodeCounts ?? (_CodeCounts = new byte[12]);
		}
		private set
		{
			_CodeCounts = value;
		}
	}

	public byte[] CodeValues
	{
		get
		{
			return _CodeValues ?? (_CodeValues = new byte[12]);
		}
		private set
		{
			_CodeValues = value;
		}
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (byte* ptr = &CodeCounts[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.CodeCounts)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr4, 12);
			}
		}
		fixed (byte* ptr3 = &CodeValues[0])
		{
			void* ptr5 = ptr3;
			fixed (byte* ptr = &@ref.CodeValues)
			{
				void* ptr6 = ptr;
				Utilities.CopyMemory((IntPtr)ptr5, (IntPtr)ptr6, 12);
			}
		}
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (byte* ptr = &CodeCounts[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.CodeCounts)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr4, (IntPtr)ptr2, 12);
			}
		}
		fixed (byte* ptr3 = &CodeValues[0])
		{
			void* ptr5 = ptr3;
			fixed (byte* ptr = &@ref.CodeValues)
			{
				void* ptr6 = ptr;
				Utilities.CopyMemory((IntPtr)ptr6, (IntPtr)ptr5, 12);
			}
		}
	}
}

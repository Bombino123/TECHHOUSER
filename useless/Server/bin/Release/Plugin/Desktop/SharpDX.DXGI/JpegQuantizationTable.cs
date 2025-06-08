using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

public struct JpegQuantizationTable
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public byte Elements;

		public byte __Elements1;

		public byte __Elements2;

		public byte __Elements3;

		public byte __Elements4;

		public byte __Elements5;

		public byte __Elements6;

		public byte __Elements7;

		public byte __Elements8;

		public byte __Elements9;

		public byte __Elements10;

		public byte __Elements11;

		public byte __Elements12;

		public byte __Elements13;

		public byte __Elements14;

		public byte __Elements15;

		public byte __Elements16;

		public byte __Elements17;

		public byte __Elements18;

		public byte __Elements19;

		public byte __Elements20;

		public byte __Elements21;

		public byte __Elements22;

		public byte __Elements23;

		public byte __Elements24;

		public byte __Elements25;

		public byte __Elements26;

		public byte __Elements27;

		public byte __Elements28;

		public byte __Elements29;

		public byte __Elements30;

		public byte __Elements31;

		public byte __Elements32;

		public byte __Elements33;

		public byte __Elements34;

		public byte __Elements35;

		public byte __Elements36;

		public byte __Elements37;

		public byte __Elements38;

		public byte __Elements39;

		public byte __Elements40;

		public byte __Elements41;

		public byte __Elements42;

		public byte __Elements43;

		public byte __Elements44;

		public byte __Elements45;

		public byte __Elements46;

		public byte __Elements47;

		public byte __Elements48;

		public byte __Elements49;

		public byte __Elements50;

		public byte __Elements51;

		public byte __Elements52;

		public byte __Elements53;

		public byte __Elements54;

		public byte __Elements55;

		public byte __Elements56;

		public byte __Elements57;

		public byte __Elements58;

		public byte __Elements59;

		public byte __Elements60;

		public byte __Elements61;

		public byte __Elements62;

		public byte __Elements63;
	}

	internal byte[] _Elements;

	public byte[] Elements
	{
		get
		{
			return _Elements ?? (_Elements = new byte[64]);
		}
		private set
		{
			_Elements = value;
		}
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (byte* ptr = &Elements[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.Elements)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr4, 64);
			}
		}
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (byte* ptr = &Elements[0])
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = &@ref.Elements)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr4, (IntPtr)ptr2, 64);
			}
		}
	}
}

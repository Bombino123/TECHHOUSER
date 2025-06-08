using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

public struct JpegAcHuffmanTable
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

		public byte __CodeCounts12;

		public byte __CodeCounts13;

		public byte __CodeCounts14;

		public byte __CodeCounts15;

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

		public byte __CodeValues12;

		public byte __CodeValues13;

		public byte __CodeValues14;

		public byte __CodeValues15;

		public byte __CodeValues16;

		public byte __CodeValues17;

		public byte __CodeValues18;

		public byte __CodeValues19;

		public byte __CodeValues20;

		public byte __CodeValues21;

		public byte __CodeValues22;

		public byte __CodeValues23;

		public byte __CodeValues24;

		public byte __CodeValues25;

		public byte __CodeValues26;

		public byte __CodeValues27;

		public byte __CodeValues28;

		public byte __CodeValues29;

		public byte __CodeValues30;

		public byte __CodeValues31;

		public byte __CodeValues32;

		public byte __CodeValues33;

		public byte __CodeValues34;

		public byte __CodeValues35;

		public byte __CodeValues36;

		public byte __CodeValues37;

		public byte __CodeValues38;

		public byte __CodeValues39;

		public byte __CodeValues40;

		public byte __CodeValues41;

		public byte __CodeValues42;

		public byte __CodeValues43;

		public byte __CodeValues44;

		public byte __CodeValues45;

		public byte __CodeValues46;

		public byte __CodeValues47;

		public byte __CodeValues48;

		public byte __CodeValues49;

		public byte __CodeValues50;

		public byte __CodeValues51;

		public byte __CodeValues52;

		public byte __CodeValues53;

		public byte __CodeValues54;

		public byte __CodeValues55;

		public byte __CodeValues56;

		public byte __CodeValues57;

		public byte __CodeValues58;

		public byte __CodeValues59;

		public byte __CodeValues60;

		public byte __CodeValues61;

		public byte __CodeValues62;

		public byte __CodeValues63;

		public byte __CodeValues64;

		public byte __CodeValues65;

		public byte __CodeValues66;

		public byte __CodeValues67;

		public byte __CodeValues68;

		public byte __CodeValues69;

		public byte __CodeValues70;

		public byte __CodeValues71;

		public byte __CodeValues72;

		public byte __CodeValues73;

		public byte __CodeValues74;

		public byte __CodeValues75;

		public byte __CodeValues76;

		public byte __CodeValues77;

		public byte __CodeValues78;

		public byte __CodeValues79;

		public byte __CodeValues80;

		public byte __CodeValues81;

		public byte __CodeValues82;

		public byte __CodeValues83;

		public byte __CodeValues84;

		public byte __CodeValues85;

		public byte __CodeValues86;

		public byte __CodeValues87;

		public byte __CodeValues88;

		public byte __CodeValues89;

		public byte __CodeValues90;

		public byte __CodeValues91;

		public byte __CodeValues92;

		public byte __CodeValues93;

		public byte __CodeValues94;

		public byte __CodeValues95;

		public byte __CodeValues96;

		public byte __CodeValues97;

		public byte __CodeValues98;

		public byte __CodeValues99;

		public byte __CodeValues100;

		public byte __CodeValues101;

		public byte __CodeValues102;

		public byte __CodeValues103;

		public byte __CodeValues104;

		public byte __CodeValues105;

		public byte __CodeValues106;

		public byte __CodeValues107;

		public byte __CodeValues108;

		public byte __CodeValues109;

		public byte __CodeValues110;

		public byte __CodeValues111;

		public byte __CodeValues112;

		public byte __CodeValues113;

		public byte __CodeValues114;

		public byte __CodeValues115;

		public byte __CodeValues116;

		public byte __CodeValues117;

		public byte __CodeValues118;

		public byte __CodeValues119;

		public byte __CodeValues120;

		public byte __CodeValues121;

		public byte __CodeValues122;

		public byte __CodeValues123;

		public byte __CodeValues124;

		public byte __CodeValues125;

		public byte __CodeValues126;

		public byte __CodeValues127;

		public byte __CodeValues128;

		public byte __CodeValues129;

		public byte __CodeValues130;

		public byte __CodeValues131;

		public byte __CodeValues132;

		public byte __CodeValues133;

		public byte __CodeValues134;

		public byte __CodeValues135;

		public byte __CodeValues136;

		public byte __CodeValues137;

		public byte __CodeValues138;

		public byte __CodeValues139;

		public byte __CodeValues140;

		public byte __CodeValues141;

		public byte __CodeValues142;

		public byte __CodeValues143;

		public byte __CodeValues144;

		public byte __CodeValues145;

		public byte __CodeValues146;

		public byte __CodeValues147;

		public byte __CodeValues148;

		public byte __CodeValues149;

		public byte __CodeValues150;

		public byte __CodeValues151;

		public byte __CodeValues152;

		public byte __CodeValues153;

		public byte __CodeValues154;

		public byte __CodeValues155;

		public byte __CodeValues156;

		public byte __CodeValues157;

		public byte __CodeValues158;

		public byte __CodeValues159;

		public byte __CodeValues160;

		public byte __CodeValues161;
	}

	internal byte[] _CodeCounts;

	internal byte[] _CodeValues;

	public byte[] CodeCounts
	{
		get
		{
			return _CodeCounts ?? (_CodeCounts = new byte[16]);
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
			return _CodeValues ?? (_CodeValues = new byte[162]);
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
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr4, 16);
			}
		}
		fixed (byte* ptr3 = &CodeValues[0])
		{
			void* ptr5 = ptr3;
			fixed (byte* ptr = &@ref.CodeValues)
			{
				void* ptr6 = ptr;
				Utilities.CopyMemory((IntPtr)ptr5, (IntPtr)ptr6, 162);
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
				Utilities.CopyMemory((IntPtr)ptr4, (IntPtr)ptr2, 16);
			}
		}
		fixed (byte* ptr3 = &CodeValues[0])
		{
			void* ptr5 = ptr3;
			fixed (byte* ptr = &@ref.CodeValues)
			{
				void* ptr6 = ptr;
				Utilities.CopyMemory((IntPtr)ptr6, (IntPtr)ptr5, 162);
			}
		}
	}
}

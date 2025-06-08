using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

public struct AdapterDescription
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
	internal struct __Native
	{
		public char Description;

		public char __Description1;

		public char __Description2;

		public char __Description3;

		public char __Description4;

		public char __Description5;

		public char __Description6;

		public char __Description7;

		public char __Description8;

		public char __Description9;

		public char __Description10;

		public char __Description11;

		public char __Description12;

		public char __Description13;

		public char __Description14;

		public char __Description15;

		public char __Description16;

		public char __Description17;

		public char __Description18;

		public char __Description19;

		public char __Description20;

		public char __Description21;

		public char __Description22;

		public char __Description23;

		public char __Description24;

		public char __Description25;

		public char __Description26;

		public char __Description27;

		public char __Description28;

		public char __Description29;

		public char __Description30;

		public char __Description31;

		public char __Description32;

		public char __Description33;

		public char __Description34;

		public char __Description35;

		public char __Description36;

		public char __Description37;

		public char __Description38;

		public char __Description39;

		public char __Description40;

		public char __Description41;

		public char __Description42;

		public char __Description43;

		public char __Description44;

		public char __Description45;

		public char __Description46;

		public char __Description47;

		public char __Description48;

		public char __Description49;

		public char __Description50;

		public char __Description51;

		public char __Description52;

		public char __Description53;

		public char __Description54;

		public char __Description55;

		public char __Description56;

		public char __Description57;

		public char __Description58;

		public char __Description59;

		public char __Description60;

		public char __Description61;

		public char __Description62;

		public char __Description63;

		public char __Description64;

		public char __Description65;

		public char __Description66;

		public char __Description67;

		public char __Description68;

		public char __Description69;

		public char __Description70;

		public char __Description71;

		public char __Description72;

		public char __Description73;

		public char __Description74;

		public char __Description75;

		public char __Description76;

		public char __Description77;

		public char __Description78;

		public char __Description79;

		public char __Description80;

		public char __Description81;

		public char __Description82;

		public char __Description83;

		public char __Description84;

		public char __Description85;

		public char __Description86;

		public char __Description87;

		public char __Description88;

		public char __Description89;

		public char __Description90;

		public char __Description91;

		public char __Description92;

		public char __Description93;

		public char __Description94;

		public char __Description95;

		public char __Description96;

		public char __Description97;

		public char __Description98;

		public char __Description99;

		public char __Description100;

		public char __Description101;

		public char __Description102;

		public char __Description103;

		public char __Description104;

		public char __Description105;

		public char __Description106;

		public char __Description107;

		public char __Description108;

		public char __Description109;

		public char __Description110;

		public char __Description111;

		public char __Description112;

		public char __Description113;

		public char __Description114;

		public char __Description115;

		public char __Description116;

		public char __Description117;

		public char __Description118;

		public char __Description119;

		public char __Description120;

		public char __Description121;

		public char __Description122;

		public char __Description123;

		public char __Description124;

		public char __Description125;

		public char __Description126;

		public char __Description127;

		public int VendorId;

		public int DeviceId;

		public int SubsystemId;

		public int Revision;

		public IntPtr DedicatedVideoMemory;

		public IntPtr DedicatedSystemMemory;

		public IntPtr SharedSystemMemory;

		public long Luid;
	}

	public string Description;

	public int VendorId;

	public int DeviceId;

	public int SubsystemId;

	public int Revision;

	public PointerSize DedicatedVideoMemory;

	public PointerSize DedicatedSystemMemory;

	public PointerSize SharedSystemMemory;

	public long Luid;

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (char* ptr = &@ref.Description)
		{
			void* ptr2 = ptr;
			Description = Utilities.PtrToStringUni((IntPtr)ptr2, 127);
		}
		VendorId = @ref.VendorId;
		DeviceId = @ref.DeviceId;
		SubsystemId = @ref.SubsystemId;
		Revision = @ref.Revision;
		DedicatedVideoMemory = @ref.DedicatedVideoMemory;
		DedicatedSystemMemory = @ref.DedicatedSystemMemory;
		SharedSystemMemory = @ref.SharedSystemMemory;
		Luid = @ref.Luid;
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (char* ptr2 = Description)
		{
			fixed (char* ptr = &@ref.Description)
			{
				int num = Math.Min((Description?.Length ?? 0) * 2, 254);
				Utilities.CopyMemory((IntPtr)ptr, (IntPtr)ptr2, num);
				ptr[num] = '\0';
			}
		}
		@ref.VendorId = VendorId;
		@ref.DeviceId = DeviceId;
		@ref.SubsystemId = SubsystemId;
		@ref.Revision = Revision;
		@ref.DedicatedVideoMemory = DedicatedVideoMemory;
		@ref.DedicatedSystemMemory = DedicatedSystemMemory;
		@ref.SharedSystemMemory = SharedSystemMemory;
		@ref.Luid = Luid;
	}
}

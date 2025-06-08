using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

public struct BlendStateDescription1
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public RawBool AlphaToCoverageEnable;

		public RawBool IndependentBlendEnable;

		public RenderTargetBlendDescription1 RenderTarget;

		public RenderTargetBlendDescription1 __RenderTarget1;

		public RenderTargetBlendDescription1 __RenderTarget2;

		public RenderTargetBlendDescription1 __RenderTarget3;

		public RenderTargetBlendDescription1 __RenderTarget4;

		public RenderTargetBlendDescription1 __RenderTarget5;

		public RenderTargetBlendDescription1 __RenderTarget6;

		public RenderTargetBlendDescription1 __RenderTarget7;
	}

	public RawBool AlphaToCoverageEnable;

	public RawBool IndependentBlendEnable;

	internal RenderTargetBlendDescription1[] _RenderTarget;

	public RenderTargetBlendDescription1[] RenderTarget
	{
		get
		{
			return _RenderTarget ?? (_RenderTarget = new RenderTargetBlendDescription1[8]);
		}
		private set
		{
			_RenderTarget = value;
		}
	}

	public static BlendStateDescription1 Default()
	{
		BlendStateDescription1 blendStateDescription = default(BlendStateDescription1);
		blendStateDescription.AlphaToCoverageEnable = false;
		blendStateDescription.IndependentBlendEnable = false;
		BlendStateDescription1 result = blendStateDescription;
		RenderTargetBlendDescription1[] renderTarget = result.RenderTarget;
		for (int i = 0; i < renderTarget.Length; i++)
		{
			renderTarget[i].IsBlendEnabled = false;
			renderTarget[i].IsLogicOperationEnabled = false;
			renderTarget[i].SourceBlend = BlendOption.One;
			renderTarget[i].DestinationBlend = BlendOption.Zero;
			renderTarget[i].BlendOperation = BlendOperation.Add;
			renderTarget[i].SourceAlphaBlend = BlendOption.One;
			renderTarget[i].DestinationAlphaBlend = BlendOption.Zero;
			renderTarget[i].AlphaBlendOperation = BlendOperation.Add;
			renderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
			renderTarget[i].LogicOperation = LogicOperation.Noop;
		}
		return result;
	}

	public BlendStateDescription1 Clone()
	{
		BlendStateDescription1 blendStateDescription = default(BlendStateDescription1);
		blendStateDescription.AlphaToCoverageEnable = AlphaToCoverageEnable;
		blendStateDescription.IndependentBlendEnable = IndependentBlendEnable;
		BlendStateDescription1 result = blendStateDescription;
		RenderTargetBlendDescription1[] renderTarget = RenderTarget;
		RenderTargetBlendDescription1[] renderTarget2 = result.RenderTarget;
		for (int i = 0; i < renderTarget.Length; i++)
		{
			renderTarget2[i] = renderTarget[i];
		}
		return result;
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		AlphaToCoverageEnable = @ref.AlphaToCoverageEnable;
		IndependentBlendEnable = @ref.IndependentBlendEnable;
		fixed (RenderTargetBlendDescription1* ptr = &RenderTarget[0])
		{
			void* ptr2 = ptr;
			fixed (RenderTargetBlendDescription1* ptr3 = &@ref.RenderTarget)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr4, 8 * sizeof(RenderTargetBlendDescription1));
			}
		}
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		@ref.AlphaToCoverageEnable = AlphaToCoverageEnable;
		@ref.IndependentBlendEnable = IndependentBlendEnable;
		fixed (RenderTargetBlendDescription1* ptr = &RenderTarget[0])
		{
			void* ptr2 = ptr;
			fixed (RenderTargetBlendDescription1* ptr3 = &@ref.RenderTarget)
			{
				void* ptr4 = ptr3;
				Utilities.CopyMemory((IntPtr)ptr4, (IntPtr)ptr2, 8 * sizeof(RenderTargetBlendDescription1));
			}
		}
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct VideoDecoderExtension
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public int Function;

		public IntPtr PPrivateInputData;

		public int PrivateInputDataSize;

		public IntPtr PPrivateOutputData;

		public int PrivateOutputDataSize;

		public int ResourceCount;

		public IntPtr PpResourceList;
	}

	public int Function;

	public IntPtr PPrivateInputData;

	public int PrivateInputDataSize;

	public IntPtr PPrivateOutputData;

	public int PrivateOutputDataSize;

	public int ResourceCount;

	public Resource PpResourceList;

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Function = @ref.Function;
		PPrivateInputData = @ref.PPrivateInputData;
		PrivateInputDataSize = @ref.PrivateInputDataSize;
		PPrivateOutputData = @ref.PPrivateOutputData;
		PrivateOutputDataSize = @ref.PrivateOutputDataSize;
		ResourceCount = @ref.ResourceCount;
		if (@ref.PpResourceList != IntPtr.Zero)
		{
			PpResourceList = new Resource(@ref.PpResourceList);
		}
		else
		{
			PpResourceList = null;
		}
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.Function = Function;
		@ref.PPrivateInputData = PPrivateInputData;
		@ref.PrivateInputDataSize = PrivateInputDataSize;
		@ref.PPrivateOutputData = PPrivateOutputData;
		@ref.PrivateOutputDataSize = PrivateOutputDataSize;
		@ref.ResourceCount = ResourceCount;
		@ref.PpResourceList = CppObject.ToCallbackPtr<Resource>(PpResourceList);
	}
}

using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

public struct VideoProcessorStream
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public RawBool Enable;

		public int OutputIndex;

		public int InputFrameOrField;

		public int PastFrames;

		public int FutureFrames;

		public IntPtr PpPastSurfaces;

		public IntPtr PInputSurface;

		public IntPtr PpFutureSurfaces;

		public IntPtr PpPastSurfacesRight;

		public IntPtr PInputSurfaceRight;

		public IntPtr PpFutureSurfacesRight;
	}

	public RawBool Enable;

	public int OutputIndex;

	public int InputFrameOrField;

	public int PastFrames;

	public int FutureFrames;

	public VideoProcessorInputView PpPastSurfaces;

	public VideoProcessorInputView PInputSurface;

	public VideoProcessorInputView PpFutureSurfaces;

	public VideoProcessorInputView PpPastSurfacesRight;

	public VideoProcessorInputView PInputSurfaceRight;

	public VideoProcessorInputView PpFutureSurfacesRight;

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Enable = @ref.Enable;
		OutputIndex = @ref.OutputIndex;
		InputFrameOrField = @ref.InputFrameOrField;
		PastFrames = @ref.PastFrames;
		FutureFrames = @ref.FutureFrames;
		if (@ref.PpPastSurfaces != IntPtr.Zero)
		{
			PpPastSurfaces = new VideoProcessorInputView(@ref.PpPastSurfaces);
		}
		else
		{
			PpPastSurfaces = null;
		}
		if (@ref.PInputSurface != IntPtr.Zero)
		{
			PInputSurface = new VideoProcessorInputView(@ref.PInputSurface);
		}
		else
		{
			PInputSurface = null;
		}
		if (@ref.PpFutureSurfaces != IntPtr.Zero)
		{
			PpFutureSurfaces = new VideoProcessorInputView(@ref.PpFutureSurfaces);
		}
		else
		{
			PpFutureSurfaces = null;
		}
		if (@ref.PpPastSurfacesRight != IntPtr.Zero)
		{
			PpPastSurfacesRight = new VideoProcessorInputView(@ref.PpPastSurfacesRight);
		}
		else
		{
			PpPastSurfacesRight = null;
		}
		if (@ref.PInputSurfaceRight != IntPtr.Zero)
		{
			PInputSurfaceRight = new VideoProcessorInputView(@ref.PInputSurfaceRight);
		}
		else
		{
			PInputSurfaceRight = null;
		}
		if (@ref.PpFutureSurfacesRight != IntPtr.Zero)
		{
			PpFutureSurfacesRight = new VideoProcessorInputView(@ref.PpFutureSurfacesRight);
		}
		else
		{
			PpFutureSurfacesRight = null;
		}
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.Enable = Enable;
		@ref.OutputIndex = OutputIndex;
		@ref.InputFrameOrField = InputFrameOrField;
		@ref.PastFrames = PastFrames;
		@ref.FutureFrames = FutureFrames;
		@ref.PpPastSurfaces = CppObject.ToCallbackPtr<VideoProcessorInputView>(PpPastSurfaces);
		@ref.PInputSurface = CppObject.ToCallbackPtr<VideoProcessorInputView>(PInputSurface);
		@ref.PpFutureSurfaces = CppObject.ToCallbackPtr<VideoProcessorInputView>(PpFutureSurfaces);
		@ref.PpPastSurfacesRight = CppObject.ToCallbackPtr<VideoProcessorInputView>(PpPastSurfacesRight);
		@ref.PInputSurfaceRight = CppObject.ToCallbackPtr<VideoProcessorInputView>(PInputSurfaceRight);
		@ref.PpFutureSurfacesRight = CppObject.ToCallbackPtr<VideoProcessorInputView>(PpFutureSurfacesRight);
	}
}

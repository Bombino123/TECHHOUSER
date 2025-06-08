using System;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

public struct PresentParameters
{
	internal struct __Native
	{
		public int DirtyRectsCount;

		public IntPtr PDirtyRects;

		public IntPtr PScrollRect;

		public IntPtr PScrollOffset;
	}

	public RawRectangle[] DirtyRectangles;

	public RawRectangle? ScrollRectangle;

	public RawPoint? ScrollOffset;

	internal int DirtyRectsCount;

	internal IntPtr PDirtyRects;

	internal IntPtr PScrollRect;

	internal IntPtr PScrollOffset;
}

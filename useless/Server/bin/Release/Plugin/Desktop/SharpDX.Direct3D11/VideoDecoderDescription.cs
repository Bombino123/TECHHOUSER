using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoDecoderDescription
{
	public Guid Guid;

	public int SampleWidth;

	public int SampleHeight;

	public Format OutputFormat;
}

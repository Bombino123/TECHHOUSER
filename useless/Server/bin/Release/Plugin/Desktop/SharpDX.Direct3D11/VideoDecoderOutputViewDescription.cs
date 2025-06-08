using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoDecoderOutputViewDescription
{
	public Guid DecodeProfile;

	public VdovDimension Dimension;

	public Texture2DVdov Texture2D;
}

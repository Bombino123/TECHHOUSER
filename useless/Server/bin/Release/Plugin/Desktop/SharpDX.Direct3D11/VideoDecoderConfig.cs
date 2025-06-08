using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoDecoderConfig
{
	public Guid GuidConfigBitstreamEncryption;

	public Guid GuidConfigMBcontrolEncryption;

	public Guid GuidConfigResidDiffEncryption;

	public int ConfigBitstreamRaw;

	public int ConfigMBcontrolRasterOrder;

	public int ConfigResidDiffHost;

	public int ConfigSpatialResid8;

	public int ConfigResid8Subtraction;

	public int ConfigSpatialHost8or9Clipping;

	public int ConfigSpatialResidInterleaved;

	public int ConfigIntraResidUnsigned;

	public int ConfigResidDiffAccelerator;

	public int ConfigHostInverseScan;

	public int ConfigSpecificIDCT;

	public int Config4GroupedCoefs;

	public short ConfigMinRenderTargetBuffCount;

	public short ConfigDecoderSpecific;
}

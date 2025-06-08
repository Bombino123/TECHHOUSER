using System;

namespace SharpDX.DXGI;

[Flags]
public enum MultiplaneOverlayYCbCrFlags
{
	NominalRange = 1,
	Bt709 = 2,
	XvYCC = 4,
	None = 0
}

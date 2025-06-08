using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("98ECEE1E-752D-11d3-8D56-00C04F680B2B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPdbWriter
{
	void _VtblGap1_4();

	void GetSignatureAge(out uint sig, out uint age);
}

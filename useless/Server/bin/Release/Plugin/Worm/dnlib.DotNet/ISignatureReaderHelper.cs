using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface ISignatureReaderHelper
{
	ITypeDefOrRef ResolveTypeDefOrRef(uint codedToken, GenericParamContext gpContext);

	TypeSig ConvertRTInternalAddress(IntPtr address);
}

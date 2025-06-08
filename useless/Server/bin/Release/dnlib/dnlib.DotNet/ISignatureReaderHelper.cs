using System;

namespace dnlib.DotNet;

public interface ISignatureReaderHelper
{
	ITypeDefOrRef ResolveTypeDefOrRef(uint codedToken, GenericParamContext gpContext);

	TypeSig ConvertRTInternalAddress(IntPtr address);
}

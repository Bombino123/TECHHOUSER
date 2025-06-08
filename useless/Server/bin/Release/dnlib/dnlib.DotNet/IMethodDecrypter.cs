using System.Collections.Generic;
using dnlib.DotNet.Emit;
using dnlib.PE;

namespace dnlib.DotNet;

public interface IMethodDecrypter
{
	bool GetMethodBody(uint rid, RVA rva, IList<Parameter> parameters, GenericParamContext gpContext, out MethodBody methodBody);
}

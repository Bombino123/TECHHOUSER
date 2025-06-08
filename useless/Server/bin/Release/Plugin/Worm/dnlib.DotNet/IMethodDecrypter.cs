using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.Emit;
using dnlib.PE;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IMethodDecrypter
{
	bool GetMethodBody(uint rid, RVA rva, IList<Parameter> parameters, GenericParamContext gpContext, out MethodBody methodBody);
}

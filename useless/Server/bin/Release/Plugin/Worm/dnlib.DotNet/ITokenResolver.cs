using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface ITokenResolver
{
	IMDTokenProvider ResolveToken(uint token, GenericParamContext gpContext);
}

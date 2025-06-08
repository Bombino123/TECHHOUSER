namespace dnlib.DotNet;

public interface ITokenResolver
{
	IMDTokenProvider ResolveToken(uint token, GenericParamContext gpContext);
}

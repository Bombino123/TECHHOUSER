namespace dnlib.DotNet.Emit;

public interface IStringResolver
{
	string ReadUserString(uint token);
}

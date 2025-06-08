namespace dnlib.DotNet;

public interface IStringDecrypter
{
	string ReadUserString(uint token);
}

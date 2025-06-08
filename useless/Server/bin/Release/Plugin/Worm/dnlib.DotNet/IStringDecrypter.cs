using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IStringDecrypter
{
	string ReadUserString(uint token);
}

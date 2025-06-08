using System.Runtime.InteropServices;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public interface IStringResolver
{
	string ReadUserString(uint token);
}

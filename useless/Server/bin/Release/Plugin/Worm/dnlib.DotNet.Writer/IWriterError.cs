using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public interface IWriterError
{
	void Error(string message);
}

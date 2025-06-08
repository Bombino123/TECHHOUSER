using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public interface IWriterError2 : IWriterError
{
	void Error(string message, params object[] args);
}

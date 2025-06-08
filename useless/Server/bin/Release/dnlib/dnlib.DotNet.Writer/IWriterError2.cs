namespace dnlib.DotNet.Writer;

public interface IWriterError2 : IWriterError
{
	void Error(string message, params object[] args);
}

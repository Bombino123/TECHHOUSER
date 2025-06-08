namespace dnlib.DotNet;

public interface ILogger
{
	void Log(object sender, LoggerEvent loggerEvent, string format, params object[] args);

	bool IgnoresEvent(LoggerEvent loggerEvent);
}

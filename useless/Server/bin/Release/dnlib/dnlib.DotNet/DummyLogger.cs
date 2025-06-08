using System;
using System.Reflection;
using dnlib.DotNet.Writer;

namespace dnlib.DotNet;

public sealed class DummyLogger : ILogger
{
	private ConstructorInfo ctor;

	public static readonly DummyLogger NoThrowInstance = new DummyLogger();

	public static readonly DummyLogger ThrowModuleWriterExceptionOnErrorInstance = new DummyLogger(typeof(ModuleWriterException));

	private DummyLogger()
	{
	}

	public DummyLogger(Type exceptionToThrow)
	{
		if ((object)exceptionToThrow != null)
		{
			if (!exceptionToThrow.IsSubclassOf(typeof(Exception)))
			{
				throw new ArgumentException($"Not a System.Exception sub class: {exceptionToThrow.GetType()}");
			}
			ctor = exceptionToThrow.GetConstructor(new Type[1] { typeof(string) });
			if ((object)ctor == null)
			{
				throw new ArgumentException($"Exception type {exceptionToThrow.GetType()} doesn't have a public constructor that takes a string as the only argument");
			}
		}
	}

	public void Log(object sender, LoggerEvent loggerEvent, string format, params object[] args)
	{
		if (loggerEvent == LoggerEvent.Error && (object)ctor != null)
		{
			throw (Exception)ctor.Invoke(new object[1] { string.Format(format, args) });
		}
	}

	public bool IgnoresEvent(LoggerEvent loggerEvent)
	{
		if ((object)ctor == null)
		{
			return true;
		}
		return loggerEvent != LoggerEvent.Error;
	}
}

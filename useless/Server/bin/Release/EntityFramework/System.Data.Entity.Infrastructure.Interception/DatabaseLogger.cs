using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Utilities;
using System.IO;

namespace System.Data.Entity.Infrastructure.Interception;

public class DatabaseLogger : IDisposable, IDbConfigurationInterceptor, IDbInterceptor
{
	private TextWriter _writer;

	private DatabaseLogFormatter _formatter;

	private readonly object _lock = new object();

	public DatabaseLogger()
	{
	}

	public DatabaseLogger(string path)
		: this(path, append: false)
	{
	}

	public DatabaseLogger(string path, bool append)
	{
		Check.NotEmpty(path, "path");
		_writer = new StreamWriter(path, append)
		{
			AutoFlush = true
		};
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		StopLogging();
		if (disposing && _writer != null)
		{
			_writer.Dispose();
			_writer = null;
		}
	}

	public void StartLogging()
	{
		StartLogging(DbConfiguration.DependencyResolver);
	}

	public void StopLogging()
	{
		if (_formatter != null)
		{
			DbInterception.Remove(_formatter);
			_formatter = null;
		}
	}

	void IDbConfigurationInterceptor.Loaded(DbConfigurationLoadedEventArgs loadedEventArgs, DbConfigurationInterceptionContext interceptionContext)
	{
		Check.NotNull(loadedEventArgs, "loadedEventArgs");
		Check.NotNull(interceptionContext, "interceptionContext");
		StartLogging(loadedEventArgs.DependencyResolver);
	}

	private void StartLogging(IDbDependencyResolver resolver)
	{
		if (_formatter == null)
		{
			_formatter = resolver.GetService<Func<DbContext, Action<string>, DatabaseLogFormatter>>()(null, (_writer == null) ? new Action<string>(Console.Write) : new Action<string>(WriteThreadSafe));
			DbInterception.Add(_formatter);
		}
	}

	private void WriteThreadSafe(string value)
	{
		lock (_lock)
		{
			_writer.Write(value);
		}
	}
}

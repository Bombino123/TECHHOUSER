#define TRACE
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace System.Data.SQLite;

public static class SQLiteLog
{
	private const int _initializeTimeout = 1000;

	private static object syncRoot = new object();

	private static EventWaitHandle _initializeEvent;

	private static EventHandler _domainUnload;

	private static SQLiteLogEventHandler _defaultHandler;

	private static SQLiteLogCallback _callback;

	private static SQLiteBase _sql;

	private static int _initializeCallCount;

	private static int _uninitializeCallCount;

	private static int _initializeDoneCount;

	private static int _attemptedInitialize;

	private static bool _enabled;

	public static bool Enabled
	{
		get
		{
			lock (syncRoot)
			{
				return InternalEnabled;
			}
		}
		set
		{
			lock (syncRoot)
			{
				InternalEnabled = value;
			}
		}
	}

	internal static bool InternalEnabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
		}
	}

	private static event SQLiteLogEventHandler _handlers;

	public static event SQLiteLogEventHandler Log
	{
		add
		{
			lock (syncRoot)
			{
				_handlers -= value;
				_handlers += value;
			}
		}
		remove
		{
			lock (syncRoot)
			{
				_handlers -= value;
			}
		}
	}

	private static EventWaitHandle CreateAndOrGetTheEvent()
	{
		bool flag = false;
		EventWaitHandle eventWaitHandle = null;
		try
		{
			EventWaitHandle eventWaitHandle2 = Interlocked.CompareExchange(ref _initializeEvent, null, null);
			if (eventWaitHandle2 == null)
			{
				eventWaitHandle = new ManualResetEvent(initialState: false);
				eventWaitHandle2 = Interlocked.CompareExchange(ref _initializeEvent, eventWaitHandle, null);
			}
			if (eventWaitHandle2 == null)
			{
				eventWaitHandle2 = eventWaitHandle;
				flag = true;
			}
			return eventWaitHandle2;
		}
		finally
		{
			if (!flag && eventWaitHandle != null)
			{
				eventWaitHandle.Close();
				eventWaitHandle = null;
			}
		}
	}

	public static void Initialize()
	{
		Initialize(null);
	}

	internal static void Initialize(string className)
	{
		if (UnsafeNativeMethods.GetSettingValue("No_SQLiteLog", null) != null)
		{
			return;
		}
		EventWaitHandle eventWaitHandle = CreateAndOrGetTheEvent();
		if (Interlocked.Increment(ref _initializeDoneCount) == 1)
		{
			bool flag = false;
			try
			{
				flag = PrivateInitialize(className);
			}
			finally
			{
				eventWaitHandle?.Set();
				if (!flag)
				{
					Interlocked.Decrement(ref _initializeDoneCount);
				}
			}
		}
		else
		{
			Interlocked.Decrement(ref _initializeDoneCount);
		}
		if (eventWaitHandle != null && !eventWaitHandle.WaitOne(1000, exitContext: false))
		{
			Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "TIMED OUT ({0}) waiting for logging subsystem", 1000));
		}
	}

	private static bool PrivateInitialize(string className)
	{
		Interlocked.Increment(ref _initializeCallCount);
		if (UnsafeNativeMethods.GetSettingValue("Initialize_SQLiteLog", null) == null && Interlocked.Increment(ref _attemptedInitialize) > 1)
		{
			Interlocked.Decrement(ref _attemptedInitialize);
			return false;
		}
		if (SQLite3.StaticIsInitialized())
		{
			return false;
		}
		if (!AppDomain.CurrentDomain.IsDefaultAppDomain() && UnsafeNativeMethods.GetSettingValue("Force_SQLiteLog", null) == null)
		{
			return false;
		}
		lock (syncRoot)
		{
			if (SQLite3.StaticIsInitialized())
			{
				return false;
			}
			if (_domainUnload == null)
			{
				_domainUnload = DomainUnload;
				AppDomain.CurrentDomain.DomainUnload += _domainUnload;
			}
			if (_sql == null)
			{
				_sql = new SQLite3(SQLiteDateFormats.ISO8601, DateTimeKind.Unspecified, null, IntPtr.Zero, null, ownHandle: false);
			}
			if (_callback == null)
			{
				_callback = LogCallback;
				SQLiteErrorCode sQLiteErrorCode = _sql.SetLogCallback(_callback);
				if (sQLiteErrorCode != 0)
				{
					_callback = null;
					throw new SQLiteException(sQLiteErrorCode, "Failed to configure managed assembly logging.");
				}
			}
			if (UnsafeNativeMethods.GetSettingValue("Disable_SQLiteLog", null) == null)
			{
				_enabled = true;
			}
			AddDefaultHandler();
		}
		return true;
	}

	public static void Uninitialize()
	{
		Uninitialize(null, shutdown: false);
	}

	internal static void Uninitialize(string className, bool shutdown)
	{
		Interlocked.Increment(ref _uninitializeCallCount);
		lock (syncRoot)
		{
			RemoveDefaultHandler();
			_enabled = false;
			if (_sql != null)
			{
				SQLiteErrorCode sQLiteErrorCode;
				if (shutdown)
				{
					sQLiteErrorCode = _sql.Shutdown();
					if (sQLiteErrorCode != 0)
					{
						throw new SQLiteException(sQLiteErrorCode, "Failed to shutdown interface.");
					}
				}
				sQLiteErrorCode = _sql.SetLogCallback(null);
				if (sQLiteErrorCode != 0)
				{
					throw new SQLiteException(sQLiteErrorCode, "Failed to shutdown logging.");
				}
			}
			if (_callback != null)
			{
				_callback = null;
			}
			if (_domainUnload != null)
			{
				AppDomain.CurrentDomain.DomainUnload -= _domainUnload;
				_domainUnload = null;
			}
			CreateAndOrGetTheEvent()?.Reset();
		}
	}

	private static void DomainUnload(object sender, EventArgs e)
	{
		Uninitialize(null, shutdown: true);
	}

	public static void LogMessage(string message)
	{
		LogMessage(null, message);
	}

	public static void LogMessage(SQLiteErrorCode errorCode, string message)
	{
		LogMessage((object)errorCode, message);
	}

	public static void LogMessage(int errorCode, string message)
	{
		LogMessage((object)errorCode, message);
	}

	private static void LogMessage(object errorCode, string message)
	{
		bool flag;
		SQLiteLogEventHandler sQLiteLogEventHandler;
		lock (syncRoot)
		{
			if (_enabled && SQLiteLog._handlers != null)
			{
				flag = true;
				sQLiteLogEventHandler = SQLiteLog._handlers.Clone() as SQLiteLogEventHandler;
			}
			else
			{
				flag = false;
				sQLiteLogEventHandler = null;
			}
		}
		if (flag)
		{
			sQLiteLogEventHandler?.Invoke(null, new LogEventArgs(IntPtr.Zero, errorCode, message, null));
		}
	}

	private static void InitializeDefaultHandler()
	{
		lock (syncRoot)
		{
			if (_defaultHandler == null)
			{
				_defaultHandler = LogEventHandler;
			}
		}
	}

	public static void AddDefaultHandler()
	{
		lock (syncRoot)
		{
			InitializeDefaultHandler();
			Log += _defaultHandler;
		}
	}

	public static void RemoveDefaultHandler()
	{
		lock (syncRoot)
		{
			InitializeDefaultHandler();
			Log -= _defaultHandler;
		}
	}

	private static void LogCallback(IntPtr pUserData, int errorCode, IntPtr pMessage)
	{
		bool flag;
		SQLiteLogEventHandler sQLiteLogEventHandler;
		lock (syncRoot)
		{
			if (_enabled && SQLiteLog._handlers != null)
			{
				flag = true;
				sQLiteLogEventHandler = SQLiteLog._handlers.Clone() as SQLiteLogEventHandler;
			}
			else
			{
				flag = false;
				sQLiteLogEventHandler = null;
			}
		}
		if (flag)
		{
			sQLiteLogEventHandler?.Invoke(null, new LogEventArgs(pUserData, errorCode, SQLiteConvert.UTF8ToString(pMessage, -1), null));
		}
	}

	private static void LogEventHandler(object sender, LogEventArgs e)
	{
		if (e == null)
		{
			return;
		}
		string message = e.Message;
		if (message == null)
		{
			message = "<null>";
		}
		else
		{
			message = message.Trim();
			if (message.Length == 0)
			{
				message = "<empty>";
			}
		}
		object errorCode = e.ErrorCode;
		string text = "error";
		if (errorCode is SQLiteErrorCode || errorCode is int)
		{
			SQLiteErrorCode sQLiteErrorCode = (SQLiteErrorCode)(int)errorCode;
			switch (sQLiteErrorCode & SQLiteErrorCode.NonExtendedMask)
			{
			case SQLiteErrorCode.Ok:
				text = "message";
				break;
			case SQLiteErrorCode.Notice:
				text = "notice";
				break;
			case SQLiteErrorCode.Warning:
				text = "warning";
				break;
			case SQLiteErrorCode.Row:
			case SQLiteErrorCode.Done:
				text = "data";
				break;
			}
		}
		else if (errorCode == null)
		{
			text = "trace";
		}
		if (errorCode != null && errorCode != string.Empty)
		{
			Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "SQLite {0} ({1}): {2}", text, errorCode, message));
		}
		else
		{
			Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "SQLite {0}: {1}", text, message));
		}
	}
}

#define TRACE
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System.Data.SQLite;

internal static class HelperMethods
{
	private const string DisplayNullObject = "<nullObject>";

	private const string DisplayEmptyString = "<emptyString>";

	private const string DisplayStringFormat = "\"{0}\"";

	private const string DisplayNullArray = "<nullArray>";

	private const string DisplayEmptyArray = "<emptyArray>";

	private const char ArrayOpen = '[';

	private const string ElementSeparator = ", ";

	private const char ArrayClose = ']';

	private static readonly char[] SpaceChars = new char[6] { '\t', '\n', '\r', '\v', '\f', ' ' };

	private static readonly object staticSyncRoot = new object();

	private static readonly string MonoRuntimeType = "Mono.Runtime";

	private static readonly string DotNetCoreLibType = "System.CoreLib";

	private static bool? isMono = null;

	private static bool? isDotNetCore = null;

	private static bool? debuggerBreak = null;

	private static int GetProcessId()
	{
		return Process.GetCurrentProcess()?.Id ?? 0;
	}

	private static bool IsMono()
	{
		try
		{
			lock (staticSyncRoot)
			{
				if (!isMono.HasValue)
				{
					isMono = Type.GetType(MonoRuntimeType) != null;
				}
				return isMono.Value;
			}
		}
		catch
		{
		}
		return false;
	}

	public static bool IsDotNetCore()
	{
		try
		{
			lock (staticSyncRoot)
			{
				if (!isDotNetCore.HasValue)
				{
					isDotNetCore = Type.GetType(DotNetCoreLibType) != null;
				}
				return isDotNetCore.Value;
			}
		}
		catch
		{
		}
		return false;
	}

	internal static void ResetBreakIntoDebugger()
	{
		lock (staticSyncRoot)
		{
			debuggerBreak = null;
		}
	}

	internal static void MaybeBreakIntoDebugger()
	{
		lock (staticSyncRoot)
		{
			if (debuggerBreak.HasValue)
			{
				return;
			}
		}
		if (UnsafeNativeMethods.GetSettingValue("PreLoadSQLite_BreakIntoDebugger", null) != null)
		{
			try
			{
				Console.WriteLine(StringFormat(CultureInfo.CurrentCulture, "Attach a debugger to process {0} and press any key to continue.", GetProcessId()));
				Console.ReadKey();
			}
			catch (Exception ex)
			{
				try
				{
					Trace.WriteLine(StringFormat(CultureInfo.CurrentCulture, "Failed to issue debugger prompt, {0} may be unusable: {1}", typeof(Console), ex));
				}
				catch
				{
				}
			}
			try
			{
				Debugger.Break();
				lock (staticSyncRoot)
				{
					debuggerBreak = true;
					return;
				}
			}
			catch
			{
				lock (staticSyncRoot)
				{
					debuggerBreak = false;
				}
				throw;
			}
		}
		lock (staticSyncRoot)
		{
			debuggerBreak = false;
		}
	}

	internal static int GetThreadId()
	{
		return AppDomain.GetCurrentThreadId();
	}

	internal static bool HasFlags(SQLiteConnectionFlags flags, SQLiteConnectionFlags hasFlags)
	{
		return (flags & hasFlags) == hasFlags;
	}

	internal static bool LogPrepare(SQLiteConnectionFlags flags)
	{
		return HasFlags(flags, SQLiteConnectionFlags.LogPrepare);
	}

	internal static bool LogPreBind(SQLiteConnectionFlags flags)
	{
		return HasFlags(flags, SQLiteConnectionFlags.LogPreBind);
	}

	internal static bool LogBind(SQLiteConnectionFlags flags)
	{
		return HasFlags(flags, SQLiteConnectionFlags.LogBind);
	}

	internal static bool LogCallbackExceptions(SQLiteConnectionFlags flags)
	{
		return HasFlags(flags, SQLiteConnectionFlags.LogCallbackException);
	}

	internal static bool LogBackup(SQLiteConnectionFlags flags)
	{
		return HasFlags(flags, SQLiteConnectionFlags.LogBackup);
	}

	internal static bool NoLogModule(SQLiteConnectionFlags flags)
	{
		return HasFlags(flags, SQLiteConnectionFlags.NoLogModule);
	}

	internal static bool LogModuleError(SQLiteConnectionFlags flags)
	{
		return HasFlags(flags, SQLiteConnectionFlags.LogModuleError);
	}

	internal static bool LogModuleException(SQLiteConnectionFlags flags)
	{
		return HasFlags(flags, SQLiteConnectionFlags.LogModuleException);
	}

	internal static bool IsWindows()
	{
		PlatformID platform = Environment.OSVersion.Platform;
		if (platform == PlatformID.Win32S || platform == PlatformID.Win32Windows || platform == PlatformID.Win32NT || platform == PlatformID.WinCE)
		{
			return true;
		}
		return false;
	}

	internal static string StringFormat(IFormatProvider provider, string format, params object[] args)
	{
		if (IsMono())
		{
			return string.Format(format, args);
		}
		return string.Format(provider, format, args);
	}

	public static string ToDisplayString(object value)
	{
		if (value == null)
		{
			return "<nullObject>";
		}
		string text = value.ToString();
		if (text.Length == 0)
		{
			return "<emptyString>";
		}
		if (text.IndexOfAny(SpaceChars) < 0)
		{
			return text;
		}
		return StringFormat(CultureInfo.InvariantCulture, "\"{0}\"", text);
	}

	public static string ToDisplayString(Array array)
	{
		if (array == null)
		{
			return "<nullArray>";
		}
		if (array.Length == 0)
		{
			return "<emptyArray>";
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (object item in array)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(ToDisplayString(item));
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Insert(0, '[');
			stringBuilder.Append(']');
		}
		return stringBuilder.ToString();
	}
}

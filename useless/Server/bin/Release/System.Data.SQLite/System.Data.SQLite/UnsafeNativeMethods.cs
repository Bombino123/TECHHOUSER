#define TRACE
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text;
using System.Xml;

namespace System.Data.SQLite;

[SuppressUnmanagedCodeSecurity]
internal static class UnsafeNativeMethods
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int xSessionFilter(IntPtr context, IntPtr pTblName);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate SQLiteChangeSetConflictResult xSessionConflict(IntPtr context, SQLiteChangeSetConflictType type, IntPtr iterator);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate SQLiteErrorCode xSessionInput(IntPtr context, IntPtr pData, ref int nData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate SQLiteErrorCode xSessionOutput(IntPtr context, IntPtr pData, int nData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xCreate(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xConnect(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xBestIndex(IntPtr pVtab, IntPtr pIndex);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xDisconnect(IntPtr pVtab);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xDestroy(IntPtr pVtab);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xOpen(IntPtr pVtab, ref IntPtr pCursor);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xClose(IntPtr pCursor);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xFilter(IntPtr pCursor, int idxNum, IntPtr idxStr, int argc, IntPtr argv);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xNext(IntPtr pCursor);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int xEof(IntPtr pCursor);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xColumn(IntPtr pCursor, IntPtr pContext, int index);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xRowId(IntPtr pCursor, ref long rowId);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xUpdate(IntPtr pVtab, int argc, IntPtr argv, ref long rowId);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xBegin(IntPtr pVtab);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xSync(IntPtr pVtab);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xCommit(IntPtr pVtab);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xRollback(IntPtr pVtab);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int xFindFunction(IntPtr pVtab, int nArg, IntPtr zName, ref SQLiteCallback callback, ref IntPtr pUserData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xRename(IntPtr pVtab, IntPtr zNew);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xSavepoint(IntPtr pVtab, int iSavepoint);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xRelease(IntPtr pVtab, int iSavepoint);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate SQLiteErrorCode xRollbackTo(IntPtr pVtab, int iSavepoint);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void xDestroyModule(IntPtr pClientData);

	internal struct sqlite3_module
	{
		public int iVersion;

		public xCreate xCreate;

		public xConnect xConnect;

		public xBestIndex xBestIndex;

		public xDisconnect xDisconnect;

		public xDestroy xDestroy;

		public xOpen xOpen;

		public xClose xClose;

		public xFilter xFilter;

		public xNext xNext;

		public xEof xEof;

		public xColumn xColumn;

		public xRowId xRowId;

		public xUpdate xUpdate;

		public xBegin xBegin;

		public xSync xSync;

		public xCommit xCommit;

		public xRollback xRollback;

		public xFindFunction xFindFunction;

		public xRename xRename;

		public xSavepoint xSavepoint;

		public xRelease xRelease;

		public xRollbackTo xRollbackTo;
	}

	internal struct sqlite3_vtab
	{
		public IntPtr pModule;

		public int nRef;

		public IntPtr zErrMsg;
	}

	internal struct sqlite3_vtab_cursor
	{
		public IntPtr pVTab;
	}

	internal struct sqlite3_index_constraint
	{
		public int iColumn;

		public SQLiteIndexConstraintOp op;

		public byte usable;

		public int iTermOffset;

		public sqlite3_index_constraint(SQLiteIndexConstraint constraint)
		{
			this = default(sqlite3_index_constraint);
			if (constraint != null)
			{
				iColumn = constraint.iColumn;
				op = constraint.op;
				usable = constraint.usable;
				iTermOffset = constraint.iTermOffset;
			}
		}
	}

	internal struct sqlite3_index_orderby
	{
		public int iColumn;

		public byte desc;

		public sqlite3_index_orderby(SQLiteIndexOrderBy orderBy)
		{
			this = default(sqlite3_index_orderby);
			if (orderBy != null)
			{
				iColumn = orderBy.iColumn;
				desc = orderBy.desc;
			}
		}
	}

	internal struct sqlite3_index_constraint_usage
	{
		public int argvIndex;

		public byte omit;

		public sqlite3_index_constraint_usage(SQLiteIndexConstraintUsage constraintUsage)
		{
			this = default(sqlite3_index_constraint_usage);
			if (constraintUsage != null)
			{
				argvIndex = constraintUsage.argvIndex;
				omit = constraintUsage.omit;
			}
		}
	}

	internal struct sqlite3_index_info
	{
		public int nConstraint;

		public IntPtr aConstraint;

		public int nOrderBy;

		public IntPtr aOrderBy;

		public IntPtr aConstraintUsage;

		public int idxNum;

		public string idxStr;

		public int needToFreeIdxStr;

		public int orderByConsumed;

		public double estimatedCost;

		public long estimatedRows;

		public SQLiteIndexFlags idxFlags;

		public long colUsed;
	}

	public const string ExceptionMessageFormat = "Caught exception in \"{0}\" method: {1}";

	private static readonly string DllFileExtension;

	private static readonly string ConfigFileExtension;

	private static readonly string AltConfigFileExtension;

	private static readonly string XmlConfigFileName;

	private static readonly string XmlAltConfigFileName;

	private static readonly string XmlConfigDirectoryToken;

	private static readonly string AssemblyDirectoryToken;

	private static readonly string TargetFrameworkToken;

	private static readonly object staticSyncRoot;

	private static Dictionary<string, string> targetFrameworkAbbreviations;

	private static Dictionary<string, string> processorArchitecturePlatforms;

	private static string cachedAssemblyDirectory;

	private static bool noAssemblyDirectory;

	private static string cachedXmlConfigFileName;

	private static bool noXmlConfigFileName;

	private static readonly string PROCESSOR_ARCHITECTURE;

	private static string _SQLiteNativeModuleFileName;

	private static IntPtr _SQLiteNativeModuleHandle;

	internal const string SQLITE_DLL = "SQLite.Interop.dll";

	static UnsafeNativeMethods()
	{
		DllFileExtension = ".dll";
		ConfigFileExtension = ".config";
		AltConfigFileExtension = ".altconfig";
		XmlConfigFileName = typeof(UnsafeNativeMethods).Namespace + DllFileExtension + ConfigFileExtension;
		XmlAltConfigFileName = typeof(UnsafeNativeMethods).Namespace + DllFileExtension + AltConfigFileExtension;
		XmlConfigDirectoryToken = "%PreLoadSQLite_XmlConfigDirectory%";
		AssemblyDirectoryToken = "%PreLoadSQLite_AssemblyDirectory%";
		TargetFrameworkToken = "%PreLoadSQLite_TargetFramework%";
		staticSyncRoot = new object();
		PROCESSOR_ARCHITECTURE = "PROCESSOR_ARCHITECTURE";
		_SQLiteNativeModuleFileName = null;
		_SQLiteNativeModuleHandle = IntPtr.Zero;
		Initialize();
	}

	internal static void Initialize()
	{
		SQLiteExtra.PreVerify();
		lock (staticSyncRoot)
		{
			if (_SQLiteNativeModuleHandle != IntPtr.Zero)
			{
				return;
			}
		}
		HelperMethods.MaybeBreakIntoDebugger();
		if (GetSettingValue("No_PreLoadSQLite", null) != null)
		{
			return;
		}
		lock (staticSyncRoot)
		{
			if (targetFrameworkAbbreviations == null)
			{
				targetFrameworkAbbreviations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v2.0", "net20");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v3.5", "net35");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.0", "net40");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.5", "net45");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.5.1", "net451");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.5.2", "net452");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.6", "net46");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.6.1", "net461");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.6.2", "net462");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.7", "net47");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.7.1", "net471");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.7.2", "net472");
				targetFrameworkAbbreviations.Add(".NETFramework,Version=v4.8", "net48");
				targetFrameworkAbbreviations.Add(".NETStandard,Version=v2.0", "netstandard2.0");
				targetFrameworkAbbreviations.Add(".NETStandard,Version=v2.1", "netstandard2.1");
			}
			if (processorArchitecturePlatforms == null)
			{
				processorArchitecturePlatforms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				processorArchitecturePlatforms.Add("x86", "Win32");
				processorArchitecturePlatforms.Add("x86_64", "x64");
				processorArchitecturePlatforms.Add("AMD64", "x64");
				processorArchitecturePlatforms.Add("IA64", "Itanium");
				processorArchitecturePlatforms.Add("ARM", "WinCE");
			}
			if (_SQLiteNativeModuleHandle == IntPtr.Zero)
			{
				string baseDirectory = null;
				string processorArchitecture = null;
				bool allowBaseDirectoryOnly = false;
				SearchForDirectory(ref baseDirectory, ref processorArchitecture, ref allowBaseDirectoryOnly);
				PreLoadSQLiteDll(baseDirectory, processorArchitecture, allowBaseDirectoryOnly, ref _SQLiteNativeModuleFileName, ref _SQLiteNativeModuleHandle);
			}
		}
	}

	private static string MaybeCombinePath(string path1, string path2)
	{
		if (path1 != null)
		{
			if (path2 != null)
			{
				return Path.Combine(path1, path2);
			}
			return path1;
		}
		if (path2 != null)
		{
			return path2;
		}
		return null;
	}

	private static void ResetCachedXmlConfigFileName()
	{
		lock (staticSyncRoot)
		{
			cachedXmlConfigFileName = null;
			noXmlConfigFileName = false;
		}
	}

	private static string GetCachedXmlConfigFileName()
	{
		lock (staticSyncRoot)
		{
			if (cachedXmlConfigFileName != null)
			{
				return cachedXmlConfigFileName;
			}
			if (noXmlConfigFileName)
			{
				return null;
			}
		}
		return GetXmlConfigFileName();
	}

	private static string GetXmlConfigFileName()
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		string text = MaybeCombinePath(baseDirectory, XmlConfigFileName);
		if (File.Exists(text))
		{
			lock (staticSyncRoot)
			{
				cachedXmlConfigFileName = text;
				return text;
			}
		}
		text = MaybeCombinePath(baseDirectory, XmlAltConfigFileName);
		if (File.Exists(text))
		{
			lock (staticSyncRoot)
			{
				cachedXmlConfigFileName = text;
				return text;
			}
		}
		baseDirectory = GetCachedAssemblyDirectory();
		text = MaybeCombinePath(baseDirectory, XmlConfigFileName);
		if (File.Exists(text))
		{
			lock (staticSyncRoot)
			{
				cachedXmlConfigFileName = text;
				return text;
			}
		}
		text = MaybeCombinePath(baseDirectory, XmlAltConfigFileName);
		if (File.Exists(text))
		{
			lock (staticSyncRoot)
			{
				cachedXmlConfigFileName = text;
				return text;
			}
		}
		lock (staticSyncRoot)
		{
			noXmlConfigFileName = true;
		}
		return null;
	}

	private static string ReplaceXmlConfigFileTokens(string fileName, string value)
	{
		if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(fileName) && value.IndexOf(XmlConfigDirectoryToken) != -1)
		{
			try
			{
				string directoryName = Path.GetDirectoryName(fileName);
				if (!string.IsNullOrEmpty(directoryName))
				{
					value = value.Replace(XmlConfigDirectoryToken, directoryName);
				}
			}
			catch (Exception ex)
			{
				try
				{
					Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Native library pre-loader failed to replace XML configuration file \"{0}\" tokens: {1}", fileName, ex));
				}
				catch
				{
				}
			}
		}
		return value;
	}

	private static string GetSettingValueViaXmlConfigFile(string fileName, string name, string @default, bool expand, bool tokens)
	{
		try
		{
			if (fileName == null || name == null)
			{
				return @default;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);
			if (xmlDocument.SelectSingleNode(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "/configuration/appSettings/add[@key='{0}']", name)) is XmlElement xmlElement)
			{
				string text = null;
				if (xmlElement.HasAttribute("value"))
				{
					text = xmlElement.GetAttribute("value");
				}
				if (!string.IsNullOrEmpty(text))
				{
					if (expand)
					{
						text = Environment.ExpandEnvironmentVariables(text);
					}
					if (tokens)
					{
						text = ReplaceEnvironmentVariableTokens(text);
					}
					if (tokens)
					{
						text = ReplaceXmlConfigFileTokens(fileName, text);
					}
				}
				if (text != null)
				{
					return text;
				}
			}
		}
		catch (Exception ex)
		{
			try
			{
				Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Native library pre-loader failed to get setting \"{0}\" value from XML configuration file \"{1}\": {2}", name, fileName, ex));
			}
			catch
			{
			}
		}
		return @default;
	}

	private static string GetAssemblyTargetFramework(Assembly assembly)
	{
		if (assembly != null)
		{
			try
			{
				if (assembly.IsDefined(typeof(TargetFrameworkAttribute), inherit: false))
				{
					return ((TargetFrameworkAttribute)assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute), inherit: false)[0]).FrameworkName;
				}
			}
			catch
			{
			}
		}
		return null;
	}

	private static string AbbreviateTargetFramework(string targetFramework)
	{
		if (!string.IsNullOrEmpty(targetFramework))
		{
			lock (staticSyncRoot)
			{
				if (targetFrameworkAbbreviations != null && targetFrameworkAbbreviations.TryGetValue(targetFramework, out var value))
				{
					return value;
				}
			}
			int num = targetFramework.IndexOf(".NETFramework,Version=v");
			if (num != -1)
			{
				string value = targetFramework;
				value = value.Replace(".NETFramework,Version=v", "net");
				value = value.Replace(".", string.Empty);
				num = value.IndexOf(',');
				if (num != -1)
				{
					return value.Substring(0, num);
				}
				return value;
			}
		}
		return targetFramework;
	}

	private static string ReplaceEnvironmentVariableTokens(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			if (value.IndexOf(AssemblyDirectoryToken) != -1)
			{
				string text = GetCachedAssemblyDirectory();
				if (!string.IsNullOrEmpty(text))
				{
					try
					{
						value = value.Replace(AssemblyDirectoryToken, text);
					}
					catch (Exception ex)
					{
						try
						{
							Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Native library pre-loader failed to replace assembly directory token: {0}", ex));
						}
						catch
						{
						}
					}
				}
			}
			if (value.IndexOf(TargetFrameworkToken) != -1)
			{
				Assembly assembly = null;
				try
				{
					assembly = Assembly.GetExecutingAssembly();
				}
				catch (Exception ex2)
				{
					try
					{
						Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Native library pre-loader failed to obtain executing assembly: {0}", ex2));
					}
					catch
					{
					}
				}
				string text2 = AbbreviateTargetFramework(GetAssemblyTargetFramework(assembly));
				if (!string.IsNullOrEmpty(text2))
				{
					try
					{
						value = value.Replace(TargetFrameworkToken, text2);
					}
					catch (Exception ex3)
					{
						try
						{
							Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Native library pre-loader failed to replace target framework token: {0}", ex3));
						}
						catch
						{
						}
					}
				}
			}
		}
		return value;
	}

	internal static string GetSettingValue(string name, string @default)
	{
		if (Environment.GetEnvironmentVariable("No_SQLiteGetSettingValue") != null)
		{
			return @default;
		}
		if (name == null)
		{
			return @default;
		}
		bool flag = true;
		bool flag2 = true;
		string text = null;
		if (Environment.GetEnvironmentVariable("No_Expand") != null)
		{
			flag = false;
		}
		else if (Environment.GetEnvironmentVariable(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "No_Expand_{0}", name)) != null)
		{
			flag = false;
		}
		if (Environment.GetEnvironmentVariable("No_Tokens") != null)
		{
			flag2 = false;
		}
		else if (Environment.GetEnvironmentVariable(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "No_Tokens_{0}", name)) != null)
		{
			flag2 = false;
		}
		text = Environment.GetEnvironmentVariable(name);
		if (!string.IsNullOrEmpty(text))
		{
			if (flag)
			{
				text = Environment.ExpandEnvironmentVariables(text);
			}
			if (flag2)
			{
				text = ReplaceEnvironmentVariableTokens(text);
			}
		}
		if (text != null)
		{
			return text;
		}
		if (Environment.GetEnvironmentVariable("No_SQLiteXmlConfigFile") != null)
		{
			return @default;
		}
		return GetSettingValueViaXmlConfigFile(GetCachedXmlConfigFileName(), name, @default, flag, flag2);
	}

	private static string ListToString(IList<string> list)
	{
		if (list == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in list)
		{
			if (item != null)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(item);
			}
		}
		return stringBuilder.ToString();
	}

	private static int CheckForArchitecturesAndPlatforms(string directory, ref List<string> matches)
	{
		int num = 0;
		if (matches == null)
		{
			matches = new List<string>();
		}
		lock (staticSyncRoot)
		{
			if (!string.IsNullOrEmpty(directory) && processorArchitecturePlatforms != null)
			{
				foreach (KeyValuePair<string, string> processorArchitecturePlatform in processorArchitecturePlatforms)
				{
					if (Directory.Exists(MaybeCombinePath(directory, processorArchitecturePlatform.Key)))
					{
						matches.Add(processorArchitecturePlatform.Key);
						num++;
					}
					string value = processorArchitecturePlatform.Value;
					if (value != null && Directory.Exists(MaybeCombinePath(directory, value)))
					{
						matches.Add(value);
						num++;
					}
				}
			}
		}
		return num;
	}

	private static bool CheckAssemblyCodeBase(Assembly assembly, ref string fileName)
	{
		try
		{
			if (assembly == null)
			{
				return false;
			}
			string codeBase = assembly.CodeBase;
			if (string.IsNullOrEmpty(codeBase))
			{
				return false;
			}
			string localPath = new Uri(codeBase).LocalPath;
			if (!File.Exists(localPath))
			{
				return false;
			}
			string directoryName = Path.GetDirectoryName(localPath);
			if (File.Exists(MaybeCombinePath(directoryName, XmlConfigFileName)))
			{
				fileName = localPath;
				return true;
			}
			if (File.Exists(MaybeCombinePath(directoryName, XmlAltConfigFileName)))
			{
				fileName = localPath;
				return true;
			}
			List<string> matches = null;
			if (CheckForArchitecturesAndPlatforms(directoryName, ref matches) > 0)
			{
				fileName = localPath;
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			try
			{
				Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Native library pre-loader failed to check code base for currently executing assembly: {0}", ex));
			}
			catch
			{
			}
		}
		return false;
	}

	private static void ResetCachedAssemblyDirectory()
	{
		lock (staticSyncRoot)
		{
			cachedAssemblyDirectory = null;
			noAssemblyDirectory = false;
		}
	}

	private static string GetCachedAssemblyDirectory()
	{
		lock (staticSyncRoot)
		{
			if (cachedAssemblyDirectory != null)
			{
				return cachedAssemblyDirectory;
			}
			if (noAssemblyDirectory)
			{
				return null;
			}
		}
		return GetAssemblyDirectory();
	}

	private static string GetAssemblyDirectory()
	{
		try
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			if (executingAssembly == null)
			{
				lock (staticSyncRoot)
				{
					noAssemblyDirectory = true;
				}
				return null;
			}
			string fileName = null;
			if (!CheckAssemblyCodeBase(executingAssembly, ref fileName))
			{
				fileName = executingAssembly.Location;
			}
			if (string.IsNullOrEmpty(fileName))
			{
				lock (staticSyncRoot)
				{
					noAssemblyDirectory = true;
				}
				return null;
			}
			string directoryName = Path.GetDirectoryName(fileName);
			if (string.IsNullOrEmpty(directoryName))
			{
				lock (staticSyncRoot)
				{
					noAssemblyDirectory = true;
				}
				return null;
			}
			lock (staticSyncRoot)
			{
				cachedAssemblyDirectory = directoryName;
			}
			return directoryName;
		}
		catch (Exception ex)
		{
			try
			{
				Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Native library pre-loader failed to get directory for currently executing assembly: {0}", ex));
			}
			catch
			{
			}
		}
		lock (staticSyncRoot)
		{
			noAssemblyDirectory = true;
		}
		return null;
	}

	internal static string GetNativeModuleFileName()
	{
		lock (staticSyncRoot)
		{
			if (_SQLiteNativeModuleFileName != null)
			{
				return _SQLiteNativeModuleFileName;
			}
		}
		return "SQLite.Interop.dll";
	}

	internal static string GetNativeLibraryFileNameOnly()
	{
		string settingValue = GetSettingValue("PreLoadSQLite_LibraryFileNameOnly", null);
		if (settingValue != null)
		{
			return settingValue;
		}
		return "SQLite.Interop.dll";
	}

	private static bool SearchForDirectory(ref string baseDirectory, ref string processorArchitecture, ref bool allowBaseDirectoryOnly)
	{
		if (GetSettingValue("PreLoadSQLite_NoSearchForDirectory", null) != null)
		{
			return false;
		}
		string nativeLibraryFileNameOnly = GetNativeLibraryFileNameOnly();
		if (nativeLibraryFileNameOnly == null)
		{
			return false;
		}
		string[] array = new string[2]
		{
			GetAssemblyDirectory(),
			AppDomain.CurrentDomain.BaseDirectory
		};
		string text = null;
		if (GetSettingValue("PreLoadSQLite_AllowBaseDirectoryOnly", null) != null || (HelperMethods.IsDotNetCore() && !HelperMethods.IsWindows()))
		{
			text = string.Empty;
		}
		string[] array2 = new string[3]
		{
			GetProcessorArchitecture(),
			GetPlatformName(null),
			text
		};
		string[] array3 = array;
		foreach (string text2 in array3)
		{
			if (text2 == null)
			{
				continue;
			}
			string[] array4 = array2;
			foreach (string text3 in array4)
			{
				if (text3 != null && File.Exists(FixUpDllFileName(MaybeCombinePath(MaybeCombinePath(text2, text3), nativeLibraryFileNameOnly))))
				{
					baseDirectory = text2;
					processorArchitecture = text3;
					allowBaseDirectoryOnly = text3.Length == 0;
					return true;
				}
			}
		}
		return false;
	}

	private static string GetBaseDirectory()
	{
		string settingValue = GetSettingValue("PreLoadSQLite_BaseDirectory", null);
		if (settingValue != null)
		{
			return settingValue;
		}
		if (GetSettingValue("PreLoadSQLite_UseAssemblyDirectory", null) != null)
		{
			settingValue = GetAssemblyDirectory();
			if (settingValue != null)
			{
				return settingValue;
			}
		}
		return AppDomain.CurrentDomain.BaseDirectory;
	}

	private static string FixUpDllFileName(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName) && HelperMethods.IsWindows() && !fileName.EndsWith(DllFileExtension, StringComparison.OrdinalIgnoreCase))
		{
			return fileName + DllFileExtension;
		}
		return fileName;
	}

	private static string GetProcessorArchitecture()
	{
		string settingValue = GetSettingValue("PreLoadSQLite_ProcessorArchitecture", null);
		if (settingValue != null)
		{
			return settingValue;
		}
		settingValue = GetSettingValue(PROCESSOR_ARCHITECTURE, null);
		if (IntPtr.Size == 4 && string.Equals(settingValue, "AMD64", StringComparison.OrdinalIgnoreCase))
		{
			settingValue = "x86";
		}
		if (settingValue == null)
		{
			settingValue = NativeLibraryHelper.GetMachine();
			if (settingValue == null)
			{
				settingValue = string.Empty;
			}
		}
		return settingValue;
	}

	private static string GetPlatformName(string processorArchitecture)
	{
		if (processorArchitecture == null)
		{
			processorArchitecture = GetProcessorArchitecture();
		}
		if (string.IsNullOrEmpty(processorArchitecture))
		{
			return null;
		}
		lock (staticSyncRoot)
		{
			if (processorArchitecturePlatforms == null)
			{
				return null;
			}
			if (processorArchitecturePlatforms.TryGetValue(processorArchitecture, out var value))
			{
				return value;
			}
		}
		return null;
	}

	private static bool PreLoadSQLiteDll(string baseDirectory, string processorArchitecture, bool allowBaseDirectoryOnly, ref string nativeModuleFileName, ref IntPtr nativeModuleHandle)
	{
		if (baseDirectory == null)
		{
			baseDirectory = GetBaseDirectory();
		}
		if (baseDirectory == null)
		{
			return false;
		}
		string nativeLibraryFileNameOnly = GetNativeLibraryFileNameOnly();
		if (nativeLibraryFileNameOnly == null)
		{
			return false;
		}
		string text = FixUpDllFileName(MaybeCombinePath(baseDirectory, nativeLibraryFileNameOnly));
		if (File.Exists(text))
		{
			if (!allowBaseDirectoryOnly || !string.IsNullOrEmpty(processorArchitecture))
			{
				return false;
			}
		}
		else
		{
			if (processorArchitecture == null)
			{
				processorArchitecture = GetProcessorArchitecture();
			}
			if (processorArchitecture == null)
			{
				return false;
			}
			text = FixUpDllFileName(MaybeCombinePath(MaybeCombinePath(baseDirectory, processorArchitecture), nativeLibraryFileNameOnly));
			if (!File.Exists(text))
			{
				string platformName = GetPlatformName(processorArchitecture);
				if (platformName == null)
				{
					return false;
				}
				text = FixUpDllFileName(MaybeCombinePath(MaybeCombinePath(baseDirectory, platformName), nativeLibraryFileNameOnly));
				if (!File.Exists(text))
				{
					return false;
				}
			}
		}
		try
		{
			try
			{
				Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Native library pre-loader is trying to load native SQLite library \"{0}\"...", text));
			}
			catch
			{
			}
			nativeModuleFileName = text;
			nativeModuleHandle = NativeLibraryHelper.LoadLibrary(text);
			return nativeModuleHandle != IntPtr.Zero;
		}
		catch (Exception ex)
		{
			try
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Native library pre-loader failed to load native SQLite library \"{0}\" (getLastError = {1}): {2}", text, lastWin32Error, ex));
			}
			catch
			{
			}
		}
		return false;
	}

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIa9b758d3ae2e9aea")]
	internal static extern IntPtr sqlite3_bind_parameter_name_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI7da3b9691a73b4a2")]
	internal static extern IntPtr sqlite3_column_database_name_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIeea5cda6d9846aa5")]
	internal static extern IntPtr sqlite3_column_database_name16_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI8dd2c969f2a6a31e")]
	internal static extern IntPtr sqlite3_column_decltype_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI9ea49276dcf9849f")]
	internal static extern IntPtr sqlite3_column_decltype16_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI64119fb980b7e962")]
	internal static extern IntPtr sqlite3_column_name_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIc5d5610a73781fac")]
	internal static extern IntPtr sqlite3_column_name16_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIdeb28053ce71d5b4")]
	internal static extern IntPtr sqlite3_column_origin_name_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI57335d3e329eea8d")]
	internal static extern IntPtr sqlite3_column_origin_name16_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI0fe7fecb92f1b44b")]
	internal static extern IntPtr sqlite3_column_table_name_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI72b05b1ad5f34d61")]
	internal static extern IntPtr sqlite3_column_table_name16_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIe2cb515ade8c0235")]
	internal static extern IntPtr sqlite3_column_text_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIb84ee16d149e7562")]
	internal static extern IntPtr sqlite3_column_text16_interop(IntPtr stmt, int index, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI9bf461db9bf83c22")]
	internal static extern IntPtr sqlite3_errmsg_interop(IntPtr db, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIdca7c218fe9393fb")]
	internal static extern SQLiteErrorCode sqlite3_prepare_interop(IntPtr db, IntPtr pSql, int nBytes, ref IntPtr stmt, ref IntPtr ptrRemain, ref int nRemain);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIc8c63d94dd03040b")]
	internal static extern SQLiteErrorCode sqlite3_table_column_metadata_interop(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, ref IntPtr ptrDataType, ref IntPtr ptrCollSeq, ref int notNull, ref int primaryKey, ref int autoInc, ref int dtLen, ref int csLen);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI013768ea228eaf06")]
	internal static extern IntPtr sqlite3_value_text_interop(IntPtr p, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIa8e1a742a358dc07")]
	internal static extern IntPtr sqlite3_value_text16_interop(IntPtr p, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI24f5c83eaddd65e8")]
	internal static extern int sqlite3_malloc_size_interop(IntPtr p);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI075258fe7332f5c9")]
	internal static extern IntPtr interop_libversion();

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIab3ed5feb3c43f0c")]
	internal static extern IntPtr interop_sourceid();

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIada7f27769d7f95b")]
	internal static extern int interop_compileoption_used(IntPtr zOptName);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIc2d8715ee8c9e908")]
	internal static extern IntPtr interop_compileoption_get(int N);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIa1d7e21a548b910c")]
	internal static extern SQLiteErrorCode sqlite3_close_interop(IntPtr db);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIb391996b7b633820")]
	internal static extern SQLiteErrorCode sqlite3_create_function_interop(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal, int needCollSeq);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIdbbe1b52f304f379")]
	internal static extern SQLiteErrorCode sqlite3_finalize_interop(IntPtr stmt);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI7cd7250399b1ea60")]
	internal static extern SQLiteErrorCode sqlite3_backup_finish_interop(IntPtr backup);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIf8090240c1414098")]
	internal static extern SQLiteErrorCode sqlite3_blob_close_interop(IntPtr blob);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIfcfad09d1b0a60ec")]
	internal static extern SQLiteErrorCode sqlite3_open_interop(byte[] utf8Filename, byte[] vfsName, SQLiteOpenFlagsEnum flags, int extFuncs, ref IntPtr db);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SId00f524badf4f5cf")]
	internal static extern SQLiteErrorCode sqlite3_open16_interop(byte[] utf8Filename, byte[] vfsName, SQLiteOpenFlagsEnum flags, int extFuncs, ref IntPtr db);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI7a4943591207dd14")]
	internal static extern SQLiteErrorCode sqlite3_reset_interop(IntPtr stmt);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI6e6c544c028b2de9")]
	internal static extern int sqlite3_changes_interop(IntPtr db);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI2b074e8cf29cb472")]
	internal static extern IntPtr sqlite3_context_collseq_interop(IntPtr context, ref int type, ref int enc, ref int len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SIc0bb4eab77f5a999")]
	internal static extern int sqlite3_context_collcompare_interop(IntPtr context, byte[] p1, int p1len, byte[] p2, int p2len);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI050169b0f137dcac")]
	internal static extern SQLiteErrorCode sqlite3_cursor_rowid_interop(IntPtr stmt, int cursor, ref long rowid);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI77ffe3fede90bfbb")]
	internal static extern SQLiteErrorCode sqlite3_index_column_info_interop(IntPtr db, byte[] catalog, byte[] IndexName, byte[] ColumnName, ref int sortOrder, ref int onError, ref IntPtr Collation, ref int colllen);

	[DllImport("SQLite.Interop.dll", EntryPoint = "SI349980d776ee39ac")]
	internal static extern int sqlite3_table_cursor_interop(IntPtr stmt, int db, int tableRootPage);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI32b0162a49ae8729")]
	internal static extern IntPtr sqlite3_libversion();

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI00b5de0dbe05adc8")]
	internal static extern int sqlite3_libversion_number();

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI609efcf96ea8408a")]
	internal static extern IntPtr sqlite3_sourceid();

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa26b8e5116b7d93b")]
	internal static extern int sqlite3_compileoption_used(IntPtr zOptName);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIc3b0ae4f7a27b7a8")]
	internal static extern IntPtr sqlite3_compileoption_get(int N);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI768767362ea03a94")]
	internal static extern SQLiteErrorCode sqlite3_enable_shared_cache(int enable);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI87a11c29fe1dc17b")]
	internal static extern SQLiteErrorCode sqlite3_enable_load_extension(IntPtr db, int enable);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIdc2e4e1ccfa9a043")]
	internal static extern SQLiteErrorCode sqlite3_load_extension(IntPtr db, byte[] fileName, byte[] procName, ref IntPtr pError);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI70e4ec628dd7e188")]
	internal static extern SQLiteErrorCode sqlite3_overload_function(IntPtr db, IntPtr zName, int nArgs);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "SIf060fffd3e5c94b5")]
	internal static extern SQLiteErrorCode sqlite3_win32_set_directory(uint type, string value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI4fb6877896ef7303")]
	internal static extern SQLiteErrorCode sqlite3_win32_reset_heap();

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI8d447a033d183aaf")]
	internal static extern SQLiteErrorCode sqlite3_win32_compact_heap(ref uint largest);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf8a71e249d9c661f")]
	internal static extern IntPtr sqlite3_malloc(int n);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI6e539204336d5b4b")]
	internal static extern IntPtr sqlite3_malloc64(ulong n);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI19b2677a019ab7ab")]
	internal static extern IntPtr sqlite3_realloc(IntPtr p, int n);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI4bf5a93645714882")]
	internal static extern IntPtr sqlite3_realloc64(IntPtr p, ulong n);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI8a3a9f59ab5f24ee")]
	internal static extern ulong sqlite3_msize(IntPtr p);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIfc350ae509dc2b53")]
	internal static extern void sqlite3_free(IntPtr p);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI3723ba2973e3b57b")]
	internal static extern void sqlite3_interrupt(IntPtr db);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI3f596ca698b243fc")]
	internal static extern long sqlite3_last_insert_rowid(IntPtr db);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf216ef3874529d42")]
	internal static extern int sqlite3_changes(IntPtr db);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIc001296960f3e921")]
	internal static extern long sqlite3_memory_used();

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI919f742193b92a4b")]
	internal static extern long sqlite3_memory_highwater(int resetFlag);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa62692883d0c456a")]
	internal static extern SQLiteErrorCode sqlite3_shutdown();

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI8e33e5864c547f3f")]
	internal static extern SQLiteErrorCode sqlite3_busy_timeout(IntPtr db, int ms);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa3401e98cbad673e")]
	internal static extern SQLiteErrorCode sqlite3_clear_bindings(IntPtr stmt);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SId6deafdcc0c75049")]
	internal static extern SQLiteErrorCode sqlite3_bind_blob(IntPtr stmt, int index, byte[] value, int nSize, IntPtr nTransient);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI5b7374ef2d63d8eb")]
	internal static extern SQLiteErrorCode sqlite3_bind_double(IntPtr stmt, int index, double value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIde347320a2d725fa")]
	internal static extern SQLiteErrorCode sqlite3_bind_int(IntPtr stmt, int index, int value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIde347320a2d725fa")]
	internal static extern SQLiteErrorCode sqlite3_bind_uint(IntPtr stmt, int index, uint value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI46481015c7f49c68")]
	internal static extern SQLiteErrorCode sqlite3_bind_int64(IntPtr stmt, int index, long value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI46481015c7f49c68")]
	internal static extern SQLiteErrorCode sqlite3_bind_uint64(IntPtr stmt, int index, ulong value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa618e7f1e95b5c32")]
	internal static extern SQLiteErrorCode sqlite3_bind_null(IntPtr stmt, int index);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI2bc16556d60ccfcd")]
	internal static extern SQLiteErrorCode sqlite3_bind_text(IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI31a566678d8c54ab")]
	internal static extern int sqlite3_bind_parameter_count(IntPtr stmt);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIb3f045f05d2d1b0e")]
	internal static extern int sqlite3_bind_parameter_index(IntPtr stmt, byte[] strName);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf8ee6276be88ce12")]
	internal static extern int sqlite3_column_count(IntPtr stmt);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI9c6d7cd7b7d38055")]
	internal static extern SQLiteErrorCode sqlite3_step(IntPtr stmt);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIb4089165600bb7f5")]
	internal static extern int sqlite3_stmt_readonly(IntPtr stmt);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa3f7b31190ce0815")]
	internal static extern double sqlite3_column_double(IntPtr stmt, int index);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIbff6307869d58daf")]
	internal static extern int sqlite3_column_int(IntPtr stmt, int index);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf8364af380546f2d")]
	internal static extern long sqlite3_column_int64(IntPtr stmt, int index);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIbc9b0b73a965892b")]
	internal static extern IntPtr sqlite3_column_blob(IntPtr stmt, int index);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SId99ac2a61d035e11")]
	internal static extern int sqlite3_column_bytes(IntPtr stmt, int index);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI989b754accd7cddf")]
	internal static extern int sqlite3_column_bytes16(IntPtr stmt, int index);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI4983499b64278231")]
	internal static extern TypeAffinity sqlite3_column_type(IntPtr stmt, int index);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf86df75514f4442f")]
	internal static extern SQLiteErrorCode sqlite3_create_collation(IntPtr db, byte[] strName, int nType, IntPtr pvUser, SQLiteCollation func);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI12a7c0808f3f99f2")]
	internal static extern int sqlite3_aggregate_count(IntPtr context);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI4abff63f9a080046")]
	internal static extern IntPtr sqlite3_value_blob(IntPtr p);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIea8388f7613ed158")]
	internal static extern int sqlite3_value_bytes(IntPtr p);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIe7632ce587b97772")]
	internal static extern int sqlite3_value_bytes16(IntPtr p);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI863e1ae0679961f5")]
	internal static extern double sqlite3_value_double(IntPtr p);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf356c1132676af25")]
	internal static extern int sqlite3_value_int(IntPtr p);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIe57aa77c8884d3f9")]
	internal static extern long sqlite3_value_int64(IntPtr p);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa0f9dd1158cbfb0c")]
	internal static extern TypeAffinity sqlite3_value_type(IntPtr p);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIfca3960780d005fa")]
	internal static extern void sqlite3_result_blob(IntPtr context, byte[] value, int nSize, IntPtr pvReserved);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI24bb313f312e2857")]
	internal static extern void sqlite3_result_double(IntPtr context, double value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIca6f27da046939cc")]
	internal static extern void sqlite3_result_error(IntPtr context, byte[] strErr, int nLen);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI633e1c91fb9a8aa1")]
	internal static extern void sqlite3_result_error_code(IntPtr context, SQLiteErrorCode value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI69d8a1d378771295")]
	internal static extern void sqlite3_result_error_toobig(IntPtr context);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI943321d364f02e5d")]
	internal static extern void sqlite3_result_error_nomem(IntPtr context);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI23e7cf9c8e559e93")]
	internal static extern void sqlite3_result_value(IntPtr context, IntPtr value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI4987aea8bdedf163")]
	internal static extern void sqlite3_result_zeroblob(IntPtr context, int nLen);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIcd6b4ac0aeff7202")]
	internal static extern void sqlite3_result_int(IntPtr context, int value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI1527d54f96ad891e")]
	internal static extern void sqlite3_result_int64(IntPtr context, long value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI21d24eac30bb9014")]
	internal static extern void sqlite3_result_null(IntPtr context);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI9196a02c851acbfb")]
	internal static extern void sqlite3_result_text(IntPtr context, byte[] value, int nLen, IntPtr pvReserved);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI79f6c64948ab63d2")]
	internal static extern IntPtr sqlite3_aggregate_context(IntPtr context, int nBytes);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "SI462a496f3247faf2")]
	internal static extern SQLiteErrorCode sqlite3_bind_text16(IntPtr stmt, int index, string value, int nlen, IntPtr pvReserved);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "SI36e67160052f7f69")]
	internal static extern void sqlite3_result_error16(IntPtr context, string strName, int nLen);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "SIc38599462746964c")]
	internal static extern void sqlite3_result_text16(IntPtr context, string strName, int nLen, IntPtr pvReserved);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIcbf931747ce28d75")]
	internal static extern SQLiteErrorCode sqlite3_key(IntPtr db, byte[] key, int keylen);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI36ddf861177da59c")]
	internal static extern SQLiteErrorCode sqlite3_rekey(IntPtr db, byte[] key, int keylen);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI4ceb9ee614df7639")]
	internal static extern void sqlite3_busy_handler(IntPtr db, SQLiteBusyCallback func, IntPtr pvUser);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI0a8f1a0337a9feb7")]
	internal static extern void sqlite3_progress_handler(IntPtr db, int ops, SQLiteProgressCallback func, IntPtr pvUser);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI2c7820fd77bf7594")]
	internal static extern IntPtr sqlite3_set_authorizer(IntPtr db, SQLiteAuthorizerCallback func, IntPtr pvUser);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI546abd8b90c0f48d")]
	internal static extern IntPtr sqlite3_update_hook(IntPtr db, SQLiteUpdateCallback func, IntPtr pvUser);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIfd52d1389825aac4")]
	internal static extern IntPtr sqlite3_commit_hook(IntPtr db, SQLiteCommitCallback func, IntPtr pvUser);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI824276b89178ecbe")]
	internal static extern IntPtr sqlite3_trace(IntPtr db, SQLiteTraceCallback func, IntPtr pvUser);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI3f8b1cdb4dc92eff")]
	internal static extern IntPtr sqlite3_trace_v2(IntPtr db, SQLiteTraceFlags mask, SQLiteTraceCallback2 func, IntPtr pvUser);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI0e7c9abcb195c68c")]
	internal static extern int sqlite3_limit(IntPtr db, SQLiteLimitOpsEnum op, int value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa069da76968b7553")]
	internal static extern SQLiteErrorCode sqlite3_config_none(SQLiteConfigOpsEnum op);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa069da76968b7553")]
	internal static extern SQLiteErrorCode sqlite3_config_int(SQLiteConfigOpsEnum op, int value);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa069da76968b7553")]
	internal static extern SQLiteErrorCode sqlite3_config_log(SQLiteConfigOpsEnum op, SQLiteLogCallback func, IntPtr pvUser);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf4199c70bfb81a15")]
	internal static extern SQLiteErrorCode sqlite3_db_config_charptr(IntPtr db, SQLiteConfigDbOpsEnum op, IntPtr charPtr);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf4199c70bfb81a15")]
	internal static extern SQLiteErrorCode sqlite3_db_config_int_refint(IntPtr db, SQLiteConfigDbOpsEnum op, int value, ref int result);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf4199c70bfb81a15")]
	internal static extern SQLiteErrorCode sqlite3_db_config_intptr_two_ints(IntPtr db, SQLiteConfigDbOpsEnum op, IntPtr ptr, int int0, int int1);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI066df4a47deb7b95")]
	internal static extern SQLiteErrorCode sqlite3_db_status(IntPtr db, SQLiteStatusOpsEnum op, ref int current, ref int highwater, int resetFlag);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIea4313bfab0cfe57")]
	internal static extern IntPtr sqlite3_rollback_hook(IntPtr db, SQLiteRollbackCallback func, IntPtr pvUser);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI7f41c842e0d89557")]
	internal static extern IntPtr sqlite3_db_handle(IntPtr stmt);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIe68a75d48671c8a5")]
	internal static extern SQLiteErrorCode sqlite3_db_release_memory(IntPtr db);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI61a330d2e855b61b")]
	internal static extern IntPtr sqlite3_db_filename(IntPtr db, IntPtr dbName);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI989b06a5a9b937fe")]
	internal static extern int sqlite3_db_readonly(IntPtr db, IntPtr dbName);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI61a330d2e855b61b")]
	internal static extern IntPtr sqlite3_db_filename_bytes(IntPtr db, byte[] dbName);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIae9562605f0b7c06")]
	internal static extern IntPtr sqlite3_next_stmt(IntPtr db, IntPtr stmt);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIdba35b6dcb77d463")]
	internal static extern SQLiteErrorCode sqlite3_exec(IntPtr db, byte[] strSql, IntPtr pvCallback, IntPtr pvParam, ref IntPtr errMsg);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI6d14611bb8b2c8bc")]
	internal static extern int sqlite3_release_memory(int nBytes);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI3d45b939740dff51")]
	internal static extern int sqlite3_get_autocommit(IntPtr db);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI07646c0ac405baf1")]
	internal static extern SQLiteErrorCode sqlite3_extended_result_codes(IntPtr db, int onoff);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI45d842f2d2322061")]
	internal static extern SQLiteErrorCode sqlite3_errcode(IntPtr db);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI03ee1370ff2172fd")]
	internal static extern SQLiteErrorCode sqlite3_extended_errcode(IntPtr db);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIdfd4d81e1791b463")]
	internal static extern IntPtr sqlite3_errstr(SQLiteErrorCode rc);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI950480ab972e108d")]
	internal static extern void sqlite3_log(SQLiteErrorCode iErrCode, byte[] zFormat);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIb6d52d8cb2f1045a")]
	internal static extern SQLiteErrorCode sqlite3_file_control(IntPtr db, byte[] zDbName, int op, IntPtr pArg);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI83d1cf4976f57337")]
	internal static extern IntPtr sqlite3_backup_init(IntPtr destDb, byte[] zDestName, IntPtr sourceDb, byte[] zSourceName);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI8d676d5954c732bc")]
	internal static extern SQLiteErrorCode sqlite3_backup_step(IntPtr backup, int nPage);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIa2a6050a8dbd3b7a")]
	internal static extern int sqlite3_backup_remaining(IntPtr backup);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI9cb7692f61998485")]
	internal static extern int sqlite3_backup_pagecount(IntPtr backup);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI00f2097672333949")]
	internal static extern SQLiteErrorCode sqlite3_blob_close(IntPtr blob);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIc28b5d2d1f102994")]
	internal static extern int sqlite3_blob_bytes(IntPtr blob);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI327cfc7a6b1fd1fb")]
	internal static extern SQLiteErrorCode sqlite3_blob_open(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, long rowId, int flags, ref IntPtr ptrBlob);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIe25e8f29ef2a78eb")]
	internal static extern SQLiteErrorCode sqlite3_blob_read(IntPtr blob, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, int count, int offset);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI1ae480d1861ed022")]
	internal static extern SQLiteErrorCode sqlite3_blob_reopen(IntPtr blob, long rowId);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI674c3bc07fc17979")]
	internal static extern SQLiteErrorCode sqlite3_blob_write(IntPtr blob, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, int count, int offset);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIfdb97bb7d9d0d622")]
	internal static extern SQLiteErrorCode sqlite3_declare_vtab(IntPtr db, IntPtr zSQL);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI1c7a7970970b9619")]
	internal static extern IntPtr sqlite3_mprintf(IntPtr format, __arglist);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI252aeb4a9d3dc2ff")]
	internal static extern IntPtr sqlite3_create_disposable_module(IntPtr db, IntPtr name, ref sqlite3_module module, IntPtr pClientData, xDestroyModule xDestroy);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIb739af05c351a5b0")]
	internal static extern void sqlite3_dispose_module(IntPtr pModule);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI7164645d0504ae24")]
	internal static extern long sqlite3session_memory_used(IntPtr session);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf499bb5c421377e7")]
	internal static extern SQLiteErrorCode sqlite3session_create(IntPtr db, byte[] dbName, ref IntPtr session);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI6901b186cfd089f2")]
	internal static extern void sqlite3session_delete(IntPtr session);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIef12447577961bdf")]
	internal static extern int sqlite3session_enable(IntPtr session, int enable);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI3efc17e9bbe52434")]
	internal static extern int sqlite3session_indirect(IntPtr session, int indirect);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI72621cc08869a205")]
	internal static extern SQLiteErrorCode sqlite3session_attach(IntPtr session, byte[] tblName);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI94d8f51c6c1db577")]
	internal static extern void sqlite3session_table_filter(IntPtr session, xSessionFilter xFilter, IntPtr context);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIe6405dfd7b63ead9")]
	internal static extern SQLiteErrorCode sqlite3session_changeset(IntPtr session, ref int nChangeSet, ref IntPtr pChangeSet);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIecd4a7ef4a5968a1")]
	internal static extern SQLiteErrorCode sqlite3session_diff(IntPtr session, byte[] fromDbName, byte[] tblName, ref IntPtr errMsg);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIb4e97c410be9a2cf")]
	internal static extern SQLiteErrorCode sqlite3session_patchset(IntPtr session, ref int nPatchSet, ref IntPtr pPatchSet);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI75fe921d50a95fe0")]
	internal static extern int sqlite3session_isempty(IntPtr session);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI1bc1bc471c8cd143")]
	internal static extern SQLiteErrorCode sqlite3changeset_start(ref IntPtr iterator, int nChangeSet, IntPtr pChangeSet);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI8162bd921f89d839")]
	internal static extern SQLiteErrorCode sqlite3changeset_start_v2(ref IntPtr iterator, int nChangeSet, IntPtr pChangeSet, SQLiteChangeSetStartFlags flags);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI783104ea11038afc")]
	internal static extern SQLiteErrorCode sqlite3changeset_next(IntPtr iterator);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI588da80b6a264802")]
	internal static extern SQLiteErrorCode sqlite3changeset_op(IntPtr iterator, ref IntPtr pTblName, ref int nColumns, ref SQLiteAuthorizerActionCode op, ref int bIndirect);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI9517b76a4f0b657e")]
	internal static extern SQLiteErrorCode sqlite3changeset_pk(IntPtr iterator, ref IntPtr pPrimaryKeys, ref int nColumns);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf21356bf3fa1a8ef")]
	internal static extern SQLiteErrorCode sqlite3changeset_old(IntPtr iterator, int columnIndex, ref IntPtr pValue);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI52058dea2268cd04")]
	internal static extern SQLiteErrorCode sqlite3changeset_new(IntPtr iterator, int columnIndex, ref IntPtr pValue);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI568a7d0a1f4d0ff4")]
	internal static extern SQLiteErrorCode sqlite3changeset_conflict(IntPtr iterator, int columnIndex, ref IntPtr pValue);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI5b2b3e8bff031a75")]
	internal static extern SQLiteErrorCode sqlite3changeset_fk_conflicts(IntPtr iterator, ref int conflicts);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf71b64d343ddea8b")]
	internal static extern SQLiteErrorCode sqlite3changeset_finalize(IntPtr iterator);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI6cfa851319f410c1")]
	internal static extern SQLiteErrorCode sqlite3changeset_invert(int nIn, IntPtr pIn, ref int nOut, ref IntPtr pOut);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI9662fa92cee70cf1")]
	internal static extern SQLiteErrorCode sqlite3changeset_concat(int nA, IntPtr pA, int nB, IntPtr pB, ref int nOut, ref IntPtr pOut);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI5c7fb49466251e7d")]
	internal static extern SQLiteErrorCode sqlite3changegroup_new(ref IntPtr changeGroup);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI979d7afd84d6f0ca")]
	internal static extern SQLiteErrorCode sqlite3changegroup_add(IntPtr changeGroup, int nData, IntPtr pData);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIc26100c97b176329")]
	internal static extern SQLiteErrorCode sqlite3changegroup_output(IntPtr changeGroup, ref int nData, ref IntPtr pData);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI12409cae6ddf2f1e")]
	internal static extern void sqlite3changegroup_delete(IntPtr changeGroup);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIcc134e65c80c0c61")]
	internal static extern SQLiteErrorCode sqlite3changeset_apply(IntPtr db, int nChangeSet, IntPtr pChangeSet, xSessionFilter xFilter, xSessionConflict xConflict, IntPtr context);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIf8b1716e85b0e192")]
	internal static extern SQLiteErrorCode sqlite3changeset_apply_strm(IntPtr db, xSessionInput xInput, IntPtr pIn, xSessionFilter xFilter, xSessionConflict xConflict, IntPtr context);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIec91f505db92f298")]
	internal static extern SQLiteErrorCode sqlite3changeset_concat_strm(xSessionInput xInputA, IntPtr pInA, xSessionInput xInputB, IntPtr pInB, xSessionOutput xOutput, IntPtr pOut);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI3d527d6e902c8427")]
	internal static extern SQLiteErrorCode sqlite3changeset_invert_strm(xSessionInput xInput, IntPtr pIn, xSessionOutput xOutput, IntPtr pOut);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI2b5f6f3d24eb7ba3")]
	internal static extern SQLiteErrorCode sqlite3changeset_start_strm(ref IntPtr iterator, xSessionInput xInput, IntPtr pIn);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI2409c7e1eb500ce9")]
	internal static extern SQLiteErrorCode sqlite3changeset_start_v2_strm(ref IntPtr iterator, xSessionInput xInput, IntPtr pIn, SQLiteChangeSetStartFlags flags);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI88974d2e6bd51eff")]
	internal static extern SQLiteErrorCode sqlite3session_changeset_strm(IntPtr session, xSessionOutput xOutput, IntPtr pOut);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SIdc6920f226dceb74")]
	internal static extern SQLiteErrorCode sqlite3session_patchset_strm(IntPtr session, xSessionOutput xOutput, IntPtr pOut);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI150953dc62f0a879")]
	internal static extern SQLiteErrorCode sqlite3changegroup_add_strm(IntPtr changeGroup, xSessionInput xInput, IntPtr pIn);

	[DllImport("SQLite.Interop.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SI50a9b553ac0b2e2a")]
	internal static extern SQLiteErrorCode sqlite3changegroup_output_strm(IntPtr changeGroup, xSessionOutput xOutput, IntPtr pOut);
}

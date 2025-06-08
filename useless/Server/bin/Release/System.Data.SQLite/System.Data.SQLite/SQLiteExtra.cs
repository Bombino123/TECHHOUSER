#define TRACE
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Data.SQLite;

[Obfuscation(Feature = "renaming")]
public static class SQLiteExtra
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	[SuppressUnmanagedCodeSecurity]
	private delegate int FExecuteInAppDomainCallback(IntPtr pCookie);

	private static readonly Assembly EntryAssembly = Assembly.GetEntryAssembly();

	private static readonly Assembly ThisAssembly = Assembly.GetExecutingAssembly();

	private const string VerifyPublicKeyToken = "433d9874d0bb98c5";

	private const string VerifyAssemblyNameFormat = "System.Data.SQLite.SEE.License, Version=1.0.115.5, Culture=neutral, PublicKeyToken={0}, processorArchitecture=MSIL";

	private const string VerifyTypeName = "License.Sdk.Library";

	private const string VerifyMethodName = "Verify";

	private const string NativeVerifyEnvVarFormat = "SdkCallback_{0:X}_{1:X}_{2:X}";

	private const string NativeVerifyMethodName = "NativeVerify";

	private const string FileName = "SDS-SEE.exml";

	private const string BasePurchaseUri = "https://urn.to/r/sds_see";

	private const string UnknownValue = "<unknown>";

	private const string FileNameEnvVarName = "Override_SEE_Certificate";

	private const string NoPurchaseUriEnvVarName = "No_SEE_PurchaseUri";

	private const string NoPreVerifyEnvVarName = "No_SEE_PreVerify";

	private const string OtherAppDomainEnvVarName = "LicenseOtherAppDomain";

	private static readonly object syncRoot = new object();

	private static Assembly verifyAssembly = null;

	private static Type verifyType = null;

	private static MethodInfo verifyMethodInfo = null;

	private static int preVerifyPendingCount = 0;

	private static string verifyDirectory = null;

	private static int verifyCount = 0;

	private static int verifyPurchaseCount = 0;

	private static double verifyMilliseconds = 0.0;

	private static MethodInfo nativeMethodInfo = null;

	private static Delegate nativeDelegate = null;

	private static IntPtr pNativeCallback = IntPtr.Zero;

	private static Dictionary<string, string> nativeEnvironmentVariables = null;

	private static string GetAssemblyTitle(Assembly assembly)
	{
		if (assembly == null)
		{
			return null;
		}
		try
		{
			if (assembly.IsDefined(typeof(AssemblyTitleAttribute), inherit: false))
			{
				object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), inherit: false);
				if (customAttributes == null || customAttributes.Length == 0)
				{
					return null;
				}
				if (customAttributes[0] is AssemblyTitleAttribute assemblyTitleAttribute)
				{
					return assemblyTitleAttribute.Title;
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private static string BuildUri(string baseUri, Assembly assembly, string error)
	{
		if (string.IsNullOrEmpty(baseUri))
		{
			return baseUri;
		}
		string assemblyTitle = GetAssemblyTitle(assembly);
		return string.Format("{0}?app={1}&err={2}", baseUri, Uri.EscapeUriString((assemblyTitle != null) ? assemblyTitle : "<unknown>"), Uri.EscapeUriString((error != null) ? error : "<unknown>"));
	}

	private static int InnerVerify(string argument)
	{
		lock (syncRoot)
		{
			if (verifyAssembly == null)
			{
				verifyAssembly = Assembly.Load(string.Format("System.Data.SQLite.SEE.License, Version=1.0.115.5, Culture=neutral, PublicKeyToken={0}, processorArchitecture=MSIL", "433d9874d0bb98c5"));
			}
			if (verifyType == null)
			{
				verifyType = Type.GetType(Assembly.CreateQualifiedName(verifyAssembly.GetName().FullName, "License.Sdk.Library"));
			}
			if (verifyMethodInfo == null)
			{
				verifyMethodInfo = verifyType.GetMethod("Verify");
			}
			string text = Environment.GetEnvironmentVariable("Override_SEE_Certificate");
			if (text == null)
			{
				if (verifyDirectory == null)
				{
					if (EntryAssembly != null)
					{
						verifyDirectory = Path.GetDirectoryName(EntryAssembly.Location);
					}
					else if (ThisAssembly != null)
					{
						verifyDirectory = Path.GetDirectoryName(ThisAssembly.Location);
					}
				}
				if (verifyDirectory == null)
				{
					verifyDirectory = Directory.GetCurrentDirectory();
				}
				text = Path.Combine(verifyDirectory, "SDS-SEE.exml");
			}
			IList<string> list = null;
			string text2 = null;
			object[] array = new object[3] { text, list, text2 };
			if ((bool)verifyMethodInfo.Invoke(null, array))
			{
				if (!(array[1] is IList<string> list2))
				{
					text2 = "invalid license certificate";
				}
				else if (list2.Count < 4)
				{
					text2 = "malformed license certificate";
				}
				else if (!string.Equals(list2[0], "fileName", StringComparison.OrdinalIgnoreCase) || !string.Equals(list2[1], text, StringComparison.OrdinalIgnoreCase))
				{
					text2 = "bad certificate file name";
				}
				else
				{
					if (string.Equals(list2[2], "publicKeyToken", StringComparison.OrdinalIgnoreCase) && string.Equals(list2[3], "433d9874d0bb98c5", StringComparison.OrdinalIgnoreCase))
					{
						return 0;
					}
					text2 = "bad certificate public key";
				}
			}
			else
			{
				text2 = array[2] as string;
			}
			if (Interlocked.CompareExchange(ref preVerifyPendingCount, 0, 0) == 0 && Environment.GetEnvironmentVariable("No_SEE_PurchaseUri") == null && Interlocked.Increment(ref verifyPurchaseCount) == 1 && Environment.UserInteractive)
			{
				try
				{
					Process.Start(BuildUri("https://urn.to/r/sds_see", EntryAssembly, text2));
				}
				catch
				{
				}
			}
			throw new NotSupportedException(text2);
		}
	}

	private static void PreVerifyCallback(object state)
	{
		if (Interlocked.CompareExchange(ref verifyCount, 0, 0) > 0)
		{
			return;
		}
		Interlocked.Increment(ref preVerifyPendingCount);
		try
		{
			Verify(null);
		}
		catch
		{
		}
		finally
		{
			Interlocked.Decrement(ref preVerifyPendingCount);
		}
	}

	private static void TrackNativeEnvironmentVariable(string variable)
	{
		lock (syncRoot)
		{
			if (nativeEnvironmentVariables == null)
			{
				AddNativeExitedEventHandler();
				nativeEnvironmentVariables = new Dictionary<string, string>();
			}
			if (!string.IsNullOrEmpty(variable) && !nativeEnvironmentVariables.ContainsKey(variable))
			{
				nativeEnvironmentVariables.Add(variable, null);
			}
		}
	}

	private static void CleanupNativeEnvironmentVariables(object sender, EventArgs e)
	{
		lock (syncRoot)
		{
			if (nativeEnvironmentVariables == null)
			{
				return;
			}
			int num = 0;
			foreach (KeyValuePair<string, string> nativeEnvironmentVariable in nativeEnvironmentVariables)
			{
				string key = nativeEnvironmentVariable.Key;
				if (!string.IsNullOrEmpty(key))
				{
					try
					{
						Environment.SetEnvironmentVariable(key, null);
					}
					catch (Exception arg)
					{
						Trace.WriteLine($"Could not delete environment variable \"{key}\": {arg}");
					}
					num++;
				}
			}
			Trace.WriteLine($"Deleted {num} native verify callbacks from within application domain {AppDomain.CurrentDomain.Id}.");
			nativeEnvironmentVariables.Clear();
			nativeEnvironmentVariables = null;
		}
	}

	private static void AddNativeExitedEventHandler()
	{
		AppDomain currentDomain = AppDomain.CurrentDomain;
		if (currentDomain.IsDefaultAppDomain())
		{
			currentDomain.ProcessExit -= CleanupNativeEnvironmentVariables;
			currentDomain.ProcessExit += CleanupNativeEnvironmentVariables;
		}
		else
		{
			currentDomain.DomainUnload -= CleanupNativeEnvironmentVariables;
			currentDomain.DomainUnload += CleanupNativeEnvironmentVariables;
		}
	}

	private static int NativeVerify(IntPtr pCookie)
	{
		int val = Verify(null);
		if (pCookie != IntPtr.Zero)
		{
			Marshal.WriteInt32(pCookie, val);
		}
		return 0;
	}

	private static int SetupNativeVerify(string variable)
	{
		bool flag = false;
		IntPtr intPtr;
		lock (syncRoot)
		{
			if (nativeMethodInfo == null)
			{
				nativeMethodInfo = typeof(SQLiteExtra).GetMethod("NativeVerify", BindingFlags.Static | BindingFlags.NonPublic);
			}
			if ((object)nativeDelegate == null && nativeMethodInfo != null)
			{
				nativeDelegate = Delegate.CreateDelegate(typeof(FExecuteInAppDomainCallback), nativeMethodInfo);
			}
			if (pNativeCallback == IntPtr.Zero && (object)nativeDelegate != null)
			{
				pNativeCallback = Marshal.GetFunctionPointerForDelegate(nativeDelegate);
				flag = true;
			}
			intPtr = pNativeCallback;
		}
		if (!string.IsNullOrEmpty(variable))
		{
			TrackNativeEnvironmentVariable(variable);
			Environment.SetEnvironmentVariable(variable, intPtr.ToInt64().ToString());
			Trace.WriteLine(string.Format("{0} native verify callback \"{1}\" with value {2} from within application domain {3}.", flag ? "Created" : "Set", variable, intPtr, AppDomain.CurrentDomain.Id));
			return 0;
		}
		return 1;
	}

	internal static void PreVerify()
	{
		if (Environment.GetEnvironmentVariable("No_SEE_PreVerify") == null)
		{
			ThreadPool.QueueUserWorkItem(PreVerifyCallback);
		}
	}

	public static int Verify(string argument)
	{
		DateTime utcNow = DateTime.UtcNow;
		try
		{
			if (Environment.GetEnvironmentVariable("LicenseOtherAppDomain") != null && SetupNativeVerify($"SdkCallback_{Process.GetCurrentProcess().Id:X}_{AppDomain.CurrentDomain.Id:X}_{AppDomain.GetCurrentThreadId():X}") != 0)
			{
				return 1;
			}
			return InnerVerify(argument);
		}
		finally
		{
			int num = Interlocked.Increment(ref verifyCount);
			double totalMilliseconds = DateTime.UtcNow.Subtract(utcNow).TotalMilliseconds;
			verifyMilliseconds += totalMilliseconds;
			Trace.WriteLine($"Verify completed in {totalMilliseconds} milliseconds, total of {num} times in {verifyMilliseconds} milliseconds.");
		}
	}
}

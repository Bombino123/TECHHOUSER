using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Plugin.Helper;

internal class SuperCore
{
	private struct TOKEN_PRIVILEGES
	{
		internal uint PrivilegeCount;

		internal LUID Luid;

		internal uint Attrs;
	}

	private struct LUID
	{
		internal int LowPart;

		internal uint HighPart;
	}

	[Flags]
	private enum ProcessAccessFlags : uint
	{
		All = 0x1F0FFFu,
		Terminate = 1u,
		CreateThread = 2u,
		VirtualMemoryOperation = 8u,
		VirtualMemoryRead = 0x10u,
		VirtualMemoryWrite = 0x20u,
		DuplicateHandle = 0x40u,
		CreateProcess = 0x80u,
		SetQuota = 0x100u,
		SetInformation = 0x200u,
		QueryInformation = 0x400u,
		QueryLimitedInformation = 0x1000u,
		Synchronize = 0x100000u
	}

	private struct SECURITY_ATTRIBUTES
	{
		public int nLength;

		public IntPtr lpSecurityDescriptor;

		public int bInheritHandle;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct STARTUPINFO
	{
		public int cb;

		public string lpReserved;

		public string lpDesktop;

		public string lpTitle;

		public int dwX;

		public int dwY;

		public int dwXSize;

		public int dwYSize;

		public int dwXCountChars;

		public int dwYCountChars;

		public int dwFillAttribute;

		public int dwFlags;

		public short wShowWindow;

		public short cbReserved2;

		public IntPtr lpReserved2;

		public IntPtr hStdInput;

		public IntPtr hStdOutput;

		public IntPtr hStdError;
	}

	private struct PROCESSINFO
	{
		public IntPtr hProcess;

		public IntPtr hThread;

		public int dwProcessId;

		public int dwThreadId;
	}

	private enum LogonFlags
	{
		WithProfile = 1,
		NetCredentialsOnly
	}

	private enum SecurityImpersonationLevel
	{
		SecurityAnonymous,
		SecurityIdentification,
		SecurityImpersonation,
		SecurityDelegation
	}

	private enum TokenType
	{
		TokenPrimary = 1,
		TokenImpersonation
	}

	public enum ComplainReason
	{
		FileNotFound,
		FileNotExe,
		CantGetPID,
		APICallFailed
	}

	public delegate void ComplainHandler(ComplainReason reason, string obj);

	private const uint SE_PRIVILEGE_ENABLED = 2u;

	private const uint STANDARD_RIGHTS_REQUIRED = 983040u;

	private const uint STANDARD_RIGHTS_READ = 131072u;

	private const uint TOKEN_ASSIGN_PRIMARY = 1u;

	private const uint TOKEN_DUPLICATE = 2u;

	private const uint TOKEN_IMPERSONATE = 4u;

	private const uint TOKEN_QUERY = 8u;

	private const uint TOKEN_QUERY_SOURCE = 16u;

	private const uint TOKEN_ADJUST_PRIVILEGES = 32u;

	private const uint TOKEN_ADJUST_GROUPS = 64u;

	private const uint TOKEN_ADJUST_DEFAULT = 128u;

	private const uint TOKEN_ADJUST_SESSIONID = 256u;

	private const uint TOKEN_READ = 131080u;

	private const uint TOKEN_ALL_ACCESS = 983551u;

	private const uint TOKEN_INFORMATION_CLASS_TokenSessionId = 12u;

	private const uint NORMAL_PRIORITY_CLASS = 32u;

	private const uint CREATE_NEW_CONSOLE = 16u;

	private const uint CREATE_UNICODE_ENVIRONMENT = 1024u;

	private static string _Log;

	private static bool plsThrow;

	public static bool ForceTokenUseActiveSessionID;

	private static STARTUPINFO SI;

	private static PROCESSINFO PI;

	private static SECURITY_ATTRIBUTES dummySA;

	private static IntPtr hProc;

	private static IntPtr hToken;

	private static IntPtr hDupToken;

	private static IntPtr pEnvBlock;

	public static string Log
	{
		get
		{
			return _Log;
		}
		private set
		{
			_Log = value + Environment.NewLine;
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(IntPtr hObject);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetCurrentProcess();

	[DllImport("kernel32.dll")]
	private static extern uint WTSGetActiveConsoleSessionId();

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, uint dummy, IntPtr dummy2, IntPtr dummy3);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int processId);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool SetTokenInformation(IntPtr TokenHandle, uint TokenInformationClass, ref uint TokenInformation, uint TokenInformationLength);

	[DllImport("userenv.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

	[DllImport("userenv.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool CreateProcessAsUserW(IntPtr hToken, string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESSINFO lpProcessInformation);

	[DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool CreateProcessWithTokenW(IntPtr hToken, LogonFlags dwLogonFlags, string lpApplicationName, string lpCommandLine, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESSINFO lpProcessInformation);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref SECURITY_ATTRIBUTES lpTokenAttributes, SecurityImpersonationLevel ImpersonationLevel, TokenType TokenType, out IntPtr phNewToken);

	public static void ClearLog()
	{
		_Log = "";
	}

	private static void EndLog()
	{
		Log = Log + " ---- end of log : " + DateTime.Now.ToString("d MMM yyyy hh:mm:ss tt") + " ---- ";
	}

	private static void LogErr(string msg, bool cleanup = true)
	{
		if (cleanup)
		{
			CleanUp();
		}
		Log += msg;
		EndLog();
	}

	private static void GoComplain(ComplainReason reason, string obj)
	{
		switch (reason)
		{
		case ComplainReason.FileNotFound:
			LogErr("ExeToRun is not an existing file!");
			break;
		case ComplainReason.FileNotExe:
			LogErr("ExeToRun is not an executable file!");
			break;
		case ComplainReason.CantGetPID:
			LogErr("Can't get the PID of: " + obj);
			break;
		case ComplainReason.APICallFailed:
			obj = obj + ": " + WinAPILastErrMsg();
			LogErr(obj);
			break;
		default:
			LogErr("");
			break;
		}
		if (plsThrow)
		{
			plsThrow = false;
			throw new Exception();
		}
	}

	public static void RunWithTokenOf(string ProcessName, bool OfActiveSessionOnly, string ExeToRun, string Arguments, string WorkingDir = "")
	{
		List<int> list = new List<int>();
		Process[] processesByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ProcessName));
		foreach (Process process in processesByName)
		{
			if (OfActiveSessionOnly)
			{
				if (process.SessionId == WTSGetActiveConsoleSessionId())
				{
					list.Add(process.Id);
				}
			}
			else
			{
				list.Add(process.Id);
			}
		}
		if (list.Count == 0)
		{
			GoComplain(ComplainReason.CantGetPID, ProcessName);
		}
		else
		{
			RunWithTokenOf(list[0], ExeToRun, Arguments, WorkingDir);
		}
	}

	public static void RunWithTokenOf(int ProcessID, string ExeToRun, string Arguments, string WorkingDir = "")
	{
		plsThrow = true;
		try
		{
			ExeToRun = Environment.ExpandEnvironmentVariables(ExeToRun);
			string[] array;
			if (!ExeToRun.Contains("\\"))
			{
				array = Environment.ExpandEnvironmentVariables("%path%").Split(new char[1] { ';' });
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i] + "\\" + ExeToRun;
					if (File.Exists(text))
					{
						ExeToRun = text;
						break;
					}
				}
			}
			if (!File.Exists(ExeToRun))
			{
				GoComplain(ComplainReason.FileNotFound, ExeToRun);
			}
			WorkingDir = Environment.ExpandEnvironmentVariables(WorkingDir);
			if (!Directory.Exists(WorkingDir))
			{
				WorkingDir = Path.GetDirectoryName(ExeToRun);
			}
			Arguments = Environment.ExpandEnvironmentVariables(Arguments);
			string lpCommandLine = null;
			if (Arguments != "")
			{
				lpCommandLine = ((!ExeToRun.Contains(" ")) ? (ExeToRun + " " + Arguments) : ("\"" + ExeToRun + "\" " + Arguments));
			}
			Log = Log + "Running as user: " + Environment.UserName;
			string obj = "OpenProcessToken";
			if (!OpenProcessToken(GetCurrentProcess(), 983551u, out hToken))
			{
				GoComplain(ComplainReason.APICallFailed, obj);
			}
			array = "SeDebugPrivilege".Split(new char[1] { ',' });
			foreach (string text2 in array)
			{
				obj = "LookupPrivilegeValue (" + text2 + ")";
				if (!LookupPrivilegeValue("", text2, out var lpLuid))
				{
					GoComplain(ComplainReason.APICallFailed, obj);
				}
				obj = "AdjustTokenPrivileges (" + text2 + ")";
				TOKEN_PRIVILEGES NewState = default(TOKEN_PRIVILEGES);
				NewState.PrivilegeCount = 1u;
				NewState.Luid = lpLuid;
				NewState.Attrs = 2u;
				if (AdjustTokenPrivileges(hToken, DisableAllPrivileges: false, ref NewState, 0u, IntPtr.Zero, IntPtr.Zero) & (Marshal.GetLastWin32Error() == 0))
				{
					Log = Log + obj + ": OK!";
				}
				else
				{
					GoComplain(ComplainReason.APICallFailed, obj);
				}
			}
			CloseHandle(hToken);
			obj = "OpenProcess (PID: " + ProcessID + ")";
			hProc = OpenProcess(ProcessAccessFlags.All, bInheritHandle: false, ProcessID);
			_ = hProc;
			Log = Log + obj + ": OK!";
			obj = "OpenProcessToken (TOKEN_DUPLICATE | TOKEN_QUERY)";
			if (!OpenProcessToken(hProc, 10u, out hToken))
			{
				GoComplain(ComplainReason.APICallFailed, obj);
			}
			Log = Log + obj + ": OK!";
			obj = "DuplicateTokenEx (TOKEN_ALL_ACCESS)";
			if (!DuplicateTokenEx(hToken, 983551u, ref dummySA, SecurityImpersonationLevel.SecurityIdentification, TokenType.TokenPrimary, out hDupToken))
			{
				GoComplain(ComplainReason.APICallFailed, obj);
			}
			Log = Log + obj + ": OK!";
			if (ForceTokenUseActiveSessionID)
			{
				obj = "SetTokenInformation (toActiveSessionID)";
				uint TokenInformation = WTSGetActiveConsoleSessionId();
				if (!SetTokenInformation(hDupToken, 12u, ref TokenInformation, 4u))
				{
					GoComplain(ComplainReason.APICallFailed, obj);
				}
				Log = Log + obj + ": OK!";
			}
			obj = "CreateEnvironmentBlock";
			if (!CreateEnvironmentBlock(out pEnvBlock, hToken, bInherit: true))
			{
				GoComplain(ComplainReason.APICallFailed, obj);
			}
			Log = Log + obj + ": OK!\n";
			uint dwCreationFlags = 1072u;
			SI = default(STARTUPINFO);
			SI.cb = Marshal.SizeOf((object)SI);
			SI.lpDesktop = "winsta0\\default";
			PI = default(PROCESSINFO);
			obj = "CreateProcessWithTokenW";
			if (CreateProcessWithTokenW(hDupToken, LogonFlags.WithProfile, ExeToRun, lpCommandLine, dwCreationFlags, pEnvBlock, WorkingDir, ref SI, out PI))
			{
				Log += "CreateProcessWithTokenW: Done! New process created successfully!";
			}
			else
			{
				Log = Log + "CreateProcessWithTokenW: " + WinAPILastErrMsg();
				Log += "\nTrying CreateProcessAsUserW instead.";
				obj = "CreateProcessAsUserW";
				if (CreateProcessAsUserW(hDupToken, ExeToRun, lpCommandLine, ref dummySA, ref dummySA, bInheritHandles: false, dwCreationFlags, pEnvBlock, WorkingDir, ref SI, out PI))
				{
					Log = Log + obj + ": Done! New process created successfully!";
				}
				else
				{
					switch (Marshal.GetLastWin32Error())
					{
					case 3:
						GoComplain(ComplainReason.FileNotFound, ExeToRun);
						break;
					case 193:
						GoComplain(ComplainReason.FileNotExe, ExeToRun);
						break;
					default:
						GoComplain(ComplainReason.APICallFailed, obj);
						break;
					}
				}
			}
			Log = Log + "Process name: " + Path.GetFileName(ExeToRun);
			Log = Log + "Process ID: " + PI.dwProcessId;
			CleanUp();
			EndLog();
		}
		catch (Exception)
		{
		}
	}

	private static void CleanUp()
	{
		CloseHandle(SI.hStdError);
		SI.hStdError = IntPtr.Zero;
		CloseHandle(SI.hStdInput);
		SI.hStdInput = IntPtr.Zero;
		CloseHandle(SI.hStdOutput);
		SI.hStdOutput = IntPtr.Zero;
		CloseHandle(PI.hThread);
		PI.hThread = IntPtr.Zero;
		CloseHandle(PI.hProcess);
		PI.hThread = IntPtr.Zero;
		DestroyEnvironmentBlock(pEnvBlock);
		pEnvBlock = IntPtr.Zero;
		CloseHandle(hDupToken);
		hDupToken = IntPtr.Zero;
		CloseHandle(hToken);
		hToken = IntPtr.Zero;
	}

	private static string WinAPILastErrMsg()
	{
		int lastWin32Error = Marshal.GetLastWin32Error();
		return new Win32Exception(lastWin32Error).Message + " (Error code: " + lastWin32Error + ")";
	}
}

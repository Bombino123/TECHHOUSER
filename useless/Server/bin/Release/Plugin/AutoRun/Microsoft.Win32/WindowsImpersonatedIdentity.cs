using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Microsoft.Win32;

internal class WindowsImpersonatedIdentity : IDisposable, IIdentity
{
	private const int LOGON_TYPE_NEW_CREDENTIALS = 9;

	private const int LOGON32_LOGON_INTERACTIVE = 2;

	private const int LOGON32_PROVIDER_DEFAULT = 0;

	private const int LOGON32_PROVIDER_WINNT50 = 3;

	private WindowsImpersonationContext impersonationContext;

	private NativeMethods.SafeTokenHandle token;

	private WindowsIdentity identity;

	public string AuthenticationType => identity?.AuthenticationType;

	public bool IsAuthenticated
	{
		get
		{
			if (identity != null)
			{
				return identity.IsAuthenticated;
			}
			return false;
		}
	}

	public string Name
	{
		get
		{
			if (identity != null)
			{
				return identity.Name;
			}
			return null;
		}
	}

	public WindowsImpersonatedIdentity(string userName, string domainName, string password)
	{
		if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(domainName) && string.IsNullOrEmpty(password))
		{
			identity = WindowsIdentity.GetCurrent();
			return;
		}
		if (NativeMethods.LogonUser(userName, domainName, password, (domainName == null) ? 9 : 2, (domainName == null) ? 3 : 0, out token) != 0)
		{
			identity = new WindowsIdentity(token.DangerousGetHandle());
			impersonationContext = identity.Impersonate();
			return;
		}
		throw new Win32Exception(Marshal.GetLastWin32Error());
	}

	public void Dispose()
	{
		if (impersonationContext != null)
		{
			impersonationContext.Undo();
		}
		token?.Dispose();
		if (identity != null)
		{
			identity.Dispose();
		}
	}
}

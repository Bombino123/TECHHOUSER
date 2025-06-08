using System;
using System.Security.Principal;

namespace Microsoft.Win32.TaskScheduler;

internal class User : IEquatable<User>, IDisposable
{
	private static readonly WindowsIdentity cur = WindowsIdentity.GetCurrent();

	private SecurityIdentifier sid;

	public static User Current => new User(cur);

	public WindowsIdentity Identity { get; private set; }

	public bool IsAdmin
	{
		get
		{
			if (Identity == null)
			{
				return false;
			}
			return new WindowsPrincipal(Identity).IsInRole(WindowsBuiltInRole.Administrator);
		}
	}

	public bool IsCurrent => Identity?.User.Equals(cur.User) ?? false;

	public bool IsServiceAccount
	{
		get
		{
			try
			{
				return sid != null && (sid.IsWellKnown(WellKnownSidType.LocalSystemSid) || sid.IsWellKnown(WellKnownSidType.NetworkServiceSid) || sid.IsWellKnown(WellKnownSidType.LocalServiceSid));
			}
			catch
			{
			}
			return false;
		}
	}

	public bool IsSystem
	{
		get
		{
			if (sid != null)
			{
				return sid.IsWellKnown(WellKnownSidType.LocalSystemSid);
			}
			return false;
		}
	}

	public string SidString => sid?.ToString();

	public string Name
	{
		get
		{
			object obj = Identity?.Name;
			if (obj == null)
			{
				NTAccount obj2 = (NTAccount)(sid?.Translate(typeof(NTAccount)));
				if ((object)obj2 == null)
				{
					return null;
				}
				obj = obj2.Value;
			}
			return (string)obj;
		}
	}

	public User(string userName = null)
	{
		if (string.IsNullOrEmpty(userName))
		{
			userName = null;
		}
		if (userName == null || cur.Name.Equals(userName, StringComparison.InvariantCultureIgnoreCase) || GetUser(cur.Name).Equals(userName, StringComparison.InvariantCultureIgnoreCase))
		{
			Identity = cur;
			sid = Identity.User;
		}
		else if (userName.Contains("\\") && !userName.StartsWith("NT AUTHORITY\\"))
		{
			try
			{
				using NativeMethods.DomainService domainService = new NativeMethods.DomainService();
				Identity = new WindowsIdentity(domainService.CrackName(userName));
				sid = Identity.User;
			}
			catch
			{
			}
		}
		if (Identity != null)
		{
			return;
		}
		if (userName != null && userName.Contains("@"))
		{
			Identity = new WindowsIdentity(userName);
			sid = Identity.User;
		}
		if (Identity == null && userName != null)
		{
			NTAccount nTAccount = new NTAccount(userName);
			try
			{
				sid = (SecurityIdentifier)nTAccount.Translate(typeof(SecurityIdentifier));
			}
			catch
			{
			}
		}
		static string GetUser(string domUser)
		{
			string[] array = domUser.Split(new char[1] { '\\' });
			if (array.Length != 2)
			{
				return domUser;
			}
			return array[1];
		}
	}

	internal User(WindowsIdentity wid)
	{
		Identity = wid;
		sid = wid.User;
	}

	public static User FromSidString(string sid)
	{
		return new User(((NTAccount)new SecurityIdentifier(sid).Translate(typeof(NTAccount))).Value);
	}

	public void Dispose()
	{
		Identity?.Dispose();
	}

	public override bool Equals(object obj)
	{
		if (obj is User other)
		{
			return Equals(other);
		}
		if (obj is WindowsIdentity windowsIdentity && sid != null)
		{
			return sid.Equals(windowsIdentity.User);
		}
		try
		{
			if (obj is string userName)
			{
				return Equals(new User(userName));
			}
		}
		catch
		{
		}
		return base.Equals(obj);
	}

	public bool Equals(User other)
	{
		if (other == null || !(sid != null))
		{
			return false;
		}
		return sid.Equals(other.sid);
	}

	public override int GetHashCode()
	{
		return sid?.GetHashCode() ?? 0;
	}
}

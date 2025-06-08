using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32;

internal static class NativeMethods
{
	[Flags]
	public enum AccessTypes : uint
	{
		TokenAssignPrimary = 1u,
		TokenDuplicate = 2u,
		TokenImpersonate = 4u,
		TokenQuery = 8u,
		TokenQuerySource = 0x10u,
		TokenAdjustPrivileges = 0x20u,
		TokenAdjustGroups = 0x40u,
		TokenAdjustDefault = 0x80u,
		TokenAdjustSessionID = 0x100u,
		TokenAllAccessP = 0xF00FFu,
		TokenAllAccess = 0xF01FFu,
		TokenRead = 0x20008u,
		TokenWrite = 0x200E0u,
		TokenExecute = 0x20000u,
		Delete = 0x10000u,
		ReadControl = 0x20000u,
		WriteDac = 0x40000u,
		WriteOwner = 0x80000u,
		Synchronize = 0x100000u,
		StandardRightsRequired = 0xF0000u,
		StandardRightsRead = 0x20000u,
		StandardRightsWrite = 0x20000u,
		StandardRightsExecute = 0x20000u,
		StandardRightsAll = 0x1F0000u,
		SpecificRightsAll = 0xFFFFu,
		AccessSystemSecurity = 0x1000000u,
		MaximumAllowed = 0x2000000u,
		GenericRead = 0x80000000u,
		GenericWrite = 0x40000000u,
		GenericExecute = 0x20000000u,
		GenericAll = 0x10000000u
	}

	[Flags]
	public enum PrivilegeAttributes : uint
	{
		Disabled = 0u,
		EnabledByDefault = 1u,
		Enabled = 2u,
		UsedForAccess = 0x80000000u
	}

	public enum SECURITY_IMPERSONATION_LEVEL
	{
		Anonymous,
		Identification,
		Impersonation,
		Delegation
	}

	public enum TOKEN_ELEVATION_TYPE
	{
		Default = 1,
		Full,
		Limited
	}

	public enum TOKEN_INFORMATION_CLASS
	{
		TokenUser = 1,
		TokenGroups,
		TokenPrivileges,
		TokenOwner,
		TokenPrimaryGroup,
		TokenDefaultDacl,
		TokenSource,
		TokenType,
		TokenImpersonationLevel,
		TokenStatistics,
		TokenRestrictedSids,
		TokenSessionId,
		TokenGroupsAndPrivileges,
		TokenSessionReference,
		TokenSandBoxInert,
		TokenAuditPolicy,
		TokenOrigin,
		TokenElevationType,
		TokenLinkedToken,
		TokenElevation,
		TokenHasRestrictions,
		TokenAccessInformation,
		TokenVirtualizationAllowed,
		TokenVirtualizationEnabled,
		TokenIntegrityLevel,
		TokenUIAccess,
		TokenMandatoryPolicy,
		TokenLogonSid,
		MaxTokenInfoClass
	}

	[Serializable]
	public enum TokenType
	{
		TokenImpersonation = 2,
		TokenPrimary = 1
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LUID
	{
		public uint LowPart;

		public int HighPart;

		public static LUID FromName(string name, string systemName = null)
		{
			if (!LookupPrivilegeValue(systemName, name, out var luid))
			{
				throw new Win32Exception();
			}
			return luid;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LUID_AND_ATTRIBUTES
	{
		public LUID Luid;

		public PrivilegeAttributes Attributes;

		public LUID_AND_ATTRIBUTES(LUID luid, PrivilegeAttributes attr)
		{
			Luid = luid;
			Attributes = attr;
		}
	}

	public struct PRIVILEGE_SET : IDisposable
	{
		public uint PrivilegeCount;

		public uint Control;

		public IntPtr Privilege;

		public PRIVILEGE_SET(uint control, params LUID_AND_ATTRIBUTES[] privileges)
		{
			PrivilegeCount = (uint)privileges.Length;
			Control = control;
			Privilege = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LUID_AND_ATTRIBUTES)) * (int)PrivilegeCount);
			for (int i = 0; i < PrivilegeCount; i++)
			{
				Marshal.StructureToPtr((object)privileges[i], (IntPtr)((int)Privilege + Marshal.SizeOf(typeof(LUID_AND_ATTRIBUTES)) * i), fDeleteOld: false);
			}
		}

		public void Dispose()
		{
			Marshal.FreeHGlobal(Privilege);
		}
	}

	public struct SID_AND_ATTRIBUTES
	{
		public IntPtr Sid;

		public uint Attributes;
	}

	public struct TOKEN_ELEVATION
	{
		public int TokenIsElevated;
	}

	public struct TOKEN_MANDATORY_LABEL
	{
		public SID_AND_ATTRIBUTES Label;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct TOKEN_PRIVILEGES
	{
		public uint PrivilegeCount;

		public LUID_AND_ATTRIBUTES Privileges;

		public static uint SizeInBytes => (uint)Marshal.SizeOf(typeof(TOKEN_PRIVILEGES));

		public TOKEN_PRIVILEGES(LUID luid, PrivilegeAttributes attribute)
		{
			PrivilegeCount = 1u;
			Privileges.Luid = luid;
			Privileges.Attributes = attribute;
		}
	}

	public class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private const int ERROR_NO_TOKEN = 1008;

		private const int ERROR_INSUFFICIENT_BUFFER = 122;

		private static SafeTokenHandle currentProcessToken;

		public T GetInfo<T>(TOKEN_INFORMATION_CLASS type)
		{
			int returnLength = Marshal.SizeOf(typeof(T));
			IntPtr intPtr = Marshal.AllocHGlobal(returnLength);
			try
			{
				if (!GetTokenInformation(this, type, intPtr, returnLength, out returnLength))
				{
					throw new Win32Exception();
				}
				switch (type)
				{
				case TOKEN_INFORMATION_CLASS.TokenType:
				case TOKEN_INFORMATION_CLASS.TokenImpersonationLevel:
				case TOKEN_INFORMATION_CLASS.TokenSessionId:
				case TOKEN_INFORMATION_CLASS.TokenSandBoxInert:
				case TOKEN_INFORMATION_CLASS.TokenOrigin:
				case TOKEN_INFORMATION_CLASS.TokenElevationType:
				case TOKEN_INFORMATION_CLASS.TokenHasRestrictions:
				case TOKEN_INFORMATION_CLASS.TokenVirtualizationAllowed:
				case TOKEN_INFORMATION_CLASS.TokenVirtualizationEnabled:
				case TOKEN_INFORMATION_CLASS.TokenUIAccess:
					return (T)Convert.ChangeType(Marshal.ReadInt32(intPtr), typeof(T));
				case TOKEN_INFORMATION_CLASS.TokenLinkedToken:
					return (T)Convert.ChangeType(Marshal.ReadIntPtr(intPtr), typeof(T));
				case TOKEN_INFORMATION_CLASS.TokenUser:
				case TOKEN_INFORMATION_CLASS.TokenGroups:
				case TOKEN_INFORMATION_CLASS.TokenPrivileges:
				case TOKEN_INFORMATION_CLASS.TokenOwner:
				case TOKEN_INFORMATION_CLASS.TokenPrimaryGroup:
				case TOKEN_INFORMATION_CLASS.TokenDefaultDacl:
				case TOKEN_INFORMATION_CLASS.TokenSource:
				case TOKEN_INFORMATION_CLASS.TokenStatistics:
				case TOKEN_INFORMATION_CLASS.TokenRestrictedSids:
				case TOKEN_INFORMATION_CLASS.TokenGroupsAndPrivileges:
				case TOKEN_INFORMATION_CLASS.TokenElevation:
				case TOKEN_INFORMATION_CLASS.TokenAccessInformation:
				case TOKEN_INFORMATION_CLASS.TokenIntegrityLevel:
				case TOKEN_INFORMATION_CLASS.TokenMandatoryPolicy:
				case TOKEN_INFORMATION_CLASS.TokenLogonSid:
					return (T)Marshal.PtrToStructure(intPtr, typeof(T));
				default:
					return default(T);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		public static SafeTokenHandle FromCurrentProcess(AccessTypes desiredAccess = AccessTypes.TokenDuplicate)
		{
			lock (currentProcessToken)
			{
				if (currentProcessToken == null)
				{
					currentProcessToken = FromProcess(GetCurrentProcess(), desiredAccess);
				}
				return currentProcessToken;
			}
		}

		public static SafeTokenHandle FromCurrentThread(AccessTypes desiredAccess = AccessTypes.TokenDuplicate, bool openAsSelf = true)
		{
			return FromThread(GetCurrentThread(), desiredAccess, openAsSelf);
		}

		public static SafeTokenHandle FromProcess(IntPtr hProcess, AccessTypes desiredAccess = AccessTypes.TokenDuplicate)
		{
			if (!OpenProcessToken(hProcess, desiredAccess, out var TokenHandle))
			{
				throw new Win32Exception();
			}
			return TokenHandle;
		}

		public static SafeTokenHandle FromThread(IntPtr hThread, AccessTypes desiredAccess = AccessTypes.TokenDuplicate, bool openAsSelf = true)
		{
			if (!OpenThreadToken(hThread, desiredAccess, openAsSelf, out var TokenHandle))
			{
				if (Marshal.GetLastWin32Error() != 1008)
				{
					throw new Win32Exception();
				}
				if (!DuplicateTokenEx(FromCurrentProcess(), AccessTypes.TokenImpersonate | desiredAccess, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.Impersonation, TokenType.TokenImpersonation, ref TokenHandle))
				{
					throw new Win32Exception();
				}
				if (!SetThreadToken(IntPtr.Zero, TokenHandle))
				{
					throw new Win32Exception();
				}
			}
			return TokenHandle;
		}

		private SafeTokenHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeTokenHandle(IntPtr handle, bool own = true)
			: base(own)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			return CloseHandle(handle);
		}
	}

	[Flags]
	public enum ServerTypes : uint
	{
		Workstation = 1u,
		Server = 2u,
		SqlServer = 4u,
		DomainCtrl = 8u,
		BackupDomainCtrl = 0x10u,
		TimeSource = 0x20u,
		AppleFilingProtocol = 0x40u,
		Novell = 0x80u,
		DomainMember = 0x100u,
		PrintQueueServer = 0x200u,
		DialinServer = 0x400u,
		XenixServer = 0x800u,
		UnixServer = 0x800u,
		NT = 0x1000u,
		WindowsForWorkgroups = 0x2000u,
		MicrosoftFileAndPrintServer = 0x4000u,
		NTServer = 0x8000u,
		BrowserService = 0x10000u,
		BackupBrowserService = 0x20000u,
		MasterBrowserService = 0x40000u,
		DomainMaster = 0x80000u,
		OSF1Server = 0x100000u,
		VMSServer = 0x200000u,
		Windows = 0x400000u,
		DFS = 0x800000u,
		NTCluster = 0x1000000u,
		TerminalServer = 0x2000000u,
		VirtualNTCluster = 0x4000000u,
		DCE = 0x10000000u,
		AlternateTransport = 0x20000000u,
		LocalListOnly = 0x40000000u,
		PrimaryDomain = 0x80000000u,
		All = uint.MaxValue
	}

	public enum ServerPlatform
	{
		DOS = 300,
		OS2 = 400,
		NT = 500,
		OSF = 600,
		VMS = 700
	}

	public struct SERVER_INFO_100
	{
		public ServerPlatform PlatformId;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Name;
	}

	public struct SERVER_INFO_101
	{
		public ServerPlatform PlatformId;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Name;

		public int VersionMajor;

		public int VersionMinor;

		public ServerTypes Type;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Comment;
	}

	public struct SERVER_INFO_102
	{
		public ServerPlatform PlatformId;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Name;

		public int VersionMajor;

		public int VersionMinor;

		public ServerTypes Type;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Comment;

		public int MaxUsers;

		public int AutoDisconnectMinutes;

		[MarshalAs(UnmanagedType.Bool)]
		public bool Hidden;

		public int NetworkAnnounceRate;

		public int NetworkAnnounceRateDelta;

		public int UsersPerLicense;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string UserDirectoryPath;
	}

	public struct NetworkComputerInfo
	{
		private ServerPlatform sv101_platform_id;

		[MarshalAs(UnmanagedType.LPWStr)]
		private string sv101_name;

		private int sv101_version_major;

		private int sv101_version_minor;

		private ServerTypes sv101_type;

		[MarshalAs(UnmanagedType.LPWStr)]
		private string sv101_comment;

		public ServerPlatform Platform => sv101_platform_id;

		public string Name => sv101_name;

		public string Comment => sv101_comment;

		public ServerTypes ServerTypes => sv101_type;

		public Version Version => new Version(sv101_version_major, sv101_version_minor);
	}

	public enum DS_NAME_ERROR : uint
	{
		DS_NAME_NO_ERROR,
		DS_NAME_ERROR_RESOLVING,
		DS_NAME_ERROR_NOT_FOUND,
		DS_NAME_ERROR_NOT_UNIQUE,
		DS_NAME_ERROR_NO_MAPPING,
		DS_NAME_ERROR_DOMAIN_ONLY,
		DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING,
		DS_NAME_ERROR_TRUST_REFERRAL
	}

	[Flags]
	public enum DS_NAME_FLAGS
	{
		DS_NAME_NO_FLAGS = 0,
		DS_NAME_FLAG_SYNTACTICAL_ONLY = 1,
		DS_NAME_FLAG_EVAL_AT_DC = 2,
		DS_NAME_FLAG_GCVERIFY = 4,
		DS_NAME_FLAG_TRUST_REFERRAL = 8
	}

	public enum DS_NAME_FORMAT
	{
		DS_UNKNOWN_NAME = 0,
		DS_FQDN_1779_NAME = 1,
		DS_NT4_ACCOUNT_NAME = 2,
		DS_DISPLAY_NAME = 3,
		DS_UNIQUE_ID_NAME = 6,
		DS_CANONICAL_NAME = 7,
		DS_USER_PRINCIPAL_NAME = 8,
		DS_CANONICAL_NAME_EX = 9,
		DS_SERVICE_PRINCIPAL_NAME = 10,
		DS_SID_OR_SID_HISTORY_NAME = 11
	}

	[SuppressUnmanagedCodeSecurity]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public class DomainService : IDisposable
	{
		private IntPtr handle = IntPtr.Zero;

		public DomainService(string domainControllerName = null, string dnsDomainName = null)
		{
			DsBind(domainControllerName, dnsDomainName, out handle);
		}

		public string CrackName(string name)
		{
			DS_NAME_RESULT_ITEM[] array = CrackNames(new string[1] { name });
			if (array == null || array.Length == 0 || array[0].status != 0)
			{
				throw new SecurityException("Unable to resolve user name.");
			}
			return array[0].pName;
		}

		public DS_NAME_RESULT_ITEM[] CrackNames(string[] names = null, DS_NAME_FLAGS flags = DS_NAME_FLAGS.DS_NAME_NO_FLAGS, DS_NAME_FORMAT formatOffered = DS_NAME_FORMAT.DS_UNKNOWN_NAME, DS_NAME_FORMAT formatDesired = DS_NAME_FORMAT.DS_USER_PRINCIPAL_NAME)
		{
			IntPtr ppResult;
			uint num = DsCrackNames(handle, flags, formatOffered, formatDesired, (names != null) ? ((uint)names.Length) : 0u, names, out ppResult);
			if (num != 0)
			{
				throw new Win32Exception((int)num);
			}
			try
			{
				return ((DS_NAME_RESULT)Marshal.PtrToStructure(ppResult, typeof(DS_NAME_RESULT))).Items;
			}
			finally
			{
				DsFreeNameResult(ppResult);
			}
		}

		public void Dispose()
		{
			DsUnBind(ref handle);
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct DS_NAME_RESULT
	{
		public uint cItems;

		internal IntPtr rItems;

		public DS_NAME_RESULT_ITEM[] Items
		{
			get
			{
				if (rItems == IntPtr.Zero)
				{
					return new DS_NAME_RESULT_ITEM[0];
				}
				DS_NAME_RESULT_ITEM[] array = new DS_NAME_RESULT_ITEM[cItems];
				Type typeFromHandle = typeof(DS_NAME_RESULT_ITEM);
				int num = Marshal.SizeOf(typeFromHandle);
				for (uint num2 = 0u; num2 < cItems; num2++)
				{
					IntPtr ptr = new IntPtr(rItems.ToInt64() + num2 * num);
					array[num2] = (DS_NAME_RESULT_ITEM)Marshal.PtrToStructure(ptr, typeFromHandle);
				}
				return array;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct DS_NAME_RESULT_ITEM
	{
		public DS_NAME_ERROR status;

		public string pDomain;

		public string pName;

		public override string ToString()
		{
			if (status == DS_NAME_ERROR.DS_NAME_NO_ERROR)
			{
				return pName;
			}
			return string.Empty;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct SYSTEMTIME : IConvertible
	{
		public ushort Year;

		public ushort Month;

		public ushort DayOfWeek;

		public ushort Day;

		public ushort Hour;

		public ushort Minute;

		public ushort Second;

		public ushort Milliseconds;

		public static readonly SYSTEMTIME MinValue;

		public static readonly SYSTEMTIME MaxValue;

		public SYSTEMTIME(DateTime dt)
		{
			dt = dt.ToLocalTime();
			Year = Convert.ToUInt16(dt.Year);
			Month = Convert.ToUInt16(dt.Month);
			DayOfWeek = Convert.ToUInt16(dt.DayOfWeek);
			Day = Convert.ToUInt16(dt.Day);
			Hour = Convert.ToUInt16(dt.Hour);
			Minute = Convert.ToUInt16(dt.Minute);
			Second = Convert.ToUInt16(dt.Second);
			Milliseconds = Convert.ToUInt16(dt.Millisecond);
		}

		public SYSTEMTIME(ushort year, ushort month, ushort day, ushort hour = 0, ushort minute = 0, ushort second = 0, ushort millisecond = 0)
		{
			Year = year;
			Month = month;
			Day = day;
			Hour = hour;
			Minute = minute;
			Second = second;
			Milliseconds = millisecond;
			DayOfWeek = 0;
		}

		public static implicit operator DateTime(SYSTEMTIME st)
		{
			if (st.Year == 0 || st == MinValue)
			{
				return DateTime.MinValue;
			}
			if (st == MaxValue)
			{
				return DateTime.MaxValue;
			}
			return new DateTime(st.Year, st.Month, st.Day, st.Hour, st.Minute, st.Second, st.Milliseconds, DateTimeKind.Local);
		}

		public static implicit operator SYSTEMTIME(DateTime dt)
		{
			return new SYSTEMTIME(dt);
		}

		public static bool operator ==(SYSTEMTIME s1, SYSTEMTIME s2)
		{
			if (s1.Year == s2.Year && s1.Month == s2.Month && s1.Day == s2.Day && s1.Hour == s2.Hour && s1.Minute == s2.Minute && s1.Second == s2.Second)
			{
				return s1.Milliseconds == s2.Milliseconds;
			}
			return false;
		}

		public static bool operator !=(SYSTEMTIME s1, SYSTEMTIME s2)
		{
			return !(s1 == s2);
		}

		static SYSTEMTIME()
		{
			MinValue = new SYSTEMTIME(1601, 1, 1, 0, 0, 0, 0);
			MaxValue = new SYSTEMTIME(30827, 12, 31, 23, 59, 59, 999);
		}

		public override bool Equals(object obj)
		{
			if (obj is SYSTEMTIME)
			{
				return (SYSTEMTIME)obj == this;
			}
			if (obj is DateTime)
			{
				return ((DateTime)this).Equals(obj);
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return ((DateTime)this).GetHashCode();
		}

		public override string ToString()
		{
			return ((DateTime)this).ToString();
		}

		TypeCode IConvertible.GetTypeCode()
		{
			return ((IConvertible)(DateTime)this).GetTypeCode();
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToBoolean(provider);
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToByte(provider);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToChar(provider);
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return this;
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToDecimal(provider);
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToDouble(provider);
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToInt16(provider);
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToInt32(provider);
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToInt64(provider);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToSByte(provider);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToSingle(provider);
		}

		string IConvertible.ToString(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToString(provider);
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToType(conversionType, provider);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToUInt16(provider);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToUInt32(provider);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return ((IConvertible)(DateTime)this).ToUInt64(provider);
		}
	}

	private const string ADVAPI32 = "advapi32.dll";

	private const string KERNEL32 = "Kernel32.dll";

	private const int MAX_PREFERRED_LENGTH = -1;

	private const string NTDSAPI = "ntdsapi.dll";

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static extern bool AdjustTokenPrivileges([In] SafeTokenHandle TokenHandle, [In] bool DisableAllPrivileges, [In] ref TOKEN_PRIVILEGES NewState, [In] uint BufferLength, [In][Out] ref TOKEN_PRIVILEGES PreviousState, [In][Out] ref uint ReturnLength);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static extern bool AdjustTokenPrivileges([In] SafeTokenHandle TokenHandle, [In] bool DisableAllPrivileges, [In] ref TOKEN_PRIVILEGES NewState, [In] uint BufferLength, [In] IntPtr PreviousState, [In] IntPtr ReturnLength);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool ConvertStringSidToSid([In][MarshalAs(UnmanagedType.LPTStr)] string pStringSid, ref IntPtr sid);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DuplicateToken(SafeTokenHandle ExistingTokenHandle, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, out SafeTokenHandle DuplicateTokenHandle);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool DuplicateTokenEx([In] SafeTokenHandle ExistingTokenHandle, [In] AccessTypes DesiredAccess, [In] IntPtr TokenAttributes, [In] SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, [In] TokenType TokenType, [In][Out] ref SafeTokenHandle DuplicateTokenHandle);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr GetSidSubAuthority(IntPtr pSid, uint nSubAuthority);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetTokenInformation(SafeTokenHandle hToken, TOKEN_INFORMATION_CLASS tokenInfoClass, IntPtr pTokenInfo, int tokenInfoLength, out int returnLength);

	[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern int LogonUser(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool LookupAccountSid(string systemName, byte[] accountSid, StringBuilder accountName, ref int nameLength, StringBuilder domainName, ref int domainLength, out int accountType);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool LookupAccountSid([In][MarshalAs(UnmanagedType.LPTStr)] string systemName, IntPtr sid, StringBuilder name, ref int cchName, StringBuilder referencedDomainName, ref int cchReferencedDomainName, out int use);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool LookupPrivilegeValue(string systemName, string name, out LUID luid);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool OpenProcessToken(IntPtr ProcessHandle, AccessTypes DesiredAccess, out SafeTokenHandle TokenHandle);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool OpenThreadToken(IntPtr ThreadHandle, AccessTypes DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool OpenAsSelf, out SafeTokenHandle TokenHandle);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool PrivilegeCheck(IntPtr ClientToken, ref PRIVILEGE_SET RequiredPrivileges, out int result);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool RevertToSelf();

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetThreadToken(IntPtr ThreadHandle, SafeTokenHandle TokenHandle);

	[DllImport("Kernel32.dll", SetLastError = true)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool CloseHandle(IntPtr handle);

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr GetCurrentProcess();

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr GetCurrentThread();

	[DllImport("Kernel32.dll", SetLastError = true)]
	public static extern IntPtr GlobalLock(IntPtr hMem);

	[DllImport("Kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GlobalUnlock(IntPtr hMem);

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr LoadLibrary(string filename);

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool FreeLibrary(IntPtr lib);

	[DllImport("Netapi32", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int NetServerGetInfo(string serverName, int level, out IntPtr pSERVER_INFO_XXX);

	[DllImport("Netapi32", CharSet = CharSet.Auto, SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	private static extern int NetServerEnum([MarshalAs(UnmanagedType.LPWStr)] string servernane, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, ServerTypes servertype, [MarshalAs(UnmanagedType.LPWStr)] string domain, IntPtr resume_handle);

	[DllImport("Netapi32", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	private static extern int NetApiBufferFree(IntPtr pBuf);

	public static IEnumerable<string> GetNetworkComputerNames(ServerTypes serverTypes = ServerTypes.Workstation | ServerTypes.Server, string domain = null)
	{
		return Array.ConvertAll(NetServerEnum<SERVER_INFO_100>(serverTypes, domain), (SERVER_INFO_100 si) => si.Name);
	}

	public static IEnumerable<NetworkComputerInfo> GetNetworkComputerInfo(ServerTypes serverTypes = ServerTypes.Workstation | ServerTypes.Server, string domain = null)
	{
		return NetServerEnum<NetworkComputerInfo>(serverTypes, domain, 101);
	}

	public static T[] NetServerEnum<T>(ServerTypes serverTypes = ServerTypes.Workstation | ServerTypes.Server, string domain = null, int level = 0) where T : struct
	{
		if (level == 0)
		{
			level = int.Parse(Regex.Replace(typeof(T).Name, "[^\\d]", ""));
		}
		IntPtr bufptr = IntPtr.Zero;
		try
		{
			IntPtr zero = IntPtr.Zero;
			int entriesread;
			int totalentries;
			int num = NetServerEnum(null, level, out bufptr, -1, out entriesread, out totalentries, serverTypes, domain, zero);
			if (num == 0)
			{
				return InteropUtil.ToArray<T>(bufptr, entriesread);
			}
			throw new Win32Exception(num);
		}
		finally
		{
			NetApiBufferFree(bufptr);
		}
	}

	public static T NetServerGetInfo<T>(string serverName, int level = 0) where T : struct
	{
		if (level == 0)
		{
			level = int.Parse(Regex.Replace(typeof(T).Name, "[^\\d]", ""));
		}
		IntPtr pSERVER_INFO_XXX = IntPtr.Zero;
		try
		{
			int num = NetServerGetInfo(serverName, level, out pSERVER_INFO_XXX);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			return (T)Marshal.PtrToStructure(pSERVER_INFO_XXX, typeof(T));
		}
		finally
		{
			if (pSERVER_INFO_XXX != IntPtr.Zero)
			{
				NetApiBufferFree(pSERVER_INFO_XXX);
			}
		}
	}

	[DllImport("ntdsapi.dll", CharSet = CharSet.Auto, PreserveSig = false)]
	public static extern void DsBind(string DomainControllerName, string DnsDomainName, out IntPtr phDS);

	[DllImport("ntdsapi.dll", CharSet = CharSet.Auto)]
	public static extern uint DsCrackNames(IntPtr hDS, DS_NAME_FLAGS flags, DS_NAME_FORMAT formatOffered, DS_NAME_FORMAT formatDesired, uint cNames, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPTStr, SizeParamIndex = 4)] string[] rpNames, out IntPtr ppResult);

	[DllImport("ntdsapi.dll", CharSet = CharSet.Auto)]
	public static extern void DsFreeNameResult(IntPtr pResult);

	[DllImport("ntdsapi.dll", CharSet = CharSet.Auto)]
	public static extern uint DsUnBind(ref IntPtr phDS);
}

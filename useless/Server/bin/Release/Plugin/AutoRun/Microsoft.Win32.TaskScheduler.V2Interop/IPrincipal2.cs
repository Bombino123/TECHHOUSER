using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.TaskScheduler.V2Interop;

[ComImport]
[Guid("248919AE-E345-4A6D-8AEB-E0D3165C904E")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
[SuppressUnmanagedCodeSecurity]
internal interface IPrincipal2
{
	TaskProcessTokenSidType ProcessTokenSidType
	{
		get; [param: In]
		set;
	}

	int RequiredPrivilegeCount { get; }

	string this[int index]
	{
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	void AddRequiredPrivilege([In][MarshalAs(UnmanagedType.BStr)] string privilege);
}

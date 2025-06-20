using System.Collections;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.TaskScheduler.V2Interop;

[ComImport]
[Guid("86627EB4-42A7-41E4-A4D9-AC33A72F2D52")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
[SuppressUnmanagedCodeSecurity]
internal interface IRegisteredTaskCollection
{
	int Count { get; }

	IRegisteredTask this[object index]
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[return: MarshalAs(UnmanagedType.Interface)]
	IEnumerator GetEnumerator();
}

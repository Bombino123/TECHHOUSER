using System.Collections;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.TaskScheduler.V2Interop;

[ComImport]
[Guid("85DF5081-1B24-4F32-878A-D9D14DF4CB77")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
[SuppressUnmanagedCodeSecurity]
internal interface ITriggerCollection
{
	int Count { get; }

	ITrigger this[int index]
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[return: MarshalAs(UnmanagedType.Interface)]
	IEnumerator GetEnumerator();

	[return: MarshalAs(UnmanagedType.Interface)]
	ITrigger Create([In] TaskTriggerType Type);

	void Remove([In][MarshalAs(UnmanagedType.Struct)] object index);

	void Clear();
}

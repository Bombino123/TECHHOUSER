using System.Collections;
using System.Runtime.InteropServices;
using System.Security;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler.V2Interop;

[ComImport]
[Guid("B4EF826B-63C3-46E4-A504-EF69E4F7EA4D")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
[SuppressUnmanagedCodeSecurity]
internal interface ITaskNamedValueCollection
{
	int Count { get; }

	ITaskNamedValuePair this[int index]
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[return: MarshalAs(UnmanagedType.Interface)]
	IEnumerator GetEnumerator();

	[return: MarshalAs(UnmanagedType.Interface)]
	ITaskNamedValuePair Create([In][NotNull][MarshalAs(UnmanagedType.BStr)] string Name, [In][MarshalAs(UnmanagedType.BStr)] string Value);

	void Remove([In] int index);

	void Clear();
}

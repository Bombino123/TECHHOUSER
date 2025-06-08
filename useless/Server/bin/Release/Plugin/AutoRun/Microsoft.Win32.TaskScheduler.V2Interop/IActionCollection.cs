using System.Collections;
using System.Runtime.InteropServices;
using System.Security;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler.V2Interop;

[ComImport]
[Guid("02820E19-7B98-4ED2-B2E8-FDCCCEFF619B")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
[SuppressUnmanagedCodeSecurity]
internal interface IActionCollection
{
	int Count { get; }

	IAction this[int index]
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[return: MarshalAs(UnmanagedType.Interface)]
	IEnumerator GetEnumerator();

	string XmlText
	{
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}

	[return: MarshalAs(UnmanagedType.Interface)]
	IAction Create([In] TaskActionType Type);

	void Remove([In][NotNull][MarshalAs(UnmanagedType.Struct)] object index);

	void Clear();

	string Context
	{
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}
}

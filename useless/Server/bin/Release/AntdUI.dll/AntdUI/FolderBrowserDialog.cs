using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AntdUI;

[Description("提供一个Vista样式的选择文件对话框")]
public class FolderBrowserDialog
{
	[ComImport]
	[Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
	private class FileOpenDialog
	{
	}

	[ComImport]
	[Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IFileOpenDialog
	{
		[PreserveSig]
		uint Show([In] IntPtr parent);

		void SetFileTypes();

		void SetFileTypeIndex([In] uint iFileType);

		void GetFileTypeIndex(out uint piFileType);

		void Advise();

		void Unadvise();

		void SetOptions([In] FOS fos);

		void GetOptions(out FOS pfos);

		void SetDefaultFolder(IShellItem psi);

		void SetFolder(IShellItem psi);

		void GetFolder(out IShellItem ppsi);

		void GetCurrentSelection(out IShellItem ppsi);

		void SetFileName([In][MarshalAs(UnmanagedType.LPWStr)] string pszName);

		void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

		void SetTitle([In][MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

		void SetOkButtonLabel([In][MarshalAs(UnmanagedType.LPWStr)] string pszText);

		void SetFileNameLabel([In][MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

		void GetResult(out IShellItem ppsi);

		void AddPlace(IShellItem psi, int alignment);

		void SetDefaultExtension([In][MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

		void Close(int hr);

		void SetClientGuid();

		void ClearClientData();

		void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);

		void GetResults([MarshalAs(UnmanagedType.Interface)] out IntPtr ppenum);

		void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out IntPtr ppsai);
	}

	[ComImport]
	[Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IShellItem
	{
		void BindToHandler();

		void GetParent();

		void GetDisplayName([In] SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

		void GetAttributes();

		void Compare();
	}

	private enum SIGDN : uint
	{
		SIGDN_DESKTOPABSOLUTEEDITING = 2147794944u,
		SIGDN_DESKTOPABSOLUTEPARSING = 2147647488u,
		SIGDN_FILESYSPATH = 2147844096u,
		SIGDN_NORMALDISPLAY = 0u,
		SIGDN_PARENTRELATIVE = 2148007937u,
		SIGDN_PARENTRELATIVEEDITING = 2147684353u,
		SIGDN_PARENTRELATIVEFORADDRESSBAR = 2147991553u,
		SIGDN_PARENTRELATIVEPARSING = 2147581953u,
		SIGDN_URL = 2147909632u
	}

	[Flags]
	private enum FOS
	{
		FOS_ALLNONSTORAGEITEMS = 0x80,
		FOS_ALLOWMULTISELECT = 0x200,
		FOS_CREATEPROMPT = 0x2000,
		FOS_DEFAULTNOMINIMODE = 0x20000000,
		FOS_DONTADDTORECENT = 0x2000000,
		FOS_FILEMUSTEXIST = 0x1000,
		FOS_FORCEFILESYSTEM = 0x40,
		FOS_FORCESHOWHIDDEN = 0x10000000,
		FOS_HIDEMRUPLACES = 0x20000,
		FOS_HIDEPINNEDPLACES = 0x40000,
		FOS_NOCHANGEDIR = 8,
		FOS_NODEREFERENCELINKS = 0x100000,
		FOS_NOREADONLYRETURN = 0x8000,
		FOS_NOTESTFILECREATE = 0x10000,
		FOS_NOVALIDATE = 0x100,
		FOS_OVERWRITEPROMPT = 2,
		FOS_PATHMUSTEXIST = 0x800,
		FOS_PICKFOLDERS = 0x20,
		FOS_SHAREAWARE = 0x4000,
		FOS_STRICTFILETYPES = 4
	}

	private const uint ERROR_CANCELLED = 2147943623u;

	public string? DirectoryPath { get; set; }

	public DialogResult ShowDialog(IWin32Window? owner = null)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		IntPtr parent = ((owner == null) ? GetActiveWindow() : owner.Handle);
		IFileOpenDialog fileOpenDialog = (IFileOpenDialog)new FileOpenDialog();
		try
		{
			IShellItem ppsi;
			if (DirectoryPath != null)
			{
				uint rgflnOut = 0u;
				if (SHILCreateFromPath(DirectoryPath, out var ppIdl, ref rgflnOut) == 0 && SHCreateShellItem(IntPtr.Zero, IntPtr.Zero, ppIdl, out ppsi) == 0)
				{
					fileOpenDialog.SetFolder(ppsi);
				}
			}
			fileOpenDialog.SetOptions(FOS.FOS_FORCEFILESYSTEM | FOS.FOS_PICKFOLDERS);
			switch (fileOpenDialog.Show(parent))
			{
			case 2147943623u:
				return (DialogResult)2;
			default:
				return (DialogResult)3;
			case 0u:
			{
				fileOpenDialog.GetResult(out ppsi);
				ppsi.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out string ppszName);
				DirectoryPath = ppszName;
				return (DialogResult)1;
			}
			}
		}
		finally
		{
			Marshal.ReleaseComObject(fileOpenDialog);
		}
	}

	[DllImport("shell32.dll")]
	private static extern int SHILCreateFromPath([MarshalAs(UnmanagedType.LPWStr)] string pszPath, out IntPtr ppIdl, ref uint rgflnOut);

	[DllImport("shell32.dll")]
	private static extern int SHCreateShellItem(IntPtr pidlParent, IntPtr psfParent, IntPtr pidl, out IShellItem ppsi);

	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();
}

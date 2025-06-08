using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AntdUI;

public sealed class FileDropHandler : IMessageFilter, IDisposable
{
	private struct ChangeFilterStruct
	{
		public uint CbSize;

		public ChangeFilterStatu ExtStatus;
	}

	private enum ChangeFilterAction : uint
	{
		MSGFLT_RESET,
		MSGFLT_ALLOW,
		MSGFLT_DISALLOW
	}

	private enum ChangeFilterStatu : uint
	{
		MSGFLTINFO_NONE,
		MSGFLTINFO_ALREADYALLOWED_FORWND,
		MSGFLTINFO_ALREADYDISALLOWED_FORWND,
		MSGFLTINFO_ALLOWED_HIGHER
	}

	private const uint WM_COPYGLOBALDATA = 73u;

	private const uint WM_COPYDATA = 74u;

	private const uint WM_DROPFILES = 563u;

	private const uint GetIndexCount = uint.MaxValue;

	private Control ContainerControl;

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint message, ChangeFilterAction action, in ChangeFilterStruct pChangeFilterStruct);

	[DllImport("shell32.dll")]
	private static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern uint DragQueryFile(IntPtr hWnd, uint iFile, StringBuilder lpszFile, int cch);

	[DllImport("shell32.dll")]
	private static extern void DragFinish(IntPtr hDrop);

	public FileDropHandler(Control containerControl)
	{
		ContainerControl = containerControl ?? throw new ArgumentNullException("control", "control is null.");
		if (containerControl.IsDisposed)
		{
			throw new ObjectDisposedException("control");
		}
		ChangeFilterStruct pChangeFilterStruct = new ChangeFilterStruct
		{
			CbSize = 8u
		};
		if (!ChangeWindowMessageFilterEx(containerControl.Handle, 563u, ChangeFilterAction.MSGFLT_ALLOW, in pChangeFilterStruct))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		if (!ChangeWindowMessageFilterEx(containerControl.Handle, 73u, ChangeFilterAction.MSGFLT_ALLOW, in pChangeFilterStruct))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		if (!ChangeWindowMessageFilterEx(containerControl.Handle, 74u, ChangeFilterAction.MSGFLT_ALLOW, in pChangeFilterStruct))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		DragAcceptFiles(containerControl.Handle, fAccept: true);
		Application.AddMessageFilter((IMessageFilter)(object)this);
	}

	public bool PreFilterMessage(ref Message m)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (ContainerControl == null || ContainerControl.IsDisposed)
		{
			return false;
		}
		if (ContainerControl.AllowDrop)
		{
			return ContainerControl.AllowDrop = false;
		}
		if ((long)((Message)(ref m)).Msg == 563)
		{
			IntPtr wParam = ((Message)(ref m)).WParam;
			uint num = DragQueryFile(wParam, uint.MaxValue, null, 0);
			string[] array = new string[num];
			StringBuilder stringBuilder = new StringBuilder(262);
			int capacity = stringBuilder.Capacity;
			for (uint num2 = 0u; num2 < num; num2++)
			{
				if (DragQueryFile(wParam, num2, stringBuilder, capacity) != 0)
				{
					array[num2] = stringBuilder.ToString();
				}
			}
			DragFinish(wParam);
			ContainerControl.AllowDrop = true;
			ContainerControl.DoDragDrop((object)array, (DragDropEffects)(-2147483645));
			ContainerControl.AllowDrop = false;
			return true;
		}
		return false;
	}

	public void Dispose()
	{
		Application.RemoveMessageFilter((IMessageFilter)(object)this);
	}
}

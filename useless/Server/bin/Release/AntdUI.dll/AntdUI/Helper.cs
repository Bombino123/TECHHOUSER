using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using AntdUI.Core;
using Vanara.PInvoke;

namespace AntdUI;

public static class Helper
{
	private const ContentAlignment AnyRight = 1092;

	private const ContentAlignment AnyBottom = 1792;

	private const ContentAlignment AnyCenter = 546;

	private const ContentAlignment AnyMiddle = 112;

	public static float Calculate(this float val, float add)
	{
		return (float)Math.Round(val + add, 3);
	}

	public static Size Size(this SizeF size)
	{
		return new Size((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
	}

	public static Size Size(this SizeF size, float p)
	{
		return new Size((int)Math.Ceiling(size.Width + p), (int)Math.Ceiling(size.Height + p));
	}

	public static Color ToColor(float alpha, Color color)
	{
		return ToColor((int)alpha, color);
	}

	public static Color ToColorN(float val, Color color)
	{
		return ToColor((int)(val * (float)(int)color.A), color);
	}

	public static Color ToColor(int alpha, Color color)
	{
		if (alpha > 255)
		{
			alpha = 255;
		}
		else if (alpha < 0)
		{
			alpha = 0;
		}
		return Color.FromArgb(alpha, color);
	}

	public static Form? FindPARENT(this Control? control)
	{
		if (control == null)
		{
			return null;
		}
		if (control is DoubleBufferForm result)
		{
			object tag = control.Tag;
			Form val = (Form)((tag is Form) ? tag : null);
			if (val != null)
			{
				return val;
			}
			if (control.Parent != null)
			{
				return control.Parent.FindPARENT();
			}
			return (Form?)(object)result;
		}
		Form val2 = (Form)(object)((control is Form) ? control : null);
		if (val2 != null)
		{
			return val2;
		}
		if (control.Parent != null)
		{
			return control.Parent.FindPARENT();
		}
		return null;
	}

	public static bool SetTopMost(this Control? control, IntPtr hand)
	{
		Form val = control.FindPARENT();
		if ((val != null && val.TopMost) || val is LayeredFormPopover { topMost: not false })
		{
			SetTopMost(hand);
			return true;
		}
		return false;
	}

	public static void SetTopMost(IntPtr hand)
	{
		User32.SetWindowPos(hand, new IntPtr(-1), 0, 0, 0, 0, User32.SetWindowPosFlags.SWP_NOACTIVATE);
	}

	public static bool Wait(this WaitHandle? handle, bool close = true)
	{
		if (handle == null)
		{
			return true;
		}
		try
		{
			handle.WaitOne();
			if (handle.SafeWaitHandle.IsClosed)
			{
				return close;
			}
			return false;
		}
		catch
		{
			return true;
		}
	}

	public static bool SetWait(this EventWaitHandle? handle)
	{
		if (handle == null)
		{
			return true;
		}
		try
		{
			if (handle.SafeWaitHandle.IsClosed)
			{
				return true;
			}
			handle.Set();
			return false;
		}
		catch
		{
			return true;
		}
	}

	public static bool ResetWait(this EventWaitHandle? handle)
	{
		if (handle == null)
		{
			return true;
		}
		try
		{
			if (handle.SafeWaitHandle.IsClosed)
			{
				return true;
			}
			handle.Reset();
			return false;
		}
		catch
		{
			return true;
		}
	}

	public static void WaitDispose(this EventWaitHandle? handle, bool set = true)
	{
		if (handle == null)
		{
			return;
		}
		try
		{
			if (!handle.SafeWaitHandle.IsClosed)
			{
				if (set)
				{
					handle.SetWait();
				}
				else
				{
					handle.ResetWait();
				}
				handle.Dispose();
			}
		}
		catch
		{
		}
	}

	public static bool Wait(this CancellationTokenSource? token)
	{
		try
		{
			if (token == null || token.IsCancellationRequested)
			{
				return true;
			}
			return false;
		}
		catch
		{
			return true;
		}
	}

	public static bool Wait(this CancellationTokenSource? token, Control control)
	{
		try
		{
			if (token == null || token.IsCancellationRequested || control.IsDisposed)
			{
				return true;
			}
			return false;
		}
		catch
		{
			return true;
		}
	}

	public static bool ListExceed(this IList? list, int index)
	{
		if (list == null || list.Count <= index || index < 0)
		{
			return true;
		}
		return false;
	}

	public static bool DateExceed(DateTime date, DateTime? min, DateTime? max)
	{
		if (min.HasValue && min.Value >= date)
		{
			return false;
		}
		if (max.HasValue && max.Value <= date)
		{
			return false;
		}
		return true;
	}

	public static bool DateExceedMonth(DateTime date, DateTime? min, DateTime? max)
	{
		if (min.HasValue && min.Value >= date)
		{
			return false;
		}
		if (max.HasValue)
		{
			if (max.Value.Year == date.Year && max.Value.Month == date.Month)
			{
				return true;
			}
			if (max.Value <= date)
			{
				return false;
			}
		}
		return true;
	}

	public static bool DateExceedYear(DateTime date, DateTime? min, DateTime? max)
	{
		if (min.HasValue && min.Value >= date)
		{
			return false;
		}
		if (max.HasValue)
		{
			if (max.Value.Year == date.Year)
			{
				return true;
			}
			if (max.Value <= date)
			{
				return false;
			}
		}
		return true;
	}

	public static bool DateExceedRelax(DateTime date, DateTime? min, DateTime? max)
	{
		if (min.HasValue && min.Value > date)
		{
			return false;
		}
		if (max.HasValue && max.Value < date)
		{
			return false;
		}
		return true;
	}

	public static string? ClipboardGetText(this Control control)
	{
		if (control.InvokeRequired)
		{
			string r = null;
			control.Invoke((Delegate)(Action)delegate
			{
				r = ClipboardGetText();
			});
			return r;
		}
		return ClipboardGetText();
	}

	public static string? ClipboardGetText()
	{
		try
		{
			return Win32.GetClipBoardText();
		}
		catch
		{
			return Clipboard.GetText();
		}
	}

	public static bool ClipboardSetText(this Control control, string? text)
	{
		string text2 = text;
		if (control.InvokeRequired)
		{
			bool r = false;
			control.Invoke((Delegate)(Action)delegate
			{
				r = ClipboardSetText(text2);
			});
			return r;
		}
		return ClipboardSetText(text2);
	}

	public static bool ClipboardSetText(string? text)
	{
		try
		{
			if (Win32.SetClipBoardText(text))
			{
				return true;
			}
		}
		catch
		{
			if (text == null)
			{
				Clipboard.Clear();
			}
			else
			{
				Clipboard.SetText(text);
			}
			return true;
		}
		return false;
	}

	public static bool IsAdmin()
	{
		using WindowsIdentity ntIdentity = WindowsIdentity.GetCurrent();
		return new WindowsPrincipal(ntIdentity).IsInRole(WindowsBuiltInRole.Administrator);
	}

	internal static void DpiAuto(float dpi, Control control)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if (dpi == 1f)
		{
			if (control is Window window && (int)((Form)window).StartPosition == 1)
			{
				Size size = window.sizeInit ?? ((Form)window).ClientSize;
				Rectangle workingArea = Screen.FromPoint(window.Location).WorkingArea;
				window.Location = new Point(workingArea.X + (workingArea.Width - size.Width) / 2, workingArea.Y + (workingArea.Height - size.Height) / 2);
			}
			return;
		}
		Form val = (Form)(object)((control is Form) ? control : null);
		if (val != null)
		{
			if ((int)val.WindowState == 2)
			{
				((Control)val).Scale(new SizeF(dpi, dpi));
				return;
			}
			Dictionary<Control, AnchorDock> dir = DpiSuspend(control.Controls);
			DpiLS(dpi, val);
			DpiResume(dir, control.Controls);
		}
		else
		{
			Dictionary<Control, AnchorDock> dir2 = DpiSuspend(control.Controls);
			DpiLS(dpi, control);
			DpiResume(dir2, control.Controls);
		}
	}

	private static Dictionary<Control, AnchorDock> DpiSuspend(ControlCollection controls)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		Dictionary<Control, AnchorDock> dir = new Dictionary<Control, AnchorDock>(((ArrangedElementCollection)controls).Count);
		foreach (Control item in (ArrangedElementCollection)controls)
		{
			Control val = item;
			if (!(val is Splitter))
			{
				if ((int)val.Dock != 0 || (int)val.Anchor != 5)
				{
					dir.Add(val, new AnchorDock(val));
				}
				if (((ArrangedElementCollection)controls).Count > 0)
				{
					DpiSuspend(ref dir, val.Controls);
				}
			}
		}
		return dir;
	}

	private static void DpiSuspend(ref Dictionary<Control, AnchorDock> dir, ControlCollection controls)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		foreach (Control item in (ArrangedElementCollection)controls)
		{
			Control val = item;
			if (!(val is Splitter))
			{
				if ((int)val.Dock != 0 || (int)val.Anchor != 5)
				{
					dir.Add(val, new AnchorDock(val));
				}
				if (((ArrangedElementCollection)controls).Count > 0)
				{
					DpiSuspend(ref dir, val.Controls);
				}
			}
		}
	}

	private static void DpiResume(Dictionary<Control, AnchorDock> dir, ControlCollection controls)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Control item in (ArrangedElementCollection)controls)
		{
			Control val = item;
			if (dir.TryGetValue(val, out AnchorDock value))
			{
				val.Dock = value.Dock;
				val.Anchor = value.Anchor;
			}
			if (((ArrangedElementCollection)controls).Count > 0)
			{
				DpiResume(dir, val.Controls);
			}
		}
	}

	private static void DpiLS(float dpi, Control control)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Invalid comparison between Unknown and I4
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Invalid comparison between Unknown and I4
		Size size = new Size((int)((float)control.Width * dpi), (int)((float)control.Height * dpi));
		Point location = new Point((int)((float)control.Left * dpi), (int)((float)control.Top * dpi));
		if (!control.MinimumSize.IsEmpty)
		{
			control.MinimumSize = new Size((int)((float)control.MinimumSize.Width * dpi), (int)((float)control.MinimumSize.Height * dpi));
		}
		if (!control.MaximumSize.IsEmpty)
		{
			control.MaximumSize = new Size((int)((float)control.MaximumSize.Width * dpi), (int)((float)control.MaximumSize.Height * dpi));
		}
		control.Padding = SetPadding(dpi, control.Padding);
		control.Margin = SetPadding(dpi, control.Margin);
		control.Size = size;
		control.Location = location;
		TableLayoutPanel val = (TableLayoutPanel)(object)((control is TableLayoutPanel) ? control : null);
		if (val != null)
		{
			foreach (ColumnStyle item in (IEnumerable)val.ColumnStyles)
			{
				ColumnStyle val2 = item;
				if ((int)((TableLayoutStyle)val2).SizeType == 1)
				{
					val2.Width *= dpi;
				}
			}
			foreach (RowStyle item2 in (IEnumerable)val.RowStyles)
			{
				RowStyle val3 = item2;
				if ((int)((TableLayoutStyle)val3).SizeType == 1)
				{
					val3.Height *= dpi;
				}
			}
		}
		else
		{
			TabControl val4 = (TabControl)(object)((control is TabControl) ? control : null);
			if (val4 != null && val4.ItemSize.Width > 1 && val4.ItemSize.Height > 1)
			{
				val4.ItemSize = new Size((int)((float)val4.ItemSize.Width * dpi), (int)((float)val4.ItemSize.Height * dpi));
			}
			else
			{
				SplitContainer val5 = (SplitContainer)(object)((control is SplitContainer) ? control : null);
				if (val5 != null)
				{
					val5.SplitterWidth = (int)((float)val5.SplitterWidth * dpi);
				}
				else if (control is Panel panel)
				{
					panel.padding = SetPadding(dpi, panel.padding);
				}
			}
		}
		DpiLSS(dpi, control);
	}

	private static void DpiLS(float dpi, Form form)
	{
		if (form is Window window)
		{
			DpiLS(dpi, (Form)(object)window, window.sizeInit ?? ((Form)window).ClientSize, out var point, out var size);
			Size maximumSize = ((Control)window).MaximumSize;
			Size minimumSize = ((Control)window).MinimumSize;
			Size size3 = (((Form)window).ClientSize = size);
			Size maximumSize2 = (((Control)window).MinimumSize = size3);
			((Control)window).MaximumSize = maximumSize2;
			window.Location = point;
			((Control)window).MinimumSize = minimumSize;
			((Control)window).MaximumSize = maximumSize;
		}
		else
		{
			DpiLS(dpi, form, form.ClientSize, out var point2, out var size5);
			form.ClientSize = size5;
			form.Location = point2;
		}
	}

	private static void DpiLS(float dpi, Form form, Size csize, out Point point, out Size size)
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Invalid comparison between Unknown and I4
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		size = new Size((int)((float)csize.Width * dpi), (int)((float)csize.Height * dpi));
		Rectangle workingArea = Screen.FromPoint(form.Location).WorkingArea;
		if (size.Width > workingArea.Width && size.Height > workingArea.Height)
		{
			if (csize.Width > workingArea.Width && csize.Height > workingArea.Height)
			{
				size = workingArea.Size;
				point = workingArea.Location;
			}
			else
			{
				size = csize;
				point = form.Location;
			}
		}
		else
		{
			if (size.Width > workingArea.Width)
			{
				size.Width = workingArea.Width;
			}
			if (size.Height > workingArea.Height)
			{
				size.Height = workingArea.Height;
			}
			point = new Point(((Control)form).Left + (csize.Width - size.Width) / 2, ((Control)form).Top + (csize.Height - size.Height) / 2);
			if (point.X < 0 || point.Y < 0)
			{
				point = form.Location;
			}
		}
		if ((int)form.StartPosition == 1)
		{
			point = new Point(workingArea.X + (workingArea.Width - size.Width) / 2, workingArea.Y + (workingArea.Height - size.Height) / 2);
		}
		if (!((Control)form).MinimumSize.IsEmpty)
		{
			((Control)form).MinimumSize = new Size((int)((float)((Control)form).MinimumSize.Width * dpi), (int)((float)((Control)form).MinimumSize.Height * dpi));
		}
		if (!((Control)form).MaximumSize.IsEmpty)
		{
			((Control)form).MaximumSize = new Size((int)((float)((Control)form).MaximumSize.Width * dpi), (int)((float)((Control)form).MaximumSize.Height * dpi));
		}
		((Control)form).Padding = SetPadding(dpi, ((Control)form).Padding);
		form.Margin = SetPadding(dpi, form.Margin);
		DpiLSS(dpi, (Control)(object)form);
	}

	private static void DpiLSS(float dpi, Control control)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		if (((ArrangedElementCollection)control.Controls).Count <= 0 || control is Pagination || control is Input)
		{
			return;
		}
		foreach (Control item in (ArrangedElementCollection)control.Controls)
		{
			Control control2 = item;
			DpiLS(dpi, control2);
		}
	}

	internal static Padding SetPadding(float dpi, Padding padding)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (((Padding)(ref padding)).All == 0)
		{
			return padding;
		}
		if (((Padding)(ref padding)).All > 0)
		{
			return new Padding((int)((float)((Padding)(ref padding)).All * dpi));
		}
		return new Padding((int)((float)((Padding)(ref padding)).Left * dpi), (int)((float)((Padding)(ref padding)).Top * dpi), (int)((float)((Padding)(ref padding)).Right * dpi), (int)((float)((Padding)(ref padding)).Bottom * dpi));
	}

	public static StringFormat SF(StringAlignment tb = 1, StringAlignment lr = 1)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		return new StringFormat(StringFormat.GenericTypographic)
		{
			LineAlignment = tb,
			Alignment = lr
		};
	}

	public static StringFormat SF_NoWrap(StringAlignment tb = 1, StringAlignment lr = 1)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		StringFormat val = new StringFormat(StringFormat.GenericTypographic)
		{
			LineAlignment = tb,
			Alignment = lr
		};
		val.FormatFlags = (StringFormatFlags)(val.FormatFlags | 0x1000);
		return val;
	}

	public static StringFormat SF_Ellipsis(StringAlignment tb = 1, StringAlignment lr = 1)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		return new StringFormat(StringFormat.GenericTypographic)
		{
			LineAlignment = tb,
			Alignment = lr,
			Trimming = (StringTrimming)3
		};
	}

	public static StringFormat SF_ALL(StringAlignment tb = 1, StringAlignment lr = 1)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		StringFormat val = new StringFormat(StringFormat.GenericTypographic)
		{
			LineAlignment = tb,
			Alignment = lr,
			Trimming = (StringTrimming)3
		};
		val.FormatFlags = (StringFormatFlags)(val.FormatFlags | 0x1000);
		return val;
	}

	public static StringFormat SF_MEASURE_FONT()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		StringFormat val = new StringFormat(StringFormat.GenericTypographic)
		{
			Alignment = (StringAlignment)1,
			LineAlignment = (StringAlignment)1
		};
		val.FormatFlags = (StringFormatFlags)(val.FormatFlags | 0x800);
		return val;
	}

	public static TextFormatFlags TF(StringAlignment tb = 1, StringAlignment lr = 1)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected I4, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected I4, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		TextFormatFlags val = (TextFormatFlags)8208;
		val = (TextFormatFlags)((int)tb switch
		{
			1 => val | 4, 
			0 => val | 0, 
			_ => val | 8, 
		});
		return (TextFormatFlags)((int)lr switch
		{
			1 => val | 1, 
			0 => val | 0, 
			_ => val | 2, 
		});
	}

	public static TextFormatFlags TF(StringFormat sf)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected I4, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected I4, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		TextFormatFlags val = (TextFormatFlags)8208;
		StringAlignment lineAlignment = sf.LineAlignment;
		val = (TextFormatFlags)((int)lineAlignment switch
		{
			1 => val | 4, 
			0 => val | 0, 
			_ => val | 8, 
		});
		lineAlignment = sf.Alignment;
		val = (TextFormatFlags)((int)lineAlignment switch
		{
			1 => val | 1, 
			0 => val | 0, 
			_ => val | 2, 
		});
		if (((Enum)sf.Trimming).HasFlag((Enum)(object)(StringTrimming)3))
		{
			val = (TextFormatFlags)(val | 0x8000);
		}
		if (((Enum)sf.FormatFlags).HasFlag((Enum)(object)(StringFormatFlags)4096))
		{
			val = (TextFormatFlags)(val | 0x20);
		}
		return val;
	}

	public static TextFormatFlags TF_NoWrap(StringAlignment tb = 1, StringAlignment lr = 1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return (TextFormatFlags)(TF(tb, lr) | 0x20);
	}

	public static TextFormatFlags TF_Ellipsis(StringAlignment tb = 1, StringAlignment lr = 1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return (TextFormatFlags)(TF(tb, lr) | 0x8000);
	}

	public static TextFormatFlags TF_ALL(StringAlignment tb = 1, StringAlignment lr = 1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return (TextFormatFlags)(TF(tb, lr) | 0x20 | 0x8000);
	}

	public static TextFormatFlags CreateTextFormatFlags(this ContentAlignment alignment, bool showEllipsis, bool multiLine)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		TextFormatFlags val = alignment.ConvertAlignmentToTextFormat();
		val = (TextFormatFlags)((!multiLine) ? (val | 0x2030) : (val | 0x2010));
		if (showEllipsis)
		{
			val = (TextFormatFlags)(val | 0x8000);
		}
		return val;
	}

	public static TextFormatFlags ConvertAlignmentToTextFormat(this ContentAlignment alignment)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		TextFormatFlags val = (TextFormatFlags)0;
		if ((alignment & 0x700) != 0)
		{
			val = (TextFormatFlags)(val | 8);
		}
		else if ((alignment & 0x70) != 0)
		{
			val = (TextFormatFlags)(val | 4);
		}
		if ((alignment & 0x444) != 0)
		{
			val = (TextFormatFlags)(val | 2);
		}
		else if ((alignment & 0x222) != 0)
		{
			val = (TextFormatFlags)(val | 1);
		}
		return val;
	}

	public static Brush BrushEx(this string? code, Rectangle rect, Color def)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		if (code != null)
		{
			string[] array = code.Split(new char[1] { ',' });
			if (array.Length > 1)
			{
				if (array.Length > 2 && float.TryParse(array[0], out var result))
				{
					return (Brush)new LinearGradientBrush(rect, array[1].Trim().ToColor(), array[2].Trim().ToColor(), 270f + result);
				}
				return (Brush)new LinearGradientBrush(rect, array[0].Trim().ToColor(), array[1].Trim().ToColor(), 270f);
			}
		}
		return (Brush)new SolidBrush(def);
	}

	public static bool BrushEx(this string? code, Rectangle rect, Canvas g)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		if (code != null)
		{
			string[] array = code.Split(new char[1] { ',' });
			if (array.Length > 1)
			{
				if (array.Length > 2 && float.TryParse(array[0], out var result))
				{
					LinearGradientBrush val = new LinearGradientBrush(rect, array[1].Trim().ToColor(), array[2].Trim().ToColor(), 270f + result);
					try
					{
						g.Fill((Brush)(object)val, rect);
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
					return true;
				}
				LinearGradientBrush val2 = new LinearGradientBrush(rect, array[0].Trim().ToColor(), array[1].Trim().ToColor(), 270f);
				try
				{
					g.Fill((Brush)(object)val2, rect);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				return true;
			}
		}
		return false;
	}

	public static Canvas High(this Graphics g)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		g.SmoothingMode = (SmoothingMode)4;
		g.InterpolationMode = (InterpolationMode)7;
		g.PixelOffsetMode = (PixelOffsetMode)2;
		if (Config.TextRenderingHint.HasValue)
		{
			g.TextRenderingHint = Config.TextRenderingHint.Value;
		}
		return new CanvasGDI(g);
	}

	public static Canvas HighLay(this Graphics g, bool text = false)
	{
		Config.SetDpi(g);
		g.SmoothingMode = (SmoothingMode)4;
		g.InterpolationMode = (InterpolationMode)7;
		g.PixelOffsetMode = (PixelOffsetMode)2;
		if (text)
		{
			g.TextRenderingHint = (TextRenderingHint)4;
		}
		return new CanvasGDI(g);
	}

	public static void GDI(Action<Canvas> action)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Bitmap val = new Bitmap(1, 1);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				action(val2.HighLay());
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static T GDI<T>(Func<Canvas, T> action)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Bitmap val = new Bitmap(1, 1);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				return action(val2.HighLay());
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static SolidBrush Brush(this Color? color, Color default_color)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (!color.HasValue)
		{
			return new SolidBrush(default_color);
		}
		return new SolidBrush(color.Value);
	}

	public static SolidBrush Brush(this Color? color, Color default_color, Color enabled_color, bool enabled)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (enabled)
		{
			if (!color.HasValue)
			{
				return new SolidBrush(default_color);
			}
			return new SolidBrush(color.Value);
		}
		return new SolidBrush(enabled_color);
	}

	public static GraphicsPath RoundPath(this Rectangle rect, float radius)
	{
		return RoundPathCore(rect, radius);
	}

	public static GraphicsPath RoundPath(this RectangleF rect, float radius)
	{
		return RoundPathCore(rect, radius);
	}

	internal static GraphicsPath RoundPath(this RectangleF rect, float radius, TShape shape)
	{
		return rect.RoundPath(radius, shape == TShape.Round);
	}

	internal static GraphicsPath RoundPath(this Rectangle rect, float radius, bool round)
	{
		if (round)
		{
			return CapsulePathCore(rect);
		}
		return RoundPathCore(rect, radius);
	}

	internal static GraphicsPath RoundPath(this RectangleF rect, float radius, bool round)
	{
		if (round)
		{
			return CapsulePathCore(rect);
		}
		return RoundPathCore(rect, radius);
	}

	internal static GraphicsPath RoundPath(this Rectangle rect, float radius, TAlignMini shadowAlign)
	{
		return (GraphicsPath)(shadowAlign switch
		{
			TAlignMini.Top => rect.RoundPath(radius, TL: true, TR: true, BR: false, BL: false), 
			TAlignMini.Bottom => rect.RoundPath(radius, TL: false, TR: false, BR: true, BL: true), 
			TAlignMini.Left => rect.RoundPath(radius, TL: true, TR: false, BR: false, BL: true), 
			TAlignMini.Right => rect.RoundPath(radius, TL: false, TR: true, BR: true, BL: false), 
			_ => RoundPathCore(rect, radius), 
		});
	}

	public static GraphicsPath RoundPath(this Rectangle rect, float radius, bool TL, bool TR, bool BR, bool BL)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		if (radius <= 0f)
		{
			val.AddRectangle(rect);
		}
		else
		{
			float num = radius * 2f;
			RectangleF rectangleF = new RectangleF(rect.X, rect.Y, num, num);
			if (TL)
			{
				val.AddArc(rectangleF, 180f, 90f);
			}
			else
			{
				val.AddLine((float)rect.X, (float)rect.Y, (float)rect.Right - num, (float)rect.Y);
			}
			rectangleF.X = (float)rect.Right - num;
			if (TR)
			{
				val.AddArc(rectangleF, 270f, 90f);
			}
			else
			{
				val.AddLine((float)rect.Right, (float)rect.Y, (float)rect.Right, (float)rect.Bottom - num);
			}
			rectangleF.Y = (float)rect.Bottom - num;
			if (BR)
			{
				val.AddArc(rectangleF, 0f, 90f);
			}
			else
			{
				val.AddLine((float)rect.Right, (float)rect.Bottom, (float)rect.X + num, (float)rect.Bottom);
			}
			rectangleF.X = rect.Left;
			if (BL)
			{
				val.AddArc(rectangleF, 90f, 90f);
			}
			else
			{
				val.AddLine((float)rect.X, (float)rect.Bottom, (float)rect.X, (float)rect.Y + num);
			}
			val.CloseFigure();
		}
		return val;
	}

	public static GraphicsPath RoundPath(this RectangleF rect, float radius, bool TL, bool TR, bool BR, bool BL)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		if (radius <= 0f)
		{
			val.AddRectangle(rect);
		}
		else
		{
			float num = radius * 2f;
			RectangleF rectangleF = new RectangleF(rect.X, rect.Y, num, num);
			if (TL)
			{
				val.AddArc(rectangleF, 180f, 90f);
			}
			else
			{
				val.AddLine(rect.X, rect.Y, rect.Right - num, rect.Y);
			}
			rectangleF.X = rect.Right - num;
			if (TR)
			{
				val.AddArc(rectangleF, 270f, 90f);
			}
			else
			{
				val.AddLine(rect.Right, rect.Y, rect.Right, rect.Bottom - num);
			}
			rectangleF.Y = rect.Bottom - num;
			if (BR)
			{
				val.AddArc(rectangleF, 0f, 90f);
			}
			else
			{
				val.AddLine(rect.Right, rect.Bottom, rect.X + num, rect.Bottom);
			}
			rectangleF.X = rect.Left;
			if (BL)
			{
				val.AddArc(rectangleF, 90f, 90f);
			}
			else
			{
				val.AddLine(rect.X, rect.Bottom, rect.X, rect.Y + num);
			}
			val.CloseFigure();
		}
		return val;
	}

	private static GraphicsPath RoundPathCore(RectangleF rect, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		if (radius > 0f)
		{
			if (radius >= Math.Min(rect.Width, rect.Height) / 2f)
			{
				AddCapsule(val, rect);
			}
			else
			{
				float num = radius * 2f;
				RectangleF rectangleF = new RectangleF(rect.X, rect.Y, num, num);
				val.AddArc(rectangleF, 180f, 90f);
				rectangleF.X = rect.Right - num;
				val.AddArc(rectangleF, 270f, 90f);
				rectangleF.Y = rect.Bottom - num;
				val.AddArc(rectangleF, 0f, 90f);
				rectangleF.X = rect.Left;
				val.AddArc(rectangleF, 90f, 90f);
				val.CloseFigure();
			}
		}
		else
		{
			val.AddRectangle(rect);
		}
		return val;
	}

	private static GraphicsPath CapsulePathCore(RectangleF rect)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_000d: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		AddCapsule(val, rect);
		return val;
	}

	private static void AddCapsule(GraphicsPath path, RectangleF rect)
	{
		if (rect.Width > 0f && rect.Height > 0f)
		{
			if (rect.Width > rect.Height)
			{
				float height = rect.Height;
				RectangleF rectangleF = new RectangleF(size: new SizeF(height, height), location: rect.Location);
				path.AddArc(rectangleF, 90f, 180f);
				rectangleF.X = rect.Right - height;
				path.AddArc(rectangleF, 270f, 180f);
			}
			else if (rect.Width < rect.Height)
			{
				float height = rect.Width;
				RectangleF rectangleF = new RectangleF(size: new SizeF(height, height), location: rect.Location);
				path.AddArc(rectangleF, 180f, 180f);
				rectangleF.Y = rect.Bottom - height;
				path.AddArc(rectangleF, 0f, 180f);
			}
			else
			{
				path.AddEllipse(rect);
			}
		}
		else
		{
			path.AddEllipse(rect);
		}
		path.CloseFigure();
	}

	internal static void PaintIcons(this Canvas g, TType icon, Rectangle rect, string keyid)
	{
		switch (icon)
		{
		case TType.Success:
		{
			Bitmap imgExtend4 = SvgExtend.GetImgExtend(SvgDb.IcoSuccess, rect, Colour.Success.Get(keyid));
			try
			{
				if (imgExtend4 != null)
				{
					g.Image(imgExtend4, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend4)?.Dispose();
			}
		}
		case TType.Info:
		{
			Bitmap imgExtend3 = SvgExtend.GetImgExtend(SvgDb.IcoInfo, rect, Colour.Info.Get(keyid));
			try
			{
				if (imgExtend3 != null)
				{
					g.Image(imgExtend3, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend3)?.Dispose();
			}
		}
		case TType.Warn:
		{
			Bitmap imgExtend2 = SvgExtend.GetImgExtend(SvgDb.IcoWarn, rect, Colour.Warning.Get(keyid));
			try
			{
				if (imgExtend2 != null)
				{
					g.Image(imgExtend2, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend2)?.Dispose();
			}
		}
		case TType.Error:
		{
			Bitmap imgExtend = SvgExtend.GetImgExtend(SvgDb.IcoError, rect, Colour.Error.Get(keyid));
			try
			{
				if (imgExtend != null)
				{
					g.Image(imgExtend, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		}
	}

	internal static void PaintIcons(this Canvas g, TType icon, Rectangle rect, Color back, string keyid)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(back);
		try
		{
			g.FillEllipse((Brush)(object)val, new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		switch (icon)
		{
		case TType.Success:
		{
			Bitmap imgExtend4 = SvgExtend.GetImgExtend(SvgDb.IcoSuccess, rect, Colour.Success.Get(keyid));
			try
			{
				if (imgExtend4 != null)
				{
					g.Image(imgExtend4, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend4)?.Dispose();
			}
		}
		case TType.Info:
		{
			Bitmap imgExtend3 = SvgExtend.GetImgExtend(SvgDb.IcoInfo, rect, Colour.Info.Get(keyid));
			try
			{
				if (imgExtend3 != null)
				{
					g.Image(imgExtend3, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend3)?.Dispose();
			}
		}
		case TType.Warn:
		{
			Bitmap imgExtend2 = SvgExtend.GetImgExtend(SvgDb.IcoWarn, rect, Colour.Warning.Get(keyid));
			try
			{
				if (imgExtend2 != null)
				{
					g.Image(imgExtend2, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend2)?.Dispose();
			}
		}
		case TType.Error:
		{
			Bitmap imgExtend = SvgExtend.GetImgExtend(SvgDb.IcoError, rect, Colour.Error.Get(keyid));
			try
			{
				if (imgExtend != null)
				{
					g.Image(imgExtend, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		}
	}

	internal static void PaintIconGhosts(this Canvas g, TType icon, Rectangle rect, Color color)
	{
		switch (icon)
		{
		case TType.Success:
		{
			Bitmap imgExtend4 = SvgExtend.GetImgExtend(SvgDb.IcoSuccessGhost, rect, color);
			try
			{
				if (imgExtend4 != null)
				{
					g.Image(imgExtend4, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend4)?.Dispose();
			}
		}
		case TType.Info:
		{
			Bitmap imgExtend3 = SvgExtend.GetImgExtend(SvgDb.IcoInfoGhost, rect, color);
			try
			{
				if (imgExtend3 != null)
				{
					g.Image(imgExtend3, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend3)?.Dispose();
			}
		}
		case TType.Warn:
		{
			Bitmap imgExtend2 = SvgExtend.GetImgExtend(SvgDb.IcoWarnGhost, rect, color);
			try
			{
				if (imgExtend2 != null)
				{
					g.Image(imgExtend2, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend2)?.Dispose();
			}
		}
		case TType.Error:
		{
			Bitmap imgExtend = SvgExtend.GetImgExtend(SvgDb.IcoErrorGhost, rect, color);
			try
			{
				if (imgExtend != null)
				{
					g.Image(imgExtend, rect);
				}
				break;
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		}
	}

	internal static void PaintIconClose(this Canvas g, Rectangle rect, Color color)
	{
		g.PaintIconCore(rect, SvgDb.IcoErrorGhost, color);
	}

	internal static void PaintIconClose(this Canvas g, Rectangle rect, Color color, float dot)
	{
		g.PaintIconCore(rect, SvgDb.IcoErrorGhost, color, dot);
	}

	internal static void PaintIconCoreGhost(this Canvas g, Rectangle rect, string svg, Color back, Color fore)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(back);
		try
		{
			g.FillEllipse((Brush)(object)val, rect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		g.GetImgExtend(svg, rect, fore);
	}

	internal static void PaintIconCore(this Canvas g, Rectangle rect, string svg, Color back, Color fore)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(back);
		try
		{
			g.FillEllipse((Brush)(object)val, new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		g.GetImgExtend(svg, rect, fore);
	}

	internal static void PaintIconCore(this Canvas g, Rectangle rect, string svg, Color color)
	{
		g.GetImgExtend(svg, rect, color);
	}

	internal static void PaintIconCore(this Canvas g, Rectangle rect, string svg, Color color, float dot)
	{
		int num = (int)((float)rect.Height * dot);
		Rectangle rect2 = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + (rect.Height - num) / 2, num, num);
		g.GetImgExtend(svg, rect2, color);
	}

	public static void PaintBadge(this IControl control, Canvas g)
	{
		control.PaintBadge(((Control)control).Font, ((Control)control).ClientRectangle, g);
	}

	public static void PaintBadge(this BadgeConfig badegConfig, Font Font, Rectangle Rect, Canvas g)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		if (badegConfig.BadgeSvg != null)
		{
			int x = (int)((float)badegConfig.BadgeOffsetX * Config.Dpi);
			int y = (int)((float)badegConfig.BadgeOffsetY * Config.Dpi);
			Font val = new Font(Font.FontFamily, Font.Size * badegConfig.BadgeSize);
			try
			{
				int height = g.MeasureString("龍Qq", val).Height;
				Rectangle rect = PaintBadge(Rect, badegConfig.BadgeAlign, x, y, height, height);
				g.GetImgExtend(badegConfig.BadgeSvg, rect, badegConfig.BadgeBack ?? Colour.Error.Get("Badge"));
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (badegConfig.Badge == null)
		{
			return;
		}
		Color color = badegConfig.BadgeBack ?? Colour.Error.Get("Badge");
		float dpi = Config.Dpi;
		Font val2 = new Font(Font.FontFamily, Font.Size * badegConfig.BadgeSize);
		try
		{
			int x2 = (int)((float)badegConfig.BadgeOffsetX * Config.Dpi);
			int y2 = (int)((float)badegConfig.BadgeOffsetY * Config.Dpi);
			if (string.IsNullOrWhiteSpace(badegConfig.Badge))
			{
				int num = g.MeasureString("龍Qq", val2).Height / 2;
				Rectangle rectangle = PaintBadge(Rect, badegConfig.BadgeAlign, x2, y2, num, num);
				SolidBrush val3 = new SolidBrush(color);
				try
				{
					if (badegConfig.BadgeMode)
					{
						float num2 = dpi * 2f;
						float num3 = (float)num * 0.2f;
						float num4 = num3 * 2f;
						g.FillEllipse(Colour.ErrorColor.Get("Badge"), new RectangleF((float)rectangle.X - dpi, (float)rectangle.Y - dpi, (float)rectangle.Width + num2, (float)rectangle.Height + num2));
						GraphicsPath val4 = rectangle.RoundPath(1f, round: true);
						try
						{
							val4.AddEllipse(new RectangleF((float)rectangle.X + num3, (float)rectangle.Y + num3, (float)rectangle.Width - num4, (float)rectangle.Height - num4));
							g.Fill(color, val4);
							return;
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
					}
					g.FillEllipse(color, rectangle);
					g.DrawEllipse(Colour.ErrorColor.Get("Badge"), dpi, rectangle);
					return;
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			Size size = g.MeasureString(badegConfig.Badge, val2);
			StringFormat val5 = SF_NoWrap((StringAlignment)1, (StringAlignment)1);
			try
			{
				int num5 = (int)((float)size.Height * 1.2f);
				if (size.Height > size.Width)
				{
					Rectangle rectangle2 = PaintBadge(Rect, badegConfig.BadgeAlign, x2, y2, num5, num5);
					g.FillEllipse(color, rectangle2);
					g.DrawEllipse(Colour.ErrorColor.Get("Badge"), dpi, rectangle2);
					g.String(badegConfig.Badge, val2, Colour.ErrorColor.Get("Badge"), rectangle2, val5);
					return;
				}
				int w = size.Width + (num5 - size.Height);
				Rectangle rect2 = PaintBadge(Rect, badegConfig.BadgeAlign, x2, y2, w, num5);
				GraphicsPath val6 = rect2.RoundPath(rect2.Height);
				try
				{
					g.Fill(color, val6);
					g.Draw(Colour.ErrorColor.Get("Badge"), dpi, val6);
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
				g.String(badegConfig.Badge, val2, Colour.ErrorColor.Get("Badge"), rect2, val5);
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private static Rectangle PaintBadge(Rectangle rect, TAlignFrom align, int x, int y, int w, int h)
	{
		return align switch
		{
			TAlignFrom.TL => new Rectangle(rect.X + x, rect.Y + y, w, h), 
			TAlignFrom.BL => new Rectangle(rect.X + x, rect.Bottom - y - h, w, h), 
			TAlignFrom.BR => new Rectangle(rect.Right - x - w, rect.Bottom - y - h, w, h), 
			TAlignFrom.Top => new Rectangle(rect.X + (rect.Width - w) / 2, rect.Y + y, w, h), 
			TAlignFrom.Bottom => new Rectangle(rect.X + (rect.Width - w) / 2, rect.Bottom - y - h, w, h), 
			_ => new Rectangle(rect.Right - x - w, rect.Y + y, w, h), 
		};
	}

	public static void PaintBadge(this IControl control, DateBadge badge, Rectangle rect, Canvas g)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		Color color = badge.Fill ?? control.BadgeBack ?? Colour.Error.Get("Badge");
		float dpi = Config.Dpi;
		Font val = new Font(((Control)control).Font.FontFamily, ((Control)control).Font.Size * badge.Size);
		try
		{
			int x = (int)((float)badge.OffsetX * Config.Dpi);
			int y = (int)((float)badge.OffsetY * Config.Dpi);
			if (string.IsNullOrWhiteSpace(badge.Content))
			{
				int num = g.MeasureString("龍Qq", val).Height / 2;
				Rectangle rectangle = PaintBadge(rect, badge.Align, x, y, num, num);
				g.FillEllipse(color, rectangle);
				g.DrawEllipse(Colour.ErrorColor.Get("Badge"), dpi, rectangle);
				return;
			}
			Size size = g.MeasureString(badge.Content, val);
			StringFormat val2 = SF_NoWrap((StringAlignment)1, (StringAlignment)1);
			try
			{
				int num2 = (int)((float)size.Height * 1.2f);
				if (size.Height > size.Width)
				{
					Rectangle rectangle2 = PaintBadge(rect, badge.Align, x, y, num2, num2);
					g.FillEllipse(color, rectangle2);
					g.DrawEllipse(Colour.ErrorColor.Get("Badge"), dpi, rectangle2);
					g.String(badge.Content, val, Colour.ErrorColor.Get("Badge"), rectangle2, val2);
					return;
				}
				int w = size.Width + (num2 - size.Height);
				Rectangle rect2 = PaintBadge(rect, badge.Align, x, y, w, num2);
				GraphicsPath val3 = rect2.RoundPath(rect2.Height);
				try
				{
					g.Fill(color, val3);
					g.Draw(Colour.ErrorColor.Get("Badge"), dpi, val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				g.String(badge.Content, val, Colour.ErrorColor.Get("Badge"), rect2, val2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void PaintShadow(this Canvas g, ShadowConfig config, Rectangle _rect, Rectangle rect, float radius, bool round)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		int range = (int)((float)config.Shadow * Config.Dpi);
		int num = (int)((float)config.ShadowOffsetX * Config.Dpi);
		int num2 = (int)((float)config.ShadowOffsetY * Config.Dpi);
		Bitmap val = new Bitmap(_rect.Width, _rect.Height);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				GraphicsPath val3 = rect.RoundPath(radius, round);
				try
				{
					SolidBrush val4 = config.ShadowColor.Brush(Colour.TextBase.Get());
					try
					{
						val2.FillPath((Brush)(object)val4, val3);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				Blur(val, range);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			ImageAttributes val5 = new ImageAttributes();
			try
			{
				ColorMatrix val6 = new ColorMatrix
				{
					Matrix33 = config.ShadowOpacity
				};
				val5.SetColorMatrix(val6, (ColorMatrixFlag)0, (ColorAdjustType)1);
				g.Image((Image)(object)val, new Rectangle(_rect.X + num, _rect.Y + num2, _rect.Width, _rect.Height), 0, 0, _rect.Width, _rect.Height, (GraphicsUnit)2, val5);
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void Blur(Bitmap bmp, int range)
	{
		Blur(bmp, range, new Rectangle(0, 0, ((Image)bmp).Width, ((Image)bmp).Height));
	}

	public static void Blur(Bitmap bmp, int range, Rectangle rect)
	{
		if (range > 1)
		{
			using (UnsafeBitmap unsafeBitmap = new UnsafeBitmap(bmp, lockBitmap: true, (ImageLockMode)3))
			{
				BlurHorizontal(unsafeBitmap, range, rect);
				BlurVertical(unsafeBitmap, range, rect);
				BlurHorizontal(unsafeBitmap, range, rect);
				BlurVertical(unsafeBitmap, range, rect);
			}
		}
	}

	private static void BlurHorizontal(UnsafeBitmap unsafeBitmap, int range, Rectangle rect)
	{
		int x = rect.X;
		int y = rect.Y;
		int right = rect.Right;
		int bottom = rect.Bottom;
		int num = range / 2;
		ColorBgra[] array = new ColorBgra[unsafeBitmap.Width];
		for (int i = y; i < bottom; i++)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			for (int j = x - num; j < right; j++)
			{
				int num7 = j - num - 1;
				if (num7 >= x)
				{
					ColorBgra pixel = unsafeBitmap.GetPixel(num7, i);
					if (pixel.Bgra != 0)
					{
						num3 -= pixel.Red;
						num4 -= pixel.Green;
						num5 -= pixel.Blue;
						num6 -= pixel.Alpha;
					}
					num2--;
				}
				int num8 = j + num;
				if (num8 < right)
				{
					ColorBgra pixel2 = unsafeBitmap.GetPixel(num8, i);
					if (pixel2.Bgra != 0)
					{
						num3 += pixel2.Red;
						num4 += pixel2.Green;
						num5 += pixel2.Blue;
						num6 += pixel2.Alpha;
					}
					num2++;
				}
				if (j >= x)
				{
					array[j] = new ColorBgra((byte)(num5 / num2), (byte)(num4 / num2), (byte)(num3 / num2), (byte)(num6 / num2));
				}
			}
			for (int k = x; k < right; k++)
			{
				unsafeBitmap.SetPixel(k, i, array[k]);
			}
		}
	}

	private static void BlurVertical(UnsafeBitmap unsafeBitmap, int range, Rectangle rect)
	{
		int x = rect.X;
		int y = rect.Y;
		int right = rect.Right;
		int bottom = rect.Bottom;
		int num = range / 2;
		ColorBgra[] array = new ColorBgra[unsafeBitmap.Height];
		for (int i = x; i < right; i++)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			for (int j = y - num; j < bottom; j++)
			{
				int num7 = j - num - 1;
				if (num7 >= y)
				{
					ColorBgra pixel = unsafeBitmap.GetPixel(i, num7);
					if (pixel.Bgra != 0)
					{
						num3 -= pixel.Red;
						num4 -= pixel.Green;
						num5 -= pixel.Blue;
						num6 -= pixel.Alpha;
					}
					num2--;
				}
				int num8 = j + num;
				if (num8 < bottom)
				{
					ColorBgra pixel2 = unsafeBitmap.GetPixel(i, num8);
					if (pixel2.Bgra != 0)
					{
						num3 += pixel2.Red;
						num4 += pixel2.Green;
						num5 += pixel2.Blue;
						num6 += pixel2.Alpha;
					}
					num2++;
				}
				if (j >= y)
				{
					array[j] = new ColorBgra((byte)(num5 / num2), (byte)(num4 / num2), (byte)(num3 / num2), (byte)(num6 / num2));
				}
			}
			for (int k = y; k < bottom; k++)
			{
				unsafeBitmap.SetPixel(i, k, array[k]);
			}
		}
	}

	public static Bitmap PaintShadow(this GraphicsPath path, int width, int height, int range = 10)
	{
		return path.PaintShadow(width, height, Color.Black, range);
	}

	public static Bitmap PaintShadow(this GraphicsPath path, int width, int height, Color color, int range = 10)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		Bitmap val = new Bitmap(width, height);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		try
		{
			SolidBrush val3 = new SolidBrush(color);
			try
			{
				val2.FillPath((Brush)(object)val3, path);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			Blur(val, range);
			return val;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public static ILayeredFormOpacity FormMask(this Form owner, Form form, bool MaskClosable = false)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		Form form2 = form;
		bool isclose = false;
		LayeredFormMask mask = new LayeredFormMask(owner);
		if (MaskClosable)
		{
			try
			{
				((Control)mask).Click += delegate
				{
					form2.Close();
				};
			}
			catch
			{
			}
		}
		((Form)mask).Show((IWin32Window)(object)owner);
		form2.FormClosed += (FormClosedEventHandler)delegate
		{
			if (!isclose)
			{
				isclose = true;
				mask.IClose();
			}
		};
		return mask;
	}

	public static ILayeredFormOpacity FormMask(this Form owner, ILayeredForm form, bool MaskClosable = false)
	{
		ILayeredForm form2 = form;
		bool isclose = false;
		LayeredFormMask mask = new LayeredFormMask(owner);
		if (MaskClosable)
		{
			try
			{
				((Control)mask).Click += delegate
				{
					form2.IClose();
				};
			}
			catch
			{
			}
		}
		((Form)mask).Show((IWin32Window)(object)owner);
		((Component)(object)form2).Disposed += delegate
		{
			if (!isclose)
			{
				isclose = true;
				mask.IClose();
			}
		};
		return mask;
	}

	internal static bool FormFrame(this Form form, out int Radius, out int Padd)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Padd = (Radius = 0);
		if ((int)form.WindowState != 2)
		{
			if (form is BorderlessForm borderlessForm)
			{
				Radius = (int)((float)borderlessForm.Radius * Config.Dpi);
				return false;
			}
			if ((int)form.FormBorderStyle == 0)
			{
				return false;
			}
			if (OS.Win11)
			{
				Radius = (int)(8f * Config.Dpi);
			}
			if (form is Window || form is FormNoBar)
			{
				return false;
			}
			Padd = (int)(7f * Config.Dpi);
			return true;
		}
		return false;
	}

	public static RectTextLR IconRect(this Rectangle rect, int text_height, bool icon_l, bool icon_r, bool right, bool muit, float gap_ratio = 0.4f, float sp_ratio = 0.25f, float icon_ratio = 0.7f)
	{
		RectTextLR rectTextLR = new RectTextLR();
		int num = (int)((float)text_height * gap_ratio);
		int num2 = (int)((float)text_height * icon_ratio);
		int num3 = num * 2;
		if (muit)
		{
			if (icon_l && icon_r)
			{
				int num4 = (int)((float)text_height * sp_ratio);
				rectTextLR.text = new Rectangle(rect.X + num + num2 + num4, rect.Y + num, rect.Width - (num + num2 + num4) * 2, rect.Height - num3);
				rectTextLR.l = new Rectangle(rect.X + num, rect.Y + num + (text_height - num2) / 2, num2, num2);
				rectTextLR.r = new Rectangle(rectTextLR.text.Right + num4, rectTextLR.l.Y, num2, num2);
				if (right)
				{
					Rectangle r = rectTextLR.r;
					rectTextLR.r = rectTextLR.l;
					rectTextLR.l = r;
					return rectTextLR;
				}
			}
			else if (icon_l)
			{
				int num5 = (int)((float)text_height * sp_ratio);
				if (right)
				{
					rectTextLR.text = new Rectangle(rect.X + num, rect.Y + num, rect.Width - num3 - num2 - num5, rect.Height - num3);
					rectTextLR.l = new Rectangle(rectTextLR.text.Right + num5, rect.Y + num + (text_height - num2) / 2, num2, num2);
					return rectTextLR;
				}
				rectTextLR.text = new Rectangle(rect.X + num + num2 + num5, rect.Y + num, rect.Width - num3 - num2 - num5, rect.Height - num3);
				rectTextLR.l = new Rectangle(rect.X + num, rect.Y + num + (text_height - num2) / 2, num2, num2);
			}
			else if (icon_r)
			{
				int num6 = (int)((float)text_height * sp_ratio);
				if (right)
				{
					rectTextLR.text = new Rectangle(rect.X + num + num2 + num6, rect.Y + num, rect.Width - num3 - num2 - num6, rect.Height - num3);
					rectTextLR.r = new Rectangle(rect.X + num, rect.Y + num + (text_height - num2) / 2, num2, num2);
					return rectTextLR;
				}
				rectTextLR.text = new Rectangle(rect.X + num, rect.Y + num, rect.Width - num3 - num2 - num6, rect.Height - num3);
				rectTextLR.r = new Rectangle(rectTextLR.text.Right + num6, rect.Y + num + (text_height - num2) / 2, num2, num2);
			}
			else
			{
				rectTextLR.text = new Rectangle(rect.X + num, rect.Y + num, rect.Width - num3, rect.Height - num3);
			}
		}
		else if (icon_l && icon_r)
		{
			int num7 = (int)((float)text_height * sp_ratio);
			rectTextLR.text = new Rectangle(rect.X + num + num2 + num7, rect.Y + (rect.Height - text_height) / 2, rect.Width - (num + num2 + num7) * 2, text_height);
			rectTextLR.l = new Rectangle(rect.X + num, rect.Y + (rect.Height - num2) / 2, num2, num2);
			rectTextLR.r = new Rectangle(rectTextLR.text.Right + num7, rectTextLR.l.Y, num2, num2);
			if (right)
			{
				Rectangle r2 = rectTextLR.r;
				rectTextLR.r = rectTextLR.l;
				rectTextLR.l = r2;
				return rectTextLR;
			}
		}
		else if (icon_l)
		{
			int num8 = (int)((float)text_height * sp_ratio);
			if (right)
			{
				rectTextLR.text = new Rectangle(rect.X + num, rect.Y + (rect.Height - text_height) / 2, rect.Width - num3 - num2 - num8, text_height);
				rectTextLR.l = new Rectangle(rectTextLR.text.Right + num8, rect.Y + (rect.Height - num2) / 2, num2, num2);
				return rectTextLR;
			}
			rectTextLR.text = new Rectangle(rect.X + num + num2 + num8, rect.Y + (rect.Height - text_height) / 2, rect.Width - num3 - num2 - num8, text_height);
			rectTextLR.l = new Rectangle(rect.X + num, rect.Y + (rect.Height - num2) / 2, num2, num2);
		}
		else if (icon_r)
		{
			int num9 = (int)((float)text_height * sp_ratio);
			if (right)
			{
				rectTextLR.text = new Rectangle(rect.X + num + num2 + num9, rect.Y + (rect.Height - text_height) / 2, rect.Width - num3 - num2 - num9, text_height);
				rectTextLR.r = new Rectangle(rect.X + num, rect.Y + (rect.Height - num2) / 2, num2, num2);
				return rectTextLR;
			}
			rectTextLR.text = new Rectangle(rect.X + num, rect.Y + (rect.Height - text_height) / 2, rect.Width - num3 - num2 - num9, text_height);
			rectTextLR.r = new Rectangle(rectTextLR.text.Right + num9, rect.Y + (rect.Height - num2) / 2, num2, num2);
		}
		else
		{
			rectTextLR.text = new Rectangle(rect.X + num, rect.Y + (rect.Height - text_height) / 2, rect.Width - num3, text_height);
		}
		return rectTextLR;
	}

	public static void IconRectL(this Rectangle rect, int text_height, out Rectangle icon_rect, out Rectangle text_rect, float size = 0.8f)
	{
		int num = (int)((float)text_height * size);
		int num2 = num / 2;
		int num3 = num * 2;
		icon_rect = new Rectangle(rect.X + num2, rect.Y + (rect.Height - num) / 2, num, num);
		text_rect = new Rectangle(rect.X + num3, rect.Y, rect.Width - num3, rect.Height);
	}

	public static Rectangle DeflateRect(this Rectangle rect, Padding padding)
	{
		rect.X += ((Padding)(ref padding)).Left;
		rect.Y += ((Padding)(ref padding)).Top;
		rect.Width -= ((Padding)(ref padding)).Horizontal;
		rect.Height -= ((Padding)(ref padding)).Vertical;
		return rect;
	}

	public static Rectangle DeflateRect(this Rectangle rect, Padding padding, ShadowConfig config, TAlignMini align, float borderWidth = 0f)
	{
		if (config.Shadow > 0)
		{
			int num = (int)((float)config.Shadow * Config.Dpi);
			int num2 = num * 2;
			int num3 = Math.Abs((int)((float)config.ShadowOffsetX * Config.Dpi));
			int num4 = Math.Abs((int)((float)config.ShadowOffsetY * Config.Dpi));
			int num5;
			int num7;
			int num6;
			int num8;
			switch (align)
			{
			case TAlignMini.Top:
				num5 = rect.X + ((Padding)(ref padding)).Left;
				num7 = rect.Width - ((Padding)(ref padding)).Horizontal;
				num6 = rect.Y + ((Padding)(ref padding)).Top + num;
				num8 = rect.Height - ((Padding)(ref padding)).Vertical - num;
				break;
			case TAlignMini.Bottom:
				num5 = rect.X + ((Padding)(ref padding)).Left;
				num7 = rect.Width - ((Padding)(ref padding)).Horizontal;
				num6 = rect.Y + ((Padding)(ref padding)).Top;
				num8 = rect.Height - ((Padding)(ref padding)).Vertical - num;
				break;
			case TAlignMini.Left:
				num6 = rect.Y + ((Padding)(ref padding)).Top;
				num8 = rect.Height - ((Padding)(ref padding)).Vertical;
				num5 = rect.X + ((Padding)(ref padding)).Left + num;
				num7 = rect.Width - ((Padding)(ref padding)).Horizontal - num;
				break;
			case TAlignMini.Right:
				num6 = rect.Y + ((Padding)(ref padding)).Top;
				num8 = rect.Height - ((Padding)(ref padding)).Vertical;
				num5 = rect.X + ((Padding)(ref padding)).Left;
				num7 = rect.Width - ((Padding)(ref padding)).Horizontal - num;
				break;
			default:
				num5 = rect.X + ((Padding)(ref padding)).Left + num;
				num6 = rect.Y + ((Padding)(ref padding)).Top + num;
				num7 = rect.Width - ((Padding)(ref padding)).Horizontal - num2;
				num8 = rect.Height - ((Padding)(ref padding)).Vertical - num2;
				break;
			}
			if (config.ShadowOffsetX < 0)
			{
				num5 += num3;
				num7 -= num3;
			}
			if (config.ShadowOffsetY < 0)
			{
				num6 += num4;
				num8 -= num4;
			}
			if (borderWidth > 0f)
			{
				int num9 = (int)Math.Ceiling(borderWidth * Config.Dpi);
				int num10 = num9 * 2;
				return new Rectangle(num5 + num9, num6 + num9, num7 - num10, num8 - num10);
			}
			return new Rectangle(num5, num6, num7, num8);
		}
		if (borderWidth > 0f)
		{
			int num11 = (int)Math.Ceiling(borderWidth * Config.Dpi);
			int num12 = num11 * 2;
			return new Rectangle(rect.X + ((Padding)(ref padding)).Left + num11, rect.Y + ((Padding)(ref padding)).Top + num11, rect.Width - ((Padding)(ref padding)).Horizontal - num12, rect.Height - ((Padding)(ref padding)).Vertical - num12);
		}
		return new Rectangle(rect.X + ((Padding)(ref padding)).Left, rect.Y + ((Padding)(ref padding)).Top, rect.Width - ((Padding)(ref padding)).Horizontal, rect.Height - ((Padding)(ref padding)).Vertical);
	}

	public static Rectangle PaddingRect(this Rectangle rect, ShadowConfig config, TAlignMini align, float borderWidth = 0f)
	{
		if (config.Shadow > 0)
		{
			int num = (int)((float)config.Shadow * Config.Dpi);
			int num2 = num * 2;
			int num3 = Math.Abs((int)((float)config.ShadowOffsetX * Config.Dpi));
			int num4 = Math.Abs((int)((float)config.ShadowOffsetY * Config.Dpi));
			int num5;
			int num7;
			int num6;
			int num8;
			switch (align)
			{
			case TAlignMini.Top:
				num5 = rect.X;
				num7 = rect.Width;
				num6 = rect.Y + num;
				num8 = rect.Height - num;
				break;
			case TAlignMini.Bottom:
				num5 = rect.X;
				num7 = rect.Width;
				num6 = rect.Y;
				num8 = rect.Height - num;
				break;
			case TAlignMini.Left:
				num6 = rect.Y;
				num8 = rect.Height;
				num5 = rect.X + num;
				num7 = rect.Width - num;
				break;
			case TAlignMini.Right:
				num6 = rect.Y;
				num8 = rect.Height;
				num5 = rect.X;
				num7 = rect.Width - num;
				break;
			default:
				num5 = rect.X + num;
				num6 = rect.Y + num;
				num7 = rect.Width - num2;
				num8 = rect.Height - num2;
				break;
			}
			if (config.ShadowOffsetX < 0)
			{
				num5 += num3;
				num7 -= num3;
			}
			if (config.ShadowOffsetY < 0)
			{
				num6 += num4;
				num8 -= num4;
			}
			if (borderWidth > 0f)
			{
				int num9 = (int)Math.Ceiling(borderWidth * Config.Dpi / 2f);
				int num10 = num9 * 2;
				return new Rectangle(num5 + num9, num6 + num9, num7 - num10, num8 - num10);
			}
			return new Rectangle(num5, num6, num7, num8);
		}
		if (borderWidth > 0f)
		{
			int num11 = (int)Math.Ceiling(borderWidth * Config.Dpi / 2f);
			int num12 = num11 * 2;
			return new Rectangle(rect.X + num11, rect.Y + num11, rect.Width - num12, rect.Height - num12);
		}
		return rect;
	}

	public static Rectangle PaddingRect(this Rectangle rect, Padding padding, int x, int y, int r, int b)
	{
		return new Rectangle(rect.X + ((Padding)(ref padding)).Left + x, rect.Y + ((Padding)(ref padding)).Top + y, rect.Width - ((Padding)(ref padding)).Horizontal - r, rect.Height - ((Padding)(ref padding)).Vertical - b);
	}

	public static Rectangle PaddingRect(this Rectangle rect, params Padding[] paddings)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < paddings.Length; i++)
		{
			Padding val = paddings[i];
			rect.X += ((Padding)(ref val)).Left;
			rect.Y += ((Padding)(ref val)).Top;
			rect.Width -= ((Padding)(ref val)).Horizontal;
			rect.Height -= ((Padding)(ref val)).Vertical;
		}
		return rect;
	}

	public static Rectangle PaddingRect(this Rectangle rect, Padding padding, float size = 0f)
	{
		if (size > 0f)
		{
			int num = (int)Math.Round(size);
			int num2 = num * 2;
			return new Rectangle(rect.X + ((Padding)(ref padding)).Left + num, rect.Y + ((Padding)(ref padding)).Top + num, rect.Width - ((Padding)(ref padding)).Horizontal - num2, rect.Height - ((Padding)(ref padding)).Vertical - num2);
		}
		return new Rectangle(rect.X + ((Padding)(ref padding)).Left, rect.Y + ((Padding)(ref padding)).Top, rect.Width - ((Padding)(ref padding)).Horizontal, rect.Height - ((Padding)(ref padding)).Vertical);
	}

	public static Rectangle PaddingRect(this Rectangle rect, Padding padding, int x, int y, int r, int b, float size = 0f)
	{
		if (size > 0f)
		{
			int num = (int)Math.Round(size);
			int num2 = num * 2;
			return new Rectangle(rect.X + ((Padding)(ref padding)).Left + num + x, rect.Y + ((Padding)(ref padding)).Top + num + y, rect.Width - ((Padding)(ref padding)).Horizontal - num2 - r, rect.Height - ((Padding)(ref padding)).Vertical - num2 - b);
		}
		return new Rectangle(rect.X + ((Padding)(ref padding)).Left + x, rect.Y + ((Padding)(ref padding)).Top + y, rect.Width - ((Padding)(ref padding)).Horizontal - r, rect.Height - ((Padding)(ref padding)).Vertical - b);
	}

	public static Rectangle ReadRect(this Rectangle rect, float size, TShape shape, bool joinLeft, bool joinRight)
	{
		if (shape == TShape.Circle)
		{
			int num = (int)Math.Round(size);
			int num2 = num * 2;
			if (rect.Width > rect.Height)
			{
				int num3 = rect.Height - num2;
				return new Rectangle(rect.X + (rect.Width - num3) / 2, rect.Y + num, num3, num3);
			}
			int num4 = rect.Width - num2;
			return new Rectangle(rect.X + num, rect.Y + (rect.Height - num4) / 2, num4, num4);
		}
		return rect.ReadRect(size, joinLeft, joinRight);
	}

	public static Rectangle ReadRect(this Rectangle rect, float size, bool joinLeft, bool joinRight)
	{
		int num = (int)Math.Round(size);
		int num2 = num * 2;
		if (joinLeft && joinRight)
		{
			return new Rectangle(rect.X, rect.Y + num, rect.Width, rect.Height - num2);
		}
		if (joinLeft)
		{
			Rectangle result = new Rectangle(rect.X, rect.Y + num, rect.Width - num, rect.Height - num2);
			rect.X = -num;
			rect.Width += num;
			return result;
		}
		if (joinRight)
		{
			Rectangle result2 = new Rectangle(rect.Width - (rect.Width - num), rect.Y + num, rect.Width - num, rect.Height - num2);
			rect.X = 0;
			rect.Width += num;
			return result2;
		}
		return new Rectangle(rect.X + num, rect.Y + num, rect.Width - num2, rect.Height - num2);
	}

	public static void SetAlignment(this ContentAlignment textAlign, ref StringFormat stringFormat)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected I4, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		if ((int)textAlign <= 32)
		{
			switch (textAlign - 1)
			{
			case 0:
				stringFormat.Alignment = (StringAlignment)0;
				stringFormat.LineAlignment = (StringAlignment)0;
				return;
			case 1:
				stringFormat.Alignment = (StringAlignment)1;
				stringFormat.LineAlignment = (StringAlignment)0;
				return;
			case 3:
				stringFormat.Alignment = (StringAlignment)2;
				stringFormat.LineAlignment = (StringAlignment)0;
				return;
			case 2:
				return;
			}
			if ((int)textAlign != 16)
			{
				if ((int)textAlign == 32)
				{
					stringFormat.Alignment = (StringAlignment)1;
					stringFormat.LineAlignment = (StringAlignment)1;
				}
			}
			else
			{
				stringFormat.Alignment = (StringAlignment)0;
				stringFormat.LineAlignment = (StringAlignment)1;
			}
		}
		else if ((int)textAlign <= 256)
		{
			if ((int)textAlign != 64)
			{
				if ((int)textAlign == 256)
				{
					stringFormat.Alignment = (StringAlignment)0;
					stringFormat.LineAlignment = (StringAlignment)2;
				}
			}
			else
			{
				stringFormat.Alignment = (StringAlignment)2;
				stringFormat.LineAlignment = (StringAlignment)1;
			}
		}
		else if ((int)textAlign != 512)
		{
			if ((int)textAlign == 1024)
			{
				stringFormat.Alignment = (StringAlignment)2;
				stringFormat.LineAlignment = (StringAlignment)2;
			}
		}
		else
		{
			stringFormat.Alignment = (StringAlignment)1;
			stringFormat.LineAlignment = (StringAlignment)2;
		}
	}

	public static void SetAlignment(this HorizontalAlignment textAlign, ref StringFormat stringFormat)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		switch ((int)textAlign)
		{
		case 0:
			stringFormat.Alignment = (StringAlignment)0;
			break;
		case 2:
			stringFormat.Alignment = (StringAlignment)1;
			break;
		case 1:
			stringFormat.Alignment = (StringAlignment)2;
			break;
		}
	}

	public static PointF[] CheckArrow(this Rectangle rect)
	{
		float num = (float)rect.Height * 0.15f;
		float num2 = (float)rect.Height * 0.2f;
		float num3 = (float)rect.Height * 0.26f;
		return new PointF[3]
		{
			new PointF((float)rect.X + num, rect.Y + rect.Height / 2),
			new PointF((float)rect.X + (float)rect.Width * 0.4f, (float)rect.Y + ((float)rect.Height - num3)),
			new PointF((float)(rect.X + rect.Width) - num2, (float)rect.Y + num2)
		};
	}

	public static PointF[] TriangleLines(this Rectangle rect, float prog, float d = 0.7f)
	{
		float num = (float)rect.Width * d / 2f;
		float num2 = (float)rect.X + (float)rect.Width / 2f;
		float num3 = (float)rect.Y + (float)rect.Height / 2f;
		if (prog != 0f)
		{
			if (prog > 0f)
			{
				float num4 = num * prog / 2f;
				return new PointF[3]
				{
					new PointF(num2 - num, num3 + num4),
					new PointF(num2, num3 - num4),
					new PointF(num2 + num, num3 + num4)
				};
			}
			float num5 = num * (0f - prog) / 2f;
			return new PointF[3]
			{
				new PointF(num2 - num, num3 - num5),
				new PointF(num2, num3 + num5),
				new PointF(num2 + num, num3 - num5)
			};
		}
		return new PointF[2]
		{
			new PointF(num2 - num, num3),
			new PointF(num2 + num, num3)
		};
	}

	public static PointF[] TriangleLines(this RectangleF rect, float prog, float d = 0.7f)
	{
		float num = rect.Width * d / 2f;
		float num2 = rect.X + rect.Width / 2f;
		float num3 = rect.Y + rect.Height / 2f;
		if (prog != 0f)
		{
			if (prog > 0f)
			{
				float num4 = num * prog / 2f;
				return new PointF[3]
				{
					new PointF(num2 - num, num3 + num4),
					new PointF(num2, num3 - num4),
					new PointF(num2 + num, num3 + num4)
				};
			}
			float num5 = num * (0f - prog) / 2f;
			return new PointF[3]
			{
				new PointF(num2 - num, num3 - num5),
				new PointF(num2, num3 + num5),
				new PointF(num2 + num, num3 - num5)
			};
		}
		return new PointF[2]
		{
			new PointF(num2 - num, num3),
			new PointF(num2 + num, num3)
		};
	}

	public static PointF[] TriangleLines(this TAlignMini align, RectangleF rect, float b = 0.375f)
	{
		float num = rect.Height * b / 2f;
		float num2 = rect.X + rect.Width / 2f;
		float num3 = rect.Y + rect.Height / 2f;
		float num4 = num / 2f;
		return align switch
		{
			TAlignMini.Top => new PointF[3]
			{
				new PointF(num2 - num, num3 + num4),
				new PointF(num2, num3 - num4),
				new PointF(num2 + num, num3 + num4)
			}, 
			TAlignMini.Bottom => new PointF[3]
			{
				new PointF(num2 - num, num3 - num4),
				new PointF(num2, num3 + num4),
				new PointF(num2 + num, num3 - num4)
			}, 
			TAlignMini.Left => new PointF[3]
			{
				new PointF(num2 + num4, num3 - num),
				new PointF(num2 - num4, num3),
				new PointF(num2 + num4, num3 + num)
			}, 
			_ => new PointF[3]
			{
				new PointF(num2 - num4, num3 - num),
				new PointF(num2 + num4, num3),
				new PointF(num2 - num4, num3 + num)
			}, 
		};
	}

	public static PointF[] AlignLines(this TAlign align, float arrow_size, RectangleF rect, RectangleF rect_read)
	{
		switch (align)
		{
		case TAlign.Top:
		{
			float num17 = rect.Width / 2f;
			float num18 = rect_read.Y + rect_read.Height;
			return new PointF[3]
			{
				new PointF(num17 - arrow_size, num18),
				new PointF(num17 + arrow_size, num18),
				new PointF(num17, num18 + arrow_size)
			};
		}
		case TAlign.Bottom:
		{
			float num15 = rect.Width / 2f;
			float num16 = rect_read.Y - arrow_size;
			return new PointF[3]
			{
				new PointF(num15, num16),
				new PointF(num15 - arrow_size, num16 + arrow_size),
				new PointF(num15 + arrow_size, num16 + arrow_size)
			};
		}
		case TAlign.LB:
		case TAlign.Left:
		case TAlign.LT:
		{
			float num13 = rect_read.X + rect_read.Width;
			float num14 = rect.Height / 2f;
			return new PointF[3]
			{
				new PointF(num13, num14 - arrow_size),
				new PointF(num13, num14 + arrow_size),
				new PointF(num13 + arrow_size, num14)
			};
		}
		case TAlign.RT:
		case TAlign.Right:
		case TAlign.RB:
		{
			float num11 = rect_read.X - arrow_size;
			float num12 = rect.Height / 2f;
			return new PointF[3]
			{
				new PointF(num11, num12),
				new PointF(num11 + arrow_size, num12 - arrow_size),
				new PointF(num11 + arrow_size, num12 + arrow_size)
			};
		}
		case TAlign.BL:
		{
			float num9 = rect_read.X + arrow_size * 3f;
			float num10 = rect_read.Y - arrow_size;
			return new PointF[3]
			{
				new PointF(num9, num10),
				new PointF(num9 - arrow_size, num10 + arrow_size),
				new PointF(num9 + arrow_size, num10 + arrow_size)
			};
		}
		case TAlign.BR:
		{
			float num7 = rect_read.X + rect_read.Width - arrow_size * 3f;
			float num8 = rect_read.Y - arrow_size;
			return new PointF[3]
			{
				new PointF(num7, num8),
				new PointF(num7 - arrow_size, num8 + arrow_size),
				new PointF(num7 + arrow_size, num8 + arrow_size)
			};
		}
		case TAlign.TL:
		{
			float num5 = rect_read.X + arrow_size * 3f;
			float num6 = rect_read.Y + rect_read.Height;
			return new PointF[3]
			{
				new PointF(num5 - arrow_size, num6),
				new PointF(num5 + arrow_size, num6),
				new PointF(num5, num6 + arrow_size)
			};
		}
		case TAlign.TR:
		{
			float num3 = rect_read.X + rect_read.Width - arrow_size * 3f;
			float num4 = rect_read.Y + rect_read.Height;
			return new PointF[3]
			{
				new PointF(num3 - arrow_size, num4),
				new PointF(num3 + arrow_size, num4),
				new PointF(num3, num4 + arrow_size)
			};
		}
		default:
		{
			float num = rect.Width / 2f;
			float num2 = rect_read.Y + rect_read.Height;
			return new PointF[3]
			{
				new PointF(num - arrow_size, num2),
				new PointF(num + arrow_size, num2),
				new PointF(num, num2 + arrow_size)
			};
		}
		}
	}

	public static TAlignMini AlignMini(this TAlign align)
	{
		switch (align)
		{
		case TAlign.BR:
		case TAlign.Bottom:
		case TAlign.BL:
			return TAlignMini.Bottom;
		case TAlign.TL:
		case TAlign.Top:
		case TAlign.TR:
			return TAlignMini.Top;
		case TAlign.RT:
		case TAlign.Right:
		case TAlign.RB:
			return TAlignMini.Right;
		case TAlign.LB:
		case TAlign.Left:
		case TAlign.LT:
			return TAlignMini.Left;
		default:
			return TAlignMini.None;
		}
	}

	public static TAlign AlignMiniReverse(this TAlign align, bool vertical)
	{
		if (vertical)
		{
			if (align == TAlign.TL || align == TAlign.BL || align == TAlign.LB || align == TAlign.Left || align == TAlign.LT)
			{
				return TAlign.Right;
			}
			return TAlign.Left;
		}
		if (align == TAlign.TL || align == TAlign.Top || align == TAlign.TR || align == TAlign.RT)
		{
			return TAlign.Bottom;
		}
		return TAlign.Top;
	}

	public static Point AlignPoint(this TAlign align, Point point, Size size, int width, int height)
	{
		switch (align)
		{
		case TAlign.Top:
			return new Point(point.X + (size.Width - width) / 2, point.Y - height);
		case TAlign.TL:
			return new Point(point.X, point.Y - height);
		case TAlign.TR:
			return new Point(point.X + size.Width - width, point.Y - height);
		case TAlign.Bottom:
			return new Point(point.X + (size.Width - width) / 2, point.Y + size.Height);
		case TAlign.BL:
			return new Point(point.X, point.Y + size.Height);
		case TAlign.BR:
			return new Point(point.X + size.Width - width, point.Y + size.Height);
		case TAlign.LB:
		case TAlign.Left:
		case TAlign.LT:
			return new Point(point.X - width, point.Y + (size.Height - height) / 2);
		case TAlign.RT:
		case TAlign.Right:
		case TAlign.RB:
			return new Point(point.X + size.Width, point.Y + (size.Height - height) / 2);
		default:
			return new Point(point.X + (size.Width - width) / 2, point.Y - height);
		}
	}

	public static Point AlignPoint(this TAlign align, Rectangle rect, Rectangle size)
	{
		return align.AlignPoint(rect.Location, rect.Size, size.Width, size.Height);
	}

	public static Point AlignPoint(this TAlign align, Rectangle rect, int width, int height)
	{
		return align.AlignPoint(rect.Location, rect.Size, width, height);
	}
}

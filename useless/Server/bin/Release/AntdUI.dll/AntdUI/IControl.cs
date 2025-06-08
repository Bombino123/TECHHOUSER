using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Vanara.PInvoke;

namespace AntdUI;

[ToolboxItem(false)]
[Localizable(true)]
public class IControl : Control, BadgeConfig
{
	public delegate void DragEventHandler(object sender, StringsEventArgs e);

	private bool visible = true;

	private string? badge;

	private string? badgeSvg;

	private TAlignFrom badgeAlign = TAlignFrom.TR;

	private float badgeSize = 0.6f;

	private bool badgeMode;

	private Color? badgeback;

	private int badgeOffsetX = 1;

	private int badgeOffsetY = 1;

	private static bool disableDataBinding;

	private CursorType oldcursor;

	private bool setwindow;

	private bool mdown;

	private int mdownd;

	private int oldX;

	private int oldY;

	private int oldMY;

	private ITask? taskTouch;

	private const int WM_POINTERDOWN = 582;

	private const int WM_POINTERUP = 583;

	private const int WM_LBUTTONDOWN = 513;

	private const int WM_LBUTTONUP = 514;

	private FileDropHandler? fileDrop;

	internal Func<string[], string[]?>? ONDRAG;

	[Description("确定该控件是可见的还是隐藏的")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			if (visible == value)
			{
				return;
			}
			visible = value;
			if (((Control)this).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					((Control)this).Visible = value;
				});
			}
			else
			{
				((Control)this).Visible = value;
			}
		}
	}

	[Description("指示是否已启用该控件")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool Enabled
	{
		get
		{
			return ((Control)this).Enabled;
		}
		set
		{
			if (((Control)this).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					((Control)this).Enabled = value;
				});
			}
			else
			{
				((Control)this).Enabled = value;
			}
		}
	}

	[Description("徽标内容")]
	[Category("徽标")]
	[DefaultValue(null)]
	public string? Badge
	{
		get
		{
			return badge;
		}
		set
		{
			if (!(badge == value))
			{
				badge = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("徽标SVG")]
	[Category("徽标")]
	[DefaultValue(null)]
	public string? BadgeSvg
	{
		get
		{
			return badgeSvg;
		}
		set
		{
			if (!(badgeSvg == value))
			{
				badgeSvg = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("徽标方向")]
	[Category("徽标")]
	[DefaultValue(TAlignFrom.TR)]
	public TAlignFrom BadgeAlign
	{
		get
		{
			return badgeAlign;
		}
		set
		{
			if (badgeAlign != value)
			{
				badgeAlign = value;
				if (badge != null || badgeSvg != null)
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("徽标比例")]
	[Category("徽标")]
	[DefaultValue(0.6f)]
	public float BadgeSize
	{
		get
		{
			return badgeSize;
		}
		set
		{
			if (badgeSize != value)
			{
				badgeSize = value;
				if (badge != null || badgeSvg != null)
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("徽标模式（镂空）")]
	[Category("徽标")]
	[DefaultValue(false)]
	public bool BadgeMode
	{
		get
		{
			return badgeMode;
		}
		set
		{
			if (badgeMode != value)
			{
				badgeMode = value;
				if (badge != null || badgeSvg != null)
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("徽标背景颜色")]
	[Category("徽标")]
	[DefaultValue(null)]
	public Color? BadgeBack
	{
		get
		{
			return badgeback;
		}
		set
		{
			if (!(badgeback == value))
			{
				badgeback = value;
				if (badge != null || badgeSvg != null)
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("徽标偏移X")]
	[Category("徽标")]
	[DefaultValue(1)]
	public int BadgeOffsetX
	{
		get
		{
			return badgeOffsetX;
		}
		set
		{
			if (badgeOffsetX != value)
			{
				badgeOffsetX = value;
				if (badge != null || badgeSvg != null)
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("徽标偏移Y")]
	[Category("徽标")]
	[DefaultValue(1)]
	public int BadgeOffsetY
	{
		get
		{
			return badgeOffsetY;
		}
		set
		{
			if (badgeOffsetY != value)
			{
				badgeOffsetY = value;
				if (badge != null || badgeSvg != null)
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Browsable(false)]
	public virtual GraphicsPath RenderRegion
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			GraphicsPath val = new GraphicsPath();
			val.AddRectangle(((Control)this).ClientRectangle);
			return val;
		}
	}

	[Browsable(false)]
	public virtual Rectangle ReadRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);

	[Description("悬停光标")]
	[Category("光标")]
	[DefaultValue(typeof(Cursor), "Hand")]
	public virtual Cursor HandCursor { get; set; } = Cursors.Hand;


	[Description("拖拽文件夹处理")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool HandDragFolder { get; set; } = true;


	[Description("文件拖拽后时发生")]
	[Category("行为")]
	public event DragEventHandler? DragChanged;

	public IControl(ControlType ctype = ControlType.Default)
	{
		switch (ctype)
		{
		case ControlType.Default:
			((Control)this).SetStyle((ControlStyles)206866, true);
			((Control)this).SetStyle((ControlStyles)513, false);
			break;
		case ControlType.Select:
			((Control)this).SetStyle((ControlStyles)207379, true);
			break;
		case ControlType.Button:
			((Control)this).SetStyle((ControlStyles)207379, true);
			((Control)this).SetStyle((ControlStyles)4352, false);
			break;
		}
		((Control)this).UpdateStyles();
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		visible = ((Control)this).Visible;
		((Control)this).OnVisibleChanged(e);
	}

	public void Spin(Action<Spin.Config> action, Action? end = null)
	{
		Spin(new Spin.Config(), action, end);
	}

	public void Spin(string text, Action<Spin.Config> action, Action? end = null)
	{
		Spin(new Spin.Config
		{
			Text = text
		}, action, end);
	}

	public void Spin(Spin.Config config, Action<Spin.Config> action, Action? end = null)
	{
		AntdUI.Spin.open((Control)(object)this, config, action, end);
	}

	internal void IOnSizeChanged()
	{
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				IOnSizeChanged();
			});
		}
		else
		{
			((Control)this).OnSizeChanged(EventArgs.Empty);
		}
	}

	public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		if (disableDataBinding)
		{
			return;
		}
		try
		{
			foreach (Binding item in (BaseCollection)((Control)this).DataBindings)
			{
				Binding val = item;
				if (val.PropertyName == propertyName)
				{
					val.WriteValue();
					break;
				}
			}
		}
		catch (NotSupportedException)
		{
			disableDataBinding = true;
		}
	}

	public void SetCursor(bool val)
	{
		SetCursor(val ? CursorType.Hand : CursorType.Default);
	}

	public void SetCursor(CursorType cursor = CursorType.Default)
	{
		if (oldcursor != cursor)
		{
			oldcursor = cursor;
			bool window = true;
			switch (cursor)
			{
			case CursorType.Hand:
				SetCursor(HandCursor);
				break;
			case CursorType.IBeam:
				SetCursor(Cursors.IBeam);
				break;
			case CursorType.No:
				SetCursor(Cursors.No);
				break;
			case CursorType.SizeAll:
				window = false;
				SetCursor(Cursors.SizeAll);
				break;
			case CursorType.VSplit:
				window = false;
				SetCursor(Cursors.VSplit);
				break;
			default:
				SetCursor(((Control)this).DefaultCursor);
				break;
			}
			SetWindow(window);
		}
	}

	private void SetCursor(Cursor cursor)
	{
		Cursor cursor2 = cursor;
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				SetCursor(cursor2);
			});
		}
		else
		{
			((Control)this).Cursor = cursor2;
		}
	}

	private void SetWindow(bool flag)
	{
		if (setwindow != flag)
		{
			setwindow = flag;
			if (((Control)this).Parent.FindPARENT() is BaseForm baseForm)
			{
				baseForm.EnableHitTest = setwindow;
			}
		}
	}

	protected virtual void OnTouchDown(int x, int y)
	{
		oldMY = 0;
		oldX = x;
		oldY = y;
		if (Config.TouchEnabled)
		{
			taskTouch?.Dispose();
			taskTouch = null;
			mdownd = 0;
			mdown = true;
		}
	}

	protected virtual bool OnTouchMove(int x, int y)
	{
		if (mdown)
		{
			int num = oldX - x;
			int num2 = oldY - y;
			int num3 = Math.Abs(num);
			int num4 = Math.Abs(num2);
			int num5 = (int)((float)Config.TouchThreshold * Config.Dpi);
			if (mdownd > 0 || num3 > num5 || num4 > num5)
			{
				oldMY = num2;
				if (mdownd > 0)
				{
					if (mdownd == 1)
					{
						OnTouchScrollY(-num2);
					}
					else
					{
						OnTouchScrollX(-num);
					}
					oldX = x;
					oldY = y;
					return false;
				}
				if (num4 > num3)
				{
					mdownd = 1;
				}
				else
				{
					mdownd = 2;
				}
				oldX = x;
				oldY = y;
				return false;
			}
		}
		return true;
	}

	protected virtual bool OnTouchUp()
	{
		taskTouch?.Dispose();
		taskTouch = null;
		mdown = false;
		if (mdownd > 0)
		{
			if (mdownd == 1)
			{
				int num = oldMY;
				int moveYa = Math.Abs(num);
				if (moveYa > 10)
				{
					int duration = (int)Math.Ceiling((float)moveYa * 0.1f);
					int incremental = moveYa / 2;
					int interval = 20;
					if (num > 0)
					{
						taskTouch = new ITask((Control)(object)this, delegate
						{
							if (moveYa > 0 && OnTouchScrollY(-incremental))
							{
								moveYa -= duration;
								return true;
							}
							return false;
						}, interval);
					}
					else
					{
						taskTouch = new ITask((Control)(object)this, delegate
						{
							if (moveYa > 0 && OnTouchScrollY(incremental))
							{
								moveYa -= duration;
								return true;
							}
							return false;
						}, interval);
					}
				}
			}
			return false;
		}
		return true;
	}

	protected void OnTouchCancel()
	{
		taskTouch?.Dispose();
		taskTouch = null;
		mdown = false;
	}

	protected virtual bool OnTouchScrollX(int value)
	{
		return false;
	}

	protected virtual bool OnTouchScrollY(int value)
	{
		return false;
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		taskTouch?.Dispose();
		taskTouch = null;
		((Control)this).OnMouseWheel(e);
	}

	protected override void WndProc(ref Message m)
	{
		if (Config.TouchClickEnabled)
		{
			switch (((Message)(ref m)).Msg)
			{
			case 582:
				User32.PostMessage(((Message)(ref m)).HWnd, 513u, ((Message)(ref m)).WParam, ((Message)(ref m)).LParam);
				break;
			case 583:
				User32.PostMessage(((Message)(ref m)).HWnd, 514u, ((Message)(ref m)).WParam, ((Message)(ref m)).LParam);
				break;
			default:
				((Control)this).WndProc(ref m);
				break;
			}
		}
		else
		{
			((Control)this).WndProc(ref m);
		}
	}

	protected virtual void OnDragEnter()
	{
	}

	protected virtual void OnDragLeave()
	{
	}

	public void UseAdmin()
	{
		if (fileDrop == null)
		{
			fileDrop = new FileDropHandler((Control)(object)this);
		}
	}

	protected override void OnDragEnter(DragEventArgs e)
	{
		((Control)this).OnDragEnter(e);
		if (this.DragChanged != null && ((Control)this).AllowDrop)
		{
			OnDragEnter();
			if (DragState(e.Data))
			{
				e.Effect = (DragDropEffects)(-2147483645);
			}
			else
			{
				e.Effect = (DragDropEffects)0;
			}
		}
	}

	protected override void OnDragLeave(EventArgs e)
	{
		((Control)this).OnDragLeave(e);
		OnDragLeave();
	}

	protected override void OnDragDrop(DragEventArgs e)
	{
		((Control)this).OnDragDrop(e);
		if (this.DragChanged == null)
		{
			return;
		}
		if (DragData(e.Data, out string[] files))
		{
			if (ONDRAG == null)
			{
				this.DragChanged(this, new StringsEventArgs(files));
			}
			else
			{
				string[] array = ONDRAG(files);
				if (array != null)
				{
					this.DragChanged(this, new StringsEventArgs(array));
				}
			}
		}
		OnDragLeave();
	}

	private bool DragState(IDataObject? Data)
	{
		if (DragData(Data, out string[] files))
		{
			if (ONDRAG == null)
			{
				return true;
			}
			if (ONDRAG(files) == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool DragData(IDataObject? Data, out string[] files)
	{
		if (Data == null)
		{
			files = new string[0];
			return false;
		}
		string[] formats = Data.GetFormats();
		foreach (string text in formats)
		{
			if (!(Data.GetData(text) is string[] array) || array.Length == 0)
			{
				continue;
			}
			if (HandDragFolder)
			{
				List<string> list = new List<string>(array.Length);
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (File.Exists(text2))
					{
						list.Add(text2);
					}
					else
					{
						list.AddRange(DragDataDirTree(text2));
					}
				}
				files = list.ToArray();
			}
			else
			{
				files = array;
			}
			return true;
		}
		files = new string[0];
		return false;
	}

	private List<string> DragDataDirTree(string dir)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(dir);
		FileInfo[] files = directoryInfo.GetFiles();
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		List<string> list = new List<string>(files.Length + directories.Length);
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			list.Add(fileInfo.FullName);
		}
		DirectoryInfo[] array2 = directories;
		foreach (DirectoryInfo directoryInfo2 in array2)
		{
			list.AddRange(DragDataDirTree(directoryInfo2.FullName));
		}
		return list;
	}

	internal void OnDragChanged(string[] files)
	{
		this.DragChanged?.Invoke(this, new StringsEventArgs(files));
	}

	protected override void Dispose(bool disposing)
	{
		fileDrop?.Dispose();
		((Control)this).Dispose(disposing);
	}
}

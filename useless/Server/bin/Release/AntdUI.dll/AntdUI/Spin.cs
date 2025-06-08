using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Spin 加载中")]
[ToolboxItem(true)]
public class Spin : IControl
{
	public class Config
	{
		public string? Text { get; set; }

		public Color? Back { get; set; }

		public Color? Fore { get; set; }

		public Color? Color { get; set; }

		public Font? Font { get; set; }

		public int? Radius { get; set; }

		public float? Value { get; set; }
	}

	private Config config = new Config();

	private string? text;

	private SpinCore spin_core = new SpinCore();

	[Description("颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Fill
	{
		get
		{
			return config.Color;
		}
		set
		{
			config.Color = value;
		}
	}

	[Description("文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeColor
	{
		get
		{
			return config.Fore;
		}
		set
		{
			if (!(config.Fore == value))
			{
				config.Fore = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ForeColor");
			}
		}
	}

	[Description("文本")]
	[Category("外观")]
	[DefaultValue(null)]
	public override string? Text
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationText, text);
		}
		set
		{
			if (!(text == value))
			{
				text = value;
				spin_core.Clear();
				((Control)this).Invalidate();
				((Control)this).OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		spin_core.Start(this);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		spin_core.Clear();
		((Control)this).OnFontChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Rectangle rect = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		if (rect.Width != 0 && rect.Height != 0)
		{
			config.Text = ((Control)(object)this).GetLangI(LocalizationText, text);
			spin_core.Paint(e.Graphics.High(), rect, config, (Control)(object)this);
		}
	}

	protected override void Dispose(bool disposing)
	{
		spin_core.Dispose();
		base.Dispose(disposing);
	}

	public static void open(Control control, Action<Config> action, Action? end = null)
	{
		open(control, new Config(), action, end);
	}

	public static void open(Control control, string text, Action<Config> action, Action? end = null)
	{
		open(control, new Config
		{
			Text = text
		}, action, end);
	}

	public static void open(Control control, Config config, Action<Config> action, Action? end = null)
	{
		Control control2 = control;
		Config config2 = config;
		Action<Config> action2 = action;
		Action end2 = end;
		Form parent = control2.FindPARENT();
		if (parent is LayeredFormModal layeredFormModal)
		{
			((Form)layeredFormModal).Load += delegate
			{
				control2.BeginInvoke((Delegate)(Action)delegate
				{
					open_core(control2, parent, config2, action2, end2);
				});
			};
		}
		else if (parent is LayeredFormDrawer { LoadEnd: not false } layeredFormDrawer)
		{
			layeredFormDrawer.LoadOK = delegate
			{
				control2.BeginInvoke((Delegate)(Action)delegate
				{
					open_core(control2, parent, config2, action2, end2);
				});
			};
		}
		else if (control2.InvokeRequired)
		{
			control2.BeginInvoke((Delegate)(Action)delegate
			{
				open_core(control2, parent, config2, action2, end2);
			});
		}
		else
		{
			open_core(control2, parent, config2, action2, end2);
		}
	}

	private static void open_core(Control control, Form? parent, Config config, Action<Config> action, Action? end = null)
	{
		Action<Config> action2 = action;
		Config config2 = config;
		SpinForm frm = new SpinForm(control, parent, config2);
		((Form)frm).Show((IWin32Window)(object)control);
		ITask.Run(delegate
		{
			try
			{
				action2(config2);
			}
			catch
			{
			}
			((Control)frm).Invoke((Delegate)(Action)delegate
			{
				((Component)(object)frm).Dispose();
			});
		}, end);
	}
}

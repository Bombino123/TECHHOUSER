using System;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public static class Popover
{
	public class Config
	{
		public Control Control { get; set; }

		public object? Offset { get; set; }

		public string? Title { get; set; }

		public object Content { get; set; }

		public Font? Font { get; set; }

		public Action? OnControlLoad { get; set; }

		public int AutoClose { get; set; }

		public int Radius { get; set; } = 6;


		public float ArrowSize { get; set; } = 8f;


		public TAlign ArrowAlign { get; set; } = TAlign.Bottom;


		public object? Tag { get; set; }

		public Rectangle? CustomPoint { get; set; }

		public Config(Control control, string title, string content)
		{
			Control = control;
			Title = title;
			Content = content;
		}

		public Config(Control control, string content)
		{
			Control = control;
			Content = content;
		}

		public Config(Control control, string title, object content)
		{
			Control = control;
			Title = title;
			Content = content;
		}

		public Config(Control control, object content)
		{
			Control = control;
			Content = content;
		}
	}

	public class TextRow
	{
		public string Text { get; set; }

		public int Gap { get; set; }

		public Color? Fore { get; set; }

		public Font? Font { get; set; }

		public Action? Call { get; set; }

		public TextRow(string text)
		{
			Text = text;
		}

		public TextRow(string text, int gap)
		{
			Text = text;
			Gap = gap;
		}

		public TextRow(string text, int gap, Color fore)
		{
			Text = text;
			Gap = gap;
			Fore = fore;
		}

		public TextRow(string text, Color fore)
		{
			Text = text;
			Fore = fore;
		}
	}

	public static Form? open(Control control, string title, string content, TAlign ArrowAlign = TAlign.Bottom)
	{
		return new Config(control, title, content)
		{
			ArrowAlign = ArrowAlign
		}.open();
	}

	public static Form? open(Control control, string content, TAlign ArrowAlign = TAlign.Bottom)
	{
		return new Config(control, content)
		{
			ArrowAlign = ArrowAlign
		}.open();
	}

	public static Form? open(Control control, string title, object content, TAlign ArrowAlign = TAlign.Bottom)
	{
		return new Config(control, title, content)
		{
			ArrowAlign = ArrowAlign
		}.open();
	}

	public static Form? open(Control control, object content, TAlign ArrowAlign = TAlign.Bottom)
	{
		return new Config(control, content)
		{
			ArrowAlign = ArrowAlign
		}.open();
	}

	public static Form? open(this Config config)
	{
		Config config2 = config;
		if (config2.Control.IsHandleCreated)
		{
			if (config2.Control.InvokeRequired)
			{
				Form form = null;
				config2.Control.Invoke((Delegate)(Action)delegate
				{
					form = config2.open();
				});
				return form;
			}
			LayeredFormPopover layeredFormPopover = new LayeredFormPopover(config2);
			((Form)layeredFormPopover).Show((IWin32Window)(object)config2.Control);
			return (Form?)(object)layeredFormPopover;
		}
		return null;
	}
}

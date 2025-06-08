using System;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public static class ContextMenuStrip
{
	public class Config
	{
		public Control Control { get; set; }

		public IContextMenuStripItem[] Items { get; set; }

		public Font? Font { get; set; }

		public int Radius { get; set; } = 6;


		public bool TopMost { get; set; }

		public int CallSleep { get; set; }

		public bool UFocus { get; set; }

		public Point? Location { get; set; }

		public TAlign Align { get; set; } = TAlign.BR;


		public Action<ContextMenuStripItem> Call { get; set; }

		public Config(Control control, Action<ContextMenuStripItem> call, IContextMenuStripItem[] items, int sleep = 0)
		{
			Control = control;
			Call = call;
			Items = items;
			CallSleep = sleep;
		}
	}

	public static Form? open(Control control, Action<ContextMenuStripItem> call, IContextMenuStripItem[] items, int sleep = 0)
	{
		return new Config(control, call, items, sleep).open();
	}

	public static Form? open(Control control, NotifyIcon notifyIcon, Action<ContextMenuStripItem> call, IContextMenuStripItem[] items, int sleep = 0)
	{
		return new Config(control, call, items, sleep)
		{
			TopMost = true,
			Align = TAlign.TL
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
			LayeredFormContextMenuStrip layeredFormContextMenuStrip = new LayeredFormContextMenuStrip(config2);
			((Form)layeredFormContextMenuStrip).Show((IWin32Window)(object)config2.Control);
			return (Form?)(object)layeredFormContextMenuStrip;
		}
		return null;
	}
}

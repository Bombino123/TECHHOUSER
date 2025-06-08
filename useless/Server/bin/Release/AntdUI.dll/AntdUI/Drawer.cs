using System;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

public static class Drawer
{
	public class Config
	{
		public Form Form { get; set; }

		public Control Content { get; set; }

		public bool Mask { get; set; } = true;


		public bool MaskClosable { get; set; } = true;


		public int Padding { get; set; } = 24;


		public TAlignMini Align { get; set; } = TAlignMini.Right;


		public bool Dispose { get; set; } = true;


		public object? Tag { get; set; }

		public Action? OnLoad { get; set; }

		public Action? OnClose { get; set; }

		public int DisplayDelay { get; set; } = 100;


		public Config(Form form, Control content)
		{
			Form = form;
			Content = content;
		}
	}

	public static Form? open(Form form, Control content, TAlignMini Align = TAlignMini.Right)
	{
		return new Config(form, content)
		{
			Align = Align
		}.open();
	}

	public static Config config(Form form, Control content, TAlignMini Align = TAlignMini.Right)
	{
		return new Config(form, content)
		{
			Align = Align
		};
	}

	public static Form? open(this Config config)
	{
		Config config2 = config;
		if (((Control)config2.Form).IsHandleCreated)
		{
			if (((Control)config2.Form).InvokeRequired)
			{
				Form form = null;
				((Control)config2.Form).Invoke((Delegate)(Action)delegate
				{
					form = config2.open();
				});
				return form;
			}
			if (config2.Mask)
			{
				LayeredFormMask mask = new LayeredFormMask(config2.Form);
				((Form)mask).Show((IWin32Window)(object)config2.Form);
				LayeredFormDrawer frm = new LayeredFormDrawer(config2, mask);
				ITask.Run(delegate
				{
					if (config2.DisplayDelay > 0)
					{
						Thread.Sleep(config2.DisplayDelay);
					}
					if (!frm.isclose)
					{
						((Control)config2.Form).BeginInvoke((Delegate)(Action)delegate
						{
							((Form)frm).Show((IWin32Window)(object)mask);
						});
					}
				});
				return (Form?)(object)frm;
			}
			LayeredFormDrawer layeredFormDrawer = new LayeredFormDrawer(config2);
			((Form)layeredFormDrawer).Show((IWin32Window)(object)config2.Form);
			return (Form?)(object)layeredFormDrawer;
		}
		return null;
	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public static class Preview
{
	public class Config
	{
		public Form Form { get; set; }

		public object Content { get; set; }

		public int ContentCount { get; set; }

		public object? Tag { get; set; }

		public Btn[]? Btns { get; set; }

		public Action<string, BtnEvent>? OnBtns { get; set; }

		public Config(Form form, Image bmp)
			: this(form, (IList<Image>)(object)new Image[1] { bmp })
		{
		}

		public Config(Form form, IList<Image> bmps)
		{
			Form = form;
			Content = bmps;
			ContentCount = bmps.Count;
		}

		public Config(Form form, IList<object> list, Func<int, object, Image?> call)
		{
			Form = form;
			Content = new object[2] { list, call };
			ContentCount = list.Count;
		}

		public Config(Form form, IList<object> list, Func<int, object, Action<float, string?>, Image?> call)
		{
			Form = form;
			Content = new object[2] { list, call };
			ContentCount = list.Count;
		}
	}

	public class BtnEvent
	{
		public int Index { get; set; }

		public object? Data { get; set; }

		public object? Tag { get; set; }

		public BtnEvent(int index, object? data, object? tag)
		{
			Index = index;
			Data = data;
			Tag = tag;
		}
	}

	public class Btn
	{
		public string Name { get; set; }

		public string IconSvg { get; set; }

		public object? Tag { get; set; }

		public Btn(string name, string svg)
		{
			Name = name;
			IconSvg = svg;
		}
	}

	public static Form? open(this Config config)
	{
		Config config2 = config;
		if (((Control)config2.Form).IsHandleCreated)
		{
			if (((Control)config2.Form).InvokeRequired)
			{
				Form frm2 = null;
				((Control)config2.Form).Invoke((Delegate)(Action)delegate
				{
					frm2 = config2.open();
				});
				return frm2;
			}
			LayeredFormPreview layeredFormPreview = new LayeredFormPreview(config2);
			((Form)layeredFormPreview).Show((IWin32Window)(object)config2.Form);
			return (Form?)(object)layeredFormPreview;
		}
		return null;
	}
}

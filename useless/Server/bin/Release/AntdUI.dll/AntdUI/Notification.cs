using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public static class Notification
{
	public class Config
	{
		private string? title;

		private string? text;

		public string? ID { get; set; }

		public Form Form { get; set; }

		public string? Title
		{
			get
			{
				return Localization.GetLangI(LocalizationTitle, title, new string[2] { "{id}", ID });
			}
			set
			{
				title = value;
			}
		}

		public string? LocalizationTitle { get; set; }

		public Font? FontTitle { get; set; }

		public FontStyle? FontStyleTitle { get; set; }

		public string? Text
		{
			get
			{
				return Localization.GetLangI(LocalizationText, text, new string[2] { "{id}", ID });
			}
			set
			{
				text = value;
			}
		}

		public string? LocalizationText { get; set; }

		public TType Icon { get; set; }

		public Font? Font { get; set; }

		public TAlignFrom Align { get; set; }

		public int Radius { get; set; } = 10;


		public int AutoClose { get; set; } = 6;


		public bool ClickClose { get; set; } = true;


		public bool CloseIcon { get; set; } = true;


		public bool TopMost { get; set; }

		public ConfigLink? Link { get; set; }

		public Action? OnClose { get; set; }

		public Size Padding { get; set; } = new Size(24, 20);


		public bool ShowInWindow { get; set; }

		public Config(Form _form, string _title, string _text, TType _icon, TAlignFrom _align)
		{
			Form = _form;
			Title = _title;
			Text = _text;
			Align = _align;
			Icon = _icon;
		}

		public Config(Form _form, string _title, string _text, TType _icon, TAlignFrom _align, Font? _font)
		{
			Form = _form;
			Font = _font;
			Title = _title;
			Text = _text;
			Align = _align;
			Icon = _icon;
		}

		public Config(Form _form, string _title, string _text, TType _icon, TAlignFrom _align, Font? _font, int? autoClose)
		{
			Form = _form;
			Font = _font;
			Title = _title;
			Text = _text;
			Align = _align;
			Icon = _icon;
			if (autoClose.HasValue)
			{
				AutoClose = autoClose.Value;
			}
		}
	}

	public class ConfigLink
	{
		public string Text { get; set; }

		public Func<bool> Call { get; set; }

		public ConfigLink(string text, Func<bool> call)
		{
			Text = text;
			Call = call;
		}
	}

	public static void success(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.Success, align, font, autoClose).open();
	}

	public static void info(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.Info, align, font, autoClose).open();
	}

	public static void warn(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.Warn, align, font, autoClose).open();
	}

	public static void error(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.Error, align, font, autoClose).open();
	}

	public static void open(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.None, align, font, autoClose).open();
	}

	public static void open(this Config config)
	{
		MsgQueue.Add(config);
	}

	public static void close_all()
	{
		List<NotificationFrm> list = new List<NotificationFrm>(10);
		foreach (KeyValuePair<string, List<ILayeredFormAnimate>> item2 in ILayeredFormAnimate.list)
		{
			foreach (ILayeredFormAnimate item3 in item2.Value)
			{
				if (item3 is NotificationFrm item)
				{
					list.Add(item);
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (NotificationFrm item4 in list)
		{
			item4.CloseMe();
		}
	}

	public static void close_id(string id)
	{
		if (ILayeredFormAnimate.list.Count <= 0 && MsgQueue.queue.Count <= 0)
		{
			return;
		}
		bool flag = true;
		List<NotificationFrm> list = new List<NotificationFrm>();
		foreach (KeyValuePair<string, List<ILayeredFormAnimate>> item in ILayeredFormAnimate.list)
		{
			foreach (ILayeredFormAnimate item2 in item.Value)
			{
				if (item2 is NotificationFrm notificationFrm && notificationFrm.config.ID == id)
				{
					list.Add(notificationFrm);
					flag = false;
				}
			}
		}
		if (flag)
		{
			MsgQueue.volley.Add("N" + id);
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (NotificationFrm item3 in list)
		{
			item3.CloseMe();
		}
	}
}

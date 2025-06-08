using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public static class Message
{
	public class Config
	{
		private string? text;

		internal Action? refresh;

		public string? ID { get; set; }

		public Form Form { get; set; }

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

		public Action<Config>? Call { get; set; }

		public Font? Font { get; set; }

		public int Radius { get; set; } = 6;


		public int AutoClose { get; set; } = 6;


		public bool ClickClose { get; set; } = true;


		public TAlignFrom Align { get; set; } = TAlignFrom.Top;


		public Size Padding { get; set; } = new Size(12, 9);


		public bool ShowInWindow { get; set; }

		public Config(Form _form, string _text, TType _icon)
		{
			Form = _form;
			Text = _text;
			Icon = _icon;
		}

		public Config(Form _form, string _text, TType _icon, Font? _font)
		{
			Form = _form;
			Font = _font;
			Text = _text;
			Icon = _icon;
		}

		public Config(Form _form, string _text, TType _icon, Font? _font, int? autoClose)
		{
			Form = _form;
			Font = _font;
			Text = _text;
			Icon = _icon;
			if (autoClose.HasValue)
			{
				AutoClose = autoClose.Value;
			}
		}

		public void OK(string text)
		{
			Icon = TType.Success;
			Text = text;
			Refresh();
		}

		public void Error(string text)
		{
			Icon = TType.Error;
			Text = text;
			Refresh();
		}

		public void Warn(string text)
		{
			Icon = TType.Warn;
			Text = text;
			Refresh();
		}

		public void Info(string text)
		{
			Icon = TType.Info;
			Text = text;
			Refresh();
		}

		public void Refresh()
		{
			refresh?.Invoke();
		}
	}

	public static void success(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.Success, font, autoClose).open();
	}

	public static void info(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.Info, font, autoClose).open();
	}

	public static void warn(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.Warn, font, autoClose).open();
	}

	public static void error(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.Error, font, autoClose).open();
	}

	public static void loading(Form form, string text, Action<Config> call, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.None, font, autoClose)
		{
			Call = call
		}.open();
	}

	public static void open(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.None, font, autoClose).open();
	}

	public static void open(this Config config)
	{
		MsgQueue.Add(config);
	}

	public static void close_all()
	{
		List<MessageFrm> list = new List<MessageFrm>(10);
		foreach (KeyValuePair<string, List<ILayeredFormAnimate>> item2 in ILayeredFormAnimate.list)
		{
			foreach (ILayeredFormAnimate item3 in item2.Value)
			{
				if (item3 is MessageFrm item)
				{
					list.Add(item);
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (MessageFrm item4 in list)
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
		List<MessageFrm> list = new List<MessageFrm>();
		foreach (KeyValuePair<string, List<ILayeredFormAnimate>> item in ILayeredFormAnimate.list)
		{
			foreach (ILayeredFormAnimate item2 in item.Value)
			{
				if (item2 is MessageFrm messageFrm && messageFrm.config.ID == id)
				{
					list.Add(messageFrm);
					flag = false;
				}
			}
		}
		if (flag)
		{
			MsgQueue.volley.Add("M" + id);
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (MessageFrm item3 in list)
		{
			item3.CloseMe();
		}
	}
}

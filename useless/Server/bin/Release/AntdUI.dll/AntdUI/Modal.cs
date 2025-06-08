using System;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public static class Modal
{
	public class Config
	{
		private string? canceltext = Localization.Get("Cancel", "取消");

		private string oktext = Localization.Get("OK", "确定");

		internal LayeredFormModal? Layered;

		public Form? Form { get; set; }

		public string? Title { get; set; }

		public object Content { get; set; }

		public int Width { get; set; } = 416;


		public Font? Font { get; set; }

		public bool Keyboard { get; set; } = true;


		public bool Mask { get; set; } = true;


		public bool MaskClosable { get; set; } = true;


		public bool CloseIcon { get; set; }

		public Font? CancelFont { get; set; }

		public Font? OkFont { get; set; }

		public int BtnHeight { get; set; } = 38;


		public Size Padding { get; set; } = new Size(24, 20);


		public string? CancelText
		{
			get
			{
				return canceltext;
			}
			set
			{
				if (!(canceltext == value))
				{
					canceltext = value;
					Layered?.SetCancelText(value);
				}
			}
		}

		public string OkText
		{
			get
			{
				return oktext;
			}
			set
			{
				if (!(oktext == value))
				{
					oktext = value;
					Layered?.SetOkText(value);
				}
			}
		}

		public TTypeMini OkType { get; set; } = TTypeMini.Primary;


		public TType Icon { get; set; }

		public Func<Config, bool>? OnOk { get; set; }

		public object? Tag { get; set; }

		public bool LoadingDisableCancel { get; set; }

		public bool Draggable { get; set; } = true;


		public Btn[]? Btns { get; set; }

		public Func<Button, bool>? OnBtns { get; set; }

		public Action<string, Button>? OnButtonStyle { get; set; }

		public Config(Form form, string title, string content)
		{
			Form = form;
			Title = title;
			Content = content;
		}

		public Config(Form form, Control content)
		{
			Form = form;
			Content = content;
			Padding = new Size(0, 0);
		}

		public Config(string title, string content)
		{
			Mask = (MaskClosable = false);
			Title = title;
			Content = content;
		}

		public Config(Form form, string title, object content)
		{
			Form = form;
			Title = title;
			Content = content;
		}

		public Config(string title, object content)
		{
			Mask = (MaskClosable = false);
			Title = title;
			Content = content;
		}

		public Config(Form form, string title, string content, TType icon)
		{
			Form = form;
			Title = title;
			Content = content;
			Icon = icon;
		}

		public Config(string title, string content, TType icon)
		{
			Mask = (MaskClosable = false);
			Title = title;
			Content = content;
			Icon = icon;
		}

		public Config(Form form, string title, object content, TType icon)
		{
			Form = form;
			Title = title;
			Content = content;
			Icon = icon;
		}

		public Config(string title, object content, TType icon)
		{
			Mask = (MaskClosable = false);
			Title = title;
			Content = content;
			Icon = icon;
		}

		public void Close()
		{
			if (Layered == null)
			{
				return;
			}
			((Control)Layered).BeginInvoke((Delegate)(Action)delegate
			{
				LayeredFormModal? layered = Layered;
				if (layered != null)
				{
					((Form)layered).Close();
				}
			});
		}
	}

	public class Btn
	{
		public string Name { get; set; }

		public string Text { get; set; }

		public TTypeMini Type { get; set; }

		public Color? Fore { get; set; }

		public Color? Back { get; set; }

		public object? Tag { get; set; }

		public Btn(string name, string text, TTypeMini type = TTypeMini.Default)
		{
			Name = name;
			Text = text;
			Type = type;
		}

		public Btn(string name, string text, Color fore, Color back, TTypeMini type = TTypeMini.Default)
		{
			Name = name;
			Text = text;
			Fore = fore;
			Back = back;
			Type = type;
		}
	}

	public class TextLine
	{
		public string Text { get; set; }

		public int Gap { get; set; }

		public Color? Fore { get; set; }

		public Font? Font { get; set; }

		public TextLine(string text)
		{
			Text = text;
		}

		public TextLine(string text, int gap)
		{
			Text = text;
			Gap = gap;
		}

		public TextLine(string text, int gap, Color fore)
		{
			Text = text;
			Gap = gap;
			Fore = fore;
		}

		public TextLine(string text, Color fore)
		{
			Text = text;
			Fore = fore;
		}
	}

	public static DialogResult open(Form form, string title, string content)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return new Config(form, title, content).open();
	}

	public static DialogResult open(Form form, string title, object content)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return new Config(form, title, content).open();
	}

	public static DialogResult open(Form form, Control content)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new Config(form, content).open();
	}

	public static DialogResult open(Form form, string title, string content, TType icon)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return new Config(form, title, content, icon).open();
	}

	public static DialogResult open(this Config config)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		Config config2 = config;
		if (config2.Form == null)
		{
			Config obj = config2;
			bool mask = (config2.MaskClosable = false);
			obj.Mask = mask;
			return ((Form)new LayeredFormModal(config2)).ShowDialog((IWin32Window)(object)config2.Form);
		}
		if (!((Control)config2.Form).IsHandleCreated)
		{
			Config obj2 = config2;
			bool mask = (config2.MaskClosable = false);
			obj2.Mask = mask;
		}
		if (((Control)config2.Form).InvokeRequired)
		{
			DialogResult dialog = (DialogResult)0;
			((Control)config2.Form).Invoke((Delegate)(Action)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				dialog = config2.open();
			});
			return dialog;
		}
		LayeredFormModal layeredFormModal = new LayeredFormModal(config2);
		if (config2.Mask)
		{
			return ((Form)layeredFormModal).ShowDialog((IWin32Window)(object)config2.Form.FormMask((Form)(object)layeredFormModal));
		}
		return ((Form)layeredFormModal).ShowDialog((IWin32Window)(object)config2.Form);
	}

	public static Config config(Form form, string title, string content)
	{
		return new Config(form, title, content);
	}

	public static Config config(string title, string content)
	{
		return new Config(title, content);
	}

	public static Config config(Form form, string title, object content)
	{
		return new Config(form, title, content);
	}

	public static Config config(string title, object content)
	{
		return new Config(title, content);
	}

	public static Config config(Form form, string title, string content, TType icon)
	{
		return new Config(form, title, content, icon);
	}

	public static Config config(string title, string content, TType icon)
	{
		return new Config(title, content, icon);
	}

	public static Config config(Form form, string title, object content, TType icon)
	{
		return new Config(form, title, content, icon);
	}

	public static Config config(string title, object content, TType icon)
	{
		return new Config(title, content, icon);
	}
}

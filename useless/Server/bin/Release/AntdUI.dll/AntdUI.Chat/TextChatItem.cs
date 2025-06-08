using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI.Chat;

public class TextChatItem : IChatItem
{
	private Image? _icon;

	private string? _name;

	private string _text;

	private ITask? task;

	internal bool showlinedot;

	private bool loading;

	internal bool HasEmoji;

	internal CacheFont[] cache_font = new CacheFont[0];

	internal int selectionStart;

	internal int selectionStartTemp;

	internal int selectionLength;

	[Description("ID")]
	[Category("数据")]
	[DefaultValue(null)]
	public string? ID { get; set; }

	[Description("本人")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool Me { get; set; }

	[Description("图标")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Icon
	{
		get
		{
			return _icon;
		}
		set
		{
			if (_icon != value)
			{
				_icon = value;
				Invalidates();
			}
		}
	}

	[Description("名称")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (!(_name == value))
			{
				_name = value;
				Invalidate();
			}
		}
	}

	[Description("文本")]
	[Category("外观")]
	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (!(_text == value))
			{
				_text = value;
				Invalidates();
			}
		}
	}

	public bool Loading
	{
		get
		{
			return loading;
		}
		set
		{
			if (loading == value)
			{
				return;
			}
			loading = value;
			task?.Dispose();
			task = null;
			showlinedot = false;
			if (value && base.PARENT != null)
			{
				task = new ITask((Control)(object)base.PARENT, delegate
				{
					showlinedot = !showlinedot;
					Invalidate();
					return loading;
				}, 200);
			}
			else
			{
				Invalidate();
			}
		}
	}

	internal Rectangle rect_read { get; set; }

	internal Rectangle rect_name { get; set; }

	internal Rectangle rect_text { get; set; }

	internal Rectangle rect_icon { get; set; }

	[Browsable(false)]
	[DefaultValue(0)]
	public int SelectionStart
	{
		get
		{
			return selectionStart;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			else if (value > 0)
			{
				if (cache_font == null)
				{
					value = 0;
				}
				else if (value > cache_font.Length)
				{
					value = cache_font.Length;
				}
			}
			if (selectionStart != value)
			{
				selectionStart = (selectionStartTemp = value);
				Invalidate();
			}
		}
	}

	[Browsable(false)]
	[DefaultValue(0)]
	public int SelectionLength
	{
		get
		{
			return selectionLength;
		}
		set
		{
			if (selectionLength != value)
			{
				selectionLength = value;
				Invalidate();
			}
		}
	}

	public TextChatItem(string text)
	{
		_text = text;
	}

	public TextChatItem(string text, Bitmap? icon)
	{
		_text = text;
		_icon = (Image?)(object)icon;
	}

	public TextChatItem(string text, Bitmap? icon, string name)
	{
		_text = text;
		_name = name;
		_icon = (Image?)(object)icon;
	}

	internal int SetRect(Rectangle _rect, int y, Canvas g, Font font, Size msglen, int gap, int spilt, int spilt2, int image_size)
	{
		if (string.IsNullOrEmpty(_name))
		{
			base.rect = new Rectangle(_rect.X, _rect.Y + y, _rect.Width, msglen.Height);
			if (Me)
			{
				rect_icon = new Rectangle(base.rect.Right - gap - image_size, base.rect.Y, image_size, image_size);
				rect_read = new Rectangle(rect_icon.X - spilt - msglen.Width, rect_icon.Y, msglen.Width, msglen.Height);
			}
			else
			{
				rect_icon = new Rectangle(base.rect.X + gap, base.rect.Y, image_size, image_size);
				rect_read = new Rectangle(rect_icon.Right + spilt, rect_icon.Y, msglen.Width, msglen.Height);
			}
		}
		else
		{
			base.rect = new Rectangle(_rect.X, _rect.Y + y, _rect.Width, msglen.Height + gap);
			int width = g.MeasureString(_name, font).Width;
			if (Me)
			{
				rect_icon = new Rectangle(base.rect.Right - gap - image_size, base.rect.Y, image_size, image_size);
				rect_name = new Rectangle(rect_icon.X - spilt - msglen.Width + msglen.Width - width, rect_icon.Y, width, gap);
				rect_read = new Rectangle(rect_icon.X - spilt - msglen.Width, rect_name.Bottom, msglen.Width, msglen.Height);
			}
			else
			{
				rect_icon = new Rectangle(base.rect.X + gap, base.rect.Y, image_size, image_size);
				rect_name = new Rectangle(rect_icon.Right + spilt, rect_icon.Y, width, gap);
				rect_read = new Rectangle(rect_name.X, rect_name.Bottom, msglen.Width, msglen.Height);
			}
		}
		rect_text = new Rectangle(rect_read.X + spilt, rect_read.Y + spilt, msglen.Width - spilt2, msglen.Height - spilt2);
		CacheFont[] array = cache_font;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetOffset(rect_text.Location);
		}
		base.Show = true;
		return base.rect.Height;
	}

	internal bool ContainsRead(Point point, int x, int y)
	{
		return rect_text.Contains(new Point(point.X + x, point.Y + y));
	}
}

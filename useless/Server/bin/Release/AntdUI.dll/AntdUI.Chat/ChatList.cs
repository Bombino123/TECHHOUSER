using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AntdUI.Chat;

[Description("ChatList 气泡聊天列表")]
[ToolboxItem(true)]
public class ChatList : IControl
{
	private ChatItemCollection? items;

	[Browsable(false)]
	public ScrollBar ScrollBar;

	private StringFormat SFL = Helper.SF((StringAlignment)0, (StringAlignment)1);

	private TextChatItem? mouseDown;

	private Point oldMouseDown;

	private bool mouseDownMove;

	private StringFormat m_sf = Helper.SF_MEASURE_FONT();

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("数据集合")]
	[Category("数据")]
	public ChatItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new ChatItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	[Description("Emoji字体")]
	[Category("外观")]
	[DefaultValue("Segoe UI Emoji")]
	public string EmojiFont { get; set; } = "Segoe UI Emoji";


	public bool IsBottom
	{
		get
		{
			if (ScrollBar.Show)
			{
				return ScrollBar.Value == ScrollBar.VrValueI;
			}
			return true;
		}
	}

	[Description("单击时发生")]
	[Category("行为")]
	public event ClickEventHandler? ItemClick;

	public bool AddToBottom(IChatItem it, bool force = false)
	{
		if (force)
		{
			Items.Add(it);
			ToBottom();
			return true;
		}
		bool isBottom = IsBottom;
		Items.Add(it);
		if (isBottom)
		{
			ToBottom();
		}
		return isBottom;
	}

	public void ToBottom()
	{
		ScrollBar.Value = ScrollBar.VrValueI;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return;
		}
		if (items == null || items.Count == 0)
		{
			((Control)this).OnPaint(e);
			return;
		}
		Canvas canvas = e.Graphics.High();
		float num = ScrollBar.Value;
		float radius = Config.Dpi * 8f;
		canvas.TranslateTransform(0f, 0f - num);
		foreach (IChatItem item in items)
		{
			PaintItem(canvas, item, clientRectangle, num, radius);
		}
		canvas.ResetTransform();
		ScrollBar.Paint(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintItem(Canvas g, IChatItem it, Rectangle rect, float sy, float radius)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		it.show = it.Show && (float)it.rect.Y > sy - (float)rect.Height - (float)it.rect.Height && it.rect.Bottom < ScrollBar.Value + ScrollBar.ReadSize + it.rect.Height;
		if (!it.show || !(it is TextChatItem textChatItem))
		{
			return;
		}
		GraphicsPath val = textChatItem.rect_read.RoundPath(radius);
		try
		{
			SolidBrush val2 = new SolidBrush(Colour.TextTertiary.Get("ChatList"));
			try
			{
				g.String(textChatItem.Name, ((Control)this).Font, (Brush)(object)val2, textChatItem.rect_name, SFL);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			if (textChatItem.Me)
			{
				g.Fill(Color.FromArgb(0, 153, 255), val);
				if (textChatItem.selectionLength > 0)
				{
					g.Fill(Color.FromArgb(0, 134, 224), val);
				}
				SolidBrush val3 = new SolidBrush(Color.White);
				try
				{
					PaintItemText(g, textChatItem, val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			else
			{
				g.Fill(Brushes.White, val);
				if (textChatItem.selectionLength > 0)
				{
					g.Fill(Colour.FillQuaternary.Get("ChatList"), val);
				}
				SolidBrush val4 = new SolidBrush(Color.Black);
				try
				{
					PaintItemText(g, textChatItem, val4);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (textChatItem.Icon != null)
		{
			g.Image(textChatItem.rect_icon, textChatItem.Icon, TFit.Cover, 0f, round: true);
		}
	}

	private void PaintItemText(Canvas g, TextChatItem text, SolidBrush fore)
	{
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Expected O, but got Unknown
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Expected O, but got Unknown
		if (text.selectionLength > 0)
		{
			int num = text.selectionStartTemp + text.selectionLength - 1;
			if (num > text.cache_font.Length - 1)
			{
				num = text.cache_font.Length - 1;
			}
			CacheFont cacheFont = text.cache_font[text.selectionStartTemp];
			SolidBrush val = new SolidBrush(Color.FromArgb(text.Me ? 255 : 60, 96, 165, 250));
			try
			{
				for (int i = text.selectionStartTemp; i <= num; i++)
				{
					CacheFont cacheFont2 = text.cache_font[i];
					if (i == num)
					{
						g.Fill((Brush)(object)val, new Rectangle(cacheFont.rect.X, cacheFont.rect.Y, cacheFont2.rect.Right - cacheFont.rect.X, cacheFont.rect.Height));
					}
					else if (cacheFont.rect.Y != cacheFont2.rect.Y || cacheFont2.retun)
					{
						cacheFont2 = text.cache_font[i - 1];
						g.Fill((Brush)(object)val, new Rectangle(cacheFont.rect.X, cacheFont.rect.Y, cacheFont2.rect.Right - cacheFont.rect.X, cacheFont.rect.Height));
						cacheFont = text.cache_font[i];
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (text.HasEmoji)
		{
			Font val2 = new Font(EmojiFont, ((Control)this).Font.Size);
			try
			{
				CacheFont[] cache_font = text.cache_font;
				foreach (CacheFont cacheFont3 in cache_font)
				{
					switch (cacheFont3.type)
					{
					case GraphemeSplitter.STRE_TYPE.STR:
						if (cacheFont3.emoji)
						{
							g.String(cacheFont3.text, val2, (Brush)(object)fore, cacheFont3.rect, m_sf);
						}
						else
						{
							g.String(cacheFont3.text, ((Control)this).Font, (Brush)(object)fore, cacheFont3.rect, m_sf);
						}
						break;
					case GraphemeSplitter.STRE_TYPE.SVG:
					{
						Bitmap val4 = cacheFont3.text.SvgToBmp();
						try
						{
							if (val4 != null)
							{
								g.Image(cacheFont3.rect, (Image)(object)val4, TFit.Cover, 0f, round: false);
							}
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
						break;
					}
					case GraphemeSplitter.STRE_TYPE.BASE64IMG:
					{
						using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cacheFont3.text.Substring(cacheFont3.text.IndexOf(";base64,") + 8))))
						{
							Image val3 = Image.FromStream((Stream)memoryStream);
							try
							{
								g.Image(cacheFont3.rect, val3, TFit.Contain, 0f, round: false);
							}
							finally
							{
								((IDisposable)val3)?.Dispose();
							}
						}
						break;
					}
					}
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		else
		{
			CacheFont[] cache_font = text.cache_font;
			foreach (CacheFont cacheFont4 in cache_font)
			{
				switch (cacheFont4.type)
				{
				case GraphemeSplitter.STRE_TYPE.STR:
					g.String(cacheFont4.text, ((Control)this).Font, (Brush)(object)fore, cacheFont4.rect, m_sf);
					break;
				case GraphemeSplitter.STRE_TYPE.SVG:
				{
					Bitmap val6 = cacheFont4.text.SvgToBmp();
					try
					{
						if (val6 != null)
						{
							g.Image(cacheFont4.rect, (Image)(object)val6, TFit.Cover, 0f, round: false);
						}
					}
					finally
					{
						((IDisposable)val6)?.Dispose();
					}
					break;
				}
				case GraphemeSplitter.STRE_TYPE.BASE64IMG:
				{
					using (MemoryStream memoryStream2 = new MemoryStream(Convert.FromBase64String(cacheFont4.text.Substring(cacheFont4.text.IndexOf(";base64,") + 8))))
					{
						Image val5 = Image.FromStream((Stream)memoryStream2);
						try
						{
							g.Image(cacheFont4.rect, val5, TFit.Contain, 0f, round: false);
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
					break;
				}
				}
			}
		}
		if (!text.showlinedot)
		{
			return;
		}
		int num2 = (int)(2f * Config.Dpi);
		int num3 = num2 * 3;
		if (text.cache_font.Length != 0)
		{
			Rectangle rect = text.cache_font[text.cache_font.Length - 1].rect;
			SolidBrush val7 = new SolidBrush(Color.FromArgb(0, 153, 255));
			try
			{
				g.Fill((Brush)(object)val7, new Rectangle(rect.Right - num3 / 2, rect.Bottom - num2, num3, num2));
				return;
			}
			finally
			{
				((IDisposable)val7)?.Dispose();
			}
		}
		SolidBrush val8 = new SolidBrush(Color.FromArgb(0, 153, 255));
		try
		{
			g.Fill((Brush)(object)val8, new Rectangle(text.rect_read.X + (text.rect_read.Width - num3) / 2, text.rect_read.Bottom - num2, num3, num2));
		}
		finally
		{
			((IDisposable)val8)?.Dispose();
		}
	}

	public ChatList()
	{
		ScrollBar = new ScrollBar(this);
	}

	protected override void Dispose(bool disposing)
	{
		ScrollBar.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Invalid comparison between Unknown and I4
		((Control)this).OnMouseDown(e);
		if (!ScrollBar.MouseDown(e.Location) || items == null || items.Count == 0)
		{
			return;
		}
		((Control)this).Focus();
		int value = ScrollBar.Value;
		foreach (IChatItem item in Items)
		{
			if (item.show && item.Contains(e.Location, 0, value))
			{
				if (item is TextChatItem textChatItem)
				{
					textChatItem.SelectionLength = 0;
					if ((int)e.Button == 1048576 && textChatItem.ContainsRead(e.Location, 0, value))
					{
						oldMouseDown = e.Location;
						textChatItem.SelectionStart = GetCaretPostion(textChatItem, e.X, e.Y + value);
						mouseDown = textChatItem;
					}
				}
				this.ItemClick?.Invoke(this, new ChatItemEventArgs(item, e));
			}
			else if (item is TextChatItem textChatItem2)
			{
				textChatItem2.SelectionLength = 0;
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		int value = ScrollBar.Value;
		if (mouseDown != null)
		{
			mouseDownMove = true;
			SetCursor(CursorType.IBeam);
			int caretPostion = GetCaretPostion(mouseDown, oldMouseDown.X + (e.X - oldMouseDown.X), oldMouseDown.Y + value + (e.Y - oldMouseDown.Y));
			mouseDown.SelectionLength = Math.Abs(caretPostion - mouseDown.selectionStart);
			if (caretPostion > mouseDown.selectionStart)
			{
				mouseDown.selectionStartTemp = mouseDown.selectionStart;
			}
			else
			{
				mouseDown.selectionStartTemp = caretPostion;
			}
			((Control)this).Invalidate();
		}
		else
		{
			if (!ScrollBar.MouseMove(e.Location) || items == null || items.Count == 0)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (IChatItem item in Items)
			{
				if (item.show && item.Contains(e.Location, 0, value) && item is TextChatItem textChatItem && textChatItem.ContainsRead(e.Location, 0, value))
				{
					num3++;
				}
			}
			if (num3 > 0)
			{
				SetCursor(CursorType.IBeam);
			}
			else
			{
				SetCursor(num2 > 0);
			}
			if (num > 0)
			{
				((Control)this).Invalidate();
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		if (mouseDown != null && mouseDownMove)
		{
			int value = ScrollBar.Value;
			int caretPostion = GetCaretPostion(mouseDown, e.X, e.Y + value);
			if (mouseDown.selectionStart == caretPostion)
			{
				mouseDown.SelectionLength = 0;
			}
			else if (caretPostion > mouseDown.selectionStart)
			{
				mouseDown.SelectionLength = Math.Abs(caretPostion - mouseDown.selectionStart);
				mouseDown.SelectionStart = mouseDown.selectionStart;
			}
			else
			{
				mouseDown.SelectionLength = Math.Abs(caretPostion - mouseDown.selectionStart);
				mouseDown.SelectionStart = caretPostion;
			}
			((Control)this).Invalidate();
		}
		mouseDown = null;
		ScrollBar.MouseUp();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ScrollBar.Leave();
		SetCursor(val: false);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		((Control)this).OnLostFocus(e);
		ILeave();
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ILeave();
	}

	private void ILeave()
	{
		ScrollBar.Leave();
		SetCursor(val: false);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (IChatItem item in Items)
		{
			if (item is TextChatItem textChatItem)
			{
				textChatItem.SelectionLength = 0;
			}
		}
		if (num > 0)
		{
			((Control)this).Invalidate();
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		ScrollBar.MouseWheel(e.Delta);
		base.OnMouseWheel(e);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		bool result = ((Control)this).ProcessCmdKey(ref msg, keyData);
		if ((int)keyData != 131137)
		{
			if ((int)keyData == 131139)
			{
				Copy();
				return true;
			}
			return result;
		}
		SelectAll();
		return true;
	}

	private void SelectAll()
	{
		foreach (IChatItem item in Items)
		{
			if (item is TextChatItem { SelectionLength: >0 } textChatItem)
			{
				textChatItem.SelectionStart = 0;
				textChatItem.selectionLength = textChatItem.cache_font.Length;
				break;
			}
		}
	}

	private void Copy()
	{
		foreach (IChatItem item in Items)
		{
			if (item is TextChatItem { SelectionLength: >0 } textChatItem)
			{
				string selectionText = GetSelectionText(textChatItem);
				if (selectionText != null)
				{
					((Control)(object)this).ClipboardSetText(selectionText);
				}
				break;
			}
		}
	}

	private string? GetSelectionText(TextChatItem text)
	{
		if (text.cache_font == null)
		{
			return null;
		}
		if (text.selectionLength > 0)
		{
			int selectionStart = text.selectionStart;
			int selectionLength = text.selectionLength;
			int num = selectionStart + selectionLength;
			List<string> list = new List<string>(selectionLength);
			CacheFont[] cache_font = text.cache_font;
			foreach (CacheFont cacheFont in cache_font)
			{
				if (cacheFont.i >= selectionStart && num > cacheFont.i)
				{
					list.Add(cacheFont.text);
				}
			}
			return string.Join("", list);
		}
		return null;
	}

	private int GetCaretPostion(TextChatItem item, int x, int y)
	{
		CacheFont[] cache_font = item.cache_font;
		foreach (CacheFont cacheFont in cache_font)
		{
			if (cacheFont.rect.X <= x && cacheFont.rect.Right >= x && cacheFont.rect.Y <= y && cacheFont.rect.Bottom >= y)
			{
				if (x > cacheFont.rect.X + cacheFont.rect.Width / 2)
				{
					return cacheFont.i + 1;
				}
				return cacheFont.i;
			}
		}
		CacheFont cacheFont2 = FindNearestFont(x, y, item.cache_font);
		if (cacheFont2 == null)
		{
			if (x > item.cache_font[item.cache_font.Length - 1].rect.Right)
			{
				return item.cache_font.Length;
			}
			return 0;
		}
		if (x > cacheFont2.rect.X + cacheFont2.rect.Width / 2)
		{
			return cacheFont2.i + 1;
		}
		return cacheFont2.i;
	}

	private CacheFont? FindNearestFont(int x, int y, CacheFont[] cache_font)
	{
		CacheFont cacheFont = cache_font[0];
		CacheFont cacheFont2 = cache_font[^1];
		if (x < cacheFont.rect.X && y < cacheFont.rect.Y)
		{
			return cacheFont;
		}
		if (x > cacheFont2.rect.X && y > cacheFont2.rect.Y)
		{
			return cacheFont2;
		}
		double num = 2147483647.0;
		CacheFont result = null;
		foreach (CacheFont cacheFont3 in cache_font)
		{
			int num2 = Math.Abs(x - (cacheFont3.rect.Left + cacheFont3.rect.Width / 2));
			int num3 = Math.Abs(y - (cacheFont3.rect.Top + cacheFont3.rect.Height / 2));
			double num4 = new int[2] { num2, num3 }.Average();
			if (num4 < num)
			{
				num = num4;
				result = cacheFont3;
			}
		}
		return result;
	}

	protected override void OnFontChanged(EventArgs e)
	{
		Rectangle rect = ChangeList();
		ScrollBar.SizeChange(rect);
		((Control)this).OnFontChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		Rectangle rect = ChangeList();
		ScrollBar.SizeChange(rect);
		((Control)this).OnSizeChanged(e);
	}

	internal Rectangle ChangeList()
	{
		Rectangle rect = ((Control)this).ClientRectangle;
		if (items == null || items.Count == 0)
		{
			return rect;
		}
		if (rect.Width == 0 || rect.Height == 0)
		{
			return rect;
		}
		int y = 0;
		Helper.GDI(delegate(Canvas g)
		{
			int height = g.MeasureString("龍Qq", ((Control)this).Font).Height;
			int num = (int)Math.Ceiling((double)height * 1.714);
			int num2 = (int)Math.Round((double)num * 0.75);
			int num3 = num - num2;
			int num4 = num3 * 2;
			int max_width = (int)((float)rect.Width * 0.8f) - num;
			y = num3;
			foreach (IChatItem item in items)
			{
				item.PARENT = this;
				if (item is TextChatItem textChatItem)
				{
					y += textChatItem.SetRect(rect, y, g, ((Control)this).Font, FixFontWidth(g, ((Control)this).Font, textChatItem, max_width, num4), height, num3, num4, num) + num2;
				}
			}
		});
		ScrollBar.SetVrSize(y);
		return rect;
	}

	internal Size FixFontWidth(Canvas g, Font Font, TextChatItem item, int max_width, int spilt)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		TextChatItem item2 = item;
		Canvas g2 = g;
		Font Font2 = Font;
		item2.HasEmoji = false;
		int font_height = 0;
		List<CacheFont> font_widths = new List<CacheFont>(item2.Text.Length);
		GraphemeSplitter.EachT(item2.Text, 0, delegate(string str, GraphemeSplitter.STRE_TYPE type, int nStart, int nLen)
		{
			string text = str.Substring(nStart, nLen);
			switch (type)
			{
			case GraphemeSplitter.STRE_TYPE.BASE64IMG:
			{
				using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(text.Substring(text.IndexOf(";base64,") + 8))))
				{
					Image val3 = Image.FromStream((Stream)memoryStream);
					try
					{
						int num5 = val3.Width;
						int num6 = val3.Height;
						if (num5 > max_width)
						{
							float num7 = (float)max_width / (float)num5;
							num5 = max_width;
							num6 = (int)((float)num6 * num7);
						}
						font_widths.Add(new CacheFont(text, _emoji: false, num5, type)
						{
							imageHeight = num6
						});
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				break;
			}
			case GraphemeSplitter.STRE_TYPE.SVG:
			{
				Bitmap val2 = text.SvgToBmp(font_height, font_height, null);
				try
				{
					if (val2 != null)
					{
						int width = ((Image)val2).Width;
						int height = ((Image)val2).Height;
						font_widths.Add(new CacheFont(text, _emoji: false, width, type)
						{
							imageHeight = height
						});
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				break;
			}
			default:
			{
				UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(text[0]);
				if (IsEmoji(unicodeCategory))
				{
					item2.HasEmoji = true;
					font_widths.Add(new CacheFont(text, _emoji: true, 0, type));
				}
				else
				{
					switch (text)
					{
					case "\t":
					case "\n":
					case "\r\n":
					{
						Size size3 = g2.MeasureString(" ", Font2, 10000, m_sf);
						if (font_height < size3.Height)
						{
							font_height = size3.Height;
						}
						font_widths.Add(new CacheFont(text, _emoji: false, (int)Math.Ceiling((float)size3.Width * 8f), type));
						break;
					}
					default:
					{
						Size size2 = g2.MeasureString(text, Font2, 10000, m_sf);
						if (font_height < size2.Height)
						{
							font_height = size2.Height;
						}
						font_widths.Add(new CacheFont(text, _emoji: false, size2.Width, type));
						break;
					}
					}
				}
				break;
			}
			}
			return true;
		});
		if (item2.HasEmoji)
		{
			Font val = new Font(EmojiFont, Font2.Size);
			try
			{
				foreach (CacheFont item3 in font_widths)
				{
					if (item3.emoji)
					{
						Size size = g2.MeasureString(item3.text, val, 10000, m_sf);
						if (font_height < size.Height)
						{
							font_height = size.Height;
						}
						item3.width = size.Width;
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		for (int i = 0; i < font_widths.Count; i++)
		{
			font_widths[i].i = i;
		}
		item2.cache_font = font_widths.ToArray();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		CacheFont[] cache_font = item2.cache_font;
		foreach (CacheFont cacheFont in cache_font)
		{
			if (cacheFont.text == "\r")
			{
				cacheFont.retun = true;
				continue;
			}
			if (cacheFont.text == "\n" || cacheFont.text == "\r\n")
			{
				cacheFont.retun = true;
				num2 += font_height;
				num = 0;
				continue;
			}
			if (num + cacheFont.width > max_width)
			{
				num2 += font_height;
				num = 0;
			}
			if (cacheFont.imageHeight.HasValue)
			{
				cacheFont.rect = new Rectangle(num, num2, cacheFont.width, cacheFont.imageHeight.Value);
			}
			else
			{
				cacheFont.rect = new Rectangle(num, num2, cacheFont.width, font_height);
			}
			if (num3 < cacheFont.rect.Right)
			{
				num3 = cacheFont.rect.Right;
			}
			if (num4 < cacheFont.rect.Bottom)
			{
				num4 = cacheFont.rect.Bottom;
			}
			num += cacheFont.width;
		}
		return new Size(num3 + spilt, num4 + spilt);
	}

	private bool IsEmoji(UnicodeCategory unicodeInfo)
	{
		if (unicodeInfo != UnicodeCategory.Surrogate && unicodeInfo != UnicodeCategory.OtherSymbol && unicodeInfo != UnicodeCategory.MathSymbol && unicodeInfo != UnicodeCategory.EnclosingMark && unicodeInfo != UnicodeCategory.NonSpacingMark)
		{
			return unicodeInfo == UnicodeCategory.ModifierLetter;
		}
		return true;
	}
}

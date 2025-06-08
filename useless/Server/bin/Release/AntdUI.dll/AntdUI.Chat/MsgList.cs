using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI.Chat;

[Description("MsgList 好友消息列表")]
[ToolboxItem(true)]
public class MsgList : IControl
{
	private MsgItemCollection? items;

	[Browsable(false)]
	public ScrollBar ScrollBar;

	private StringFormat SFBage = Helper.SF((StringAlignment)1, (StringAlignment)1);

	private StringFormat SFL = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private StringFormat SFR = Helper.SF_ALL((StringAlignment)1, (StringAlignment)2);

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("数据集合")]
	[Category("数据")]
	public MsgItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new MsgItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	public event ItemSelectedEventHandler? ItemSelected;

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
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
		int value = ScrollBar.Value;
		canvas.TranslateTransform(0f, -value);
		Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 0.9f);
		try
		{
			Font val2 = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 0.82f);
			try
			{
				foreach (MsgItem item in items)
				{
					PaintItem(canvas, item, clientRectangle, value, val, val2);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		canvas.ResetTransform();
		ScrollBar.Paint(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintItem(Canvas g, MsgItem it, Rectangle rect, float sy, Font font_text, Font font_time)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		it.show = it.Show && it.Visible && (float)it.rect.Y > sy - (float)rect.Height && it.rect.Bottom < ScrollBar.Value + ScrollBar.ReadSize + it.rect.Height;
		if (!it.show)
		{
			return;
		}
		if (it.Select)
		{
			SolidBrush val = new SolidBrush(Color.FromArgb(0, 153, 255));
			try
			{
				g.Fill((Brush)(object)val, it.rect);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			SolidBrush val2 = new SolidBrush(Color.White);
			try
			{
				g.String(it.Name, ((Control)this).Font, (Brush)(object)val2, it.rect_name, SFL);
				g.String(it.Text, font_text, (Brush)(object)val2, it.rect_text, SFL);
				g.String(it.Time, font_time, (Brush)(object)val2, it.rect_time, SFR);
			}
			catch
			{
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		else
		{
			if (it.Hover)
			{
				SolidBrush val3 = new SolidBrush(Colour.FillTertiary.Get("MsgList"));
				try
				{
					g.Fill((Brush)(object)val3, it.rect);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			SolidBrush val4 = new SolidBrush(((Control)this).ForeColor);
			try
			{
				g.String(it.Name, ((Control)this).Font, (Brush)(object)val4, it.rect_name, SFL);
				g.String(it.Text, font_text, (Brush)(object)val4, it.rect_text, SFL);
				g.String(it.Time, font_time, (Brush)(object)val4, it.rect_time, SFR);
			}
			catch
			{
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
		if (it.Icon == null)
		{
			return;
		}
		g.Image(it.rect_icon, it.Icon, TFit.Cover, 0f, round: true);
		if (it.Count <= 0)
		{
			return;
		}
		if (it.Count > 99)
		{
			Size size = g.MeasureString("99+", font_time);
			Rectangle rect2 = new Rectangle(it.rect_icon.Right - size.Width + size.Width / 3, it.rect_icon.Y - size.Height / 3, size.Width, size.Height);
			GraphicsPath val5 = rect2.RoundPath(6f * Config.Dpi);
			try
			{
				g.Fill(Brushes.Red, val5);
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
			g.String("99+", font_time, Brushes.White, rect2, SFBage);
		}
		else if (it.Count > 1)
		{
			Size size2 = g.MeasureString(it.Count.ToString(), font_time);
			int num = ((size2.Width > size2.Height) ? size2.Width : size2.Height);
			int num2 = num / 3;
			Rectangle rect3 = new Rectangle(it.rect_icon.Right - num + num2, it.rect_icon.Y - num2, num, num);
			g.FillEllipse(Brushes.Red, rect3);
			g.String(it.Count.ToString(), font_time, Brushes.White, rect3, SFBage);
		}
		else
		{
			int num3 = it.rect_time.Height / 2;
			int num4 = num3 / 3;
			Rectangle rect4 = new Rectangle(it.rect_icon.Right - num3 + num4, it.rect_icon.Y - num4, num3, num3);
			g.FillEllipse(Brushes.Red, rect4);
		}
	}

	public MsgList()
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
		((Control)this).OnMouseDown(e);
		if (!ScrollBar.MouseDown(e.Location) || items == null || items.Count == 0)
		{
			return;
		}
		foreach (MsgItem item in Items)
		{
			if (item.Visible && item.Contains(e.Location, 0, ScrollBar.Value, out var _))
			{
				item.Select = true;
				OnItemSelected(item);
				break;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		ScrollBar.MouseUp();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (ScrollBar.MouseMove(e.Location))
		{
			if (items == null || items.Count == 0)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			foreach (MsgItem item in Items)
			{
				if (item.show)
				{
					if (item.Contains(e.Location, 0, ScrollBar.Value, out var change))
					{
						num2++;
					}
					if (change)
					{
						num++;
					}
				}
			}
			SetCursor(num2 > 0);
			if (num > 0)
			{
				((Control)this).Invalidate();
			}
		}
		else
		{
			ILeave();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ScrollBar.Leave();
		ILeave();
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ScrollBar.Leave();
		ILeave();
	}

	private void ILeave()
	{
		SetCursor(val: false);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (MsgItem item in Items)
		{
			if (item.Hover)
			{
				num++;
			}
			item.Hover = false;
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

	protected virtual void OnItemSelected(MsgItem selectedItem)
	{
		this.ItemSelected?.Invoke(this, new MsgItemEventArgs(selectedItem));
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
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Expected O, but got Unknown
			int num = (int)Math.Ceiling((double)g.MeasureString("龍Qq", ((Control)this).Font).Height * 3.856);
			int num2 = (int)Math.Round((double)num * 0.212);
			int spilt = (int)Math.Round((double)num2 * 0.478);
			int gap_name = (int)Math.Round((double)num2 * 0.304);
			int gap_desc = (int)Math.Round((double)num2 * 0.217);
			int name_height = (int)Math.Round((double)num * 0.185);
			int desc_height = (int)Math.Round((double)num * 0.157);
			int image_size = num - num2 * 2;
			Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 0.82f);
			try
			{
				foreach (MsgItem item in items)
				{
					item.PARENT = this;
					int time_width = 0;
					if (!string.IsNullOrEmpty(item.Time))
					{
						time_width = g.MeasureString(item.Time, val).Width;
					}
					item.SetRect(new Rectangle(rect.X, rect.Y + y, rect.Width, num), time_width, num2, spilt, gap_name, gap_desc, image_size, name_height, desc_height);
					if (item.Visible)
					{
						y += num;
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		});
		ScrollBar.SetVrSize(y);
		return rect;
	}
}

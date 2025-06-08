using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormPreview : ILayeredFormOpacity
{
	internal class PreBtns
	{
		public string id { get; set; }

		public string svg { get; set; }

		public bool div { get; set; }

		public object? tag { get; set; }

		public Rectangle Rect { get; set; }

		public Rectangle rect { get; set; }

		public bool hover { get; set; }

		public bool enabled { get; set; } = true;


		public bool mdown { get; set; }

		public PreBtns(string _id, string _svg)
		{
			id = _id;
			svg = _svg;
		}

		public PreBtns(string _id, string _svg, object? _tag)
			: this(_id, _svg)
		{
			tag = _tag;
			div = true;
		}
	}

	private int Radius;

	private int Bor;

	private bool HasBor;

	private Form form;

	private PreBtns[] btns;

	private Preview.Config config;

	private int PageSize;

	private bool loading;

	private string? LoadingProgressStr;

	private float _value = -1f;

	private Image? Img;

	private int SelectIndex;

	private object? SelectValue;

	private Size ImgSize;

	private bool autoDpi = true;

	private PointF rect_img_oxy;

	private RectangleF rect_img_dpi;

	private float offsetX;

	private float offsetY;

	private float _dpi = 1f;

	private readonly StringFormat s_f = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private readonly Color colorDefault = Color.FromArgb(166, 255, 255, 255);

	private readonly Color colorHover = Color.FromArgb(217, 255, 255, 255);

	private Rectangle rect_read;

	private Rectangle rect_left;

	private Rectangle rect_left_icon;

	private Rectangle rect_right;

	private Rectangle rect_right_icon;

	private Rectangle rect_close;

	private Rectangle rect_close_icon;

	private Rectangle rect_panel;

	private bool hoverClose;

	private bool hoverLeft;

	private bool hoverRight;

	private bool moveImg;

	private bool moveImging;

	private Point movePos;

	private float offsetXOld;

	private float offsetYOld;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Loading
	{
		get
		{
			return loading;
		}
		set
		{
			if (loading != value)
			{
				loading = value;
				Print(fore: true);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public float LoadingProgress
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				_value = value;
				if (loading)
				{
					Print();
				}
			}
		}
	}

	private float Dpi
	{
		get
		{
			return _dpi;
		}
		set
		{
			if ((double)value < 0.06)
			{
				value = 0.06f;
			}
			else if (value > _dpi && _dpi < 1f && value > 1f)
			{
				value = 1f;
			}
			_dpi = value;
			rect_img_dpi = ScaleImg(rect_read, _dpi);
		}
	}

	private bool enabledLeft => SelectIndex > 0;

	private bool enabledRight => SelectIndex < PageSize - 1;

	public LayeredFormPreview(Preview.Config _config)
	{
		maxalpha = byte.MaxValue;
		config = _config;
		form = _config.Form;
		((Control)this).Font = ((Control)form).Font;
		((Form)this).TopMost = _config.Form.TopMost;
		HasBor = form.FormFrame(out Radius, out Bor);
		if (form is Window window)
		{
			SetSize(window.Size);
			SetLocation(window.Location);
			((Form)this).Size = window.Size;
			((Form)this).Location = window.Location;
		}
		else
		{
			SetSize(form.Size);
			SetLocation(form.Location);
			((Form)this).Size = form.Size;
			((Form)this).Location = form.Location;
		}
		PageSize = config.ContentCount;
		PreBtns[] array = new PreBtns[6]
		{
			new PreBtns("@t_flipY", SvgDb.Custom["SwapOutlined"].Insert(28, " transform=\"rotate(90),translate(0 -100%)\"")),
			new PreBtns("@t_flipX", "SwapOutlined"),
			new PreBtns("@t_rotateL", "RotateLeftOutlined"),
			new PreBtns("@t_rotateR", "RotateRightOutlined"),
			new PreBtns("@t_zoomOut", "ZoomOutOutlined"),
			new PreBtns("@t_zoomIn", "ZoomInOutlined")
		};
		if (config.Btns == null || config.Btns.Length == 0)
		{
			btns = array;
			return;
		}
		List<PreBtns> list = new List<PreBtns>(config.Btns.Length + array.Length);
		Preview.Btn[] array2 = config.Btns;
		foreach (Preview.Btn btn in array2)
		{
			list.Add(new PreBtns(btn.Name, btn.IconSvg, btn.Tag));
		}
		list.AddRange(array);
		btns = list.ToArray();
	}

	protected override void OnLoad(EventArgs e)
	{
		if (form is Window window)
		{
			SetSize(window.Size);
			SetLocation(window.Location);
			((Form)this).Size = window.Size;
			((Form)this).Location = window.Location;
		}
		else
		{
			SetSize(form.Size);
			SetLocation(form.Location);
			((Form)this).Size = form.Size;
			((Form)this).Location = form.Location;
		}
		((Control)form).LocationChanged += Form_LSChanged;
		((Control)form).SizeChanged += Form_LSChanged;
		LoadImg();
		base.OnLoad(e);
	}

	private void Form_LSChanged(object? sender, EventArgs e)
	{
		if (form is Window window)
		{
			SetSize(window.Size);
			SetLocation(window.Location);
			((Form)this).Size = window.Size;
			((Form)this).Location = window.Location;
		}
		else
		{
			SetSize(form.Size);
			SetLocation(form.Location);
			((Form)this).Size = form.Size;
			((Form)this).Location = form.Location;
		}
		Print();
	}

	protected override void Dispose(bool disposing)
	{
		((Control)form).LocationChanged -= Form_LSChanged;
		((Control)form).SizeChanged -= Form_LSChanged;
		base.Dispose(disposing);
	}

	private void LoadImg()
	{
		autoDpi = true;
		if (config.Content is IList<Image> list)
		{
			Img = list[SelectIndex];
			ImgSize = Img.Size;
			FillScaleImg();
		}
		else if (config.Content is object[] array && array[0] is IList<object> list2)
		{
			if (array[1] is Func<int, object, Image> func)
			{
				Image? img = Img;
				if (img != null)
				{
					img.Dispose();
				}
				SelectValue = list2[SelectIndex];
				Img = func(SelectIndex, SelectValue);
				if (Img == null)
				{
					Print();
					return;
				}
				ImgSize = Img.Size;
				FillScaleImg();
				return;
			}
			object obj = array[1];
			Func<int, object, Action<float, string?>, Image?> callprog = obj as Func<int, object, Action<float, string>, Image>;
			if (callprog == null)
			{
				return;
			}
			LoadingProgressStr = null;
			_value = -1f;
			Loading = true;
			int selectIndex = SelectIndex;
			SelectValue = list2[SelectIndex];
			ITask.Run(delegate
			{
				Image val = callprog(SelectIndex, SelectValue, delegate(float prog, string? progstr)
				{
					LoadingProgressStr = progstr;
					LoadingProgress = prog;
				});
				if (selectIndex == SelectIndex)
				{
					if (val == null)
					{
						Image? img2 = Img;
						if (img2 != null)
						{
							img2.Dispose();
						}
						Img = null;
					}
					else
					{
						LoadingProgressStr = null;
						Image? img3 = Img;
						if (img3 != null)
						{
							img3.Dispose();
						}
						Img = val;
						ImgSize = Img.Size;
						FillScaleImg();
					}
				}
				else if (val != null)
				{
					val.Dispose();
				}
			}, delegate
			{
				if (selectIndex == SelectIndex)
				{
					Loading = false;
				}
			});
		}
		else
		{
			Img = null;
			Print();
		}
	}

	private RectangleF ScaleImg(Rectangle rect, float dpi)
	{
		float num = (float)ImgSize.Width * dpi;
		float num2 = (float)ImgSize.Height * dpi;
		rect_img_oxy = new PointF(((float)rect.Width - num) / 2f, ((float)rect.Height - num2) / 2f);
		if (num < (float)rect.Width || num2 < (float)rect.Height)
		{
			if (num < (float)rect.Width && num2 < (float)rect.Height)
			{
				if (offsetX < 0f - rect_img_oxy.X)
				{
					offsetX = 0f - rect_img_oxy.X;
				}
				else if (offsetX > rect_img_oxy.X)
				{
					offsetX = rect_img_oxy.X;
				}
				if (offsetY < 0f - rect_img_oxy.Y)
				{
					offsetY = 0f - rect_img_oxy.Y;
				}
				else if (offsetY > rect_img_oxy.Y)
				{
					offsetY = rect_img_oxy.Y;
				}
			}
			else if (num < (float)rect.Width)
			{
				offsetX = 0f;
				if (offsetY > 0f - rect_img_oxy.Y)
				{
					offsetY = 0f - rect_img_oxy.Y;
				}
				else if (offsetY < rect_img_oxy.Y)
				{
					offsetY = rect_img_oxy.Y;
				}
			}
			else
			{
				offsetY = 0f;
				if (offsetX > 0f - rect_img_oxy.X)
				{
					offsetX = 0f - rect_img_oxy.X;
				}
				else if (offsetX < rect_img_oxy.X)
				{
					offsetX = rect_img_oxy.X;
				}
			}
		}
		else
		{
			if (offsetX < rect_img_oxy.X)
			{
				offsetX = rect_img_oxy.X;
			}
			else if (offsetX > 0f - rect_img_oxy.X)
			{
				offsetX = 0f - rect_img_oxy.X;
			}
			if (offsetY < rect_img_oxy.Y)
			{
				offsetY = rect_img_oxy.Y;
			}
			else if (offsetY > 0f - rect_img_oxy.Y)
			{
				offsetY = 0f - rect_img_oxy.Y;
			}
		}
		return new RectangleF(offsetX + rect_img_oxy.X, offsetY + rect_img_oxy.Y, num, num2);
	}

	private void FillScaleImg()
	{
		if (!autoDpi)
		{
			return;
		}
		Rectangle rectangle = rect_read;
		float num = (float)((double)rectangle.Width * 1.0 / ((double)ImgSize.Width * 1.0));
		float num2 = (float)((double)rectangle.Height * 1.0 / ((double)ImgSize.Height * 1.0));
		if (num > 1f && num2 > 0f)
		{
			Dpi = 1f;
		}
		else if (ImgSize.Width > ImgSize.Height)
		{
			if (rectangle.Width > rectangle.Height)
			{
				Dpi = num;
			}
			else
			{
				Dpi = num2;
			}
		}
		else if (rectangle.Width > rectangle.Height)
		{
			Dpi = num2;
		}
		else
		{
			Dpi = (float)((double)rectangle.Width * 1.0 / ((double)ImgSize.Height * 1.0));
		}
	}

	public override Bitmap PrintBit()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		Bitmap val = new Bitmap(base.TargetRect.Width, base.TargetRect.Height);
		using Canvas canvas = Graphics.FromImage((Image)(object)val).High();
		SolidBrush val2 = new SolidBrush(Color.FromArgb(115, 0, 0, 0));
		try
		{
			if (Radius > 0)
			{
				GraphicsPath val3 = rect_read.RoundPath(Radius);
				try
				{
					canvas.Fill((Brush)(object)val2, val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			else
			{
				canvas.Fill((Brush)(object)val2, rect_read);
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		if (Img == null)
		{
			if (LoadingProgressStr != null)
			{
				PaintLoading(canvas, error: true);
			}
		}
		else
		{
			canvas.Image(Img, rect_img_dpi, new RectangleF(0f, 0f, ImgSize.Width, ImgSize.Height), (GraphicsUnit)2);
			if (loading)
			{
				PaintLoading(canvas);
			}
		}
		GraphicsPath val4 = rect_panel.RoundPath(rect_panel.Height);
		try
		{
			SolidBrush val5 = new SolidBrush(Color.FromArgb(26, 0, 0, 0));
			try
			{
				canvas.Fill((Brush)(object)val5, val4);
				PaintBtn(canvas, val5, rect_close, rect_close_icon, SvgDb.IcoClose, hoverClose, enabled: true);
				if (PageSize > 1)
				{
					PaintBtn(canvas, val5, rect_left, rect_left_icon, "LeftOutlined", hoverLeft, enabledLeft);
					PaintBtn(canvas, val5, rect_right, rect_right_icon, "RightOutlined", hoverRight, enabledRight);
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
		PreBtns[] array = btns;
		foreach (PreBtns preBtns in array)
		{
			Bitmap imgExtend = SvgExtend.GetImgExtend(preBtns.svg, preBtns.rect, preBtns.hover ? colorHover : colorDefault);
			try
			{
				if (imgExtend != null)
				{
					if (preBtns.enabled)
					{
						canvas.Image(imgExtend, preBtns.rect);
					}
					else
					{
						canvas.Image(imgExtend, preBtns.rect, 0.3f);
					}
				}
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		return val;
	}

	private void PaintLoading(Canvas g, bool error = false)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		float num = 6f * Config.Dpi;
		int num2 = (int)(40f * Config.Dpi);
		Rectangle rectangle = new Rectangle(rect_read.X + (rect_read.Width - num2) / 2, rect_read.Y + (rect_read.Height - num2) / 2, num2, num2);
		Color color;
		Color color2;
		if (error)
		{
			color = Colour.Error.Get("Preview");
			color2 = Colour.ErrorColor.Get("Preview");
		}
		else
		{
			color = Colour.Primary.Get("Preview");
			color2 = Colour.PrimaryColor.Get("Preview");
		}
		g.DrawEllipse(Color.FromArgb(220, color2), num, rectangle);
		if (_value > -1f)
		{
			Pen val = new Pen(color, num);
			try
			{
				g.DrawArc(val, rectangle, -90f, 360f * _value);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			if (LoadingProgressStr != null)
			{
				rectangle.Offset(0, num2);
				g.String(LoadingProgressStr, ((Control)this).Font, color2, rectangle, s_f);
			}
		}
		else if (LoadingProgressStr != null)
		{
			g.DrawEllipse(Colour.Error.Get("Preview"), num, rectangle);
			rectangle.Offset(0, num2);
			g.String(LoadingProgressStr, ((Control)this).Font, Colour.ErrorColor.Get("Preview"), rectangle, s_f);
		}
	}

	private void PaintBtn(Canvas g, SolidBrush brush, Rectangle rect, Rectangle rect_ico, string svg, bool hover, bool enabled)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		Bitmap imgExtend = SvgExtend.GetImgExtend(svg, rect_ico, Color.White);
		try
		{
			if (imgExtend == null)
			{
				return;
			}
			if (hover)
			{
				SolidBrush val = new SolidBrush(Color.FromArgb(51, 0, 0, 0));
				try
				{
					g.FillEllipse((Brush)(object)val, rect);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else
			{
				g.FillEllipse((Brush)(object)brush, rect);
			}
			if (enabled)
			{
				g.Image(imgExtend, rect_ico);
			}
			else
			{
				g.Image(imgExtend, rect_ico, 0.3f);
			}
		}
		finally
		{
			((IDisposable)imgExtend)?.Dispose();
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		if (btns != null)
		{
			Rectangle targetRectXY = base.TargetRectXY;
			rect_read = (HasBor ? new Rectangle(Bor, 0, targetRectXY.Width - Bor * 2, targetRectXY.Height - Bor) : targetRectXY);
			int num = (int)(46f * Config.Dpi);
			int num2 = (int)(40f * Config.Dpi);
			int num3 = (int)(42f * Config.Dpi);
			int num4 = (int)(24f * Config.Dpi);
			int num5 = (int)(12f * Config.Dpi);
			int num6 = (int)(32f * Config.Dpi);
			int size = (int)(18f * Config.Dpi);
			rect_close = new Rectangle(rect_read.Right - num6 - num3, rect_read.Y + num6, num3, num3);
			rect_close_icon = GetCentered(rect_close, size);
			if (PageSize > 1)
			{
				rect_left = new Rectangle(rect_read.X + num5, rect_read.Y + (rect_read.Height - num2) / 2, num2, num2);
				rect_left_icon = GetCentered(rect_left, size);
				rect_right = new Rectangle(rect_read.Right - num5 - num2, rect_read.Y + (rect_read.Height - num2) / 2, num2, num2);
				rect_right_icon = GetCentered(rect_right, size);
			}
			int num7 = num3 * btns.Length - 1 + num4 * 2;
			int num8 = rect_read.X + (rect_read.Width - num7) / 2;
			int y = rect_read.Bottom - num6 - num;
			rect_panel = new Rectangle(num8, y, num7, num);
			num8 += num4;
			PreBtns[] array = btns;
			foreach (PreBtns preBtns in array)
			{
				preBtns.Rect = new Rectangle(num8, y, num3, num);
				preBtns.rect = GetCentered(preBtns.Rect, size);
				num8 += num3;
			}
			((Control)this).OnSizeChanged(e);
		}
	}

	private Rectangle GetCentered(Rectangle rect, int size)
	{
		int num = (rect.Width - size) / 2;
		return new Rectangle(rect.X + num, rect.Y + num, size, size);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (Img != null)
		{
			autoDpi = false;
			if (e.Delta > 0)
			{
				Dpi += 0.1f;
				SetBtnEnabled("@t_zoomOut", enabled: true);
			}
			else
			{
				Dpi -= 0.1f;
				SetBtnEnabled("@t_zoomOut", (double)Dpi >= 0.06);
			}
			Print();
		}
		base.OnMouseWheel(e);
	}

	private void SetBtnEnabled(string id, bool enabled)
	{
		PreBtns[] array = btns;
		foreach (PreBtns preBtns in array)
		{
			if (preBtns.id == id)
			{
				preBtns.enabled = enabled;
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (moveImg && ((offsetXOld != (float)e.X && offsetYOld != (float)e.Y) || moveImging))
		{
			moveImging = true;
			offsetX = offsetXOld + (float)e.X - (float)movePos.X;
			offsetY = offsetYOld + (float)e.Y - (float)movePos.Y;
			Dpi = _dpi;
			Print();
		}
		else
		{
			if (btns == null)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			if (rect_close.Contains(e.Location))
			{
				num2++;
				if (!hoverClose)
				{
					hoverClose = true;
					num++;
				}
			}
			else if (hoverClose)
			{
				hoverClose = false;
				num++;
			}
			if (PageSize > 1)
			{
				if (enabledLeft && rect_left.Contains(e.Location))
				{
					num2++;
					if (!hoverLeft)
					{
						hoverLeft = true;
						num++;
					}
				}
				else if (hoverLeft)
				{
					hoverLeft = false;
					num++;
				}
				if (enabledRight && rect_right.Contains(e.Location))
				{
					num2++;
					if (!hoverRight)
					{
						hoverRight = true;
						num++;
					}
				}
				else if (hoverRight)
				{
					hoverRight = false;
					num++;
				}
			}
			PreBtns[] array = btns;
			foreach (PreBtns preBtns in array)
			{
				if (preBtns.enabled && preBtns.Rect.Contains(e.Location))
				{
					num2++;
					if (!preBtns.hover)
					{
						preBtns.hover = true;
						num++;
					}
				}
				else if (preBtns.hover)
				{
					preBtns.hover = false;
					num++;
				}
			}
			SetCursor(num2 > 0);
			if (num > 0)
			{
				Print();
			}
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		((Control)this).OnMouseDown(e);
		if ((int)e.Button != 1048576)
		{
			return;
		}
		PreBtns[] array = btns;
		foreach (PreBtns preBtns in array)
		{
			if (preBtns.enabled && preBtns.Rect.Contains(e.Location))
			{
				preBtns.mdown = true;
				return;
			}
		}
		if (rect_img_dpi.Contains(e.Location) && (!(rect_img_dpi.Width < (float)((Control)this).Width) || !(rect_img_dpi.Height < (float)((Control)this).Height)))
		{
			movePos = e.Location;
			offsetXOld = offsetX;
			offsetYOld = offsetY;
			moveImging = false;
			moveImg = true;
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (moveImg)
		{
			moveImg = false;
			if (moveImging)
			{
				moveImging = false;
				((Control)this).OnMouseUp(e);
				return;
			}
		}
		PreBtns[] array = btns;
		foreach (PreBtns preBtns in array)
		{
			if (!preBtns.mdown)
			{
				continue;
			}
			if (preBtns.Rect.Contains(e.Location))
			{
				switch (preBtns.id)
				{
				case "@t_flipY":
					if (Img != null)
					{
						Img.RotateFlip((RotateFlipType)6);
						Print();
					}
					break;
				case "@t_flipX":
					if (Img != null)
					{
						Img.RotateFlip((RotateFlipType)4);
						Print();
					}
					break;
				case "@t_rotateL":
					if (Img != null)
					{
						float dpi2 = _dpi;
						bool flag2 = autoDpi;
						Img.RotateFlip((RotateFlipType)3);
						ImgSize = Img.Size;
						autoDpi = true;
						FillScaleImg();
						Dpi = dpi2;
						autoDpi = flag2;
						Print();
					}
					break;
				case "@t_rotateR":
					if (Img != null)
					{
						float dpi = _dpi;
						bool flag = autoDpi;
						Img.RotateFlip((RotateFlipType)1);
						ImgSize = Img.Size;
						autoDpi = true;
						FillScaleImg();
						Dpi = dpi;
						autoDpi = flag;
						Print();
					}
					break;
				case "@t_zoomOut":
					Dpi -= 0.1f;
					SetBtnEnabled("@t_zoomOut", (double)Dpi >= 0.06);
					Print();
					break;
				case "@t_zoomIn":
					Dpi += 0.1f;
					SetBtnEnabled("@t_zoomOut", enabled: true);
					Print();
					break;
				default:
					config.OnBtns?.Invoke(preBtns.id, new Preview.BtnEvent(SelectIndex, SelectValue, preBtns.tag));
					break;
				}
			}
			preBtns.mdown = false;
			return;
		}
		if (rect_close.Contains(e.Location))
		{
			IClose();
			return;
		}
		if (PageSize > 1)
		{
			if (enabledLeft && rect_left.Contains(e.Location))
			{
				SelectIndex--;
				LoadImg();
				Print();
				return;
			}
			if (enabledRight && rect_right.Contains(e.Location))
			{
				SelectIndex++;
				LoadImg();
				Print();
				return;
			}
		}
		if (!rect_img_dpi.Contains(e.Location))
		{
			IClose();
		}
		((Control)this).OnMouseUp(e);
	}
}

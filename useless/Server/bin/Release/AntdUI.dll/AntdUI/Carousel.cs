using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

[Description("Carousel 走马灯")]
[ToolboxItem(true)]
[DefaultProperty("Image")]
[DefaultEvent("SelectIndexChanged")]
[Designer(typeof(IControlDesigner))]
public class Carousel : IControl
{
	private bool autoplay;

	private TAlignMini dotPosition;

	private bool dotPV;

	private int radius;

	private bool round;

	private CarouselItemCollection? items;

	private TFit imageFit = TFit.Cover;

	private int selectIndex;

	private DateTime now = DateTime.Now;

	private bool AnimationChangeAuto;

	private int AnimationChangeMax;

	private int AnimationChangeMaxWH;

	private float AnimationChangeValue;

	private bool AnimationChange;

	private ITask? ThreadChange;

	private CarouselDotItem[] dot_list = new CarouselDotItem[0];

	private string? bmpcode;

	private Bitmap? bmp;

	private bool _mouseHover;

	private bool down;

	private float tvaluexy;

	[Description("手势滑动")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool Touch { get; set; } = true;


	[Description("滑动到外面")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool TouchOut { get; set; }

	[Description("自动切换")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool Autoplay
	{
		get
		{
			return autoplay;
		}
		set
		{
			if (autoplay != value)
			{
				autoplay = value;
				if (value)
				{
					Thread thread = new Thread(LongTime);
					thread.IsBackground = true;
					thread.Start();
				}
			}
		}
	}

	[Description("自动切换延迟(s)")]
	[Category("行为")]
	[DefaultValue(4)]
	public int Autodelay { get; set; } = 4;


	[Description("面板指示点大小")]
	[Category("面板")]
	[DefaultValue(typeof(Size), "28, 4")]
	public Size DotSize { get; set; } = new Size(28, 4);


	[Description("面板指示点边距")]
	[Category("面板")]
	[DefaultValue(12)]
	public int DotMargin { get; set; } = 12;


	[Description("面板指示点位置")]
	[Category("面板")]
	[DefaultValue(TAlignMini.None)]
	public TAlignMini DotPosition
	{
		get
		{
			return dotPosition;
		}
		set
		{
			if (dotPosition != value)
			{
				dotPosition = value;
				dotPV = value == TAlignMini.Left || value == TAlignMini.Right;
				ChangeImg();
				((Control)this).Invalidate();
			}
		}
	}

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Radius
	{
		get
		{
			return radius;
		}
		set
		{
			if (radius != value)
			{
				radius = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("圆角样式")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Round
	{
		get
		{
			return round;
		}
		set
		{
			if (round != value)
			{
				round = value;
				((Control)this).Invalidate();
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("图片集合")]
	[Category("数据")]
	public CarouselItemCollection Image
	{
		get
		{
			if (items == null)
			{
				items = new CarouselItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	[Description("图片布局")]
	[Category("外观")]
	[DefaultValue(TFit.Cover)]
	public TFit ImageFit
	{
		get
		{
			return imageFit;
		}
		set
		{
			if (imageFit != value)
			{
				imageFit = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("选择序号")]
	[Category("数据")]
	[DefaultValue(0)]
	public int SelectIndex
	{
		get
		{
			return selectIndex;
		}
		set
		{
			if (autoplay)
			{
				now = DateTime.Now.AddSeconds(Autodelay);
			}
			if (selectIndex == value)
			{
				return;
			}
			if (items != null)
			{
				if (items.ListExceed(value))
				{
					selectIndex = 0;
				}
				else
				{
					SetSelectIndex(value);
				}
			}
			else
			{
				selectIndex = 0;
			}
		}
	}

	private bool ExtraMouseHover
	{
		get
		{
			return _mouseHover;
		}
		set
		{
			if (_mouseHover != value)
			{
				_mouseHover = value;
				SetCursor(value && base.Enabled);
				if (!value && autoplay)
				{
					now = DateTime.Now.AddSeconds(Autodelay);
				}
			}
		}
	}

	[Description("SelectIndex 属性值更改时发生")]
	[Category("行为")]
	public event IntEventHandler? SelectIndexChanged;

	private void SetSelectIndex(int value, bool auto = false)
	{
		if (dotPV)
		{
			SetSelectIndexVertical(value, auto);
		}
		else
		{
			SetSelectIndexHorizontal(value, auto);
		}
	}

	private void SetSelectIndexVertical(int value, bool auto = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		int height = ((Control)this).ClientRectangle.Height;
		Padding padding = ((Control)this).Padding;
		int num = height - ((Padding)(ref padding)).Vertical;
		if (items != null && ((Control)this).IsHandleCreated && Config.Animation)
		{
			ThreadChange?.Dispose();
			AnimationChangeAuto = false;
			float end = value * num;
			int count = items.Count;
			bool num2 = (float)(value * num) > AnimationChangeValue;
			AnimationChangeMax = count * num;
			AnimationChangeMaxWH = AnimationChangeMax - num;
			AnimationChange = true;
			int num3 = selectIndex;
			selectIndex = value;
			this.SelectIndexChanged?.Invoke(this, new IntEventArgs(value));
			float speed = Math.Abs(end - AnimationChangeValue) / 50f;
			if (speed < 8f)
			{
				speed = 8f;
			}
			if (num2)
			{
				float modera3 = end - (float)num * 0.05f;
				ThreadChange = new ITask((Control)(object)this, delegate
				{
					AnimationChangeValue = AnimationChangeValue.Calculate(Speed(speed, modera3));
					if (AnimationChangeValue > end)
					{
						AnimationChangeValue = end;
						return false;
					}
					((Control)this).Invalidate();
					return true;
				}, 10, delegate
				{
					AnimationChange = false;
					((Control)this).Invalidate();
				});
				return;
			}
			if (auto && value == 0 && count > 2 && num3 == count - 1)
			{
				AnimationChangeAuto = true;
				end = count * num;
				float modera2 = end - (float)num * 0.05f;
				ThreadChange = new ITask((Control)(object)this, delegate
				{
					AnimationChangeValue = AnimationChangeValue.Calculate(Speed(speed, modera2));
					if (AnimationChangeValue > end)
					{
						AnimationChangeValue = end;
						return false;
					}
					((Control)this).Invalidate();
					return true;
				}, 10, delegate
				{
					AnimationChange = false;
					AnimationChangeValue = 0f;
					((Control)this).Invalidate();
				});
				return;
			}
			float modera = end + (float)num * 0.05f;
			ThreadChange = new ITask((Control)(object)this, delegate
			{
				AnimationChangeValue -= Speed2(speed, modera);
				if (AnimationChangeValue <= end)
				{
					AnimationChangeValue = end;
					return false;
				}
				((Control)this).Invalidate();
				return true;
			}, 10, delegate
			{
				AnimationChange = false;
				((Control)this).Invalidate();
			});
		}
		else
		{
			selectIndex = value;
			AnimationChangeValue = value * num;
			((Control)this).Invalidate();
		}
	}

	private void SetSelectIndexHorizontal(int value, bool auto = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		int width = ((Control)this).ClientRectangle.Width;
		Padding padding = ((Control)this).Padding;
		int num = width - ((Padding)(ref padding)).Horizontal;
		if (items != null && ((Control)this).IsHandleCreated && Config.Animation)
		{
			ThreadChange?.Dispose();
			AnimationChangeAuto = false;
			float end = value * num;
			int count = items.Count;
			bool num2 = (float)(value * num) > AnimationChangeValue;
			AnimationChangeMax = count * num;
			AnimationChangeMaxWH = AnimationChangeMax - num;
			AnimationChange = true;
			int num3 = selectIndex;
			selectIndex = value;
			this.SelectIndexChanged?.Invoke(this, new IntEventArgs(value));
			float speed = Math.Abs(end - AnimationChangeValue) / 50f;
			if (speed < 8f)
			{
				speed = 8f;
			}
			if (num2)
			{
				float modera3 = end - (float)num * 0.05f;
				ThreadChange = new ITask((Control)(object)this, delegate
				{
					AnimationChangeValue = AnimationChangeValue.Calculate(Speed(speed, modera3));
					if (AnimationChangeValue > end)
					{
						AnimationChangeValue = end;
						return false;
					}
					((Control)this).Invalidate();
					return true;
				}, 10, delegate
				{
					AnimationChange = false;
					((Control)this).Invalidate();
				});
				return;
			}
			if (auto && value == 0 && count > 2 && num3 == count - 1)
			{
				AnimationChangeAuto = true;
				end = count * num;
				float modera2 = end - (float)num * 0.05f;
				ThreadChange = new ITask((Control)(object)this, delegate
				{
					AnimationChangeValue = AnimationChangeValue.Calculate(Speed(speed, modera2));
					if (AnimationChangeValue > end)
					{
						AnimationChangeValue = end;
						return false;
					}
					((Control)this).Invalidate();
					return true;
				}, 10, delegate
				{
					AnimationChange = false;
					AnimationChangeValue = 0f;
					((Control)this).Invalidate();
				});
				return;
			}
			float modera = end + (float)num * 0.05f;
			ThreadChange = new ITask((Control)(object)this, delegate
			{
				AnimationChangeValue -= Speed2(speed, modera);
				if (AnimationChangeValue <= end)
				{
					AnimationChangeValue = end;
					return false;
				}
				((Control)this).Invalidate();
				return true;
			}, 10, delegate
			{
				AnimationChange = false;
				((Control)this).Invalidate();
			});
		}
		else
		{
			selectIndex = value;
			AnimationChangeValue = value * num;
			((Control)this).Invalidate();
		}
	}

	private float Speed(float speed, float modera)
	{
		if (modera < AnimationChangeValue)
		{
			return 0.8f;
		}
		return speed;
	}

	private float Speed2(float speed, float modera)
	{
		if (modera > AnimationChangeValue)
		{
			return 0.8f;
		}
		return speed;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		if (dotPV)
		{
			int height = ((Control)this).ClientRectangle.Height;
			AnimationChangeValue = selectIndex * height;
		}
		else
		{
			int width = ((Control)this).ClientRectangle.Width;
			AnimationChangeValue = selectIndex * width;
		}
		((Control)this).OnHandleCreated(e);
	}

	protected override void Dispose(bool disposing)
	{
		ThreadChange?.Dispose();
		base.Dispose(disposing);
	}

	private void LongTime()
	{
		while (autoplay)
		{
			if (Autodelay > 0)
			{
				Thread.Sleep(Autodelay * 1000);
			}
			else
			{
				Thread.Sleep(1000);
			}
			try
			{
				if (!down && !ExtraMouseHover && DateTime.Now > now && items != null)
				{
					if (selectIndex >= items.Count - 1)
					{
						SetSelectIndex(0, auto: true);
					}
					else
					{
						SetSelectIndex(selectIndex + 1, auto: true);
					}
				}
			}
			catch
			{
			}
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		ChangeImg();
		((Control)this).OnSizeChanged(e);
	}

	internal void ChangeImg()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (DotPosition == TAlignMini.None || items == null || items.Count == 0)
		{
			return;
		}
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return;
		}
		Bitmap? obj = bmp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		bmp = null;
		foreach (CarouselItem item in items)
		{
			item.PARENT = this;
		}
		Rectangle rectangle = clientRectangle.PaddingRect(((Control)this).Padding);
		int count = items.Count;
		List<CarouselDotItem> list = new List<CarouselDotItem>(count);
		if (DotPosition == TAlignMini.Top || DotPosition == TAlignMini.Bottom)
		{
			int num = DotSize.Width * count;
			int y = ((DotPosition == TAlignMini.Bottom) ? (rectangle.Y + rectangle.Height - (DotMargin + DotSize.Height)) : (rectangle.Y + DotMargin));
			int y2 = ((DotPosition == TAlignMini.Bottom) ? (rectangle.Y + rectangle.Height - (DotMargin + DotSize.Height) - DotMargin / 2) : (rectangle.Y + DotMargin / 2));
			int num2 = rectangle.X + (rectangle.Width - num) / 2;
			for (int i = 0; i < count; i++)
			{
				list.Add(new CarouselDotItem
				{
					i = i,
					rect_fill = new Rectangle(num2, y2, DotSize.Width, DotMargin),
					rect_action = new Rectangle(num2 + 2, y, DotSize.Width - 4, DotSize.Height),
					rect = new Rectangle(num2 + 4, y, DotSize.Width - 8, DotSize.Height)
				});
				num2 += DotSize.Width;
			}
		}
		else
		{
			int num3 = DotSize.Width * count;
			int x = ((DotPosition == TAlignMini.Right) ? (rectangle.X + rectangle.Width - (DotMargin + DotSize.Height)) : (rectangle.X + DotMargin));
			int x2 = ((DotPosition == TAlignMini.Right) ? (rectangle.X + rectangle.Width - (DotMargin + DotSize.Height) - DotMargin / 2) : (rectangle.X + DotMargin / 2));
			int num4 = rectangle.Y + (rectangle.Height - num3) / 2;
			for (int j = 0; j < count; j++)
			{
				list.Add(new CarouselDotItem
				{
					i = j,
					rect_fill = new Rectangle(x2, num4, DotMargin, DotSize.Width),
					rect_action = new Rectangle(x, num4 + 2, DotSize.Height, DotSize.Width - 4),
					rect = new Rectangle(x, num4 + 4, DotSize.Height, DotSize.Width - 8)
				});
				num4 += DotSize.Width;
			}
		}
		dot_list = list.ToArray();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Expected O, but got Unknown
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Expected O, but got Unknown
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
		Rectangle rectangle = clientRectangle.PaddingRect(((Control)this).Padding);
		Canvas canvas = e.Graphics.High();
		int count = items.Count;
		Image img = items[selectIndex].Img;
		float num = (float)radius * Config.Dpi;
		if (img != null)
		{
			if (AnimationChange)
			{
				if (dotPV)
				{
					CarouselRectPanel carouselRectPanel = SelectRangeVertical(count, rectangle);
					if (bmp == null || bmpcode != carouselRectPanel.i)
					{
						bmpcode = carouselRectPanel.i;
						bmp = PaintBmpVertical(items, carouselRectPanel, rectangle, num);
					}
					canvas.Image((Image)(object)bmp, rectangle.X, (int)((float)rectangle.Y - AnimationChangeValue), ((Image)bmp).Width, ((Image)bmp).Height);
				}
				else
				{
					CarouselRectPanel carouselRectPanel2 = SelectRangeHorizontal(count, rectangle);
					if (bmp == null || bmpcode != carouselRectPanel2.i)
					{
						bmpcode = carouselRectPanel2.i;
						bmp = PaintBmpHorizontal(items, carouselRectPanel2, rectangle, num);
					}
					canvas.Image((Image)(object)bmp, (int)((float)rectangle.X - AnimationChangeValue), rectangle.Y, ((Image)bmp).Width, ((Image)bmp).Height);
				}
			}
			else
			{
				canvas.Image(rectangle, img, imageFit, num, round);
			}
		}
		if (dot_list.Length != 0)
		{
			SolidBrush val = new SolidBrush(Colour.BgBase.Get("Carousel"));
			try
			{
				SolidBrush val2 = new SolidBrush(Color.FromArgb(77, val.Color));
				try
				{
					if (round || radius > 0)
					{
						CarouselDotItem[] array = dot_list;
						foreach (CarouselDotItem carouselDotItem in array)
						{
							if (carouselDotItem.i == selectIndex)
							{
								GraphicsPath val3 = carouselDotItem.rect_action.RoundPath(DotSize.Height);
								try
								{
									canvas.Fill((Brush)(object)val, val3);
								}
								finally
								{
									((IDisposable)val3)?.Dispose();
								}
							}
							else
							{
								GraphicsPath val4 = carouselDotItem.rect.RoundPath(DotSize.Height);
								try
								{
									canvas.Fill((Brush)(object)val2, val4);
								}
								finally
								{
									((IDisposable)val4)?.Dispose();
								}
							}
						}
					}
					else
					{
						CarouselDotItem[] array = dot_list;
						foreach (CarouselDotItem carouselDotItem2 in array)
						{
							if (carouselDotItem2.i == selectIndex)
							{
								canvas.Fill((Brush)(object)val, carouselDotItem2.rect_action);
							}
							else
							{
								canvas.Fill((Brush)(object)val2, carouselDotItem2.rect);
							}
						}
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
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private Bitmap PaintBmpVertical(CarouselItemCollection items, CarouselRectPanel select_range, Rectangle rect, float radius)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		bmpcode = select_range.i;
		Bitmap val;
		if (AnimationChangeAuto)
		{
			val = new Bitmap(rect.Width, AnimationChangeMax + rect.Height);
			using Canvas canvas = Graphics.FromImage((Image)(object)val).High();
			PaintBmp(items, select_range, canvas, radius);
			Image img = items[0].Img;
			if (img != null)
			{
				canvas.Image(new Rectangle(0, AnimationChangeMax, rect.Width, rect.Height), img, imageFit, radius, round);
			}
		}
		else
		{
			val = new Bitmap(rect.Width, AnimationChangeMax);
			using Canvas g = Graphics.FromImage((Image)(object)val).High();
			PaintBmp(items, select_range, g, radius);
		}
		return val;
	}

	private Bitmap PaintBmpHorizontal(CarouselItemCollection items, CarouselRectPanel select_range, Rectangle rect, float radius)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		bmpcode = select_range.i;
		Bitmap val;
		if (AnimationChangeAuto)
		{
			val = new Bitmap(AnimationChangeMax + rect.Width, rect.Height);
			using Canvas canvas = Graphics.FromImage((Image)(object)val).High();
			PaintBmp(items, select_range, canvas, radius);
			Image img = items[0].Img;
			if (img != null)
			{
				canvas.Image(new Rectangle(AnimationChangeMax, 0, rect.Width, rect.Height), img, imageFit, radius, round);
			}
		}
		else
		{
			val = new Bitmap(AnimationChangeMax, rect.Height);
			using Canvas g = Graphics.FromImage((Image)(object)val).High();
			PaintBmp(items, select_range, g, radius);
		}
		return val;
	}

	private void PaintBmp(CarouselItemCollection items, CarouselRectPanel select_range, Canvas g2, float radius)
	{
		foreach (CarouselRect item in select_range.list)
		{
			Image img = items[item.i].Img;
			if (img != null)
			{
				g2.Image(item.rect, img, imageFit, radius, round);
			}
		}
	}

	private CarouselRectPanel SelectRangeVertical(int len, Rectangle rect)
	{
		CarouselRectPanel carouselRectPanel = new CarouselRectPanel
		{
			list = new List<CarouselRect>(len)
		};
		List<int> list = new List<int>(len);
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			Rectangle rect2 = new Rectangle(0, num, rect.Width, rect.Height);
			if (rect2.Contains(0, (int)AnimationChangeValue))
			{
				list.Add(i);
				carouselRectPanel.list.Add(new CarouselRect
				{
					i = i,
					rect = rect2
				});
			}
			if (i < len - 1)
			{
				Rectangle rect3 = new Rectangle(0, num + rect.Height, rect.Width, rect.Height);
				if (rect3.Contains(0, (int)(AnimationChangeValue + (float)rect.Height)))
				{
					list.Add(i + 1);
					carouselRectPanel.list.Add(new CarouselRect
					{
						i = i + 1,
						rect = rect3
					});
				}
			}
			num += rect.Height;
			if ((float)num > AnimationChangeValue + (float)rect.Height)
			{
				break;
			}
		}
		if (carouselRectPanel.list.Count == 0 && AnimationChangeValue < 0f)
		{
			list.Add(0);
			carouselRectPanel.list.Add(new CarouselRect
			{
				i = 0,
				rect = new Rectangle(0, 0, rect.Width, rect.Height)
			});
		}
		carouselRectPanel.i = string.Join("", list);
		return carouselRectPanel;
	}

	private CarouselRectPanel SelectRangeHorizontal(int len, Rectangle rect)
	{
		CarouselRectPanel carouselRectPanel = new CarouselRectPanel
		{
			list = new List<CarouselRect>(len)
		};
		List<int> list = new List<int>(len);
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			Rectangle rect2 = new Rectangle(num, 0, rect.Width, rect.Height);
			if (rect2.Contains((int)AnimationChangeValue, 0))
			{
				list.Add(i);
				carouselRectPanel.list.Add(new CarouselRect
				{
					i = i,
					rect = rect2
				});
			}
			if (i < len - 1)
			{
				Rectangle rect3 = new Rectangle(num + rect.Width, 0, rect.Width, rect.Height);
				if (rect3.Contains((int)(AnimationChangeValue + (float)rect.Width), 0))
				{
					list.Add(i + 1);
					carouselRectPanel.list.Add(new CarouselRect
					{
						i = i + 1,
						rect = rect3
					});
				}
			}
			num += rect.Width;
			if ((float)num > AnimationChangeValue + (float)rect.Width)
			{
				break;
			}
		}
		if (carouselRectPanel.list.Count == 0 && AnimationChangeValue < 0f)
		{
			list.Add(0);
			carouselRectPanel.list.Add(new CarouselRect
			{
				i = 0,
				rect = new Rectangle(0, 0, rect.Width, rect.Height)
			});
		}
		carouselRectPanel.i = string.Join("", list);
		return carouselRectPanel;
	}

	private CarouselRect? SelectRangeOneVertical(int len, Rectangle rect)
	{
		List<CarouselRect> list = new List<CarouselRect>(len);
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			int num2 = num + rect.Height / 2;
			if (AnimationChangeValue > (float)(num2 - rect.Height) && AnimationChangeValue < (float)(num2 + rect.Height))
			{
				float p = AnimationChangeValue / (float)num2;
				list.Add(new CarouselRect
				{
					p = p,
					i = i,
					rect = new Rectangle(0, num, rect.Width, rect.Height)
				});
			}
			num += rect.Height;
			if ((float)num > AnimationChangeValue + (float)rect.Height)
			{
				break;
			}
		}
		if (list.Count > 0)
		{
			list.Sort((CarouselRect x, CarouselRect y) => x.p.CompareTo(y.p));
			return list[0];
		}
		return null;
	}

	private CarouselRect? SelectRangeOneHorizontal(int len, Rectangle rect)
	{
		List<CarouselRect> list = new List<CarouselRect>(len);
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			int num2 = num + rect.Width / 2;
			if (AnimationChangeValue > (float)(num2 - rect.Width) && AnimationChangeValue < (float)(num2 + rect.Width))
			{
				float p = AnimationChangeValue / (float)num2;
				list.Add(new CarouselRect
				{
					p = p,
					i = i,
					rect = new Rectangle(num, 0, rect.Width, rect.Height)
				});
			}
			num += rect.Width;
			if ((float)num > AnimationChangeValue + (float)rect.Width)
			{
				break;
			}
		}
		if (list.Count > 0)
		{
			list.Sort((CarouselRect x, CarouselRect y) => x.p.CompareTo(y.p));
			return list[0];
		}
		return null;
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
		ExtraMouseHover = true;
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576 && items != null)
		{
			if (dot_list.Length != 0)
			{
				for (int i = 0; i < dot_list.Length; i++)
				{
					if (dot_list[i].rect_fill.Contains(e.Location))
					{
						SetSelectIndex(dot_list[i].i);
						return;
					}
				}
			}
			if (Touch)
			{
				int count = items.Count;
				if (dotPV)
				{
					tvaluexy = AnimationChangeValue + (float)e.Y;
					int height = ((Control)this).ClientRectangle.Height;
					AnimationChangeMax = count * height;
					AnimationChangeMaxWH = AnimationChangeMax - height;
				}
				else
				{
					tvaluexy = AnimationChangeValue + (float)e.X;
					int width = ((Control)this).ClientRectangle.Width;
					AnimationChangeMax = count * width;
					AnimationChangeMaxWH = AnimationChangeMax - width;
				}
				down = true;
			}
		}
		((Control)this).OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (down)
		{
			float num = tvaluexy - (float)(dotPV ? e.Y : e.X);
			if (!TouchOut)
			{
				if (num < 0f)
				{
					num = 0f;
				}
				else if (num > (float)AnimationChangeMaxWH)
				{
					num = AnimationChangeMaxWH;
				}
			}
			AnimationChange = true;
			AnimationChangeValue = num;
			((Control)this).Invalidate();
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (down)
		{
			if (items == null)
			{
				down = false;
				return;
			}
			int count = items.Count;
			Rectangle clientRectangle = ((Control)this).ClientRectangle;
			AnimationChange = false;
			if (dotPV)
			{
				float num = tvaluexy - (float)e.Y;
				CarouselRect carouselRect = SelectRangeOneVertical(count, clientRectangle);
				if (carouselRect != null)
				{
					SetSelectIndexVertical(carouselRect.i);
				}
				else if (num > (float)(AnimationChangeMax - clientRectangle.Height))
				{
					SetSelectIndexVertical(items.Count - 1);
				}
				else if (num < 0f)
				{
					SetSelectIndexVertical(0);
				}
			}
			else
			{
				float num2 = tvaluexy - (float)e.X;
				CarouselRect carouselRect2 = SelectRangeOneHorizontal(count, clientRectangle);
				if (carouselRect2 != null)
				{
					SetSelectIndexHorizontal(carouselRect2.i);
				}
				else if (num2 > (float)(AnimationChangeMax - clientRectangle.Width))
				{
					SetSelectIndexHorizontal(items.Count - 1);
				}
				else if (num2 < 0f)
				{
					SetSelectIndexHorizontal(0);
				}
			}
			((Control)this).Invalidate();
		}
		down = false;
		((Control)this).OnMouseUp(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (e.Delta > 0)
		{
			if (selectIndex <= 0)
			{
				return;
			}
			SetSelectIndex(selectIndex - 1);
		}
		else
		{
			if (items == null || selectIndex >= items.Count - 1)
			{
				return;
			}
			SetSelectIndex(selectIndex + 1);
		}
		base.OnMouseWheel(e);
	}
}

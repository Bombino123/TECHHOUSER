using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

public class ScrollBar
{
	private Action? ChangeSize;

	private Action<Rectangle?> Invalidate;

	internal Action? OnInvalidate;

	private Rectangle RectY;

	private bool showY;

	private int valueY;

	private int maxY;

	private bool hoverY;

	private Rectangle RectX;

	private bool showX;

	private int valueX;

	private int maxX;

	private bool hoverX;

	private int oldx;

	private int oldy;

	private string show_oldx = "";

	private string show_oldy = "";

	private Point old;

	private bool SliderDownX;

	private float SliderX;

	private bool SliderDownY;

	private float SliderY;

	private ITask? ThreadHoverY;

	private float AnimationHoverYValue;

	private bool AnimationHoverY;

	private ITask? ThreadHoverX;

	private float AnimationHoverXValue;

	private bool AnimationHoverX;

	internal int Radius { get; set; }

	internal bool RB { get; set; }

	public bool Back { get; set; } = true;


	public int SIZE { get; set; } = 20;


	public int SIZE_BAR { get; set; } = 8;


	public int SIZE_MINIY { get; set; } = 30;


	public bool EnabledY { get; set; }

	public bool ShowY
	{
		get
		{
			return showY;
		}
		set
		{
			if (showY != value)
			{
				showY = value;
				Invalidate(null);
				ChangeSize?.Invoke();
			}
		}
	}

	public int ValueY
	{
		get
		{
			return valueY;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (maxY > 0)
			{
				int num = maxY - RectY.Height;
				if (value > num)
				{
					value = num;
				}
			}
			if (valueY != value)
			{
				valueY = value;
				Invalidate(null);
			}
		}
	}

	public int MaxY
	{
		get
		{
			return maxY;
		}
		set
		{
			if (maxY != value)
			{
				maxY = value;
				Invalidate(null);
			}
		}
	}

	public bool HoverY
	{
		get
		{
			return hoverY;
		}
		set
		{
			if (hoverY == value)
			{
				return;
			}
			hoverY = value;
			if (Config.Animation)
			{
				ThreadHoverY?.Dispose();
				AnimationHoverY = true;
				int t = Animation.TotalFrames(10, 100);
				if (value)
				{
					ThreadHoverY = new ITask(delegate(int i)
					{
						AnimationHoverYValue = Animation.Animate(i, t, 1f, AnimationType.Ball);
						Invalidate(RectY);
						return true;
					}, 10, t, delegate
					{
						AnimationHoverYValue = 1f;
						AnimationHoverY = false;
						Invalidate(RectY);
					});
				}
				else
				{
					ThreadHoverY = new ITask(delegate(int i)
					{
						AnimationHoverYValue = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
						Invalidate(RectY);
						return true;
					}, 10, t, delegate
					{
						AnimationHoverYValue = 0f;
						AnimationHoverY = false;
						Invalidate(RectY);
					});
				}
			}
			else
			{
				Invalidate(RectY);
			}
		}
	}

	public bool EnabledX { get; set; }

	public bool ShowX
	{
		get
		{
			return showX;
		}
		set
		{
			if (showX != value)
			{
				showX = value;
				Invalidate(null);
				ChangeSize?.Invoke();
			}
		}
	}

	public int ValueX
	{
		get
		{
			return valueX;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (maxX > 0)
			{
				int num = maxX - RectX.Width;
				if (value > num)
				{
					value = num;
				}
			}
			if (valueX != value)
			{
				valueX = value;
				Invalidate(null);
			}
		}
	}

	public int MaxX
	{
		get
		{
			return maxX;
		}
		set
		{
			if (maxX != value)
			{
				maxX = value;
				Invalidate(null);
			}
		}
	}

	public bool HoverX
	{
		get
		{
			return hoverX;
		}
		set
		{
			if (hoverX == value)
			{
				return;
			}
			hoverX = value;
			if (Config.Animation)
			{
				ThreadHoverX?.Dispose();
				AnimationHoverX = true;
				int t = Animation.TotalFrames(10, 100);
				if (value)
				{
					ThreadHoverX = new ITask(delegate(int i)
					{
						AnimationHoverXValue = Animation.Animate(i, t, 1f, AnimationType.Ball);
						Invalidate(RectX);
						return true;
					}, 10, t, delegate
					{
						AnimationHoverXValue = 1f;
						AnimationHoverX = false;
						Invalidate(RectX);
					});
				}
				else
				{
					ThreadHoverX = new ITask(delegate(int i)
					{
						AnimationHoverXValue = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
						Invalidate(RectX);
						return true;
					}, 10, t, delegate
					{
						AnimationHoverXValue = 0f;
						AnimationHoverX = false;
						Invalidate(RectX);
					});
				}
			}
			else
			{
				Invalidate(RectX);
			}
		}
	}

	public bool Show
	{
		get
		{
			if (EnabledY)
			{
				return showY;
			}
			return showX;
		}
	}

	public int Value
	{
		get
		{
			if (EnabledY)
			{
				return valueY;
			}
			return valueX;
		}
		set
		{
			if (EnabledY)
			{
				ValueY = value;
			}
			else
			{
				ValueX = value;
			}
		}
	}

	public int VrValueI
	{
		get
		{
			if (EnabledY)
			{
				return maxY - RectY.Height;
			}
			return maxX - RectX.Width;
		}
	}

	public int ReadSize
	{
		get
		{
			if (EnabledY)
			{
				return RectY.Height;
			}
			return RectX.Width;
		}
	}

	public int Max
	{
		get
		{
			if (EnabledY)
			{
				return maxY;
			}
			return maxX;
		}
	}

	public ScrollBar(FlowPanel control, bool enabledY = true, bool enabledX = false)
	{
		FlowPanel control2 = control;
		base._002Ector();
		ScrollBar scrollBar = this;
		OnInvalidate = (ChangeSize = delegate
		{
			control2.IOnSizeChanged();
		});
		Invalidate = delegate
		{
			scrollBar.OnInvalidate?.Invoke();
		};
		EnabledX = enabledX;
		EnabledY = enabledY;
		Init();
	}

	public ScrollBar(StackPanel control)
	{
		StackPanel control2 = control;
		base._002Ector();
		ScrollBar scrollBar = this;
		OnInvalidate = (ChangeSize = delegate
		{
			control2.IOnSizeChanged();
		});
		Invalidate = delegate
		{
			scrollBar.OnInvalidate?.Invoke();
		};
		if (control2.Vertical)
		{
			EnabledY = true;
		}
		else
		{
			EnabledX = true;
		}
		Init();
	}

	public ScrollBar(IControl control, bool enabledY = true, bool enabledX = false, int radius = 0, bool radiusy = false)
	{
		IControl control2 = control;
		base._002Ector();
		ScrollBar scrollBar = this;
		Radius = radius;
		RB = radiusy;
		Invalidate = delegate(Rectangle? rect)
		{
			scrollBar.OnInvalidate?.Invoke();
			if (rect.HasValue)
			{
				((Control)control2).Invalidate(rect.Value);
			}
			else
			{
				((Control)control2).Invalidate();
			}
		};
		ChangeSize = delegate
		{
			control2.IOnSizeChanged();
		};
		EnabledX = enabledX;
		EnabledY = enabledY;
		Init();
	}

	public ScrollBar(Action change, Action<Rectangle?> invalidate, bool enabledY = true, bool enabledX = false)
	{
		EnabledX = enabledX;
		EnabledY = enabledY;
		ChangeSize = change;
		Invalidate = invalidate;
		Init();
	}

	private void Init()
	{
		SIZE = (int)(16f * Config.Dpi);
		SIZE_BAR = (int)(6f * Config.Dpi);
		SIZE_MINIY = (int)((float)Config.ScrollMinSizeY * Config.Dpi);
	}

	public void Clear()
	{
		valueX = (valueY = 0);
	}

	public void SizeChange(Rectangle rect)
	{
		RectX = new Rectangle(rect.X, rect.Bottom - SIZE, rect.Width, SIZE);
		RectY = new Rectangle(rect.Right - SIZE, rect.Top, SIZE, rect.Height);
		SetShow(oldx, oldy);
	}

	public void SetVrSize(int x, int y)
	{
		oldx = x;
		oldy = y;
		SetShow(oldx, oldy);
	}

	private void SetShow(int x, int y)
	{
		SetShow(x, RectX.Width, y, RectY.Height);
	}

	private void SetShow(int x, int x2, int y, int y2)
	{
		string text = x + "_" + x2;
		string text2 = y + "_" + y2;
		if (show_oldx == text && show_oldy == text2)
		{
			return;
		}
		show_oldx = text;
		show_oldy = text2;
		if (x2 > 0 && x > 0 && x > x2)
		{
			maxX = x;
			ShowX = maxX > x2;
			if (ShowX)
			{
				int num = x - x2;
				if (valueX > num)
				{
					ValueX = num;
				}
			}
		}
		else
		{
			maxX = (valueX = 0);
			ShowX = false;
		}
		if (y2 > 0 && y > 0 && y > y2)
		{
			maxY = y;
			ShowY = maxY > y2;
			if (ShowY)
			{
				int num2 = y - y2;
				if (valueY > num2)
				{
					ValueY = num2;
				}
			}
		}
		else
		{
			maxY = (valueY = 0);
			ShowY = false;
		}
		if (showX && showY)
		{
			maxX += SIZE;
			maxY += SIZE;
		}
	}

	public virtual void Paint(Canvas g)
	{
		Paint(g, Colour.TextBase.Get("ScrollBar"));
	}

	public virtual void Paint(Canvas g, Color baseColor)
	{
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Expected O, but got Unknown
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Expected O, but got Unknown
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Expected O, but got Unknown
		if (Config.ScrollBarHide)
		{
			if (showY && showX)
			{
				if (Back && (hoverY || AnimationHoverY || hoverX || AnimationHoverX))
				{
					if (hoverY || AnimationHoverY)
					{
						SolidBrush val = BackBrushY(baseColor);
						try
						{
							if (Radius > 0)
							{
								float radius = (float)Radius * Config.Dpi;
								GraphicsPath val2 = RectY.RoundPath(radius, TL: false, TR: true, RB, BL: false);
								try
								{
									g.Fill((Brush)(object)val, val2);
								}
								finally
								{
									((IDisposable)val2)?.Dispose();
								}
							}
							else
							{
								g.Fill((Brush)(object)val, RectY);
							}
						}
						finally
						{
							((IDisposable)val)?.Dispose();
						}
					}
					else
					{
						SolidBrush val3 = BackBrushX(baseColor);
						try
						{
							if (RB && Radius > 0)
							{
								float radius2 = (float)Radius * Config.Dpi;
								GraphicsPath val4 = new Rectangle(RectX.X, RectX.Y, RectX.Width, RectX.Height).RoundPath(radius2, TL: false, TR: false, BR: true, BL: true);
								try
								{
									g.Fill((Brush)(object)val3, val4);
								}
								finally
								{
									((IDisposable)val4)?.Dispose();
								}
							}
							else
							{
								g.Fill((Brush)(object)val3, new Rectangle(RectX.X, RectX.Y, RectX.Width, RectX.Height));
							}
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
				}
				PaintX(g, baseColor);
				PaintY(g, baseColor);
			}
			else if (showY)
			{
				if (Back && (hoverY || AnimationHoverY))
				{
					SolidBrush val5 = BackBrushY(baseColor);
					try
					{
						if (Radius > 0)
						{
							float radius3 = (float)Radius * Config.Dpi;
							GraphicsPath val6 = RectY.RoundPath(radius3, TL: false, TR: true, RB, BL: false);
							try
							{
								g.Fill((Brush)(object)val5, val6);
							}
							finally
							{
								((IDisposable)val6)?.Dispose();
							}
						}
						else
						{
							g.Fill((Brush)(object)val5, RectY);
						}
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
				}
				PaintY(g, baseColor);
			}
			else
			{
				if (!showX)
				{
					return;
				}
				if (Back && (hoverX || AnimationHoverX))
				{
					SolidBrush val7 = BackBrushX(baseColor);
					try
					{
						if (RB && Radius > 0)
						{
							float radius4 = (float)Radius * Config.Dpi;
							GraphicsPath val8 = new Rectangle(RectX.X, RectX.Y, RectX.Width, RectX.Height).RoundPath(radius4, TL: false, TR: false, BR: true, BL: true);
							try
							{
								g.Fill((Brush)(object)val7, val8);
							}
							finally
							{
								((IDisposable)val8)?.Dispose();
							}
						}
						else
						{
							g.Fill((Brush)(object)val7, RectX);
						}
					}
					finally
					{
						((IDisposable)val7)?.Dispose();
					}
				}
				PaintX(g, baseColor);
			}
		}
		else if (showY && showX)
		{
			if (Back)
			{
				SolidBrush val9 = new SolidBrush(Color.FromArgb(10, baseColor));
				try
				{
					Rectangle rect = new Rectangle(RectX.X, RectX.Y, RectX.Width - RectY.Width, RectX.Height);
					if (Radius > 0)
					{
						float radius5 = (float)Radius * Config.Dpi;
						GraphicsPath val10 = RectY.RoundPath(radius5, TL: false, TR: true, RB, BL: false);
						try
						{
							g.Fill((Brush)(object)val9, val10);
						}
						finally
						{
							((IDisposable)val10)?.Dispose();
						}
						if (RB)
						{
							GraphicsPath val11 = rect.RoundPath(radius5, TL: false, TR: false, BR: false, BL: true);
							try
							{
								g.Fill((Brush)(object)val9, val11);
							}
							finally
							{
								((IDisposable)val11)?.Dispose();
							}
						}
						else
						{
							g.Fill((Brush)(object)val9, rect);
						}
					}
					else
					{
						g.Fill((Brush)(object)val9, RectY);
						g.Fill((Brush)(object)val9, rect);
					}
				}
				finally
				{
					((IDisposable)val9)?.Dispose();
				}
			}
			PaintX(g, baseColor);
			PaintY(g, baseColor);
		}
		else if (showY)
		{
			if (Back)
			{
				SolidBrush val12 = new SolidBrush(Color.FromArgb(10, baseColor));
				try
				{
					if (Radius > 0)
					{
						float radius6 = (float)Radius * Config.Dpi;
						GraphicsPath val13 = RectY.RoundPath(radius6, TL: false, TR: true, RB, BL: false);
						try
						{
							g.Fill((Brush)(object)val12, val13);
						}
						finally
						{
							((IDisposable)val13)?.Dispose();
						}
					}
					else
					{
						g.Fill((Brush)(object)val12, RectY);
					}
				}
				finally
				{
					((IDisposable)val12)?.Dispose();
				}
			}
			PaintY(g, baseColor);
		}
		else
		{
			if (!showX)
			{
				return;
			}
			if (Back)
			{
				SolidBrush val14 = new SolidBrush(Color.FromArgb(10, baseColor));
				try
				{
					if (RB && Radius > 0)
					{
						float radius7 = (float)Radius * Config.Dpi;
						GraphicsPath val15 = new Rectangle(RectX.X, RectX.Y, RectX.Width, RectX.Height).RoundPath(radius7, TL: false, TR: false, BR: true, BL: true);
						try
						{
							g.Fill((Brush)(object)val14, val15);
						}
						finally
						{
							((IDisposable)val15)?.Dispose();
						}
					}
					else
					{
						g.Fill((Brush)(object)val14, RectX);
					}
				}
				finally
				{
					((IDisposable)val14)?.Dispose();
				}
			}
			PaintX(g, baseColor);
		}
	}

	private SolidBrush BackBrushY(Color color)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (!AnimationHoverY)
		{
			return new SolidBrush(Color.FromArgb(10, color));
		}
		return new SolidBrush(Color.FromArgb((int)(10f * AnimationHoverYValue), color));
	}

	private SolidBrush BackBrushX(Color color)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (!AnimationHoverX)
		{
			return new SolidBrush(Color.FromArgb(10, color));
		}
		return new SolidBrush(Color.FromArgb((int)(10f * AnimationHoverXValue), color));
	}

	private void PaintY(Canvas g, Color color)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (AnimationHoverY)
		{
			SolidBrush val = new SolidBrush(Color.FromArgb(110 + (int)(31f * AnimationHoverYValue), color));
			try
			{
				RectangleF rect = RectSliderY();
				GraphicsPath val2 = rect.RoundPath(rect.Width);
				try
				{
					g.Fill((Brush)(object)val, val2);
					return;
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
		int alpha = ((!SliderDownY) ? (hoverY ? 141 : 110) : 172);
		RectangleF rect2 = RectSliderY();
		GraphicsPath val3 = rect2.RoundPath(rect2.Width);
		try
		{
			g.Fill(Color.FromArgb(alpha, color), val3);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	private void PaintX(Canvas g, Color color)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (AnimationHoverX)
		{
			SolidBrush val = new SolidBrush(Color.FromArgb(110 + (int)(31f * AnimationHoverXValue), color));
			try
			{
				RectangleF rect = RectSliderX();
				GraphicsPath val2 = rect.RoundPath(rect.Height);
				try
				{
					g.Fill((Brush)(object)val, val2);
					return;
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
		int alpha = ((!SliderDownX) ? (hoverX ? 141 : 110) : 172);
		RectangleF rect2 = RectSliderX();
		GraphicsPath val3 = rect2.RoundPath(rect2.Height);
		try
		{
			g.Fill(Color.FromArgb(alpha, color), val3);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	private RectangleF RectSliderX()
	{
		float num = RectX.Width - (showY ? SIZE : 0);
		float num2 = (float)RectX.Width * 1f / (float)maxX * num;
		if (num2 < (float)SIZE)
		{
			num2 = SIZE;
		}
		float num3 = (float)valueX * 1f / (float)(maxX - RectX.Width) * (num - num2);
		float num4 = (float)(RectX.Height - SIZE_BAR) / 2f;
		return new RectangleF((float)RectX.X + num3 + num4, (float)RectX.Y + num4, num2 - num4 * 2f, SIZE_BAR);
	}

	private RectangleF RectSliderY()
	{
		float num = (RectY.Width - SIZE_BAR) / 2;
		float num2 = (float)SIZE_MINIY + num * 2f;
		float num3 = RectY.Height - (showX ? SIZE : 0);
		float num4 = (float)RectY.Height * 1f / (float)maxY * num3;
		if (num4 < num2)
		{
			num4 = num2;
		}
		else if (num4 < (float)SIZE)
		{
			num4 = SIZE;
		}
		float num5 = (float)valueY * 1f / (float)(maxY - RectY.Height) * (num3 - num4);
		return new RectangleF((float)RectY.X + num, (float)RectY.Y + num5 + num, SIZE_BAR, num4 - num * 2f);
	}

	private RectangleF RectSliderFullX()
	{
		float num = RectX.Width - (showY ? SIZE : 0);
		float num2 = (float)RectX.Width * 1f / (float)maxX * num;
		if (num2 < (float)SIZE)
		{
			num2 = SIZE;
		}
		float num3 = (float)valueX * 1f / (float)(maxX - RectX.Width) * (num - num2);
		return new RectangleF((float)RectX.X + num3, RectX.Y, num2, RectX.Height);
	}

	private RectangleF RectSliderFullY()
	{
		float num = RectY.Height - (showX ? SIZE : 0);
		float num2 = (float)RectY.Height * 1f / (float)maxY * num;
		if (num2 < (float)SIZE_MINIY)
		{
			num2 = SIZE_MINIY;
		}
		else if (num2 < (float)SIZE)
		{
			num2 = SIZE;
		}
		float num3 = (float)valueY * 1f / (float)(maxY - RectY.Height) * (num - num2);
		return new RectangleF(RectY.X, (float)RectY.Y + num3, RectY.Width, num2);
	}

	public bool MouseDownX(Point e)
	{
		if (EnabledX && ShowX && RectX.Contains(e))
		{
			old = e;
			RectangleF rectangleF = RectSliderFullX();
			if (!rectangleF.Contains(e))
			{
				float num = RectX.Width - (showY ? SIZE : 0);
				float num2 = ((float)e.X - rectangleF.Width / 2f) / num;
				ValueX = (int)Math.Round(num2 * (float)maxX);
				SliderX = RectSliderFullX().X;
			}
			else
			{
				SliderX = rectangleF.X;
			}
			SliderDownX = true;
			Window.CanHandMessage = false;
			return false;
		}
		return true;
	}

	public bool MouseDownY(Point e)
	{
		if (EnabledY && ShowY && RectY.Contains(e))
		{
			old = e;
			RectangleF rectangleF = RectSliderFullY();
			if (!rectangleF.Contains(e))
			{
				float num = RectY.Height - (showX ? SIZE : 0);
				float num2 = ((float)e.Y - rectangleF.Height / 2f) / num;
				ValueY = (int)Math.Round(num2 * (float)maxY);
				SliderY = RectSliderFullY().Y;
			}
			else
			{
				SliderY = rectangleF.Y;
			}
			SliderDownY = true;
			Window.CanHandMessage = false;
			return false;
		}
		return true;
	}

	public bool MouseMoveX(Point e)
	{
		if (EnabledX && !SliderDownY)
		{
			if (SliderDownX)
			{
				HoverX = true;
				RectangleF rectangleF = RectSliderFullX();
				float num = RectX.Width - (showY ? SIZE : 0);
				float num2 = SliderX + (float)e.X - (float)old.X;
				ValueX = (int)(num2 / (num - rectangleF.Width) * (float)(maxX - RectX.Width));
				return false;
			}
			if (ShowX && RectX.Contains(e))
			{
				HoverX = true;
				return false;
			}
			HoverX = false;
		}
		return true;
	}

	public bool MouseMoveY(Point e)
	{
		if (EnabledY && !SliderDownX)
		{
			if (SliderDownY)
			{
				HoverY = true;
				RectangleF rectangleF = RectSliderFullY();
				float num = RectY.Height - (showX ? SIZE : 0);
				float num2 = SliderY + (float)e.Y - (float)old.Y;
				ValueY = (int)(num2 / (num - rectangleF.Height) * (float)(maxY - RectY.Height));
				return false;
			}
			if (ShowY && RectY.Contains(e))
			{
				HoverY = true;
				return false;
			}
			HoverY = false;
		}
		return true;
	}

	public bool MouseUpX()
	{
		if (SliderDownX)
		{
			SliderDownX = false;
			Window.CanHandMessage = true;
			return false;
		}
		return true;
	}

	public bool MouseUpY()
	{
		if (SliderDownY)
		{
			SliderDownY = false;
			Window.CanHandMessage = true;
			return false;
		}
		return true;
	}

	public bool MouseWheelX(int delta)
	{
		if (EnabledX && ShowX && delta != 0)
		{
			int num = (ValueX -= delta);
			if (ValueX != num)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool MouseWheelY(int delta)
	{
		if (EnabledY && ShowY && delta != 0)
		{
			int num = (ValueY -= delta);
			if (ValueY != num)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool MouseWheel(int delta)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((Control.ModifierKeys & 0x10000) == 65536 && EnabledX && ShowX)
		{
			if (delta != 0)
			{
				ValueX -= delta;
				return true;
			}
		}
		else if (EnabledY)
		{
			if (ShowY && delta != 0)
			{
				ValueY -= delta;
				return true;
			}
		}
		else if (EnabledX && ShowX && delta != 0)
		{
			ValueX -= delta;
			return true;
		}
		return false;
	}

	public void Leave()
	{
		bool flag2 = (HoverY = false);
		HoverX = flag2;
	}

	public void SetVrSize(int len)
	{
		if (EnabledY)
		{
			SetVrSize(oldx, len);
		}
		else
		{
			SetVrSize(len, oldy);
		}
	}

	public bool MouseDown(Point e)
	{
		if (EnabledY)
		{
			return MouseDownY(e);
		}
		return MouseDownX(e);
	}

	public bool MouseMove(Point e)
	{
		if (EnabledY)
		{
			return MouseMoveY(e);
		}
		return MouseMoveX(e);
	}

	public bool MouseUp()
	{
		if (EnabledY)
		{
			return MouseUpY();
		}
		return MouseUpX();
	}

	public void Dispose()
	{
		ThreadHoverY?.Dispose();
		ThreadHoverX?.Dispose();
	}
}

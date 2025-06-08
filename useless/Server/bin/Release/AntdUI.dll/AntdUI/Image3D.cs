using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[ToolboxItem(true)]
public class Image3D : IControl, ShadowConfig
{
	private Color back = Color.Transparent;

	private int radius;

	private bool round;

	private Image? image;

	private TFit imageFit = TFit.Cover;

	private int shadow;

	private float shadowOpacity = 0.3f;

	private int shadowOffsetX;

	private int shadowOffsetY;

	private Bitmap? run;

	[Description("背景颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "Transparent")]
	public Color Back
	{
		get
		{
			return back;
		}
		set
		{
			if (!(back == value))
			{
				back = value;
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

	[Description("图片")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Image
	{
		get
		{
			return image;
		}
		set
		{
			Image value2 = value;
			if (image == value2)
			{
				return;
			}
			if (image != null && value2 != null)
			{
				int t = Animation.TotalFrames(Speed, Duration);
				float _radius = (float)radius * Config.Dpi;
				ITask.Run(delegate
				{
					//IL_0013: Unknown result type (might be due to invalid IL or missing references)
					//IL_0031: Unknown result type (might be due to invalid IL or missing references)
					//IL_0037: Expected O, but got Unknown
					//IL_0045: Unknown result type (might be due to invalid IL or missing references)
					//IL_004b: Expected O, but got Unknown
					Rectangle clientRectangle = ((Control)this).ClientRectangle;
					Rectangle rectangle = clientRectangle.PaddingRect(((Control)this).Padding);
					Bitmap val = new Bitmap(clientRectangle.Width, clientRectangle.Height);
					try
					{
						Bitmap val2 = new Bitmap(clientRectangle.Width, clientRectangle.Height);
						try
						{
							using (Canvas canvas = Graphics.FromImage((Image)(object)val).High())
							{
								if (shadow > 0 && shadowOpacity > 0f)
								{
									canvas.PaintShadow(this, clientRectangle, rectangle, _radius, round);
								}
								canvas.Image(rectangle, image, imageFit, _radius, round);
							}
							using (Canvas canvas2 = Graphics.FromImage((Image)(object)val2).High())
							{
								if (shadow > 0 && shadowOpacity > 0f)
								{
									canvas2.PaintShadow(this, clientRectangle, rectangle, _radius, round);
								}
								canvas2.Image(rectangle, value2, imageFit, _radius, round);
							}
							List<Bitmap> list = new List<Bitmap>(t);
							if (Vertical)
							{
								for (int i = 0; i < t; i++)
								{
									float num = Animation.Animate(i + 1, t, 180f, AnimationType.Ease);
									Cube cube = new Cube(((Image)val).Width, ((Image)val).Height, 1);
									if (num > 90f)
									{
										num -= 180f;
										cube.RotateX = num;
										cube.calcCube(clientRectangle.Location);
										Bitmap val3 = cube.ToBitmap(val2);
										((Image)val3).Tag = cube.CentreX();
										list.Add(val3);
									}
									else
									{
										cube.RotateX = num;
										cube.calcCube(clientRectangle.Location);
										Bitmap val4 = cube.ToBitmap(val);
										((Image)val4).Tag = cube.CentreX();
										list.Add(val4);
									}
								}
							}
							else
							{
								for (int j = 0; j < t; j++)
								{
									float num2 = Animation.Animate(j + 1, t, 180f, AnimationType.Ease);
									Cube cube2 = new Cube(((Image)val).Width, ((Image)val).Height, 1);
									if (num2 > 90f)
									{
										num2 -= 180f;
										cube2.RotateY = num2;
										cube2.calcCube(clientRectangle.Location);
										Bitmap val5 = cube2.ToBitmap(val2);
										((Image)val5).Tag = cube2.CentreY();
										list.Add(val5);
									}
									else
									{
										cube2.RotateY = num2;
										cube2.calcCube(clientRectangle.Location);
										Bitmap val6 = cube2.ToBitmap(val);
										((Image)val6).Tag = cube2.CentreY();
										list.Add(val6);
									}
								}
							}
							for (int k = 0; k < list.Count; k++)
							{
								run = list[k];
								((Control)this).Invalidate();
								Thread.Sleep(Speed);
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
					Bitmap? obj = run;
					if (obj != null)
					{
						((Image)obj).Dispose();
					}
					run = null;
					image = value2;
					((Control)this).Invalidate();
				});
			}
			else
			{
				image = value2;
				((Control)this).Invalidate();
			}
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

	[Description("是否竖向")]
	[Category("动画")]
	[DefaultValue(false)]
	public bool Vertical { get; set; }

	[Description("速度")]
	[Category("动画")]
	[DefaultValue(10)]
	public int Speed { get; set; } = 10;


	[Description("速度")]
	[Category("动画")]
	[DefaultValue(400)]
	public int Duration { get; set; } = 400;


	[Description("阴影")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Shadow
	{
		get
		{
			return shadow;
		}
		set
		{
			if (shadow != value)
			{
				shadow = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("阴影颜色")]
	[Category("阴影")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ShadowColor { get; set; }

	[Description("阴影透明度")]
	[Category("阴影")]
	[DefaultValue(0.3f)]
	public float ShadowOpacity
	{
		get
		{
			return shadowOpacity;
		}
		set
		{
			if (shadowOpacity != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				shadowOpacity = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("阴影偏移X")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetX
	{
		get
		{
			return shadowOffsetX;
		}
		set
		{
			if (shadowOffsetX != value)
			{
				shadowOffsetX = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("阴影偏移Y")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetY
	{
		get
		{
			return shadowOffsetY;
		}
		set
		{
			if (shadowOffsetY != value)
			{
				shadowOffsetY = value;
				((Control)this).Invalidate();
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return;
		}
		if (image == null)
		{
			((Control)this).OnPaint(e);
			return;
		}
		Canvas canvas = e.Graphics.High();
		Rectangle rectangle = clientRectangle.PaddingRect(((Control)this).Padding);
		float num = (float)radius * Config.Dpi;
		FillRect(canvas, rectangle, back, num, round);
		if (run != null && ((Image)run).Tag is PointF pointF)
		{
			canvas.Image((Image)(object)run, pointF.X, pointF.Y, ((Image)run).Width, ((Image)run).Height);
		}
		else
		{
			if (shadow > 0 && shadowOpacity > 0f)
			{
				canvas.PaintShadow(this, clientRectangle, rectangle, num, round);
			}
			canvas.Image(rectangle, image, imageFit, num, round);
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void FillRect(Canvas g, RectangleF rect, Color color, float radius, bool round)
	{
		if (round)
		{
			g.FillEllipse(color, rect);
			return;
		}
		if (radius > 0f)
		{
			GraphicsPath val = rect.RoundPath(radius);
			try
			{
				g.Fill(color, val);
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		g.Fill(color, rect);
	}
}

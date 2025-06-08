using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using WinFormAnimation;

namespace Server.Helper
{
	// Token: 0x0200006D RID: 109
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(CircularProgressBar), "CircularProgressBar.bmp")]
	[DefaultBindingProperty("Value")]
	public class CircularProgressBar : ProgressBar
	{
		// Token: 0x06000219 RID: 537 RVA: 0x00022040 File Offset: 0x00020240
		public CircularProgressBar()
		{
			base.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
			this._animator = (base.DesignMode ? null : new Animator());
			this.AnimationFunction = KnownAnimationFunctions.Liner;
			this.AnimationSpeed = 500;
			base.MarqueeAnimationSpeed = 2000;
			this.StartAngle = 270;
			this._lastValue = base.Value;
			this.BackColor = Color.Transparent;
			this.ForeColor = Color.FromArgb(64, 64, 64);
			this.DoubleBuffered = true;
			this.Font = new Font(this.Font.FontFamily, 72f, FontStyle.Bold);
			this.SecondaryFont = new Font(this.Font.FontFamily, (float)((double)this.Font.Size * 0.5), FontStyle.Regular);
			this.OuterMargin = -25;
			this.OuterWidth = 26;
			this.OuterColor = Color.Gray;
			this.ProgressWidth = 25;
			this.ProgressColor = Color.FromArgb(255, 128, 0);
			this.InnerMargin = 2;
			this.InnerWidth = -1;
			this.InnerColor = Color.FromArgb(224, 224, 224);
			this.TextMargin = new Padding(8, 8, 0, 0);
			base.Value = 68;
			this.SuperscriptMargin = new Padding(10, 35, 0, 0);
			this.SuperscriptColor = Color.FromArgb(166, 166, 166);
			this.SuperscriptText = "°C";
			this.SubscriptMargin = new Padding(10, -35, 0, 0);
			this.SubscriptColor = Color.FromArgb(166, 166, 166);
			this.SubscriptText = ".23";
			base.Size = new Size(320, 320);
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600021A RID: 538 RVA: 0x0002220F File Offset: 0x0002040F
		// (set) Token: 0x0600021B RID: 539 RVA: 0x00022217 File Offset: 0x00020417
		[Category("Behavior")]
		public KnownAnimationFunctions AnimationFunction
		{
			get
			{
				return this._knownAnimationFunction;
			}
			set
			{
				this._animationFunction = AnimationFunctions.FromKnown(value);
				this._knownAnimationFunction = value;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600021C RID: 540 RVA: 0x0002222C File Offset: 0x0002042C
		// (set) Token: 0x0600021D RID: 541 RVA: 0x00022234 File Offset: 0x00020434
		[Category("Behavior")]
		public int AnimationSpeed { get; set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600021E RID: 542 RVA: 0x0002223D File Offset: 0x0002043D
		// (set) Token: 0x0600021F RID: 543 RVA: 0x00022245 File Offset: 0x00020445
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public AnimationFunctions.Function CustomAnimationFunction
		{
			private get
			{
				return this._animationFunction;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				this._knownAnimationFunction = KnownAnimationFunctions.None;
				this._animationFunction = value;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000220 RID: 544 RVA: 0x00022263 File Offset: 0x00020463
		// (set) Token: 0x06000221 RID: 545 RVA: 0x0002226B File Offset: 0x0002046B
		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(true)]
		public override Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000222 RID: 546 RVA: 0x00022274 File Offset: 0x00020474
		// (set) Token: 0x06000223 RID: 547 RVA: 0x0002227C File Offset: 0x0002047C
		[Category("Appearance")]
		public Color InnerColor { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000224 RID: 548 RVA: 0x00022285 File Offset: 0x00020485
		// (set) Token: 0x06000225 RID: 549 RVA: 0x0002228D File Offset: 0x0002048D
		[Category("Layout")]
		public int InnerMargin { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000226 RID: 550 RVA: 0x00022296 File Offset: 0x00020496
		// (set) Token: 0x06000227 RID: 551 RVA: 0x0002229E File Offset: 0x0002049E
		[Category("Layout")]
		public int InnerWidth { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000228 RID: 552 RVA: 0x000222A7 File Offset: 0x000204A7
		// (set) Token: 0x06000229 RID: 553 RVA: 0x000222AF File Offset: 0x000204AF
		[Category("Appearance")]
		public Color OuterColor { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600022A RID: 554 RVA: 0x000222B8 File Offset: 0x000204B8
		// (set) Token: 0x0600022B RID: 555 RVA: 0x000222C0 File Offset: 0x000204C0
		[Category("Layout")]
		public int OuterMargin { get; set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600022C RID: 556 RVA: 0x000222C9 File Offset: 0x000204C9
		// (set) Token: 0x0600022D RID: 557 RVA: 0x000222D1 File Offset: 0x000204D1
		[Category("Layout")]
		public int OuterWidth { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600022E RID: 558 RVA: 0x000222DA File Offset: 0x000204DA
		// (set) Token: 0x0600022F RID: 559 RVA: 0x000222E2 File Offset: 0x000204E2
		[Category("Appearance")]
		public Color ProgressColor { get; set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000230 RID: 560 RVA: 0x000222EB File Offset: 0x000204EB
		// (set) Token: 0x06000231 RID: 561 RVA: 0x000222F3 File Offset: 0x000204F3
		[Category("Layout")]
		public int ProgressWidth { get; set; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000232 RID: 562 RVA: 0x000222FC File Offset: 0x000204FC
		// (set) Token: 0x06000233 RID: 563 RVA: 0x00022304 File Offset: 0x00020504
		[Category("Appearance")]
		public Font SecondaryFont { get; set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000234 RID: 564 RVA: 0x0002230D File Offset: 0x0002050D
		// (set) Token: 0x06000235 RID: 565 RVA: 0x00022315 File Offset: 0x00020515
		[Category("Layout")]
		public int StartAngle { get; set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000236 RID: 566 RVA: 0x0002231E File Offset: 0x0002051E
		// (set) Token: 0x06000237 RID: 567 RVA: 0x00022326 File Offset: 0x00020526
		[Category("Appearance")]
		public Color SubscriptColor { get; set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000238 RID: 568 RVA: 0x0002232F File Offset: 0x0002052F
		// (set) Token: 0x06000239 RID: 569 RVA: 0x00022337 File Offset: 0x00020537
		[Category("Layout")]
		public Padding SubscriptMargin { get; set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600023A RID: 570 RVA: 0x00022340 File Offset: 0x00020540
		// (set) Token: 0x0600023B RID: 571 RVA: 0x00022348 File Offset: 0x00020548
		[Category("Appearance")]
		public string SubscriptText { get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600023C RID: 572 RVA: 0x00022351 File Offset: 0x00020551
		// (set) Token: 0x0600023D RID: 573 RVA: 0x00022359 File Offset: 0x00020559
		[Category("Appearance")]
		public Color SuperscriptColor { get; set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600023E RID: 574 RVA: 0x00022362 File Offset: 0x00020562
		// (set) Token: 0x0600023F RID: 575 RVA: 0x0002236A File Offset: 0x0002056A
		[Category("Layout")]
		public Padding SuperscriptMargin { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000240 RID: 576 RVA: 0x00022373 File Offset: 0x00020573
		// (set) Token: 0x06000241 RID: 577 RVA: 0x0002237B File Offset: 0x0002057B
		[Category("Appearance")]
		public string SuperscriptText { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000242 RID: 578 RVA: 0x00022384 File Offset: 0x00020584
		// (set) Token: 0x06000243 RID: 579 RVA: 0x0002238C File Offset: 0x0002058C
		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(true)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000244 RID: 580 RVA: 0x00022395 File Offset: 0x00020595
		// (set) Token: 0x06000245 RID: 581 RVA: 0x0002239D File Offset: 0x0002059D
		[Category("Layout")]
		public Padding TextMargin { get; set; }

		// Token: 0x06000246 RID: 582 RVA: 0x000223A6 File Offset: 0x000205A6
		private static PointF AddPoint(PointF p, int v)
		{
			p.X += (float)v;
			p.Y += (float)v;
			return p;
		}

		// Token: 0x06000247 RID: 583 RVA: 0x000223C9 File Offset: 0x000205C9
		private static SizeF AddSize(SizeF s, int v)
		{
			s.Height += (float)v;
			s.Width += (float)v;
			return s;
		}

		// Token: 0x06000248 RID: 584 RVA: 0x000223EC File Offset: 0x000205EC
		private static Rectangle ToRectangle(RectangleF rect)
		{
			return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
		}

		// Token: 0x06000249 RID: 585 RVA: 0x00022413 File Offset: 0x00020613
		protected override void OnLocationChanged(EventArgs e)
		{
			base.OnLocationChanged(e);
			base.Invalidate();
		}

		// Token: 0x0600024A RID: 586 RVA: 0x00022424 File Offset: 0x00020624
		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				if (!base.DesignMode)
				{
					if (base.Style == ProgressBarStyle.Marquee)
					{
						ProgressBarStyle? lastStyle = this._lastStyle;
						ProgressBarStyle style = base.Style;
						this.InitializeMarquee(!(lastStyle.GetValueOrDefault() == style & lastStyle != null));
					}
					else
					{
						ProgressBarStyle? lastStyle = this._lastStyle;
						ProgressBarStyle style = base.Style;
						this.InitializeContinues(!(lastStyle.GetValueOrDefault() == style & lastStyle != null));
					}
					this._lastStyle = new ProgressBarStyle?(base.Style);
				}
				if (this._backBrush == null)
				{
					this.RecreateBackgroundBrush();
				}
				this.StartPaint(e.Graphics);
			}
			catch
			{
			}
		}

		// Token: 0x0600024B RID: 587 RVA: 0x000224D8 File Offset: 0x000206D8
		protected override void OnParentBackColorChanged(EventArgs e)
		{
			this.RecreateBackgroundBrush();
		}

		// Token: 0x0600024C RID: 588 RVA: 0x000224E0 File Offset: 0x000206E0
		protected override void OnParentBackgroundImageChanged(EventArgs e)
		{
			this.RecreateBackgroundBrush();
		}

		// Token: 0x0600024D RID: 589 RVA: 0x000224E8 File Offset: 0x000206E8
		protected override void OnParentChanged(EventArgs e)
		{
			if (base.Parent != null)
			{
				base.Parent.Invalidated -= this.ParentOnInvalidated;
				base.Parent.Resize -= this.ParentOnResize;
			}
			base.OnParentChanged(e);
			if (base.Parent != null)
			{
				base.Parent.Invalidated += this.ParentOnInvalidated;
				base.Parent.Resize += this.ParentOnResize;
			}
		}

		// Token: 0x0600024E RID: 590 RVA: 0x0002256C File Offset: 0x0002076C
		protected override void OnStyleChanged(EventArgs e)
		{
			base.OnStyleChanged(e);
			base.Invalidate();
		}

		// Token: 0x0600024F RID: 591 RVA: 0x0002257C File Offset: 0x0002077C
		protected virtual void InitializeContinues(bool firstTime)
		{
			if (this._lastValue == base.Value && !firstTime)
			{
				return;
			}
			this._lastValue = base.Value;
			this._animator.Stop();
			this._animatedStartAngle = null;
			if (this.AnimationSpeed <= 0)
			{
				this._animatedValue = new float?((float)base.Value);
				base.Invalidate();
				return;
			}
			this._animator.Paths = new Path(this._animatedValue ?? ((float)base.Value), (float)base.Value, (ulong)((long)this.AnimationSpeed), this.CustomAnimationFunction).ToArray();
			this._animator.Repeat = false;
			this._animator.Play(new SafeInvoker<float>(delegate(float v)
			{
				try
				{
					this._animatedValue = new float?(v);
					base.Invalidate();
				}
				catch
				{
					this._animator.Stop();
				}
			}, this));
		}

		// Token: 0x06000250 RID: 592 RVA: 0x00022654 File Offset: 0x00020854
		protected virtual void InitializeMarquee(bool firstTime)
		{
			if (!firstTime && (this._animator.ActivePath == null || (this._animator.ActivePath.Duration == (ulong)((long)base.MarqueeAnimationSpeed) && this._animator.ActivePath.Function == this.CustomAnimationFunction)))
			{
				return;
			}
			this._animator.Stop();
			this._animatedValue = null;
			if (this.AnimationSpeed <= 0)
			{
				this._animatedStartAngle = new int?(0);
				base.Invalidate();
				return;
			}
			this._animator.Paths = new Path(0f, 359f, (ulong)((long)base.MarqueeAnimationSpeed), this.CustomAnimationFunction).ToArray();
			this._animator.Repeat = true;
			this._animator.Play(new SafeInvoker<float>(delegate(float v)
			{
				try
				{
					this._animatedStartAngle = new int?((int)(v % 360f));
					base.Invalidate();
				}
				catch
				{
					this._animator.Stop();
				}
			}, this));
		}

		// Token: 0x06000251 RID: 593 RVA: 0x00022730 File Offset: 0x00020930
		protected virtual void ParentOnInvalidated(object sender, InvalidateEventArgs invalidateEventArgs)
		{
			this.RecreateBackgroundBrush();
		}

		// Token: 0x06000252 RID: 594 RVA: 0x00022738 File Offset: 0x00020938
		protected virtual void ParentOnResize(object sender, EventArgs eventArgs)
		{
			this.RecreateBackgroundBrush();
		}

		// Token: 0x06000253 RID: 595 RVA: 0x00022740 File Offset: 0x00020940
		protected virtual void RecreateBackgroundBrush()
		{
			lock (this)
			{
				Brush backBrush = this._backBrush;
				if (backBrush != null)
				{
					backBrush.Dispose();
				}
				this._backBrush = new SolidBrush(this.BackColor);
				if (this.BackColor.A != 255)
				{
					if (base.Parent != null && base.Parent.Width > 0 && base.Parent.Height > 0)
					{
						using (Bitmap bitmap = new Bitmap(base.Parent.Width, base.Parent.Height))
						{
							using (Graphics graphics = Graphics.FromImage(bitmap))
							{
								PaintEventArgs e = new PaintEventArgs(graphics, new Rectangle(new Point(0, 0), bitmap.Size));
								base.InvokePaintBackground(base.Parent, e);
								base.InvokePaint(base.Parent, e);
								if (this.BackColor.A > 0)
								{
									graphics.FillRectangle(this._backBrush, base.Bounds);
								}
							}
							this._backBrush = new TextureBrush(bitmap);
							((TextureBrush)this._backBrush).TranslateTransform((float)(-(float)base.Bounds.X), (float)(-(float)base.Bounds.Y));
							return;
						}
					}
					this._backBrush = new SolidBrush(Color.FromArgb(255, this.BackColor));
				}
			}
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0002290C File Offset: 0x00020B0C
		protected virtual void StartPaint(Graphics g)
		{
			try
			{
				lock (this)
				{
					g.TextRenderingHint = TextRenderingHint.AntiAlias;
					g.SmoothingMode = SmoothingMode.AntiAlias;
					PointF pointF = CircularProgressBar.AddPoint(Point.Empty, 2);
					SizeF sizeF = CircularProgressBar.AddSize(base.Size, -4);
					if (this.OuterWidth + this.OuterMargin < 0)
					{
						int num = Math.Abs(this.OuterWidth + this.OuterMargin);
						pointF = CircularProgressBar.AddPoint(Point.Empty, num);
						sizeF = CircularProgressBar.AddSize(base.Size, -2 * num);
					}
					if (this.OuterColor != Color.Empty && this.OuterColor != Color.Transparent && this.OuterWidth != 0)
					{
						g.FillEllipse(new SolidBrush(this.OuterColor), new RectangleF(pointF, sizeF));
						if (this.OuterWidth >= 0)
						{
							pointF = CircularProgressBar.AddPoint(pointF, this.OuterWidth);
							sizeF = CircularProgressBar.AddSize(sizeF, -2 * this.OuterWidth);
							g.FillEllipse(this._backBrush, new RectangleF(pointF, sizeF));
						}
					}
					pointF = CircularProgressBar.AddPoint(pointF, this.OuterMargin);
					sizeF = CircularProgressBar.AddSize(sizeF, -2 * this.OuterMargin);
					g.FillPie(new SolidBrush(this.ProgressColor), CircularProgressBar.ToRectangle(new RectangleF(pointF, sizeF)), (float)(this._animatedStartAngle ?? this.StartAngle), ((this._animatedValue ?? ((float)base.Value)) - (float)base.Minimum) / (float)(base.Maximum - base.Minimum) * 360f);
					if (this.ProgressWidth >= 0)
					{
						pointF = CircularProgressBar.AddPoint(pointF, this.ProgressWidth);
						sizeF = CircularProgressBar.AddSize(sizeF, -2 * this.ProgressWidth);
						g.FillEllipse(this._backBrush, new RectangleF(pointF, sizeF));
					}
					pointF = CircularProgressBar.AddPoint(pointF, this.InnerMargin);
					sizeF = CircularProgressBar.AddSize(sizeF, -2 * this.InnerMargin);
					if (this.InnerColor != Color.Empty && this.InnerColor != Color.Transparent && this.InnerWidth != 0)
					{
						g.FillEllipse(new SolidBrush(this.InnerColor), new RectangleF(pointF, sizeF));
						if (this.InnerWidth >= 0)
						{
							pointF = CircularProgressBar.AddPoint(pointF, this.InnerWidth);
							sizeF = CircularProgressBar.AddSize(sizeF, -2 * this.InnerWidth);
							g.FillEllipse(this._backBrush, new RectangleF(pointF, sizeF));
						}
					}
					if (!(this.Text == string.Empty))
					{
						pointF.X += (float)this.TextMargin.Left;
						pointF.Y += (float)this.TextMargin.Top;
						sizeF.Width -= (float)this.TextMargin.Right;
						sizeF.Height -= (float)this.TextMargin.Bottom;
						StringFormat format = new StringFormat((this.RightToLeft == RightToLeft.Yes) ? StringFormatFlags.DirectionRightToLeft : ((StringFormatFlags)0))
						{
							Alignment = StringAlignment.Center,
							LineAlignment = StringAlignment.Near
						};
						SizeF size = g.MeasureString(this.Text, this.Font);
						PointF location = new PointF(pointF.X + (sizeF.Width - size.Width) / 2f, pointF.Y + (sizeF.Height - size.Height) / 2f);
						if (this.SubscriptText != string.Empty || this.SuperscriptText != string.Empty)
						{
							float num2 = 0f;
							SizeF size2 = SizeF.Empty;
							SizeF size3 = SizeF.Empty;
							if (this.SuperscriptText != string.Empty)
							{
								size2 = g.MeasureString(this.SuperscriptText, this.SecondaryFont);
								num2 = Math.Max(size2.Width, num2);
								size2.Width -= (float)this.SuperscriptMargin.Right;
								size2.Height -= (float)this.SuperscriptMargin.Bottom;
							}
							if (this.SubscriptText != string.Empty)
							{
								size3 = g.MeasureString(this.SubscriptText, this.SecondaryFont);
								num2 = Math.Max(size3.Width, num2);
								size3.Width -= (float)this.SubscriptMargin.Right;
								size3.Height -= (float)this.SubscriptMargin.Bottom;
							}
							location.X -= num2 / 4f;
							if (this.SuperscriptText != string.Empty)
							{
								PointF location2 = new PointF(location.X + size.Width - size2.Width / 2f, location.Y - size2.Height * 0.85f);
								location2.X += (float)this.SuperscriptMargin.Left;
								location2.Y += (float)this.SuperscriptMargin.Top;
								g.DrawString(this.SuperscriptText, this.SecondaryFont, new SolidBrush(this.SuperscriptColor), new RectangleF(location2, size2), format);
							}
							if (this.SubscriptText != string.Empty)
							{
								PointF location3 = new PointF(location.X + size.Width - size3.Width / 2f, location.Y + size.Height * 0.85f);
								location3.X += (float)this.SubscriptMargin.Left;
								location3.Y += (float)this.SubscriptMargin.Top;
								g.DrawString(this.SubscriptText, this.SecondaryFont, new SolidBrush(this.SubscriptColor), new RectangleF(location3, size3), format);
							}
						}
						g.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), new RectangleF(location, size), format);
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x04000157 RID: 343
		private readonly Animator _animator;

		// Token: 0x04000158 RID: 344
		private int? _animatedStartAngle;

		// Token: 0x04000159 RID: 345
		private float? _animatedValue;

		// Token: 0x0400015A RID: 346
		private AnimationFunctions.Function _animationFunction;

		// Token: 0x0400015B RID: 347
		private Brush _backBrush;

		// Token: 0x0400015C RID: 348
		private KnownAnimationFunctions _knownAnimationFunction;

		// Token: 0x0400015D RID: 349
		private ProgressBarStyle? _lastStyle;

		// Token: 0x0400015E RID: 350
		private int _lastValue;
	}
}

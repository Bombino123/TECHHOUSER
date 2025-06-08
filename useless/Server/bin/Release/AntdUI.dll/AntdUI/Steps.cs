using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Steps 步骤条")]
[ToolboxItem(true)]
[DefaultProperty("Current")]
[DefaultEvent("ItemClick")]
public class Steps : IControl
{
	private Color? fore;

	private int current;

	private TStepState status = TStepState.Process;

	private bool vertical;

	private StepsItemCollection? items;

	private bool pauseLayout;

	private RectangleF[] splits = new RectangleF[0];

	private readonly StringFormat stringLeft = Helper.SF((StringAlignment)1, (StringAlignment)0);

	private readonly StringFormat stringCenter = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	[Description("文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeColor
	{
		get
		{
			return fore;
		}
		set
		{
			if (!(fore == value))
			{
				fore = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ForeColor");
			}
		}
	}

	[Description("指定当前步骤")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Current
	{
		get
		{
			return current;
		}
		set
		{
			if (current != value)
			{
				current = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Current");
			}
		}
	}

	[Description("指定当前步骤的状态")]
	[Category("外观")]
	[DefaultValue(TStepState.Process)]
	public TStepState Status
	{
		get
		{
			return status;
		}
		set
		{
			if (status != value)
			{
				status = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Status");
			}
		}
	}

	[Description("垂直方向")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Vertical
	{
		get
		{
			return vertical;
		}
		set
		{
			if (vertical != value)
			{
				vertical = value;
				ChangeList();
				((Control)this).Invalidate();
				OnPropertyChanged("Vertical");
			}
		}
	}

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(8)]
	public int Gap { get; set; } = 8;


	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("数据")]
	public StepsItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new StepsItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	[Browsable(false)]
	[Description("暂停布局")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool PauseLayout
	{
		get
		{
			return pauseLayout;
		}
		set
		{
			if (pauseLayout != value)
			{
				pauseLayout = value;
				if (!value)
				{
					ChangeList();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	[Description("点击项时发生")]
	[Category("行为")]
	public event StepsItemEventHandler? ItemClick;

	protected override void OnFontChanged(EventArgs e)
	{
		ChangeList();
		((Control)this).OnFontChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		ChangeList();
		((Control)this).OnSizeChanged(e);
	}

	internal void ChangeList()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Rectangle rect = ((Control)this).ClientRectangle.DeflateRect(((Control)this).Padding);
		if (pauseLayout || items == null || items.Count == 0 || rect.Width == 0 || rect.Height == 0)
		{
			return;
		}
		Helper.GDI(delegate(Canvas g)
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			int num = (int)((float)Gap * Config.Dpi);
			int num2 = (int)Config.Dpi;
			List<RectangleF> list = new List<RectangleF>(items.Count);
			Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 0.875f);
			try
			{
				int num3 = num * 2;
				if (vertical)
				{
					int num4 = rect.Height / items.Count;
					int num5 = 0;
					foreach (StepsItem item in items)
					{
						item.PARENT = this;
						item.TitleSize = g.MeasureString(item.Title, ((Control)this).Font);
						int num6 = (int)((float)item.TitleSize.Height * 1.6f);
						item.pen_w = (float)item.TitleSize.Height * 0.136f;
						_ = item.TitleSize.Width;
						int num7 = num6;
						if (item.showSub)
						{
							item.SubTitleSize = g.MeasureString(item.SubTitle, ((Control)this).Font);
							num7 += item.SubTitleSize.Height;
						}
						if (item.showDescription)
						{
							item.DescriptionSize = g.MeasureString(item.Description, val);
							_ = item.DescriptionSize.Width;
						}
						int num8 = rect.Y + num4 * num5 + num4 / 2;
						item.title_rect = new Rectangle(rect.X + num + num6, num8 - num7 / 2, item.TitleSize.Width, num7);
						_ = item.title_rect.Y;
						item.ico_rect = new Rectangle(rect.X, item.title_rect.Y + (item.title_rect.Height - num6) / 2, num6, num6);
						int num9 = item.title_rect.Width;
						int num10 = item.ico_rect.Height;
						int right = item.title_rect.Right;
						if (item.showSub)
						{
							item.subtitle_rect = new Rectangle(item.title_rect.X + item.TitleSize.Width, item.title_rect.Y, item.SubTitleSize.Width, num7);
							num9 = item.subtitle_rect.Width + item.title_rect.Width;
							right = item.subtitle_rect.Right;
						}
						if (item.showDescription)
						{
							item.description_rect = new Rectangle(item.title_rect.X, item.title_rect.Y + (num7 - item.TitleSize.Height) / 2 + item.TitleSize.Height + num / 2, item.DescriptionSize.Width, item.DescriptionSize.Height);
							if (item.description_rect.Width > num9)
							{
								num9 = item.description_rect.Width;
								right = item.description_rect.Right;
							}
							num10 += item.DescriptionSize.Height;
						}
						item.rect = new Rectangle(item.ico_rect.X - num, item.ico_rect.Y - num, right - item.ico_rect.X + num3, num10 + num3);
						if (num5 > 0)
						{
							StepsItem stepsItem2 = items[num5 - 1];
							if (stepsItem2 != null)
							{
								list.Add(new RectangleF((float)item.ico_rect.X + (float)(num6 - num2) / 2f, stepsItem2.ico_rect.Bottom + num, num2, item.ico_rect.Y - stepsItem2.ico_rect.Bottom - num3));
							}
						}
						num5++;
					}
				}
				else
				{
					int height;
					int num11 = MaxHeight(g, val, num, out height);
					int num12 = 0;
					int count = items.Count;
					int num13 = (rect.Width - num11) / count;
					int num14 = num13 - num;
					int num15 = rect.X + num13 / 2;
					count--;
					foreach (StepsItem item2 in items)
					{
						int num16 = item2.IconSize ?? ((int)((float)item2.TitleSize.Height * 1.6f));
						int num17 = rect.Y + (rect.Height - height) / 2;
						item2.ico_rect = new Rectangle(num15, num17 + (item2.TitleSize.Height - num16) / 2, num16, num16);
						item2.title_rect = new Rectangle(item2.ico_rect.Right + num, num17, item2.TitleSize.Width, item2.TitleSize.Height);
						int num18 = item2.ico_rect.Height;
						if (item2.showSub)
						{
							item2.subtitle_rect = new Rectangle(item2.title_rect.X + item2.TitleSize.Width, item2.title_rect.Y, item2.SubTitleSize.Width, item2.title_rect.Height);
						}
						if (item2.showDescription)
						{
							item2.description_rect = new Rectangle(item2.title_rect.X, item2.title_rect.Bottom + num / 2, item2.DescriptionSize.Width, item2.DescriptionSize.Height);
							num18 += item2.DescriptionSize.Height;
						}
						item2.rect = new Rectangle(item2.ico_rect.X - num, item2.ico_rect.Y - num, item2.ReadWidth + num3, num18 + num3);
						if (num14 > 0 && num12 != count)
						{
							list.Add(new RectangleF(item2.rect.Right - num, (float)item2.ico_rect.Y + (float)(item2.ico_rect.Height - num2) / 2f, num14, num2));
						}
						num15 += item2.ReadWidth + num13;
						num12++;
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			splits = list.ToArray();
		});
	}

	private int MaxHeight(Canvas g, Font font_description, int gap, out int height)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (StepsItem item in Items)
		{
			item.PARENT = this;
			item.TitleSize = g.MeasureString(item.Title, ((Control)this).Font);
			if (item.showSub)
			{
				item.SubTitleSize = g.MeasureString(item.SubTitle, ((Control)this).Font);
			}
			if (item.showDescription)
			{
				item.DescriptionSize = g.MeasureString(item.Description, font_description);
			}
			int num4 = item.IconSize ?? ((int)((float)item.TitleSize.Height * 1.6f));
			int num5 = item.TitleSize.Width + (item.showSub ? item.SubTitleSize.Width : 0);
			int num6 = (item.showDescription ? item.DescriptionSize.Width : 0);
			item.ReadWidth = num4 + gap + ((num5 > num6) ? num5 : num6);
			item.pen_w = (float)item.TitleSize.Height * 0.136f;
			num += item.ReadWidth;
			if (num2 == 0)
			{
				num2 = item.TitleSize.Height;
			}
			if (num3 == 0 && item.showDescription)
			{
				num3 = item.DescriptionSize.Height + gap / 2;
			}
		}
		height = num2 + num3;
		return num;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Expected O, but got Unknown
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
		SolidBrush val = new SolidBrush(fore ?? Colour.Text.Get("Steps"));
		try
		{
			SolidBrush val2 = new SolidBrush(Colour.PrimaryBg.Get("Steps"));
			try
			{
				SolidBrush val3 = new SolidBrush(Colour.Primary.Get("Steps"));
				try
				{
					SolidBrush val4 = new SolidBrush(Colour.PrimaryColor.Get("Steps"));
					try
					{
						SolidBrush val5 = new SolidBrush(Colour.BgBase.Get("Steps"));
						try
						{
							SolidBrush val6 = new SolidBrush(Colour.TextTertiary.Get("Steps"));
							try
							{
								SolidBrush val7 = new SolidBrush(Colour.TextSecondary.Get("Steps"));
								try
								{
									SolidBrush val8 = new SolidBrush(Colour.FillSecondary.Get("Steps"));
									try
									{
										Font val9 = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 0.875f);
										try
										{
											SolidBrush val10 = new SolidBrush(Colour.Split.Get("Steps"));
											try
											{
												for (int i = 0; i < splits.Length; i++)
												{
													if (i < current)
													{
														canvas.Fill((Brush)(object)val3, splits[i]);
													}
													else
													{
														canvas.Fill((Brush)(object)val10, splits[i]);
													}
												}
											}
											finally
											{
												((IDisposable)val10)?.Dispose();
											}
											int num = 0;
											foreach (StepsItem item in items)
											{
												if (item.Visible)
												{
													Color color;
													if (num == current)
													{
														switch (status)
														{
														case TStepState.Finish:
															canvas.String(item.Title, ((Control)this).Font, (Brush)(object)val, item.title_rect, stringLeft);
															canvas.String(item.SubTitle, ((Control)this).Font, (Brush)(object)val6, item.subtitle_rect, stringLeft);
															canvas.String(item.Description, val9, (Brush)(object)val6, item.description_rect, stringLeft);
															color = val3.Color;
															break;
														case TStepState.Wait:
															canvas.String(item.Title, ((Control)this).Font, (Brush)(object)val6, item.title_rect, stringLeft);
															canvas.String(item.SubTitle, ((Control)this).Font, (Brush)(object)val6, item.subtitle_rect, stringLeft);
															canvas.String(item.Description, val9, (Brush)(object)val6, item.description_rect, stringLeft);
															color = val6.Color;
															break;
														case TStepState.Error:
														{
															SolidBrush val11 = new SolidBrush(Colour.Error.Get("Steps"));
															try
															{
																canvas.String(item.Title, ((Control)this).Font, (Brush)(object)val11, item.title_rect, stringLeft);
																canvas.String(item.SubTitle, ((Control)this).Font, (Brush)(object)val6, item.subtitle_rect, stringLeft);
																canvas.String(item.Description, val9, (Brush)(object)val11, item.description_rect, stringLeft);
																color = val11.Color;
															}
															finally
															{
																((IDisposable)val11)?.Dispose();
															}
															break;
														}
														default:
															canvas.String(item.Title, ((Control)this).Font, (Brush)(object)val, item.title_rect, stringLeft);
															canvas.String(item.SubTitle, ((Control)this).Font, (Brush)(object)val6, item.subtitle_rect, stringLeft);
															canvas.String(item.Description, val9, (Brush)(object)val, item.description_rect, stringLeft);
															color = val.Color;
															break;
														}
													}
													else if (num < current)
													{
														canvas.String(item.Title, ((Control)this).Font, (Brush)(object)val, item.title_rect, stringLeft);
														canvas.String(item.SubTitle, ((Control)this).Font, (Brush)(object)val6, item.subtitle_rect, stringLeft);
														canvas.String(item.Description, val9, (Brush)(object)val6, item.description_rect, stringLeft);
														color = val.Color;
													}
													else
													{
														canvas.String(item.Title, ((Control)this).Font, (Brush)(object)val6, item.title_rect, stringLeft);
														canvas.String(item.SubTitle, ((Control)this).Font, (Brush)(object)val6, item.subtitle_rect, stringLeft);
														canvas.String(item.Description, val9, (Brush)(object)val6, item.description_rect, stringLeft);
														color = val6.Color;
													}
													if (PaintIcon(canvas, item, color))
													{
														if (num == current)
														{
															switch (status)
															{
															case TStepState.Finish:
																canvas.PaintIconCore(item.ico_rect, SvgDb.IcoSuccess, val3.Color, val2.Color);
																break;
															case TStepState.Wait:
																canvas.FillEllipse((Brush)(object)val8, item.ico_rect);
																canvas.String((num + 1).ToString(), val9, (Brush)(object)val7, item.ico_rect, stringCenter);
																break;
															case TStepState.Error:
																canvas.PaintIconCore(item.ico_rect, SvgDb.IcoError, Colour.ErrorColor.Get("Steps"), Colour.Error.Get("Steps"));
																break;
															default:
																canvas.FillEllipse((Brush)(object)val3, item.ico_rect);
																canvas.String((num + 1).ToString(), val9, (Brush)(object)val4, item.ico_rect, stringCenter);
																break;
															}
														}
														else if (num < current)
														{
															canvas.PaintIconCore(item.ico_rect, SvgDb.IcoSuccess, val3.Color, val2.Color);
														}
														else
														{
															canvas.FillEllipse((Brush)(object)val8, item.ico_rect);
															canvas.String((num + 1).ToString(), val9, (Brush)(object)val7, item.ico_rect, stringCenter);
														}
													}
												}
												num++;
											}
										}
										finally
										{
											((IDisposable)val9)?.Dispose();
										}
									}
									finally
									{
										((IDisposable)val8)?.Dispose();
									}
								}
								finally
								{
									((IDisposable)val7)?.Dispose();
								}
							}
							finally
							{
								((IDisposable)val6)?.Dispose();
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
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
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
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private bool PaintIcon(Canvas g, StepsItem it, Color fore)
	{
		if (it.Icon != null)
		{
			g.Image(it.Icon, it.ico_rect);
			return false;
		}
		if (it.IconSvg != null && g.GetImgExtend(it.IconSvg, it.ico_rect, fore))
		{
			return false;
		}
		return true;
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		((Control)this).OnMouseClick(e);
		if (items == null || items.Count == 0 || this.ItemClick == null)
		{
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			StepsItem stepsItem = items[i];
			if (stepsItem != null && stepsItem.rect.Contains(e.Location))
			{
				this.ItemClick(this, new StepsItemEventArgs(stepsItem, e));
				break;
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (items == null || items.Count == 0 || this.ItemClick == null)
		{
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			StepsItem stepsItem = items[i];
			if (stepsItem != null && stepsItem.rect.Contains(e.Location))
			{
				SetCursor(val: true);
				return;
			}
		}
		SetCursor(val: false);
	}
}

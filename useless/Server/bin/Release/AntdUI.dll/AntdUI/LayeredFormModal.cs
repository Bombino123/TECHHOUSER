using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormModal : Window, IEventListener
{
	private Modal.Config config;

	private Panel? panel_main;

	private DateTime old_now;

	private int count;

	private Rectangle rectIcon;

	private Rectangle rectTitle;

	private Rectangle rectContent;

	private Rectangle[] rectsContent;

	private bool rtext;

	private readonly StringFormat stringLeft = Helper.SF_Ellipsis((StringAlignment)1, (StringAlignment)0);

	private readonly StringFormat stringTL = Helper.SF_Ellipsis((StringAlignment)0, (StringAlignment)0);

	private ITaskOpacity close_button;

	private Rectangle rect_close;

	private bool isclose = true;

	private Button? btn_ok;

	private Button? btn_no;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public override bool AutoHandDpi { get; set; }

	public LayeredFormModal(Modal.Config _config)
	{
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Expected O, but got Unknown
		Modal.Config _config2 = _config;
		base._002Ector();
		LayeredFormModal layeredFormModal = this;
		((Control)this).SetStyle((ControlStyles)204818, true);
		((Control)this).UpdateStyles();
		base.Resizable = false;
		base.FormBorderStyle = (FormBorderStyle)1;
		config = _config2;
		if (config.Form != null)
		{
			((Form)this).TopMost = config.Form.TopMost;
		}
		close_button = new ITaskOpacity((Form)(object)this);
		((Control)this).SuspendLayout();
		int butt_h = (int)Math.Round((float)config.BtnHeight * Config.Dpi);
		((Control)this).BackColor = Colour.BgElevated.Get("Modal");
		base.Size = new Size(416, 122 + butt_h);
		if (config.Form == null)
		{
			if (config.Font != null)
			{
				((Control)this).Font = config.Font;
			}
		}
		else
		{
			((Control)this).Font = config.Font ?? ((Control)config.Form).Font;
		}
		((Control)this).ForeColor = Colour.TextBase.Get("Modal");
		base.ShowInTaskbar = false;
		if (config.Form == null)
		{
			((Form)this).StartPosition = (FormStartPosition)1;
		}
		else
		{
			((Form)this).StartPosition = (FormStartPosition)4;
		}
		if (butt_h > 0)
		{
			Button obj = new Button
			{
				AutoSizeMode = TAutoSize.Width
			};
			((Control)obj).Dock = (DockStyle)4;
			((Control)obj).Location = new Point(304, 0);
			((Control)obj).Name = "btn_ok";
			((Control)obj).Size = new Size(64, butt_h);
			((Control)obj).TabIndex = 0;
			obj.Type = config.OkType;
			((Control)obj).Text = config.OkText;
			btn_ok = obj;
			config.OnButtonStyle?.Invoke("OK", btn_ok);
			((Control)btn_ok).Click += btn_ok_Click;
			if (config.OkFont != null)
			{
				((Control)btn_ok).Font = config.OkFont;
			}
			if (config.CancelText != null)
			{
				Button obj2 = new Button
				{
					AutoSizeMode = TAutoSize.Width,
					BorderWidth = 1f
				};
				((Control)obj2).Dock = (DockStyle)4;
				((Control)obj2).Location = new Point(240, 0);
				((Control)obj2).Name = "btn_no";
				((Control)obj2).Size = new Size(64, butt_h);
				((Control)obj2).TabIndex = 1;
				((Control)obj2).Text = config.CancelText;
				btn_no = obj2;
				config.OnButtonStyle?.Invoke("Cancel", btn_no);
				((Control)btn_no).Click += btn_no_Click;
				if (config.CancelFont != null)
				{
					((Control)btn_no).Font = config.CancelFont;
				}
			}
			Panel panel = new Panel();
			((Control)panel).Dock = (DockStyle)2;
			panel.Back = Colour.BgElevated.Get("Modal");
			((Control)panel).Size = new Size(368, butt_h);
			panel_main = panel;
			if (btn_no != null)
			{
				((Control)panel_main).Controls.Add((Control)(object)btn_no);
			}
			((Control)panel_main).Controls.Add((Control)(object)btn_ok);
			if (config.Btns != null)
			{
				List<Button> list = new List<Button>(config.Btns.Length);
				Modal.Btn[] btns = config.Btns;
				foreach (Modal.Btn btn2 in btns)
				{
					Button obj3 = new Button
					{
						AutoSizeMode = TAutoSize.Width
					};
					((Control)obj3).Dock = (DockStyle)4;
					((Control)obj3).Size = new Size(64, butt_h);
					((Control)obj3).Name = btn2.Name;
					((Control)obj3).Text = btn2.Text;
					obj3.Type = btn2.Type;
					obj3.BackColor = btn2.Back;
					obj3.ForeColor = btn2.Fore;
					((Control)obj3).Tag = btn2.Tag;
					Button button = obj3;
					config.OnButtonStyle?.Invoke(btn2.Name, button);
					((Control)panel_main).Controls.Add((Control)(object)button);
					list.Insert(0, button);
				}
				foreach (Button btn in list)
				{
					((Control)btn).BringToFront();
					((Control)btn).Click += delegate
					{
						layeredFormModal.isclose = false;
						btn.Loading = true;
						bool DisableCancel = false;
						if (layeredFormModal.config.LoadingDisableCancel && layeredFormModal.btn_no != null)
						{
							layeredFormModal.btn_no.Enabled = false;
							DisableCancel = true;
						}
						ITask.Run(delegate
						{
							bool num29 = layeredFormModal.config.OnBtns?.Invoke(btn) ?? true;
							btn.Loading = false;
							layeredFormModal.isclose = true;
							if (num29)
							{
								Thread.Sleep(10);
								((Control)layeredFormModal).BeginInvoke((Delegate)(Action)delegate
								{
									if (((Control)layeredFormModal).IsHandleCreated && !((Control)layeredFormModal).IsDisposed)
									{
										((Form)layeredFormModal).Close();
									}
								});
							}
							else if (DisableCancel && layeredFormModal.btn_no != null)
							{
								((Control)layeredFormModal).BeginInvoke((Delegate)(Action)delegate
								{
									if (((Control)layeredFormModal.btn_no).IsHandleCreated && !((Control)layeredFormModal.btn_no).IsDisposed)
									{
										layeredFormModal.btn_no.Enabled = true;
									}
								});
							}
						});
					};
				}
			}
			((Control)this).Controls.Add((Control)(object)panel_main);
			if (config.Draggable)
			{
				((Control)panel_main).MouseMove += new MouseEventHandler(Window_MouseDown);
			}
		}
		if (config.Keyboard)
		{
			if (butt_h > 0)
			{
				if (btn_no == null)
				{
					((Form)this).AcceptButton = (((Form)this).CancelButton = (IButtonControl)(object)btn_ok);
				}
				else
				{
					((Form)this).AcceptButton = (IButtonControl)(object)btn_ok;
					((Form)this).CancelButton = (IButtonControl)(object)btn_no;
				}
			}
			else
			{
				ONESC = delegate
				{
					((Form)layeredFormModal).DialogResult = (DialogResult)7;
				};
			}
		}
		rectsContent = Helper.GDI(delegate(Canvas g)
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Expected O, but got Unknown
			float dpi = Config.Dpi;
			int num = (int)Math.Round(8f * dpi);
			int num2 = (int)Math.Round((float)layeredFormModal.config.Padding.Width * dpi);
			int num3 = (int)Math.Round((float)layeredFormModal.config.Padding.Height * dpi);
			int num4 = (int)Math.Round((float)layeredFormModal.config.Width * dpi);
			int num5 = num4 - num2 * 2;
			((Control)layeredFormModal).Padding = new Padding(num2, num3, num2, num3);
			if (layeredFormModal.panel_main != null)
			{
				((Control)layeredFormModal.panel_main).Height = butt_h;
			}
			Font val2 = new Font(((Control)layeredFormModal).Font.FontFamily, ((Control)layeredFormModal).Font.Size * 1.14f, (FontStyle)1);
			try
			{
				int num6 = 0;
				object content = layeredFormModal.config.Content;
				Control val3 = (Control)((content is Control) ? content : null);
				if (val3 != null)
				{
					Helper.DpiAuto(dpi, val3);
					num4 = val3.Width + num2 * 2;
					num5 = val3.Width;
					((Control)layeredFormModal).Controls.Add(val3);
					((Component)(object)val3).Disposed += delegate
					{
						((Form)layeredFormModal).Close();
					};
					if (_config2.Icon == TType.None)
					{
						if (layeredFormModal.config.Title == null && !layeredFormModal.config.CloseIcon)
						{
							layeredFormModal.rectTitle = new Rectangle(0, 0, 0, 0);
							int num7 = val3.Height + butt_h;
							layeredFormModal.rectContent = new Rectangle(num2, num3, num5, num7 - butt_h);
							LayeredFormModal layeredFormModal2 = layeredFormModal;
							LayeredFormModal layeredFormModal3 = layeredFormModal;
							Size size2 = (layeredFormModal.Size = new Size(num4, num7 + num3 * 2));
							Size minimumSize = (((Control)layeredFormModal3).MaximumSize = size2);
							((Control)layeredFormModal2).MinimumSize = minimumSize;
						}
						else
						{
							Size size4 = g.MeasureString(layeredFormModal.config.Title, val2, num5);
							int num8 = size4.Height + num + val3.Height + butt_h;
							layeredFormModal.rectTitle = new Rectangle(num2, num3, num5, size4.Height + num);
							layeredFormModal.rectContent = new Rectangle(layeredFormModal.rectTitle.X, layeredFormModal.rectTitle.Bottom, num5, num8 - butt_h - size4.Height - num);
							LayeredFormModal layeredFormModal4 = layeredFormModal;
							LayeredFormModal layeredFormModal5 = layeredFormModal;
							Size size2 = (layeredFormModal.Size = new Size(num4, num8 + num3 * 2));
							Size minimumSize = (((Control)layeredFormModal5).MaximumSize = size2);
							((Control)layeredFormModal4).MinimumSize = minimumSize;
						}
					}
					else
					{
						int num9 = (num6 = g.MeasureString("龍Qq", val2).Height);
						int num10 = (int)((float)num9 * 0.54f);
						num5 -= num9 + num10;
						Size size7 = g.MeasureString(layeredFormModal.config.Title, val2, num5);
						int num11 = size7.Height + num + val3.Height + butt_h;
						layeredFormModal.rectTitle = new Rectangle(num2 + num9 + num10, num3, num5, size7.Height + num);
						layeredFormModal.rectContent = new Rectangle(layeredFormModal.rectTitle.X, layeredFormModal.rectTitle.Bottom, num5, num11 - butt_h - size7.Height - num);
						val3.Location = new Point(layeredFormModal.rectContent.X, layeredFormModal.rectContent.Y);
						layeredFormModal.rectIcon = new Rectangle(num2, layeredFormModal.rectTitle.Y + (layeredFormModal.rectTitle.Height - num9) / 2, num9, num9);
						LayeredFormModal layeredFormModal6 = layeredFormModal;
						LayeredFormModal layeredFormModal7 = layeredFormModal;
						Size size2 = (layeredFormModal.Size = new Size(num4, num11 + num3 * 2));
						Size minimumSize = (((Control)layeredFormModal7).MaximumSize = size2);
						((Control)layeredFormModal6).MinimumSize = minimumSize;
					}
					if (layeredFormModal.config.CloseIcon)
					{
						if (num6 == 0)
						{
							num6 = g.MeasureString("龍Qq", val2).Height;
						}
						int num12 = num6;
						layeredFormModal.rect_close = new Rectangle(layeredFormModal.rectTitle.Right - num12, layeredFormModal.rectTitle.Y, num12, num12);
					}
					val3.Location = new Point(layeredFormModal.rectContent.X, layeredFormModal.rectContent.Y);
					val3.Size = new Size(layeredFormModal.rectContent.Width, layeredFormModal.rectContent.Height);
				}
				else
				{
					if (layeredFormModal.config.Content is IList<Modal.TextLine> list2)
					{
						layeredFormModal.rtext = true;
						List<Rectangle> list3 = new List<Rectangle>(list2.Count);
						if (_config2.Icon == TType.None)
						{
							Size size10 = g.MeasureString(layeredFormModal.config.Title, val2, num5);
							layeredFormModal.rectTitle = new Rectangle(num2, num3, num5, size10.Height + num);
							int num13 = num3 + size10.Height + num;
							int num14 = 0;
							foreach (Modal.TextLine item in list2)
							{
								int num15 = g.MeasureString(item.Text, item.Font ?? ((Control)layeredFormModal).Font, num5).Height + (int)((float)item.Gap * dpi);
								list3.Add(new Rectangle(layeredFormModal.rectTitle.X, num13, num5, num15));
								num13 += num15;
								num14 += num15;
							}
							int num16 = size10.Height + num + num14 + butt_h;
							layeredFormModal.rectContent = new Rectangle(layeredFormModal.rectTitle.X, layeredFormModal.rectTitle.Bottom, num5, num16 - butt_h - size10.Height - num);
							LayeredFormModal layeredFormModal8 = layeredFormModal;
							LayeredFormModal layeredFormModal9 = layeredFormModal;
							Size size2 = (layeredFormModal.Size = new Size(num4, num16 + num3 * 2));
							Size minimumSize = (((Control)layeredFormModal9).MaximumSize = size2);
							((Control)layeredFormModal8).MinimumSize = minimumSize;
						}
						else
						{
							int num17 = (num6 = g.MeasureString("龍Qq", val2).Height);
							int num18 = (int)((float)num17 * 0.54f);
							num5 -= num17 + num18;
							Size size13 = g.MeasureString(layeredFormModal.config.Title, val2, num5);
							layeredFormModal.rectTitle = new Rectangle(num2 + num17 + num18, num3, num5, size13.Height + num);
							layeredFormModal.rectIcon = new Rectangle(num2, layeredFormModal.rectTitle.Y + (layeredFormModal.rectTitle.Height - num17) / 2, num17, num17);
							int num19 = num3 + size13.Height + num;
							int num20 = 0;
							foreach (Modal.TextLine item2 in list2)
							{
								int num21 = g.MeasureString(item2.Text, item2.Font ?? ((Control)layeredFormModal).Font, num5).Height + (int)((float)item2.Gap * dpi);
								list3.Add(new Rectangle(layeredFormModal.rectTitle.X, num19, num5, num21));
								num19 += num21;
								num20 += num21;
							}
							int num22 = size13.Height + num + num20 + butt_h;
							layeredFormModal.rectContent = new Rectangle(layeredFormModal.rectTitle.X, layeredFormModal.rectTitle.Bottom, num5, num22 - butt_h - size13.Height - num);
							LayeredFormModal layeredFormModal10 = layeredFormModal;
							LayeredFormModal layeredFormModal11 = layeredFormModal;
							Size size2 = (layeredFormModal.Size = new Size(num4, num22 + num3 * 2));
							Size minimumSize = (((Control)layeredFormModal11).MaximumSize = size2);
							((Control)layeredFormModal10).MinimumSize = minimumSize;
						}
						if (layeredFormModal.config.CloseIcon)
						{
							if (num6 == 0)
							{
								num6 = g.MeasureString("龍Qq", val2).Height;
							}
							int num23 = num6;
							layeredFormModal.rect_close = new Rectangle(layeredFormModal.rectTitle.Right - num23, layeredFormModal.rectTitle.Y, num23, num23);
						}
						return list3.ToArray();
					}
					layeredFormModal.rtext = true;
					string text = layeredFormModal.config.Content.ToString();
					if (_config2.Icon == TType.None)
					{
						Size size16 = g.MeasureString(layeredFormModal.config.Title, val2, num5);
						Size size17 = g.MeasureString(text, ((Control)layeredFormModal).Font, num5);
						int num24 = size16.Height + num + size17.Height + butt_h;
						layeredFormModal.rectTitle = new Rectangle(num2, num3, num5, size16.Height + num);
						layeredFormModal.rectContent = new Rectangle(layeredFormModal.rectTitle.X, layeredFormModal.rectTitle.Bottom, num5, num24 - butt_h - size16.Height - num);
						LayeredFormModal layeredFormModal12 = layeredFormModal;
						LayeredFormModal layeredFormModal13 = layeredFormModal;
						Size size2 = (layeredFormModal.Size = new Size(num4, num24 + num3 * 2));
						Size minimumSize = (((Control)layeredFormModal13).MaximumSize = size2);
						((Control)layeredFormModal12).MinimumSize = minimumSize;
					}
					else
					{
						int num25 = (num6 = g.MeasureString("龍Qq", val2).Height);
						int num26 = (int)((float)num25 * 0.54f);
						num5 -= num25 + num26;
						Size size20 = g.MeasureString(layeredFormModal.config.Title, val2, num5);
						Size size21 = g.MeasureString(text, ((Control)layeredFormModal).Font, num5);
						int num27 = size20.Height + num + size21.Height + butt_h;
						layeredFormModal.rectTitle = new Rectangle(num2 + num25 + num26, num3, num5, size20.Height + num);
						layeredFormModal.rectContent = new Rectangle(layeredFormModal.rectTitle.X, layeredFormModal.rectTitle.Bottom, num5, num27 - butt_h - size20.Height - num);
						layeredFormModal.rectIcon = new Rectangle(num2, layeredFormModal.rectTitle.Y + (layeredFormModal.rectTitle.Height - num25) / 2, num25, num25);
						LayeredFormModal layeredFormModal14 = layeredFormModal;
						LayeredFormModal layeredFormModal15 = layeredFormModal;
						Size size2 = (layeredFormModal.Size = new Size(num4, num27 + num3 * 2));
						Size minimumSize = (((Control)layeredFormModal15).MaximumSize = size2);
						((Control)layeredFormModal14).MinimumSize = minimumSize;
					}
					if (layeredFormModal.config.CloseIcon)
					{
						if (num6 == 0)
						{
							num6 = g.MeasureString("龍Qq", val2).Height;
						}
						int num28 = num6;
						layeredFormModal.rect_close = new Rectangle(layeredFormModal.rectTitle.Right - num28, layeredFormModal.rectTitle.Y, num28, num28);
					}
				}
				return new Rectangle[0];
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		});
		((Control)this).ResumeLayout();
		config.Layered = this;
	}

	protected override void DestroyHandle()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		((Control)this).DestroyHandle();
		((Component)(object)btn_ok)?.Dispose();
		((Component)(object)btn_no)?.Dispose();
		close_button.Dispose();
		if (panel_main != null)
		{
			((Control)panel_main).MouseMove -= new MouseEventHandler(Window_MouseDown);
			((Component)(object)panel_main)?.Dispose();
			panel_main = null;
		}
		object content = config.Content;
		((Component)((content is Control) ? content : null))?.Dispose();
		stringLeft.Dispose();
		stringTL.Dispose();
		((Component)(object)this).Dispose();
	}

	protected override void WndProc(ref Message m)
	{
		if (config.MaskClosable && isclose)
		{
			if (((Message)(ref m)).Msg == 160 || ((Message)(ref m)).Msg == 512)
			{
				count = 0;
			}
			else if (((Message)(ref m)).Msg == 134)
			{
				DateTime now = DateTime.Now;
				if (now > old_now)
				{
					count = 0;
					old_now = now.AddSeconds(1.0);
				}
				count++;
				if (count > 2)
				{
					((Form)this).DialogResult = (DialogResult)7;
				}
			}
		}
		base.WndProc(ref m);
	}

	private void Window_MouseDown(object? sender, MouseEventArgs e)
	{
		DraggableMouseDown();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Expected O, but got Unknown
		Canvas canvas = e.Graphics.High();
		if (config.Icon != 0)
		{
			canvas.PaintIcons(config.Icon, rectIcon, "Modal");
		}
		if (config.CloseIcon)
		{
			if (close_button.Animation)
			{
				GraphicsPath val = rect_close.RoundPath((int)(4f * Config.Dpi));
				try
				{
					canvas.Fill(Helper.ToColor(close_button.Value, Colour.FillSecondary.Get("Modal")), val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				canvas.PaintIconClose(rect_close, Colour.Text.Get("Modal"), 0.6f);
			}
			else if (close_button.Switch)
			{
				GraphicsPath val2 = rect_close.RoundPath((int)(4f * Config.Dpi));
				try
				{
					canvas.Fill(Colour.FillSecondary.Get("Modal"), val2);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				canvas.PaintIconClose(rect_close, Colour.Text.Get("Modal"), 0.6f);
			}
			else
			{
				canvas.PaintIconClose(rect_close, Colour.TextTertiary.Get("Modal"), 0.6f);
			}
		}
		SolidBrush val3 = new SolidBrush(Colour.Text.Get("Modal"));
		try
		{
			Font val4 = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 1.14f, (FontStyle)1);
			try
			{
				canvas.String(config.Title, val4, (Brush)(object)val3, rectTitle, stringLeft);
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
			if (!rtext)
			{
				return;
			}
			if (config.Content is IList<Modal.TextLine> list)
			{
				for (int i = 0; i < list.Count; i++)
				{
					Modal.TextLine textLine = list[i];
					if (textLine.Fore.HasValue)
					{
						SolidBrush val5 = new SolidBrush(textLine.Fore.Value);
						try
						{
							canvas.String(textLine.Text, textLine.Font ?? ((Control)this).Font, (Brush)(object)val5, rectsContent[i], stringLeft);
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
					else
					{
						canvas.String(textLine.Text, textLine.Font ?? ((Control)this).Font, (Brush)(object)val3, rectsContent[i], stringLeft);
					}
				}
			}
			else
			{
				canvas.String(config.Content.ToString(), ((Control)this).Font, (Brush)(object)val3, rectContent, stringTL);
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (config.CloseIcon)
		{
			close_button.MaxValue = Colour.FillSecondary.Get("Modal").A;
			close_button.Switch = rect_close.Contains(e.Location);
			SetCursor(close_button.Switch);
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576 && config.CloseIcon && rect_close.Contains(e.Location))
		{
			((Control)this).OnMouseUp(e);
			return;
		}
		if (config.Draggable)
		{
			DraggableMouseDown();
		}
		((Control)this).OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576 && config.CloseIcon && rect_close.Contains(e.Location))
		{
			((Form)this).DialogResult = (DialogResult)7;
		}
		else
		{
			((Control)this).OnMouseUp(e);
		}
	}

	private void btn_no_Click(object? sender, EventArgs e)
	{
		((Form)this).DialogResult = (DialogResult)7;
	}

	private void btn_ok_Click(object? sender, EventArgs e)
	{
		if (config.OnOk == null)
		{
			((Form)this).DialogResult = (DialogResult)1;
		}
		else
		{
			if (btn_ok == null)
			{
				return;
			}
			isclose = false;
			btn_ok.Loading = true;
			bool DisableCancel = false;
			if (config.LoadingDisableCancel && btn_no != null)
			{
				btn_no.Enabled = false;
				DisableCancel = true;
			}
			ITask.Run(delegate
			{
				bool flag = false;
				try
				{
					flag = config.OnOk(config);
				}
				catch
				{
				}
				isclose = true;
				btn_ok.Loading = false;
				if (((Control)this).IsHandleCreated && !((Control)this).IsDisposed)
				{
					if (flag)
					{
						Thread.Sleep(10);
						((Control)this).BeginInvoke((Delegate)(Action)delegate
						{
							if (((Control)this).IsHandleCreated && !((Control)this).IsDisposed)
							{
								((Form)this).DialogResult = (DialogResult)1;
							}
						});
					}
					else if (DisableCancel && btn_no != null)
					{
						((Control)this).BeginInvoke((Delegate)(Action)delegate
						{
							if (((Control)btn_no).IsHandleCreated && !((Control)btn_no).IsDisposed)
							{
								btn_no.Enabled = true;
							}
						});
					}
				}
			});
		}
	}

	public void SetOkText(string value)
	{
		if (btn_ok != null)
		{
			((Control)btn_ok).Text = value;
		}
	}

	public void SetCancelText(string? value)
	{
		if (btn_no == null)
		{
			if (panel_main != null && !string.IsNullOrEmpty(value))
			{
				Button obj = new Button
				{
					AutoSizeMode = TAutoSize.Width,
					BorderWidth = 1f
				};
				((Control)obj).Dock = (DockStyle)4;
				((Control)obj).Location = new Point(240, 0);
				((Control)obj).Name = "btn_no";
				((Control)obj).Size = new Size(64, config.BtnHeight);
				((Control)obj).TabIndex = 1;
				((Control)obj).Text = value;
				btn_no = obj;
				((Control)btn_no).Click += btn_no_Click;
				if (config.CancelFont != null)
				{
					((Control)btn_no).Font = config.CancelFont;
				}
				((Control)panel_main).Controls.Add((Control)(object)btn_no);
				((Form)this).CancelButton = (IButtonControl)(object)btn_no;
			}
		}
		else if (string.IsNullOrEmpty(value))
		{
			btn_no.Visible = false;
		}
		else
		{
			((Control)btn_no).Text = value;
			btn_no.Visible = true;
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		((Control)(object)this).AddListener();
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.THEME)
		{
			((Control)this).BackColor = Colour.BgElevated.Get("Modal");
			((Control)this).ForeColor = Colour.TextBase.Get("Modal");
			if (panel_main != null)
			{
				panel_main.Back = Colour.BgElevated.Get("Modal");
			}
		}
	}
}

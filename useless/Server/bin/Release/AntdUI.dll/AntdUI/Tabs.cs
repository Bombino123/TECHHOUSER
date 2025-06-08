using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using AntdUI.Design;

namespace AntdUI;

[Description("Tabs 标签页")]
[ToolboxItem(true)]
[DefaultEvent("SelectedIndexChanged")]
[DefaultProperty("Pages")]
[Designer(typeof(TabControlDesigner))]
public class Tabs : IControl
{
	internal class TabControlDesigner : ParentControlDesigner
	{
		private DesignerActionListCollection actionLists;

		public Tabs Control => (Tabs)(object)((ControlDesigner)this).Control;

		public IDesignerHost DesignerHost { get; private set; }

		public ISelectionService SelectionService { get; private set; }

		public override DesignerActionListCollection ActionLists
		{
			get
			{
				if (actionLists == null)
				{
					actionLists = ((ComponentDesigner)this).ActionLists;
					actionLists.Add((DesignerActionList)(object)new TabControlActionList((IComponent)Control));
				}
				return actionLists;
			}
		}

		public override void Initialize(IComponent component)
		{
			((ParentControlDesigner)this).Initialize(component);
			DesignerHost = (IDesignerHost)((ComponentDesigner)this).GetService(typeof(IDesignerHost));
			SelectionService = (ISelectionService)((ComponentDesigner)this).GetService(typeof(ISelectionService));
		}

		protected override bool GetHitTest(Point point)
		{
			Point point2 = ((Control)Control).PointToClient(point);
			foreach (TabPage page in Control.Pages)
			{
				if (page.Contains(point2.X, point2.Y))
				{
					return true;
				}
			}
			return ((ControlDesigner)this).GetHitTest(point);
		}
	}

	internal class TabControlActionList : DesignerActionList
	{
		public Tabs Control { get; private set; }

		public IDesignerHost DesignerHost { get; private set; }

		public ISelectionService SelectionService { get; private set; }

		public TabAlignment Alignment
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return Control.Alignment;
			}
			set
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				GetPropertyByName("Alignment").SetValue(Control, value);
			}
		}

		public void AddTab()
		{
			if (DesignerHost != null)
			{
				TabPage tabPage = (TabPage)DesignerHost.CreateComponent(typeof(TabPage));
				string value = (string)TypeDescriptor.GetProperties(tabPage)["Name"].GetValue(tabPage);
				TypeDescriptor.GetProperties(tabPage)["Text"].SetValue(tabPage, value);
				Control.Pages.Add(tabPage);
				Control.SelectedIndex = Control.Pages.IndexOf(tabPage);
			}
		}

		protected void RemoveTab()
		{
			if (DesignerHost != null && Control.Pages.Count > 1)
			{
				int selectedIndex = Control.SelectedIndex;
				Control.SelectedIndex = selectedIndex;
			}
		}

		public TabControlActionList(IComponent component)
			: base(component)
		{
			Control = (Tabs)component;
			DesignerHost = (IDesignerHost)((DesignerActionList)this).GetService(typeof(IDesignerHost));
			SelectionService = (ISelectionService)((DesignerActionList)this).GetService(typeof(ISelectionService));
		}

		private PropertyDescriptor GetPropertyByName(string propName)
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(Control)[propName];
			if (propertyDescriptor == null)
			{
				throw new ArgumentException("Unknown property.", propName);
			}
			return propertyDescriptor;
		}

		public override DesignerActionItemCollection GetSortedActionItems()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Expected O, but got Unknown
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			//IL_008a: Expected O, but got Unknown
			DesignerActionItemCollection val = new DesignerActionItemCollection();
			val.Add((DesignerActionItem)new DesignerActionHeaderItem("外观"));
			val.Add((DesignerActionItem)new DesignerActionHeaderItem("数据"));
			val.Add((DesignerActionItem)new DesignerActionPropertyItem("Alignment", "选项卡位置", "外观", "确定选项卡是否显示在控件的顶部、底部、左侧或右侧(在左侧或右侧将隐式地分为多行)。"));
			val.Add((DesignerActionItem)new DesignerActionMethodItem((DesignerActionList)(object)this, "AddTab", "添加选项卡", "数据", "向控件添加新选项卡"));
			val.Add((DesignerActionItem)new DesignerActionMethodItem((DesignerActionList)(object)this, "RemoveTab", "移除选项卡", "数据", "删除当前选项卡"));
			return val;
		}
	}

	public class StyleLine : IStyle
	{
		private Tabs? owner;

		private int size = 3;

		private int padding = 8;

		private int radius;

		private int backsize = 1;

		private Rectangle rect_ful;

		private Rectangle rect_line_top;

		private TabPageRect[] rects = new TabPageRect[0];

		private bool AnimationBar;

		private RectangleF AnimationBarValue;

		private ITask? ThreadBar;

		[Description("条大小")]
		[Category("样式")]
		[DefaultValue(3)]
		public int Size
		{
			get
			{
				return size;
			}
			set
			{
				if (size != value)
				{
					size = value;
					owner?.LoadLayout();
				}
			}
		}

		[Description("条边距")]
		[Category("样式")]
		[DefaultValue(8)]
		public int Padding
		{
			get
			{
				return padding;
			}
			set
			{
				if (padding != value)
				{
					padding = value;
					owner?.LoadLayout();
				}
			}
		}

		[Description("条圆角")]
		[Category("样式")]
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
					Tabs? tabs = owner;
					if (tabs != null)
					{
						((Control)tabs).Invalidate();
					}
				}
			}
		}

		[Description("条背景大小")]
		[Category("样式")]
		[DefaultValue(1)]
		public int BackSize
		{
			get
			{
				return backsize;
			}
			set
			{
				if (backsize != value)
				{
					backsize = value;
					owner?.LoadLayout();
				}
			}
		}

		[Description("条背景")]
		[Category("样式")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? Back { get; set; }

		public StyleLine()
		{
		}

		public StyleLine(Tabs tabs)
		{
			owner = tabs;
		}

		public void LoadLayout(Tabs tabs, Rectangle rect, TabCollection items)
		{
			Tabs tabs2 = tabs;
			TabCollection items2 = items;
			rect_ful = rect;
			owner = tabs2;
			rects = Helper.GDI(delegate(Canvas g)
			{
				//IL_0084: Unknown result type (might be due to invalid IL or missing references)
				//IL_0089: Unknown result type (might be due to invalid IL or missing references)
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a2: Expected I4, but got Unknown
				//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
				//IL_08c5: Unknown result type (might be due to invalid IL or missing references)
				//IL_08c7: Unknown result type (might be due to invalid IL or missing references)
				//IL_08ca: Invalid comparison between Unknown and I4
				//IL_08cf: Unknown result type (might be due to invalid IL or missing references)
				//IL_08d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_08d4: Invalid comparison between Unknown and I4
				int num = (int)((float)tabs2.Gap * Config.Dpi);
				int num2 = num / 2;
				int num3 = 0;
				int sizewh = 0;
				int num4 = (int)((float)Size * Config.Dpi);
				int num5 = (int)((float)Padding * Config.Dpi);
				int num6 = num5 * 2;
				List<TabPageRect> list = new List<TabPageRect>(items2.Count);
				int ico_size;
				Dictionary<TabPage, Size> dir = GetDir(tabs2, g, items2, num, num2, out ico_size, out sizewh);
				TabAlignment alignment = tabs2.Alignment;
				switch ((int)alignment)
				{
				case 1:
				{
					int y = rect.Bottom - sizewh;
					foreach (KeyValuePair<TabPage, Size> item in dir)
					{
						if (((Control)item.Key).Visible)
						{
							Rectangle rectangle3;
							if (item.Key.HasIcon)
							{
								rectangle3 = new Rectangle(rect.X + num3, y, item.Value.Width + num + ico_size + num2, sizewh);
								list.Add(new TabPageRect(rectangle3, new Rectangle(rectangle3.X + num5, rectangle3.Y, rectangle3.Width - num6, num4), item.Value, ico_size, num, num2));
							}
							else
							{
								rectangle3 = new Rectangle(rect.X + num3, y, item.Value.Width + num, sizewh);
								list.Add(new TabPageRect(rectangle3, new Rectangle(rectangle3.X + num5, rectangle3.Y, rectangle3.Width - num6, num4)));
							}
							item.Key.SetRect(rectangle3);
							num3 += rectangle3.Width;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					tabs2.SetPadding(0, 0, 0, sizewh);
					if (BackSize > 0)
					{
						int height = (int)((float)BackSize * Config.Dpi);
						rect_line_top = new Rectangle(rect.X, rect.Bottom - sizewh, rect.Width, height);
					}
					owner.scroll_max = num3 - rect.Width;
					owner.scroll_show = num3 > rect.Width;
					break;
				}
				case 2:
					foreach (KeyValuePair<TabPage, Size> item2 in dir)
					{
						if (((Control)item2.Key).Visible)
						{
							Rectangle rectangle4 = new Rectangle(rect.X, rect.Y + num3, sizewh, item2.Value.Height + num);
							if (item2.Key.HasIcon)
							{
								list.Add(new TabPageRect(rectangle4, new Rectangle(rectangle4.X + sizewh - num4, rectangle4.Y + num5, num4, rectangle4.Height - num6), item2.Value, ico_size, num, num2));
							}
							else
							{
								list.Add(new TabPageRect(rectangle4, new Rectangle(rectangle4.X + sizewh - num4, rectangle4.Y + num5, num4, rectangle4.Height - num6)));
							}
							item2.Key.SetRect(rectangle4);
							num3 += rectangle4.Height;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					tabs2.SetPadding(sizewh, 0, 0, 0);
					if (BackSize > 0)
					{
						int num8 = (int)((float)BackSize * Config.Dpi);
						rect_line_top = new Rectangle(rect.X + sizewh - num8, rect.Y, num8, rect.Height);
					}
					owner.scroll_max = num3 - rect.Height;
					owner.scroll_show = num3 > rect.Height;
					break;
				case 3:
				{
					int x = rect.Right - sizewh;
					foreach (KeyValuePair<TabPage, Size> item3 in dir)
					{
						if (((Control)item3.Key).Visible)
						{
							Rectangle rectangle2 = new Rectangle(x, rect.Y + num3, sizewh, item3.Value.Height + num);
							if (item3.Key.HasIcon)
							{
								list.Add(new TabPageRect(rectangle2, new Rectangle(rectangle2.X, rectangle2.Y + num5, num4, rectangle2.Height - num6), item3.Value, ico_size, num, num2));
							}
							else
							{
								list.Add(new TabPageRect(rectangle2, new Rectangle(rectangle2.X, rectangle2.Y + num5, num4, rectangle2.Height - num6)));
							}
							item3.Key.SetRect(rectangle2);
							num3 += rectangle2.Height;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					tabs2.SetPadding(0, 0, sizewh, 0);
					if (BackSize > 0)
					{
						int width = (int)((float)BackSize * Config.Dpi);
						rect_line_top = new Rectangle(x, rect.Y, width, rect.Height);
					}
					owner.scroll_max = num3 - rect.Height;
					owner.scroll_show = num3 > rect.Height;
					break;
				}
				default:
					foreach (KeyValuePair<TabPage, Size> item4 in dir)
					{
						if (((Control)item4.Key).Visible)
						{
							Rectangle rectangle;
							if (item4.Key.HasIcon)
							{
								rectangle = new Rectangle(rect.X + num3, rect.Y, item4.Value.Width + num + ico_size + num2, sizewh);
								list.Add(new TabPageRect(rectangle, new Rectangle(rectangle.X + num5, rectangle.Bottom - num4, rectangle.Width - num6, num4), item4.Value, ico_size, num, num2));
							}
							else
							{
								rectangle = new Rectangle(rect.X + num3, rect.Y, item4.Value.Width + num, sizewh);
								list.Add(new TabPageRect(rectangle, new Rectangle(rectangle.X + num5, rectangle.Bottom - num4, rectangle.Width - num6, num4)));
							}
							item4.Key.SetRect(rectangle);
							num3 += rectangle.Width;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					tabs2.SetPadding(0, sizewh, 0, 0);
					if (BackSize > 0)
					{
						int num7 = (int)((float)BackSize * Config.Dpi);
						rect_line_top = new Rectangle(rect.Left, rect.Y + sizewh - num7, rect.Width, num7);
					}
					owner.scroll_max = num3 - rect.Width;
					owner.scroll_show = num3 > rect.Width;
					break;
				}
				if (owner.scroll_show)
				{
					owner.scroll_max += owner.SizeExceed(rect, list[0].Rect, list[list.Count - 1].Rect);
				}
				else
				{
					Tabs? tabs3 = owner;
					int scroll_x = (owner.scroll_y = 0);
					tabs3.scroll_x = scroll_x;
					if (tabs2.centered)
					{
						alignment = tabs2.Alignment;
						if ((int)alignment > 1 && alignment - 2 <= 1)
						{
							int y2 = (rect.Height - num3) / 2;
							foreach (KeyValuePair<TabPage, Size> item5 in dir)
							{
								item5.Key.SetOffset(0, y2);
							}
							foreach (TabPageRect item6 in list)
							{
								item6.Rect.Offset(0, y2);
								item6.Rect_Text.Offset(0, y2);
								item6.Rect_Ico.Offset(0, y2);
								item6.Rect_Line.Offset(0, y2);
							}
						}
						else
						{
							int x2 = (rect.Width - num3) / 2;
							foreach (KeyValuePair<TabPage, Size> item7 in dir)
							{
								item7.Key.SetOffset(x2, 0);
							}
							foreach (TabPageRect item8 in list)
							{
								item8.Rect.Offset(x2, 0);
								item8.Rect_Text.Offset(x2, 0);
								item8.Rect_Ico.Offset(x2, 0);
								item8.Rect_Line.Offset(x2, 0);
							}
						}
					}
				}
				return list.ToArray();
			});
		}

		public void Paint(Tabs owner, Canvas g, TabCollection items)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Expected O, but got Unknown
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Expected O, but got Unknown
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Expected O, but got Unknown
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Expected O, but got Unknown
			if (rects.Length == 0)
			{
				return;
			}
			if (BackSize > 0)
			{
				SolidBrush val = new SolidBrush(Back ?? Colour.BorderSecondary.Get("Tabs"));
				try
				{
					g.Fill((Brush)(object)val, rect_line_top);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			if (owner.scroll_show)
			{
				g.SetClip(owner.PaintExceedPre(rect_ful, rects[rects.Length - 1].Rect.Height));
			}
			else
			{
				g.SetClip(rect_ful);
			}
			SolidBrush val2 = new SolidBrush(owner.ForeColor ?? Colour.Text.Get("Tabs"));
			try
			{
				SolidBrush val3 = new SolidBrush(owner.Fill ?? Colour.Primary.Get("Tabs"));
				try
				{
					SolidBrush val4 = new SolidBrush(owner.FillActive ?? Colour.PrimaryActive.Get("Tabs"));
					try
					{
						SolidBrush val5 = new SolidBrush(owner.FillHover ?? Colour.PrimaryHover.Get("Tabs"));
						try
						{
							if (owner.scroll_show)
							{
								g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
							}
							if (AnimationBar)
							{
								PaintBar(g, AnimationBarValue, val3);
								int num = 0;
								foreach (TabPage item in items)
								{
									if (((Control)item).Visible)
									{
										if (owner.SelectedIndex == num)
										{
											PaintText(g, rects[num], owner, item, val3);
										}
										else if (owner.hover_i == num)
										{
											PaintText(g, rects[num], owner, item, val5);
										}
										else
										{
											PaintText(g, rects[num], owner, item, val2);
										}
									}
									num++;
								}
							}
							else
							{
								int num2 = 0;
								foreach (TabPage item2 in items)
								{
									if (((Control)item2).Visible)
									{
										if (owner.SelectedIndex == num2)
										{
											PaintBar(g, rects[num2].Rect_Line, val3);
											PaintText(g, rects[num2], owner, item2, val3);
										}
										else if (owner.hover_i == num2)
										{
											PaintText(g, rects[num2], owner, item2, item2.MDown ? val4 : val5);
										}
										else
										{
											PaintText(g, rects[num2], owner, item2, val2);
										}
									}
									num2++;
								}
							}
							if (owner.scroll_show)
							{
								owner.PaintExceed(g, val2.Color, (int)((float)radius * Config.Dpi), rect_ful, rects[0].Rect, rects[rects.Length - 1].Rect, full: false);
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

		public TabPageRect GetTabRect(int i)
		{
			return rects[i];
		}

		private Dictionary<TabPage, Size> GetDir(Tabs owner, Canvas g, TabCollection items, int gap, int gapI, out int ico_size, out int sizewh)
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Invalid comparison between Unknown and I4
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Invalid comparison between Unknown and I4
			sizewh = 0;
			Size size = g.MeasureString("龍Qq", ((Control)owner).Font);
			Dictionary<TabPage, Size> dictionary = new Dictionary<TabPage, Size>(items.Count);
			foreach (TabPage item in items)
			{
				Size value = g.MeasureString(((Control)item).Text, ((Control)owner).Font);
				dictionary.Add(item, value);
			}
			ico_size = (int)((float)size.Height * owner.IconRatio);
			TabAlignment alignment = owner.Alignment;
			if ((int)alignment > 1 && alignment - 2 <= 1)
			{
				foreach (KeyValuePair<TabPage, Size> item2 in dictionary)
				{
					if (((Control)item2.Key).Visible)
					{
						int num = ((!item2.Key.HasIcon) ? (item2.Value.Width + gap) : (item2.Value.Width + ico_size + gap + gapI));
						if (sizewh < num)
						{
							sizewh = num;
						}
					}
				}
			}
			else
			{
				foreach (KeyValuePair<TabPage, Size> item3 in dictionary)
				{
					if (((Control)item3.Key).Visible)
					{
						int num2 = item3.Value.Height + gap;
						if (sizewh < num2)
						{
							sizewh = num2;
						}
					}
				}
			}
			return owner.HandItemSize(dictionary, ref sizewh);
		}

		private void PaintText(Canvas g, TabPageRect rects, Tabs owner, TabPage page, SolidBrush brush)
		{
			if (page.HasIcon)
			{
				if (page.Icon != null)
				{
					g.Image(page.Icon, rects.Rect_Ico);
				}
				else if (page.IconSvg != null)
				{
					g.GetImgExtend(page.IconSvg, rects.Rect_Ico, brush.Color);
				}
			}
			g.String(((Control)page).Text, ((Control)owner).Font, (Brush)(object)brush, rects.Rect_Text, owner.s_c);
			owner.PaintBadge(g, page, rects.Rect_Text);
		}

		private void PaintBar(Canvas g, RectangleF rect, SolidBrush brush)
		{
			if (radius > 0)
			{
				GraphicsPath val = rect.RoundPath((float)radius * Config.Dpi);
				try
				{
					g.Fill((Brush)(object)brush, val);
					return;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			g.Fill((Brush)(object)brush, rect);
		}

		private void PaintBar(Canvas g, Rectangle rect, SolidBrush brush)
		{
			if (radius > 0)
			{
				GraphicsPath val = rect.RoundPath((float)radius * Config.Dpi);
				try
				{
					g.Fill((Brush)(object)brush, val);
					return;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			g.Fill((Brush)(object)brush, rect);
		}

		private void SetRect(int old, int value)
		{
			if (owner == null || owner.items == null || rects.Length == 0 || owner.items.ListExceed(value))
			{
				return;
			}
			if (owner.items.ListExceed(old))
			{
				AnimationBarValue = rects[value].Rect_Line;
				return;
			}
			ThreadBar?.Dispose();
			Helper.GDI(delegate
			{
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0094: Invalid comparison between Unknown and I4
				//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Invalid comparison between Unknown and I4
				RectangleF OldValue = rects[old].Rect_Line;
				RectangleF NewValue = rects[value].Rect_Line;
				if (AnimationBarValue.Height == 0f)
				{
					AnimationBarValue = OldValue;
				}
				if (Config.Animation)
				{
					if ((int)owner.alignment == 2 || (int)owner.alignment == 3)
					{
						if (OldValue.X == NewValue.X)
						{
							AnimationBarValue.X = OldValue.X;
							AnimationBar = true;
							float p_val = Math.Abs(NewValue.Y - AnimationBarValue.Y) * 0.09f;
							float p_w_val2 = Math.Abs(NewValue.Height - AnimationBarValue.Height) * 0.1f;
							float p_val4 = (NewValue.Y - AnimationBarValue.Y) * 0.5f;
							ThreadBar = new ITask((Control)(object)owner, delegate
							{
								if (AnimationBarValue.Height != NewValue.Height)
								{
									if (NewValue.Height > OldValue.Height)
									{
										AnimationBarValue.Height += p_w_val2;
										if (AnimationBarValue.Height > NewValue.Height)
										{
											AnimationBarValue.Height = NewValue.Height;
										}
									}
									else
									{
										AnimationBarValue.Height -= p_w_val2;
										if (AnimationBarValue.Height < NewValue.Height)
										{
											AnimationBarValue.Height = NewValue.Height;
										}
									}
								}
								if (NewValue.Y > OldValue.Y)
								{
									if (AnimationBarValue.Y > p_val4)
									{
										AnimationBarValue.Y += p_val / 2f;
									}
									else
									{
										AnimationBarValue.Y += p_val;
									}
									if (AnimationBarValue.Y > NewValue.Y)
									{
										AnimationBarValue.Y = NewValue.Y;
										((Control)owner).Invalidate();
										return false;
									}
								}
								else
								{
									AnimationBarValue.Y -= p_val;
									if (AnimationBarValue.Y < NewValue.Y)
									{
										AnimationBarValue.Y = NewValue.Y;
										((Control)owner).Invalidate();
										return false;
									}
								}
								((Control)owner).Invalidate();
								return true;
							}, 10, delegate
							{
								AnimationBarValue = NewValue;
								AnimationBar = false;
								((Control)owner).Invalidate();
							});
							return;
						}
					}
					else if (OldValue.Y == NewValue.Y)
					{
						AnimationBarValue.Y = OldValue.Y;
						AnimationBar = true;
						float p_val2 = Math.Abs(NewValue.X - AnimationBarValue.X) * 0.09f;
						float p_w_val = Math.Abs(NewValue.Width - AnimationBarValue.Width) * 0.1f;
						float p_val3 = (NewValue.X - AnimationBarValue.X) * 0.5f;
						ThreadBar = new ITask((Control)(object)owner, delegate
						{
							if (AnimationBarValue.Width != NewValue.Width)
							{
								if (NewValue.Width > OldValue.Width)
								{
									AnimationBarValue.Width += p_w_val;
									if (AnimationBarValue.Width > NewValue.Width)
									{
										AnimationBarValue.Width = NewValue.Width;
									}
								}
								else
								{
									AnimationBarValue.Width -= p_w_val;
									if (AnimationBarValue.Width < NewValue.Width)
									{
										AnimationBarValue.Width = NewValue.Width;
									}
								}
							}
							if (NewValue.X > OldValue.X)
							{
								if (AnimationBarValue.X > p_val3)
								{
									AnimationBarValue.X += p_val2 / 2f;
								}
								else
								{
									AnimationBarValue.X += p_val2;
								}
								if (AnimationBarValue.X > NewValue.X)
								{
									AnimationBarValue.X = NewValue.X;
									((Control)owner).Invalidate();
									return false;
								}
							}
							else
							{
								AnimationBarValue.X -= p_val2;
								if (AnimationBarValue.X < NewValue.X)
								{
									AnimationBarValue.X = NewValue.X;
									((Control)owner).Invalidate();
									return false;
								}
							}
							((Control)owner).Invalidate();
							return true;
						}, 10, delegate
						{
							AnimationBarValue = NewValue;
							AnimationBar = false;
							((Control)owner).Invalidate();
						});
						return;
					}
				}
				AnimationBarValue = NewValue;
				((Control)owner).Invalidate();
			});
		}

		public void SelectedIndexChanged(int i, int old)
		{
			SetRect(old, i);
		}

		public void Dispose()
		{
			ThreadBar?.Dispose();
		}

		public void MouseWheel(int delta)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Invalid comparison between Unknown and I4
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			if (owner != null && owner.scroll_show)
			{
				TabAlignment alignment = owner.Alignment;
				if ((int)alignment > 1 && alignment - 2 <= 1)
				{
					owner.scroll_x = 0;
					owner.scroll_y -= delta;
				}
				else
				{
					owner.scroll_y = 0;
					owner.scroll_x -= delta;
				}
			}
		}

		public void MouseMove(int x, int y)
		{
		}

		public bool MouseClick(TabPage page, int i, int x, int y)
		{
			return false;
		}

		public void MouseLeave()
		{
		}
	}

	public class StyleCard : IStyle
	{
		private Tabs? owner;

		private int radius = 6;

		private int bordersize = 1;

		private Color? border;

		private Color? fill;

		private int gap = 2;

		private bool closable;

		private TabPageRect[] rects = new TabPageRect[0];

		[Description("卡片圆角")]
		[Category("卡片")]
		[DefaultValue(6)]
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
					Tabs? tabs = owner;
					if (tabs != null)
					{
						((Control)tabs).Invalidate();
					}
				}
			}
		}

		[Description("边框大小")]
		[Category("卡片")]
		[DefaultValue(1)]
		public int Border
		{
			get
			{
				return bordersize;
			}
			set
			{
				if (bordersize != value)
				{
					bordersize = value;
					owner?.LoadLayout();
				}
			}
		}

		[Description("卡片边框颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? BorderColor
		{
			get
			{
				return border;
			}
			set
			{
				if (!(border == value))
				{
					border = value;
					Tabs? tabs = owner;
					if (tabs != null)
					{
						((Control)tabs).Invalidate();
					}
				}
			}
		}

		[Description("卡片边框激活颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? BorderActive { get; set; }

		[Description("卡片颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? Fill
		{
			get
			{
				return fill;
			}
			set
			{
				if (!(fill == value))
				{
					fill = value;
					Tabs? tabs = owner;
					if (tabs != null)
					{
						((Control)tabs).Invalidate();
					}
				}
			}
		}

		[Description("卡片悬停颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? FillHover { get; set; }

		[Description("卡片激活颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? FillActive { get; set; }

		[Description("卡片间距")]
		[Category("卡片")]
		[DefaultValue(2)]
		public int Gap
		{
			get
			{
				return gap;
			}
			set
			{
				if (gap != value)
				{
					gap = value;
					owner?.LoadLayout();
				}
			}
		}

		[Description("可关闭")]
		[Category("卡片")]
		[DefaultValue(false)]
		public bool Closable
		{
			get
			{
				return closable;
			}
			set
			{
				if (closable != value)
				{
					closable = value;
					owner?.LoadLayout();
				}
			}
		}

		public StyleCard()
		{
		}

		public StyleCard(Tabs tabs)
		{
			owner = tabs;
		}

		public void LoadLayout(Tabs tabs, Rectangle rect, TabCollection items)
		{
			Tabs tabs2 = tabs;
			TabCollection items2 = items;
			owner = tabs2;
			rects = Helper.GDI(delegate(Canvas g)
			{
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_0088: Expected I4, but got Unknown
				//IL_0939: Unknown result type (might be due to invalid IL or missing references)
				//IL_093e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0940: Unknown result type (might be due to invalid IL or missing references)
				//IL_0943: Invalid comparison between Unknown and I4
				//IL_0948: Unknown result type (might be due to invalid IL or missing references)
				//IL_094b: Unknown result type (might be due to invalid IL or missing references)
				//IL_094d: Invalid comparison between Unknown and I4
				int num = (int)((float)tabs2.Gap * Config.Dpi);
				int gapI = num / 2;
				int num2 = 0;
				int sizewh = 0;
				int num3 = (int)((float)Gap * Config.Dpi);
				List<TabPageRect> list = new List<TabPageRect>(items2.Count);
				int ico_size;
				int close_size;
				Dictionary<TabPage, Size> dir = GetDir(tabs2, g, items2, num, out ico_size, out close_size, out sizewh);
				TabAlignment alignment = tabs2.Alignment;
				switch ((int)alignment)
				{
				case 1:
				{
					int y = rect.Bottom - sizewh;
					foreach (KeyValuePair<TabPage, Size> item in dir)
					{
						if (((Control)item.Key).Visible)
						{
							Rectangle rectangle4;
							if (closable && !item.Key.ReadOnly)
							{
								if (item.Key.HasIcon)
								{
									rectangle4 = new Rectangle(rect.X + num2, y, item.Value.Width + num + ico_size + close_size + num * 2, sizewh);
									list.Add(new TabPageRect(rectangle4, item.Value, ico_size, close_size, num, gapI));
								}
								else
								{
									rectangle4 = new Rectangle(rect.X + num2, y, item.Value.Width + num + close_size + num, sizewh);
									list.Add(new TabPageRect(rectangle4, test: false, item.Value, close_size, num, gapI));
								}
							}
							else if (item.Key.HasIcon)
							{
								rectangle4 = new Rectangle(rect.X + num2, y, item.Value.Width + num + ico_size + num, sizewh);
								list.Add(new TabPageRect(rectangle4, item.Value, ico_size, num, gapI));
							}
							else
							{
								rectangle4 = new Rectangle(rect.X + num2, y, item.Value.Width + num, sizewh);
								list.Add(new TabPageRect(rectangle4));
							}
							item.Key.SetRect(rectangle4);
							num2 += rectangle4.Width + num3;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					num2 -= num3;
					tabs2.SetPadding(0, 0, 0, sizewh);
					owner.scroll_max = num2 - rect.Width;
					owner.scroll_show = num2 > rect.Width;
					break;
				}
				case 2:
					foreach (KeyValuePair<TabPage, Size> item2 in dir)
					{
						if (((Control)item2.Key).Visible)
						{
							Rectangle rectangle2;
							if (closable && !item2.Key.ReadOnly)
							{
								rectangle2 = new Rectangle(rect.X, rect.Y + num2, sizewh, item2.Value.Height + num);
								if (item2.Key.HasIcon)
								{
									list.Add(new TabPageRect(rectangle2, item2.Value, ico_size, close_size, num, gapI));
								}
								else
								{
									list.Add(new TabPageRect(rectangle2, test: false, item2.Value, close_size, num, gapI));
								}
							}
							else
							{
								rectangle2 = new Rectangle(rect.X, rect.Y + num2, sizewh, item2.Value.Height + num);
								if (item2.Key.HasIcon)
								{
									list.Add(new TabPageRect(rectangle2, item2.Value, ico_size, num, gapI));
								}
								else
								{
									list.Add(new TabPageRect(rectangle2));
								}
							}
							item2.Key.SetRect(rectangle2);
							num2 += rectangle2.Height + num3;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					num2 -= num3;
					tabs2.SetPadding(sizewh, 0, 0, 0);
					owner.scroll_max = num2 - rect.Height;
					owner.scroll_show = num2 > rect.Height;
					break;
				case 3:
				{
					int x = rect.Right - sizewh;
					foreach (KeyValuePair<TabPage, Size> item3 in dir)
					{
						if (((Control)item3.Key).Visible)
						{
							Rectangle rectangle3;
							if (closable && !item3.Key.ReadOnly)
							{
								rectangle3 = new Rectangle(x, rect.Y + num2, sizewh, item3.Value.Height + num);
								if (item3.Key.HasIcon)
								{
									list.Add(new TabPageRect(rectangle3, item3.Value, ico_size, close_size, num, gapI));
								}
								else
								{
									list.Add(new TabPageRect(rectangle3, test: false, item3.Value, close_size, num, gapI));
								}
							}
							else
							{
								rectangle3 = new Rectangle(x, rect.Y + num2, sizewh, item3.Value.Height + num);
								if (item3.Key.HasIcon)
								{
									list.Add(new TabPageRect(rectangle3, item3.Value, ico_size, num, gapI));
								}
								else
								{
									list.Add(new TabPageRect(rectangle3));
								}
							}
							item3.Key.SetRect(rectangle3);
							num2 += rectangle3.Height + num3;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					num2 -= num3;
					tabs2.SetPadding(0, 0, sizewh, 0);
					owner.scroll_max = num2 - rect.Height;
					owner.scroll_show = num2 > rect.Height;
					break;
				}
				default:
					foreach (KeyValuePair<TabPage, Size> item4 in dir)
					{
						if (((Control)item4.Key).Visible)
						{
							Rectangle rectangle;
							if (closable && !item4.Key.ReadOnly)
							{
								if (item4.Key.HasIcon)
								{
									rectangle = new Rectangle(rect.X + num2, rect.Y, item4.Value.Width + num + ico_size + close_size + num * 2, sizewh);
									list.Add(new TabPageRect(rectangle, item4.Value, ico_size, close_size, num, gapI));
								}
								else
								{
									rectangle = new Rectangle(rect.X + num2, rect.Y, item4.Value.Width + num + close_size + num, sizewh);
									list.Add(new TabPageRect(rectangle, test: false, item4.Value, close_size, num, gapI));
								}
							}
							else if (item4.Key.HasIcon)
							{
								rectangle = new Rectangle(rect.X + num2, rect.Y, item4.Value.Width + num + ico_size + num, sizewh);
								list.Add(new TabPageRect(rectangle, item4.Value, ico_size, num, gapI));
							}
							else
							{
								rectangle = new Rectangle(rect.X + num2, rect.Y, item4.Value.Width + num, sizewh);
								list.Add(new TabPageRect(rectangle));
							}
							item4.Key.SetRect(rectangle);
							num2 += rectangle.Width + num3;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					num2 -= num3;
					tabs2.SetPadding(0, sizewh, 0, 0);
					owner.scroll_max = num2 - rect.Width;
					owner.scroll_show = num2 > rect.Width;
					break;
				}
				if (owner.scroll_show)
				{
					owner.scroll_max += owner.SizeExceed(((Control)owner).ClientRectangle, list[0].Rect, list[list.Count - 1].Rect);
				}
				else
				{
					Tabs? tabs3 = owner;
					int scroll_x = (owner.scroll_y = 0);
					tabs3.scroll_x = scroll_x;
					if (tabs2.centered)
					{
						alignment = tabs2.Alignment;
						if ((int)alignment > 1 && alignment - 2 <= 1)
						{
							int y2 = (rect.Height - num2) / 2;
							foreach (KeyValuePair<TabPage, Size> item5 in dir)
							{
								item5.Key.SetOffset(0, y2);
							}
							foreach (TabPageRect item6 in list)
							{
								item6.Rect.Offset(0, y2);
								item6.Rect_Text.Offset(0, y2);
								item6.Rect_Ico.Offset(0, y2);
								item6.Rect_Close.Offset(0, y2);
							}
						}
						else
						{
							int x2 = (rect.Width - num2) / 2;
							foreach (KeyValuePair<TabPage, Size> item7 in dir)
							{
								item7.Key.SetOffset(x2, 0);
							}
							foreach (TabPageRect item8 in list)
							{
								item8.Rect.Offset(x2, 0);
								item8.Rect_Text.Offset(x2, 0);
								item8.Rect_Ico.Offset(x2, 0);
								item8.Rect_Close.Offset(x2, 0);
							}
						}
					}
				}
				return list.ToArray();
			});
		}

		public void Paint(Tabs owner, Canvas g, TabCollection items)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected O, but got Unknown
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Expected O, but got Unknown
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Expected O, but got Unknown
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Expected O, but got Unknown
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Expected I4, but got Unknown
			//IL_0d06: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d0d: Expected O, but got Unknown
			//IL_039a: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Expected O, but got Unknown
			//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06bc: Expected O, but got Unknown
			//IL_09eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f2: Expected O, but got Unknown
			if (rects.Length != items.Count)
			{
				return;
			}
			SolidBrush val = new SolidBrush(owner.ForeColor ?? Colour.Text.Get("Tabs"));
			try
			{
				SolidBrush val2 = new SolidBrush(owner.Fill ?? Colour.Primary.Get("Tabs"));
				try
				{
					SolidBrush val3 = new SolidBrush(owner.FillActive ?? Colour.PrimaryActive.Get("Tabs"));
					try
					{
						SolidBrush val4 = new SolidBrush(owner.FillHover ?? Colour.PrimaryHover.Get("Tabs"));
						try
						{
							SolidBrush val5 = new SolidBrush(Fill ?? Colour.FillQuaternary.Get("Tabs"));
							try
							{
								SolidBrush val6 = new SolidBrush(FillHover ?? Colour.FillQuaternary.Get("Tabs"));
								try
								{
									SolidBrush val7 = new SolidBrush(FillActive ?? Colour.BgContainer.Get("Tabs"));
									try
									{
										Rectangle clientRectangle = ((Control)owner).ClientRectangle;
										int num = (int)((float)Radius * Config.Dpi);
										int num2 = (int)((float)bordersize * Config.Dpi);
										int num3 = num2 * 6;
										float num4 = (float)num2 / 2f;
										TabPage tabPage = null;
										int num5 = 0;
										int selectedIndex = owner.SelectedIndex;
										TabAlignment alignment = owner.Alignment;
										switch ((int)alignment)
										{
										case 1:
										{
											int num7 = rects[0].Rect.Height + rects[0].Rect.X;
											Rectangle rectangle2 = new Rectangle(clientRectangle.X, clientRectangle.Bottom - num7, clientRectangle.Width, num7);
											if (owner.scroll_show)
											{
												g.SetClip(owner.PaintExceedPre(rectangle2, rects[0].Rect.Height));
												g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
											}
											else
											{
												g.SetClip(rectangle2);
											}
											foreach (TabPage item in items)
											{
												if (((Control)item).Visible)
												{
													if (selectedIndex == num5)
													{
														tabPage = item;
													}
													else
													{
														GraphicsPath val12 = item.Rect.RoundPath(num, TL: false, TR: false, BR: true, BL: true);
														try
														{
															g.Fill((Brush)(object)((owner.hover_i == num5) ? val6 : val5), val12);
															if (num2 > 0)
															{
																g.Draw(border ?? Colour.BorderSecondary.Get("Tabs"), num2, val12);
															}
															if (owner.hover_i == num5)
															{
																PaintText(g, rects[num5], owner, item, item.MDown ? val3 : val4);
															}
															else
															{
																PaintText(g, rects[num5], owner, item, val);
															}
														}
														finally
														{
															((IDisposable)val12)?.Dispose();
														}
													}
												}
												num5++;
											}
											g.ResetClip();
											g.ResetTransform();
											if (tabPage == null)
											{
												break;
											}
											Rectangle rect2 = tabPage.Rect;
											GraphicsPath val13 = rect2.RoundPath(num, TL: false, TR: false, BR: true, BL: true);
											try
											{
												if (num2 > 0)
												{
													Pen val14 = new Pen(BorderActive ?? Colour.BorderColor.Get("Tabs"), (float)num2);
													try
													{
														float num8 = (float)rect2.Y + num4;
														g.DrawLine(val14, clientRectangle.X, num8, clientRectangle.Right, num8);
														if (owner.scroll_show)
														{
															g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
														}
														GraphicsPath val15 = new RectangleF((float)rect2.X + num4, (float)rect2.Y - num4, rect2.Width - num2, (float)rect2.Height + num4).RoundPath(num, TL: false, TR: false, BR: true, BL: true);
														try
														{
															g.Fill((Brush)(object)val7, val15);
														}
														finally
														{
															((IDisposable)val15)?.Dispose();
														}
														g.SetClip(new Rectangle(rect2.X - num2, rect2.Y + num2, rect2.Width + num3, rect2.Height + num2));
														g.Draw(val14, val13);
														g.ResetClip();
													}
													finally
													{
														((IDisposable)val14)?.Dispose();
													}
												}
												else
												{
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													g.Fill((Brush)(object)val7, val13);
												}
												PaintText(g, rects[selectedIndex], owner, tabPage, val2);
											}
											finally
											{
												((IDisposable)val13)?.Dispose();
											}
											break;
										}
										case 2:
										{
											Rectangle rectangle4 = new Rectangle(clientRectangle.X, clientRectangle.Y, rects[0].Rect.Right, clientRectangle.Height);
											if (owner.scroll_show)
											{
												g.SetClip(owner.PaintExceedPre(rectangle4, rects[0].Rect.Height));
												g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
											}
											else
											{
												g.SetClip(rectangle4);
											}
											foreach (TabPage item2 in items)
											{
												if (((Control)item2).Visible)
												{
													if (owner.SelectedIndex == num5)
													{
														tabPage = item2;
													}
													else
													{
														GraphicsPath val20 = item2.Rect.RoundPath(num, TL: true, TR: false, BR: false, BL: true);
														try
														{
															g.Fill((Brush)(object)((owner.hover_i == num5) ? val6 : val5), val20);
															if (num2 > 0)
															{
																g.Draw(border ?? Colour.BorderSecondary.Get("Tabs"), num2, val20);
															}
															if (owner.hover_i == num5)
															{
																PaintText(g, rects[num5], owner, item2, item2.MDown ? val3 : val4);
															}
															else
															{
																PaintText(g, rects[num5], owner, item2, val);
															}
														}
														finally
														{
															((IDisposable)val20)?.Dispose();
														}
													}
												}
												num5++;
											}
											g.ResetClip();
											g.ResetTransform();
											if (tabPage == null)
											{
												break;
											}
											Rectangle rect4 = tabPage.Rect;
											GraphicsPath val21 = rect4.RoundPath(num, TL: true, TR: false, BR: false, BL: true);
											try
											{
												if (num2 > 0)
												{
													Pen val22 = new Pen(BorderActive ?? Colour.BorderColor.Get("Tabs"), (float)num2);
													try
													{
														float num11 = (float)rect4.Right - num4;
														g.DrawLine(val22, num11, clientRectangle.Y, num11, clientRectangle.Bottom);
														if (owner.scroll_show)
														{
															g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
														}
														GraphicsPath val23 = new RectangleF((float)rect4.X - num4, (float)rect4.Y + num4, (float)rect4.Width + num4, rect4.Height - num2).RoundPath(num, TL: true, TR: false, BR: false, BL: true);
														try
														{
															g.Fill((Brush)(object)val7, val23);
														}
														finally
														{
															((IDisposable)val23)?.Dispose();
														}
														g.SetClip(new RectangleF((float)rect4.X - num4, rect4.Y - num2, rect4.Width, rect4.Height + num3));
														g.Draw(val22, val21);
														g.ResetClip();
													}
													finally
													{
														((IDisposable)val22)?.Dispose();
													}
												}
												else
												{
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													g.Fill((Brush)(object)val7, val21);
												}
												PaintText(g, rects[selectedIndex], owner, tabPage, val2);
											}
											finally
											{
												((IDisposable)val21)?.Dispose();
											}
											break;
										}
										case 3:
										{
											int num9 = rects[0].Rect.Width + rects[0].Rect.Y;
											Rectangle rectangle3 = new Rectangle(clientRectangle.Right - num9, clientRectangle.Y, num9, clientRectangle.Height);
											if (owner.scroll_show)
											{
												g.SetClip(owner.PaintExceedPre(rectangle3, rects[0].Rect.Height));
												g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
											}
											else
											{
												g.SetClip(rectangle3);
											}
											foreach (TabPage item3 in items)
											{
												if (((Control)item3).Visible)
												{
													if (owner.SelectedIndex == num5)
													{
														tabPage = item3;
													}
													else
													{
														GraphicsPath val16 = item3.Rect.RoundPath(num, TL: false, TR: true, BR: true, BL: false);
														try
														{
															g.Fill((Brush)(object)((owner.hover_i == num5) ? val6 : val5), val16);
															if (num2 > 0)
															{
																g.Draw(border ?? Colour.BorderSecondary.Get("Tabs"), num2, val16);
															}
															if (owner.hover_i == num5)
															{
																PaintText(g, rects[num5], owner, item3, item3.MDown ? val3 : val4);
															}
															else
															{
																PaintText(g, rects[num5], owner, item3, val);
															}
														}
														finally
														{
															((IDisposable)val16)?.Dispose();
														}
													}
												}
												num5++;
											}
											g.ResetClip();
											g.ResetTransform();
											if (tabPage == null)
											{
												break;
											}
											Rectangle rect3 = tabPage.Rect;
											GraphicsPath val17 = rect3.RoundPath(num, TL: false, TR: true, BR: true, BL: false);
											try
											{
												if (num2 > 0)
												{
													Pen val18 = new Pen(BorderActive ?? Colour.BorderColor.Get("Tabs"), (float)num2);
													try
													{
														float num10 = (float)rect3.X + num4;
														g.DrawLine(val18, num10, clientRectangle.Y, num10, clientRectangle.Bottom);
														if (owner.scroll_show)
														{
															g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
														}
														GraphicsPath val19 = new RectangleF((float)rect3.X - num4, (float)rect3.Y + num4, (float)rect3.Width + num4, rect3.Height - num2).RoundPath(num, TL: false, TR: true, BR: true, BL: false);
														try
														{
															g.Fill((Brush)(object)val7, val19);
														}
														finally
														{
															((IDisposable)val19)?.Dispose();
														}
														g.SetClip(new Rectangle(rect3.X + num2, rect3.Y - num2, rect3.Width + num2, rect3.Height + num3));
														g.Draw(val18, val17);
														g.ResetClip();
													}
													finally
													{
														((IDisposable)val18)?.Dispose();
													}
												}
												else
												{
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													g.Fill((Brush)(object)val7, val17);
												}
												PaintText(g, rects[selectedIndex], owner, tabPage, val2);
											}
											finally
											{
												((IDisposable)val17)?.Dispose();
											}
											break;
										}
										default:
										{
											Rectangle rectangle = new Rectangle(clientRectangle.X, clientRectangle.Y, clientRectangle.Width, rects[0].Rect.Bottom);
											if (owner.scroll_show)
											{
												g.SetClip(owner.PaintExceedPre(rectangle, rects[0].Rect.Height));
												g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
											}
											else
											{
												g.SetClip(rectangle);
											}
											foreach (TabPage item4 in items)
											{
												if (((Control)item4).Visible)
												{
													if (owner.SelectedIndex == num5)
													{
														tabPage = item4;
													}
													else
													{
														GraphicsPath val8 = item4.Rect.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
														try
														{
															g.Fill((Brush)(object)((owner.hover_i == num5) ? val6 : val5), val8);
															if (num2 > 0)
															{
																g.Draw(border ?? Colour.BorderSecondary.Get("Tabs"), num2, val8);
															}
															if (owner.hover_i == num5)
															{
																PaintText(g, rects[num5], owner, item4, item4.MDown ? val3 : val4);
															}
															else
															{
																PaintText(g, rects[num5], owner, item4, val);
															}
														}
														finally
														{
															((IDisposable)val8)?.Dispose();
														}
													}
												}
												num5++;
											}
											g.ResetClip();
											g.ResetTransform();
											if (tabPage == null)
											{
												break;
											}
											Rectangle rect = tabPage.Rect;
											GraphicsPath val9 = rect.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
											try
											{
												if (num2 > 0)
												{
													Pen val10 = new Pen(BorderActive ?? Colour.BorderColor.Get("Tabs"), (float)num2);
													try
													{
														float num6 = (float)rect.Bottom - num4;
														g.DrawLine(val10, clientRectangle.X, num6, clientRectangle.Right, num6);
														if (owner.scroll_show)
														{
															g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
														}
														GraphicsPath val11 = new RectangleF((float)rect.X + num4, (float)rect.Y - num4, rect.Width - num2, (float)rect.Height + num4).RoundPath(num, TL: true, TR: true, BR: false, BL: false);
														try
														{
															g.Fill((Brush)(object)val7, val11);
														}
														finally
														{
															((IDisposable)val11)?.Dispose();
														}
														g.SetClip(new Rectangle(rect.X - num2, rect.Y - num2, rect.Width + num3, rect.Height));
														g.Draw(val10, val9);
														g.ResetClip();
													}
													finally
													{
														((IDisposable)val10)?.Dispose();
													}
												}
												else
												{
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													g.Fill((Brush)(object)val7, val9);
												}
												PaintText(g, rects[selectedIndex], owner, tabPage, val2);
											}
											finally
											{
												((IDisposable)val9)?.Dispose();
											}
											break;
										}
										}
										if (owner.scroll_show)
										{
											owner.PaintExceed(g, val.Color, num, clientRectangle, rects[0].Rect, rects[rects.Length - 1].Rect, full: true);
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
		}

		public TabPageRect GetTabRect(int i)
		{
			return rects[i];
		}

		private Dictionary<TabPage, Size> GetDir(Tabs owner, Canvas g, TabCollection items, int gap, out int ico_size, out int close_size, out int sizewh)
		{
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Invalid comparison between Unknown and I4
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Invalid comparison between Unknown and I4
			sizewh = 0;
			Size size = g.MeasureString("龍Qq", ((Control)owner).Font);
			Dictionary<TabPage, Size> dictionary = new Dictionary<TabPage, Size>(items.Count);
			foreach (TabPage item in items)
			{
				Size value = g.MeasureString(((Control)item).Text, ((Control)owner).Font);
				dictionary.Add(item, value);
			}
			ico_size = (int)((float)size.Height * owner.IconRatio);
			close_size = (int)((float)size.Height * (owner.IconRatio * 0.8f));
			TabAlignment alignment = owner.Alignment;
			if ((int)alignment > 1 && alignment - 2 <= 1)
			{
				if (closable)
				{
					foreach (KeyValuePair<TabPage, Size> item2 in dictionary)
					{
						if (((Control)item2.Key).Visible)
						{
							int num = ((!item2.Key.HasIcon) ? (item2.Value.Width + gap) : (item2.Value.Width + ico_size + gap * 2));
							num += ico_size + gap;
							if (sizewh < num)
							{
								sizewh = num;
							}
						}
					}
				}
				else
				{
					foreach (KeyValuePair<TabPage, Size> item3 in dictionary)
					{
						if (((Control)item3.Key).Visible)
						{
							int num2 = ((!item3.Key.HasIcon) ? (item3.Value.Width + gap) : (item3.Value.Width + ico_size + gap * 2));
							if (sizewh < num2)
							{
								sizewh = num2;
							}
						}
					}
				}
			}
			else
			{
				foreach (KeyValuePair<TabPage, Size> item4 in dictionary)
				{
					if (((Control)item4.Key).Visible)
					{
						int num3 = item4.Value.Height + gap;
						if (sizewh < num3)
						{
							sizewh = num3;
						}
					}
				}
			}
			return owner.HandItemSize(dictionary, ref sizewh);
		}

		private void PaintText(Canvas g, TabPageRect rects, Tabs owner, TabPage page, SolidBrush brush)
		{
			if (page.HasIcon)
			{
				if (page.Icon != null)
				{
					g.Image(page.Icon, rects.Rect_Ico);
				}
				else if (page.IconSvg != null)
				{
					g.GetImgExtend(page.IconSvg, rects.Rect_Ico, brush.Color);
				}
			}
			if (closable)
			{
				if (rects.hover_close == null)
				{
					g.PaintIconClose(rects.Rect_Close, Colour.TextQuaternary.Get("Tabs"));
				}
				else if (rects.hover_close.Animation)
				{
					g.PaintIconClose(rects.Rect_Close, Helper.ToColor(rects.hover_close.Value + Colour.TextQuaternary.Get("Tabs").A, Colour.Text.Get("Tabs")));
				}
				else if (rects.hover_close.Switch)
				{
					g.PaintIconClose(rects.Rect_Close, Colour.Text.Get("Tabs"));
				}
				else
				{
					g.PaintIconClose(rects.Rect_Close, Colour.TextQuaternary.Get("Tabs"));
				}
			}
			g.String(((Control)page).Text, ((Control)owner).Font, (Brush)(object)brush, rects.Rect_Text, owner.s_c);
			owner.PaintBadge(g, page, rects.Rect_Text);
		}

		public void SelectedIndexChanged(int i, int old)
		{
			Tabs? tabs = owner;
			if (tabs != null)
			{
				((Control)tabs).Invalidate();
			}
		}

		public void Dispose()
		{
			TabPageRect[] array = rects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].hover_close?.Dispose();
			}
		}

		public bool MouseClick(TabPage page, int i, int x, int y)
		{
			if (owner == null)
			{
				return false;
			}
			if (closable && !page.ReadOnly && rects[i].Rect_Close.Contains(x, y))
			{
				bool flag = true;
				if (owner.ClosingPage != null)
				{
					flag = owner.ClosingPage(owner, new ClosingPageEventArgs(page));
				}
				if (flag)
				{
					owner.Pages.Remove(page);
				}
				return true;
			}
			return false;
		}

		public void MouseMove(int x, int y)
		{
			if (owner == null || !closable)
			{
				return;
			}
			int num = 0;
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				if (tabPageRect.hover_close == null)
				{
					tabPageRect.hover_close = new ITaskOpacity((IControl)owner);
				}
				if (num == owner.hover_i)
				{
					tabPageRect.hover_close.MaxValue = Colour.Text.Get("Tabs").A - Colour.TextQuaternary.Get("Tabs").A;
					tabPageRect.hover_close.Switch = tabPageRect.Rect_Close.Contains(x, y);
				}
				else
				{
					tabPageRect.hover_close.Switch = false;
				}
				num++;
			}
		}

		public void MouseLeave()
		{
			if (!closable)
			{
				return;
			}
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				if (tabPageRect.hover_close != null)
				{
					tabPageRect.hover_close.Switch = false;
				}
			}
		}

		public void MouseWheel(int delta)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Invalid comparison between Unknown and I4
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			if (owner != null && owner.scroll_show)
			{
				TabAlignment alignment = owner.Alignment;
				if ((int)alignment > 1 && alignment - 2 <= 1)
				{
					owner.scroll_x = 0;
					owner.scroll_y -= delta;
				}
				else
				{
					owner.scroll_y = 0;
					owner.scroll_x -= delta;
				}
			}
		}
	}

	public class StyleCard2 : IStyle
	{
		public enum CloseType
		{
			none,
			always,
			activate
		}

		private Tabs? owner;

		private int radius = 6;

		private int bordersize = 1;

		private Color? border;

		private Color? fill;

		private int gap = 2;

		private CloseType closable;

		private TabPageRect[] rects = new TabPageRect[0];

		[Description("卡片圆角")]
		[Category("卡片")]
		[DefaultValue(6)]
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
					Tabs? tabs = owner;
					if (tabs != null)
					{
						((Control)tabs).Invalidate();
					}
				}
			}
		}

		[Description("边框大小")]
		[Category("卡片")]
		[DefaultValue(1)]
		public int Border
		{
			get
			{
				return bordersize;
			}
			set
			{
				if (bordersize != value)
				{
					bordersize = value;
					owner?.LoadLayout();
				}
			}
		}

		[Description("卡片边框颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? BorderColor
		{
			get
			{
				return border;
			}
			set
			{
				if (!(border == value))
				{
					border = value;
					Tabs? tabs = owner;
					if (tabs != null)
					{
						((Control)tabs).Invalidate();
					}
				}
			}
		}

		[Description("卡片边框激活颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? BorderActive { get; set; }

		[Description("卡片颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? Fill
		{
			get
			{
				return fill;
			}
			set
			{
				if (!(fill == value))
				{
					fill = value;
					Tabs? tabs = owner;
					if (tabs != null)
					{
						((Control)tabs).Invalidate();
					}
				}
			}
		}

		[Description("卡片悬停颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? FillHover { get; set; }

		[Description("卡片激活颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? FillActive { get; set; }

		[Description("卡片间距")]
		[Category("卡片")]
		[DefaultValue(2)]
		public int Gap
		{
			get
			{
				return gap;
			}
			set
			{
				if (gap != value)
				{
					gap = value;
					owner?.LoadLayout();
				}
			}
		}

		[Description("可关闭")]
		[Category("卡片")]
		[DefaultValue(false)]
		public CloseType Closable
		{
			get
			{
				return closable;
			}
			set
			{
				if (closable != value)
				{
					closable = value;
					owner?.LoadLayout();
				}
			}
		}

		public StyleCard2()
		{
		}

		public StyleCard2(Tabs tabs)
		{
			owner = tabs;
		}

		public void LoadLayout(Tabs tabs, Rectangle rect, TabCollection items)
		{
			Tabs tabs2 = tabs;
			TabCollection items2 = items;
			owner = tabs2;
			rects = Helper.GDI(delegate(Canvas g)
			{
				//IL_0816: Unknown result type (might be due to invalid IL or missing references)
				//IL_081b: Unknown result type (might be due to invalid IL or missing references)
				//IL_081d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0834: Expected I4, but got Unknown
				//IL_007a: Unknown result type (might be due to invalid IL or missing references)
				//IL_007f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0081: Unknown result type (might be due to invalid IL or missing references)
				//IL_0098: Expected I4, but got Unknown
				//IL_0e14: Unknown result type (might be due to invalid IL or missing references)
				//IL_0e19: Unknown result type (might be due to invalid IL or missing references)
				//IL_0e1b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0e1e: Invalid comparison between Unknown and I4
				//IL_0e23: Unknown result type (might be due to invalid IL or missing references)
				//IL_0e26: Unknown result type (might be due to invalid IL or missing references)
				//IL_0e28: Invalid comparison between Unknown and I4
				int num = (int)((float)tabs2.Gap * Config.Dpi);
				int gapI = num / 2;
				int num2 = 0;
				int sizewh = 0;
				int num3 = (int)((float)Gap * Config.Dpi);
				List<TabPageRect> list = new List<TabPageRect>(items2.Count);
				int ico_size;
				int close_size;
				Dictionary<TabPage, Size> dir = GetDir(tabs2, g, items2, num, out ico_size, out close_size, out sizewh);
				if (closable != 0)
				{
					TabAlignment alignment = tabs2.Alignment;
					switch ((int)alignment)
					{
					case 1:
					{
						int y = rect.Bottom - sizewh;
						foreach (KeyValuePair<TabPage, Size> item in dir)
						{
							if (((Control)item.Key).Visible)
							{
								Rectangle rectangle4 = default(Rectangle);
								if (item.Key.ReadOnly)
								{
									if (item.Key.HasIcon)
									{
										rectangle4 = new Rectangle(rect.X + num2, y, item.Value.Width + num + ico_size + num, sizewh);
										list.Add(new TabPageRect(rectangle4, item.Value, ico_size, num, gapI));
									}
									else
									{
										rectangle4 = new Rectangle(rect.X + num2, y, item.Value.Width + num, sizewh);
										list.Add(new TabPageRect(rectangle4));
									}
								}
								else if (item.Key.HasIcon)
								{
									rectangle4 = new Rectangle(rect.X + num2, y, item.Value.Width + num + ico_size + close_size + num * 2, sizewh);
									list.Add(new TabPageRect(rectangle4, item.Value, ico_size, close_size, num, gapI));
								}
								else
								{
									rectangle4 = new Rectangle(rect.X + num2, y, item.Value.Width + num + close_size + num, sizewh);
									list.Add(new TabPageRect(rectangle4, test: false, item.Value, close_size, num, gapI));
								}
								item.Key.SetRect(rectangle4);
								num2 += rectangle4.Width + num3;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num2 -= num3;
						tabs2.SetPadding(0, 0, 0, sizewh + 2);
						owner.scroll_max = num2 - rect.Width;
						owner.scroll_show = num2 > rect.Width;
						break;
					}
					case 2:
						foreach (KeyValuePair<TabPage, Size> item2 in dir)
						{
							if (((Control)item2.Key).Visible)
							{
								Rectangle rectangle2 = new Rectangle(rect.X, rect.Y + num2, sizewh, item2.Value.Height + num);
								if (item2.Key.ReadOnly)
								{
									if (item2.Key.HasIcon)
									{
										list.Add(new TabPageRect(rectangle2, item2.Value, ico_size, num, gapI));
									}
									else
									{
										list.Add(new TabPageRect(rectangle2));
									}
								}
								else if (item2.Key.HasIcon)
								{
									list.Add(new TabPageRect(rectangle2, item2.Value, ico_size, close_size, num, gapI));
								}
								else
								{
									list.Add(new TabPageRect(rectangle2, test: false, item2.Value, close_size, num, gapI));
								}
								item2.Key.SetRect(rectangle2);
								num2 += rectangle2.Height + num3;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num2 -= num3;
						tabs2.SetPadding(sizewh + 2, 0, 0, 0);
						owner.scroll_max = num2 - rect.Height;
						owner.scroll_show = num2 > rect.Height;
						break;
					case 3:
					{
						int x = rect.Right - sizewh;
						foreach (KeyValuePair<TabPage, Size> item3 in dir)
						{
							if (((Control)item3.Key).Visible)
							{
								Rectangle rectangle3 = new Rectangle(x, rect.Y + num2, sizewh, item3.Value.Height + num);
								if (item3.Key.ReadOnly)
								{
									if (item3.Key.HasIcon)
									{
										list.Add(new TabPageRect(rectangle3, item3.Value, ico_size, num, gapI));
									}
									else
									{
										list.Add(new TabPageRect(rectangle3));
									}
								}
								else if (item3.Key.HasIcon)
								{
									list.Add(new TabPageRect(rectangle3, item3.Value, ico_size, close_size, num, gapI));
								}
								else
								{
									list.Add(new TabPageRect(rectangle3, test: false, item3.Value, close_size, num, gapI));
								}
								item3.Key.SetRect(rectangle3);
								num2 += rectangle3.Height + num3;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num2 -= num3;
						tabs2.SetPadding(0, 0, sizewh + 2, 0);
						owner.scroll_max = num2 - rect.Height;
						owner.scroll_show = num2 > rect.Height;
						break;
					}
					default:
						foreach (KeyValuePair<TabPage, Size> item4 in dir)
						{
							if (((Control)item4.Key).Visible)
							{
								Rectangle rectangle = default(Rectangle);
								if (item4.Key.ReadOnly)
								{
									if (item4.Key.HasIcon)
									{
										rectangle = new Rectangle(rect.X + num2, rect.Y, item4.Value.Width + num + ico_size + num, sizewh);
										list.Add(new TabPageRect(rectangle, item4.Value, ico_size, num, gapI));
									}
									else
									{
										rectangle = new Rectangle(rect.X + num2, rect.Y, item4.Value.Width + num, sizewh);
										list.Add(new TabPageRect(rectangle));
									}
								}
								else if (item4.Key.HasIcon)
								{
									rectangle = new Rectangle(rect.X + num2, rect.Y, item4.Value.Width + num + ico_size + close_size + num * 2, sizewh);
									list.Add(new TabPageRect(rectangle, item4.Value, ico_size, close_size, num, gapI));
								}
								else
								{
									rectangle = new Rectangle(rect.X + num2, rect.Y, item4.Value.Width + num + close_size + num, sizewh);
									list.Add(new TabPageRect(rectangle, test: false, item4.Value, close_size, num, gapI));
								}
								item4.Key.SetRect(rectangle);
								num2 += rectangle.Width + num3;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num2 -= num3;
						tabs2.SetPadding(0, sizewh + 2, 0, 0);
						owner.scroll_max = num2 - rect.Width;
						owner.scroll_show = num2 > rect.Width;
						break;
					}
				}
				else
				{
					TabAlignment alignment = tabs2.Alignment;
					switch ((int)alignment)
					{
					case 1:
					{
						int num4 = rect.Bottom - sizewh;
						foreach (KeyValuePair<TabPage, Size> item5 in dir)
						{
							if (((Control)item5.Key).Visible)
							{
								Rectangle rectangle8;
								if (item5.Key.HasIcon)
								{
									rectangle8 = new Rectangle(rect.X + num2, num4, item5.Value.Width + num + ico_size + num, sizewh);
									list.Add(new TabPageRect(rectangle8, item5.Value, ico_size, num, gapI));
								}
								else
								{
									rectangle8 = new Rectangle(rect.X + num2, num4 + 2 + bordersize, item5.Value.Width + num, sizewh);
									list.Add(new TabPageRect(rectangle8));
								}
								item5.Key.SetRect(rectangle8);
								num2 += rectangle8.Width + num3;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num2 -= num3;
						tabs2.SetPadding(0, 0, 0, sizewh + 2);
						owner.scroll_max = num2 - rect.Width;
						owner.scroll_show = num2 > rect.Width;
						break;
					}
					case 2:
						foreach (KeyValuePair<TabPage, Size> item6 in dir)
						{
							if (((Control)item6.Key).Visible)
							{
								Rectangle rectangle6 = new Rectangle(rect.X, rect.Y + num2, sizewh, item6.Value.Height + num);
								if (item6.Key.HasIcon)
								{
									list.Add(new TabPageRect(rectangle6, item6.Value, ico_size, num, gapI));
								}
								else
								{
									list.Add(new TabPageRect(rectangle6));
								}
								item6.Key.SetRect(rectangle6);
								num2 += rectangle6.Height + num3;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num2 -= num3;
						tabs2.SetPadding(sizewh + 2, 0, 0, 0);
						owner.scroll_max = num2 - rect.Height;
						owner.scroll_show = num2 > rect.Height;
						break;
					case 3:
					{
						int x2 = rect.Right - sizewh;
						foreach (KeyValuePair<TabPage, Size> item7 in dir)
						{
							if (((Control)item7.Key).Visible)
							{
								Rectangle rectangle7 = new Rectangle(x2, rect.Y + num2, sizewh, item7.Value.Height + num);
								if (item7.Key.HasIcon)
								{
									list.Add(new TabPageRect(rectangle7, item7.Value, ico_size, num, gapI));
								}
								else
								{
									list.Add(new TabPageRect(rectangle7));
								}
								item7.Key.SetRect(rectangle7);
								num2 += rectangle7.Height + num3;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num2 -= num3;
						tabs2.SetPadding(0, 0, sizewh + 2, 0);
						owner.scroll_max = num2 - rect.Height;
						owner.scroll_show = num2 > rect.Height;
						break;
					}
					default:
						foreach (KeyValuePair<TabPage, Size> item8 in dir)
						{
							if (((Control)item8.Key).Visible)
							{
								Rectangle rectangle5;
								if (item8.Key.HasIcon)
								{
									rectangle5 = new Rectangle(rect.X + num2, rect.Y, item8.Value.Width + num + ico_size + num, sizewh);
									list.Add(new TabPageRect(rectangle5, item8.Value, ico_size, num, gapI));
								}
								else
								{
									rectangle5 = new Rectangle(rect.X + num2, rect.Y, item8.Value.Width + num, sizewh);
									list.Add(new TabPageRect(rectangle5));
								}
								item8.Key.SetRect(rectangle5);
								num2 += rectangle5.Width + num3;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num2 -= num3;
						tabs2.SetPadding(0, sizewh + 2, 0, 0);
						owner.scroll_max = num2 - rect.Width;
						owner.scroll_show = num2 > rect.Width;
						break;
					}
				}
				if (owner.scroll_show)
				{
					owner.scroll_max += owner.SizeExceed(((Control)owner).ClientRectangle, list[0].Rect, list[list.Count - 1].Rect);
				}
				else
				{
					Tabs? tabs3 = owner;
					int scroll_x = (owner.scroll_y = 0);
					tabs3.scroll_x = scroll_x;
					if (tabs2.centered)
					{
						TabAlignment alignment = tabs2.Alignment;
						if ((int)alignment > 1 && alignment - 2 <= 1)
						{
							int y2 = (rect.Height - num2) / 2;
							foreach (KeyValuePair<TabPage, Size> item9 in dir)
							{
								item9.Key.SetOffset(0, y2);
							}
							foreach (TabPageRect item10 in list)
							{
								item10.Rect.Offset(0, y2);
								item10.Rect_Text.Offset(0, y2);
								item10.Rect_Ico.Offset(0, y2);
								item10.Rect_Close.Offset(0, y2);
							}
						}
						else
						{
							int x3 = (rect.Width - num2) / 2;
							foreach (KeyValuePair<TabPage, Size> item11 in dir)
							{
								item11.Key.SetOffset(x3, 0);
							}
							foreach (TabPageRect item12 in list)
							{
								item12.Rect.Offset(x3, 0);
								item12.Rect_Text.Offset(x3, 0);
								item12.Rect_Ico.Offset(x3, 0);
								item12.Rect_Close.Offset(x3, 0);
							}
						}
					}
				}
				return list.ToArray();
			});
		}

		public void Paint(Tabs owner, Canvas g, TabCollection items)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected O, but got Unknown
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Expected O, but got Unknown
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Expected O, but got Unknown
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Expected O, but got Unknown
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Expected I4, but got Unknown
			if (rects.Length != items.Count)
			{
				return;
			}
			SolidBrush val = new SolidBrush(owner.ForeColor ?? Colour.Text.Get("Tabs"));
			try
			{
				SolidBrush val2 = new SolidBrush(owner.Fill ?? Colour.Primary.Get("Tabs"));
				try
				{
					SolidBrush val3 = new SolidBrush(owner.FillActive ?? Colour.PrimaryActive.Get("Tabs"));
					try
					{
						SolidBrush val4 = new SolidBrush(owner.FillHover ?? Colour.PrimaryHover.Get("Tabs"));
						try
						{
							SolidBrush val5 = new SolidBrush(Fill ?? Colour.FillQuaternary.Get("Tabs"));
							try
							{
								SolidBrush val6 = new SolidBrush(FillHover ?? Colour.FillQuaternary.Get("Tabs"));
								try
								{
									SolidBrush val7 = new SolidBrush(FillActive ?? Colour.BgContainer.Get("Tabs"));
									try
									{
										Rectangle clientRectangle = ((Control)owner).ClientRectangle;
										int num = (int)((float)Radius * Config.Dpi);
										int num2 = (int)((float)bordersize * Config.Dpi);
										_ = num2 * 6;
										float num3 = (float)num2 / 2f;
										TabPage tabPage = null;
										int num4 = 0;
										int selectedIndex = owner.SelectedIndex;
										TabAlignment alignment = owner.Alignment;
										switch ((int)alignment)
										{
										case 1:
										{
											int num5 = rects[0].Rect.Height + rects[0].Rect.X;
											Rectangle rectangle2 = new Rectangle(clientRectangle.X, clientRectangle.Bottom - num5, clientRectangle.Width, num5);
											if (owner.scroll_show)
											{
												g.SetClip(owner.PaintExceedPre(rectangle2, rects[0].Rect.Height));
												g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
											}
											else
											{
												g.SetClip(rectangle2);
											}
											foreach (TabPage item in items)
											{
												if (((Control)item).Visible)
												{
													if (selectedIndex == num4)
													{
														tabPage = item;
													}
													else
													{
														GraphicsPath val11 = item.Rect.RoundPath(num, TL: true, TR: true, BR: true, BL: true);
														try
														{
															g.Fill((Brush)(object)((owner.hover_i == num4) ? val6 : val5), val11);
															if (num2 > 0)
															{
																g.Draw(border ?? Colour.BorderSecondary.Get("Tabs"), num2, val11);
															}
															if (owner.hover_i == num4)
															{
																PaintText(g, rects[num4], owner, item, item.MDown ? val3 : val4, closshow: true);
															}
															else
															{
																PaintText(g, rects[num4], owner, item, val, closable == CloseType.always);
															}
														}
														finally
														{
															((IDisposable)val11)?.Dispose();
														}
													}
												}
												num4++;
											}
											g.ResetTransform();
											if (tabPage == null)
											{
												break;
											}
											Rectangle rect2 = tabPage.Rect;
											GraphicsPath val12 = rect2.RoundPath(num, TL: true, TR: true, BR: true, BL: true);
											try
											{
												if (num2 > 0)
												{
													_ = rect2.Y;
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													GraphicsPath val13 = new RectangleF((float)rect2.X + num3, (float)rect2.Y - num3, rect2.Width - num2, (float)rect2.Height + num3).RoundPath(num, TL: true, TR: true, BR: true, BL: true);
													try
													{
														g.Fill((Brush)(object)val7, val13);
													}
													finally
													{
														((IDisposable)val13)?.Dispose();
													}
													g.Draw(BorderActive ?? Colour.BorderColor.Get("Tabs"), num2, val12);
												}
												else
												{
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													g.Fill((Brush)(object)val7, val12);
												}
												PaintText(g, rects[selectedIndex], owner, tabPage, val2, closshow: true);
											}
											finally
											{
												((IDisposable)val12)?.Dispose();
											}
											break;
										}
										case 2:
										{
											Rectangle rectangle4 = new Rectangle(clientRectangle.X, clientRectangle.Y, rects[0].Rect.Right, clientRectangle.Height);
											if (owner.scroll_show)
											{
												g.SetClip(owner.PaintExceedPre(rectangle4, rects[0].Rect.Height));
												g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
											}
											else
											{
												g.SetClip(rectangle4);
											}
											foreach (TabPage item2 in items)
											{
												if (((Control)item2).Visible)
												{
													if (owner.SelectedIndex == num4)
													{
														tabPage = item2;
													}
													else
													{
														GraphicsPath val17 = item2.Rect.RoundPath(num, TL: true, TR: true, BR: true, BL: true);
														try
														{
															g.Fill((Brush)(object)((owner.hover_i == num4) ? val6 : val5), val17);
															if (num2 > 0)
															{
																g.Draw(border ?? Colour.BorderSecondary.Get("Tabs"), num2, val17);
															}
															if (owner.hover_i == num4)
															{
																PaintText(g, rects[num4], owner, item2, item2.MDown ? val3 : val4, closshow: true);
															}
															else
															{
																PaintText(g, rects[num4], owner, item2, val, closable == CloseType.always);
															}
														}
														finally
														{
															((IDisposable)val17)?.Dispose();
														}
													}
												}
												num4++;
											}
											g.ResetTransform();
											if (tabPage == null)
											{
												break;
											}
											Rectangle rect4 = tabPage.Rect;
											GraphicsPath val18 = rect4.RoundPath(num, TL: true, TR: true, BR: true, BL: true);
											try
											{
												if (num2 > 0)
												{
													_ = rect4.Right;
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													GraphicsPath val19 = new RectangleF((float)rect4.X - num3, (float)rect4.Y + num3, (float)rect4.Width + num3, rect4.Height - num2).RoundPath(num, TL: true, TR: true, BR: true, BL: true);
													try
													{
														g.Fill((Brush)(object)val7, val19);
													}
													finally
													{
														((IDisposable)val19)?.Dispose();
													}
													g.Draw(BorderActive ?? Colour.BorderColor.Get("Tabs"), num2, val18);
												}
												else
												{
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													g.Fill((Brush)(object)val7, val18);
												}
												PaintText(g, rects[selectedIndex], owner, tabPage, val2, closshow: true);
											}
											finally
											{
												((IDisposable)val18)?.Dispose();
											}
											break;
										}
										case 3:
										{
											int num6 = rects[0].Rect.Width + rects[0].Rect.Y;
											Rectangle rectangle3 = new Rectangle(clientRectangle.Right - num6, clientRectangle.Y, num6, clientRectangle.Height);
											if (owner.scroll_show)
											{
												g.SetClip(owner.PaintExceedPre(rectangle3, rects[0].Rect.Height));
												g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
											}
											else
											{
												g.SetClip(rectangle3);
											}
											foreach (TabPage item3 in items)
											{
												if (((Control)item3).Visible)
												{
													if (owner.SelectedIndex == num4)
													{
														tabPage = item3;
													}
													else
													{
														GraphicsPath val14 = item3.Rect.RoundPath(num, TL: true, TR: true, BR: true, BL: true);
														try
														{
															g.Fill((Brush)(object)((owner.hover_i == num4) ? val6 : val5), val14);
															if (num2 > 0)
															{
																g.Draw(border ?? Colour.BorderSecondary.Get("Tabs"), num2, val14);
															}
															if (owner.hover_i == num4)
															{
																PaintText(g, rects[num4], owner, item3, item3.MDown ? val3 : val4, closshow: true);
															}
															else
															{
																PaintText(g, rects[num4], owner, item3, val, closable == CloseType.always);
															}
														}
														finally
														{
															((IDisposable)val14)?.Dispose();
														}
													}
												}
												num4++;
											}
											g.ResetTransform();
											if (tabPage == null)
											{
												break;
											}
											Rectangle rect3 = tabPage.Rect;
											GraphicsPath val15 = rect3.RoundPath(num, TL: true, TR: true, BR: true, BL: true);
											try
											{
												if (num2 > 0)
												{
													_ = rect3.X;
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													GraphicsPath val16 = new RectangleF((float)rect3.X - num3, (float)rect3.Y + num3, (float)rect3.Width + num3, rect3.Height - num2).RoundPath(num, TL: true, TR: true, BR: true, BL: true);
													try
													{
														g.Fill((Brush)(object)val7, val16);
													}
													finally
													{
														((IDisposable)val16)?.Dispose();
													}
													g.Draw(BorderActive ?? Colour.BorderColor.Get("Tabs"), num2, val15);
												}
												else
												{
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													g.Fill((Brush)(object)val7, val15);
												}
												PaintText(g, rects[selectedIndex], owner, tabPage, val2, closshow: true);
											}
											finally
											{
												((IDisposable)val15)?.Dispose();
											}
											break;
										}
										default:
										{
											Rectangle rectangle = new Rectangle(clientRectangle.X, clientRectangle.Y, clientRectangle.Width, rects[0].Rect.Bottom);
											if (owner.scroll_show)
											{
												g.SetClip(owner.PaintExceedPre(rectangle, rects[0].Rect.Height));
												g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
											}
											else
											{
												g.SetClip(rectangle);
											}
											foreach (TabPage item4 in items)
											{
												if (((Control)item4).Visible)
												{
													if (owner.SelectedIndex == num4)
													{
														tabPage = item4;
													}
													else
													{
														GraphicsPath val8 = item4.Rect.RoundPath(num, TL: true, TR: true, BR: true, BL: true);
														try
														{
															g.Fill((Brush)(object)((owner.hover_i == num4) ? val6 : val5), val8);
															if (num2 > 0)
															{
																g.Draw(border ?? Colour.BorderSecondary.Get("Tabs"), num2, val8);
															}
															if (owner.hover_i == num4)
															{
																PaintText(g, rects[num4], owner, item4, item4.MDown ? val3 : val4, closshow: true);
															}
															else
															{
																PaintText(g, rects[num4], owner, item4, val, (closable == CloseType.always && !item4.ReadOnly) ? true : false);
															}
														}
														finally
														{
															((IDisposable)val8)?.Dispose();
														}
													}
												}
												num4++;
											}
											g.ResetTransform();
											if (tabPage == null)
											{
												break;
											}
											Rectangle rect = tabPage.Rect;
											GraphicsPath val9 = rect.RoundPath(num, TL: true, TR: true, BR: true, BL: true);
											try
											{
												if (num2 > 0)
												{
													_ = rect.Bottom;
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													GraphicsPath val10 = new RectangleF((float)rect.X + num3, (float)rect.Y - num3, rect.Width - num2, (float)rect.Height + num3).RoundPath(num, TL: true, TR: true, BR: true, BL: true);
													try
													{
														g.Fill((Brush)(object)val7, val10);
													}
													finally
													{
														((IDisposable)val10)?.Dispose();
													}
													g.Draw(BorderActive ?? Colour.BorderColor.Get("Tabs"), num2, val9);
												}
												else
												{
													if (owner.scroll_show)
													{
														g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
													}
													g.Fill((Brush)(object)val7, val9);
												}
												PaintText(g, rects[selectedIndex], owner, tabPage, val2, !tabPage.ReadOnly);
											}
											finally
											{
												((IDisposable)val9)?.Dispose();
											}
											break;
										}
										}
										if (owner.scroll_show)
										{
											owner.PaintExceed(g, val.Color, num, clientRectangle, rects[0].Rect, rects[rects.Length - 1].Rect, full: true);
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
		}

		public TabPageRect GetTabRect(int i)
		{
			return rects[i];
		}

		private Dictionary<TabPage, Size> GetDir(Tabs owner, Canvas g, TabCollection items, int gap, out int ico_size, out int close_size, out int sizewh)
		{
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Invalid comparison between Unknown and I4
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Invalid comparison between Unknown and I4
			sizewh = 0;
			Size size = g.MeasureString("龍Qq", ((Control)owner).Font);
			Dictionary<TabPage, Size> dictionary = new Dictionary<TabPage, Size>(items.Count);
			foreach (TabPage item in items)
			{
				Size value = g.MeasureString(((Control)item).Text, ((Control)owner).Font);
				dictionary.Add(item, value);
			}
			ico_size = (int)((float)size.Height * owner.IconRatio);
			close_size = (int)((float)size.Height * (owner.IconRatio * 0.8f));
			TabAlignment alignment = owner.Alignment;
			if ((int)alignment > 1 && alignment - 2 <= 1)
			{
				if (closable != 0)
				{
					foreach (KeyValuePair<TabPage, Size> item2 in dictionary)
					{
						if (((Control)item2.Key).Visible)
						{
							int num = ((!item2.Key.HasIcon) ? (item2.Value.Width + gap) : (item2.Value.Width + ico_size + gap * 2));
							num += ico_size + gap;
							if (sizewh < num)
							{
								sizewh = num;
							}
						}
					}
				}
				else
				{
					foreach (KeyValuePair<TabPage, Size> item3 in dictionary)
					{
						if (((Control)item3.Key).Visible)
						{
							int num2 = ((!item3.Key.HasIcon) ? (item3.Value.Width + gap) : (item3.Value.Width + ico_size + gap * 2));
							if (sizewh < num2)
							{
								sizewh = num2;
							}
						}
					}
				}
			}
			else
			{
				foreach (KeyValuePair<TabPage, Size> item4 in dictionary)
				{
					if (((Control)item4.Key).Visible)
					{
						int num3 = item4.Value.Height + gap;
						if (sizewh < num3)
						{
							sizewh = num3;
						}
					}
				}
			}
			return owner.HandItemSize(dictionary, ref sizewh);
		}

		private void PaintText(Canvas g, TabPageRect rects, Tabs owner, TabPage page, SolidBrush brush, bool closshow = false)
		{
			if (page.HasIcon)
			{
				if (page.Icon != null)
				{
					g.Image(page.Icon, rects.Rect_Ico);
				}
				else if (page.IconSvg != null)
				{
					g.GetImgExtend(page.IconSvg, rects.Rect_Ico, brush.Color);
				}
			}
			if (closable != CloseType.none && closshow)
			{
				if (rects.hover_close == null)
				{
					g.PaintIconClose(rects.Rect_Close, Colour.TextQuaternary.Get("Tabs"));
				}
				else if (rects.hover_close.Animation)
				{
					g.PaintIconClose(rects.Rect_Close, Helper.ToColor(rects.hover_close.Value + Colour.TextQuaternary.Get("Tabs").A, Colour.Text.Get("Tabs")));
				}
				else if (rects.hover_close.Switch)
				{
					g.PaintIconClose(rects.Rect_Close, Colour.Text.Get("Tabs"));
				}
				else
				{
					g.PaintIconClose(rects.Rect_Close, Colour.TextQuaternary.Get("Tabs"));
				}
			}
			g.String(((Control)page).Text, ((Control)owner).Font, (Brush)(object)brush, rects.Rect_Text, owner.s_c);
			owner.PaintBadge(g, page, rects.Rect_Text);
		}

		public void SelectedIndexChanged(int i, int old)
		{
			Tabs? tabs = owner;
			if (tabs != null)
			{
				((Control)tabs).Invalidate();
			}
		}

		public void Dispose()
		{
			TabPageRect[] array = rects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].hover_close?.Dispose();
			}
		}

		public bool MouseClick(TabPage page, int i, int x, int y)
		{
			if (owner == null)
			{
				return false;
			}
			if (closable != 0 && rects[i].Rect_Close.Contains(x, y))
			{
				bool flag = true;
				if (owner.ClosingPage != null)
				{
					flag = owner.ClosingPage(owner, new ClosingPageEventArgs(page));
				}
				if (flag)
				{
					owner.Pages.Remove(page);
				}
				return true;
			}
			return false;
		}

		public void MouseMove(int x, int y)
		{
			if (owner == null || closable == CloseType.none)
			{
				return;
			}
			int num = 0;
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				if (tabPageRect.hover_close == null)
				{
					tabPageRect.hover_close = new ITaskOpacity((IControl)owner);
				}
				if (num == owner.hover_i)
				{
					tabPageRect.hover_close.MaxValue = Colour.Text.Get("Tabs").A - Colour.TextQuaternary.Get("Tabs").A;
					tabPageRect.hover_close.Switch = tabPageRect.Rect_Close.Contains(x, y);
				}
				else
				{
					tabPageRect.hover_close.Switch = false;
				}
				num++;
			}
		}

		public bool MouseMovePre(int x, int y)
		{
			return false;
		}

		public void MouseLeave()
		{
			if (closable == CloseType.none)
			{
				return;
			}
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				if (tabPageRect.hover_close != null)
				{
					tabPageRect.hover_close.Switch = false;
				}
			}
		}

		public void MouseWheel(int delta)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Invalid comparison between Unknown and I4
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			if (owner != null && owner.scroll_show)
			{
				TabAlignment alignment = owner.Alignment;
				if ((int)alignment > 1 && alignment - 2 <= 1)
				{
					owner.scroll_x = 0;
					owner.scroll_y -= delta;
				}
				else
				{
					owner.scroll_y = 0;
					owner.scroll_x -= delta;
				}
			}
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface IStyle
	{
		void LoadLayout(Tabs owner, Rectangle rect, TabCollection items);

		void Paint(Tabs owner, Canvas g, TabCollection items);

		void SelectedIndexChanged(int i, int old);

		bool MouseClick(TabPage page, int i, int x, int y);

		void MouseMove(int x, int y);

		void MouseLeave();

		void MouseWheel(int delta);

		void Dispose();

		TabPageRect GetTabRect(int i);
	}

	private Color? fore;

	private Color? fill;

	private TabAlignment alignment;

	private bool centered;

	private TabTypExceed typExceed = TabTypExceed.Button;

	private Color? scrollback;

	private Color? scrollfore;

	private IStyle style;

	private TabType type;

	private int _gap = 8;

	private float iconratio = 0.7f;

	private bool _tabMenuVisible = true;

	private int? _itemSize;

	private TabCollection? items;

	private int _select;

	private bool showok;

	private Padding _padding = new Padding(0);

	private readonly StringFormat s_c = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

	private readonly StringFormat s_f = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private int hover_i = -1;

	internal bool scroll_show;

	private int _scroll_x;

	private int _scroll_y;

	private int scroll_max;

	private LayeredFormSelectDown? subForm;

	private Bitmap? bitblock_l;

	private Bitmap? bitblock_r;

	private Rectangle rect_l;

	private Rectangle rect_r;

	private bool hover_l;

	private bool hover_r;

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

	[Description("颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Fill
	{
		get
		{
			return fill;
		}
		set
		{
			if (!(fill == value))
			{
				fill = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Fill");
			}
		}
	}

	[Description("悬停颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? FillHover { get; set; }

	[Description("激活颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? FillActive { get; set; }

	[Description("位置")]
	[Category("外观")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public TabAlignment Alignment
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return alignment;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (alignment != value)
			{
				alignment = value;
				LoadLayout();
				OnPropertyChanged("Alignment");
			}
		}
	}

	[Description("标签居中展示")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Centered
	{
		get
		{
			return centered;
		}
		set
		{
			if (centered != value)
			{
				centered = value;
				LoadLayout();
				OnPropertyChanged("Centered");
			}
		}
	}

	[Description("超出UI类型")]
	[Category("外观")]
	[DefaultValue(TabTypExceed.Button)]
	public TabTypExceed TypExceed
	{
		get
		{
			return typExceed;
		}
		set
		{
			if (typExceed != value)
			{
				typExceed = value;
				Bitmap? obj = bitblock_l;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				Bitmap? obj2 = bitblock_r;
				if (obj2 != null)
				{
					((Image)obj2).Dispose();
				}
				bitblock_l = (bitblock_r = null);
				LoadLayout();
				OnPropertyChanged("TypExceed");
			}
		}
	}

	[Description("滚动条颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ScrollBack
	{
		get
		{
			return scrollback;
		}
		set
		{
			if (!(scrollback == value))
			{
				scrollback = value;
				Bitmap? obj = bitblock_l;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				Bitmap? obj2 = bitblock_r;
				if (obj2 != null)
				{
					((Image)obj2).Dispose();
				}
				bitblock_l = (bitblock_r = null);
				((Control)this).Invalidate();
				OnPropertyChanged("ScrollBack");
			}
		}
	}

	[Description("滚动条文本颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ScrollFore
	{
		get
		{
			return scrollfore;
		}
		set
		{
			if (!(scrollfore == value))
			{
				scrollfore = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ScrollFore");
			}
		}
	}

	[Description("滚动条悬停文本颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ScrollForeHover { get; set; }

	[Description("滚动条悬停颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ScrollBackHover { get; set; }

	[Description("样式")]
	[Category("外观")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public IStyle Style
	{
		get
		{
			return style;
		}
		set
		{
			style = value;
		}
	}

	[Description("样式类型")]
	[Category("外观")]
	[DefaultValue(TabType.Line)]
	public TabType Type
	{
		get
		{
			return type;
		}
		set
		{
			if (type != value)
			{
				type = value;
				style = SetType(value);
				LoadLayout();
				OnPropertyChanged("Type");
			}
		}
	}

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(8)]
	public int Gap
	{
		get
		{
			return _gap;
		}
		set
		{
			if (_gap != value)
			{
				_gap = value;
				LoadLayout();
				OnPropertyChanged("Gap");
			}
		}
	}

	[Description("图标比例")]
	[Category("外观")]
	[DefaultValue(0.7f)]
	public float IconRatio
	{
		get
		{
			return iconratio;
		}
		set
		{
			if (iconratio != value)
			{
				iconratio = value;
				LoadLayout();
				OnPropertyChanged("IconRatio");
			}
		}
	}

	[Description("是否显示头")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool TabMenuVisible
	{
		get
		{
			return _tabMenuVisible;
		}
		set
		{
			_tabMenuVisible = value;
			LoadLayout();
			OnPropertyChanged("TabMenuVisible");
		}
	}

	[Description("自定义项大小")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? ItemSize
	{
		get
		{
			return _itemSize;
		}
		set
		{
			if (_itemSize != value)
			{
				_itemSize = value;
				LoadLayout();
				OnPropertyChanged("ItemSize");
			}
		}
	}

	public override Rectangle DisplayRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Margin, ((Control)this).Padding, _padding);

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("数据")]
	public TabCollection Pages
	{
		get
		{
			if (items == null)
			{
				items = new TabCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public TabPage? SelectedTab
	{
		get
		{
			if (items == null || items.Count <= _select || _select < 0)
			{
				return null;
			}
			return items[_select];
		}
		set
		{
			if (items != null && value != null)
			{
				int selectedIndex = items.IndexOf(value);
				SelectedIndex = selectedIndex;
				OnPropertyChanged("SelectedTab");
			}
		}
	}

	[Description("选中序号")]
	[Category("数据")]
	[DefaultValue(0)]
	public int SelectedIndex
	{
		get
		{
			return _select;
		}
		set
		{
			if (_select != value)
			{
				int select = _select;
				_select = value;
				style.SelectedIndexChanged(value, select);
				this.SelectedIndexChanged?.Invoke(this, new IntEventArgs(value));
				((Control)this).Invalidate();
				ShowPage(_select);
				OnPropertyChanged("SelectedIndex");
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ControlCollection Controls => ((Control)this).Controls;

	private int Hover_i
	{
		get
		{
			return hover_i;
		}
		set
		{
			if (hover_i != value)
			{
				hover_i = value;
				((Control)this).Invalidate();
			}
		}
	}

	private int scroll_x
	{
		get
		{
			return _scroll_x;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			else if (scroll_max > 0 && value > scroll_max)
			{
				value = scroll_max;
			}
			if (value != _scroll_x)
			{
				_scroll_x = value;
				((Control)this).Invalidate();
			}
		}
	}

	private int scroll_y
	{
		get
		{
			return _scroll_y;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			else if (scroll_max > 0 && value > scroll_max)
			{
				value = scroll_max;
			}
			if (value != _scroll_y)
			{
				_scroll_y = value;
				((Control)this).Invalidate();
			}
		}
	}

	private bool Hover_l
	{
		get
		{
			return hover_l;
		}
		set
		{
			if (hover_l != value)
			{
				hover_l = value;
				((Control)this).Invalidate();
			}
		}
	}

	private bool Hover_r
	{
		get
		{
			return hover_r;
		}
		set
		{
			if (hover_r != value)
			{
				hover_r = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("点击标签时发生")]
	[Category("行为")]
	public event TabsItemEventHandler? TabClick;

	[Description("SelectedIndex 属性值更改时发生")]
	[Category("行为")]
	public event IntEventHandler? SelectedIndexChanged;

	[Description("关闭页面前发生")]
	[Category("行为")]
	public event ClosingPageEventHandler? ClosingPage;

	public Tabs()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		style = SetType(type);
	}

	private IStyle SetType(TabType type)
	{
		switch (type)
		{
		case TabType.Card:
			if (style is StyleCard result2)
			{
				return result2;
			}
			return new StyleCard(this);
		case TabType.Card2:
			if (style is StyleCard2 result3)
			{
				return result3;
			}
			return new StyleCard2(this);
		default:
			if (style is StyleLine result)
			{
				return result;
			}
			return new StyleLine(this);
		}
	}

	internal Dictionary<TabPage, Size> HandItemSize(Dictionary<TabPage, Size> rect_dir, ref int sizewh)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Invalid comparison between Unknown and I4
		if (_itemSize.HasValue)
		{
			int num = (int)Math.Ceiling((float)_itemSize.Value * Config.Dpi);
			Dictionary<TabPage, Size> dictionary = new Dictionary<TabPage, Size>(rect_dir.Count);
			foreach (KeyValuePair<TabPage, Size> item in rect_dir)
			{
				dictionary.Add(item.Key, new Size(num, item.Value.Height));
			}
			if ((int)alignment == 2 || (int)alignment == 3)
			{
				sizewh = num;
			}
			return dictionary;
		}
		return rect_dir;
	}

	public void SelectTab(string tabPageName)
	{
		if (items == null)
		{
			return;
		}
		foreach (TabPage item in items)
		{
			if (((Control)item).Text == tabPageName)
			{
				SelectedTab = item;
				break;
			}
		}
	}

	public void SelectTab(TabPage tabPage)
	{
		SelectedTab = tabPage;
	}

	public void SelectTab(int index)
	{
		SelectedIndex = index;
	}

	internal void ShowPage(int index)
	{
		if (!showok || items == null)
		{
			return;
		}
		if (items.Count > 1)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (i != index)
				{
					items[i].SetDock(isdock: false);
				}
			}
		}
		if (items.Count > _select && _select >= 0)
		{
			items[_select].SetDock(isdock: true);
		}
	}

	protected override void Dispose(bool disposing)
	{
		style.Dispose();
		Bitmap? obj = bitblock_l;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		Bitmap? obj2 = bitblock_r;
		if (obj2 != null)
		{
			((Image)obj2).Dispose();
		}
		if (items == null || items.Count == 0)
		{
			return;
		}
		foreach (TabPage item in items)
		{
			((Component)(object)item).Dispose();
		}
		items.Clear();
		base.Dispose(disposing);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		LoadLayout(r: false);
		showok = true;
		ShowPage(_select);
	}

	protected override void OnMarginChanged(EventArgs e)
	{
		LoadLayout();
		((Control)this).OnMarginChanged(e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		LoadLayout(r: false);
		((Control)this).OnFontChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		LoadLayout(r: false);
		((Control)this).OnSizeChanged(e);
	}

	private bool SetPadding(int x, int y, int r, int b)
	{
		if (((Padding)(ref _padding)).Left == x && ((Padding)(ref _padding)).Top == y && ((Padding)(ref _padding)).Right == r && ((Padding)(ref _padding)).Bottom == b)
		{
			return true;
		}
		((Padding)(ref _padding)).Left = x;
		((Padding)(ref _padding)).Top = y;
		((Padding)(ref _padding)).Right = r;
		((Padding)(ref _padding)).Bottom = b;
		((Control)this).OnSizeChanged(EventArgs.Empty);
		return false;
	}

	internal void LoadLayout(bool r = true)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!((Control)this).IsHandleCreated || items == null)
		{
			return;
		}
		if (_tabMenuVisible)
		{
			Rectangle clientRectangle = ((Control)this).ClientRectangle;
			if (clientRectangle.Width > 0 && clientRectangle.Height > 0)
			{
				Rectangle rect = clientRectangle.DeflateRect(((Control)this).Margin);
				style.LoadLayout(this, rect, items);
				if (r)
				{
					((Control)this).Invalidate();
				}
			}
		}
		else
		{
			SetPadding(0, 0, 0, 0);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (items != null && items.Count != 0 && _tabMenuVisible)
		{
			Canvas g = e.Graphics.High();
			style.Paint(this, g, items);
			this.PaintBadge(g);
			((Control)this).OnPaint(e);
		}
	}

	private void PaintBadge(Canvas g, TabPage page, Rectangle rect)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		if (page.Badge == null)
		{
			return;
		}
		Color color = page.BadgeBack ?? Colour.Error.Get("Tabs");
		SolidBrush val = new SolidBrush(Colour.ErrorColor.Get("Tabs"));
		try
		{
			Font val2 = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * page.BadgeSize);
			try
			{
				if (string.IsNullOrEmpty(page.Badge) || page.Badge == "" || page.Badge == " ")
				{
					int num = g.MeasureString("龍Qq", val2).Width / 2;
					RectangleF rect2 = new RectangleF((float)(rect.Right - num) - (float)page.BadgeOffsetX * Config.Dpi, (float)rect.Y + (float)page.BadgeOffsetY * Config.Dpi, num, num);
					SolidBrush val3 = new SolidBrush(color);
					try
					{
						g.FillEllipse((Brush)(object)val3, rect2);
						g.DrawEllipse(val.Color, Config.Dpi, rect2);
						return;
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				Size size = g.MeasureString(page.Badge, val2);
				int num2 = (int)((float)size.Height * 1.2f);
				if (size.Height > size.Width)
				{
					Rectangle rectangle = new Rectangle(rect.Right - num2 - (int)((float)page.BadgeOffsetX * Config.Dpi), rect.Y + (int)((float)page.BadgeOffsetY * Config.Dpi), num2, num2);
					SolidBrush val4 = new SolidBrush(color);
					try
					{
						g.FillEllipse((Brush)(object)val4, rectangle);
						g.DrawEllipse(val.Color, Config.Dpi, rectangle);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
					g.String(page.Badge, val2, (Brush)(object)val, rectangle, s_f);
					return;
				}
				int num3 = size.Width + (num2 - size.Height);
				Rectangle rect3 = new Rectangle(rect.Right - num3 - (int)((float)page.BadgeOffsetX * Config.Dpi), rect.Y + (int)((float)page.BadgeOffsetY * Config.Dpi), num3, num2);
				SolidBrush val5 = new SolidBrush(color);
				try
				{
					GraphicsPath val6 = rect3.RoundPath(rect3.Height);
					try
					{
						g.Fill((Brush)(object)val5, val6);
						g.Draw(val.Color, Config.Dpi, val6);
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
				g.String(page.Badge, val2, (Brush)(object)val, rect3, s_f);
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

	protected override void OnMouseDown(MouseEventArgs e)
	{
		((Control)this).OnMouseDown(e);
		if (items == null || MouseDownPre(e.X, e.Y) || !_tabMenuVisible)
		{
			return;
		}
		int num = 0;
		int x = e.X + scroll_x;
		int y = e.Y + scroll_y;
		foreach (TabPage item in items)
		{
			if (((Control)item).Visible && item.Contains(x, y))
			{
				item.MDown = true;
				((Control)this).Invalidate();
				break;
			}
			num++;
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		if (items == null || !_tabMenuVisible)
		{
			return;
		}
		int num = 0;
		int x = e.X + scroll_x;
		int y = e.Y + scroll_y;
		foreach (TabPage item in items)
		{
			if (item.MDown)
			{
				item.MDown = false;
				if (item.Contains(x, y))
				{
					if (!style.MouseClick(item, num, x, y))
					{
						this.TabClick?.Invoke(this, new TabsItemEventArgs(item, style, e));
						SelectedIndex = num;
					}
				}
				else
				{
					((Control)this).Invalidate();
				}
				break;
			}
			num++;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (items == null)
		{
			return;
		}
		if (MouseMovePre(e.X, e.Y))
		{
			Hover_i = -1;
			SetCursor(val: true);
			return;
		}
		int num = 0;
		int x = e.X + scroll_x;
		int y = e.Y + scroll_y;
		foreach (TabPage item in items)
		{
			if (((Control)item).Visible && item.Contains(x, y))
			{
				SetCursor(val: true);
				Hover_i = num;
				style.MouseMove(x, y);
				return;
			}
			num++;
		}
		style.MouseMove(x, y);
		SetCursor(val: false);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		style.MouseLeave();
		bool flag2 = (Hover_r = false);
		Hover_l = flag2;
		Hover_i = -1;
		SetCursor(val: false);
		((Control)this).OnMouseLeave(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		style.MouseWheel(e.Delta);
		base.OnMouseWheel(e);
	}

	private bool MouseMovePre(int x, int y)
	{
		if (items == null)
		{
			return false;
		}
		switch (typExceed)
		{
		case TabTypExceed.Button:
			if (scroll_show && rect_r.Contains(x, y))
			{
				if (subForm == null)
				{
					List<SelectItem> list = new List<SelectItem>(items.Count);
					foreach (TabPage item in items)
					{
						list.Add(new SelectItem(((Control)item).Text, item));
					}
					subForm = new LayeredFormSelectDown(this, 6, list.ToArray(), SelectedTab, rect_r);
					((Component)(object)subForm).Disposed += delegate
					{
						subForm = null;
					};
					((Control)subForm).MouseEnter += delegate(object a, EventArgs b)
					{
						if (a is LayeredFormSelectDown layeredFormSelectDown3)
						{
							layeredFormSelectDown3.tag1 = false;
						}
					};
					((Control)subForm).MouseLeave += delegate(object a, EventArgs b)
					{
						if (a is LayeredFormSelectDown layeredFormSelectDown2)
						{
							layeredFormSelectDown2.IClose();
						}
					};
					((Control)subForm).Leave += delegate(object a, EventArgs b)
					{
						if (a is LayeredFormSelectDown layeredFormSelectDown)
						{
							layeredFormSelectDown.IClose();
						}
					};
					((Form)subForm).Show((IWin32Window)(object)this);
				}
				return true;
			}
			subForm?.IClose();
			subForm = null;
			break;
		case TabTypExceed.LR:
		case TabTypExceed.LR_Shadow:
			if (scroll_show)
			{
				if (MouseDownLRL(x, y, out var _))
				{
					Hover_l = true;
					return true;
				}
				Hover_l = false;
				if (MouseDownLRR(x, y, out var _))
				{
					Hover_r = true;
					return true;
				}
				Hover_r = false;
			}
			break;
		}
		return false;
	}

	private bool MouseDownPre(int x, int y)
	{
		if (items == null)
		{
			return false;
		}
		if (scroll_show && (typExceed == TabTypExceed.LR || typExceed == TabTypExceed.LR_Shadow))
		{
			if (MouseDownLRL(x, y, out var lr))
			{
				if (lr)
				{
					scroll_y -= 120;
				}
				else
				{
					scroll_x -= 120;
				}
				return true;
			}
			if (MouseDownLRR(x, y, out var lr2))
			{
				if (lr2)
				{
					scroll_y += 120;
				}
				else
				{
					scroll_x += 120;
				}
				return true;
			}
		}
		return false;
	}

	private bool MouseDownLRL(int x, int y, out bool lr)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		TabAlignment val = alignment;
		if ((int)val > 1 && val - 2 <= 1)
		{
			lr = true;
			if (scroll_y > 0 && rect_l.Contains(x, y))
			{
				return true;
			}
			return false;
		}
		lr = false;
		if (scroll_x > 0 && rect_l.Contains(x, y))
		{
			return true;
		}
		return false;
	}

	private bool MouseDownLRR(int x, int y, out bool lr)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		TabAlignment val = alignment;
		if ((int)val > 1 && val - 2 <= 1)
		{
			lr = true;
			if (scroll_max != scroll_y && rect_r.Contains(x, y))
			{
				return true;
			}
			return false;
		}
		lr = false;
		if (scroll_max != scroll_x && rect_r.Contains(x, y))
		{
			return true;
		}
		return false;
	}

	public virtual int SizeExceed(Rectangle rect, Rectangle first, Rectangle last)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		TabTypExceed tabTypExceed = typExceed;
		if (tabTypExceed != 0 && tabTypExceed == TabTypExceed.Button)
		{
			int height = last.Height;
			TabAlignment val = alignment;
			if ((int)val > 1 && val - 2 <= 1)
			{
				rect_r = new Rectangle(last.X, rect.Bottom - height, last.Width, height);
			}
			else
			{
				rect_r = new Rectangle(rect.Right - height, last.Y, height, height);
			}
			return height;
		}
		rect_r = Rectangle.Empty;
		return 0;
	}

	public virtual Rectangle PaintExceedPre(Rectangle rect, int size)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Invalid comparison between Unknown and I4
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Invalid comparison between Unknown and I4
		switch (typExceed)
		{
		case TabTypExceed.Button:
		{
			TabAlignment val = alignment;
			if ((int)val > 1 && val - 2 <= 1)
			{
				if (scroll_max != scroll_y)
				{
					return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - size);
				}
				return rect;
			}
			if (scroll_max != scroll_x)
			{
				return new Rectangle(rect.X, rect.Y, rect.Width - size, rect.Height);
			}
			return rect;
		}
		case TabTypExceed.LR:
		case TabTypExceed.LR_Shadow:
		{
			TabAlignment val = alignment;
			if ((int)val > 1 && val - 2 <= 1)
			{
				int num = (int)((float)size * 0.6f);
				if (scroll_max != scroll_y)
				{
					if (scroll_y > 0)
					{
						return new Rectangle(rect.X, rect.Y + num, rect.Width, rect.Height - num * 2);
					}
					return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - num);
				}
				if (scroll_y > 0)
				{
					return new Rectangle(rect.X, rect.Y + num, rect.Width, rect.Height - num);
				}
				return rect;
			}
			int num2 = (int)((float)size * 0.6f);
			if (scroll_max != scroll_x)
			{
				if (scroll_x > 0)
				{
					return new Rectangle(rect.X + num2, rect.Y, rect.Width - num2 * 2, rect.Height);
				}
				return new Rectangle(rect.X, rect.Y, rect.Width - num2, rect.Height);
			}
			if (scroll_x > 0)
			{
				return new Rectangle(rect.X + num2, rect.Y, rect.Width - num2, rect.Height);
			}
			return rect;
		}
		default:
			return rect;
		}
	}

	public virtual void PaintExceed(Canvas g, Color color, int radius, Rectangle rect, Rectangle first, Rectangle last, bool full)
	{
		switch (typExceed)
		{
		case TabTypExceed.Button:
			PaintExceedButton(g, color, radius, rect, first, last, full);
			break;
		case TabTypExceed.LR:
			PaintExceedLR(g, color, radius, rect, first, last, full);
			break;
		case TabTypExceed.LR_Shadow:
			PaintExceedLR_Shadow(g, color, radius, rect, first, last, full);
			break;
		case TabTypExceed.None:
			break;
		}
	}

	public virtual void PaintExceedButton(Canvas g, Color color, int radius, Rectangle rect, Rectangle first, Rectangle last, bool full)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Expected O, but got Unknown
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Expected O, but got Unknown
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Expected O, but got Unknown
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Expected O, but got Unknown
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		g.ResetClip();
		g.ResetTransform();
		TabAlignment val = alignment;
		if ((int)val > 1 && val - 2 <= 1)
		{
			if (scroll_y <= 0 && scroll_max == scroll_y)
			{
				return;
			}
			int num = (int)((float)_gap * Config.Dpi);
			int num2 = num * 2;
			int height = last.Height;
			int num3 = (int)((float)height * 0.4f);
			Rectangle rectangle = new Rectangle(last.X, rect.Bottom - height, last.Width, height);
			if (scroll_y > 0)
			{
				Rectangle rect2 = new Rectangle(first.X, first.Y, rectangle.Width, num2);
				if (full)
				{
					rect2.Y = 0;
				}
				if (bitblock_l == null || ((Image)bitblock_l).Width != rect2.Width || ((Image)bitblock_l).Height != rect2.Height)
				{
					Bitmap? obj = bitblock_l;
					if (obj != null)
					{
						((Image)obj).Dispose();
					}
					bitblock_l = new Bitmap(rect2.Width, rect2.Height);
					using (Canvas canvas = Graphics.FromImage((Image)(object)bitblock_l).HighLay())
					{
						SolidBrush val2 = new SolidBrush(color);
						try
						{
							GraphicsPath val3 = new Rectangle(0, 0, ((Image)bitblock_l).Width, num).RoundPath(num, TL: false, TR: false, BR: true, BL: true);
							try
							{
								canvas.Fill((Brush)(object)val2, val3);
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
					Helper.Blur(bitblock_l, num);
				}
				g.Image(bitblock_l, rect2, 0.1f);
			}
			if (scroll_max != scroll_y)
			{
				Rectangle rect3 = new Rectangle(rectangle.X, rectangle.Y - num2, rectangle.Width, num2);
				if (bitblock_r == null || ((Image)bitblock_r).Width != rect3.Width || ((Image)bitblock_r).Height != rect3.Height)
				{
					Bitmap? obj2 = bitblock_r;
					if (obj2 != null)
					{
						((Image)obj2).Dispose();
					}
					bitblock_r = new Bitmap(rect3.Width, rect3.Height);
					using (Canvas canvas2 = Graphics.FromImage((Image)(object)bitblock_r).HighLay())
					{
						SolidBrush val4 = new SolidBrush(color);
						try
						{
							GraphicsPath val5 = new Rectangle(0, num, ((Image)bitblock_r).Width, num).RoundPath(num, TL: true, TR: true, BR: false, BL: false);
							try
							{
								canvas2.Fill((Brush)(object)val4, val5);
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
					Helper.Blur(bitblock_r, num);
				}
				g.Image(bitblock_r, rect3, 0.1f);
			}
			SvgExtend.GetImgExtend(rect: new Rectangle(rectangle.X + (rectangle.Width - num3) / 2, rectangle.Y + (rectangle.Height - num3) / 2, num3, num3), g: g, svg: SvgDb.IcoMore, color: color);
		}
		else
		{
			if (scroll_x <= 0 && scroll_max == scroll_x)
			{
				return;
			}
			int num4 = (int)((float)_gap * Config.Dpi);
			int num5 = num4 * 2;
			int height2 = last.Height;
			int num6 = (int)((float)height2 * 0.4f);
			Rectangle rectangle2 = new Rectangle(rect.Right - height2, last.Y, height2, height2);
			if (scroll_x > 0)
			{
				Rectangle rect5 = new Rectangle(first.X, first.Y, num5, height2);
				if (full)
				{
					rect5.X = 0;
				}
				if (bitblock_l == null || ((Image)bitblock_l).Width != rect5.Width || ((Image)bitblock_l).Height != rect5.Height)
				{
					Bitmap? obj3 = bitblock_l;
					if (obj3 != null)
					{
						((Image)obj3).Dispose();
					}
					bitblock_l = new Bitmap(rect5.Width, rect5.Height);
					using (Canvas canvas3 = Graphics.FromImage((Image)(object)bitblock_l).HighLay())
					{
						SolidBrush val6 = new SolidBrush(color);
						try
						{
							GraphicsPath val7 = new Rectangle(0, 0, num4, ((Image)bitblock_l).Height).RoundPath(num4, TL: false, TR: true, BR: true, BL: false);
							try
							{
								canvas3.Fill((Brush)(object)val6, val7);
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
					Helper.Blur(bitblock_l, num4);
				}
				g.Image(bitblock_l, rect5, 0.1f);
			}
			if (scroll_max != scroll_x)
			{
				Rectangle rect6 = new Rectangle(rectangle2.X - num5, rectangle2.Y, num5, height2);
				if (bitblock_r == null || ((Image)bitblock_r).Width != rect6.Width || ((Image)bitblock_r).Height != rect6.Height)
				{
					Bitmap? obj4 = bitblock_r;
					if (obj4 != null)
					{
						((Image)obj4).Dispose();
					}
					bitblock_r = new Bitmap(rect6.Width, rect6.Height);
					using (Canvas canvas4 = Graphics.FromImage((Image)(object)bitblock_r).HighLay())
					{
						SolidBrush val8 = new SolidBrush(color);
						try
						{
							GraphicsPath val9 = new Rectangle(num4, 0, num4, ((Image)bitblock_r).Height).RoundPath(num4, TL: true, TR: false, BR: false, BL: true);
							try
							{
								canvas4.Fill((Brush)(object)val8, val9);
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
					Helper.Blur(bitblock_r, num4);
				}
				g.Image(bitblock_r, rect6, 0.1f);
			}
			SvgExtend.GetImgExtend(rect: new Rectangle(rectangle2.X + (rectangle2.Width - num6) / 2, rectangle2.Y + (rectangle2.Height - num6) / 2, num6, num6), g: g, svg: SvgDb.IcoMore, color: color);
		}
	}

	public virtual void PaintExceedLR(Canvas g, Color color, int radius, Rectangle rect, Rectangle first, Rectangle last, bool full)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Expected O, but got Unknown
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		g.ResetClip();
		g.ResetTransform();
		TabAlignment val = alignment;
		if ((int)val > 1 && val - 2 <= 1)
		{
			if (scroll_y <= 0 && scroll_max == scroll_y)
			{
				return;
			}
			int num = (int)((float)last.Height * 0.6f);
			SolidBrush val2 = new SolidBrush(scrollback ?? Colour.FillSecondary.Get("Tabs"));
			try
			{
				SolidBrush val3 = new SolidBrush(ScrollBackHover ?? Colour.Primary.Get("Tabs"));
				try
				{
					Pen val4 = new Pen(scrollfore.GetValueOrDefault(color), 2f * Config.Dpi);
					try
					{
						Pen val5 = new Pen(ScrollForeHover ?? Colour.PrimaryColor.Get("Tabs"), 2f * Config.Dpi);
						try
						{
							if (scroll_y > 0)
							{
								rect_l = new Rectangle(last.X, rect.Y, last.Width, num);
								GraphicsPath val6 = rect_l.RoundPath(radius);
								try
								{
									if (hover_l)
									{
										g.Fill((Brush)(object)val3, val6);
										g.DrawLines(val5, TAlignMini.Top.TriangleLines(rect_l, 0.5f));
									}
									else
									{
										g.Fill((Brush)(object)val2, val6);
										g.DrawLines(val4, TAlignMini.Top.TriangleLines(rect_l, 0.5f));
									}
								}
								finally
								{
									((IDisposable)val6)?.Dispose();
								}
							}
							if (scroll_max == scroll_y)
							{
								return;
							}
							rect_r = new Rectangle(last.X, rect.Y + rect.Height - num, last.Width, num);
							GraphicsPath val7 = rect_r.RoundPath(radius);
							try
							{
								if (hover_r)
								{
									g.Fill((Brush)(object)val3, val7);
									g.DrawLines(val5, TAlignMini.Bottom.TriangleLines(rect_r, 0.5f));
								}
								else
								{
									g.Fill((Brush)(object)val2, val7);
									g.DrawLines(val4, TAlignMini.Bottom.TriangleLines(rect_r, 0.5f));
								}
								return;
							}
							finally
							{
								((IDisposable)val7)?.Dispose();
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
		if (scroll_x <= 0 && scroll_max == scroll_x)
		{
			return;
		}
		int num2 = (int)((float)last.Height * 0.6f);
		SolidBrush val8 = new SolidBrush(scrollback ?? Colour.FillSecondary.Get("Tabs"));
		try
		{
			SolidBrush val9 = new SolidBrush(ScrollBackHover ?? Colour.Primary.Get("Tabs"));
			try
			{
				Pen val10 = new Pen(scrollfore.GetValueOrDefault(color), 2f * Config.Dpi);
				try
				{
					Pen val11 = new Pen(ScrollForeHover ?? Colour.PrimaryColor.Get("Tabs"), 2f * Config.Dpi);
					try
					{
						if (scroll_x > 0)
						{
							rect_l = new Rectangle(rect.X, last.Y, num2, last.Height);
							GraphicsPath val12 = rect_l.RoundPath(radius);
							try
							{
								if (hover_l)
								{
									g.Fill((Brush)(object)val9, val12);
									g.DrawLines(val11, TAlignMini.Left.TriangleLines(rect_l, 0.5f));
								}
								else
								{
									g.Fill((Brush)(object)val8, val12);
									g.DrawLines(val10, TAlignMini.Left.TriangleLines(rect_l, 0.5f));
								}
							}
							finally
							{
								((IDisposable)val12)?.Dispose();
							}
						}
						if (scroll_max == scroll_x)
						{
							return;
						}
						rect_r = new Rectangle(rect.X + rect.Width - num2, last.Y, num2, last.Height);
						GraphicsPath val13 = rect_r.RoundPath(radius);
						try
						{
							if (hover_r)
							{
								g.Fill((Brush)(object)val9, val13);
								g.DrawLines(val11, TAlignMini.Right.TriangleLines(rect_r, 0.5f));
							}
							else
							{
								g.Fill((Brush)(object)val8, val13);
								g.DrawLines(val10, TAlignMini.Right.TriangleLines(rect_r, 0.5f));
							}
						}
						finally
						{
							((IDisposable)val13)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val11)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val10)?.Dispose();
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

	public virtual void PaintExceedLR_Shadow(Canvas g, Color color, int radius, Rectangle rect, Rectangle first, Rectangle last, bool full)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0512: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Expected O, but got Unknown
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_055c: Expected O, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_063e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0648: Expected O, but got Unknown
		//IL_07fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Expected O, but got Unknown
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Expected O, but got Unknown
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Expected O, but got Unknown
		g.ResetClip();
		g.ResetTransform();
		TabAlignment val = alignment;
		if ((int)val > 1 && val - 2 <= 1)
		{
			if (scroll_y <= 0 && scroll_max == scroll_y)
			{
				return;
			}
			int num = (int)((float)_gap * Config.Dpi);
			int num2 = num * 2;
			int num3 = (int)((float)last.Height * 0.6f);
			SolidBrush val2 = new SolidBrush(scrollback ?? Colour.FillSecondary.Get("Tabs"));
			try
			{
				SolidBrush val3 = new SolidBrush(ScrollBackHover ?? Colour.Primary.Get("Tabs"));
				try
				{
					Pen val4 = new Pen(scrollfore.GetValueOrDefault(color), 2f * Config.Dpi);
					try
					{
						Pen val5 = new Pen(ScrollForeHover ?? Colour.PrimaryColor.Get("Tabs"), 2f * Config.Dpi);
						try
						{
							if (scroll_y > 0)
							{
								rect_l = new Rectangle(last.X, rect.Y, last.Width, num3);
								Rectangle rect2 = new Rectangle(rect_l.X, rect_l.Bottom, rect_l.Width, num2);
								if (bitblock_l == null || ((Image)bitblock_l).Width != rect2.Width || ((Image)bitblock_l).Height != rect2.Height)
								{
									Bitmap? obj = bitblock_l;
									if (obj != null)
									{
										((Image)obj).Dispose();
									}
									bitblock_l = new Bitmap(rect2.Width, rect2.Height);
									using (Canvas canvas = Graphics.FromImage((Image)(object)bitblock_l).HighLay())
									{
										GraphicsPath val6 = new Rectangle(0, 0, ((Image)bitblock_l).Width, num).RoundPath(num, TL: false, TR: false, BR: true, BL: true);
										try
										{
											canvas.Fill((Brush)(object)val2, val6);
										}
										finally
										{
											((IDisposable)val6)?.Dispose();
										}
									}
									Helper.Blur(bitblock_l, num);
								}
								g.Image(bitblock_l, rect2, 0.1f);
								GraphicsPath val7 = rect_l.RoundPath(radius, TL: true, TR: true, BR: false, BL: false);
								try
								{
									if (hover_l)
									{
										g.Fill((Brush)(object)val3, val7);
										g.DrawLines(val5, TAlignMini.Top.TriangleLines(rect_l, 0.5f));
									}
									else
									{
										g.Fill((Brush)(object)val2, val7);
										g.DrawLines(val4, TAlignMini.Top.TriangleLines(rect_l, 0.5f));
									}
								}
								finally
								{
									((IDisposable)val7)?.Dispose();
								}
							}
							if (scroll_max == scroll_y)
							{
								return;
							}
							rect_r = new Rectangle(last.X, rect.Y + rect.Height - num3, last.Width, num3);
							Rectangle rect3 = new Rectangle(rect_r.X, rect_r.Y - num2, rect_r.Width, num2);
							if (bitblock_r == null || ((Image)bitblock_r).Width != rect3.Width || ((Image)bitblock_r).Height != rect3.Height)
							{
								Bitmap? obj2 = bitblock_r;
								if (obj2 != null)
								{
									((Image)obj2).Dispose();
								}
								bitblock_r = new Bitmap(rect3.Width, rect3.Height);
								using (Canvas canvas2 = Graphics.FromImage((Image)(object)bitblock_r).HighLay())
								{
									GraphicsPath val8 = new Rectangle(0, num, ((Image)bitblock_r).Width, num).RoundPath(num, TL: true, TR: true, BR: false, BL: false);
									try
									{
										canvas2.Fill((Brush)(object)val2, val8);
									}
									finally
									{
										((IDisposable)val8)?.Dispose();
									}
								}
								Helper.Blur(bitblock_r, num);
							}
							g.Image(bitblock_r, rect3, 0.1f);
							GraphicsPath val9 = rect_r.RoundPath(radius, TL: false, TR: false, BR: true, BL: true);
							try
							{
								if (hover_r)
								{
									g.Fill((Brush)(object)val3, val9);
									g.DrawLines(val5, TAlignMini.Bottom.TriangleLines(rect_r, 0.5f));
								}
								else
								{
									g.Fill((Brush)(object)val2, val9);
									g.DrawLines(val4, TAlignMini.Bottom.TriangleLines(rect_r, 0.5f));
								}
								return;
							}
							finally
							{
								((IDisposable)val9)?.Dispose();
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
		if (scroll_x <= 0 && scroll_max == scroll_x)
		{
			return;
		}
		int num4 = (int)((float)_gap * Config.Dpi);
		int num5 = num4 * 2;
		int num6 = (int)((float)last.Height * 0.6f);
		SolidBrush val10 = new SolidBrush(scrollback ?? Colour.FillSecondary.Get("Tabs"));
		try
		{
			SolidBrush val11 = new SolidBrush(ScrollBackHover ?? Colour.Primary.Get("Tabs"));
			try
			{
				Pen val12 = new Pen(scrollfore.GetValueOrDefault(color), 2f * Config.Dpi);
				try
				{
					Pen val13 = new Pen(ScrollForeHover ?? Colour.PrimaryColor.Get("Tabs"), 2f * Config.Dpi);
					try
					{
						if (scroll_x > 0)
						{
							rect_l = new Rectangle(rect.X, last.Y, num6, last.Height);
							Rectangle rect4 = new Rectangle(rect_l.Right, rect_l.Y, num5, rect_l.Height);
							if (bitblock_l == null || ((Image)bitblock_l).Width != rect4.Width || ((Image)bitblock_l).Height != rect4.Height)
							{
								Bitmap? obj3 = bitblock_l;
								if (obj3 != null)
								{
									((Image)obj3).Dispose();
								}
								bitblock_l = new Bitmap(rect4.Width, rect4.Height);
								using (Canvas canvas3 = Graphics.FromImage((Image)(object)bitblock_l).HighLay())
								{
									GraphicsPath val14 = new Rectangle(0, 0, num4, ((Image)bitblock_l).Height).RoundPath(num4, TL: false, TR: true, BR: true, BL: false);
									try
									{
										canvas3.Fill((Brush)(object)val10, val14);
									}
									finally
									{
										((IDisposable)val14)?.Dispose();
									}
								}
								Helper.Blur(bitblock_l, num4);
							}
							g.Image(bitblock_l, rect4, 0.1f);
							GraphicsPath val15 = rect_l.RoundPath(radius, TL: true, TR: false, BR: false, BL: true);
							try
							{
								if (hover_l)
								{
									g.Fill((Brush)(object)val11, val15);
									g.DrawLines(val13, TAlignMini.Left.TriangleLines(rect_l, 0.5f));
								}
								else
								{
									g.Fill((Brush)(object)val10, val15);
									g.DrawLines(val12, TAlignMini.Left.TriangleLines(rect_l, 0.5f));
								}
							}
							finally
							{
								((IDisposable)val15)?.Dispose();
							}
						}
						if (scroll_max == scroll_x)
						{
							return;
						}
						rect_r = new Rectangle(rect.X + rect.Width - num6, last.Y, num6, last.Height);
						Rectangle rect5 = new Rectangle(rect_r.X - num5, rect_r.Y, num5, rect_r.Height);
						if (bitblock_r == null || ((Image)bitblock_r).Width != rect5.Width || ((Image)bitblock_r).Height != rect5.Height)
						{
							Bitmap? obj4 = bitblock_r;
							if (obj4 != null)
							{
								((Image)obj4).Dispose();
							}
							bitblock_r = new Bitmap(rect5.Width, rect5.Height);
							using (Canvas canvas4 = Graphics.FromImage((Image)(object)bitblock_r).HighLay())
							{
								GraphicsPath val16 = new Rectangle(num4, 0, num4, ((Image)bitblock_r).Height).RoundPath(num4, TL: true, TR: false, BR: false, BL: true);
								try
								{
									canvas4.Fill((Brush)(object)val10, val16);
								}
								finally
								{
									((IDisposable)val16)?.Dispose();
								}
							}
							Helper.Blur(bitblock_r, num4);
						}
						g.Image(bitblock_r, rect5, 0.1f);
						GraphicsPath val17 = rect_r.RoundPath(radius, TL: false, TR: true, BR: true, BL: false);
						try
						{
							if (hover_r)
							{
								g.Fill((Brush)(object)val11, val17);
								g.DrawLines(val13, TAlignMini.Right.TriangleLines(rect_r, 0.5f));
							}
							else
							{
								g.Fill((Brush)(object)val10, val17);
								g.DrawLines(val12, TAlignMini.Right.TriangleLines(rect_r, 0.5f));
							}
						}
						finally
						{
							((IDisposable)val17)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val13)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val12)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val11)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val10)?.Dispose();
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

[Description("Select 多选器")]
[ToolboxItem(true)]
[DefaultEvent("SelectedValueChanged")]
public class SelectMultiple : Input, SubLayeredForm
{
	private bool _list;

	private bool canDelete = true;

	private BaseCollection? items;

	private string filtertext = "";

	private bool TerminateExpand;

	private object[] selectedValue = new object[0];

	private bool showicon = true;

	private Rectangle[] rect_lefts = new Rectangle[0];

	private Rectangle[] rect_left_txts = new Rectangle[0];

	private Rectangle[] rect_left_dels = new Rectangle[0];

	private SelectItem?[] style_left = new SelectItem[0];

	private int select_del = -1;

	private ISelectMultiple? subForm;

	private ITask? ThreadExpand;

	private float ArrowProg = -1f;

	private bool expand;

	internal int select_x;

	private bool expandDrop;

	protected override bool BanInput => _list;

	[Description("是否列表样式")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool List
	{
		get
		{
			return _list;
		}
		set
		{
			if (_list != value)
			{
				_list = value;
				CaretInfo.ReadShow = value;
				if (value)
				{
					CaretInfo.Show = false;
				}
			}
		}
	}

	[Description("复选框模式")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool CheckMode { get; set; }

	[Description("自动高度")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool AutoHeight { get; set; }

	[Description("是否可以删除")]
	[Category("交互")]
	[DefaultValue(true)]
	public bool CanDelete
	{
		get
		{
			return canDelete;
		}
		set
		{
			if (canDelete != value)
			{
				canDelete = value;
				CalculateRect();
				Invalidate();
			}
		}
	}

	[Description("菜单弹出位置")]
	[Category("行为")]
	[DefaultValue(TAlignFrom.BL)]
	public TAlignFrom Placement { get; set; } = TAlignFrom.BL;


	[Description("是否列表自动宽度")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool ListAutoWidth { get; set; }

	[Description("列表最多显示条数")]
	[Category("行为")]
	[DefaultValue(4)]
	public int MaxCount { get; set; } = 4;


	[Description("最大选中数量")]
	[Category("行为")]
	[DefaultValue(0)]
	public int MaxChoiceCount { get; set; }

	[Description("下拉箭头是否显示")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool DropDownArrow { get; set; }

	[Description("下拉边距")]
	[Category("外观")]
	[DefaultValue(typeof(Size), "12, 5")]
	public Size DropDownPadding { get; set; } = new Size(12, 5);


	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor", typeof(UITypeEditor))]
	[Description("集合")]
	[Category("数据")]
	public BaseCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new BaseCollection();
			}
			return items;
		}
		set
		{
			items = value;
		}
	}

	protected override bool HasValue => selectedValue.Length != 0;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object[] SelectedValue
	{
		get
		{
			return selectedValue;
		}
		set
		{
			if (selectedValue != value)
			{
				selectedValue = value;
				if (value.Length == 0 || items == null || items.Count == 0)
				{
					ClearSelect();
					this.SelectedValueChanged?.Invoke(this, new ObjectsEventArgs(selectedValue));
				}
				else
				{
					CalculateRect();
					Invalidate();
					this.SelectedValueChanged?.Invoke(this, new ObjectsEventArgs(selectedValue));
				}
			}
		}
	}

	protected override bool ShowPlaceholder => selectedValue.Length == 0;

	[Description("是否显示图标")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool ShowIcon
	{
		get
		{
			return showicon;
		}
		set
		{
			if (showicon != value)
			{
				showicon = value;
				CalculateRect();
				Invalidate();
			}
		}
	}

	public override bool HasSuffix => showicon;

	private bool Expand
	{
		get
		{
			return expand;
		}
		set
		{
			if (expand == value)
			{
				return;
			}
			expand = value;
			if (Config.Animation)
			{
				ThreadExpand?.Dispose();
				int t = Animation.TotalFrames(10, 100);
				if (value)
				{
					ThreadExpand = new ITask(delegate(int i)
					{
						ArrowProg = Animation.Animate(i, t, 2f, AnimationType.Ball) - 1f;
						Invalidate();
						return true;
					}, 10, t, delegate
					{
						ArrowProg = 1f;
						Invalidate();
					});
				}
				else
				{
					ThreadExpand = new ITask(delegate(int i)
					{
						ArrowProg = 0f - (Animation.Animate(i, t, 2f, AnimationType.Ball) - 1f);
						Invalidate();
						return true;
					}, 10, t, delegate
					{
						ArrowProg = -1f;
						Invalidate();
					});
				}
			}
			else
			{
				ArrowProg = (value ? 1f : (-1f));
			}
		}
	}

	private bool ExpandDrop
	{
		get
		{
			return expandDrop;
		}
		set
		{
			if (expandDrop == value)
			{
				return;
			}
			expandDrop = value;
			if (value)
			{
				if (base.ReadOnly || items == null || items.Count == 0)
				{
					subForm?.IClose();
					expandDrop = false;
				}
				else
				{
					if (subForm != null)
					{
						return;
					}
					List<object> list = new List<object>(items.Count);
					foreach (object item in items)
					{
						list.Add(item);
					}
					ShowLayeredForm(list);
				}
			}
			else
			{
				subForm?.IClose();
				filtertext = "";
			}
		}
	}

	[Description("SelectedValue 属性值更改时发生")]
	[Category("行为")]
	public event ObjectsEventHandler? SelectedValueChanged;

	public void SelectAllItems()
	{
		if (items == null)
		{
			return;
		}
		List<object> list = new List<object>(items.Count);
		foreach (object item in items)
		{
			if (!(item is DividerSelectItem))
			{
				list.Add(item);
			}
		}
		selectedValue = list.ToArray();
		CalculateRect();
		SetCaretPostion();
		subForm?.SetValues(list);
	}

	protected override void OnTextChanged(EventArgs e)
	{
		((Control)this).OnTextChanged(e);
		if (!base.HasFocus)
		{
			return;
		}
		if (TerminateExpand)
		{
			TerminateExpand = false;
			return;
		}
		filtertext = ((Control)this).Text;
		ExpandDrop = true;
		if (expandDrop)
		{
			subForm?.TextChange(((Control)this).Text);
		}
	}

	public void ClearSelect()
	{
		((Control)this).Text = "";
		selectedValue = new object[0];
		CalculateRect();
		SetCaretPostion();
		Invalidate();
		subForm?.ClearValues();
	}

	protected override void IBackSpaceKey()
	{
		if (selectedValue.Length != 0)
		{
			List<object> list = new List<object>(selectedValue.Length);
			list.AddRange(selectedValue);
			list.RemoveAt(list.Count - 1);
			SelectedValue = list.ToArray();
			subForm?.SetValues(selectedValue);
		}
	}

	protected override void PaintRIcon(Canvas g, Rectangle rect_r)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (showicon)
		{
			Pen val = new Pen(Colour.TextQuaternary.Get("Select"), 2f);
			try
			{
				LineCap startCap = (LineCap)2;
				val.EndCap = (LineCap)2;
				val.StartCap = startCap;
				g.DrawLines(val, rect_r.TriangleLines(ArrowProg));
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	protected override bool HasLeft()
	{
		return selectedValue.Length != 0;
	}

	protected override int[] UseLeft(Rectangle rect_read, bool delgap)
	{
		if (selectedValue.Length != 0)
		{
			Dictionary<object, SelectItem> style_dir = new Dictionary<object, SelectItem>(selectedValue.Length);
			List<object> enable_dir = new List<object>(selectedValue.Length);
			if (items != null && items.Count > 0)
			{
				foreach (object item7 in items)
				{
					if (item7 is SelectItem selectItem)
					{
						style_dir.Add(selectItem.Tag, selectItem);
						if (!selectItem.Enable)
						{
							enable_dir.Add(selectItem.Tag);
						}
					}
				}
			}
			return Helper.GDI(delegate(Canvas g)
			{
				List<SelectItem> list = new List<SelectItem>(selectedValue.Length);
				List<Rectangle> list2 = new List<Rectangle>(selectedValue.Length);
				List<Rectangle> list3 = new List<Rectangle>(selectedValue.Length);
				List<Rectangle> list4 = new List<Rectangle>(selectedValue.Length);
				int num = (int)(2f * Config.Dpi);
				int num2 = num * 2;
				int num3 = num2 * 2;
				int num4 = g.MeasureString("龍Qq", ((Control)this).Font).Height + num2;
				int num5 = (int)((double)num4 * 0.4);
				if (!AutoHeight && rect_read.Height <= num4 * 2)
				{
					int num6 = (rect_read.Height - num4) / 2;
					int num7 = ((!delgap) ? num6 : 0);
					for (int i = 0; i < selectedValue.Length; i++)
					{
						object obj = selectedValue[i];
						SelectItem item = null;
						string text;
						if (style_dir.TryGetValue(obj, out SelectItem value))
						{
							item = value;
							text = value.Text;
						}
						else
						{
							text = obj.ToString();
						}
						int num8 = g.MeasureString(text, ((Control)this).Font).Width + num3;
						int num9 = g.MeasureString("+" + (selectedValue.Length - i), ((Control)this).Font).Width + num3;
						if (num7 + num8 + num4 + num + (num9 + num) > rect_read.Width)
						{
							list3.Add(new Rectangle(rect_read.X + num7, rect_read.Y + num6, num9, num4));
							style_left = list.ToArray();
							rect_left_txts = list3.ToArray();
							rect_left_dels = list4.ToArray();
							rect_lefts = list2.ToArray();
							if (list3.Count != 1)
							{
								return new int[2]
								{
									num7 + num9 + num,
									0
								};
							}
							return new int[2]
							{
								num9 + num,
								0
							};
						}
						list.Add(item);
						if (enable_dir.Contains(obj) || !canDelete)
						{
							Rectangle item2 = new Rectangle(rect_read.X + num7, rect_read.Y + num6, num8, num4);
							list3.Add(item2);
							list4.Add(new Rectangle(-10, -10, 0, 0));
							list2.Add(item2);
							num7 += num8 + num;
						}
						else
						{
							Rectangle item3 = new Rectangle(rect_read.X + num7, rect_read.Y + num6, num8, num4);
							list3.Add(item3);
							int num10 = (num4 - num5) / 2;
							list4.Add(new Rectangle(item3.Right + num10 / 2, item3.Y + num10, num5, num5));
							item3.Width += num4;
							list2.Add(item3);
							num7 += num8 + num4 + num;
						}
					}
					style_left = list.ToArray();
					rect_left_txts = list3.ToArray();
					rect_left_dels = list4.ToArray();
					rect_lefts = list2.ToArray();
					return new int[2]
					{
						num7 - ((!delgap) ? num : 0),
						0
					};
				}
				int num11 = num;
				int num12 = ((!delgap) ? num11 : 0);
				int num13 = 0;
				for (int j = 0; j < selectedValue.Length; j++)
				{
					object obj2 = selectedValue[j];
					SelectItem item4 = null;
					string text2;
					if (style_dir.TryGetValue(obj2, out SelectItem value2))
					{
						item4 = value2;
						text2 = value2.Text;
					}
					else
					{
						text2 = obj2.ToString();
					}
					int num14 = g.MeasureString(text2, ((Control)this).Font).Width + num3;
					int num15 = g.MeasureString("+" + (selectedValue.Length - j), ((Control)this).Font).Width + num3;
					if (num12 + num14 + num4 + num + (num15 + num) > rect_read.Width)
					{
						if (AutoHeight)
						{
							num13 += num4 + num;
							num12 = ((!delgap) ? num11 : 0);
						}
						else
						{
							if (num13 + num4 + num + (num4 + num) > rect_read.Height)
							{
								list3.Add(new Rectangle(rect_read.X + num12, rect_read.Y + num11 + num13, num15, num4));
								style_left = list.ToArray();
								rect_left_txts = list3.ToArray();
								rect_left_dels = list4.ToArray();
								rect_lefts = list2.ToArray();
								if (list3.Count != 1)
								{
									return new int[2]
									{
										num12 + num15 + num,
										num13
									};
								}
								return new int[2]
								{
									num15 + num,
									num13
								};
							}
							num13 += num4 + num;
							num12 = ((!delgap) ? num11 : 0);
						}
					}
					list.Add(item4);
					if (enable_dir.Contains(obj2) || !canDelete)
					{
						Rectangle item5 = new Rectangle(rect_read.X + num12, rect_read.Y + num11 + num13, num14, num4);
						list3.Add(item5);
						list4.Add(new Rectangle(-10, -10, 0, 0));
						list2.Add(item5);
						num12 += num14 + num;
					}
					else
					{
						Rectangle item6 = new Rectangle(rect_read.X + num12, rect_read.Y + num11 + num13, num14, num4);
						list3.Add(item6);
						int num16 = (num4 - num5) / 2;
						list4.Add(new Rectangle(item6.Right + num16 / 2, item6.Y + num16, num5, num5));
						item6.Width += num4;
						list2.Add(item6);
						num12 += num14 + num4 + num;
					}
				}
				style_left = list.ToArray();
				rect_left_txts = list3.ToArray();
				rect_left_dels = list4.ToArray();
				rect_lefts = list2.ToArray();
				return new int[2]
				{
					num12 - ((!delgap) ? num : 0),
					num13
				};
			});
		}
		return new int[2];
	}

	protected override void UseLeftAutoHeight(int height, int gap, int y)
	{
		if (AutoHeight)
		{
			if (y > 0)
			{
				((Control)this).Height = height + gap + y + (int)(2f * Config.Dpi) * 3;
			}
			else
			{
				((Control)this).Height = height + gap + y;
			}
		}
	}

	protected override void PaintOtherBor(Canvas g, RectangleF rect_read, float radius, Color back, Color borderColor, Color borderActive)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		if (selectedValue.Length == 0 || style_left.Length != rect_lefts.Length)
		{
			return;
		}
		SolidBrush val = new SolidBrush(Colour.TagDefaultColor.Get("Select"));
		try
		{
			if (rect_lefts.Length != 0)
			{
				for (int i = 0; i < rect_lefts.Length; i++)
				{
					object obj = selectedValue[i];
					SelectItem selectItem = style_left[i];
					GraphicsPath val2 = rect_lefts[i].RoundPath(radius);
					try
					{
						if (selectItem == null)
						{
							g.Fill(Colour.TagDefaultBg.Get("Select"), val2);
							Rectangle rect = rect_left_dels[i];
							if (rect.Width > 0 && rect.Height > 0)
							{
								g.PaintIconClose(rect, Colour.TagDefaultColor.Get("Select"));
							}
							g.String(obj.ToString(), ((Control)this).Font, (Brush)(object)val, rect_left_txts[i], sf_center);
							continue;
						}
						Brush val3 = selectItem.TagBackExtend.BrushEx(rect_lefts[i], selectItem.TagBack ?? Colour.TagDefaultBg.Get("Select"));
						try
						{
							g.Fill(val3, val2);
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
						if (selectItem.TagFore.HasValue)
						{
							Rectangle rect2 = rect_left_dels[i];
							if (rect2.Width > 0 && rect2.Height > 0)
							{
								g.PaintIconClose(rect2, selectItem.TagFore.Value);
							}
							SolidBrush val4 = new SolidBrush(selectItem.TagFore.Value);
							try
							{
								g.String(selectItem.Text, ((Control)this).Font, (Brush)(object)val4, rect_left_txts[i], sf_center);
							}
							finally
							{
								((IDisposable)val4)?.Dispose();
							}
						}
						else
						{
							Rectangle rect3 = rect_left_dels[i];
							if (rect3.Width > 0 && rect3.Height > 0)
							{
								g.PaintIconClose(rect3, Colour.TagDefaultColor.Get("Select"));
							}
							g.String(selectItem.Text, ((Control)this).Font, (Brush)(object)val, rect_left_txts[i], sf_center);
						}
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
			}
			if (rect_lefts.Length != selectedValue.Length)
			{
				g.String("+" + (selectedValue.Length - rect_lefts.Length), ((Control)this).Font, (Brush)(object)val, rect_left_txts[rect_left_txts.Length - 1], sf_center);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override bool IMouseDown(Point e)
	{
		select_del = -1;
		if (selectedValue.Length != 0 && rect_left_dels.Length != 0)
		{
			int num = ((selectedValue.Length > rect_left_dels.Length) ? rect_left_dels.Length : selectedValue.Length);
			for (int i = 0; i < num; i++)
			{
				if (rect_left_dels[i].Contains(e))
				{
					select_del = i;
					return true;
				}
			}
		}
		return false;
	}

	protected override bool IMouseMove(Point e)
	{
		if (selectedValue.Length != 0 && rect_left_dels.Length != 0)
		{
			int num = ((selectedValue.Length > rect_left_dels.Length) ? rect_left_dels.Length : selectedValue.Length);
			for (int i = 0; i < num; i++)
			{
				if (rect_left_dels[i].Contains(e))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override bool IMouseUp(Point e)
	{
		if (select_del > -1)
		{
			if (rect_left_dels[select_del].Contains(e))
			{
				List<object> list = new List<object>(selectedValue.Length);
				list.AddRange(selectedValue);
				list.RemoveAt(select_del);
				SelectedValue = list.ToArray();
				if (subForm == null)
				{
					return true;
				}
				subForm.SetValues(selectedValue);
			}
			select_del = -1;
			return true;
		}
		select_del = -1;
		return false;
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	private void ShowLayeredForm(IList<object> list)
	{
		IList<object> list2 = list;
		if (((Control)this).InvokeRequired)
		{
			((Control)this).BeginInvoke((Delegate)(Action)delegate
			{
				ShowLayeredForm(list2);
			});
			return;
		}
		Expand = true;
		if (CheckMode)
		{
			subForm = new LayeredFormSelectMultipleCheck(this, ReadRectangle, list2, filtertext);
		}
		else
		{
			subForm = new LayeredFormSelectMultiple(this, ReadRectangle, list2, filtertext);
		}
		((Component)(object)subForm).Disposed += delegate
		{
			select_x = 0;
			subForm = null;
			Expand = false;
			ExpandDrop = false;
		};
		((Form)subForm).Show((IWin32Window)(object)this);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		bool result = base.ProcessCmdKey(ref msg, keyData);
		if ((int)keyData == 13 || (int)keyData == 40)
		{
			ExpandDrop = true;
			return true;
		}
		return result;
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		ExpandDrop = false;
	}

	protected override void OnClearValue()
	{
		if (selectedValue.Length != 0)
		{
			TerminateExpand = true;
			SelectedValue = new object[0];
		}
		base.OnClearValue();
	}

	protected override void OnClickContent()
	{
		ExpandDrop = !expandDrop;
	}
}

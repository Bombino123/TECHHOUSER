using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

[Description("Select 选择器")]
[ToolboxItem(true)]
[DefaultEvent("SelectedIndexChanged")]
public class Select : Input, SubLayeredForm
{
	public delegate IList<object>? FilterEventHandler(object sender, string value);

	private bool _list;

	private BaseCollection? items;

	private int selectedIndexX = -1;

	private int selectedIndex = -1;

	private object? selectedValue;

	private string filtertext = "";

	private bool TerminateExpand;

	private bool showicon = true;

	private LayeredFormSelectDown? subForm;

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


	[Description("下拉圆角")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? DropDownRadius { get; set; }

	[Description("下拉箭头是否显示")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool DropDownArrow { get; set; }

	[Description("下拉边距")]
	[Category("外观")]
	[DefaultValue(typeof(Size), "12, 5")]
	public Size DropDownPadding { get; set; } = new Size(12, 5);


	[Description("点击到最里层（无节点才能点击）")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool ClickEnd { get; set; }

	[Description("点击切换下拉")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool ClickSwitchDropdown { get; set; } = true;


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

	[Description("选中序号")]
	[Category("数据")]
	[DefaultValue(-1)]
	public int SelectedIndex
	{
		get
		{
			return selectedIndex;
		}
		set
		{
			if (selectedIndex == value)
			{
				return;
			}
			TerminateExpand = true;
			if (items == null || items.Count == 0 || value == -1)
			{
				ChangeValueNULL();
			}
			else
			{
				object obj = items[value];
				if (obj == null)
				{
					return;
				}
				ChangeValue(value, obj);
			}
			if (_list)
			{
				Invalidate();
			}
			OnPropertyChanged("SelectedIndex");
		}
	}

	[Browsable(false)]
	[Description("选中值")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? SelectedValue
	{
		get
		{
			return selectedValue;
		}
		set
		{
			if (selectedValue != value)
			{
				TerminateExpand = true;
				if (value == null || items == null || items.Count == 0)
				{
					ChangeValueNULL();
				}
				else
				{
					SetChangeValue(items, value);
				}
				if (_list)
				{
					Invalidate();
				}
				OnPropertyChanged("SelectedValue");
			}
		}
	}

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

	[Description("SelectedIndex 属性值更改时发生")]
	[Category("行为")]
	public event IntEventHandler? SelectedIndexChanged;

	[Description("多层树结构更改时发生")]
	[Category("行为")]
	public event IntXYEventHandler? SelectedIndexsChanged;

	[Description("SelectedValue 属性值更改时发生")]
	[Category("行为")]
	public event ObjectNEventHandler? SelectedValueChanged;

	[Description("控制筛选 Text更改时发生")]
	[Category("行为")]
	public event FilterEventHandler? FilterChanged;

	private void ChangeValueNULL()
	{
		((Control)this).Text = "";
		selectedValue = null;
		selectedIndex = -1;
		this.SelectedValueChanged?.Invoke(this, new ObjectNEventArgs(selectedValue));
		this.SelectedIndexChanged?.Invoke(this, new IntEventArgs(selectedIndex));
	}

	private void ChangeValue(int value, object? obj)
	{
		selectedIndex = value;
		if (obj is SelectItem selectItem)
		{
			selectedValue = selectItem.Tag;
			((Control)this).Text = selectItem.Text;
		}
		else
		{
			selectedValue = obj;
			if (obj == null)
			{
				((Control)this).Text = "";
			}
			else
			{
				((Control)this).Text = obj.ToString() ?? "";
			}
		}
		this.SelectedValueChanged?.Invoke(this, new ObjectNEventArgs(selectedValue));
		this.SelectedIndexChanged?.Invoke(this, new IntEventArgs(selectedIndex));
	}

	private void SetChangeValue(BaseCollection items, object val)
	{
		for (int i = 0; i < items.Count; i++)
		{
			object obj = items[i];
			if (val.Equals(obj))
			{
				ChangeValue(i, obj);
				return;
			}
			if (obj is SelectItem selectItem && selectItem.Tag.Equals(val))
			{
				ChangeValue(i, obj);
				return;
			}
			if (obj is SelectItem { Sub: not null } selectItem2 && selectItem2.Sub.Count > 0)
			{
				foreach (object item in selectItem2.Sub)
				{
					if (val.Equals(item))
					{
						ChangeValue(i, item);
						return;
					}
					if (item is SelectItem selectItem3 && selectItem3.Tag.Equals(val))
					{
						ChangeValue(i, item);
						return;
					}
				}
			}
			else
			{
				if (!(obj is GroupSelectItem { Sub: not null } groupSelectItem) || groupSelectItem.Sub.Count <= 0)
				{
					continue;
				}
				foreach (object item2 in groupSelectItem.Sub)
				{
					if (val.Equals(item2))
					{
						ChangeValue(i, item2);
						return;
					}
					if (item2 is SelectItem selectItem4 && selectItem4.Tag.Equals(val))
					{
						ChangeValue(i, item2);
						return;
					}
				}
			}
		}
		ChangeValue(items.IndexOf(val), val);
	}

	private void ChangeValue(int x, int y, object obj)
	{
		selectedIndexX = x;
		selectedIndex = y;
		if (obj is SelectItem selectItem)
		{
			selectedValue = selectItem.Tag;
			((Control)this).Text = selectItem.Text;
		}
		else
		{
			selectedValue = obj;
			((Control)this).Text = obj.ToString() ?? "";
		}
		this.SelectedValueChanged?.Invoke(this, new ObjectNEventArgs(selectedValue));
		this.SelectedIndexsChanged?.Invoke(this, new IntXYEventArgs(selectedIndexX, selectedIndex));
	}

	internal void DropDownChange(int i)
	{
		selectedIndexX = 0;
		if (items == null || items.Count == 0)
		{
			ChangeValueNULL();
		}
		else
		{
			ChangeValue(i, items[i]);
		}
		ExpandDrop = false;
		select_x = 0;
		subForm = null;
	}

	internal bool DropDownChange()
	{
		return this.SelectedIndexsChanged == null;
	}

	internal void DropDownChange(int x, int y, object value)
	{
		ChangeValue(x, y, value);
		ExpandDrop = false;
		select_x = 0;
		subForm = null;
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
		if (this.FilterChanged == null)
		{
			ExpandDrop = true;
			if (expandDrop)
			{
				subForm?.TextChange(((Control)this).Text);
			}
			return;
		}
		string temp = filtertext;
		ITask.Run(delegate
		{
			IList<object> list = this.FilterChanged(this, temp);
			if (filtertext == temp)
			{
				Items.Clear();
				if (list == null || list.Count == 0)
				{
					subForm?.IClose();
				}
				else
				{
					Items.AddRange(list);
					if (subForm == null)
					{
						ShowLayeredForm(list);
					}
					else
					{
						subForm.TextChange(((Control)this).Text, list);
					}
				}
			}
		});
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
		subForm = new LayeredFormSelectDown(this, list2, filtertext);
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
		if (selectedIndex > -1 || selectedValue != null || !isempty)
		{
			TerminateExpand = true;
			ChangeValueNULL();
			Invalidate();
		}
	}

	protected override void OnClickContent()
	{
		if (_list || ClickSwitchDropdown)
		{
			ExpandDrop = !expandDrop;
		}
		else if (base.HasFocus)
		{
			if (!expandDrop)
			{
				ExpandDrop = !expandDrop;
			}
		}
		else
		{
			((Control)this).Focus();
		}
	}
}

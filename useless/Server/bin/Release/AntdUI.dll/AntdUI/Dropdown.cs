using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace AntdUI;

[Description("Dropdown 下拉菜单")]
[ToolboxItem(true)]
[DefaultEvent("SelectedValueChanged")]
public class Dropdown : Button, SubLayeredForm
{
	private BaseCollection? items;

	private LayeredFormSelectDown? subForm;

	private ITask? ThreadExpand;

	private bool expand;

	internal int select_x;

	[Description("列表自动宽度")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool ListAutoWidth { get; set; } = true;


	[Description("触发下拉的行为")]
	[Category("行为")]
	[DefaultValue(Trigger.Click)]
	public Trigger Trigger { get; set; }

	[Description("菜单弹出位置")]
	[Category("行为")]
	[DefaultValue(TAlignFrom.BL)]
	public TAlignFrom Placement { get; set; } = TAlignFrom.BL;


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

	[Browsable(false)]
	[Description("选中值")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? SelectedValue { get; set; }

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
			if (base.ShowArrow && Config.Animation)
			{
				ThreadExpand?.Dispose();
				int t = Animation.TotalFrames(10, 100);
				if (value)
				{
					ThreadExpand = new ITask(delegate(int i)
					{
						base.ArrowProg = Animation.Animate(i, t, 2f, AnimationType.Ball) - 1f;
						((Control)this).Invalidate();
						return true;
					}, 10, t, delegate
					{
						base.ArrowProg = 1f;
						((Control)this).Invalidate();
					});
				}
				else
				{
					ThreadExpand = new ITask(delegate(int i)
					{
						base.ArrowProg = 0f - (Animation.Animate(i, t, 2f, AnimationType.Ball) - 1f);
						((Control)this).Invalidate();
						return true;
					}, 10, t, delegate
					{
						base.ArrowProg = -1f;
						((Control)this).Invalidate();
					});
				}
			}
			else
			{
				base.ArrowProg = (value ? 1f : (-1f));
			}
		}
	}

	[Description("SelectedValue 属性值更改时发生")]
	[Category("行为")]
	public event ObjectNEventHandler? SelectedValueChanged;

	internal void DropDownChange(object value)
	{
		this.SelectedValueChanged?.Invoke(this, new ObjectNEventArgs(value));
		select_x = 0;
		subForm = null;
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		if (Trigger == Trigger.Click)
		{
			ClickDown();
		}
		else
		{
			((Control)this).OnMouseClick(e);
		}
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		if (Trigger == Trigger.Hover && subForm == null)
		{
			ClickDown();
		}
		base.OnMouseEnter(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		subForm?.IClose();
		base.OnLostFocus(e);
	}

	private void ClickDown()
	{
		if (items != null && items.Count > 0)
		{
			if (subForm == null)
			{
				List<object> list = new List<object>(items.Count);
				foreach (object item in items)
				{
					list.Add(item);
				}
				Expand = true;
				subForm = new LayeredFormSelectDown(this, base.Radius, list);
				((Component)(object)subForm).Disposed += delegate
				{
					select_x = 0;
					subForm = null;
					Expand = false;
				};
				((Form)subForm).Show((IWin32Window)(object)this);
			}
			else
			{
				subForm?.IClose();
			}
		}
		else
		{
			subForm?.IClose();
		}
	}
}

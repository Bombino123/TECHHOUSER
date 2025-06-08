using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

[ProvideProperty("Tip", typeof(Control))]
[Description("提示")]
public class TooltipComponent : Component, IExtenderProvider, ITooltipConfig
{
	private readonly Dictionary<Control, string> dic = new Dictionary<Control, string>();

	private readonly List<Control> dic_in = new List<Control>();

	[Description("字体")]
	[DefaultValue(null)]
	public Font? Font { get; set; }

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(6)]
	public int Radius { get; set; } = 6;


	[Description("箭头大小")]
	[Category("外观")]
	[DefaultValue(8)]
	public int ArrowSize { get; set; } = 8;


	[Description("箭头方向")]
	[Category("外观")]
	[DefaultValue(TAlign.Top)]
	public TAlign ArrowAlign { get; set; } = TAlign.Top;


	[Description("自定义宽度")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? CustomWidth { get; set; }

	public bool CanExtend(object target)
	{
		return target is Control;
	}

	[Description("设置是否提示")]
	[DefaultValue(null)]
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
	public string? GetTip(Control item)
	{
		if (dic.TryGetValue(item, out string value))
		{
			return value;
		}
		return null;
	}

	public void SetTip(Control control, string? val)
	{
		if (val == null)
		{
			if (dic.ContainsKey(control))
			{
				dic.Remove(control);
				control.MouseEnter -= Control_Enter;
				control.MouseLeave -= Control_Leave;
				control.Leave -= Control_Leave;
			}
		}
		else if (dic.ContainsKey(control))
		{
			dic[control] = val.Trim();
		}
		else
		{
			dic.Add(control, val.Trim());
			control.MouseEnter += Control_Enter;
			control.MouseLeave += Control_Leave;
			control.Leave -= Control_Leave;
		}
	}

	private void Control_Leave(object? sender, EventArgs e)
	{
		if (sender == null)
		{
			return;
		}
		Control val = (Control)((sender is Control) ? sender : null);
		if (val != null)
		{
			lock (dic_in)
			{
				dic_in.Remove(val);
			}
		}
	}

	private void Control_Enter(object? sender, EventArgs e)
	{
		if (sender == null)
		{
			return;
		}
		Control obj = (Control)((sender is Control) ? sender : null);
		if (obj == null)
		{
			return;
		}
		lock (dic_in)
		{
			dic_in.Add(obj);
		}
		ITask.Run(delegate
		{
			Thread.Sleep(500);
			if (dic_in.Contains(obj))
			{
				obj.BeginInvoke((Delegate)(Action)delegate
				{
					((Control)new TooltipForm(obj, dic[obj], this)).Show();
				});
			}
		});
	}
}

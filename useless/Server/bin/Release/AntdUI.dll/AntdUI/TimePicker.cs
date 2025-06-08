using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

[Description("TimePicker 时间选择框")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
[DefaultEvent("ValueChanged")]
public class TimePicker : Input, SubLayeredForm
{
	private TimeSpan _value = new TimeSpan(0, 0, 0);

	private bool showicon = true;

	private bool expandDrop;

	private ILayeredForm? subForm;

	[Description("格式化")]
	[Category("行为")]
	[DefaultValue("HH:mm:ss")]
	public string Format { get; set; } = "HH:mm:ss";


	[Description("控件当前日期")]
	[Category("数据")]
	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	public TimeSpan Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			((Control)this).Text = new DateTime(1997, 1, 1, value.Hours, value.Minutes, value.Seconds).ToString(Format);
			this.ValueChanged?.Invoke(this, new TimeSpanNEventArgs(value));
			OnPropertyChanged("Value");
		}
	}

	[Description("菜单弹出位置")]
	[Category("行为")]
	[DefaultValue(TAlignFrom.BL)]
	public TAlignFrom Placement { get; set; } = TAlignFrom.BL;


	[Description("时间值水平对齐")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool ValueTimeHorizontal { get; set; }

	[Description("下拉箭头是否显示")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool DropDownArrow { get; set; }

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
			if (!base.ReadOnly && value)
			{
				if (subForm == null)
				{
					subForm = new LayeredFormCalendarTime(this, ReadRectangle, _value, delegate(TimeSpan date)
					{
						Value = date;
					});
					((Component)(object)subForm).Disposed += delegate
					{
						subForm = null;
						ExpandDrop = false;
					};
					((Form)subForm).Show((IWin32Window)(object)this);
				}
			}
			else
			{
				subForm?.IClose();
			}
		}
	}

	public event TimeSpanNEventHandler? ValueChanged;

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).Text = new DateTime(1997, 1, 1, _value.Hours, _value.Minutes, _value.Seconds).ToString(Format);
		base.OnHandleCreated(e);
	}

	protected override void PaintRIcon(Canvas g, Rectangle rect_r)
	{
		if (!showicon)
		{
			return;
		}
		Bitmap val = SvgDb.IcoTime.SvgToBmp(rect_r.Width, rect_r.Height, Colour.TextQuaternary.Get("TimePicker"));
		try
		{
			if (val != null)
			{
				g.Image(val, rect_r);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		ExpandDrop = false;
		if (((Control)this).IsHandleCreated)
		{
			if (DateTime.TryParse("1997-1-1 " + ((Control)this).Text, out var result))
			{
				Value = new TimeSpan(result.Hour, result.Minute, result.Second);
			}
			else
			{
				((Control)this).Text = new DateTime(1997, 1, 1, _value.Hours, _value.Minutes, _value.Seconds).ToString(Format);
			}
		}
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	protected override void OnClearValue()
	{
		Value = new TimeSpan(0, 0, 0);
	}

	protected override void OnClickContent()
	{
		if (base.HasFocus)
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

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		bool result = base.ProcessCmdKey(ref msg, keyData);
		if ((int)keyData == 27 && subForm != null)
		{
			subForm.IClose();
			return true;
		}
		if ((int)keyData == 40 && subForm == null)
		{
			ExpandDrop = true;
			return true;
		}
		if ((int)keyData == 13 && DateTime.TryParse("1997-1-1 " + ((Control)this).Text, out var result2))
		{
			Value = new TimeSpan(result2.Hour, result2.Minute, result2.Second);
			if (subForm is LayeredFormCalendarTime layeredFormCalendarTime)
			{
				layeredFormCalendarTime.SelDate = Value;
				layeredFormCalendarTime.Print();
			}
			return true;
		}
		return result;
	}
}

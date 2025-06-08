using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;

namespace AntdUI;

[Description("DatePicker 日期选择框")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
[DefaultEvent("ValueChanged")]
public class DatePicker : Input, SubLayeredForm
{
	private string dateFormat = "yyyy-MM-dd";

	internal bool ShowTime;

	private DateTime? _value;

	public Func<DateTime[], List<DateBadge>?>? BadgeAction;

	private BaseCollection? items;

	private bool showicon = true;

	private bool expandDrop;

	private LayeredFormCalendar? subForm;

	[Description("格式化")]
	[Category("行为")]
	[DefaultValue("yyyy-MM-dd")]
	public string Format
	{
		get
		{
			return dateFormat;
		}
		set
		{
			if (!(dateFormat == value))
			{
				dateFormat = value;
				ShowTime = dateFormat.Contains("H");
				((Control)this).Text = (_value.HasValue ? _value.Value.ToString(dateFormat) : "");
				OnPropertyChanged("Format");
			}
		}
	}

	[Description("控件当前日期")]
	[Category("数据")]
	[DefaultValue(null)]
	public DateTime? Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (!(_value == value))
			{
				_value = value;
				this.ValueChanged?.Invoke(this, new DateTimeNEventArgs(value));
				((Control)this).Text = (value.HasValue ? value.Value.ToString(Format) : "");
				OnPropertyChanged("Value");
			}
		}
	}

	[Description("最小日期")]
	[Category("数据")]
	[DefaultValue(null)]
	public DateTime? MinDate { get; set; }

	[Description("最大日期")]
	[Category("数据")]
	[DefaultValue(null)]
	public DateTime? MaxDate { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor", typeof(UITypeEditor))]
	[Description("预置")]
	[Category("数据")]
	[DefaultValue(null)]
	public BaseCollection Presets
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
					subForm = new LayeredFormCalendar(this, ReadRectangle, _value, delegate(DateTime date)
					{
						Value = date;
					}, delegate(object btn)
					{
						this.PresetsClickChanged?.Invoke(this, new ObjectNEventArgs(btn));
					}, BadgeAction);
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

	public event DateTimeNEventHandler? ValueChanged;

	[Description("预置点击时发生")]
	[Category("行为")]
	public event ObjectNEventHandler? PresetsClickChanged;

	protected override void OnHandleCreated(EventArgs e)
	{
		if (_value.HasValue)
		{
			((Control)this).Text = _value.Value.ToString(Format);
		}
		base.OnHandleCreated(e);
	}

	protected override void PaintRIcon(Canvas g, Rectangle rect_r)
	{
		if (!showicon)
		{
			return;
		}
		Bitmap val = SvgDb.IcoDate.SvgToBmp(rect_r.Width, rect_r.Height, Colour.TextQuaternary.Get("DatePicker"));
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
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		if (DateTime.TryParseExact(((Control)this).Text, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			Value = result;
			if (subForm != null)
			{
				LayeredFormCalendar? layeredFormCalendar = subForm;
				DateTime value = (subForm.Date = result);
				layeredFormCalendar.SelDate = value;
				subForm.Print();
			}
		}
		else if (_value.HasValue)
		{
			((Control)this).Text = _value.Value.ToString(Format);
		}
		else
		{
			((Control)this).Text = "";
		}
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	protected override void OnClearValue()
	{
		Value = null;
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
		if ((int)keyData == 13 && DateTime.TryParse(((Control)this).Text, out var result2))
		{
			Value = result2;
			if (subForm != null)
			{
				LayeredFormCalendar? layeredFormCalendar = subForm;
				DateTime value = (subForm.Date = result2);
				layeredFormCalendar.SelDate = value;
				subForm.Print();
			}
			return true;
		}
		return result;
	}
}

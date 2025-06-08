using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CustomControls.RJControls;

[DefaultEvent("OnSelectedIndexChanged")]
public class RJComboBox : UserControl
{
	private Color backColor = Color.WhiteSmoke;

	private Color iconColor = Color.MediumSlateBlue;

	private Color listBackColor = Color.FromArgb(230, 228, 245);

	private Color listTextColor = Color.DimGray;

	private Color borderColor = Color.MediumSlateBlue;

	private int borderSize = 1;

	private ComboBox cmbList;

	private Label lblText;

	private Button btnIcon;

	[Category("RJ Code - Appearance")]
	public Color BackColor
	{
		get
		{
			return backColor;
		}
		set
		{
			backColor = value;
			((Control)lblText).BackColor = backColor;
			((Control)btnIcon).BackColor = backColor;
		}
	}

	[Category("RJ Code - Appearance")]
	public Color IconColor
	{
		get
		{
			return iconColor;
		}
		set
		{
			iconColor = value;
			((Control)btnIcon).Invalidate();
		}
	}

	[Category("RJ Code - Appearance")]
	public Color ListBackColor
	{
		get
		{
			return listBackColor;
		}
		set
		{
			listBackColor = value;
			((Control)cmbList).BackColor = listBackColor;
		}
	}

	[Category("RJ Code - Appearance")]
	public Color ListTextColor
	{
		get
		{
			return listTextColor;
		}
		set
		{
			listTextColor = value;
			((Control)cmbList).ForeColor = listTextColor;
		}
	}

	[Category("RJ Code - Appearance")]
	public Color BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			borderColor = value;
			((Control)this).BackColor = borderColor;
		}
	}

	[Category("RJ Code - Appearance")]
	public int BorderSize
	{
		get
		{
			return borderSize;
		}
		set
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			borderSize = value;
			((Control)this).Padding = new Padding(borderSize);
			AdjustComboBoxDimensions();
		}
	}

	[Category("RJ Code - Appearance")]
	public override Color ForeColor
	{
		get
		{
			return ((Control)this).ForeColor;
		}
		set
		{
			((Control)this).ForeColor = value;
			((Control)lblText).ForeColor = value;
		}
	}

	[Category("RJ Code - Appearance")]
	public override Font Font
	{
		get
		{
			return ((Control)this).Font;
		}
		set
		{
			((Control)this).Font = value;
			((Control)lblText).Font = value;
			((Control)cmbList).Font = value;
			AdjustComboBoxDimensions();
		}
	}

	[Category("RJ Code - Appearance")]
	public string Texts
	{
		get
		{
			return ((Control)lblText).Text;
		}
		set
		{
			((Control)lblText).Text = value;
		}
	}

	[Category("RJ Code - Appearance")]
	public ComboBoxStyle DropDownStyle
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return cmbList.DropDownStyle;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if ((int)cmbList.DropDownStyle != 0)
			{
				cmbList.DropDownStyle = value;
			}
		}
	}

	[Category("RJ Code - Data")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
	[Localizable(true)]
	[MergableProperty(false)]
	public ObjectCollection Items => cmbList.Items;

	[Category("RJ Code - Data")]
	[AttributeProvider(typeof(IListSource))]
	[DefaultValue(null)]
	public object DataSource
	{
		get
		{
			return cmbList.DataSource;
		}
		set
		{
			cmbList.DataSource = value;
		}
	}

	[Category("RJ Code - Data")]
	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
	[EditorBrowsable(EditorBrowsableState.Always)]
	[Localizable(true)]
	public AutoCompleteStringCollection AutoCompleteCustomSource
	{
		get
		{
			return cmbList.AutoCompleteCustomSource;
		}
		set
		{
			cmbList.AutoCompleteCustomSource = value;
		}
	}

	[Category("RJ Code - Data")]
	[Browsable(true)]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	[EditorBrowsable(EditorBrowsableState.Always)]
	public AutoCompleteSource AutoCompleteSource
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return cmbList.AutoCompleteSource;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			cmbList.AutoCompleteSource = value;
		}
	}

	[Category("RJ Code - Data")]
	[Browsable(true)]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	[EditorBrowsable(EditorBrowsableState.Always)]
	public AutoCompleteMode AutoCompleteMode
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return cmbList.AutoCompleteMode;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			cmbList.AutoCompleteMode = value;
		}
	}

	[Category("RJ Code - Data")]
	[Bindable(true)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object SelectedItem
	{
		get
		{
			return cmbList.SelectedItem;
		}
		set
		{
			cmbList.SelectedItem = value;
		}
	}

	[Category("RJ Code - Data")]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int SelectedIndex
	{
		get
		{
			return ((ListControl)cmbList).SelectedIndex;
		}
		set
		{
			((ListControl)cmbList).SelectedIndex = value;
		}
	}

	[Category("RJ Code - Data")]
	[DefaultValue("")]
	[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
	[TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public string DisplayMember
	{
		get
		{
			return ((ListControl)cmbList).DisplayMember;
		}
		set
		{
			((ListControl)cmbList).DisplayMember = value;
		}
	}

	[Category("RJ Code - Data")]
	[DefaultValue("")]
	[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
	public string ValueMember
	{
		get
		{
			return ((ListControl)cmbList).ValueMember;
		}
		set
		{
			((ListControl)cmbList).ValueMember = value;
		}
	}

	public event EventHandler OnSelectedIndexChanged;

	public RJComboBox()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected O, but got Unknown
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Expected O, but got Unknown
		cmbList = new ComboBox();
		lblText = new Label();
		btnIcon = new Button();
		((Control)this).SuspendLayout();
		((Control)cmbList).BackColor = listBackColor;
		((Control)cmbList).Font = new Font(((Control)this).Font.Name, 10f);
		((Control)cmbList).ForeColor = listTextColor;
		cmbList.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
		((Control)cmbList).TextChanged += ComboBox_TextChanged;
		((Control)btnIcon).Dock = (DockStyle)4;
		((ButtonBase)btnIcon).FlatStyle = (FlatStyle)0;
		((ButtonBase)btnIcon).FlatAppearance.BorderSize = 0;
		((Control)btnIcon).BackColor = backColor;
		((Control)btnIcon).Size = new Size(30, 30);
		((Control)btnIcon).Cursor = Cursors.Hand;
		((Control)btnIcon).Click += Icon_Click;
		((Control)btnIcon).Paint += new PaintEventHandler(Icon_Paint);
		((Control)lblText).Dock = (DockStyle)5;
		((Control)lblText).AutoSize = false;
		((Control)lblText).BackColor = backColor;
		lblText.TextAlign = (ContentAlignment)16;
		((Control)lblText).Padding = new Padding(8, 0, 0, 0);
		((Control)lblText).Font = new Font(((Control)this).Font.Name, 10f);
		((Control)lblText).Click += Surface_Click;
		((Control)lblText).MouseEnter += Surface_MouseEnter;
		((Control)lblText).MouseLeave += Surface_MouseLeave;
		((Control)this).Controls.Add((Control)(object)lblText);
		((Control)this).Controls.Add((Control)(object)btnIcon);
		((Control)this).Controls.Add((Control)(object)cmbList);
		((Control)this).MinimumSize = new Size(200, 30);
		((Control)this).Size = new Size(200, 30);
		((Control)this).ForeColor = Color.DimGray;
		((Control)this).Padding = new Padding(borderSize);
		((Control)this).Font = new Font(((Control)this).Font.Name, 10f);
		((Control)this).BackColor = borderColor;
		((UserControl)this).Load += RJComboBox_Load;
		((Control)this).ResumeLayout();
		AdjustComboBoxDimensions();
	}

	private void AdjustComboBoxDimensions()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		((Control)cmbList).Width = ((Control)lblText).Width;
		ComboBox obj = cmbList;
		Point location = default(Point);
		int width = ((Control)this).Width;
		Padding padding = ((Control)this).Padding;
		location.X = width - ((Padding)(ref padding)).Right - ((Control)cmbList).Width;
		location.Y = ((Control)lblText).Bottom - ((Control)cmbList).Height;
		((Control)obj).Location = location;
		if (((Control)cmbList).Height >= ((Control)this).Height)
		{
			((Control)this).Height = ((Control)cmbList).Height + borderSize * 2;
		}
	}

	private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (this.OnSelectedIndexChanged != null)
		{
			this.OnSelectedIndexChanged(sender, e);
		}
		((Control)lblText).Text = ((Control)cmbList).Text;
	}

	private void Icon_Paint(object sender, PaintEventArgs e)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		int num = 14;
		int num2 = 6;
		Rectangle rectangle = new Rectangle((((Control)btnIcon).Width - num) / 2, (((Control)btnIcon).Height - num2) / 2, num, num2);
		Graphics graphics = e.Graphics;
		GraphicsPath val = new GraphicsPath();
		try
		{
			Pen val2 = new Pen(iconColor, 2f);
			try
			{
				graphics.SmoothingMode = (SmoothingMode)4;
				val.AddLine(rectangle.X, rectangle.Y, rectangle.X + num / 2, rectangle.Bottom);
				val.AddLine(rectangle.X + num / 2, rectangle.Bottom, rectangle.Right, rectangle.Y);
				graphics.DrawPath(val2, val);
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

	private void Icon_Click(object sender, EventArgs e)
	{
		((Control)cmbList).Select();
		cmbList.DroppedDown = true;
	}

	private void Surface_Click(object sender, EventArgs e)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		((Control)this).OnClick(e);
		((Control)cmbList).Select();
		if ((int)cmbList.DropDownStyle == 2)
		{
			cmbList.DroppedDown = true;
		}
	}

	private void ComboBox_TextChanged(object sender, EventArgs e)
	{
		((Control)lblText).Text = ((Control)cmbList).Text;
	}

	private void Surface_MouseLeave(object sender, EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
	}

	private void Surface_MouseEnter(object sender, EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
	}

	protected override void OnResize(EventArgs e)
	{
		((UserControl)this).OnResize(e);
		if (((Component)this).DesignMode)
		{
			AdjustComboBoxDimensions();
		}
	}

	private void RJComboBox_Load(object sender, EventArgs e)
	{
		AdjustComboBoxDimensions();
	}
}

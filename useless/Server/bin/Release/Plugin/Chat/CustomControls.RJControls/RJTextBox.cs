using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CustomControls.RJControls;

[DefaultEvent("_TextChanged")]
public class RJTextBox : UserControl
{
	private Color borderColor = Color.MediumSlateBlue;

	private Color borderFocusColor = Color.HotPink;

	private int borderSize = 2;

	private bool underlinedStyle;

	private bool isFocused;

	private int borderRadius;

	private Color placeholderColor = Color.DarkGray;

	private string placeholderText = "";

	private bool isPlaceholder;

	private bool isPasswordChar;

	private IContainer components;

	public TextBox textBox1;

	[Category("RJ Code Advance")]
	public Color BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			borderColor = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public Color BorderFocusColor
	{
		get
		{
			return borderFocusColor;
		}
		set
		{
			borderFocusColor = value;
		}
	}

	[Category("RJ Code Advance")]
	public int BorderSize
	{
		get
		{
			return borderSize;
		}
		set
		{
			if (value >= 1)
			{
				borderSize = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Category("RJ Code Advance")]
	public bool UnderlinedStyle
	{
		get
		{
			return underlinedStyle;
		}
		set
		{
			underlinedStyle = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public bool PasswordChar
	{
		get
		{
			return isPasswordChar;
		}
		set
		{
			isPasswordChar = value;
			if (!isPlaceholder)
			{
				textBox1.UseSystemPasswordChar = value;
			}
		}
	}

	[Category("RJ Code Advance")]
	public bool Multiline
	{
		get
		{
			return ((TextBoxBase)textBox1).Multiline;
		}
		set
		{
			((TextBoxBase)textBox1).Multiline = value;
		}
	}

	[Category("RJ Code Advance")]
	public override Color BackColor
	{
		get
		{
			return ((Control)this).BackColor;
		}
		set
		{
			((Control)this).BackColor = value;
			((Control)textBox1).BackColor = value;
		}
	}

	[Category("RJ Code Advance")]
	public override Color ForeColor
	{
		get
		{
			return ((Control)this).ForeColor;
		}
		set
		{
			((Control)this).ForeColor = value;
			((Control)textBox1).ForeColor = value;
		}
	}

	[Category("RJ Code Advance")]
	public override Font Font
	{
		get
		{
			return ((Control)this).Font;
		}
		set
		{
			((Control)this).Font = value;
			((Control)textBox1).Font = value;
			if (((Component)this).DesignMode)
			{
				UpdateControlHeight();
			}
		}
	}

	[Category("RJ Code Advance")]
	public string Texts
	{
		get
		{
			if (isPlaceholder)
			{
				return "";
			}
			return ((Control)textBox1).Text;
		}
		set
		{
			RemovePlaceholder();
			((Control)textBox1).Text = value;
			SetPlaceholder();
		}
	}

	[Category("RJ Code Advance")]
	public int BorderRadius
	{
		get
		{
			return borderRadius;
		}
		set
		{
			if (value >= 0)
			{
				borderRadius = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Category("RJ Code Advance")]
	public Color PlaceholderColor
	{
		get
		{
			return placeholderColor;
		}
		set
		{
			placeholderColor = value;
			if (isPlaceholder)
			{
				((Control)textBox1).ForeColor = value;
			}
		}
	}

	[Category("RJ Code Advance")]
	public string PlaceholderText
	{
		get
		{
			return placeholderText;
		}
		set
		{
			placeholderText = value;
			((Control)textBox1).Text = "";
			SetPlaceholder();
		}
	}

	public event EventHandler _TextChanged;

	public RJTextBox()
	{
		InitializeComponent();
	}

	protected override void OnResize(EventArgs e)
	{
		((UserControl)this).OnResize(e);
		if (((Component)this).DesignMode)
		{
			UpdateControlHeight();
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		((UserControl)this).OnLoad(e);
		UpdateControlHeight();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		((Control)this).OnPaint(e);
		Graphics graphics = e.Graphics;
		if (borderRadius > 1)
		{
			Rectangle clientRectangle = ((Control)this).ClientRectangle;
			Rectangle rect = Rectangle.Inflate(clientRectangle, -borderSize, -borderSize);
			int num = ((borderSize <= 0) ? 1 : borderSize);
			GraphicsPath figurePath = GetFigurePath(clientRectangle, borderRadius);
			try
			{
				GraphicsPath figurePath2 = GetFigurePath(rect, borderRadius - borderSize);
				try
				{
					Pen val = new Pen(((Control)this).Parent.BackColor, (float)num);
					try
					{
						Pen val2 = new Pen(borderColor, (float)borderSize);
						try
						{
							((Control)this).Region = new Region(figurePath);
							if (borderRadius > 15)
							{
								SetTextBoxRoundedRegion();
							}
							graphics.SmoothingMode = (SmoothingMode)4;
							val2.Alignment = (PenAlignment)0;
							if (isFocused)
							{
								val2.Color = borderFocusColor;
							}
							if (underlinedStyle)
							{
								graphics.DrawPath(val, figurePath);
								graphics.SmoothingMode = (SmoothingMode)3;
								graphics.DrawLine(val2, 0, ((Control)this).Height - 1, ((Control)this).Width, ((Control)this).Height - 1);
							}
							else
							{
								graphics.DrawPath(val, figurePath);
								graphics.DrawPath(val2, figurePath2);
							}
							return;
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
				finally
				{
					((IDisposable)figurePath2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)figurePath)?.Dispose();
			}
		}
		Pen val3 = new Pen(borderColor, (float)borderSize);
		try
		{
			((Control)this).Region = new Region(((Control)this).ClientRectangle);
			val3.Alignment = (PenAlignment)1;
			if (isFocused)
			{
				val3.Color = borderFocusColor;
			}
			if (underlinedStyle)
			{
				graphics.DrawLine(val3, 0, ((Control)this).Height - 1, ((Control)this).Width, ((Control)this).Height - 1);
			}
			else
			{
				graphics.DrawRectangle(val3, 0f, 0f, (float)((Control)this).Width - 0.5f, (float)((Control)this).Height - 0.5f);
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	private void SetPlaceholder()
	{
		if (string.IsNullOrWhiteSpace(((Control)textBox1).Text) && placeholderText != "")
		{
			isPlaceholder = true;
			((Control)textBox1).Text = placeholderText;
			((Control)textBox1).ForeColor = placeholderColor;
			if (isPasswordChar)
			{
				textBox1.UseSystemPasswordChar = false;
			}
		}
	}

	private void RemovePlaceholder()
	{
		if (isPlaceholder && placeholderText != "")
		{
			isPlaceholder = false;
			((Control)textBox1).Text = "";
			((Control)textBox1).ForeColor = ((Control)this).ForeColor;
			if (isPasswordChar)
			{
				textBox1.UseSystemPasswordChar = true;
			}
		}
	}

	private GraphicsPath GetFigurePath(Rectangle rect, int radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		float num = (float)radius * 2f;
		val.StartFigure();
		val.AddArc((float)rect.X, (float)rect.Y, num, num, 180f, 90f);
		val.AddArc((float)rect.Right - num, (float)rect.Y, num, num, 270f, 90f);
		val.AddArc((float)rect.Right - num, (float)rect.Bottom - num, num, num, 0f, 90f);
		val.AddArc((float)rect.X, (float)rect.Bottom - num, num, num, 90f, 90f);
		val.CloseFigure();
		return val;
	}

	private void SetTextBoxRoundedRegion()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		GraphicsPath figurePath;
		if (Multiline)
		{
			figurePath = GetFigurePath(((Control)textBox1).ClientRectangle, borderRadius - borderSize);
			((Control)textBox1).Region = new Region(figurePath);
		}
		else
		{
			figurePath = GetFigurePath(((Control)textBox1).ClientRectangle, borderSize * 2);
			((Control)textBox1).Region = new Region(figurePath);
		}
		figurePath.Dispose();
	}

	private void UpdateControlHeight()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((TextBoxBase)textBox1).Multiline)
		{
			int height = TextRenderer.MeasureText("Text", ((Control)this).Font).Height + 1;
			((TextBoxBase)textBox1).Multiline = true;
			((Control)textBox1).MinimumSize = new Size(0, height);
			((TextBoxBase)textBox1).Multiline = false;
			int height2 = ((Control)textBox1).Height;
			Padding padding = ((Control)this).Padding;
			int num = height2 + ((Padding)(ref padding)).Top;
			padding = ((Control)this).Padding;
			((Control)this).Height = num + ((Padding)(ref padding)).Bottom;
		}
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
		if (this._TextChanged != null)
		{
			this._TextChanged(sender, e);
		}
	}

	private void textBox1_Click(object sender, EventArgs e)
	{
		((Control)this).OnClick(e);
	}

	private void textBox1_MouseEnter(object sender, EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
	}

	private void textBox1_MouseLeave(object sender, EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
	}

	private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
	{
		((Control)this).OnKeyPress(e);
	}

	private void textBox1_Enter(object sender, EventArgs e)
	{
		isFocused = true;
		((Control)this).Invalidate();
		RemovePlaceholder();
	}

	private void textBox1_Leave(object sender, EventArgs e)
	{
		isFocused = false;
		((Control)this).Invalidate();
		SetPlaceholder();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((ContainerControl)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		textBox1 = new TextBox();
		((Control)this).SuspendLayout();
		((TextBoxBase)textBox1).BorderStyle = (BorderStyle)0;
		((Control)textBox1).Dock = (DockStyle)5;
		((Control)textBox1).Location = new Point(10, 7);
		((Control)textBox1).Name = "textBox1";
		((Control)textBox1).Size = new Size(230, 15);
		((Control)textBox1).TabIndex = 0;
		((TextBoxBase)textBox1).Click += textBox1_Click;
		((Control)textBox1).TextChanged += textBox1_TextChanged;
		((Control)textBox1).Enter += textBox1_Enter;
		((Control)textBox1).KeyPress += new KeyPressEventHandler(textBox1_KeyPress);
		((Control)textBox1).Leave += textBox1_Leave;
		((Control)textBox1).MouseEnter += textBox1_MouseEnter;
		((Control)textBox1).MouseLeave += textBox1_MouseLeave;
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)0;
		((Control)this).BackColor = SystemColors.Window;
		((Control)this).Controls.Add((Control)(object)textBox1);
		((Control)this).Font = new Font("Microsoft Sans Serif", 9.5f, (FontStyle)0, (GraphicsUnit)3, (byte)0);
		((Control)this).ForeColor = Color.FromArgb(64, 64, 64);
		((Control)this).Margin = new Padding(4);
		((Control)this).Name = "RJTextBox";
		((Control)this).Padding = new Padding(10, 7, 10, 7);
		((Control)this).Size = new Size(250, 30);
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}

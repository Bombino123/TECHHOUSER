using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NAudio.Gui;

public class VolumeMeter : Control
{
	private Brush foregroundBrush;

	private float amplitude;

	private IContainer components;

	[DefaultValue(-3.0)]
	public float Amplitude
	{
		get
		{
			return amplitude;
		}
		set
		{
			amplitude = value;
			((Control)this).Invalidate();
		}
	}

	[DefaultValue(-60.0)]
	public float MinDb { get; set; }

	[DefaultValue(18.0)]
	public float MaxDb { get; set; }

	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public Orientation Orientation { get; set; }

	public VolumeMeter()
	{
		((Control)this).SetStyle((ControlStyles)139266, true);
		MinDb = -60f;
		MaxDb = 18f;
		Amplitude = 0f;
		Orientation = (Orientation)1;
		InitializeComponent();
		((Control)this).OnForeColorChanged(EventArgs.Empty);
	}

	protected override void OnForeColorChanged(EventArgs e)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		foregroundBrush = (Brush)new SolidBrush(((Control)this).ForeColor);
		((Control)this).OnForeColorChanged(e);
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		pe.Graphics.DrawRectangle(Pens.Black, 0, 0, ((Control)this).Width - 1, ((Control)this).Height - 1);
		double num = 20.0 * Math.Log10(Amplitude);
		if (num < (double)MinDb)
		{
			num = MinDb;
		}
		if (num > (double)MaxDb)
		{
			num = MaxDb;
		}
		double num2 = (num - (double)MinDb) / (double)(MaxDb - MinDb);
		int num3 = ((Control)this).Width - 2;
		int num4 = ((Control)this).Height - 2;
		if ((int)Orientation == 0)
		{
			num3 = (int)((double)num3 * num2);
			pe.Graphics.FillRectangle(foregroundBrush, 1, 1, num3, num4);
		}
		else
		{
			num4 = (int)((double)num4 * num2);
			pe.Graphics.FillRectangle(foregroundBrush, 1, ((Control)this).Height - 1 - num4, num3, num4);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Control)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
	}
}

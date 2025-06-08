using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NAudio.Gui;

public class PanSlider : UserControl
{
	private Container components;

	private float pan;

	public float Pan
	{
		get
		{
			return pan;
		}
		set
		{
			if (value < -1f)
			{
				value = -1f;
			}
			if (value > 1f)
			{
				value = 1f;
			}
			if (value != pan)
			{
				pan = value;
				if (this.PanChanged != null)
				{
					this.PanChanged(this, EventArgs.Empty);
				}
				((Control)this).Invalidate();
			}
		}
	}

	public event EventHandler PanChanged;

	public PanSlider()
	{
		InitializeComponent();
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
		((Control)this).Name = "PanSlider";
		((Control)this).Size = new Size(104, 16);
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		StringFormat val = new StringFormat();
		val.LineAlignment = (StringAlignment)1;
		val.Alignment = (StringAlignment)1;
		string text;
		if ((double)pan == 0.0)
		{
			pe.Graphics.FillRectangle(Brushes.Orange, ((Control)this).Width / 2 - 1, 1, 3, ((Control)this).Height - 2);
			text = "C";
		}
		else if (pan > 0f)
		{
			pe.Graphics.FillRectangle(Brushes.Orange, ((Control)this).Width / 2, 1, (int)((float)(((Control)this).Width / 2) * pan), ((Control)this).Height - 2);
			text = $"{pan * 100f:F0}%R";
		}
		else
		{
			pe.Graphics.FillRectangle(Brushes.Orange, (int)((float)(((Control)this).Width / 2) * (pan + 1f)), 1, (int)((float)(((Control)this).Width / 2) * (0f - pan)), ((Control)this).Height - 2);
			text = $"{pan * -100f:F0}%L";
		}
		pe.Graphics.DrawRectangle(Pens.Black, 0, 0, ((Control)this).Width - 1, ((Control)this).Height - 1);
		pe.Graphics.DrawString(text, ((Control)this).Font, Brushes.Black, (RectangleF)((Control)this).ClientRectangle, val);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576)
		{
			SetPanFromMouse(e.X);
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		SetPanFromMouse(e.X);
		((UserControl)this).OnMouseDown(e);
	}

	private void SetPanFromMouse(int x)
	{
		Pan = (float)x / (float)((Control)this).Width * 2f - 1f;
	}
}

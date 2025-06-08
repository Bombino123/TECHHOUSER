using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NAudio.Gui;

public class WaveformPainter : Control
{
	private Pen foregroundPen;

	private List<float> samples = new List<float>(1000);

	private int maxSamples;

	private int insertPos;

	private IContainer components;

	public WaveformPainter()
	{
		((Control)this).SetStyle((ControlStyles)139266, true);
		InitializeComponent();
		((Control)this).OnForeColorChanged(EventArgs.Empty);
		((Control)this).OnResize(EventArgs.Empty);
	}

	protected override void OnResize(EventArgs e)
	{
		maxSamples = ((Control)this).Width;
		((Control)this).OnResize(e);
	}

	protected override void OnForeColorChanged(EventArgs e)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		foregroundPen = new Pen(((Control)this).ForeColor);
		((Control)this).OnForeColorChanged(e);
	}

	public void AddMax(float maxSample)
	{
		if (maxSamples != 0)
		{
			if (samples.Count <= maxSamples)
			{
				samples.Add(maxSample);
			}
			else if (insertPos < maxSamples)
			{
				samples[insertPos] = maxSample;
			}
			insertPos++;
			insertPos %= maxSamples;
			((Control)this).Invalidate();
		}
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		((Control)this).OnPaint(pe);
		for (int i = 0; i < ((Control)this).Width; i++)
		{
			float num = (float)((Control)this).Height * GetSample(i - ((Control)this).Width + insertPos);
			float num2 = ((float)((Control)this).Height - num) / 2f;
			pe.Graphics.DrawLine(foregroundPen, (float)i, num2, (float)i, num2 + num);
		}
	}

	private float GetSample(int index)
	{
		if (index < 0)
		{
			index += maxSamples;
		}
		if ((index >= 0) & (index < samples.Count))
		{
			return samples[index];
		}
		return 0f;
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

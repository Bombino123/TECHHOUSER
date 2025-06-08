using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NAudio.Utils;

public class ProgressLog : UserControl
{
	private delegate void LogMessageDelegate(Color color, string message);

	private delegate void ClearLogDelegate();

	private IContainer components;

	private RichTextBox richTextBoxLog;

	public string Text => ((Control)richTextBoxLog).Text;

	public ProgressLog()
	{
		InitializeComponent();
	}

	public void LogMessage(Color color, string message)
	{
		if (((Control)richTextBoxLog).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)new LogMessageDelegate(LogMessage), new object[2] { color, message });
		}
		else
		{
			((TextBoxBase)richTextBoxLog).SelectionStart = ((TextBoxBase)richTextBoxLog).TextLength;
			richTextBoxLog.SelectionColor = color;
			((TextBoxBase)richTextBoxLog).AppendText(message);
			((TextBoxBase)richTextBoxLog).AppendText(Environment.NewLine);
		}
	}

	public void ClearLog()
	{
		if (((Control)richTextBoxLog).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)new ClearLogDelegate(ClearLog), new object[0]);
		}
		else
		{
			((TextBoxBase)richTextBoxLog).Clear();
		}
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
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		richTextBoxLog = new RichTextBox();
		((Control)this).SuspendLayout();
		((TextBoxBase)richTextBoxLog).BorderStyle = (BorderStyle)0;
		((Control)richTextBoxLog).Dock = (DockStyle)5;
		((Control)richTextBoxLog).Location = new Point(1, 1);
		((Control)richTextBoxLog).Name = "richTextBoxLog";
		((TextBoxBase)richTextBoxLog).ReadOnly = true;
		((Control)richTextBoxLog).Size = new Size(311, 129);
		((Control)richTextBoxLog).TabIndex = 0;
		((Control)richTextBoxLog).Text = "";
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = SystemColors.ControlDarkDark;
		((Control)this).Controls.Add((Control)(object)richTextBoxLog);
		((Control)this).Name = "ProgressLog";
		((Control)this).Padding = new Padding(1);
		((Control)this).Size = new Size(313, 131);
		((Control)this).ResumeLayout(false);
	}
}

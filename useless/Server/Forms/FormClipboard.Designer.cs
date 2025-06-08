namespace Server.Forms
{
	// Token: 0x0200009E RID: 158
	public partial class FormClipboard : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000453 RID: 1107 RVA: 0x000366A8 File Offset: 0x000348A8
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x000366C8 File Offset: 0x000348C8
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.richTextBox1 = new global::System.Windows.Forms.RichTextBox();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.rjButton3 = new global::CustomControls.RJControls.RJButton();
			this.rjButton2 = new global::CustomControls.RJControls.RJButton();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.richTextBox1.BackColor = global::System.Drawing.Color.White;
			this.richTextBox1.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Enabled = false;
			this.richTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.richTextBox1.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.richTextBox1.Location = new global::System.Drawing.Point(3, 64);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new global::System.Drawing.Size(467, 397);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = "";
			this.panel1.Controls.Add(this.rjButton3);
			this.panel1.Controls.Add(this.rjButton2);
			this.panel1.Controls.Add(this.rjButton1);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new global::System.Drawing.Point(3, 461);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(467, 34);
			this.panel1.TabIndex = 2;
			this.rjButton3.BackColor = global::System.Drawing.Color.White;
			this.rjButton3.BackgroundColor = global::System.Drawing.Color.White;
			this.rjButton3.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton3.BorderRadius = 0;
			this.rjButton3.BorderSize = 1;
			this.rjButton3.Enabled = false;
			this.rjButton3.FlatAppearance.BorderSize = 0;
			this.rjButton3.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton3.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton3.Location = new global::System.Drawing.Point(199, 3);
			this.rjButton3.Name = "rjButton3";
			this.rjButton3.Size = new global::System.Drawing.Size(92, 28);
			this.rjButton3.TabIndex = 16;
			this.rjButton3.Text = "Clear Clipboard";
			this.rjButton3.TextColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton3.UseVisualStyleBackColor = false;
			this.rjButton3.Click += new global::System.EventHandler(this.rjButton3_Click);
			this.rjButton2.BackColor = global::System.Drawing.Color.White;
			this.rjButton2.BackgroundColor = global::System.Drawing.Color.White;
			this.rjButton2.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton2.BorderRadius = 0;
			this.rjButton2.BorderSize = 1;
			this.rjButton2.Enabled = false;
			this.rjButton2.FlatAppearance.BorderSize = 0;
			this.rjButton2.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton2.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton2.Location = new global::System.Drawing.Point(101, 3);
			this.rjButton2.Name = "rjButton2";
			this.rjButton2.Size = new global::System.Drawing.Size(92, 28);
			this.rjButton2.TabIndex = 15;
			this.rjButton2.Text = "Set Clipboard";
			this.rjButton2.TextColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton2.UseVisualStyleBackColor = false;
			this.rjButton2.Click += new global::System.EventHandler(this.rjButton2_Click);
			this.rjButton1.BackColor = global::System.Drawing.Color.White;
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.White;
			this.rjButton1.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 1;
			this.rjButton1.Enabled = false;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.Location = new global::System.Drawing.Point(3, 3);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(92, 28);
			this.rjButton1.TabIndex = 14;
			this.rjButton1.Text = "Get Clipboard";
			this.rjButton1.TextColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(473, 498);
			base.Controls.Add(this.richTextBox1);
			base.Controls.Add(this.panel1);
			base.Name = "FormClipboard";
			this.Text = "Clipboard";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.panel1.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		// Token: 0x040002AE RID: 686
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040002AF RID: 687
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x040002B0 RID: 688
		public global::System.Windows.Forms.RichTextBox richTextBox1;

		// Token: 0x040002B1 RID: 689
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x040002B2 RID: 690
		public global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x040002B3 RID: 691
		public global::CustomControls.RJControls.RJButton rjButton2;

		// Token: 0x040002B4 RID: 692
		public global::CustomControls.RJControls.RJButton rjButton3;
	}
}

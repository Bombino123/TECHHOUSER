namespace Server.Forms
{
	// Token: 0x020000B0 RID: 176
	public partial class FormChat : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600057C RID: 1404 RVA: 0x0004F212 File Offset: 0x0004D412
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x0004F234 File Offset: 0x0004D434
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.richTextBox1 = new global::System.Windows.Forms.RichTextBox();
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.richTextBox1.BackColor = global::System.Drawing.Color.White;
			this.richTextBox1.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Enabled = false;
			this.richTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.richTextBox1.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.richTextBox1.Location = new global::System.Drawing.Point(3, 64);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new global::System.Drawing.Size(484, 437);
			this.richTextBox1.TabIndex = 15;
			this.richTextBox1.Text = "";
			this.rjTextBox1.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.HotPink;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.rjTextBox1.Enabled = false;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox1.Location = new global::System.Drawing.Point(3, 501);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "";
			this.rjTextBox1.Size = new global::System.Drawing.Size(484, 31);
			this.rjTextBox1.TabIndex = 16;
			this.rjTextBox1.Texts = "";
			this.rjTextBox1.UnderlinedStyle = false;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(490, 535);
			base.Controls.Add(this.richTextBox1);
			base.Controls.Add(this.rjTextBox1);
			base.Name = "FormChat";
			this.Text = "Chat";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x0400042E RID: 1070
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400042F RID: 1071
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000430 RID: 1072
		public global::System.Windows.Forms.RichTextBox richTextBox1;

		// Token: 0x04000431 RID: 1073
		public global::CustomControls.RJControls.RJTextBox rjTextBox1;
	}
}

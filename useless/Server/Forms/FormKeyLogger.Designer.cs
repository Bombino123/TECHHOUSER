namespace Server.Forms
{
	// Token: 0x020000B4 RID: 180
	public partial class FormKeyLogger : global::Server.Helper.FormMaterial
	{
		// Token: 0x060005A7 RID: 1447 RVA: 0x00052565 File Offset: 0x00050765
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x00052584 File Offset: 0x00050784
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.richTextBox1 = new global::System.Windows.Forms.RichTextBox();
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
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new global::System.Drawing.Size(479, 451);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = "";
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(485, 518);
			base.Controls.Add(this.richTextBox1);
			base.Name = "FormKeyLogger";
			this.Text = "KeyLogger";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x04000478 RID: 1144
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000479 RID: 1145
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x0400047A RID: 1146
		public global::System.Windows.Forms.RichTextBox richTextBox1;
	}
}

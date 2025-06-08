namespace Server.Forms
{
	// Token: 0x0200009A RID: 154
	public partial class FormNotepad : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600042E RID: 1070 RVA: 0x000335C0 File Offset: 0x000317C0
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x000335E0 File Offset: 0x000317E0
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
			this.richTextBox1.Size = new global::System.Drawing.Size(473, 451);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = "";
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(479, 518);
			base.Controls.Add(this.richTextBox1);
			base.Name = "FormNotepad";
			this.Text = "Notepad";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x0400027F RID: 639
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000280 RID: 640
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000281 RID: 641
		public global::System.Windows.Forms.RichTextBox richTextBox1;
	}
}

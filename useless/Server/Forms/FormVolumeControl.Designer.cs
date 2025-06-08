namespace Server.Forms
{
	// Token: 0x02000099 RID: 153
	public partial class FormVolumeControl : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000426 RID: 1062 RVA: 0x00033281 File Offset: 0x00031481
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x000332A0 File Offset: 0x000314A0
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.trackBar1 = new global::System.Windows.Forms.TrackBar();
			this.panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.panel1.Controls.Add(this.trackBar1);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(338, 70);
			this.panel1.TabIndex = 0;
			this.trackBar1.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.trackBar1.Enabled = false;
			this.trackBar1.Location = new global::System.Drawing.Point(14, 14);
			this.trackBar1.Maximum = 100;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new global::System.Drawing.Size(309, 45);
			this.trackBar1.TabIndex = 30;
			this.trackBar1.Scroll += new global::System.EventHandler(this.trackBar1_Scroll);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(344, 137);
			base.Controls.Add(this.panel1);
			base.Name = "FormVolumeControl";
			this.Text = "Volume ";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x04000279 RID: 633
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400027A RID: 634
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x0400027B RID: 635
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x0400027C RID: 636
		public global::System.Windows.Forms.TrackBar trackBar1;
	}
}

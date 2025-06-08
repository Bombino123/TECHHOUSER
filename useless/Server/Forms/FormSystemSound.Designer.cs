namespace Server.Forms
{
	// Token: 0x020000BD RID: 189
	public partial class FormSystemSound : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000624 RID: 1572 RVA: 0x00059C14 File Offset: 0x00057E14
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x00059C34 File Offset: 0x00057E34
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.trackBar1 = new global::System.Windows.Forms.TrackBar();
			this.materialSwitch1 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			((global::System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
			base.SuspendLayout();
			this.trackBar1.Enabled = false;
			this.trackBar1.Location = new global::System.Drawing.Point(25, 80);
			this.trackBar1.Maximum = 100;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new global::System.Drawing.Size(250, 45);
			this.trackBar1.TabIndex = 30;
			this.trackBar1.Value = 10;
			this.trackBar1.Scroll += new global::System.EventHandler(this.trackBar1_Scroll);
			this.materialSwitch1.AutoSize = true;
			this.materialSwitch1.Depth = 0;
			this.materialSwitch1.Enabled = false;
			this.materialSwitch1.Location = new global::System.Drawing.Point(25, 128);
			this.materialSwitch1.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch1.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch1.Name = "materialSwitch1";
			this.materialSwitch1.Ripple = true;
			this.materialSwitch1.Size = new global::System.Drawing.Size(92, 37);
			this.materialSwitch1.TabIndex = 29;
			this.materialSwitch1.Text = "Start";
			this.materialSwitch1.UseVisualStyleBackColor = true;
			this.materialSwitch1.CheckedChanged += new global::System.EventHandler(this.materialSwitch1_CheckedChanged);
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(335, 194);
			base.Controls.Add(this.trackBar1);
			base.Controls.Add(this.materialSwitch1);
			base.Name = "FormSystemSound";
			this.Text = "System Sound";
			base.Load += new global::System.EventHandler(this.FormSystemSound_Load);
			((global::System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x040004F5 RID: 1269
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040004F6 RID: 1270
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch1;

		// Token: 0x040004F7 RID: 1271
		public global::System.Windows.Forms.TrackBar trackBar1;

		// Token: 0x040004F8 RID: 1272
		public global::System.Windows.Forms.Timer timer1;
	}
}

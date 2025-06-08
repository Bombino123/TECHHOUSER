namespace Server.Forms
{
	// Token: 0x02000095 RID: 149
	public partial class FormAbout : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000405 RID: 1029 RVA: 0x000315D7 File Offset: 0x0002F7D7
		protected override void Dispose(bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x000315F8 File Offset: 0x0002F7F8
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(82, 78);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(313, 178);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(82, 270);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(313, 20);
			this.label1.TabIndex = 1;
			this.label1.Text = "About";
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(477, 336);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pictureBox1);
			this.Name = "FormAbout";
			this.Text = "About";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		// Token: 0x04000250 RID: 592
		private System.ComponentModel.IContainer components;

		// Token: 0x04000251 RID: 593
		private System.Windows.Forms.PictureBox pictureBox1;

		// Token: 0x04000252 RID: 594
		private System.Windows.Forms.Label label1;
	}
}

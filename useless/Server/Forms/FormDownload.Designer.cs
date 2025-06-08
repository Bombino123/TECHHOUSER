namespace Server.Forms
{
	// Token: 0x020000BE RID: 190
	public partial class FormDownload : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600062C RID: 1580 RVA: 0x0005A029 File Offset: 0x00058229
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x0005A048 File Offset: 0x00058248
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.materialProgressBar1 = new global::MaterialSkin.Controls.MaterialProgressBar();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.materialLabel2 = new global::MaterialSkin.Controls.MaterialLabel();
			this.materialLabel3 = new global::MaterialSkin.Controls.MaterialLabel();
			this.materialLabel4 = new global::MaterialSkin.Controls.MaterialLabel();
			this.timer2 = new global::System.Windows.Forms.Timer(this.components);
			this.materialLabel5 = new global::MaterialSkin.Controls.MaterialLabel();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			base.SuspendLayout();
			this.materialProgressBar1.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.materialProgressBar1.Depth = 0;
			this.materialProgressBar1.Location = new global::System.Drawing.Point(6, 121);
			this.materialProgressBar1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialProgressBar1.Name = "materialProgressBar1";
			this.materialProgressBar1.Size = new global::System.Drawing.Size(323, 5);
			this.materialProgressBar1.TabIndex = 0;
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.Depth = 0;
			this.materialLabel1.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel1.Location = new global::System.Drawing.Point(6, 129);
			this.materialLabel1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new global::System.Drawing.Size(61, 19);
			this.materialLabel1.TabIndex = 1;
			this.materialLabel1.Text = "0% done";
			this.materialLabel2.AutoSize = true;
			this.materialLabel2.Depth = 0;
			this.materialLabel2.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel2.Location = new global::System.Drawing.Point(243, 129);
			this.materialLabel2.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel2.Name = "materialLabel2";
			this.materialLabel2.Size = new global::System.Drawing.Size(50, 19);
			this.materialLabel2.TabIndex = 2;
			this.materialLabel2.Text = "0b/sec";
			this.materialLabel3.AutoSize = true;
			this.materialLabel3.Depth = 0;
			this.materialLabel3.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel3.Location = new global::System.Drawing.Point(6, 95);
			this.materialLabel3.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel3.Name = "materialLabel3";
			this.materialLabel3.Size = new global::System.Drawing.Size(91, 19);
			this.materialLabel3.TabIndex = 3;
			this.materialLabel3.Text = "File: none.txt";
			this.materialLabel4.AutoSize = true;
			this.materialLabel4.Depth = 0;
			this.materialLabel4.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel4.Location = new global::System.Drawing.Point(6, 73);
			this.materialLabel4.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel4.Name = "materialLabel4";
			this.materialLabel4.Size = new global::System.Drawing.Size(71, 19);
			this.materialLabel4.TabIndex = 4;
			this.materialLabel4.Text = "Size: 0mb";
			this.timer2.Interval = 1000;
			this.timer2.Tick += new global::System.EventHandler(this.timer2_Tick);
			this.materialLabel5.AutoSize = true;
			this.materialLabel5.Depth = 0;
			this.materialLabel5.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel5.Location = new global::System.Drawing.Point(119, 73);
			this.materialLabel5.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel5.Name = "materialLabel5";
			this.materialLabel5.Size = new global::System.Drawing.Size(121, 19);
			this.materialLabel5.TabIndex = 5;
			this.materialLabel5.Text = "Size Recive: 0mb";
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(335, 162);
			base.Controls.Add(this.materialLabel5);
			base.Controls.Add(this.materialLabel4);
			base.Controls.Add(this.materialLabel3);
			base.Controls.Add(this.materialLabel2);
			base.Controls.Add(this.materialLabel1);
			base.Controls.Add(this.materialProgressBar1);
			this.MaximumSize = new global::System.Drawing.Size(335, 162);
			this.MinimumSize = new global::System.Drawing.Size(335, 162);
			base.Name = "FormDownload";
			this.Text = "Download";
			base.Load += new global::System.EventHandler(this.FormUpload_Load);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000500 RID: 1280
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000501 RID: 1281
		private global::MaterialSkin.Controls.MaterialProgressBar materialProgressBar1;

		// Token: 0x04000502 RID: 1282
		private global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x04000503 RID: 1283
		private global::MaterialSkin.Controls.MaterialLabel materialLabel2;

		// Token: 0x04000504 RID: 1284
		private global::MaterialSkin.Controls.MaterialLabel materialLabel3;

		// Token: 0x04000505 RID: 1285
		private global::MaterialSkin.Controls.MaterialLabel materialLabel4;

		// Token: 0x04000506 RID: 1286
		public global::System.Windows.Forms.Timer timer2;

		// Token: 0x04000507 RID: 1287
		private global::MaterialSkin.Controls.MaterialLabel materialLabel5;

		// Token: 0x04000508 RID: 1288
		public global::System.Windows.Forms.Timer timer1;
	}
}

namespace Server.Forms
{
	// Token: 0x020000A9 RID: 169
	public partial class FormDesktop : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600051B RID: 1307 RVA: 0x00047673 File Offset: 0x00045873
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x00047694 File Offset: 0x00045894
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.materialSwitch1 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.materialSwitch2 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.materialSwitch3 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.materialSwitch4 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.pictureBox1 = new global::System.Windows.Forms.PictureBox();
			this.numericUpDown2 = new global::System.Windows.Forms.NumericUpDown();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			((global::System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
			base.SuspendLayout();
			this.materialSwitch1.AutoSize = true;
			this.materialSwitch1.Depth = 0;
			this.materialSwitch1.Enabled = false;
			this.materialSwitch1.Location = new global::System.Drawing.Point(3, 64);
			this.materialSwitch1.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch1.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch1.Name = "materialSwitch1";
			this.materialSwitch1.Ripple = true;
			this.materialSwitch1.Size = new global::System.Drawing.Size(92, 37);
			this.materialSwitch1.TabIndex = 0;
			this.materialSwitch1.Text = "Start";
			this.materialSwitch1.UseVisualStyleBackColor = true;
			this.materialSwitch1.CheckedChanged += new global::System.EventHandler(this.materialSwitch1_CheckedChanged);
			this.materialSwitch2.AutoSize = true;
			this.materialSwitch2.Depth = 0;
			this.materialSwitch2.Enabled = false;
			this.materialSwitch2.Location = new global::System.Drawing.Point(234, 64);
			this.materialSwitch2.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch2.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch2.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch2.Name = "materialSwitch2";
			this.materialSwitch2.Ripple = true;
			this.materialSwitch2.Size = new global::System.Drawing.Size(106, 37);
			this.materialSwitch2.TabIndex = 1;
			this.materialSwitch2.Text = "Mouse";
			this.materialSwitch2.UseVisualStyleBackColor = true;
			this.materialSwitch3.AutoSize = true;
			this.materialSwitch3.Depth = 0;
			this.materialSwitch3.Enabled = false;
			this.materialSwitch3.Location = new global::System.Drawing.Point(352, 64);
			this.materialSwitch3.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch3.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch3.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch3.Name = "materialSwitch3";
			this.materialSwitch3.Ripple = true;
			this.materialSwitch3.Size = new global::System.Drawing.Size(125, 37);
			this.materialSwitch3.TabIndex = 2;
			this.materialSwitch3.Text = "Keyboard";
			this.materialSwitch3.UseVisualStyleBackColor = true;
			this.materialSwitch4.AutoSize = true;
			this.materialSwitch4.Depth = 0;
			this.materialSwitch4.Enabled = false;
			this.materialSwitch4.Location = new global::System.Drawing.Point(491, 64);
			this.materialSwitch4.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch4.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch4.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch4.Name = "materialSwitch4";
			this.materialSwitch4.Ripple = true;
			this.materialSwitch4.Size = new global::System.Drawing.Size(109, 37);
			this.materialSwitch4.TabIndex = 3;
			this.materialSwitch4.Text = "DirectX";
			this.materialSwitch4.UseVisualStyleBackColor = true;
			this.materialSwitch4.CheckedChanged += new global::System.EventHandler(this.materialSwitch4_CheckedChanged);
			this.pictureBox1.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.pictureBox1.Location = new global::System.Drawing.Point(6, 104);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new global::System.Drawing.Size(813, 427);
			this.pictureBox1.SizeMode = global::System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			this.numericUpDown2.BackColor = global::System.Drawing.Color.White;
			this.numericUpDown2.BorderStyle = global::System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDown2.Enabled = false;
			this.numericUpDown2.ForeColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			global::System.Windows.Forms.NumericUpDown numericUpDown = this.numericUpDown2;
			int[] array = new int[4];
			array[0] = 10;
			numericUpDown.Increment = new decimal(array);
			this.numericUpDown2.Location = new global::System.Drawing.Point(112, 73);
			this.numericUpDown2.Margin = new global::System.Windows.Forms.Padding(2);
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new global::System.Drawing.Size(42, 20);
			this.numericUpDown2.TabIndex = 19;
			this.numericUpDown2.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Center;
			this.numericUpDown2.UpDownAlign = global::System.Windows.Forms.LeftRightAlignment.Left;
			global::System.Windows.Forms.NumericUpDown numericUpDown2 = this.numericUpDown2;
			int[] array2 = new int[4];
			array2[0] = 80;
			numericUpDown2.Value = new decimal(array2);
			this.numericUpDown2.ValueChanged += new global::System.EventHandler(this.numericUpDown2_ValueChanged);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(819, 470);
			this.panel1.TabIndex = 20;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(825, 537);
			base.Controls.Add(this.numericUpDown2);
			base.Controls.Add(this.pictureBox1);
			base.Controls.Add(this.materialSwitch4);
			base.Controls.Add(this.materialSwitch3);
			base.Controls.Add(this.materialSwitch2);
			base.Controls.Add(this.materialSwitch1);
			base.Controls.Add(this.panel1);
			base.DrawerUseColors = true;
			base.Name = "FormDesktop";
			this.Text = "Desktop";
			base.Load += new global::System.EventHandler(this.FormDesktop_Load);
			((global::System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x040003B1 RID: 945
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040003B2 RID: 946
		public global::System.Windows.Forms.NumericUpDown numericUpDown2;

		// Token: 0x040003B3 RID: 947
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch1;

		// Token: 0x040003B4 RID: 948
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch2;

		// Token: 0x040003B5 RID: 949
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch3;

		// Token: 0x040003B6 RID: 950
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch4;

		// Token: 0x040003B7 RID: 951
		public global::System.Windows.Forms.PictureBox pictureBox1;

		// Token: 0x040003B8 RID: 952
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x040003B9 RID: 953
		public global::System.Windows.Forms.Timer timer1;
	}
}

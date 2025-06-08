namespace Server.Forms
{
	// Token: 0x02000093 RID: 147
	public partial class FormCamera : global::Server.Helper.FormMaterial
	{
		// Token: 0x060003FB RID: 1019 RVA: 0x00030CB3 File Offset: 0x0002EEB3
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00030CD4 File Offset: 0x0002EED4
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.numericUpDown2 = new global::System.Windows.Forms.NumericUpDown();
			this.pictureBox1 = new global::System.Windows.Forms.PictureBox();
			this.materialSwitch1 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.rjComboBox1 = new global::CustomControls.RJControls.RJComboBox();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
			base.SuspendLayout();
			this.numericUpDown2.BackColor = global::System.Drawing.Color.White;
			this.numericUpDown2.BorderStyle = global::System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDown2.Enabled = false;
			this.numericUpDown2.ForeColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			global::System.Windows.Forms.NumericUpDown numericUpDown = this.numericUpDown2;
			int[] array = new int[4];
			array[0] = 10;
			numericUpDown.Increment = new decimal(array);
			this.numericUpDown2.Location = new global::System.Drawing.Point(112, 78);
			this.numericUpDown2.Margin = new global::System.Windows.Forms.Padding(2);
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new global::System.Drawing.Size(42, 20);
			this.numericUpDown2.TabIndex = 22;
			this.numericUpDown2.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Center;
			this.numericUpDown2.UpDownAlign = global::System.Windows.Forms.LeftRightAlignment.Left;
			global::System.Windows.Forms.NumericUpDown numericUpDown2 = this.numericUpDown2;
			int[] array2 = new int[4];
			array2[0] = 80;
			numericUpDown2.Value = new decimal(array2);
			this.numericUpDown2.ValueChanged += new global::System.EventHandler(this.numericUpDown2_ValueChanged);
			this.pictureBox1.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.pictureBox1.BackColor = global::System.Drawing.Color.White;
			this.pictureBox1.Location = new global::System.Drawing.Point(6, 109);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new global::System.Drawing.Size(813, 422);
			this.pictureBox1.SizeMode = global::System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 21;
			this.pictureBox1.TabStop = false;
			this.materialSwitch1.AutoSize = true;
			this.materialSwitch1.BackColor = global::System.Drawing.Color.White;
			this.materialSwitch1.Depth = 0;
			this.materialSwitch1.Enabled = false;
			this.materialSwitch1.Location = new global::System.Drawing.Point(3, 69);
			this.materialSwitch1.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch1.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch1.Name = "materialSwitch1";
			this.materialSwitch1.Ripple = true;
			this.materialSwitch1.Size = new global::System.Drawing.Size(92, 37);
			this.materialSwitch1.TabIndex = 20;
			this.materialSwitch1.Text = "Start";
			this.materialSwitch1.UseVisualStyleBackColor = false;
			this.materialSwitch1.CheckedChanged += new global::System.EventHandler(this.materialSwitch1_CheckedChanged);
			this.rjComboBox1.BackColor = global::System.Drawing.Color.White;
			this.rjComboBox1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjComboBox1.BorderSize = 1;
			this.rjComboBox1.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDown;
			this.rjComboBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 10f);
			this.rjComboBox1.ForeColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox1.IconColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjComboBox1.ListBackColor = global::System.Drawing.Color.White;
			this.rjComboBox1.ListTextColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox1.Location = new global::System.Drawing.Point(180, 73);
			this.rjComboBox1.MinimumSize = new global::System.Drawing.Size(200, 30);
			this.rjComboBox1.Name = "rjComboBox1";
			this.rjComboBox1.Padding = new global::System.Windows.Forms.Padding(1);
			this.rjComboBox1.Size = new global::System.Drawing.Size(250, 30);
			this.rjComboBox1.TabIndex = 23;
			this.rjComboBox1.Texts = "";
			this.panel1.BackColor = global::System.Drawing.Color.White;
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(819, 470);
			this.panel1.TabIndex = 24;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(825, 537);
			base.Controls.Add(this.rjComboBox1);
			base.Controls.Add(this.numericUpDown2);
			base.Controls.Add(this.pictureBox1);
			base.Controls.Add(this.materialSwitch1);
			base.Controls.Add(this.panel1);
			base.Name = "FormCamera";
			this.Text = "Camera";
			base.Load += new global::System.EventHandler(this.FormCamera_Load);
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000246 RID: 582
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000247 RID: 583
		public global::System.Windows.Forms.NumericUpDown numericUpDown2;

		// Token: 0x04000248 RID: 584
		public global::System.Windows.Forms.PictureBox pictureBox1;

		// Token: 0x04000249 RID: 585
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch1;

		// Token: 0x0400024A RID: 586
		public global::CustomControls.RJControls.RJComboBox rjComboBox1;

		// Token: 0x0400024B RID: 587
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x0400024C RID: 588
		public global::System.Windows.Forms.Timer timer1;
	}
}

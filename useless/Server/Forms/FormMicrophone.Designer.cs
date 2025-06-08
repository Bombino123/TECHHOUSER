namespace Server.Forms
{
	// Token: 0x020000AE RID: 174
	public partial class FormMicrophone : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000568 RID: 1384 RVA: 0x0004DC6D File Offset: 0x0004BE6D
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x0004DC8C File Offset: 0x0004BE8C
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.groupBox1 = new global::System.Windows.Forms.GroupBox();
			this.trackBar1 = new global::System.Windows.Forms.TrackBar();
			this.rjComboBox1 = new global::CustomControls.RJControls.RJComboBox();
			this.materialSwitch1 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.groupBox2 = new global::System.Windows.Forms.GroupBox();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.trackBar2 = new global::System.Windows.Forms.TrackBar();
			this.rjComboBox2 = new global::CustomControls.RJControls.RJComboBox();
			this.materialSwitch2 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.groupBox1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
			this.groupBox2.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar2).BeginInit();
			base.SuspendLayout();
			this.timer1.Enabled = true;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.groupBox1.Controls.Add(this.trackBar1);
			this.groupBox1.Controls.Add(this.rjComboBox1);
			this.groupBox1.Controls.Add(this.materialSwitch1);
			this.groupBox1.Enabled = false;
			this.groupBox1.Location = new global::System.Drawing.Point(26, 88);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new global::System.Drawing.Size(300, 149);
			this.groupBox1.TabIndex = 29;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Client";
			this.trackBar1.Location = new global::System.Drawing.Point(25, 55);
			this.trackBar1.Maximum = 100;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new global::System.Drawing.Size(250, 45);
			this.trackBar1.TabIndex = 28;
			this.trackBar1.Value = 10;
			this.trackBar1.Scroll += new global::System.EventHandler(this.trackBar1_Scroll);
			this.rjComboBox1.BackColor = global::System.Drawing.Color.White;
			this.rjComboBox1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjComboBox1.BorderSize = 1;
			this.rjComboBox1.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDown;
			this.rjComboBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 10f);
			this.rjComboBox1.ForeColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox1.IconColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjComboBox1.ListBackColor = global::System.Drawing.Color.White;
			this.rjComboBox1.ListTextColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox1.Location = new global::System.Drawing.Point(25, 19);
			this.rjComboBox1.MinimumSize = new global::System.Drawing.Size(200, 30);
			this.rjComboBox1.Name = "rjComboBox1";
			this.rjComboBox1.Padding = new global::System.Windows.Forms.Padding(1);
			this.rjComboBox1.Size = new global::System.Drawing.Size(250, 30);
			this.rjComboBox1.TabIndex = 27;
			this.rjComboBox1.Texts = "";
			this.materialSwitch1.AutoSize = true;
			this.materialSwitch1.Depth = 0;
			this.materialSwitch1.Location = new global::System.Drawing.Point(6, 103);
			this.materialSwitch1.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch1.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch1.Name = "materialSwitch1";
			this.materialSwitch1.Ripple = true;
			this.materialSwitch1.Size = new global::System.Drawing.Size(92, 37);
			this.materialSwitch1.TabIndex = 26;
			this.materialSwitch1.Text = "Start";
			this.materialSwitch1.UseVisualStyleBackColor = true;
			this.materialSwitch1.CheckedChanged += new global::System.EventHandler(this.materialSwitch1_CheckedChanged);
			this.groupBox2.Controls.Add(this.materialLabel1);
			this.groupBox2.Controls.Add(this.trackBar2);
			this.groupBox2.Controls.Add(this.rjComboBox2);
			this.groupBox2.Controls.Add(this.materialSwitch2);
			this.groupBox2.Enabled = false;
			this.groupBox2.Location = new global::System.Drawing.Point(348, 88);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new global::System.Drawing.Size(300, 149);
			this.groupBox2.TabIndex = 30;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "You";
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.Depth = 0;
			this.materialLabel1.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel1.Location = new global::System.Drawing.Point(6, 64);
			this.materialLabel1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new global::System.Drawing.Size(85, 19);
			this.materialLabel1.TabIndex = 29;
			this.materialLabel1.Text = "Tone Speak";
			this.trackBar2.Location = new global::System.Drawing.Point(97, 55);
			this.trackBar2.Maximum = 21;
			this.trackBar2.Minimum = 1;
			this.trackBar2.Name = "trackBar2";
			this.trackBar2.Size = new global::System.Drawing.Size(197, 45);
			this.trackBar2.TabIndex = 28;
			this.trackBar2.Value = 10;
			this.trackBar2.Scroll += new global::System.EventHandler(this.trackBar2_Scroll);
			this.rjComboBox2.BackColor = global::System.Drawing.Color.White;
			this.rjComboBox2.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjComboBox2.BorderSize = 1;
			this.rjComboBox2.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDown;
			this.rjComboBox2.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 10f);
			this.rjComboBox2.ForeColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox2.IconColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjComboBox2.ListBackColor = global::System.Drawing.Color.White;
			this.rjComboBox2.ListTextColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox2.Location = new global::System.Drawing.Point(25, 19);
			this.rjComboBox2.MinimumSize = new global::System.Drawing.Size(200, 30);
			this.rjComboBox2.Name = "rjComboBox2";
			this.rjComboBox2.Padding = new global::System.Windows.Forms.Padding(1);
			this.rjComboBox2.Size = new global::System.Drawing.Size(250, 30);
			this.rjComboBox2.TabIndex = 27;
			this.rjComboBox2.Texts = "";
			this.materialSwitch2.AutoSize = true;
			this.materialSwitch2.Depth = 0;
			this.materialSwitch2.Location = new global::System.Drawing.Point(6, 103);
			this.materialSwitch2.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch2.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch2.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch2.Name = "materialSwitch2";
			this.materialSwitch2.Ripple = true;
			this.materialSwitch2.Size = new global::System.Drawing.Size(92, 37);
			this.materialSwitch2.TabIndex = 26;
			this.materialSwitch2.Text = "Start";
			this.materialSwitch2.UseVisualStyleBackColor = true;
			this.materialSwitch2.CheckedChanged += new global::System.EventHandler(this.materialSwitch2_CheckedChanged);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(667, 198);
			this.panel1.TabIndex = 31;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(673, 265);
			base.Controls.Add(this.groupBox2);
			base.Controls.Add(this.groupBox1);
			base.Controls.Add(this.panel1);
			base.Name = "FormMicrophone";
			this.Text = "Microphone";
			base.Load += new global::System.EventHandler(this.FormMicrophone_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar2).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x04000412 RID: 1042
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000413 RID: 1043
		public global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000414 RID: 1044
		private global::System.Windows.Forms.TrackBar trackBar1;

		// Token: 0x04000415 RID: 1045
		public global::CustomControls.RJControls.RJComboBox rjComboBox1;

		// Token: 0x04000416 RID: 1046
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch1;

		// Token: 0x04000417 RID: 1047
		private global::System.Windows.Forms.TrackBar trackBar2;

		// Token: 0x04000418 RID: 1048
		public global::CustomControls.RJControls.RJComboBox rjComboBox2;

		// Token: 0x04000419 RID: 1049
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch2;

		// Token: 0x0400041A RID: 1050
		private global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x0400041B RID: 1051
		public global::System.Windows.Forms.GroupBox groupBox1;

		// Token: 0x0400041C RID: 1052
		public global::System.Windows.Forms.GroupBox groupBox2;

		// Token: 0x0400041D RID: 1053
		private global::System.Windows.Forms.Panel panel1;
	}
}

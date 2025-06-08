namespace Server.Forms
{
	// Token: 0x020000BC RID: 188
	public partial class FormSettings : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600061A RID: 1562 RVA: 0x000589BD File Offset: 0x00056BBD
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x000589DC File Offset: 0x00056BDC
		private void InitializeComponent()
		{
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.groupBox1 = new global::System.Windows.Forms.GroupBox();
			this.checkBox2 = new global::System.Windows.Forms.CheckBox();
			this.materialLabel3 = new global::MaterialSkin.Controls.MaterialLabel();
			this.numericUpDown1 = new global::System.Windows.Forms.NumericUpDown();
			this.materialLabel1 = new global::System.Windows.Forms.Label();
			this.materialLabel2 = new global::MaterialSkin.Controls.MaterialLabel();
			this.groupBox2 = new global::System.Windows.Forms.GroupBox();
			this.materialSwitch2 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.materialSwitch1 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.rjTextBox2 = new global::CustomControls.RJControls.RJTextBox();
			this.groupBox3 = new global::System.Windows.Forms.GroupBox();
			this.rjTextBox3 = new global::CustomControls.RJControls.RJTextBox();
			this.groupBox4 = new global::System.Windows.Forms.GroupBox();
			this.materialLabel4 = new global::MaterialSkin.Controls.MaterialLabel();
			this.rjComboBox1 = new global::CustomControls.RJControls.RJComboBox();
			this.groupBox1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown1).BeginInit();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			base.SuspendLayout();
			this.rjTextBox1.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.DarkViolet;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 2;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox1.Location = new global::System.Drawing.Point(16, 20);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "Ports";
			this.rjTextBox1.Size = new global::System.Drawing.Size(250, 31);
			this.rjTextBox1.TabIndex = 0;
			this.rjTextBox1.Texts = "";
			this.rjTextBox1.UnderlinedStyle = false;
			this.rjButton1.BackColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.BorderColor = global::System.Drawing.Color.DarkViolet;
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 0;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.ForeColor = global::System.Drawing.Color.White;
			this.rjButton1.Location = new global::System.Drawing.Point(16, 58);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(103, 29);
			this.rjButton1.TabIndex = 1;
			this.rjButton1.Text = "Start";
			this.rjButton1.TextColor = global::System.Drawing.Color.White;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			this.groupBox1.Controls.Add(this.checkBox2);
			this.groupBox1.Controls.Add(this.materialLabel3);
			this.groupBox1.Controls.Add(this.numericUpDown1);
			this.groupBox1.Controls.Add(this.materialLabel1);
			this.groupBox1.Controls.Add(this.materialLabel2);
			this.groupBox1.Controls.Add(this.rjTextBox1);
			this.groupBox1.Controls.Add(this.rjButton1);
			this.groupBox1.Location = new global::System.Drawing.Point(15, 67);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new global::System.Drawing.Size(407, 200);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Server";
			this.checkBox2.AutoSize = true;
			this.checkBox2.Location = new global::System.Drawing.Point(315, 166);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new global::System.Drawing.Size(84, 17);
			this.checkBox2.TabIndex = 7;
			this.checkBox2.Text = "Auto Stealer";
			this.checkBox2.UseVisualStyleBackColor = true;
			this.materialLabel3.AutoSize = true;
			this.materialLabel3.Depth = 0;
			this.materialLabel3.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel3.Location = new global::System.Drawing.Point(193, 68);
			this.materialLabel3.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel3.Name = "materialLabel3";
			this.materialLabel3.Size = new global::System.Drawing.Size(116, 19);
			this.materialLabel3.TabIndex = 6;
			this.materialLabel3.Text = "Ping Disconnect";
			this.numericUpDown1.Location = new global::System.Drawing.Point(315, 67);
			global::System.Windows.Forms.NumericUpDown numericUpDown = this.numericUpDown1;
			int[] array = new int[4];
			array[0] = 120;
			numericUpDown.Maximum = new decimal(array);
			global::System.Windows.Forms.NumericUpDown numericUpDown2 = this.numericUpDown1;
			int[] array2 = new int[4];
			array2[0] = 15;
			numericUpDown2.Minimum = new decimal(array2);
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new global::System.Drawing.Size(62, 20);
			this.numericUpDown1.TabIndex = 5;
			global::System.Windows.Forms.NumericUpDown numericUpDown3 = this.numericUpDown1;
			int[] array3 = new int[4];
			array3[0] = 35;
			numericUpDown3.Value = new decimal(array3);
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.Font = new global::System.Drawing.Font("Cambria", 14.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.materialLabel1.Location = new global::System.Drawing.Point(12, 102);
			this.materialLabel1.MaximumSize = new global::System.Drawing.Size(270, 0);
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new global::System.Drawing.Size(134, 22);
			this.materialLabel1.TabIndex = 4;
			this.materialLabel1.Text = "Status: [offline]";
			this.materialLabel2.AutoSize = true;
			this.materialLabel2.Depth = 0;
			this.materialLabel2.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel2.Location = new global::System.Drawing.Point(13, 166);
			this.materialLabel2.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel2.Name = "materialLabel2";
			this.materialLabel2.Size = new global::System.Drawing.Size(160, 19);
			this.materialLabel2.TabIndex = 3;
			this.materialLabel2.Text = "Certificate: [Not Exists]";
			this.groupBox2.Controls.Add(this.materialSwitch2);
			this.groupBox2.Controls.Add(this.materialSwitch1);
			this.groupBox2.Controls.Add(this.rjTextBox2);
			this.groupBox2.Location = new global::System.Drawing.Point(15, 282);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new global::System.Drawing.Size(407, 109);
			this.groupBox2.TabIndex = 7;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Discord Notificator";
			this.materialSwitch2.AutoSize = true;
			this.materialSwitch2.Depth = 0;
			this.materialSwitch2.Location = new global::System.Drawing.Point(190, 59);
			this.materialSwitch2.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch2.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch2.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch2.Name = "materialSwitch2";
			this.materialSwitch2.Ripple = true;
			this.materialSwitch2.Size = new global::System.Drawing.Size(116, 37);
			this.materialSwitch2.TabIndex = 63;
			this.materialSwitch2.Text = "Connect";
			this.materialSwitch2.UseVisualStyleBackColor = true;
			this.materialSwitch1.AutoSize = true;
			this.materialSwitch1.Depth = 0;
			this.materialSwitch1.Location = new global::System.Drawing.Point(16, 59);
			this.materialSwitch1.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch1.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch1.Name = "materialSwitch1";
			this.materialSwitch1.Ripple = true;
			this.materialSwitch1.Size = new global::System.Drawing.Size(151, 37);
			this.materialSwitch1.TabIndex = 62;
			this.materialSwitch1.Text = "New Connect";
			this.materialSwitch1.UseVisualStyleBackColor = true;
			this.rjTextBox2.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox2.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox2.BorderFocusColor = global::System.Drawing.Color.DarkViolet;
			this.rjTextBox2.BorderRadius = 0;
			this.rjTextBox2.BorderSize = 2;
			this.rjTextBox2.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox2.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox2.Location = new global::System.Drawing.Point(16, 20);
			this.rjTextBox2.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox2.Multiline = false;
			this.rjTextBox2.Name = "rjTextBox2";
			this.rjTextBox2.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox2.PasswordChar = false;
			this.rjTextBox2.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox2.PlaceholderText = "Webhook";
			this.rjTextBox2.Size = new global::System.Drawing.Size(384, 31);
			this.rjTextBox2.TabIndex = 0;
			this.rjTextBox2.Texts = "";
			this.rjTextBox2.UnderlinedStyle = false;
			this.groupBox3.Controls.Add(this.rjTextBox3);
			this.groupBox3.Location = new global::System.Drawing.Point(15, 397);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new global::System.Drawing.Size(407, 65);
			this.groupBox3.TabIndex = 64;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Miner Download";
			this.rjTextBox3.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox3.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox3.BorderFocusColor = global::System.Drawing.Color.DarkViolet;
			this.rjTextBox3.BorderRadius = 0;
			this.rjTextBox3.BorderSize = 2;
			this.rjTextBox3.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox3.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox3.Location = new global::System.Drawing.Point(16, 20);
			this.rjTextBox3.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox3.Multiline = false;
			this.rjTextBox3.Name = "rjTextBox3";
			this.rjTextBox3.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox3.PasswordChar = false;
			this.rjTextBox3.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox3.PlaceholderText = "url";
			this.rjTextBox3.Size = new global::System.Drawing.Size(384, 31);
			this.rjTextBox3.TabIndex = 0;
			this.rjTextBox3.Texts = "";
			this.rjTextBox3.UnderlinedStyle = false;
			this.groupBox4.Controls.Add(this.materialLabel4);
			this.groupBox4.Controls.Add(this.rjComboBox1);
			this.groupBox4.Location = new global::System.Drawing.Point(428, 67);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new global::System.Drawing.Size(407, 200);
			this.groupBox4.TabIndex = 65;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Panel";
			this.materialLabel4.AutoSize = true;
			this.materialLabel4.Depth = 0;
			this.materialLabel4.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel4.Location = new global::System.Drawing.Point(16, 20);
			this.materialLabel4.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel4.Name = "materialLabel4";
			this.materialLabel4.Size = new global::System.Drawing.Size(36, 19);
			this.materialLabel4.TabIndex = 7;
			this.materialLabel4.Text = "Style";
			this.rjComboBox1.BackColor = global::System.Drawing.Color.WhiteSmoke;
			this.rjComboBox1.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjComboBox1.BorderSize = 1;
			this.rjComboBox1.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDown;
			this.rjComboBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 10f);
			this.rjComboBox1.ForeColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox1.IconColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjComboBox1.Items.AddRange(new object[]
			{
				"Purple600",
				"Blue600",
				"Red600",
				"Green600",
				"Orange600",
				"Yellow600",
				"DeepPurple600",
				"Teal600",
				"Cyan600",
				"LightBlue600",
				"Lime600",
				"Indigo600",
				"DeepOrange600",
				"Amber600",
				"Pink600",
				"LightGreen600"
			});
			this.rjComboBox1.ListBackColor = global::System.Drawing.Color.FromArgb(230, 228, 245);
			this.rjComboBox1.ListTextColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox1.Location = new global::System.Drawing.Point(19, 45);
			this.rjComboBox1.MinimumSize = new global::System.Drawing.Size(200, 30);
			this.rjComboBox1.Name = "rjComboBox1";
			this.rjComboBox1.Padding = new global::System.Windows.Forms.Padding(1);
			this.rjComboBox1.Size = new global::System.Drawing.Size(212, 30);
			this.rjComboBox1.TabIndex = 8;
			this.rjComboBox1.Texts = "";
			this.rjComboBox1.OnSelectedIndexChanged += new global::System.EventHandler(this.rjComboBox1_OnSelectedIndexChanged);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(867, 553);
			base.Controls.Add(this.groupBox4);
			base.Controls.Add(this.groupBox3);
			base.Controls.Add(this.groupBox2);
			base.Controls.Add(this.groupBox1);
			base.Name = "FormSettings";
			this.Text = "Settings";
			base.Load += new global::System.EventHandler(this.FormSettings_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown1).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			base.ResumeLayout(false);
		}

		// Token: 0x040004DE RID: 1246
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040004DF RID: 1247
		private global::CustomControls.RJControls.RJTextBox rjTextBox1;

		// Token: 0x040004E0 RID: 1248
		private global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x040004E1 RID: 1249
		private global::System.Windows.Forms.GroupBox groupBox1;

		// Token: 0x040004E2 RID: 1250
		private global::MaterialSkin.Controls.MaterialLabel materialLabel2;

		// Token: 0x040004E3 RID: 1251
		private global::System.Windows.Forms.Label materialLabel1;

		// Token: 0x040004E4 RID: 1252
		private global::MaterialSkin.Controls.MaterialLabel materialLabel3;

		// Token: 0x040004E5 RID: 1253
		private global::System.Windows.Forms.NumericUpDown numericUpDown1;

		// Token: 0x040004E6 RID: 1254
		private global::System.Windows.Forms.GroupBox groupBox2;

		// Token: 0x040004E7 RID: 1255
		private global::CustomControls.RJControls.RJTextBox rjTextBox2;

		// Token: 0x040004E8 RID: 1256
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch1;

		// Token: 0x040004E9 RID: 1257
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch2;

		// Token: 0x040004EA RID: 1258
		private global::System.Windows.Forms.GroupBox groupBox3;

		// Token: 0x040004EB RID: 1259
		private global::CustomControls.RJControls.RJTextBox rjTextBox3;

		// Token: 0x040004EC RID: 1260
		private global::System.Windows.Forms.GroupBox groupBox4;

		// Token: 0x040004ED RID: 1261
		private global::MaterialSkin.Controls.MaterialLabel materialLabel4;

		// Token: 0x040004EE RID: 1262
		private global::CustomControls.RJControls.RJComboBox rjComboBox1;

		// Token: 0x040004EF RID: 1263
		private global::System.Windows.Forms.CheckBox checkBox2;
	}
}

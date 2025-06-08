namespace Server.Forms
{
	// Token: 0x020000B1 RID: 177
	public partial class FormDDos : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000585 RID: 1413 RVA: 0x0004F7E8 File Offset: 0x0004D9E8
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x0004F808 File Offset: 0x0004DA08
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.panel2 = new global::System.Windows.Forms.Panel();
			this.checkBox14 = new global::System.Windows.Forms.CheckBox();
			this.checkBox13 = new global::System.Windows.Forms.CheckBox();
			this.checkBox12 = new global::System.Windows.Forms.CheckBox();
			this.numericUpDown2 = new global::System.Windows.Forms.NumericUpDown();
			this.label3 = new global::System.Windows.Forms.Label();
			this.label2 = new global::System.Windows.Forms.Label();
			this.label1 = new global::System.Windows.Forms.Label();
			this.checkBox11 = new global::System.Windows.Forms.CheckBox();
			this.checkBox10 = new global::System.Windows.Forms.CheckBox();
			this.checkBox9 = new global::System.Windows.Forms.CheckBox();
			this.checkBox8 = new global::System.Windows.Forms.CheckBox();
			this.checkBox7 = new global::System.Windows.Forms.CheckBox();
			this.checkBox6 = new global::System.Windows.Forms.CheckBox();
			this.checkBox5 = new global::System.Windows.Forms.CheckBox();
			this.checkBox3 = new global::System.Windows.Forms.CheckBox();
			this.checkBox1 = new global::System.Windows.Forms.CheckBox();
			this.checkBox2 = new global::System.Windows.Forms.CheckBox();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.materialSwitch7 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Controls.Add(this.rjButton1);
			this.panel1.Controls.Add(this.materialSwitch7);
			this.panel1.Controls.Add(this.rjTextBox1);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(459, 255);
			this.panel1.TabIndex = 0;
			this.panel2.Controls.Add(this.checkBox14);
			this.panel2.Controls.Add(this.checkBox13);
			this.panel2.Controls.Add(this.checkBox12);
			this.panel2.Controls.Add(this.numericUpDown2);
			this.panel2.Controls.Add(this.label3);
			this.panel2.Controls.Add(this.label2);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Controls.Add(this.checkBox11);
			this.panel2.Controls.Add(this.checkBox10);
			this.panel2.Controls.Add(this.checkBox9);
			this.panel2.Controls.Add(this.checkBox8);
			this.panel2.Controls.Add(this.checkBox7);
			this.panel2.Controls.Add(this.checkBox6);
			this.panel2.Controls.Add(this.checkBox5);
			this.panel2.Controls.Add(this.checkBox3);
			this.panel2.Controls.Add(this.checkBox1);
			this.panel2.Controls.Add(this.checkBox2);
			this.panel2.Location = new global::System.Drawing.Point(14, 50);
			this.panel2.Name = "panel2";
			this.panel2.Size = new global::System.Drawing.Size(436, 190);
			this.panel2.TabIndex = 14;
			this.checkBox14.AutoSize = true;
			this.checkBox14.Location = new global::System.Drawing.Point(330, 144);
			this.checkBox14.Name = "checkBox14";
			this.checkBox14.Size = new global::System.Drawing.Size(72, 17);
			this.checkBox14.TabIndex = 23;
			this.checkBox14.Text = "Get Flood";
			this.checkBox14.UseVisualStyleBackColor = true;
			this.checkBox13.AutoSize = true;
			this.checkBox13.Location = new global::System.Drawing.Point(330, 121);
			this.checkBox13.Name = "checkBox13";
			this.checkBox13.Size = new global::System.Drawing.Size(86, 17);
			this.checkBox13.TabIndex = 22;
			this.checkBox13.Text = "Get Scanner";
			this.checkBox13.UseVisualStyleBackColor = true;
			this.checkBox12.AutoSize = true;
			this.checkBox12.Location = new global::System.Drawing.Point(330, 98);
			this.checkBox12.Name = "checkBox12";
			this.checkBox12.Size = new global::System.Drawing.Size(90, 17);
			this.checkBox12.TabIndex = 21;
			this.checkBox12.Text = "Post Scanner";
			this.checkBox12.UseVisualStyleBackColor = true;
			this.numericUpDown2.BackColor = global::System.Drawing.Color.White;
			this.numericUpDown2.BorderStyle = global::System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDown2.ForeColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.numericUpDown2.Location = new global::System.Drawing.Point(14, 151);
			this.numericUpDown2.Margin = new global::System.Windows.Forms.Padding(2);
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new global::System.Drawing.Size(42, 20);
			this.numericUpDown2.TabIndex = 20;
			this.numericUpDown2.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Center;
			this.numericUpDown2.UpDownAlign = global::System.Windows.Forms.LeftRightAlignment.Left;
			global::System.Windows.Forms.NumericUpDown numericUpDown = this.numericUpDown2;
			int[] array = new int[4];
			array[0] = 5;
			numericUpDown.Value = new decimal(array);
			this.label3.AutoSize = true;
			this.label3.Location = new global::System.Drawing.Point(341, 10);
			this.label3.Name = "label3";
			this.label3.Size = new global::System.Drawing.Size(42, 13);
			this.label3.TabIndex = 16;
			this.label3.Text = "Layer 7";
			this.label2.AutoSize = true;
			this.label2.Location = new global::System.Drawing.Point(22, 10);
			this.label2.Name = "label2";
			this.label2.Size = new global::System.Drawing.Size(42, 13);
			this.label2.TabIndex = 15;
			this.label2.Text = "Layer 4";
			this.label1.AutoSize = true;
			this.label1.Location = new global::System.Drawing.Point(188, 10);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(42, 13);
			this.label1.TabIndex = 14;
			this.label1.Text = "Layer 3";
			this.checkBox11.AutoSize = true;
			this.checkBox11.Location = new global::System.Drawing.Point(14, 118);
			this.checkBox11.Name = "checkBox11";
			this.checkBox11.Size = new global::System.Drawing.Size(75, 17);
			this.checkBox11.TabIndex = 13;
			this.checkBox11.Text = "Udp Flood";
			this.checkBox11.UseVisualStyleBackColor = true;
			this.checkBox10.AutoSize = true;
			this.checkBox10.Location = new global::System.Drawing.Point(14, 96);
			this.checkBox10.Name = "checkBox10";
			this.checkBox10.Size = new global::System.Drawing.Size(78, 17);
			this.checkBox10.TabIndex = 12;
			this.checkBox10.Text = "Icmp Flood";
			this.checkBox10.UseVisualStyleBackColor = true;
			this.checkBox9.AutoSize = true;
			this.checkBox9.Location = new global::System.Drawing.Point(330, 75);
			this.checkBox9.Name = "checkBox9";
			this.checkBox9.Size = new global::System.Drawing.Size(103, 17);
			this.checkBox9.TabIndex = 11;
			this.checkBox9.Text = "Slow Loris Flood";
			this.checkBox9.UseVisualStyleBackColor = true;
			this.checkBox8.AutoSize = true;
			this.checkBox8.Location = new global::System.Drawing.Point(330, 52);
			this.checkBox8.Name = "checkBox8";
			this.checkBox8.Size = new global::System.Drawing.Size(73, 17);
			this.checkBox8.TabIndex = 10;
			this.checkBox8.Text = "Pps Flood";
			this.checkBox8.UseVisualStyleBackColor = true;
			this.checkBox7.AutoSize = true;
			this.checkBox7.Location = new global::System.Drawing.Point(330, 29);
			this.checkBox7.Name = "checkBox7";
			this.checkBox7.Size = new global::System.Drawing.Size(67, 17);
			this.checkBox7.TabIndex = 9;
			this.checkBox7.Text = "Null Http";
			this.checkBox7.UseVisualStyleBackColor = true;
			this.checkBox6.AutoSize = true;
			this.checkBox6.Location = new global::System.Drawing.Point(181, 52);
			this.checkBox6.Name = "checkBox6";
			this.checkBox6.Size = new global::System.Drawing.Size(77, 17);
			this.checkBox6.TabIndex = 8;
			this.checkBox6.Text = "IPv6 Flood";
			this.checkBox6.UseVisualStyleBackColor = true;
			this.checkBox5.AutoSize = true;
			this.checkBox5.Location = new global::System.Drawing.Point(181, 29);
			this.checkBox5.Name = "checkBox5";
			this.checkBox5.Size = new global::System.Drawing.Size(77, 17);
			this.checkBox5.TabIndex = 7;
			this.checkBox5.Text = "IPv4 Flood";
			this.checkBox5.UseVisualStyleBackColor = true;
			this.checkBox3.AutoSize = true;
			this.checkBox3.Location = new global::System.Drawing.Point(14, 72);
			this.checkBox3.Name = "checkBox3";
			this.checkBox3.Size = new global::System.Drawing.Size(117, 17);
			this.checkBox3.TabIndex = 5;
			this.checkBox3.Text = "Tcp Connect Flood";
			this.checkBox3.UseVisualStyleBackColor = true;
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new global::System.Drawing.Point(14, 49);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new global::System.Drawing.Size(142, 17);
			this.checkBox1.TabIndex = 4;
			this.checkBox1.Text = "Tcp Connect Wait Flood";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox2.AutoSize = true;
			this.checkBox2.Location = new global::System.Drawing.Point(14, 26);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new global::System.Drawing.Size(74, 17);
			this.checkBox2.TabIndex = 3;
			this.checkBox2.Text = "Tcp Flood";
			this.checkBox2.UseVisualStyleBackColor = true;
			this.rjButton1.BackColor = global::System.Drawing.Color.White;
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.White;
			this.rjButton1.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 1;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.Location = new global::System.Drawing.Point(263, 16);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(92, 28);
			this.rjButton1.TabIndex = 13;
			this.rjButton1.Text = "Hide";
			this.rjButton1.TextColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			this.materialSwitch7.AutoSize = true;
			this.materialSwitch7.Depth = 0;
			this.materialSwitch7.Location = new global::System.Drawing.Point(358, 13);
			this.materialSwitch7.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch7.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch7.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch7.Name = "materialSwitch7";
			this.materialSwitch7.Ripple = true;
			this.materialSwitch7.Size = new global::System.Drawing.Size(92, 37);
			this.materialSwitch7.TabIndex = 12;
			this.materialSwitch7.Text = "Start";
			this.materialSwitch7.UseVisualStyleBackColor = true;
			this.materialSwitch7.CheckedChanged += new global::System.EventHandler(this.materialSwitch7_CheckedChanged);
			this.rjTextBox1.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox1.Location = new global::System.Drawing.Point(14, 16);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "Host";
			this.rjTextBox1.Size = new global::System.Drawing.Size(242, 28);
			this.rjTextBox1.TabIndex = 11;
			this.rjTextBox1.Texts = "";
			this.rjTextBox1.UnderlinedStyle = false;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(465, 322);
			base.Controls.Add(this.panel1);
			base.Name = "FormDDos";
			this.Text = "DDos           Online [0]";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x04000434 RID: 1076
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000435 RID: 1077
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000436 RID: 1078
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x04000437 RID: 1079
		private global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x04000438 RID: 1080
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch7;

		// Token: 0x04000439 RID: 1081
		public global::CustomControls.RJControls.RJTextBox rjTextBox1;

		// Token: 0x0400043A RID: 1082
		private global::System.Windows.Forms.Panel panel2;

		// Token: 0x0400043B RID: 1083
		private global::System.Windows.Forms.CheckBox checkBox2;

		// Token: 0x0400043C RID: 1084
		private global::System.Windows.Forms.CheckBox checkBox1;

		// Token: 0x0400043D RID: 1085
		private global::System.Windows.Forms.CheckBox checkBox3;

		// Token: 0x0400043E RID: 1086
		private global::System.Windows.Forms.CheckBox checkBox8;

		// Token: 0x0400043F RID: 1087
		private global::System.Windows.Forms.CheckBox checkBox7;

		// Token: 0x04000440 RID: 1088
		private global::System.Windows.Forms.CheckBox checkBox6;

		// Token: 0x04000441 RID: 1089
		private global::System.Windows.Forms.CheckBox checkBox5;

		// Token: 0x04000442 RID: 1090
		private global::System.Windows.Forms.CheckBox checkBox9;

		// Token: 0x04000443 RID: 1091
		private global::System.Windows.Forms.CheckBox checkBox10;

		// Token: 0x04000444 RID: 1092
		private global::System.Windows.Forms.CheckBox checkBox11;

		// Token: 0x04000445 RID: 1093
		private global::System.Windows.Forms.Label label3;

		// Token: 0x04000446 RID: 1094
		private global::System.Windows.Forms.Label label2;

		// Token: 0x04000447 RID: 1095
		private global::System.Windows.Forms.Label label1;

		// Token: 0x04000448 RID: 1096
		public global::System.Windows.Forms.NumericUpDown numericUpDown2;

		// Token: 0x04000449 RID: 1097
		private global::System.Windows.Forms.CheckBox checkBox12;

		// Token: 0x0400044A RID: 1098
		private global::System.Windows.Forms.CheckBox checkBox13;

		// Token: 0x0400044B RID: 1099
		private global::System.Windows.Forms.CheckBox checkBox14;
	}
}

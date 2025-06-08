namespace Server.Forms
{
	// Token: 0x020000B9 RID: 185
	public partial class FormRegisterValueEditWord : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000604 RID: 1540 RVA: 0x00056FCE File Offset: 0x000551CE
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000605 RID: 1541 RVA: 0x00056FF0 File Offset: 0x000551F0
		private void InitializeComponent()
		{
			this.rjButton4 = new global::CustomControls.RJControls.RJButton();
			this.valueNameTxtBox = new global::CustomControls.RJControls.RJTextBox();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.baseBox = new global::System.Windows.Forms.GroupBox();
			this.radioDecimal = new global::System.Windows.Forms.RadioButton();
			this.radioHexa = new global::System.Windows.Forms.RadioButton();
			this.valueDataTxtBox = new global::Server.Helper.WordTextBox();
			this.baseBox.SuspendLayout();
			base.SuspendLayout();
			this.rjButton4.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton4.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton4.BorderColor = global::System.Drawing.Color.PaleVioletRed;
			this.rjButton4.BorderRadius = 0;
			this.rjButton4.BorderSize = 0;
			this.rjButton4.FlatAppearance.BorderSize = 0;
			this.rjButton4.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton4.Font = new global::System.Drawing.Font("Arial", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.rjButton4.ForeColor = global::System.Drawing.Color.White;
			this.rjButton4.Location = new global::System.Drawing.Point(280, 155);
			this.rjButton4.Name = "rjButton4";
			this.rjButton4.Size = new global::System.Drawing.Size(74, 31);
			this.rjButton4.TabIndex = 50;
			this.rjButton4.Text = "Cancel";
			this.rjButton4.TextColor = global::System.Drawing.Color.White;
			this.rjButton4.UseVisualStyleBackColor = false;
			this.rjButton4.Click += new global::System.EventHandler(this.rjButton4_Click);
			this.valueNameTxtBox.BackColor = global::System.Drawing.SystemColors.Window;
			this.valueNameTxtBox.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.valueNameTxtBox.BorderFocusColor = global::System.Drawing.Color.HotPink;
			this.valueNameTxtBox.BorderRadius = 0;
			this.valueNameTxtBox.BorderSize = 1;
			this.valueNameTxtBox.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.valueNameTxtBox.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.valueNameTxtBox.Location = new global::System.Drawing.Point(7, 117);
			this.valueNameTxtBox.Margin = new global::System.Windows.Forms.Padding(4);
			this.valueNameTxtBox.Multiline = false;
			this.valueNameTxtBox.Name = "valueNameTxtBox";
			this.valueNameTxtBox.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.valueNameTxtBox.PasswordChar = false;
			this.valueNameTxtBox.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.valueNameTxtBox.PlaceholderText = "Name";
			this.valueNameTxtBox.Size = new global::System.Drawing.Size(347, 31);
			this.valueNameTxtBox.TabIndex = 49;
			this.valueNameTxtBox.Texts = "";
			this.valueNameTxtBox.UnderlinedStyle = false;
			this.rjButton1.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderColor = global::System.Drawing.Color.PaleVioletRed;
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 0;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.Font = new global::System.Drawing.Font("Arial", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.rjButton1.ForeColor = global::System.Drawing.Color.White;
			this.rjButton1.Location = new global::System.Drawing.Point(200, 155);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(74, 31);
			this.rjButton1.TabIndex = 52;
			this.rjButton1.Text = "Save";
			this.rjButton1.TextColor = global::System.Drawing.Color.White;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			this.baseBox.Controls.Add(this.radioDecimal);
			this.baseBox.Controls.Add(this.radioHexa);
			this.baseBox.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.baseBox.Location = new global::System.Drawing.Point(7, 155);
			this.baseBox.Name = "baseBox";
			this.baseBox.Size = new global::System.Drawing.Size(156, 68);
			this.baseBox.TabIndex = 53;
			this.baseBox.TabStop = false;
			this.baseBox.Text = "Base";
			this.radioDecimal.AutoSize = true;
			this.radioDecimal.Location = new global::System.Drawing.Point(14, 43);
			this.radioDecimal.Name = "radioDecimal";
			this.radioDecimal.Size = new global::System.Drawing.Size(63, 17);
			this.radioDecimal.TabIndex = 4;
			this.radioDecimal.Text = "Decimal";
			this.radioDecimal.UseVisualStyleBackColor = true;
			this.radioHexa.AutoSize = true;
			this.radioHexa.Checked = true;
			this.radioHexa.Location = new global::System.Drawing.Point(14, 18);
			this.radioHexa.Name = "radioHexa";
			this.radioHexa.Size = new global::System.Drawing.Size(86, 17);
			this.radioHexa.TabIndex = 3;
			this.radioHexa.TabStop = true;
			this.radioHexa.Text = "Hexadecimal";
			this.radioHexa.UseVisualStyleBackColor = true;
			this.radioHexa.CheckedChanged += new global::System.EventHandler(this.radioHexa_CheckedChanged);
			this.valueDataTxtBox.BackColor = global::System.Drawing.Color.White;
			this.valueDataTxtBox.ForeColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.valueDataTxtBox.IsHexNumber = false;
			this.valueDataTxtBox.Location = new global::System.Drawing.Point(7, 81);
			this.valueDataTxtBox.MaxLength = 8;
			this.valueDataTxtBox.Multiline = true;
			this.valueDataTxtBox.Name = "valueDataTxtBox";
			this.valueDataTxtBox.Size = new global::System.Drawing.Size(347, 29);
			this.valueDataTxtBox.TabIndex = 54;
			this.valueDataTxtBox.Type = global::Server.Helper.WordTextBox.WordType.DWORD;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(378, 240);
			base.Controls.Add(this.valueDataTxtBox);
			base.Controls.Add(this.baseBox);
			base.Controls.Add(this.rjButton1);
			base.Controls.Add(this.rjButton4);
			base.Controls.Add(this.valueNameTxtBox);
			base.Name = "FormRegisterValueEditWord";
			this.Text = "Register Value Edit Word";
			base.Load += new global::System.EventHandler(this.FormRegisterValueEditString_Load);
			this.baseBox.ResumeLayout(false);
			this.baseBox.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x040004C8 RID: 1224
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040004C9 RID: 1225
		private global::CustomControls.RJControls.RJButton rjButton4;

		// Token: 0x040004CA RID: 1226
		private global::CustomControls.RJControls.RJTextBox valueNameTxtBox;

		// Token: 0x040004CB RID: 1227
		private global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x040004CC RID: 1228
		private global::System.Windows.Forms.GroupBox baseBox;

		// Token: 0x040004CD RID: 1229
		private global::System.Windows.Forms.RadioButton radioDecimal;

		// Token: 0x040004CE RID: 1230
		private global::System.Windows.Forms.RadioButton radioHexa;

		// Token: 0x040004CF RID: 1231
		private global::Server.Helper.WordTextBox valueDataTxtBox;
	}
}

namespace Server.Forms
{
	// Token: 0x020000AB RID: 171
	public partial class FormAutoRunSet : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600054C RID: 1356 RVA: 0x0004C3B5 File Offset: 0x0004A5B5
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0004C3D4 File Offset: 0x0004A5D4
		private void InitializeComponent()
		{
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox2 = new global::CustomControls.RJControls.RJTextBox();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.rjButton2 = new global::CustomControls.RJControls.RJButton();
			this.rjComboBox2 = new global::CustomControls.RJControls.RJComboBox();
			base.SuspendLayout();
			this.rjTextBox1.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.HotPink;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox1.Location = new global::System.Drawing.Point(7, 68);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "Path";
			this.rjTextBox1.Size = new global::System.Drawing.Size(345, 31);
			this.rjTextBox1.TabIndex = 14;
			this.rjTextBox1.Texts = "";
			this.rjTextBox1.UnderlinedStyle = false;
			this.rjTextBox2.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox2.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox2.BorderFocusColor = global::System.Drawing.Color.HotPink;
			this.rjTextBox2.BorderRadius = 0;
			this.rjTextBox2.BorderSize = 1;
			this.rjTextBox2.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox2.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox2.Location = new global::System.Drawing.Point(7, 107);
			this.rjTextBox2.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox2.Multiline = false;
			this.rjTextBox2.Name = "rjTextBox2";
			this.rjTextBox2.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox2.PasswordChar = false;
			this.rjTextBox2.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox2.PlaceholderText = "Name";
			this.rjTextBox2.Size = new global::System.Drawing.Size(345, 31);
			this.rjTextBox2.TabIndex = 15;
			this.rjTextBox2.Texts = "";
			this.rjTextBox2.UnderlinedStyle = false;
			this.rjButton1.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderColor = global::System.Drawing.Color.PaleVioletRed;
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 0;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.Font = new global::System.Drawing.Font("Arial", 9f);
			this.rjButton1.ForeColor = global::System.Drawing.Color.White;
			this.rjButton1.Location = new global::System.Drawing.Point(286, 183);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(64, 31);
			this.rjButton1.TabIndex = 16;
			this.rjButton1.Text = "Set";
			this.rjButton1.TextColor = global::System.Drawing.Color.White;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			this.rjButton2.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton2.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton2.BorderColor = global::System.Drawing.Color.PaleVioletRed;
			this.rjButton2.BorderRadius = 0;
			this.rjButton2.BorderSize = 0;
			this.rjButton2.FlatAppearance.BorderSize = 0;
			this.rjButton2.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton2.Font = new global::System.Drawing.Font("Arial", 9f);
			this.rjButton2.ForeColor = global::System.Drawing.Color.White;
			this.rjButton2.Location = new global::System.Drawing.Point(216, 183);
			this.rjButton2.Name = "rjButton2";
			this.rjButton2.Size = new global::System.Drawing.Size(64, 31);
			this.rjButton2.TabIndex = 17;
			this.rjButton2.Text = "Cancel";
			this.rjButton2.TextColor = global::System.Drawing.Color.White;
			this.rjButton2.UseVisualStyleBackColor = false;
			this.rjButton2.Click += new global::System.EventHandler(this.rjButton2_Click);
			this.rjComboBox2.BackColor = global::System.Drawing.Color.White;
			this.rjComboBox2.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjComboBox2.BorderSize = 1;
			this.rjComboBox2.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDown;
			this.rjComboBox2.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 10f);
			this.rjComboBox2.ForeColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox2.IconColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjComboBox2.Items.AddRange(new object[]
			{
				"StartUp",
				"CurrentUser",
				"LocalMachine",
				"Node32",
				"UserInit",
				"Sheduler"
			});
			this.rjComboBox2.ListBackColor = global::System.Drawing.Color.White;
			this.rjComboBox2.ListTextColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox2.Location = new global::System.Drawing.Point(7, 145);
			this.rjComboBox2.MinimumSize = new global::System.Drawing.Size(200, 30);
			this.rjComboBox2.Name = "rjComboBox2";
			this.rjComboBox2.Padding = new global::System.Windows.Forms.Padding(1);
			this.rjComboBox2.Size = new global::System.Drawing.Size(343, 30);
			this.rjComboBox2.TabIndex = 22;
			this.rjComboBox2.Texts = "";
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(359, 220);
			base.Controls.Add(this.rjComboBox2);
			base.Controls.Add(this.rjButton2);
			base.Controls.Add(this.rjButton1);
			base.Controls.Add(this.rjTextBox2);
			base.Controls.Add(this.rjTextBox1);
			base.Name = "FormAutoRunSet";
			this.Text = "Auto Run Set";
			base.Load += new global::System.EventHandler(this.FormHVNCrun_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x040003F7 RID: 1015
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040003F8 RID: 1016
		private global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x040003F9 RID: 1017
		private global::CustomControls.RJControls.RJButton rjButton2;

		// Token: 0x040003FA RID: 1018
		public global::CustomControls.RJControls.RJTextBox rjTextBox1;

		// Token: 0x040003FB RID: 1019
		public global::CustomControls.RJControls.RJTextBox rjTextBox2;

		// Token: 0x040003FC RID: 1020
		private global::CustomControls.RJControls.RJComboBox rjComboBox2;
	}
}

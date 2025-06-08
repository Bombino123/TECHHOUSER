namespace Server.Forms
{
	// Token: 0x020000AD RID: 173
	public partial class FormInput : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600055B RID: 1371 RVA: 0x0004D2A6 File Offset: 0x0004B4A6
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600055C RID: 1372 RVA: 0x0004D2C8 File Offset: 0x0004B4C8
		private void InitializeComponent()
		{
			this.rjButton2 = new global::CustomControls.RJControls.RJButton();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			base.SuspendLayout();
			this.rjButton2.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton2.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton2.BorderColor = global::System.Drawing.Color.PaleVioletRed;
			this.rjButton2.BorderRadius = 0;
			this.rjButton2.BorderSize = 0;
			this.rjButton2.FlatAppearance.BorderSize = 0;
			this.rjButton2.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton2.Font = new global::System.Drawing.Font("Arial", 9f);
			this.rjButton2.ForeColor = global::System.Drawing.Color.White;
			this.rjButton2.Location = new global::System.Drawing.Point(144, 136);
			this.rjButton2.Name = "rjButton2";
			this.rjButton2.Size = new global::System.Drawing.Size(64, 31);
			this.rjButton2.TabIndex = 20;
			this.rjButton2.Text = "Cancel";
			this.rjButton2.TextColor = global::System.Drawing.Color.White;
			this.rjButton2.UseVisualStyleBackColor = false;
			this.rjButton2.Click += new global::System.EventHandler(this.rjButton2_Click);
			this.rjButton1.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderColor = global::System.Drawing.Color.PaleVioletRed;
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 0;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.Font = new global::System.Drawing.Font("Arial", 9f);
			this.rjButton1.ForeColor = global::System.Drawing.Color.White;
			this.rjButton1.Location = new global::System.Drawing.Point(214, 136);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(64, 31);
			this.rjButton1.TabIndex = 19;
			this.rjButton1.Text = "Run";
			this.rjButton1.TextColor = global::System.Drawing.Color.White;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			this.rjTextBox1.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.HotPink;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox1.Location = new global::System.Drawing.Point(28, 86);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "";
			this.rjTextBox1.Size = new global::System.Drawing.Size(250, 31);
			this.rjTextBox1.TabIndex = 18;
			this.rjTextBox1.Texts = "";
			this.rjTextBox1.UnderlinedStyle = false;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(312, 187);
			base.Controls.Add(this.rjButton2);
			base.Controls.Add(this.rjButton1);
			base.Controls.Add(this.rjTextBox1);
			base.Name = "FormInput";
			this.Text = "FormInput";
			base.Load += new global::System.EventHandler(this.FormInput_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x04000404 RID: 1028
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000405 RID: 1029
		private global::CustomControls.RJControls.RJButton rjButton2;

		// Token: 0x04000406 RID: 1030
		private global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x04000407 RID: 1031
		public global::CustomControls.RJControls.RJTextBox rjTextBox1;
	}
}

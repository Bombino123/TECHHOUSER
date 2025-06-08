namespace Server.Forms
{
	// Token: 0x020000AC RID: 172
	public partial class FormHVNCrun : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000553 RID: 1363 RVA: 0x0004CBC6 File Offset: 0x0004ADC6
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000554 RID: 1364 RVA: 0x0004CBE8 File Offset: 0x0004ADE8
		private void InitializeComponent()
		{
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox2 = new global::CustomControls.RJControls.RJTextBox();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.rjButton2 = new global::CustomControls.RJControls.RJButton();
			base.SuspendLayout();
			this.rjTextBox1.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.HotPink;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox1.Location = new global::System.Drawing.Point(47, 91);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "File Path";
			this.rjTextBox1.Size = new global::System.Drawing.Size(250, 31);
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
			this.rjTextBox2.Location = new global::System.Drawing.Point(47, 130);
			this.rjTextBox2.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox2.Multiline = false;
			this.rjTextBox2.Name = "rjTextBox2";
			this.rjTextBox2.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox2.PasswordChar = false;
			this.rjTextBox2.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox2.PlaceholderText = "Args";
			this.rjTextBox2.Size = new global::System.Drawing.Size(250, 31);
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
			this.rjButton1.Location = new global::System.Drawing.Point(233, 168);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(64, 31);
			this.rjButton1.TabIndex = 16;
			this.rjButton1.Text = "Run";
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
			this.rjButton2.Location = new global::System.Drawing.Point(163, 168);
			this.rjButton2.Name = "rjButton2";
			this.rjButton2.Size = new global::System.Drawing.Size(64, 31);
			this.rjButton2.TabIndex = 17;
			this.rjButton2.Text = "Cancel";
			this.rjButton2.TextColor = global::System.Drawing.Color.White;
			this.rjButton2.UseVisualStyleBackColor = false;
			this.rjButton2.Click += new global::System.EventHandler(this.rjButton2_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(359, 220);
			base.Controls.Add(this.rjButton2);
			base.Controls.Add(this.rjButton1);
			base.Controls.Add(this.rjTextBox2);
			base.Controls.Add(this.rjTextBox1);
			base.Name = "FormHVNCrun";
			this.Text = "Custom Run";
			base.Load += new global::System.EventHandler(this.FormHVNCrun_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x040003FE RID: 1022
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040003FF RID: 1023
		private global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x04000400 RID: 1024
		private global::CustomControls.RJControls.RJButton rjButton2;

		// Token: 0x04000401 RID: 1025
		public global::CustomControls.RJControls.RJTextBox rjTextBox1;

		// Token: 0x04000402 RID: 1026
		public global::CustomControls.RJControls.RJTextBox rjTextBox2;
	}
}

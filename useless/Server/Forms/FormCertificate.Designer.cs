namespace Server.Forms
{
	// Token: 0x02000094 RID: 148
	public partial class FormCertificate : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000401 RID: 1025 RVA: 0x000312BF File Offset: 0x0002F4BF
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x000312E0 File Offset: 0x0002F4E0
		private void InitializeComponent()
		{
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			base.SuspendLayout();
			this.rjTextBox1.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.DarkViolet;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 2;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox1.Location = new global::System.Drawing.Point(37, 98);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "Ca Name";
			this.rjTextBox1.Size = new global::System.Drawing.Size(250, 31);
			this.rjTextBox1.TabIndex = 2;
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
			this.rjButton1.Location = new global::System.Drawing.Point(111, 157);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(103, 29);
			this.rjButton1.TabIndex = 3;
			this.rjButton1.Text = "Create";
			this.rjButton1.TextColor = global::System.Drawing.Color.White;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(324, 248);
			base.Controls.Add(this.rjTextBox1);
			base.Controls.Add(this.rjButton1);
			base.Name = "FormCertificate";
			this.Text = "Certificate Create";
			base.Load += new global::System.EventHandler(this.FormAbout_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x0400024D RID: 589
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400024E RID: 590
		private global::CustomControls.RJControls.RJTextBox rjTextBox1;

		// Token: 0x0400024F RID: 591
		private global::CustomControls.RJControls.RJButton rjButton1;
	}
}

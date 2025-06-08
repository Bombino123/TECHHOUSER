namespace Server.Forms
{
	// Token: 0x020000A5 RID: 165
	public partial class FormWinlocker : global::Server.Helper.FormMaterial
	{
		// Token: 0x060004AE RID: 1198 RVA: 0x0003CCC0 File Offset: 0x0003AEC0
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x0003CCE0 File Offset: 0x0003AEE0
		private void InitializeComponent()
		{
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox2 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox3 = new global::CustomControls.RJControls.RJTextBox();
			this.rjButton5 = new global::CustomControls.RJControls.RJButton();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.panel1.BackColor = global::System.Drawing.Color.White;
			this.panel1.Controls.Add(this.rjButton1);
			this.panel1.Controls.Add(this.rjButton5);
			this.panel1.Controls.Add(this.rjTextBox3);
			this.panel1.Controls.Add(this.rjTextBox2);
			this.panel1.Controls.Add(this.rjTextBox1);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(624, 383);
			this.panel1.TabIndex = 0;
			this.rjTextBox1.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.HotPink;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox1.Location = new global::System.Drawing.Point(4, 4);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "Password";
			this.rjTextBox1.Size = new global::System.Drawing.Size(240, 31);
			this.rjTextBox1.TabIndex = 33;
			this.rjTextBox1.Texts = "";
			this.rjTextBox1.UnderlinedStyle = false;
			this.rjTextBox2.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox2.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox2.BorderFocusColor = global::System.Drawing.Color.HotPink;
			this.rjTextBox2.BorderRadius = 0;
			this.rjTextBox2.BorderSize = 1;
			this.rjTextBox2.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox2.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox2.Location = new global::System.Drawing.Point(252, 4);
			this.rjTextBox2.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox2.Multiline = false;
			this.rjTextBox2.Name = "rjTextBox2";
			this.rjTextBox2.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox2.PasswordChar = false;
			this.rjTextBox2.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox2.PlaceholderText = "Title";
			this.rjTextBox2.Size = new global::System.Drawing.Size(368, 31);
			this.rjTextBox2.TabIndex = 34;
			this.rjTextBox2.Texts = "";
			this.rjTextBox2.UnderlinedStyle = false;
			this.rjTextBox3.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox3.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox3.BorderFocusColor = global::System.Drawing.Color.HotPink;
			this.rjTextBox3.BorderRadius = 0;
			this.rjTextBox3.BorderSize = 1;
			this.rjTextBox3.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox3.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox3.Location = new global::System.Drawing.Point(4, 43);
			this.rjTextBox3.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox3.Multiline = true;
			this.rjTextBox3.Name = "rjTextBox3";
			this.rjTextBox3.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox3.PasswordChar = false;
			this.rjTextBox3.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox3.PlaceholderText = "Description";
			this.rjTextBox3.Size = new global::System.Drawing.Size(616, 290);
			this.rjTextBox3.TabIndex = 35;
			this.rjTextBox3.Texts = "";
			this.rjTextBox3.UnderlinedStyle = false;
			this.rjButton5.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton5.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton5.BorderColor = global::System.Drawing.Color.PaleVioletRed;
			this.rjButton5.BorderRadius = 0;
			this.rjButton5.BorderSize = 0;
			this.rjButton5.FlatAppearance.BorderSize = 0;
			this.rjButton5.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton5.Font = new global::System.Drawing.Font("Arial", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.rjButton5.ForeColor = global::System.Drawing.Color.White;
			this.rjButton5.Location = new global::System.Drawing.Point(527, 340);
			this.rjButton5.Name = "rjButton5";
			this.rjButton5.Size = new global::System.Drawing.Size(93, 31);
			this.rjButton5.TabIndex = 47;
			this.rjButton5.Text = "Send";
			this.rjButton5.TextColor = global::System.Drawing.Color.White;
			this.rjButton5.UseVisualStyleBackColor = false;
			this.rjButton5.Click += new global::System.EventHandler(this.rjButton5_Click);
			this.rjButton1.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderColor = global::System.Drawing.Color.PaleVioletRed;
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 0;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.Font = new global::System.Drawing.Font("Arial", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.rjButton1.ForeColor = global::System.Drawing.Color.White;
			this.rjButton1.Location = new global::System.Drawing.Point(428, 340);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(93, 31);
			this.rjButton1.TabIndex = 48;
			this.rjButton1.Text = "Cancel";
			this.rjButton1.TextColor = global::System.Drawing.Color.White;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(630, 450);
			base.Controls.Add(this.panel1);
			base.Name = "FormWinlocker";
			this.Text = "Winlocker";
			base.Load += new global::System.EventHandler(this.FormWinlocker_Load);
			this.panel1.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		// Token: 0x0400031B RID: 795
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400031C RID: 796
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x0400031D RID: 797
		private global::CustomControls.RJControls.RJTextBox rjTextBox1;

		// Token: 0x0400031E RID: 798
		private global::CustomControls.RJControls.RJTextBox rjTextBox2;

		// Token: 0x0400031F RID: 799
		private global::CustomControls.RJControls.RJTextBox rjTextBox3;

		// Token: 0x04000320 RID: 800
		private global::CustomControls.RJControls.RJButton rjButton5;

		// Token: 0x04000321 RID: 801
		private global::CustomControls.RJControls.RJButton rjButton1;
	}
}

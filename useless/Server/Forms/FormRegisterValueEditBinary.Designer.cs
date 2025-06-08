namespace Server.Forms
{
	// Token: 0x020000BA RID: 186
	public partial class FormRegisterValueEditBinary : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600060B RID: 1547 RVA: 0x00057830 File Offset: 0x00055A30
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x00057850 File Offset: 0x00055A50
		private void InitializeComponent()
		{
			this.rjButton4 = new global::CustomControls.RJControls.RJButton();
			this.valueNameTxtBox = new global::CustomControls.RJControls.RJTextBox();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.hexEditor = new global::Server.Helper.HexEditor.HexEditor();
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
			this.rjButton4.Location = new global::System.Drawing.Point(280, 448);
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
			this.valueNameTxtBox.Location = new global::System.Drawing.Point(7, 410);
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
			this.rjButton1.Location = new global::System.Drawing.Point(200, 448);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(74, 31);
			this.rjButton1.TabIndex = 52;
			this.rjButton1.Text = "Save";
			this.rjButton1.TextColor = global::System.Drawing.Color.White;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			this.hexEditor.BorderColor = global::System.Drawing.Color.Empty;
			this.hexEditor.Cursor = global::System.Windows.Forms.Cursors.IBeam;
			this.hexEditor.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.hexEditor.Location = new global::System.Drawing.Point(7, 67);
			this.hexEditor.Name = "hexEditor";
			this.hexEditor.SelectionBackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.hexEditor.Size = new global::System.Drawing.Size(347, 336);
			this.hexEditor.TabIndex = 53;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(367, 495);
			base.Controls.Add(this.hexEditor);
			base.Controls.Add(this.rjButton1);
			base.Controls.Add(this.rjButton4);
			base.Controls.Add(this.valueNameTxtBox);
			base.Name = "FormRegisterValueEditBinary";
			this.Text = "Register Value Edit Binary";
			base.Load += new global::System.EventHandler(this.FormRegisterValueEditString_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x040004D2 RID: 1234
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040004D3 RID: 1235
		private global::CustomControls.RJControls.RJButton rjButton4;

		// Token: 0x040004D4 RID: 1236
		private global::CustomControls.RJControls.RJTextBox valueNameTxtBox;

		// Token: 0x040004D5 RID: 1237
		private global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x040004D6 RID: 1238
		private global::Server.Helper.HexEditor.HexEditor hexEditor;
	}
}

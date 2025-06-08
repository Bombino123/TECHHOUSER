namespace Server.Forms
{
	// Token: 0x0200009C RID: 156
	public partial class FormSpeakerBot : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600043E RID: 1086 RVA: 0x00033BC8 File Offset: 0x00031DC8
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x00033BE8 File Offset: 0x00031DE8
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.rjComboBox1 = new global::CustomControls.RJControls.RJComboBox();
			this.materialLabel3 = new global::MaterialSkin.Controls.MaterialLabel();
			this.materialLabel2 = new global::MaterialSkin.Controls.MaterialLabel();
			this.trackBar1 = new global::System.Windows.Forms.TrackBar();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.trackBar2 = new global::System.Windows.Forms.TrackBar();
			this.richTextBox1 = new global::System.Windows.Forms.RichTextBox();
			this.panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar2).BeginInit();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.panel1.Controls.Add(this.rjButton1);
			this.panel1.Controls.Add(this.rjComboBox1);
			this.panel1.Controls.Add(this.materialLabel3);
			this.panel1.Controls.Add(this.materialLabel2);
			this.panel1.Controls.Add(this.trackBar1);
			this.panel1.Controls.Add(this.materialLabel1);
			this.panel1.Controls.Add(this.trackBar2);
			this.panel1.Controls.Add(this.richTextBox1);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(524, 479);
			this.panel1.TabIndex = 0;
			this.rjButton1.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left);
			this.rjButton1.BackColor = global::System.Drawing.Color.White;
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.White;
			this.rjButton1.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 1;
			this.rjButton1.Enabled = false;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.Location = new global::System.Drawing.Point(395, 376);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(92, 30);
			this.rjButton1.TabIndex = 36;
			this.rjButton1.Text = "Speak";
			this.rjButton1.TextColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click_1);
			this.rjComboBox1.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left);
			this.rjComboBox1.BackColor = global::System.Drawing.Color.WhiteSmoke;
			this.rjComboBox1.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjComboBox1.BorderSize = 1;
			this.rjComboBox1.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDown;
			this.rjComboBox1.Enabled = false;
			this.rjComboBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 10f);
			this.rjComboBox1.ForeColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox1.IconColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjComboBox1.ListBackColor = global::System.Drawing.Color.FromArgb(230, 228, 245);
			this.rjComboBox1.ListTextColor = global::System.Drawing.Color.DimGray;
			this.rjComboBox1.Location = new global::System.Drawing.Point(98, 429);
			this.rjComboBox1.MinimumSize = new global::System.Drawing.Size(200, 30);
			this.rjComboBox1.Name = "rjComboBox1";
			this.rjComboBox1.Padding = new global::System.Windows.Forms.Padding(1);
			this.rjComboBox1.Size = new global::System.Drawing.Size(250, 30);
			this.rjComboBox1.TabIndex = 35;
			this.rjComboBox1.Texts = "";
			this.materialLabel3.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left);
			this.materialLabel3.AutoSize = true;
			this.materialLabel3.Depth = 0;
			this.materialLabel3.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel3.Location = new global::System.Drawing.Point(7, 428);
			this.materialLabel3.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel3.Name = "materialLabel3";
			this.materialLabel3.Size = new global::System.Drawing.Size(48, 19);
			this.materialLabel3.TabIndex = 34;
			this.materialLabel3.Text = "Voices";
			this.materialLabel2.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left);
			this.materialLabel2.AutoSize = true;
			this.materialLabel2.Depth = 0;
			this.materialLabel2.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel2.Location = new global::System.Drawing.Point(7, 340);
			this.materialLabel2.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel2.Name = "materialLabel2";
			this.materialLabel2.Size = new global::System.Drawing.Size(55, 19);
			this.materialLabel2.TabIndex = 33;
			this.materialLabel2.Text = "Volume";
			this.trackBar1.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left);
			this.trackBar1.Enabled = false;
			this.trackBar1.Location = new global::System.Drawing.Point(98, 327);
			this.trackBar1.Maximum = 100;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new global::System.Drawing.Size(250, 45);
			this.trackBar1.TabIndex = 30;
			this.trackBar1.Value = 100;
			this.materialLabel1.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left);
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.Depth = 0;
			this.materialLabel1.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel1.Location = new global::System.Drawing.Point(7, 387);
			this.materialLabel1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new global::System.Drawing.Size(85, 19);
			this.materialLabel1.TabIndex = 32;
			this.materialLabel1.Text = "Tone Speak";
			this.trackBar2.Anchor = (global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left);
			this.trackBar2.Enabled = false;
			this.trackBar2.Location = new global::System.Drawing.Point(98, 378);
			this.trackBar2.Minimum = -10;
			this.trackBar2.Name = "trackBar2";
			this.trackBar2.Size = new global::System.Drawing.Size(250, 45);
			this.trackBar2.TabIndex = 31;
			this.richTextBox1.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.richTextBox1.BackColor = global::System.Drawing.Color.White;
			this.richTextBox1.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Enabled = false;
			this.richTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.richTextBox1.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.richTextBox1.Location = new global::System.Drawing.Point(0, 0);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new global::System.Drawing.Size(521, 321);
			this.richTextBox1.TabIndex = 2;
			this.richTextBox1.Text = "Your fucking computer infected by Leberium rat";
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(530, 546);
			base.Controls.Add(this.panel1);
			base.Name = "FormSpeakerBot";
			this.Text = "Speaker Bot";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.trackBar2).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x04000289 RID: 649
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400028A RID: 650
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x0400028B RID: 651
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x0400028C RID: 652
		public global::System.Windows.Forms.RichTextBox richTextBox1;

		// Token: 0x0400028D RID: 653
		private global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x0400028E RID: 654
		private global::MaterialSkin.Controls.MaterialLabel materialLabel2;

		// Token: 0x0400028F RID: 655
		private global::MaterialSkin.Controls.MaterialLabel materialLabel3;

		// Token: 0x04000290 RID: 656
		public global::System.Windows.Forms.TrackBar trackBar1;

		// Token: 0x04000291 RID: 657
		public global::System.Windows.Forms.TrackBar trackBar2;

		// Token: 0x04000292 RID: 658
		public global::CustomControls.RJControls.RJComboBox rjComboBox1;

		// Token: 0x04000293 RID: 659
		public global::CustomControls.RJControls.RJButton rjButton1;
	}
}

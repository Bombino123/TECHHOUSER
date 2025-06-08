namespace Server.Forms
{
	// Token: 0x02000096 RID: 150
	public partial class FormDeviceManager : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000410 RID: 1040 RVA: 0x00031A2F File Offset: 0x0002FC2F
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x00031A50 File Offset: 0x0002FC50
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormDeviceManager));
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.contextMenuStrip1 = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.refreshToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.sToolStripMenuItem = new global::System.Windows.Forms.ToolStripSeparator();
			this.startToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.stopToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.dataGridView2 = new global::System.Windows.Forms.DataGridView();
			this.Column1 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column3 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column4 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.contextMenuStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).BeginInit();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.contextMenuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.refreshToolStripMenuItem,
				this.sToolStripMenuItem,
				this.startToolStripMenuItem,
				this.stopToolStripMenuItem
			});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new global::System.Drawing.Size(114, 76);
			this.refreshToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("refreshToolStripMenuItem.Image");
			this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
			this.refreshToolStripMenuItem.Size = new global::System.Drawing.Size(113, 22);
			this.refreshToolStripMenuItem.Text = "Refresh";
			this.refreshToolStripMenuItem.Click += new global::System.EventHandler(this.refreshToolStripMenuItem_Click);
			this.sToolStripMenuItem.Name = "sToolStripMenuItem";
			this.sToolStripMenuItem.Size = new global::System.Drawing.Size(110, 6);
			this.startToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("startToolStripMenuItem.Image");
			this.startToolStripMenuItem.Name = "startToolStripMenuItem";
			this.startToolStripMenuItem.Size = new global::System.Drawing.Size(113, 22);
			this.startToolStripMenuItem.Text = "Enable";
			this.startToolStripMenuItem.Click += new global::System.EventHandler(this.startToolStripMenuItem_Click);
			this.stopToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("stopToolStripMenuItem.Image");
			this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
			this.stopToolStripMenuItem.Size = new global::System.Drawing.Size(113, 22);
			this.stopToolStripMenuItem.Text = "Disable";
			this.stopToolStripMenuItem.Click += new global::System.EventHandler(this.stopToolStripMenuItem_Click);
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
				this.Column4
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
			this.Column1.MinimumWidth = 150;
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			this.Column1.Width = 150;
			this.Column2.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column2.HeaderText = "Class";
			this.Column2.MinimumWidth = 100;
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			this.Column3.HeaderText = "Status";
			this.Column3.MinimumWidth = 70;
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			this.Column3.Width = 70;
			this.Column4.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column4.HeaderText = "Description";
			this.Column4.MinimumWidth = 100;
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(800, 535);
			base.Controls.Add(this.dataGridView2);
			base.Controls.Add(this.panel1);
			base.Name = "FormDeviceManager";
			this.Text = "Device Manager ";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x04000255 RID: 597
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000256 RID: 598
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000257 RID: 599
		private global::System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

		// Token: 0x04000258 RID: 600
		private global::System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;

		// Token: 0x04000259 RID: 601
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x0400025A RID: 602
		public global::System.Windows.Forms.DataGridView dataGridView2;

		// Token: 0x0400025B RID: 603
		public global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x0400025C RID: 604
		private global::System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;

		// Token: 0x0400025D RID: 605
		private global::System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;

		// Token: 0x0400025E RID: 606
		private global::System.Windows.Forms.ToolStripSeparator sToolStripMenuItem;

		// Token: 0x0400025F RID: 607
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column1;

		// Token: 0x04000260 RID: 608
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column2;

		// Token: 0x04000261 RID: 609
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column3;

		// Token: 0x04000262 RID: 610
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column4;
	}
}

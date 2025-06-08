namespace Server.Forms
{
	// Token: 0x020000B2 RID: 178
	public partial class FormService : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000591 RID: 1425 RVA: 0x00050A0B File Offset: 0x0004EC0B
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x00050A2C File Offset: 0x0004EC2C
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormService));
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.contextMenuStrip1 = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.startToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.pauseToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.stopToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.killRemoveToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.dataGridView2 = new global::System.Windows.Forms.DataGridView();
			this.Column1 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column3 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column4 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column5 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.contextMenuStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).BeginInit();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.contextMenuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.startToolStripMenuItem,
				this.pauseToolStripMenuItem,
				this.stopToolStripMenuItem,
				this.killRemoveToolStripMenuItem
			});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new global::System.Drawing.Size(181, 114);
			this.startToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("startToolStripMenuItem.Image");
			this.startToolStripMenuItem.Name = "startToolStripMenuItem";
			this.startToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.startToolStripMenuItem.Text = "Start";
			this.startToolStripMenuItem.Click += new global::System.EventHandler(this.startToolStripMenuItem_Click);
			this.pauseToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("pauseToolStripMenuItem.Image");
			this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
			this.pauseToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.pauseToolStripMenuItem.Text = "Pause";
			this.pauseToolStripMenuItem.Click += new global::System.EventHandler(this.pauseToolStripMenuItem_Click);
			this.stopToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("stopToolStripMenuItem.Image");
			this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
			this.stopToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.stopToolStripMenuItem.Text = "Stop";
			this.stopToolStripMenuItem.Click += new global::System.EventHandler(this.stopToolStripMenuItem_Click);
			this.killRemoveToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("killRemoveToolStripMenuItem.Image");
			this.killRemoveToolStripMenuItem.Name = "killRemoveToolStripMenuItem";
			this.killRemoveToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.killRemoveToolStripMenuItem.Text = "Refresh";
			this.killRemoveToolStripMenuItem.Click += new global::System.EventHandler(this.killRemoveToolStripMenuItem_Click);
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
			this.dataGridView2.Size = new global::System.Drawing.Size(794, 448);
			this.dataGridView2.TabIndex = 17;
			this.Column1.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column1.HeaderText = "Service";
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			this.Column1.Width = 70;
			this.Column2.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column2.HeaderText = "Display Name";
			this.Column2.MinimumWidth = 150;
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			this.Column2.Width = 150;
			this.Column3.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column3.HeaderText = "Type";
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			this.Column3.Width = 55;
			this.Column4.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column4.HeaderText = "Status";
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			this.Column4.Width = 65;
			this.Column5.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column5.HeaderText = "Can Stop";
			this.Column5.Name = "Column5";
			this.Column5.ReadOnly = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(800, 535);
			base.Controls.Add(this.dataGridView2);
			base.Controls.Add(this.panel1);
			base.Name = "FormService";
			this.Text = "Netstat";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x0400044E RID: 1102
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400044F RID: 1103
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000450 RID: 1104
		private global::System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

		// Token: 0x04000451 RID: 1105
		private global::System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;

		// Token: 0x04000452 RID: 1106
		private global::System.Windows.Forms.ToolStripMenuItem killRemoveToolStripMenuItem;

		// Token: 0x04000453 RID: 1107
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x04000454 RID: 1108
		public global::System.Windows.Forms.DataGridView dataGridView2;

		// Token: 0x04000455 RID: 1109
		public global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x04000456 RID: 1110
		private global::System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;

		// Token: 0x04000457 RID: 1111
		private global::System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;

		// Token: 0x04000458 RID: 1112
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column1;

		// Token: 0x04000459 RID: 1113
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column2;

		// Token: 0x0400045A RID: 1114
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column3;

		// Token: 0x0400045B RID: 1115
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column4;

		// Token: 0x0400045C RID: 1116
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column5;
	}
}

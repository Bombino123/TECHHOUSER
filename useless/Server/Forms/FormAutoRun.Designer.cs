namespace Server.Forms
{
	// Token: 0x020000AF RID: 175
	public partial class FormAutoRun : Server.Helper.FormMaterial
	{
		// Token: 0x06000574 RID: 1396 RVA: 0x0004E82C File Offset: 0x0004CA2C
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x0004E84C File Offset: 0x0004CA4C
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAutoRun));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.killToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.killRemoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
			this.dataGridView2 = new System.Windows.Forms.DataGridView();
			this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.contextMenuStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
			this.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
			{
				this.toolStripMenuItem1,
				this.killToolStripMenuItem,
				this.killRemoveToolStripMenuItem
			});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(181, 92);
			this.toolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItem1.Image")));
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
			this.toolStripMenuItem1.Text = "Add";
			this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
			this.killToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("killToolStripMenuItem.Image")));
			this.killToolStripMenuItem.Name = "killToolStripMenuItem";
			this.killToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.killToolStripMenuItem.Text = "Remove";
			this.killToolStripMenuItem.Click += new System.EventHandler(this.killToolStripMenuItem_Click);
			this.killRemoveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("killRemoveToolStripMenuItem.Image")));
			this.killRemoveToolStripMenuItem.Name = "killRemoveToolStripMenuItem";
			this.killRemoveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.killRemoveToolStripMenuItem.Text = "Refresh";
			this.killRemoveToolStripMenuItem.Click += new System.EventHandler(this.killRemoveToolStripMenuItem_Click);
			this.panel1.Controls.Add(this.materialLabel1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(3, 512);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(794, 20);
			this.panel1.TabIndex = 1;
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.Depth = 0;
			this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel1.Location = new System.Drawing.Point(2, 1);
			this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new System.Drawing.Size(94, 19);
			this.materialLabel1.TabIndex = 1;
			this.materialLabel1.Text = "Please wait...";
			this.dataGridView2.AllowDrop = true;
			this.dataGridView2.AllowUserToAddRows = false;
			this.dataGridView2.AllowUserToDeleteRows = false;
			this.dataGridView2.AllowUserToResizeColumns = false;
			this.dataGridView2.AllowUserToResizeRows = false;
			this.dataGridView2.BackgroundColor = System.Drawing.Color.White;
			this.dataGridView2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dataGridView2.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGridView2.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Arial", 9F);
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Purple;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Purple;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
			{
				this.Column1,
				this.Column2,
				this.Column3
			});
			this.dataGridView2.ContextMenuStrip = this.contextMenuStrip1;
			this.dataGridView2.Cursor = System.Windows.Forms.Cursors.Default;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView2.DefaultCellStyle = dataGridViewCellStyle2;
			this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView2.Enabled = false;
			this.dataGridView2.EnableHeadersVisualStyles = false;
			this.dataGridView2.GridColor = System.Drawing.Color.FromArgb(17, 17, 17);
			this.dataGridView2.Location = new System.Drawing.Point(3, 64);
			this.dataGridView2.Name = "dataGridView2";
			this.dataGridView2.ReadOnly = true;
			this.dataGridView2.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView2.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridView2.RowHeadersVisible = false;
			this.dataGridView2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView2.ShowCellErrors = false;
			this.dataGridView2.ShowCellToolTips = false;
			this.dataGridView2.ShowEditingIcon = false;
			this.dataGridView2.ShowRowErrors = false;
			this.dataGridView2.Size = new System.Drawing.Size(794, 448);
			this.dataGridView2.TabIndex = 17;
			this.Column1.HeaderText = "Type";
			this.Column1.MinimumWidth = 100;
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column2.HeaderText = "Name";
			this.Column2.MinimumWidth = 100;
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column3.HeaderText = "Path";
			this.Column3.MinimumWidth = 100;
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(800, 535);
			this.Controls.Add(this.dataGridView2);
			this.Controls.Add(this.panel1);
			this.Name = "FormAutoRun";
			this.Text = "AutoRun";
			this.Load += new System.EventHandler(this.FormProcess_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
			this.ResumeLayout(false);
		}

		// Token: 0x04000420 RID: 1056
		private System.ComponentModel.IContainer components;

		// Token: 0x04000421 RID: 1057
		private System.Windows.Forms.Timer timer1;

		// Token: 0x04000422 RID: 1058
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

		// Token: 0x04000423 RID: 1059
		private System.Windows.Forms.ToolStripMenuItem killToolStripMenuItem;

		// Token: 0x04000424 RID: 1060
		private System.Windows.Forms.ToolStripMenuItem killRemoveToolStripMenuItem;

		// Token: 0x04000425 RID: 1061
		private System.Windows.Forms.Panel panel1;

		// Token: 0x04000426 RID: 1062
		public System.Windows.Forms.DataGridView dataGridView2;

		// Token: 0x04000427 RID: 1063
		public MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x04000428 RID: 1064
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;

		// Token: 0x04000429 RID: 1065
		private System.Windows.Forms.DataGridViewTextBoxColumn Column1;

		// Token: 0x0400042A RID: 1066
		private System.Windows.Forms.DataGridViewTextBoxColumn Column2;

		// Token: 0x0400042B RID: 1067
		private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
	}
}

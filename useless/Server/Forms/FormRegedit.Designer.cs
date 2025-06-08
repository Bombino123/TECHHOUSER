namespace Server.Forms
{
	// Token: 0x020000B7 RID: 183
	public partial class FormRegedit : global::Server.Helper.FormMaterial
	{
		// Token: 0x060005F3 RID: 1523 RVA: 0x00054F64 File Offset: 0x00053164
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00054F84 File Offset: 0x00053184
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormRegedit));
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.imageList1 = new global::System.Windows.Forms.ImageList(this.components);
			this.imageRegistryKeyTypeList = new global::System.Windows.Forms.ImageList(this.components);
			this.imageRegistryDirectoryList = new global::System.Windows.Forms.ImageList(this.components);
			this.materialLabel1 = new global::MaterialSkin.Controls.MaterialLabel();
			this.panel3 = new global::System.Windows.Forms.Panel();
			this.tvRegistryDirectory = new global::Server.Helper.RegistryTreeView();
			this.lstRegistryValues = new global::Server.Helper.AeroListView();
			this.hName = new global::System.Windows.Forms.ColumnHeader();
			this.hType = new global::System.Windows.Forms.ColumnHeader();
			this.hValue = new global::System.Windows.Forms.ColumnHeader();
			this.tv_ContextMenuStrip = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.newToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.keyToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new global::System.Windows.Forms.ToolStripSeparator();
			this.stringValueToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.binaryValueToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.dWORD32bitValueToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.qWORD64bitValueToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.multiStringValueToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.expandableStringValueToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.renameToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.selectedItem_ContextMenuStrip = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.modifyToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.modifyBinaryDataToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.renameToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.lst_ContextMenuStrip = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.newToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.keyToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new global::System.Windows.Forms.ToolStripSeparator();
			this.stringValueToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.binaryValueToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.dWORD32bitValueToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.qWORD64bitValueToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.multiStringValueToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.expandableStringValueToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.panel3.SuspendLayout();
			this.tv_ContextMenuStrip.SuspendLayout();
			this.selectedItem_ContextMenuStrip.SuspendLayout();
			this.lst_ContextMenuStrip.SuspendLayout();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.imageList1.ImageStream = (global::System.Windows.Forms.ImageListStreamer)componentResourceManager.GetObject("imageList1.ImageStream");
			this.imageList1.TransparentColor = global::System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "regedit_1983.png");
			this.imageList1.Images.SetKeyName(1, "folder.png");
			this.imageRegistryKeyTypeList.ImageStream = (global::System.Windows.Forms.ImageListStreamer)componentResourceManager.GetObject("imageRegistryKeyTypeList.ImageStream");
			this.imageRegistryKeyTypeList.TransparentColor = global::System.Drawing.Color.Transparent;
			this.imageRegistryKeyTypeList.Images.SetKeyName(0, "reg_string.png");
			this.imageRegistryKeyTypeList.Images.SetKeyName(1, "reg_binary.png");
			this.imageRegistryDirectoryList.ImageStream = (global::System.Windows.Forms.ImageListStreamer)componentResourceManager.GetObject("imageRegistryDirectoryList.ImageStream");
			this.imageRegistryDirectoryList.TransparentColor = global::System.Drawing.Color.Transparent;
			this.imageRegistryDirectoryList.Images.SetKeyName(0, "folder.png");
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.Depth = 0;
			this.materialLabel1.Enabled = false;
			this.materialLabel1.Font = new global::System.Drawing.Font("Roboto", 14f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Pixel);
			this.materialLabel1.Location = new global::System.Drawing.Point(6, 3);
			this.materialLabel1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new global::System.Drawing.Size(94, 19);
			this.materialLabel1.TabIndex = 0;
			this.materialLabel1.Text = "Please wait...";
			this.panel3.Controls.Add(this.materialLabel1);
			this.panel3.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panel3.Location = new global::System.Drawing.Point(3, 548);
			this.panel3.Name = "panel3";
			this.panel3.Size = new global::System.Drawing.Size(867, 23);
			this.panel3.TabIndex = 18;
			this.tvRegistryDirectory.BackColor = global::System.Drawing.Color.White;
			this.tvRegistryDirectory.Dock = global::System.Windows.Forms.DockStyle.Left;
			this.tvRegistryDirectory.Enabled = false;
			this.tvRegistryDirectory.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.tvRegistryDirectory.HideSelection = false;
			this.tvRegistryDirectory.ImageIndex = 0;
			this.tvRegistryDirectory.ImageList = this.imageRegistryDirectoryList;
			this.tvRegistryDirectory.Location = new global::System.Drawing.Point(3, 64);
			this.tvRegistryDirectory.Name = "tvRegistryDirectory";
			this.tvRegistryDirectory.SelectedImageIndex = 0;
			this.tvRegistryDirectory.Size = new global::System.Drawing.Size(257, 484);
			this.tvRegistryDirectory.TabIndex = 19;
			this.lstRegistryValues.BackColor = global::System.Drawing.Color.White;
			this.lstRegistryValues.Columns.AddRange(new global::System.Windows.Forms.ColumnHeader[]
			{
				this.hName,
				this.hType,
				this.hValue
			});
			this.lstRegistryValues.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.lstRegistryValues.Enabled = false;
			this.lstRegistryValues.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.lstRegistryValues.FullRowSelect = true;
			this.lstRegistryValues.HeaderStyle = global::System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstRegistryValues.HideSelection = false;
			this.lstRegistryValues.Location = new global::System.Drawing.Point(260, 64);
			this.lstRegistryValues.Name = "lstRegistryValues";
			this.lstRegistryValues.Size = new global::System.Drawing.Size(610, 484);
			this.lstRegistryValues.SmallImageList = this.imageRegistryKeyTypeList;
			this.lstRegistryValues.TabIndex = 20;
			this.lstRegistryValues.UseCompatibleStateImageBehavior = false;
			this.lstRegistryValues.View = global::System.Windows.Forms.View.Details;
			this.lstRegistryValues.AfterLabelEdit += new global::System.Windows.Forms.LabelEditEventHandler(this.lstRegistryKeys_AfterLabelEdit);
			this.lstRegistryValues.KeyUp += new global::System.Windows.Forms.KeyEventHandler(this.lstRegistryKeys_KeyUp);
			this.lstRegistryValues.MouseUp += new global::System.Windows.Forms.MouseEventHandler(this.lstRegistryKeys_MouseClick);
			this.hName.Text = "Name";
			this.hName.Width = 173;
			this.hType.Text = "Type";
			this.hType.Width = 104;
			this.hValue.Text = "Value";
			this.hValue.Width = 214;
			this.tv_ContextMenuStrip.ImageScalingSize = new global::System.Drawing.Size(20, 20);
			this.tv_ContextMenuStrip.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.newToolStripMenuItem,
				this.deleteToolStripMenuItem,
				this.renameToolStripMenuItem
			});
			this.tv_ContextMenuStrip.Name = "contextMenuStrip";
			this.tv_ContextMenuStrip.Size = new global::System.Drawing.Size(185, 104);
			this.newToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.newToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.keyToolStripMenuItem,
				this.toolStripSeparator2,
				this.stringValueToolStripMenuItem,
				this.binaryValueToolStripMenuItem,
				this.dWORD32bitValueToolStripMenuItem,
				this.qWORD64bitValueToolStripMenuItem,
				this.multiStringValueToolStripMenuItem,
				this.expandableStringValueToolStripMenuItem
			});
			this.newToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.newToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("newToolStripMenuItem.Image");
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.Size = new global::System.Drawing.Size(184, 26);
			this.newToolStripMenuItem.Text = "New";
			this.keyToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.keyToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.keyToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("keyToolStripMenuItem.Image");
			this.keyToolStripMenuItem.Name = "keyToolStripMenuItem";
			this.keyToolStripMenuItem.Size = new global::System.Drawing.Size(204, 26);
			this.keyToolStripMenuItem.Text = "Key";
			this.keyToolStripMenuItem.Click += new global::System.EventHandler(this.createNewRegistryKey_Click);
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new global::System.Drawing.Size(197, 6);
			this.stringValueToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.stringValueToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.stringValueToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("stringValueToolStripMenuItem.Image");
			this.stringValueToolStripMenuItem.Name = "stringValueToolStripMenuItem";
			this.stringValueToolStripMenuItem.Size = new global::System.Drawing.Size(204, 26);
			this.stringValueToolStripMenuItem.Text = "String Value";
			this.stringValueToolStripMenuItem.Click += new global::System.EventHandler(this.createStringRegistryValue_Click);
			this.binaryValueToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.binaryValueToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.binaryValueToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("binaryValueToolStripMenuItem.Image");
			this.binaryValueToolStripMenuItem.Name = "binaryValueToolStripMenuItem";
			this.binaryValueToolStripMenuItem.Size = new global::System.Drawing.Size(204, 26);
			this.binaryValueToolStripMenuItem.Text = "Binary Value";
			this.binaryValueToolStripMenuItem.Click += new global::System.EventHandler(this.createBinaryRegistryValue_Click);
			this.dWORD32bitValueToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.dWORD32bitValueToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.dWORD32bitValueToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("dWORD32bitValueToolStripMenuItem.Image");
			this.dWORD32bitValueToolStripMenuItem.Name = "dWORD32bitValueToolStripMenuItem";
			this.dWORD32bitValueToolStripMenuItem.Size = new global::System.Drawing.Size(204, 26);
			this.dWORD32bitValueToolStripMenuItem.Text = "DWORD (32-bit) Value";
			this.dWORD32bitValueToolStripMenuItem.Click += new global::System.EventHandler(this.createDwordRegistryValue_Click);
			this.qWORD64bitValueToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.qWORD64bitValueToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.qWORD64bitValueToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("qWORD64bitValueToolStripMenuItem.Image");
			this.qWORD64bitValueToolStripMenuItem.Name = "qWORD64bitValueToolStripMenuItem";
			this.qWORD64bitValueToolStripMenuItem.Size = new global::System.Drawing.Size(204, 26);
			this.qWORD64bitValueToolStripMenuItem.Text = "QWORD (64-bit) Value";
			this.qWORD64bitValueToolStripMenuItem.Click += new global::System.EventHandler(this.createQwordRegistryValue_Click);
			this.multiStringValueToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.multiStringValueToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.multiStringValueToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("multiStringValueToolStripMenuItem.Image");
			this.multiStringValueToolStripMenuItem.Name = "multiStringValueToolStripMenuItem";
			this.multiStringValueToolStripMenuItem.Size = new global::System.Drawing.Size(204, 26);
			this.multiStringValueToolStripMenuItem.Text = "Multi-String Value";
			this.multiStringValueToolStripMenuItem.Click += new global::System.EventHandler(this.createMultiStringRegistryValue_Click);
			this.expandableStringValueToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.expandableStringValueToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.expandableStringValueToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("expandableStringValueToolStripMenuItem.Image");
			this.expandableStringValueToolStripMenuItem.Name = "expandableStringValueToolStripMenuItem";
			this.expandableStringValueToolStripMenuItem.Size = new global::System.Drawing.Size(204, 26);
			this.expandableStringValueToolStripMenuItem.Text = "Expandable String Value";
			this.expandableStringValueToolStripMenuItem.Click += new global::System.EventHandler(this.createExpandStringRegistryValue_Click);
			this.deleteToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.deleteToolStripMenuItem.Enabled = false;
			this.deleteToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.deleteToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("deleteToolStripMenuItem.Image");
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new global::System.Drawing.Size(184, 26);
			this.deleteToolStripMenuItem.Text = "Delete";
			this.deleteToolStripMenuItem.Click += new global::System.EventHandler(this.renameRegistryKey_Click);
			this.renameToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.renameToolStripMenuItem.Enabled = false;
			this.renameToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.renameToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("renameToolStripMenuItem.Image");
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			this.renameToolStripMenuItem.Size = new global::System.Drawing.Size(184, 26);
			this.renameToolStripMenuItem.Text = "Rename";
			this.renameToolStripMenuItem.Click += new global::System.EventHandler(this.renameRegistryValue_Click);
			this.selectedItem_ContextMenuStrip.BackColor = global::System.Drawing.Color.White;
			this.selectedItem_ContextMenuStrip.ImageScalingSize = new global::System.Drawing.Size(20, 20);
			this.selectedItem_ContextMenuStrip.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.modifyToolStripMenuItem,
				this.modifyBinaryDataToolStripMenuItem,
				this.deleteToolStripMenuItem1,
				this.renameToolStripMenuItem1
			});
			this.selectedItem_ContextMenuStrip.Name = "selectedItem_ContextMenuStrip";
			this.selectedItem_ContextMenuStrip.Size = new global::System.Drawing.Size(189, 108);
			this.modifyToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.modifyToolStripMenuItem.Enabled = false;
			this.modifyToolStripMenuItem.Font = new global::System.Drawing.Font("Segoe UI", 9f, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, 0);
			this.modifyToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.modifyToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("modifyToolStripMenuItem.Image");
			this.modifyToolStripMenuItem.Name = "modifyToolStripMenuItem";
			this.modifyToolStripMenuItem.Size = new global::System.Drawing.Size(188, 26);
			this.modifyToolStripMenuItem.Text = "Modify...";
			this.modifyToolStripMenuItem.Click += new global::System.EventHandler(this.modifyRegistryValue_Click);
			this.modifyBinaryDataToolStripMenuItem.BackColor = global::System.Drawing.Color.White;
			this.modifyBinaryDataToolStripMenuItem.Enabled = false;
			this.modifyBinaryDataToolStripMenuItem.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.modifyBinaryDataToolStripMenuItem.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("modifyBinaryDataToolStripMenuItem.Image");
			this.modifyBinaryDataToolStripMenuItem.Name = "modifyBinaryDataToolStripMenuItem";
			this.modifyBinaryDataToolStripMenuItem.Size = new global::System.Drawing.Size(188, 26);
			this.modifyBinaryDataToolStripMenuItem.Text = "Modify Binary Data...";
			this.modifyBinaryDataToolStripMenuItem.Click += new global::System.EventHandler(this.modifyBinaryDataRegistryValue_Click);
			this.deleteToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.deleteToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.deleteToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("deleteToolStripMenuItem1.Image");
			this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
			this.deleteToolStripMenuItem1.Size = new global::System.Drawing.Size(188, 26);
			this.deleteToolStripMenuItem1.Text = "Delete";
			this.deleteToolStripMenuItem1.Click += new global::System.EventHandler(this.deleteRegistryValue_Click);
			this.renameToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.renameToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.renameToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("renameToolStripMenuItem1.Image");
			this.renameToolStripMenuItem1.Name = "renameToolStripMenuItem1";
			this.renameToolStripMenuItem1.Size = new global::System.Drawing.Size(188, 26);
			this.renameToolStripMenuItem1.Text = "Rename";
			this.renameToolStripMenuItem1.Click += new global::System.EventHandler(this.renameRegistryValue_Click);
			this.lst_ContextMenuStrip.BackColor = global::System.Drawing.Color.White;
			this.lst_ContextMenuStrip.ImageScalingSize = new global::System.Drawing.Size(20, 20);
			this.lst_ContextMenuStrip.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.newToolStripMenuItem1
			});
			this.lst_ContextMenuStrip.Name = "lst_ContextMenuStrip";
			this.lst_ContextMenuStrip.Size = new global::System.Drawing.Size(103, 30);
			this.newToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.newToolStripMenuItem1.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.keyToolStripMenuItem1,
				this.toolStripSeparator4,
				this.stringValueToolStripMenuItem1,
				this.binaryValueToolStripMenuItem1,
				this.dWORD32bitValueToolStripMenuItem1,
				this.qWORD64bitValueToolStripMenuItem1,
				this.multiStringValueToolStripMenuItem1,
				this.expandableStringValueToolStripMenuItem1
			});
			this.newToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.newToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("newToolStripMenuItem1.Image");
			this.newToolStripMenuItem1.Name = "newToolStripMenuItem1";
			this.newToolStripMenuItem1.Size = new global::System.Drawing.Size(102, 26);
			this.newToolStripMenuItem1.Text = "New";
			this.keyToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.keyToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.keyToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("keyToolStripMenuItem1.Image");
			this.keyToolStripMenuItem1.Name = "keyToolStripMenuItem1";
			this.keyToolStripMenuItem1.Size = new global::System.Drawing.Size(204, 26);
			this.keyToolStripMenuItem1.Text = "Key";
			this.keyToolStripMenuItem1.Click += new global::System.EventHandler(this.createNewRegistryKey_Click);
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new global::System.Drawing.Size(197, 6);
			this.stringValueToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.stringValueToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.stringValueToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("stringValueToolStripMenuItem1.Image");
			this.stringValueToolStripMenuItem1.Name = "stringValueToolStripMenuItem1";
			this.stringValueToolStripMenuItem1.Size = new global::System.Drawing.Size(204, 26);
			this.stringValueToolStripMenuItem1.Text = "String Value";
			this.stringValueToolStripMenuItem1.Click += new global::System.EventHandler(this.createStringRegistryValue_Click);
			this.binaryValueToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.binaryValueToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.binaryValueToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("binaryValueToolStripMenuItem1.Image");
			this.binaryValueToolStripMenuItem1.Name = "binaryValueToolStripMenuItem1";
			this.binaryValueToolStripMenuItem1.Size = new global::System.Drawing.Size(204, 26);
			this.binaryValueToolStripMenuItem1.Text = "Binary Value";
			this.binaryValueToolStripMenuItem1.Click += new global::System.EventHandler(this.createBinaryRegistryValue_Click);
			this.dWORD32bitValueToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.dWORD32bitValueToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.dWORD32bitValueToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("dWORD32bitValueToolStripMenuItem1.Image");
			this.dWORD32bitValueToolStripMenuItem1.Name = "dWORD32bitValueToolStripMenuItem1";
			this.dWORD32bitValueToolStripMenuItem1.Size = new global::System.Drawing.Size(204, 26);
			this.dWORD32bitValueToolStripMenuItem1.Text = "DWORD (32-bit) Value";
			this.dWORD32bitValueToolStripMenuItem1.Click += new global::System.EventHandler(this.createDwordRegistryValue_Click);
			this.qWORD64bitValueToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.qWORD64bitValueToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.qWORD64bitValueToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("qWORD64bitValueToolStripMenuItem1.Image");
			this.qWORD64bitValueToolStripMenuItem1.Name = "qWORD64bitValueToolStripMenuItem1";
			this.qWORD64bitValueToolStripMenuItem1.Size = new global::System.Drawing.Size(204, 26);
			this.qWORD64bitValueToolStripMenuItem1.Text = "QWORD (64-bit) Value";
			this.qWORD64bitValueToolStripMenuItem1.Click += new global::System.EventHandler(this.createQwordRegistryValue_Click);
			this.multiStringValueToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.multiStringValueToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.multiStringValueToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("multiStringValueToolStripMenuItem1.Image");
			this.multiStringValueToolStripMenuItem1.Name = "multiStringValueToolStripMenuItem1";
			this.multiStringValueToolStripMenuItem1.Size = new global::System.Drawing.Size(204, 26);
			this.multiStringValueToolStripMenuItem1.Text = "Multi-String Value";
			this.multiStringValueToolStripMenuItem1.Click += new global::System.EventHandler(this.createMultiStringRegistryValue_Click);
			this.expandableStringValueToolStripMenuItem1.BackColor = global::System.Drawing.Color.White;
			this.expandableStringValueToolStripMenuItem1.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 192);
			this.expandableStringValueToolStripMenuItem1.Image = (global::System.Drawing.Image)componentResourceManager.GetObject("expandableStringValueToolStripMenuItem1.Image");
			this.expandableStringValueToolStripMenuItem1.Name = "expandableStringValueToolStripMenuItem1";
			this.expandableStringValueToolStripMenuItem1.Size = new global::System.Drawing.Size(204, 26);
			this.expandableStringValueToolStripMenuItem1.Text = "Expandable String Value";
			this.expandableStringValueToolStripMenuItem1.Click += new global::System.EventHandler(this.createExpandStringRegistryValue_Click);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(873, 574);
			base.Controls.Add(this.lstRegistryValues);
			base.Controls.Add(this.tvRegistryDirectory);
			base.Controls.Add(this.panel3);
			base.Name = "FormRegedit";
			this.Text = "Regedit";
			base.Load += new global::System.EventHandler(this.FormRegedit_Load);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.tv_ContextMenuStrip.ResumeLayout(false);
			this.selectedItem_ContextMenuStrip.ResumeLayout(false);
			this.lst_ContextMenuStrip.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		// Token: 0x04000498 RID: 1176
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000499 RID: 1177
		public global::System.Windows.Forms.Timer timer1;

		// Token: 0x0400049A RID: 1178
		public global::System.Windows.Forms.ImageList imageList1;

		// Token: 0x0400049B RID: 1179
		private global::System.Windows.Forms.ImageList imageRegistryKeyTypeList;

		// Token: 0x0400049C RID: 1180
		private global::System.Windows.Forms.ImageList imageRegistryDirectoryList;

		// Token: 0x0400049D RID: 1181
		public global::MaterialSkin.Controls.MaterialLabel materialLabel1;

		// Token: 0x0400049E RID: 1182
		private global::System.Windows.Forms.Panel panel3;

		// Token: 0x0400049F RID: 1183
		public global::Server.Helper.RegistryTreeView tvRegistryDirectory;

		// Token: 0x040004A0 RID: 1184
		private global::System.Windows.Forms.ColumnHeader hName;

		// Token: 0x040004A1 RID: 1185
		private global::System.Windows.Forms.ColumnHeader hType;

		// Token: 0x040004A2 RID: 1186
		private global::System.Windows.Forms.ColumnHeader hValue;

		// Token: 0x040004A3 RID: 1187
		private global::System.Windows.Forms.ContextMenuStrip tv_ContextMenuStrip;

		// Token: 0x040004A4 RID: 1188
		private global::System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;

		// Token: 0x040004A5 RID: 1189
		private global::System.Windows.Forms.ToolStripMenuItem keyToolStripMenuItem;

		// Token: 0x040004A6 RID: 1190
		private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator2;

		// Token: 0x040004A7 RID: 1191
		private global::System.Windows.Forms.ToolStripMenuItem stringValueToolStripMenuItem;

		// Token: 0x040004A8 RID: 1192
		private global::System.Windows.Forms.ToolStripMenuItem binaryValueToolStripMenuItem;

		// Token: 0x040004A9 RID: 1193
		private global::System.Windows.Forms.ToolStripMenuItem dWORD32bitValueToolStripMenuItem;

		// Token: 0x040004AA RID: 1194
		private global::System.Windows.Forms.ToolStripMenuItem qWORD64bitValueToolStripMenuItem;

		// Token: 0x040004AB RID: 1195
		private global::System.Windows.Forms.ToolStripMenuItem multiStringValueToolStripMenuItem;

		// Token: 0x040004AC RID: 1196
		private global::System.Windows.Forms.ToolStripMenuItem expandableStringValueToolStripMenuItem;

		// Token: 0x040004AD RID: 1197
		private global::System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;

		// Token: 0x040004AE RID: 1198
		private global::System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;

		// Token: 0x040004AF RID: 1199
		private global::System.Windows.Forms.ContextMenuStrip selectedItem_ContextMenuStrip;

		// Token: 0x040004B0 RID: 1200
		private global::System.Windows.Forms.ToolStripMenuItem modifyToolStripMenuItem;

		// Token: 0x040004B1 RID: 1201
		private global::System.Windows.Forms.ToolStripMenuItem modifyBinaryDataToolStripMenuItem;

		// Token: 0x040004B2 RID: 1202
		private global::System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;

		// Token: 0x040004B3 RID: 1203
		private global::System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem1;

		// Token: 0x040004B4 RID: 1204
		private global::System.Windows.Forms.ContextMenuStrip lst_ContextMenuStrip;

		// Token: 0x040004B5 RID: 1205
		private global::System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem1;

		// Token: 0x040004B6 RID: 1206
		private global::System.Windows.Forms.ToolStripMenuItem keyToolStripMenuItem1;

		// Token: 0x040004B7 RID: 1207
		private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator4;

		// Token: 0x040004B8 RID: 1208
		private global::System.Windows.Forms.ToolStripMenuItem stringValueToolStripMenuItem1;

		// Token: 0x040004B9 RID: 1209
		private global::System.Windows.Forms.ToolStripMenuItem binaryValueToolStripMenuItem1;

		// Token: 0x040004BA RID: 1210
		private global::System.Windows.Forms.ToolStripMenuItem dWORD32bitValueToolStripMenuItem1;

		// Token: 0x040004BB RID: 1211
		private global::System.Windows.Forms.ToolStripMenuItem qWORD64bitValueToolStripMenuItem1;

		// Token: 0x040004BC RID: 1212
		private global::System.Windows.Forms.ToolStripMenuItem multiStringValueToolStripMenuItem1;

		// Token: 0x040004BD RID: 1213
		private global::System.Windows.Forms.ToolStripMenuItem expandableStringValueToolStripMenuItem1;

		// Token: 0x040004BE RID: 1214
		public global::Server.Helper.AeroListView lstRegistryValues;
	}
}

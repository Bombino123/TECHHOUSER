namespace Server.Forms
{
	// Token: 0x0200009F RID: 159
	public partial class FormKeyLoggerPanel : global::Server.Helper.FormMaterial
	{
		// Token: 0x0600045B RID: 1115 RVA: 0x00036E45 File Offset: 0x00035045
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x00036E64 File Offset: 0x00035064
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.richTextBox1 = new global::System.Windows.Forms.RichTextBox();
			this.dataGridView3 = new global::System.Windows.Forms.DataGridView();
			this.Column1 = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView3).BeginInit();
			base.SuspendLayout();
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			this.richTextBox1.BackColor = global::System.Drawing.Color.White;
			this.richTextBox1.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Enabled = false;
			this.richTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.richTextBox1.ForeColor = global::System.Drawing.Color.MediumSlateBlue;
			this.richTextBox1.Location = new global::System.Drawing.Point(203, 64);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new global::System.Drawing.Size(594, 486);
			this.richTextBox1.TabIndex = 2;
			this.richTextBox1.Text = "";
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
			this.dataGridView3.Dock = global::System.Windows.Forms.DockStyle.Left;
			this.dataGridView3.Enabled = false;
			this.dataGridView3.EnableHeadersVisualStyles = false;
			this.dataGridView3.GridColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			this.dataGridView3.Location = new global::System.Drawing.Point(3, 64);
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
			this.dataGridView3.Size = new global::System.Drawing.Size(200, 486);
			this.dataGridView3.TabIndex = 14;
			this.Column1.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column1.HeaderText = "";
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(800, 553);
			base.Controls.Add(this.richTextBox1);
			base.Controls.Add(this.dataGridView3);
			base.Name = "FormKeyLoggerPanel";
			this.Text = "KeyLogger Panel";
			base.Load += new global::System.EventHandler(this.FormKeyLoggerPanel_Load);
			((global::System.ComponentModel.ISupportInitialize)this.dataGridView3).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x040002B7 RID: 695
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040002B8 RID: 696
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x040002B9 RID: 697
		public global::System.Windows.Forms.RichTextBox richTextBox1;

		// Token: 0x040002BA RID: 698
		public global::System.Windows.Forms.DataGridView dataGridView3;

		// Token: 0x040002BB RID: 699
		private global::System.Windows.Forms.DataGridViewTextBoxColumn Column1;
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x0200009F RID: 159
	public partial class FormKeyLoggerPanel : FormMaterial
	{
		// Token: 0x06000455 RID: 1109 RVA: 0x00036C68 File Offset: 0x00034E68
		public FormKeyLoggerPanel()
		{
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
			this.InitializeComponent();
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x00036C88 File Offset: 0x00034E88
		private void FormKeyLoggerPanel_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			this.dataGridView3.DoubleClick += this.dataGridView3_DoubleClick;
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x00036CD4 File Offset: 0x00034ED4
		private void ChangeScheme(object sender)
		{
			this.dataGridView3.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView3.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView3.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView3.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.richTextBox1.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x00036D48 File Offset: 0x00034F48
		private void dataGridView3_DoubleClick(object sender, EventArgs e)
		{
			if (this.dataGridView3.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView3.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((string)dataGridViewRow.Cells[0].Value);
			}
			this.client.Send(new object[]
			{
				"GetLog",
				string.Join(",", list.ToArray())
			});
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x00036E00 File Offset: 0x00035000
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x00036E15 File Offset: 0x00035015
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (!this.parrent.itsConnect)
			{
				base.Close();
			}
			if (this.client != null && !this.client.itsConnect)
			{
				base.Close();
			}
		}

		// Token: 0x040002B5 RID: 693
		public Clients client;

		// Token: 0x040002B6 RID: 694
		public Clients parrent;
	}
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000A0 RID: 160
	public partial class FormPrograms : FormMaterial
	{
		// Token: 0x0600045D RID: 1117 RVA: 0x00037352 File Offset: 0x00035552
		public FormPrograms()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x00037374 File Offset: 0x00035574
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, this.dataGridView2, new object[]
			{
				true
			});
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x000373D9 File Offset: 0x000355D9
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x000373F0 File Offset: 0x000355F0
		private void ChangeScheme(object sender)
		{
			this.dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x00037451 File Offset: 0x00035651
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

		// Token: 0x06000462 RID: 1122 RVA: 0x00037481 File Offset: 0x00035681
		private void killRemoveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Refresh"
			}));
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x000374A4 File Offset: 0x000356A4
		private void toolStripMenuItem2_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				this.client.Send(LEB128.Write(new object[]
				{
					"UninstallQuet",
					(string)dataGridViewRow.Cells[5].Value
				}));
			}
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x00037548 File Offset: 0x00035748
		private void toolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				this.client.Send(LEB128.Write(new object[]
				{
					"Uninstall",
					(string)dataGridViewRow.Cells[5].Value
				}));
			}
		}

		// Token: 0x040002BC RID: 700
		public Clients client;

		// Token: 0x040002BD RID: 701
		public Clients parrent;
	}
}

using System;
using System.Collections.Generic;
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
	// Token: 0x020000AF RID: 175
	public partial class FormAutoRun : FormMaterial
	{
		// Token: 0x0600056B RID: 1387 RVA: 0x0004E5C5 File Offset: 0x0004C7C5
		public FormAutoRun()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x0004E5E8 File Offset: 0x0004C7E8
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

		// Token: 0x0600056D RID: 1389 RVA: 0x0004E650 File Offset: 0x0004C850
		private void ChangeScheme(object sender)
		{
			this.dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x0004E6B1 File Offset: 0x0004C8B1
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x0004E6C6 File Offset: 0x0004C8C6
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

		// Token: 0x06000570 RID: 1392 RVA: 0x0004E6F8 File Offset: 0x0004C8F8
		private string[] NamesSelected()
		{
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((string)dataGridViewRow.Cells[0].Value + ";" + (string)dataGridViewRow.Cells[1].Value);
			}
			return list.ToArray();
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x0004E798 File Offset: 0x0004C998
		private void killToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (string text in this.NamesSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Remove",
					text
				}));
			}
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0004E7F3 File Offset: 0x0004C9F3
		private void killRemoveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Refresh"
			}));
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x0004E813 File Offset: 0x0004CA13
		private void toolStripMenuItem1_Click(object sender, EventArgs e)
		{
			new FormAutoRunSet
			{
				client = this.client
			}.ShowDialog();
		}

		// Token: 0x0400041E RID: 1054
		public Clients client;

		// Token: 0x0400041F RID: 1055
		public Clients parrent;
	}
}

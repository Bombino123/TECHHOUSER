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
	// Token: 0x020000B2 RID: 178
	public partial class FormService : FormMaterial
	{
		// Token: 0x06000587 RID: 1415 RVA: 0x00050727 File Offset: 0x0004E927
		public FormService()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x00050748 File Offset: 0x0004E948
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

		// Token: 0x06000589 RID: 1417 RVA: 0x000507B0 File Offset: 0x0004E9B0
		private void ChangeScheme(object sender)
		{
			this.dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x00050811 File Offset: 0x0004EA11
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x00050826 File Offset: 0x0004EA26
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

		// Token: 0x0600058C RID: 1420 RVA: 0x00050858 File Offset: 0x0004EA58
		private string[] DisplayNameSelected()
		{
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((string)dataGridViewRow.Cells[1].Value);
			}
			return list.ToArray();
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x000508D8 File Offset: 0x0004EAD8
		private void killRemoveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Refresh"
			}));
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x000508F8 File Offset: 0x0004EAF8
		private void startToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (string text in this.DisplayNameSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Start",
					text
				}));
			}
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x00050954 File Offset: 0x0004EB54
		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (string text in this.DisplayNameSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Pause",
					text
				}));
			}
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x000509B0 File Offset: 0x0004EBB0
		private void stopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (string text in this.DisplayNameSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Stop",
					text
				}));
			}
		}

		// Token: 0x0400044C RID: 1100
		public Clients client;

		// Token: 0x0400044D RID: 1101
		public Clients parrent;
	}
}

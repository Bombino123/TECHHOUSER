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
	// Token: 0x020000A3 RID: 163
	public partial class FormWindow : FormMaterial
	{
		// Token: 0x06000489 RID: 1161 RVA: 0x0003A40C File Offset: 0x0003860C
		public FormWindow()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x0003A42C File Offset: 0x0003862C
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

		// Token: 0x0600048B RID: 1163 RVA: 0x0003A494 File Offset: 0x00038694
		private void ChangeScheme(object sender)
		{
			this.dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x0003A4F5 File Offset: 0x000386F5
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x0003A50A File Offset: 0x0003870A
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

		// Token: 0x0600048E RID: 1166 RVA: 0x0003A53C File Offset: 0x0003873C
		private int[] PidsSelected()
		{
			List<int> list = new List<int>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((int)dataGridViewRow.Cells[3].Value);
			}
			return list.ToArray();
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x0003A5BC File Offset: 0x000387BC
		private int[] HandleSelected()
		{
			List<int> list = new List<int>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((int)dataGridViewRow.Cells[2].Value);
			}
			return list.ToArray();
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x0003A63C File Offset: 0x0003883C
		private void toolStripMenuItem1_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Refresh"
			}));
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x0003A65C File Offset: 0x0003885C
		private void killProcessToolStripMenuItem_Click(object sender, EventArgs e)
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

		// Token: 0x06000492 RID: 1170 RVA: 0x0003A6BC File Offset: 0x000388BC
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

		// Token: 0x06000493 RID: 1171 RVA: 0x0003A71C File Offset: 0x0003891C
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

		// Token: 0x06000494 RID: 1172 RVA: 0x0003A77C File Offset: 0x0003897C
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
					"KillRemove",
					num
				}));
			}
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x0003A7DC File Offset: 0x000389DC
		private void toolStripMenuItem3_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (int num in this.HandleSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Maximize",
					num
				}));
			}
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x0003A83C File Offset: 0x00038A3C
		private void stopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (int num in this.HandleSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Minimize",
					num
				}));
			}
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x0003A89C File Offset: 0x00038A9C
		private void startToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (int num in this.HandleSelected())
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"RestoreHide",
					num
				}));
			}
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x0003A8FC File Offset: 0x00038AFC
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

		// Token: 0x06000499 RID: 1177 RVA: 0x0003A9A8 File Offset: 0x00038BA8
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

		// Token: 0x040002EF RID: 751
		public Clients client;

		// Token: 0x040002F0 RID: 752
		public Clients parrent;
	}
}

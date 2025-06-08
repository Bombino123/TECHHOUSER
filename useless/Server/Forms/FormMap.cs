using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x02000097 RID: 151
	public partial class FormMap : FormMaterial
	{
		// Token: 0x06000412 RID: 1042 RVA: 0x00032305 File Offset: 0x00030505
		public FormMap()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x00032325 File Offset: 0x00030525
		private void FormMap_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.gMapControl1.DragButton = MouseButtons.Left;
			this.gMapControl1.MapProvider = GMapProviders.GoogleMap;
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x00032364 File Offset: 0x00030564
		private void ChangeScheme(object sender)
		{
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x00032366 File Offset: 0x00030566
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x0003237B File Offset: 0x0003057B
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

		// Token: 0x04000263 RID: 611
		public Clients client;

		// Token: 0x04000264 RID: 612
		public Clients parrent;
	}
}

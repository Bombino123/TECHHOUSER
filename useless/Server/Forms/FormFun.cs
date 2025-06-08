using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000A7 RID: 167
	public partial class FormFun : FormMaterial
	{
		// Token: 0x060004BD RID: 1213 RVA: 0x0003E7E6 File Offset: 0x0003C9E6
		public FormFun()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x0003E808 File Offset: 0x0003CA08
		private void FormFun_Load(object sender, EventArgs e)
		{
			this.rjComboBox2.SelectedIndex = 0;
			this.rjComboBox1.SelectedIndex = 0;
			this.rjComboBox3.SelectedIndex = 0;
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x0003E864 File Offset: 0x0003CA64
		private void ChangeScheme(object sender)
		{
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton2.BackColor = FormMaterial.PrimaryColor;
			this.rjButton3.BackColor = FormMaterial.PrimaryColor;
			this.rjButton4.BackColor = FormMaterial.PrimaryColor;
			this.rjButton5.BackColor = FormMaterial.PrimaryColor;
			this.rjButton6.BackColor = FormMaterial.PrimaryColor;
			this.rjButton7.BackColor = FormMaterial.PrimaryColor;
			this.rjButton8.BackColor = FormMaterial.PrimaryColor;
			this.rjButton9.BackColor = FormMaterial.PrimaryColor;
			this.rjButton10.BackColor = FormMaterial.PrimaryColor;
			this.rjButton11.BackColor = FormMaterial.PrimaryColor;
			this.rjButton12.BackColor = FormMaterial.PrimaryColor;
			this.rjButton13.BackColor = FormMaterial.PrimaryColor;
			this.rjButton14.BackColor = FormMaterial.PrimaryColor;
			this.rjButton15.BackColor = FormMaterial.PrimaryColor;
			this.rjButton16.BackColor = FormMaterial.PrimaryColor;
			this.rjButton17.BackColor = FormMaterial.PrimaryColor;
			this.rjButton18.BackColor = FormMaterial.PrimaryColor;
			this.rjButton19.BackColor = FormMaterial.PrimaryColor;
			this.rjButton20.BackColor = FormMaterial.PrimaryColor;
			this.rjButton21.BackColor = FormMaterial.PrimaryColor;
			this.rjButton22.BackColor = FormMaterial.PrimaryColor;
			this.rjButton23.BackColor = FormMaterial.PrimaryColor;
			this.rjButton24.BackColor = FormMaterial.PrimaryColor;
			this.rjButton25.BackColor = FormMaterial.PrimaryColor;
			this.rjButton26.BackColor = FormMaterial.PrimaryColor;
			this.rjButton27.BackColor = FormMaterial.PrimaryColor;
			this.rjButton28.BackColor = FormMaterial.PrimaryColor;
			this.rjButton29.BackColor = FormMaterial.PrimaryColor;
			this.rjButton30.BackColor = FormMaterial.PrimaryColor;
			this.rjButton31.BackColor = FormMaterial.PrimaryColor;
			this.rjButton32.BackColor = FormMaterial.PrimaryColor;
			this.rjButton33.BackColor = FormMaterial.PrimaryColor;
			this.rjButton34.BackColor = FormMaterial.PrimaryColor;
			this.rjButton35.BackColor = FormMaterial.PrimaryColor;
			this.rjButton36.BackColor = FormMaterial.PrimaryColor;
			this.rjButton37.BackColor = FormMaterial.PrimaryColor;
			this.rjButton38.BackColor = FormMaterial.PrimaryColor;
			this.rjButton39.BackColor = FormMaterial.PrimaryColor;
			this.rjButton40.BackColor = FormMaterial.PrimaryColor;
			this.rjButton41.BackColor = FormMaterial.PrimaryColor;
			this.rjButton42.BackColor = FormMaterial.PrimaryColor;
			this.rjButton43.BackColor = FormMaterial.PrimaryColor;
			this.rjButton44.BackColor = FormMaterial.PrimaryColor;
			this.rjButton45.BackColor = FormMaterial.PrimaryColor;
			this.rjButton46.BackColor = FormMaterial.PrimaryColor;
			this.rjButton47.BackColor = FormMaterial.PrimaryColor;
			this.rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjComboBox2.BorderColor = FormMaterial.PrimaryColor;
			this.rjComboBox3.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x0003EBC1 File Offset: 0x0003CDC1
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x0003EBD6 File Offset: 0x0003CDD6
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

		// Token: 0x060004C2 RID: 1218 RVA: 0x0003EC08 File Offset: 0x0003CE08
		private void rjButton1_Click(object sender, EventArgs e)
		{
			if (this.rjButton1.Text == "Hidden")
			{
				this.rjButton1.Text = "Show";
				this.client.Send(new object[]
				{
					"Taskbar-"
				});
				return;
			}
			this.rjButton1.Text = "Hidden";
			this.client.Send(new object[]
			{
				"Taskbar+"
			});
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x0003EC80 File Offset: 0x0003CE80
		private void rjButton2_Click(object sender, EventArgs e)
		{
			if (this.rjButton2.Text == "Hidden")
			{
				this.rjButton2.Text = "Show";
				this.client.Send(new object[]
				{
					"Desktop-"
				});
				return;
			}
			this.rjButton2.Text = "Hidden";
			this.client.Send(new object[]
			{
				"Desktop+"
			});
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x0003ECF8 File Offset: 0x0003CEF8
		private void rjButton14_Click(object sender, EventArgs e)
		{
			if (this.rjButton14.Text == "Start")
			{
				this.rjButton14.Text = "Stop";
				this.client.Send(new object[]
				{
					"blankscreen+"
				});
				return;
			}
			this.rjButton14.Text = "Start";
			this.client.Send(new object[]
			{
				"blankscreen-"
			});
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x0003ED70 File Offset: 0x0003CF70
		private void rjButton3_Click(object sender, EventArgs e)
		{
			if (this.rjButton3.Text == "Start")
			{
				this.rjButton3.Text = "Stop";
				this.client.Send(new object[]
				{
					"holdMouse+"
				});
				return;
			}
			this.rjButton3.Text = "Start";
			this.client.Send(new object[]
			{
				"holdMouse-"
			});
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x0003EDE8 File Offset: 0x0003CFE8
		private void rjButton4_Click(object sender, EventArgs e)
		{
			if (this.rjButton4.Text == "Start")
			{
				this.rjButton4.Text = "Stop";
				this.client.Send(new object[]
				{
					"blockInput+"
				});
				return;
			}
			this.rjButton4.Text = "Start";
			this.client.Send(new object[]
			{
				"blockInput-"
			});
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x0003EE5F File Offset: 0x0003D05F
		private void rjButton5_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"openCD+"
			});
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x0003EE7A File Offset: 0x0003D07A
		private void rjButton13_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"openCD-"
			});
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x0003EE98 File Offset: 0x0003D098
		private void rjButton41_Click(object sender, EventArgs e)
		{
			if (this.rjButton41.Text == "Swap")
			{
				this.rjButton41.Text = "Restore";
				this.client.Send(new object[]
				{
					"swapMouseButtons"
				});
				return;
			}
			this.rjButton41.Text = "Swap";
			this.client.Send(new object[]
			{
				"restoreMouseButtons"
			});
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x0003EF0F File Offset: 0x0003D10F
		private void rjButton6_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"monitorOff"
			});
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x0003EF2A File Offset: 0x0003D12A
		private void rjButton15_Click(object sender, EventArgs e)
		{
			if (this.rjComboBox2.SelectedIndex != -1)
			{
				this.client.Send(new object[]
				{
					"ScreenS",
					this.rjComboBox2.Texts
				});
			}
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x0003EF61 File Offset: 0x0003D161
		private void rjButton42_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"hangSystem"
			});
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x0003EF7C File Offset: 0x0003D17C
		private void rjButton32_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.rjTextBox1.Texts))
			{
				this.client.Send(new object[]
				{
					"OpenLink",
					this.rjTextBox1.Texts
				});
			}
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x0003EFB7 File Offset: 0x0003D1B7
		private void rjButton9_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"Powershell"
			});
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x0003EFD2 File Offset: 0x0003D1D2
		private void rjButton10_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"Taskmgr"
			});
		}

		// Token: 0x060004D0 RID: 1232 RVA: 0x0003EFED File Offset: 0x0003D1ED
		private void rjButton11_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"Notepad"
			});
		}

		// Token: 0x060004D1 RID: 1233 RVA: 0x0003F008 File Offset: 0x0003D208
		private void rjButton8_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"Calc"
			});
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x0003F023 File Offset: 0x0003D223
		private void rjButton7_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"Cmd"
			});
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x0003F03E File Offset: 0x0003D23E
		private void rjButton12_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"Explorer"
			});
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x0003F059 File Offset: 0x0003D259
		private void rjButton37_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"ScreenColor",
				"Default"
			});
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x0003F07C File Offset: 0x0003D27C
		private void rjButton40_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"ScreenColor",
				"Red"
			});
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x0003F09F File Offset: 0x0003D29F
		private void rjButton38_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"ScreenColor",
				"Purple"
			});
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x0003F0C2 File Offset: 0x0003D2C2
		private void rjButton39_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"ScreenColor",
				"Yellow"
			});
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x0003F0E5 File Offset: 0x0003D2E5
		private void rjButton33_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"ScreenColor",
				"Dark"
			});
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x0003F108 File Offset: 0x0003D308
		private void rjButton36_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"ScreenColor",
				"Cyan"
			});
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x0003F12B File Offset: 0x0003D32B
		private void rjButton34_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"ScreenColor",
				"Blue"
			});
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x0003F14E File Offset: 0x0003D34E
		private void rjButton35_Click(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"ScreenColor",
				"Green"
			});
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x0003F174 File Offset: 0x0003D374
		private void rjButton19_Click(object sender, EventArgs e)
		{
			if (this.rjButton19.Text == "Start")
			{
				this.rjButton19.Text = "Stop";
				this.client.Send(new object[]
				{
					"Smelt+"
				});
				return;
			}
			this.rjButton19.Text = "Start";
			this.client.Send(new object[]
			{
				"Smelt-"
			});
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x0003F1EC File Offset: 0x0003D3EC
		private void rjButton16_Click(object sender, EventArgs e)
		{
			if (this.rjButton16.Text == "Start")
			{
				this.rjButton16.Text = "Stop";
				this.client.Send(new object[]
				{
					"InvertSmelt+"
				});
				return;
			}
			this.rjButton16.Text = "Start";
			this.client.Send(new object[]
			{
				"InvertSmelt-"
			});
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x0003F264 File Offset: 0x0003D464
		private void rjButton18_Click(object sender, EventArgs e)
		{
			if (this.rjButton18.Text == "Start")
			{
				this.rjButton18.Text = "Stop";
				this.client.Send(new object[]
				{
					"VerticalWide+"
				});
				return;
			}
			this.rjButton18.Text = "Start";
			this.client.Send(new object[]
			{
				"VerticalWide-"
			});
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x0003F2DC File Offset: 0x0003D4DC
		private void rjButton17_Click(object sender, EventArgs e)
		{
			if (this.rjButton17.Text == "Start")
			{
				this.rjButton17.Text = "Stop";
				this.client.Send(new object[]
				{
					"Wide+"
				});
				return;
			}
			this.rjButton17.Text = "Start";
			this.client.Send(new object[]
			{
				"Wide-"
			});
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x0003F354 File Offset: 0x0003D554
		private void rjButton23_Click(object sender, EventArgs e)
		{
			if (this.rjButton23.Text == "Start")
			{
				this.rjButton23.Text = "Stop";
				this.client.Send(new object[]
				{
					"Train1+"
				});
				return;
			}
			this.rjButton23.Text = "Start";
			this.client.Send(new object[]
			{
				"Train1-"
			});
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x0003F3CC File Offset: 0x0003D5CC
		private void rjButton22_Click(object sender, EventArgs e)
		{
			if (this.rjButton22.Text == "Start")
			{
				this.rjButton22.Text = "Stop";
				this.client.Send(new object[]
				{
					"Train2+"
				});
				return;
			}
			this.rjButton22.Text = "Start";
			this.client.Send(new object[]
			{
				"Train2-"
			});
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x0003F444 File Offset: 0x0003D644
		private void rjButton21_Click(object sender, EventArgs e)
		{
			if (this.rjButton21.Text == "Start")
			{
				this.rjButton21.Text = "Stop";
				this.client.Send(new object[]
				{
					"Train3+"
				});
				return;
			}
			this.rjButton21.Text = "Start";
			this.client.Send(new object[]
			{
				"Train3-"
			});
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x0003F4BC File Offset: 0x0003D6BC
		private void rjButton20_Click(object sender, EventArgs e)
		{
			if (this.rjButton20.Text == "Start")
			{
				this.rjButton20.Text = "Stop";
				this.client.Send(new object[]
				{
					"Rgbtrain+"
				});
				return;
			}
			this.rjButton20.Text = "Start";
			this.client.Send(new object[]
			{
				"Rgbtrain-"
			});
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x0003F534 File Offset: 0x0003D734
		private void rjButton27_Click(object sender, EventArgs e)
		{
			if (this.rjButton27.Text == "Start")
			{
				this.rjButton27.Text = "Stop";
				this.client.Send(new object[]
				{
					"Sinewaves+"
				});
				return;
			}
			this.rjButton27.Text = "Start";
			this.client.Send(new object[]
			{
				"Sinewaves-"
			});
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x0003F5AC File Offset: 0x0003D7AC
		private void rjButton26_Click(object sender, EventArgs e)
		{
			if (this.rjButton26.Text == "Start")
			{
				this.rjButton26.Text = "Stop";
				this.client.Send(new object[]
				{
					"Shake+"
				});
				return;
			}
			this.rjButton26.Text = "Start";
			this.client.Send(new object[]
			{
				"Shake-"
			});
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x0003F624 File Offset: 0x0003D824
		private void rjButton25_Click(object sender, EventArgs e)
		{
			if (this.rjButton25.Text == "Start")
			{
				this.rjButton25.Text = "Stop";
				this.client.Send(new object[]
				{
					"Setpixel+"
				});
				return;
			}
			this.rjButton25.Text = "Start";
			this.client.Send(new object[]
			{
				"Setpixel-"
			});
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x0003F69C File Offset: 0x0003D89C
		private void rjButton24_Click(object sender, EventArgs e)
		{
			if (this.rjButton24.Text == "Start")
			{
				this.rjButton24.Text = "Stop";
				this.client.Send(new object[]
				{
					"DumpVD+"
				});
				return;
			}
			this.rjButton24.Text = "Start";
			this.client.Send(new object[]
			{
				"DumpVD-"
			});
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x0003F714 File Offset: 0x0003D914
		private void rjButton31_Click(object sender, EventArgs e)
		{
			if (this.rjButton31.Text == "Start")
			{
				this.rjButton31.Text = "Stop";
				this.client.Send(new object[]
				{
					"Dark+"
				});
				return;
			}
			this.rjButton31.Text = "Start";
			this.client.Send(new object[]
			{
				"Dark-"
			});
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x0003F78C File Offset: 0x0003D98C
		private void rjButton30_Click(object sender, EventArgs e)
		{
			if (this.rjButton30.Text == "Start")
			{
				this.rjButton30.Text = "Stop";
				this.client.Send(new object[]
				{
					"Stripes+"
				});
				return;
			}
			this.rjButton30.Text = "Start";
			this.client.Send(new object[]
			{
				"Stripes-"
			});
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0003F804 File Offset: 0x0003DA04
		private void rjButton29_Click(object sender, EventArgs e)
		{
			if (this.rjButton29.Text == "Start")
			{
				this.rjButton29.Text = "Stop";
				this.client.Send(new object[]
				{
					"Tunnel+"
				});
				return;
			}
			this.rjButton29.Text = "Start";
			this.client.Send(new object[]
			{
				"Tunnel-"
			});
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x0003F87C File Offset: 0x0003DA7C
		private void rjButton28_Click(object sender, EventArgs e)
		{
			if (this.rjButton28.Text == "Start")
			{
				this.rjButton28.Text = "Stop";
				this.client.Send(new object[]
				{
					"InvertColor+"
				});
				return;
			}
			this.rjButton28.Text = "Start";
			this.client.Send(new object[]
			{
				"InvertColor-"
			});
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x0003F8F4 File Offset: 0x0003DAF4
		private void rjButton44_Click(object sender, EventArgs e)
		{
			if (!(this.rjButton28.Text == "Spam"))
			{
				this.rjButton28.Text = "Spam";
				this.client.Send(new object[]
				{
					"MessageBoxSpam-"
				});
				return;
			}
			if (string.IsNullOrEmpty(this.rjTextBox2.Texts) || string.IsNullOrEmpty(this.rjTextBox3.Texts) || this.rjComboBox1.SelectedIndex == -1 || this.rjComboBox3.SelectedIndex == -1)
			{
				return;
			}
			this.rjButton28.Text = "Stop";
			this.client.Send(new object[]
			{
				"MessageBoxSpam+",
				this.rjTextBox3.Texts,
				this.rjTextBox2.Texts,
				this.rjComboBox1.SelectedIndex,
				this.rjComboBox3.SelectedIndex * 16
			});
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x0003F9F4 File Offset: 0x0003DBF4
		private void rjButton43_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.rjTextBox2.Texts) || string.IsNullOrEmpty(this.rjTextBox3.Texts) || this.rjComboBox1.SelectedIndex == -1 || this.rjComboBox3.SelectedIndex == -1)
			{
				return;
			}
			this.client.Send(new object[]
			{
				"MessageBox",
				this.rjTextBox3.Texts,
				this.rjTextBox2.Texts,
				this.rjComboBox1.SelectedIndex,
				this.rjComboBox3.SelectedIndex * 16
			});
		}

		// Token: 0x060004EE RID: 1262 RVA: 0x0003FAA0 File Offset: 0x0003DCA0
		private void rjButton45_Click(object sender, EventArgs e)
		{
			if (this.rjButton45.Text == "Start")
			{
				this.rjButton45.Text = "Stop";
				this.client.Send(new object[]
				{
					"FuckScreen+"
				});
				return;
			}
			this.rjButton45.Text = "Start";
			this.client.Send(new object[]
			{
				"FuckScreen-"
			});
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x0003FB18 File Offset: 0x0003DD18
		private void rjButton46_Click(object sender, EventArgs e)
		{
			if (this.rjButton46.Text == "Start")
			{
				this.rjButton46.Text = "Stop";
				this.client.Send(new object[]
				{
					"Led+"
				});
				return;
			}
			this.rjButton46.Text = "Start";
			this.client.Send(new object[]
			{
				"Led-"
			});
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x0003FB90 File Offset: 0x0003DD90
		private void rjButton47_Click(object sender, EventArgs e)
		{
			if (this.rjButton47.Text == "Start")
			{
				this.rjButton47.Text = "Stop";
				this.client.Send(new object[]
				{
					"Keyboard+"
				});
				return;
			}
			this.rjButton47.Text = "Start";
			this.client.Send(new object[]
			{
				"Keyboard-"
			});
		}

		// Token: 0x04000334 RID: 820
		public Clients client;

		// Token: 0x04000335 RID: 821
		public Clients parrent;
	}
}

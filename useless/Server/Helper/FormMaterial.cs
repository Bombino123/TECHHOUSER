using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using Server.Data;

namespace Server.Helper
{
	// Token: 0x02000070 RID: 112
	public partial class FormMaterial : MaterialForm
	{
		// Token: 0x0600025C RID: 604 RVA: 0x00023114 File Offset: 0x00021314
		public FormMaterial()
		{
			this.InitializeComponent();
			MaterialSkinManager instance = MaterialSkinManager.Instance;
			instance.ColorSchemeChanged += delegate(object sender)
			{
				this.Refresh();
			};
			if (File.Exists("local\\Settings.json"))
			{
				FormMaterial.GetColorScheme(JsonConvert.DeserializeObject<Settings>(File.ReadAllText("local\\Settings.json")).Style, instance);
			}
			else
			{
				FormMaterial.GetColorScheme(Randomizer.random.Next(15), instance);
			}
			instance.Theme = MaterialSkinManager.Themes.LIGHT;
		}

		// Token: 0x0600025D RID: 605 RVA: 0x00023188 File Offset: 0x00021388
		public static void GetColorScheme(int index, MaterialSkinManager materialSkinManager)
		{
			switch (index % 16)
			{
			case 0:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(9315498);
				break;
			case 1:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(2001125);
				break;
			case 2:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(15022389);
				break;
			case 3:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(4431943);
				break;
			case 4:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(16485376);
				break;
			case 5:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(16635957);
				break;
			case 6:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(6174129);
				break;
			case 7:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(35195);
				break;
			case 8:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(44225);
				break;
			case 9:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(236517);
				break;
			case 10:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(12634675);
				break;
			case 11:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(3754411);
				break;
			case 12:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(16011550);
				break;
			case 13:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(16757504);
				break;
			case 14:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(14162784);
				break;
			case 15:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(8172354);
				break;
			default:
				FormMaterial.PrimaryColor = FormMaterial.ToColor(9315498);
				break;
			}
			switch (index % 16)
			{
			case 0:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Purple600, Primary.Purple700, Primary.Purple800, Accent.Purple200, TextShade.WHITE);
				return;
			case 1:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue600, Primary.Blue700, Primary.Blue800, Accent.Blue200, TextShade.WHITE);
				return;
			case 2:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Red600, Primary.Red700, Primary.Red800, Accent.Red200, TextShade.WHITE);
				return;
			case 3:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Green600, Primary.Green700, Primary.Green800, Accent.Green200, TextShade.WHITE);
				return;
			case 4:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Orange600, Primary.Orange700, Primary.Orange800, Accent.Orange200, TextShade.WHITE);
				return;
			case 5:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Yellow600, Primary.Yellow700, Primary.Yellow800, Accent.Yellow200, TextShade.WHITE);
				return;
			case 6:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.DeepPurple600, Primary.DeepPurple700, Primary.DeepPurple800, Accent.DeepPurple200, TextShade.WHITE);
				return;
			case 7:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Teal600, Primary.Teal700, Primary.Teal800, Accent.Teal200, TextShade.WHITE);
				return;
			case 8:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Cyan600, Primary.Cyan700, Primary.Cyan800, Accent.Cyan200, TextShade.WHITE);
				return;
			case 9:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.LightBlue600, Primary.LightBlue700, Primary.LightBlue800, Accent.LightBlue200, TextShade.WHITE);
				return;
			case 10:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Lime600, Primary.Lime700, Primary.Lime800, Accent.Lime200, TextShade.WHITE);
				return;
			case 11:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Indigo600, Primary.Indigo700, Primary.Indigo800, Accent.Indigo200, TextShade.WHITE);
				return;
			case 12:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.DeepOrange600, Primary.DeepOrange700, Primary.DeepOrange800, Accent.DeepOrange200, TextShade.WHITE);
				return;
			case 13:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Amber600, Primary.Amber700, Primary.Amber800, Accent.Amber200, TextShade.WHITE);
				return;
			case 14:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Pink600, Primary.Pink700, Primary.Pink800, Accent.Pink200, TextShade.WHITE);
				return;
			case 15:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.LightGreen600, Primary.LightGreen700, Primary.LightGreen800, Accent.LightGreen200, TextShade.WHITE);
				return;
			default:
				materialSkinManager.ColorScheme = new ColorScheme(Primary.Purple600, Primary.Purple700, Primary.Purple800, Accent.Purple200, TextShade.WHITE);
				return;
			}
		}

		// Token: 0x0600025E RID: 606 RVA: 0x000235E3 File Offset: 0x000217E3
		private static Color ToColor(int argb)
		{
			return Color.FromArgb((argb & 16711680) >> 16, (argb & 65280) >> 8, argb & 255);
		}

		// Token: 0x04000173 RID: 371
		public static Color PrimaryColor;
	}
}

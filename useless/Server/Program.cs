using System;
using System.Windows.Forms;

namespace Server
{
	// Token: 0x0200003A RID: 58
	internal static class Program
	{
		// Token: 0x060001A3 RID: 419 RVA: 0x0001B56E File Offset: 0x0001976E
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Program.form = new Form1();
			Application.Run(Program.form);
		}

		// Token: 0x0400014B RID: 331
		public static Form1 form;
	}
}

using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.CtrlFlow
{
	// Token: 0x02000029 RID: 41
	public class Block
	{
		// Token: 0x060000A8 RID: 168 RVA: 0x00008B78 File Offset: 0x00006D78
		public Block()
		{
			this.Instructions = new List<Instruction>();
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x00008B8B File Offset: 0x00006D8B
		// (set) Token: 0x060000AA RID: 170 RVA: 0x00008B93 File Offset: 0x00006D93
		public List<Instruction> Instructions { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x060000AB RID: 171 RVA: 0x00008B9C File Offset: 0x00006D9C
		// (set) Token: 0x060000AC RID: 172 RVA: 0x00008BA4 File Offset: 0x00006DA4
		public int Number { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x060000AD RID: 173 RVA: 0x00008BAD File Offset: 0x00006DAD
		// (set) Token: 0x060000AE RID: 174 RVA: 0x00008BB5 File Offset: 0x00006DB5
		public int SubRand { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x060000AF RID: 175 RVA: 0x00008BBE File Offset: 0x00006DBE
		// (set) Token: 0x060000B0 RID: 176 RVA: 0x00008BC6 File Offset: 0x00006DC6
		public int PlusRand { get; set; }
	}
}

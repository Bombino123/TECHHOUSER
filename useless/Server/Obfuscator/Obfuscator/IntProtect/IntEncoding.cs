using System;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.IntProtect
{
	// Token: 0x02000026 RID: 38
	internal static class IntEncoding
	{
		// Token: 0x06000099 RID: 153 RVA: 0x0000802C File Offset: 0x0000622C
		public static int Next(int minValue = 0, int maxValue = 2147483647)
		{
			if (minValue >= maxValue)
			{
				throw new ArgumentOutOfRangeException("minValue");
			}
			long num = (long)maxValue - (long)minValue;
			long num2 = (long)(unchecked((ulong)-1) / (ulong)num * (ulong)num);
			uint num3;
			do
			{
				num3 = IntEncoding.RandomUInt();
			}
			while ((ulong)num3 >= (ulong)num2);
			return (int)((long)minValue + (long)((ulong)num3 % (ulong)num));
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00008068 File Offset: 0x00006268
		public static string GenerateRandomString(int length)
		{
			byte[] bytes = IntEncoding.RandomBytes(length);
			return Encoding.UTF7.GetString(bytes).Replace("\0", ".").Replace("\n", ".").Replace("\r", ".");
		}

		// Token: 0x0600009B RID: 155 RVA: 0x000080B4 File Offset: 0x000062B4
		public static int GetRandomStringLength()
		{
			return IntEncoding.Next(30, 120);
		}

		// Token: 0x0600009C RID: 156 RVA: 0x000080BF File Offset: 0x000062BF
		public static int GetRandomInt32()
		{
			return BitConverter.ToInt32(IntEncoding.RandomBytes(4), 0);
		}

		// Token: 0x0600009D RID: 157 RVA: 0x000080CD File Offset: 0x000062CD
		private static uint RandomUInt()
		{
			return BitConverter.ToUInt32(IntEncoding.RandomBytes(4), 0);
		}

		// Token: 0x0600009E RID: 158 RVA: 0x000080DC File Offset: 0x000062DC
		private static byte[] RandomBytes(int bytes)
		{
			byte[] array = new byte[bytes];
			IntEncoding.csp.GetBytes(array);
			return array;
		}

		// Token: 0x0600009F RID: 159 RVA: 0x000080FC File Offset: 0x000062FC
		public static void Execute(ModuleDef moduleDef)
		{
			IMethod method = moduleDef.Import(typeof(Math).GetMethod("Abs", new Type[]
			{
				typeof(int)
			}));
			IMethod method2 = moduleDef.Import(typeof(Math).GetMethod("Min", new Type[]
			{
				typeof(int),
				typeof(int)
			}));
			foreach (TypeDef typeDef in moduleDef.Types)
			{
				if (!typeDef.IsGlobalModuleType)
				{
					foreach (MethodDef methodDef in typeDef.Methods)
					{
						if (methodDef.HasBody)
						{
							for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
							{
								object operand = methodDef.Body.Instructions[i].Operand;
								if (operand is int)
								{
									int num = (int)operand;
									if (num > 0)
									{
										methodDef.Body.Instructions.Insert(i + 1, OpCodes.Call.ToInstruction(method));
										int num2 = IntEncoding.Next(8, IntEncoding.GetRandomStringLength());
										if (num2 % 2 != 0)
										{
											num2++;
										}
										for (int j = 0; j < num2; j++)
										{
											methodDef.Body.Instructions.Insert(i + j + 1, Instruction.Create(OpCodes.Neg));
										}
										if (num < 2147483647)
										{
											methodDef.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(int.MaxValue));
											methodDef.Body.Instructions.Insert(i + 2, OpCodes.Call.ToInstruction(method2));
										}
										i += num2 + 2;
									}
								}
							}
							methodDef.Body.SimplifyBranches();
						}
					}
				}
			}
		}

		// Token: 0x0400001A RID: 26
		private static readonly RandomNumberGenerator csp = RandomNumberGenerator.Create();
	}
}

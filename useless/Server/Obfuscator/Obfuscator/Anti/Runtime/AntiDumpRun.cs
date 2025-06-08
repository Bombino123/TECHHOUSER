using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Obfuscator.Obfuscator.Anti.Runtime
{
	// Token: 0x02000035 RID: 53
	internal class AntiDumpRun
	{
		// Token: 0x060000D0 RID: 208 RVA: 0x0000A4DC File Offset: 0x000086DC
		private unsafe static void CopyBlock(void* destination, void* source, uint byteCount)
		{
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x0000A4DE File Offset: 0x000086DE
		private unsafe static void InitBlock(void* startAddress, byte value, uint byteCount)
		{
		}

		// Token: 0x060000D2 RID: 210
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, [MarshalAs(UnmanagedType.U4)] AntiDumpRun.MemoryProtection flNewProtect, [MarshalAs(UnmanagedType.U4)] out AntiDumpRun.MemoryProtection lpflOldProtect);

		// Token: 0x060000D3 RID: 211 RVA: 0x0000A4E0 File Offset: 0x000086E0
		private unsafe static void Initialize()
		{
			Module module = typeof(AntiDumpRun).Module;
			byte* ptr = (byte*)((void*)Marshal.GetHINSTANCE(module));
			byte* ptr2 = ptr + 60;
			ptr2 = ptr + *(uint*)ptr2;
			ptr2 += 6;
			ushort num = *(ushort*)ptr2;
			ptr2 += 14;
			ushort num2 = *(ushort*)ptr2;
			ptr2 = ptr2 + 4 + num2;
			byte* ptr3 = stackalloc byte[11];
			AntiDumpRun.MemoryProtection memoryProtection;
			if (module.FullyQualifiedName[0] != '<')
			{
				byte* ptr4 = ptr + *(uint*)(ptr2 - 16);
				if (*(uint*)(ptr2 - 120) != 0U)
				{
					byte* ptr5 = ptr + *(uint*)(ptr2 - 120);
					byte* ptr6 = ptr + *(uint*)ptr5;
					byte* ptr7 = ptr + *(uint*)(ptr5 + 12);
					byte* ptr8 = ptr + *(uint*)ptr6 + 2;
					AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr7), 11U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
					*(int*)ptr3 = 1818522734;
					*(int*)(ptr3 + 4) = 1818504812;
					*(short*)(ptr3 + 8) = 108;
					ptr3[10] = 0;
					AntiDumpRun.CopyBlock((void*)ptr7, (void*)ptr3, 11U);
					AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr8), 11U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
					*(int*)ptr3 = 1866691662;
					*(int*)(ptr3 + 4) = 1852404846;
					*(short*)(ptr3 + 8) = 25973;
					ptr3[10] = 0;
					AntiDumpRun.CopyBlock((void*)ptr8, (void*)ptr3, 11U);
				}
				for (int i = 0; i < (int)num; i++)
				{
					AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr2), 8U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
					AntiDumpRun.InitBlock((void*)ptr2, 0, 8U);
					ptr2 += 40;
				}
				AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr4), 72U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
				byte* ptr9 = ptr + *(uint*)(ptr4 + 8);
				AntiDumpRun.InitBlock((void*)ptr4, 0, 16U);
				AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr9), 4U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
				*(int*)ptr9 = 0;
				ptr9 += 12;
				ptr9 += *(uint*)ptr9;
				ptr9 = (byte*)((long)ptr9 + 7L & -4L);
				ptr9 += 2;
				ushort num3 = (ushort)(*ptr9);
				ptr9 += 2;
				for (int j = 0; j < (int)num3; j++)
				{
					AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr9), 8U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
					ptr9 += 4;
					ptr9 += 4;
					for (int k = 0; k < 8; k++)
					{
						AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr9), 4U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
						*ptr9 = 0;
						ptr9++;
						if (*ptr9 == 0)
						{
							ptr9 += 3;
							break;
						}
						*ptr9 = 0;
						ptr9++;
						if (*ptr9 == 0)
						{
							ptr9 += 2;
							break;
						}
						*ptr9 = 0;
						ptr9++;
						if (*ptr9 == 0)
						{
							ptr9++;
							break;
						}
						*ptr9 = 0;
						ptr9++;
					}
				}
				return;
			}
			uint num4 = *(uint*)(ptr2 - 16);
			uint num5 = *(uint*)(ptr2 - 120);
			uint[] array = new uint[(int)num];
			uint[] array2 = new uint[(int)num];
			uint[] array3 = new uint[(int)num];
			for (int l = 0; l < (int)num; l++)
			{
				AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr2), 8U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
				Marshal.Copy(new byte[8], 0, (IntPtr)((void*)ptr2), 8);
				array[l] = *(uint*)(ptr2 + 12);
				array2[l] = *(uint*)(ptr2 + 8);
				array3[l] = *(uint*)(ptr2 + 20);
				ptr2 += 40;
			}
			if (num5 != 0U)
			{
				for (int m = 0; m < (int)num; m++)
				{
					if (array[m] <= num5 && num5 < array[m] + array2[m])
					{
						num5 = num5 - array[m] + array3[m];
						break;
					}
				}
				byte* ptr10 = ptr + num5;
				uint num6 = *(uint*)ptr10;
				for (int n = 0; n < (int)num; n++)
				{
					if (array[n] <= num6 && num6 < array[n] + array2[n])
					{
						num6 = num6 - array[n] + array3[n];
						break;
					}
				}
				byte* ptr11 = ptr + num6;
				uint num7 = *(uint*)(ptr10 + 12);
				for (int num8 = 0; num8 < (int)num; num8++)
				{
					if (array[num8] <= num7 && num7 < array[num8] + array2[num8])
					{
						num7 = num7 - array[num8] + array3[num8];
						break;
					}
				}
				uint num9 = *(uint*)ptr11 + 2U;
				for (int num10 = 0; num10 < (int)num; num10++)
				{
					if (array[num10] <= num9 && num9 < array[num10] + array2[num10])
					{
						num9 = num9 - array[num10] + array3[num10];
						break;
					}
				}
				AntiDumpRun.VirtualProtect(new IntPtr((void*)(ptr + num7)), 11U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
				*(int*)ptr3 = 1818522734;
				*(int*)(ptr3 + 4) = 1818504812;
				*(short*)(ptr3 + 8) = 108;
				ptr3[10] = 0;
				AntiDumpRun.CopyBlock((void*)(ptr + num7), (void*)ptr3, 11U);
				AntiDumpRun.VirtualProtect(new IntPtr((void*)(ptr + num9)), 11U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
				*(int*)ptr3 = 1866691662;
				*(int*)(ptr3 + 4) = 1852404846;
				*(short*)(ptr3 + 8) = 25973;
				ptr3[10] = 0;
				AntiDumpRun.CopyBlock((void*)(ptr + num9), (void*)ptr3, 11U);
			}
			for (int num11 = 0; num11 < (int)num; num11++)
			{
				if (array[num11] <= num4 && num4 < array[num11] + array2[num11])
				{
					num4 = num4 - array[num11] + array3[num11];
					break;
				}
			}
			byte* ptr12 = ptr + num4;
			AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr12), 72U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
			uint num12 = *(uint*)(ptr12 + 8);
			for (int num13 = 0; num13 < (int)num; num13++)
			{
				if (array[num13] <= num12 && num12 < array[num13] + array2[num13])
				{
					num12 = num12 - array[num13] + array3[num13];
					break;
				}
			}
			AntiDumpRun.InitBlock((void*)ptr12, 0, 16U);
			byte* ptr13 = ptr + num12;
			AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr13), 4U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
			*(int*)ptr13 = 0;
			ptr13 += 12;
			ptr13 += *(uint*)ptr13;
			ptr13 = (byte*)((long)ptr13 + 7L & -4L);
			ptr13 += 2;
			ushort num14 = (ushort)(*ptr13);
			ptr13 += 2;
			for (int num15 = 0; num15 < (int)num14; num15++)
			{
				AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr13), 8U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
				ptr13 += 4;
				ptr13 += 4;
				for (int num16 = 0; num16 < 8; num16++)
				{
					AntiDumpRun.VirtualProtect(new IntPtr((void*)ptr13), 4U, AntiDumpRun.MemoryProtection.ExecuteReadWrite, out memoryProtection);
					*ptr13 = 0;
					ptr13++;
					if (*ptr13 == 0)
					{
						ptr13 += 3;
						break;
					}
					*ptr13 = 0;
					ptr13++;
					if (*ptr13 == 0)
					{
						ptr13 += 2;
						break;
					}
					*ptr13 = 0;
					ptr13++;
					if (*ptr13 == 0)
					{
						ptr13++;
						break;
					}
					*ptr13 = 0;
					ptr13++;
				}
			}
		}

		// Token: 0x020000E1 RID: 225
		internal enum MemoryProtection
		{
			// Token: 0x040005BC RID: 1468
			ExecuteReadWrite = 64
		}
	}
}

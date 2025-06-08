using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Helper
{
	// Token: 0x02000075 RID: 117
	public class ByteConverter
	{
		// Token: 0x06000279 RID: 633 RVA: 0x00023CBB File Offset: 0x00021EBB
		public static byte[] GetBytes(int value)
		{
			return BitConverter.GetBytes(value);
		}

		// Token: 0x0600027A RID: 634 RVA: 0x00023CC3 File Offset: 0x00021EC3
		public static byte[] GetBytes(long value)
		{
			return BitConverter.GetBytes(value);
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00023CCB File Offset: 0x00021ECB
		public static byte[] GetBytes(uint value)
		{
			return BitConverter.GetBytes(value);
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00023CD3 File Offset: 0x00021ED3
		public static byte[] GetBytes(ulong value)
		{
			return BitConverter.GetBytes(value);
		}

		// Token: 0x0600027D RID: 637 RVA: 0x00023CDB File Offset: 0x00021EDB
		public static byte[] GetBytes(string value)
		{
			return ByteConverter.StringToBytes(value);
		}

		// Token: 0x0600027E RID: 638 RVA: 0x00023CE3 File Offset: 0x00021EE3
		public static byte[] GetBytes(string[] value)
		{
			return ByteConverter.StringArrayToBytes(value);
		}

		// Token: 0x0600027F RID: 639 RVA: 0x00023CEB File Offset: 0x00021EEB
		public static int ToInt32(byte[] bytes)
		{
			return BitConverter.ToInt32(bytes, 0);
		}

		// Token: 0x06000280 RID: 640 RVA: 0x00023CF4 File Offset: 0x00021EF4
		public static long ToInt64(byte[] bytes)
		{
			return BitConverter.ToInt64(bytes, 0);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x00023CFD File Offset: 0x00021EFD
		public static uint ToUInt32(byte[] bytes)
		{
			return BitConverter.ToUInt32(bytes, 0);
		}

		// Token: 0x06000282 RID: 642 RVA: 0x00023D06 File Offset: 0x00021F06
		public static ulong ToUInt64(byte[] bytes)
		{
			return BitConverter.ToUInt64(bytes, 0);
		}

		// Token: 0x06000283 RID: 643 RVA: 0x00023D0F File Offset: 0x00021F0F
		public static string ToString(byte[] bytes)
		{
			return ByteConverter.BytesToString(bytes);
		}

		// Token: 0x06000284 RID: 644 RVA: 0x00023D17 File Offset: 0x00021F17
		public static string[] ToStringArray(byte[] bytes)
		{
			return ByteConverter.BytesToStringArray(bytes);
		}

		// Token: 0x06000285 RID: 645 RVA: 0x00023D1F File Offset: 0x00021F1F
		private static byte[] GetNullBytes()
		{
			return new byte[]
			{
				ByteConverter.NULL_BYTE,
				ByteConverter.NULL_BYTE
			};
		}

		// Token: 0x06000286 RID: 646 RVA: 0x00023D38 File Offset: 0x00021F38
		private static byte[] StringToBytes(string value)
		{
			byte[] array = new byte[value.Length * 2];
			Buffer.BlockCopy(value.ToCharArray(), 0, array, 0, array.Length);
			return array;
		}

		// Token: 0x06000287 RID: 647 RVA: 0x00023D68 File Offset: 0x00021F68
		private static byte[] StringArrayToBytes(string[] strings)
		{
			List<byte> list = new List<byte>();
			foreach (string value in strings)
			{
				list.AddRange(ByteConverter.StringToBytes(value));
				list.AddRange(ByteConverter.GetNullBytes());
			}
			return list.ToArray();
		}

		// Token: 0x06000288 RID: 648 RVA: 0x00023DAC File Offset: 0x00021FAC
		private static string BytesToString(byte[] bytes)
		{
			char[] array = new char[(int)Math.Ceiling((double)((float)bytes.Length / 2f))];
			Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
			return new string(array);
		}

		// Token: 0x06000289 RID: 649 RVA: 0x00023DE4 File Offset: 0x00021FE4
		private static string[] BytesToStringArray(byte[] bytes)
		{
			List<string> list = new List<string>();
			int i = 0;
			StringBuilder stringBuilder = new StringBuilder(bytes.Length);
			while (i < bytes.Length)
			{
				int num = 0;
				while (i < bytes.Length && num < 3)
				{
					if (bytes[i] == ByteConverter.NULL_BYTE)
					{
						num++;
					}
					else
					{
						stringBuilder.Append(Convert.ToChar(bytes[i]));
						num = 0;
					}
					i++;
				}
				list.Add(stringBuilder.ToString());
				stringBuilder.Clear();
			}
			return list.ToArray();
		}

		// Token: 0x04000178 RID: 376
		private static byte NULL_BYTE;
	}
}

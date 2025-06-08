using System;
using System.IO;
using System.Text;

namespace Leb128
{
	// Token: 0x02000018 RID: 24
	internal class LEB128Coding
	{
		// Token: 0x0600007A RID: 122 RVA: 0x00006318 File Offset: 0x00004518
		public static void WriteLeb(Stream stream, byte[] buffer)
		{
			byte[] bytes = BitConverter.GetBytes(buffer.Length);
			stream.Write(bytes, 0, bytes.Length);
			stream.Write(buffer, 0, buffer.Length);
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00006348 File Offset: 0x00004548
		public static void WriteLeb(Stream stream, string data)
		{
			LEB128Coding.WriteLeb(stream, Encoding.UTF8.GetBytes(data));
		}

		// Token: 0x0600007C RID: 124 RVA: 0x0000635C File Offset: 0x0000455C
		public static void WriteLeb(Stream stream, bool data)
		{
			stream.WriteByte(Convert.ToByte(data));
		}

		// Token: 0x0600007D RID: 125 RVA: 0x0000636C File Offset: 0x0000456C
		public static void WriteLeb(Stream stream, byte data)
		{
			stream.WriteByte(data);
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00006378 File Offset: 0x00004578
		public static void WriteLeb(Stream stream, short data)
		{
			byte[] bytes = BitConverter.GetBytes(data);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x0600007F RID: 127 RVA: 0x0000639C File Offset: 0x0000459C
		public static void WriteLeb(Stream stream, int data)
		{
			byte[] bytes = BitConverter.GetBytes(data);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000080 RID: 128 RVA: 0x000063C0 File Offset: 0x000045C0
		public static void WriteLeb(Stream stream, long data)
		{
			byte[] bytes = BitConverter.GetBytes(data);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000081 RID: 129 RVA: 0x000063E4 File Offset: 0x000045E4
		public static void WriteLeb(Stream stream, float data)
		{
			byte[] bytes = BitConverter.GetBytes(data);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00006408 File Offset: 0x00004608
		public static void WriteLeb(Stream stream, double data)
		{
			byte[] bytes = BitConverter.GetBytes(data);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000083 RID: 131 RVA: 0x0000642C File Offset: 0x0000462C
		public static void WriteLeb(Stream stream, ushort data)
		{
			byte[] bytes = BitConverter.GetBytes(data);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00006450 File Offset: 0x00004650
		public static void WriteLeb(Stream stream, uint data)
		{
			byte[] bytes = BitConverter.GetBytes(data);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00006474 File Offset: 0x00004674
		public static void WriteLeb(Stream stream, ulong data)
		{
			byte[] bytes = BitConverter.GetBytes(data);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00006498 File Offset: 0x00004698
		public static byte[] ReadLebArray(Stream stream)
		{
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			int num = BitConverter.ToInt32(array, 0);
			byte[] array2 = new byte[num];
			stream.Read(array2, 0, num);
			return array2;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x000064D4 File Offset: 0x000046D4
		public static string ReadLebString(Stream stream)
		{
			return Encoding.UTF8.GetString(LEB128Coding.ReadLebArray(stream));
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000064E8 File Offset: 0x000046E8
		public static bool ReadLebBool(Stream stream)
		{
			return Convert.ToBoolean(stream.ReadByte());
		}

		// Token: 0x06000089 RID: 137 RVA: 0x000064F8 File Offset: 0x000046F8
		public static byte ReadLebByte(Stream stream)
		{
			return (byte)stream.ReadByte();
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00006504 File Offset: 0x00004704
		public static short ReadLebShort(Stream stream)
		{
			byte[] array = new byte[2];
			stream.Read(array, 0, array.Length);
			return BitConverter.ToInt16(array, 0);
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00006530 File Offset: 0x00004730
		public static int ReadLebInt(Stream stream)
		{
			byte[] array = new byte[4];
			stream.Read(array, 0, array.Length);
			return BitConverter.ToInt32(array, 0);
		}

		// Token: 0x0600008C RID: 140 RVA: 0x0000655C File Offset: 0x0000475C
		public static long ReadLebLong(Stream stream)
		{
			byte[] array = new byte[8];
			stream.Read(array, 0, array.Length);
			return BitConverter.ToInt64(array, 0);
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00006588 File Offset: 0x00004788
		public static float ReadLebFloat(Stream stream)
		{
			byte[] array = new byte[4];
			stream.Read(array, 0, array.Length);
			return BitConverter.ToSingle(array, 0);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x000065B4 File Offset: 0x000047B4
		public static double ReadLebDouble(Stream stream)
		{
			byte[] array = new byte[8];
			stream.Read(array, 0, array.Length);
			return BitConverter.ToDouble(array, 0);
		}

		// Token: 0x0600008F RID: 143 RVA: 0x000065E0 File Offset: 0x000047E0
		public static ushort ReadLebUshort(Stream stream)
		{
			byte[] array = new byte[2];
			stream.Read(array, 0, array.Length);
			return BitConverter.ToUInt16(array, 0);
		}

		// Token: 0x06000090 RID: 144 RVA: 0x0000660C File Offset: 0x0000480C
		public static uint ReadLebUint(Stream stream)
		{
			byte[] array = new byte[4];
			stream.Read(array, 0, array.Length);
			return BitConverter.ToUInt32(array, 0);
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00006638 File Offset: 0x00004838
		public static ulong ReadLebUlong(Stream stream)
		{
			byte[] array = new byte[8];
			stream.Read(array, 0, array.Length);
			return BitConverter.ToUInt64(array, 0);
		}
	}
}

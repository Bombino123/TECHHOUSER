using System;
using System.Collections.Generic;
using System.IO;

namespace Leb128
{
	// Token: 0x02000017 RID: 23
	public class LEB128
	{
		// Token: 0x06000077 RID: 119 RVA: 0x00005F14 File Offset: 0x00004114
		public static object[] Read(byte[] data)
		{
			List<object> list = new List<object>();
			object[] result;
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				int num;
				for (;;)
				{
					num = memoryStream.ReadByte();
					switch (num)
					{
					case -1:
						goto IL_164;
					case 0:
						list.Add(LEB128Coding.ReadLebString(memoryStream));
						continue;
					case 1:
						list.Add(LEB128Coding.ReadLebBool(memoryStream));
						continue;
					case 2:
						list.Add(LEB128Coding.ReadLebByte(memoryStream));
						continue;
					case 3:
						list.Add(LEB128Coding.ReadLebShort(memoryStream));
						continue;
					case 4:
						list.Add(LEB128Coding.ReadLebInt(memoryStream));
						continue;
					case 5:
						list.Add(LEB128Coding.ReadLebLong(memoryStream));
						continue;
					case 6:
						list.Add(LEB128Coding.ReadLebFloat(memoryStream));
						continue;
					case 7:
						list.Add(LEB128Coding.ReadLebDouble(memoryStream));
						continue;
					case 8:
						list.Add(LEB128Coding.ReadLebArray(memoryStream));
						continue;
					case 9:
						list.Add(LEB128Coding.ReadLebUshort(memoryStream));
						continue;
					case 10:
						list.Add(LEB128Coding.ReadLebUint(memoryStream));
						continue;
					case 11:
						list.Add(LEB128Coding.ReadLebUlong(memoryStream));
						continue;
					case 12:
						list.Add(LEB128.Read(LEB128Coding.ReadLebArray(memoryStream)));
						continue;
					}
					break;
				}
				throw new Exception(num.ToString());
				IL_164:
				result = list.ToArray();
			}
			return result;
		}

		// Token: 0x06000078 RID: 120 RVA: 0x000060C8 File Offset: 0x000042C8
		public static byte[] Write(object[] data)
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				foreach (object obj in data)
				{
					if (obj is string)
					{
						memoryStream.WriteByte(0);
						LEB128Coding.WriteLeb(memoryStream, (string)obj);
					}
					else if (obj is bool)
					{
						memoryStream.WriteByte(1);
						LEB128Coding.WriteLeb(memoryStream, (bool)obj);
					}
					else if (obj is byte)
					{
						memoryStream.WriteByte(2);
						LEB128Coding.WriteLeb(memoryStream, (byte)obj);
					}
					else if (obj is short)
					{
						memoryStream.WriteByte(3);
						LEB128Coding.WriteLeb(memoryStream, (short)obj);
					}
					else if (obj is int)
					{
						memoryStream.WriteByte(4);
						LEB128Coding.WriteLeb(memoryStream, (int)obj);
					}
					else if (obj is long)
					{
						memoryStream.WriteByte(5);
						LEB128Coding.WriteLeb(memoryStream, (long)obj);
					}
					else if (obj is float)
					{
						memoryStream.WriteByte(6);
						LEB128Coding.WriteLeb(memoryStream, (float)obj);
					}
					else if (obj is double)
					{
						memoryStream.WriteByte(7);
						LEB128Coding.WriteLeb(memoryStream, (double)obj);
					}
					else if (obj is byte[])
					{
						memoryStream.WriteByte(8);
						LEB128Coding.WriteLeb(memoryStream, (byte[])obj);
					}
					else if (obj is ushort)
					{
						memoryStream.WriteByte(9);
						LEB128Coding.WriteLeb(memoryStream, (ushort)obj);
					}
					else if (obj is uint)
					{
						memoryStream.WriteByte(10);
						LEB128Coding.WriteLeb(memoryStream, (uint)obj);
					}
					else if (obj is ulong)
					{
						memoryStream.WriteByte(11);
						LEB128Coding.WriteLeb(memoryStream, (ulong)obj);
					}
					else
					{
						if (!(obj is object[]))
						{
							throw new Exception(obj.GetType().Name);
						}
						memoryStream.WriteByte(12);
						LEB128Coding.WriteLeb(memoryStream, LEB128.Write((object[])obj));
					}
				}
				result = memoryStream.ToArray();
			}
			return result;
		}
	}
}

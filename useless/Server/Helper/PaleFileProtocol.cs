using System;
using System.Collections.Generic;
using System.IO;
using Leb128;

namespace Server.Helper
{
	// Token: 0x02000073 RID: 115
	internal class PaleFileProtocol
	{
		// Token: 0x0600026D RID: 621 RVA: 0x00023A48 File Offset: 0x00021C48
		public static void Unpack(string path, byte[] buff)
		{
			object[] array = LEB128.Read(buff);
			int i = 0;
			while (i < array.Length)
			{
				string path2 = array[i++] as string;
				byte[] bytes = array[i++] as byte[];
				try
				{
					if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(path, path2))))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(path, path2)));
					}
					File.WriteAllBytes(Path.Combine(path, path2), bytes);
				}
				catch
				{
				}
			}
			array = null;
			buff = null;
		}

		// Token: 0x0600026E RID: 622 RVA: 0x00023AD4 File Offset: 0x00021CD4
		public static byte[] Pack(string path)
		{
			List<object> list = new List<object>();
			foreach (string text in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
			{
				try
				{
					string item = text.Replace(path + "\\", "").Replace(path, "");
					byte[] item2 = File.ReadAllBytes(text);
					list.Add(item);
					list.Add(item2);
				}
				catch
				{
				}
			}
			return LEB128.Write(list.ToArray());
		}
	}
}

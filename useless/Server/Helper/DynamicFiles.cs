using System;
using System.IO;

namespace Server.Helper
{
	// Token: 0x0200006F RID: 111
	internal class DynamicFiles
	{
		// Token: 0x0600025A RID: 602 RVA: 0x000230A0 File Offset: 0x000212A0
		public static void Save(string path, object[] Dynamicfls)
		{
			int i = 0;
			while (i < Dynamicfls.Length)
			{
				object[] array = (object[])Dynamicfls[i++];
				try
				{
					string path2 = Path.Combine(path, (string)array[0]);
					byte[] bytes = (byte[])array[1];
					string directoryName = Path.GetDirectoryName(path2);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					File.WriteAllBytes(path2, bytes);
				}
				catch
				{
				}
			}
		}
	}
}

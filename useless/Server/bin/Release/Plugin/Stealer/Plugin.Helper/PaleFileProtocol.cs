using System.Collections.Generic;
using System.IO;
using Leb128;

namespace Plugin.Helper;

internal class PaleFileProtocol
{
	public static void Unpack(string path, byte[] buff)
	{
		object[] array = LEB128.Read(buff);
		int num = 0;
		while (num < array.Length)
		{
			string path2 = array[num++] as string;
			byte[] bytes = array[num++] as byte[];
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
			bytes = null;
			path2 = "";
		}
		array = null;
		buff = null;
	}

	public static byte[] Pack(string path)
	{
		List<object> list = new List<object>();
		string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
		foreach (string text in files)
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

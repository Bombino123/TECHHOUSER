using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Obfuscator.Obfuscator.Anti.Runtime
{
	// Token: 0x02000036 RID: 54
	internal class EofAntiTamper
	{
		// Token: 0x060000D5 RID: 213 RVA: 0x0000AAF8 File Offset: 0x00008CF8
		private static void Initializer()
		{
			string location = Assembly.GetExecutingAssembly().Location;
			Stream baseStream = new StreamReader(location).BaseStream;
			BinaryReader binaryReader = new BinaryReader(baseStream);
			string a = BitConverter.ToString(SHA256.Create().ComputeHash(binaryReader.ReadBytes(File.ReadAllBytes(location).Length - 32)));
			baseStream.Seek(-32L, SeekOrigin.End);
			string b = BitConverter.ToString(binaryReader.ReadBytes(32));
			if (a != b)
			{
				Process process = Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + Assembly.GetExecutingAssembly().Location + "\"")
				{
					WindowStyle = ProcessWindowStyle.Hidden
				});
				if (process != null)
				{
					process.Dispose();
				}
				Process.GetCurrentProcess().Kill();
			}
		}
	}
}

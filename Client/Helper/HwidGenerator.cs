using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Client.Helper
{
	// Token: 0x0200000B RID: 11
	internal class HwidGenerator
	{
		// Token: 0x06000034 RID: 52 RVA: 0x00003BB4 File Offset: 0x00001DB4
		public static string hwid()
		{
			if (SetRegistry.CheckValue(EncryptString.Decode("Hwid")))
			{
				return SetRegistry.GetValue(EncryptString.Decode("Hwid"));
			}
			HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider();
			byte[] buffer = Encoding.ASCII.GetBytes(HwidGenerator.Inf());
			buffer = hashAlgorithm.ComputeHash(buffer);
			return HwidGenerator.ByteToStr(buffer);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00003C0C File Offset: 0x00001E0C
		private static string ByteToStr(byte[] buffer)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in buffer)
			{
				stringBuilder.Append(b.ToString(EncryptString.Decode("x2")));
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003C58 File Offset: 0x00001E58
		private static string identifier(string wmiClass, string wmiProperty)
		{
			string text = "";
			foreach (ManagementBaseObject managementBaseObject in new ManagementClass(wmiClass).GetInstances())
			{
				ManagementObject managementObject = (ManagementObject)managementBaseObject;
				if (text == "")
				{
					try
					{
						text = managementObject[wmiProperty].ToString();
						break;
					}
					catch
					{
					}
				}
			}
			return text;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00003CEC File Offset: 0x00001EEC
		private static string Inf()
		{
			return string.Concat(new string[]
			{
				HwidGenerator.identifier(EncryptString.Decode("Win32_DiskDrive"), EncryptString.Decode("Model")),
				HwidGenerator.identifier(EncryptString.Decode("Win32_DiskDrive"), EncryptString.Decode("Manufacturer")),
				HwidGenerator.identifier(EncryptString.Decode("Win32_DiskDrive"), EncryptString.Decode("Name")),
				HwidGenerator.identifier(EncryptString.Decode("Win32_Processor"), EncryptString.Decode("Name")),
				Config.WindowsVersion,
				Config.Gpu,
				Config.DataInstall,
				Environment.ProcessorCount.ToString()
			});
		}
	}
}

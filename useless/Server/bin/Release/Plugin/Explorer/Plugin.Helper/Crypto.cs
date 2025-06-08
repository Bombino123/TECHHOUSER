using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Plugin.Helper;

internal class Crypto
{
	private static string password = "Leberium23";

	private static string salt1 = "lsk0dgjbsi";

	public static void EncryptFile(string filePath)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(salt1);
		byte[] bytes2 = Encoding.UTF8.GetBytes(password);
		string[] array = filePath.Split(new char[1] { ';' });
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
			using Aes aes = Aes.Create();
			Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(bytes2, bytes, 10000);
			aes.Key = rfc2898DeriveBytes.GetBytes(32);
			aes.IV = rfc2898DeriveBytes.GetBytes(16);
			using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			using FileStream stream = new FileStream(filePath + ".encrypted", FileMode.Create, FileAccess.Write);
			using CryptoStream destination = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write);
			fileStream.CopyTo(destination);
		}
		File.Delete(filePath);
	}

	public static void DecryptFile(string filePath)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(salt1);
		byte[] bytes2 = Encoding.UTF8.GetBytes(password);
		string[] array = filePath.Split(new char[1] { ';' });
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
			using (Aes aes = Aes.Create())
			{
				Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(bytes2, bytes, 10000);
				aes.Key = rfc2898DeriveBytes.GetBytes(32);
				aes.IV = rfc2898DeriveBytes.GetBytes(16);
				using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				using FileStream destination = new FileStream(filePath.Replace(".encrypted", ""), FileMode.Create, FileAccess.Write);
				using CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read);
				cryptoStream.CopyTo(destination);
			}
			File.Delete(filePath);
		}
	}
}

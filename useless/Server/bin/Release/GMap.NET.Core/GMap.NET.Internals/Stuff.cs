using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace GMap.NET.Internals;

internal class Stuff
{
	public static readonly Random Random = new Random();

	private static readonly string manifesto = "GMap.NET is great and Powerful, Free, cross platform, open source .NET control.";

	public static string EnumToString(Enum value)
	{
		DescriptionAttribute[] array = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
		if (array.Length == 0)
		{
			return value.ToString();
		}
		return array[0].Description;
	}

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetCursorPos(int x, int y);

	public static void Shuffle<T>(List<T> deck)
	{
		int count = deck.Count;
		for (int i = 0; i < count; i++)
		{
			int index = i + Random.Next(count - i);
			T value = deck[index];
			deck[index] = deck[i];
			deck[i] = value;
		}
	}

	public static MemoryStream CopyStream(Stream inputStream, bool seekOriginBegin)
	{
		byte[] buffer = new byte[32768];
		MemoryStream memoryStream = new MemoryStream();
		int count;
		while ((count = inputStream.Read(buffer, 0, 32768)) > 0)
		{
			memoryStream.Write(buffer, 0, count);
		}
		if (seekOriginBegin)
		{
			inputStream.Seek(0L, SeekOrigin.Begin);
		}
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return memoryStream;
	}

	public static bool IsRunningOnVistaOrLater()
	{
		OperatingSystem oSVersion = Environment.OSVersion;
		if (oSVersion.Platform == PlatformID.Win32NT)
		{
			Version version = oSVersion.Version;
			if (version.Major >= 6 && version.Minor >= 0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsRunningOnWin7OrLater()
	{
		OperatingSystem oSVersion = Environment.OSVersion;
		if (oSVersion.Platform == PlatformID.Win32NT)
		{
			Version version = oSVersion.Version;
			if (version.Major >= 6 && version.Minor > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static void RemoveInvalidPathSymbols(ref string url)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char oldChar in invalidFileNameChars)
		{
			url = url.Replace(oldChar, '_');
		}
	}

	private static string EncryptString(string message, string passphrase)
	{
		byte[] inArray;
		using (SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider())
		{
			byte[] array = sHA1CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
			Array.Resize(ref array, 16);
			using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
			tripleDESCryptoServiceProvider.Key = array;
			tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
			tripleDESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			try
			{
				using ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateEncryptor();
				inArray = cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
			}
			finally
			{
				tripleDESCryptoServiceProvider.Clear();
				sHA1CryptoServiceProvider.Clear();
			}
		}
		return Convert.ToBase64String(inArray);
	}

	private static string DecryptString(string message, string passphrase)
	{
		byte[] array3;
		using (SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider())
		{
			byte[] array = sHA1CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
			Array.Resize(ref array, 16);
			using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
			tripleDESCryptoServiceProvider.Key = array;
			tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
			tripleDESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
			byte[] array2 = Convert.FromBase64String(message);
			try
			{
				using ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor();
				array3 = cryptoTransform.TransformFinalBlock(array2, 0, array2.Length);
			}
			finally
			{
				tripleDESCryptoServiceProvider.Clear();
				sHA1CryptoServiceProvider.Clear();
			}
		}
		return Encoding.UTF8.GetString(array3, 0, array3.Length);
	}

	public static string EncryptString(string message)
	{
		return EncryptString(message, manifesto);
	}

	public static string GString(string message)
	{
		return DecryptString(message, manifesto);
	}
}

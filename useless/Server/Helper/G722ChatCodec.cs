using System;
using NAudio.Codecs;
using NAudio.Wave;

namespace Server.Helper
{
	// Token: 0x0200006B RID: 107
	public class G722ChatCodec
	{
		// Token: 0x0600020E RID: 526 RVA: 0x00021DFC File Offset: 0x0001FFFC
		public G722ChatCodec()
		{
			this.bitrate = 64000;
			this.encoderState = new G722CodecState(this.bitrate, G722Flags.None);
			this.decoderState = new G722CodecState(this.bitrate, G722Flags.None);
			this.codec = new G722Codec();
			this.RecordFormat = new WaveFormat(16000, 1);
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600020F RID: 527 RVA: 0x00021E5A File Offset: 0x0002005A
		public string Name
		{
			get
			{
				return "G.722 16kHz";
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000210 RID: 528 RVA: 0x00021E61 File Offset: 0x00020061
		public int BitsPerSecond
		{
			get
			{
				return this.bitrate;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000211 RID: 529 RVA: 0x00021E69 File Offset: 0x00020069
		public WaveFormat RecordFormat { get; }

		// Token: 0x06000212 RID: 530 RVA: 0x00021E74 File Offset: 0x00020074
		public byte[] Encode(byte[] data, int offset, int length)
		{
			if (offset != 0)
			{
				throw new ArgumentException("G722 does not yet support non-zero offsets");
			}
			WaveBuffer waveBuffer = new WaveBuffer(data);
			byte[] array = new byte[length / 4];
			this.codec.Encode(this.encoderState, array, waveBuffer.ShortBuffer, length / 2);
			return array;
		}

		// Token: 0x06000213 RID: 531 RVA: 0x00021EBC File Offset: 0x000200BC
		public byte[] Decode(byte[] data, int offset, int length)
		{
			if (offset != 0)
			{
				throw new ArgumentException("G722 does not yet support non-zero offsets");
			}
			byte[] array = new byte[length * 4];
			WaveBuffer waveBuffer = new WaveBuffer(array);
			this.codec.Decode(this.decoderState, waveBuffer.ShortBuffer, data, length);
			return array;
		}

		// Token: 0x06000214 RID: 532 RVA: 0x00021F00 File Offset: 0x00020100
		public void Dispose()
		{
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000215 RID: 533 RVA: 0x00021F02 File Offset: 0x00020102
		public bool IsAvailable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x04000152 RID: 338
		private readonly int bitrate;

		// Token: 0x04000153 RID: 339
		private readonly G722CodecState encoderState;

		// Token: 0x04000154 RID: 340
		private readonly G722CodecState decoderState;

		// Token: 0x04000155 RID: 341
		private readonly G722Codec codec;
	}
}

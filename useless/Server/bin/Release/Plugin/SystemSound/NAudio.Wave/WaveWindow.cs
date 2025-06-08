using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NAudio.Wave;

internal class WaveWindow : Form
{
	private WaveInterop.WaveCallback waveCallback;

	public WaveWindow(WaveInterop.WaveCallback waveCallback)
	{
		this.waveCallback = waveCallback;
	}

	protected override void WndProc(ref Message m)
	{
		WaveInterop.WaveMessage msg = (WaveInterop.WaveMessage)((Message)(ref m)).Msg;
		switch (msg)
		{
		case WaveInterop.WaveMessage.WaveOutDone:
		case WaveInterop.WaveMessage.WaveInData:
		{
			IntPtr wParam = ((Message)(ref m)).WParam;
			WaveHeader waveHeader = new WaveHeader();
			Marshal.PtrToStructure(((Message)(ref m)).LParam, (object)waveHeader);
			waveCallback(wParam, msg, IntPtr.Zero, waveHeader, IntPtr.Zero);
			break;
		}
		case WaveInterop.WaveMessage.WaveOutOpen:
		case WaveInterop.WaveMessage.WaveOutClose:
		case WaveInterop.WaveMessage.WaveInOpen:
		case WaveInterop.WaveMessage.WaveInClose:
			waveCallback(((Message)(ref m)).WParam, msg, IntPtr.Zero, null, IntPtr.Zero);
			break;
		default:
			((Form)this).WndProc(ref m);
			break;
		}
	}
}

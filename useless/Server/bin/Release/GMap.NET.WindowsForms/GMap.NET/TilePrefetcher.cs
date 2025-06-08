using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;

namespace GMap.NET;

public class TilePrefetcher : Form
{
	private BackgroundWorker worker = new BackgroundWorker();

	private List<GPoint> _list;

	private int _zoom;

	private GMapProvider _provider;

	private int _sleep;

	private int _all;

	public bool ShowCompleteMessage;

	private RectLatLng _area;

	private GSize _maxOfTiles;

	public GMapOverlay Overlay;

	private int _retry;

	public bool Shuffle = true;

	private readonly AutoResetEvent _done = new AutoResetEvent(initialState: true);

	public readonly Queue<GPoint> CachedTiles = new Queue<GPoint>();

	private IContainer components;

	private Label label1;

	private TableLayoutPanel tableLayoutPanel1;

	private ProgressBar progressBarDownload;

	private Label label2;

	public TilePrefetcher()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		InitializeComponent();
		GMaps instance = GMaps.Instance;
		instance.OnTileCacheComplete = (TileCacheComplete)Delegate.Combine((Delegate?)(object)instance.OnTileCacheComplete, (Delegate?)new TileCacheComplete(OnTileCacheComplete));
		GMaps instance2 = GMaps.Instance;
		instance2.OnTileCacheStart = (TileCacheStart)Delegate.Combine((Delegate?)(object)instance2.OnTileCacheStart, (Delegate?)new TileCacheStart(OnTileCacheStart));
		GMaps instance3 = GMaps.Instance;
		instance3.OnTileCacheProgress = (TileCacheProgress)Delegate.Combine((Delegate?)(object)instance3.OnTileCacheProgress, (Delegate?)new TileCacheProgress(OnTileCacheProgress));
		worker.WorkerReportsProgress = true;
		worker.WorkerSupportsCancellation = true;
		worker.ProgressChanged += worker_ProgressChanged;
		worker.DoWork += worker_DoWork;
		worker.RunWorkerCompleted += worker_RunWorkerCompleted;
	}

	private void OnTileCacheComplete()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (!((Control)this).IsDisposed)
		{
			_done.Set();
			MethodInvoker val = (MethodInvoker)delegate
			{
				((Control)label2).Text = "all tiles saved";
			};
			((Control)this).Invoke((Delegate)(object)val);
		}
	}

	private void OnTileCacheStart()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (!((Control)this).IsDisposed)
		{
			_done.Reset();
			MethodInvoker val = (MethodInvoker)delegate
			{
				((Control)label2).Text = "saving tiles...";
			};
			((Control)this).Invoke((Delegate)(object)val);
		}
	}

	private void OnTileCacheProgress(int left)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		if (!((Control)this).IsDisposed)
		{
			MethodInvoker val = (MethodInvoker)delegate
			{
				((Control)label2).Text = left + " tile to save...";
			};
			((Control)this).Invoke((Delegate)(object)val);
		}
	}

	public void Start(RectLatLng area, int zoom, GMapProvider provider, int sleep, int retry)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (!worker.IsBusy)
		{
			((Control)label1).Text = "...";
			progressBarDownload.Value = 0;
			_area = area;
			_zoom = zoom;
			_provider = provider;
			_sleep = sleep;
			_retry = retry;
			GMaps.Instance.UseMemoryCache = false;
			GMaps.Instance.CacheOnIdleRead = false;
			GMaps.Instance.BoostCacheEngine = true;
			if (Overlay != null)
			{
				Overlay.Markers.Clear();
			}
			worker.RunWorkerAsync();
			((Form)this).ShowDialog();
		}
	}

	public void Stop()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		GMaps instance = GMaps.Instance;
		instance.OnTileCacheComplete = (TileCacheComplete)Delegate.Remove((Delegate?)(object)instance.OnTileCacheComplete, (Delegate?)new TileCacheComplete(OnTileCacheComplete));
		GMaps instance2 = GMaps.Instance;
		instance2.OnTileCacheStart = (TileCacheStart)Delegate.Remove((Delegate?)(object)instance2.OnTileCacheStart, (Delegate?)new TileCacheStart(OnTileCacheStart));
		GMaps instance3 = GMaps.Instance;
		instance3.OnTileCacheProgress = (TileCacheProgress)Delegate.Remove((Delegate?)(object)instance3.OnTileCacheProgress, (Delegate?)new TileCacheProgress(OnTileCacheProgress));
		_done.Set();
		if (worker.IsBusy)
		{
			worker.CancelAsync();
		}
		GMaps.Instance.CancelTileCaching();
		_done.Close();
	}

	private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (ShowCompleteMessage)
		{
			if (!e.Cancelled)
			{
				MessageBox.Show((IWin32Window)(object)this, "Prefetch Complete! => " + (int)e.Result + " of " + _all);
			}
			else
			{
				MessageBox.Show((IWin32Window)(object)this, "Prefetch Canceled! => " + (int)e.Result + " of " + _all);
			}
		}
		_list.Clear();
		GMaps.Instance.UseMemoryCache = true;
		GMaps.Instance.CacheOnIdleRead = true;
		GMaps.Instance.BoostCacheEngine = false;
		worker.Dispose();
		((Form)this).Close();
	}

	private bool CacheTiles(int zoom, GPoint p)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		GMapProvider[] overlays = _provider.Overlays;
		Exception ex = default(Exception);
		foreach (GMapProvider val in overlays)
		{
			PureImage val2 = ((!val.InvertedAxisY) ? GMaps.Instance.GetImageFrom(val, p, zoom, ref ex) : GMaps.Instance.GetImageFrom(val, new GPoint(((GPoint)(ref p)).X, ((GSize)(ref _maxOfTiles)).Height - ((GPoint)(ref p)).Y), zoom, ref ex));
			if (val2 != null)
			{
				val2.Dispose();
				val2 = null;
				continue;
			}
			return false;
		}
		return true;
	}

	private void worker_DoWork(object sender, DoWorkEventArgs e)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if (_list != null)
		{
			_list.Clear();
			_list = null;
		}
		_list = _provider.Projection.GetAreaTileList(_area, _zoom, 0);
		_maxOfTiles = _provider.Projection.GetTileMatrixMaxXY(_zoom);
		_all = _list.Count;
		int num = 0;
		int num2 = 0;
		if (Shuffle)
		{
			Stuff.Shuffle<GPoint>(_list);
		}
		lock (this)
		{
			CachedTiles.Clear();
		}
		for (int i = 0; i < _all && !worker.CancellationPending; i++)
		{
			GPoint val = _list[i];
			if (CacheTiles(_zoom, val))
			{
				if (Overlay != null)
				{
					lock (this)
					{
						CachedTiles.Enqueue(val);
					}
				}
				num++;
				num2 = 0;
			}
			else
			{
				if (++num2 <= _retry)
				{
					i--;
					Thread.Sleep(1111);
					continue;
				}
				num2 = 0;
			}
			worker.ReportProgress((i + 1) * 100 / _all, i + 1);
			if (_sleep > 0)
			{
				Thread.Sleep(_sleep);
			}
		}
		e.Result = num;
		if (!((Control)this).IsDisposed)
		{
			_done.WaitOne();
		}
	}

	private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		((Control)label1).Text = "Fetching tile at zoom (" + _zoom + "): " + (int)e.UserState + " of " + _all + ", complete: " + e.ProgressPercentage + "%";
		progressBarDownload.Value = e.ProgressPercentage;
		if (Overlay == null)
		{
			return;
		}
		GPoint? val = null;
		lock (this)
		{
			if (CachedTiles.Count > 0)
			{
				val = CachedTiles.Dequeue();
			}
		}
		if (val.HasValue)
		{
			GPoint val2 = Overlay.Control.MapProvider.Projection.FromTileXYToPixel(val.Value);
			PointLatLng val3 = Overlay.Control.MapProvider.Projection.FromPixelToLatLng(val2, _zoom);
			double groundResolution = Overlay.Control.MapProvider.Projection.GetGroundResolution(_zoom, ((PointLatLng)(ref val3)).Lat);
			double num = Overlay.Control.MapProvider.Projection.GetGroundResolution((int)Overlay.Control.Zoom, ((PointLatLng)(ref val3)).Lat) / groundResolution;
			PointLatLng p = val3;
			GSize tileSize = Overlay.Control.MapProvider.Projection.TileSize;
			GMapMarkerTile item = new GMapMarkerTile(p, (int)((double)((GSize)(ref tileSize)).Width / num));
			Overlay.Markers.Add(item);
		}
	}

	private void Prefetch_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)e.KeyCode == 27)
		{
			((Form)this).Close();
		}
	}

	private void Prefetch_FormClosed(object sender, FormClosedEventArgs e)
	{
		Stop();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Form)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Expected O, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Expected O, but got Unknown
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Expected O, but got Unknown
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Expected O, but got Unknown
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Expected O, but got Unknown
		label1 = new Label();
		tableLayoutPanel1 = new TableLayoutPanel();
		progressBarDownload = new ProgressBar();
		label2 = new Label();
		((Control)tableLayoutPanel1).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)label1).AutoSize = true;
		((Control)label1).Dock = (DockStyle)5;
		((Control)label1).Location = new Point(4, 0);
		((Control)label1).Margin = new Padding(4, 0, 4, 0);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(406, 17);
		((Control)label1).TabIndex = 1;
		((Control)label1).Text = "label1";
		tableLayoutPanel1.ColumnCount = 2;
		tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle((SizeType)2, 50f));
		tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle((SizeType)1, 125f));
		tableLayoutPanel1.Controls.Add((Control)(object)progressBarDownload, 0, 1);
		tableLayoutPanel1.Controls.Add((Control)(object)label1, 0, 0);
		tableLayoutPanel1.Controls.Add((Control)(object)label2, 1, 1);
		((Control)tableLayoutPanel1).Dock = (DockStyle)5;
		((Control)tableLayoutPanel1).Location = new Point(5, 5);
		((Control)tableLayoutPanel1).Margin = new Padding(4);
		((Control)tableLayoutPanel1).Name = "tableLayoutPanel1";
		tableLayoutPanel1.RowCount = 2;
		tableLayoutPanel1.RowStyles.Add(new RowStyle());
		tableLayoutPanel1.RowStyles.Add(new RowStyle((SizeType)2, 50f));
		((Control)tableLayoutPanel1).Size = new Size(539, 62);
		((Control)tableLayoutPanel1).TabIndex = 2;
		((Control)progressBarDownload).Dock = (DockStyle)5;
		((Control)progressBarDownload).Location = new Point(4, 21);
		((Control)progressBarDownload).Margin = new Padding(4);
		((Control)progressBarDownload).Name = "progressBarDownload";
		((Control)progressBarDownload).Size = new Size(406, 37);
		progressBarDownload.Style = (ProgressBarStyle)1;
		((Control)progressBarDownload).TabIndex = 3;
		((Control)label2).AutoSize = true;
		((Control)label2).Dock = (DockStyle)5;
		((Control)label2).Font = new Font("Microsoft Sans Serif", 7.8f, (FontStyle)1, (GraphicsUnit)3, (byte)0);
		((Control)label2).Location = new Point(418, 17);
		((Control)label2).Margin = new Padding(4, 0, 4, 0);
		((Control)label2).Name = "label2";
		((Control)label2).Size = new Size(117, 45);
		((Control)label2).TabIndex = 2;
		((Control)label2).Text = "please wait...";
		label2.TextAlign = (ContentAlignment)32;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 16f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = Color.AliceBlue;
		((Form)this).ClientSize = new Size(549, 72);
		((Form)this).ControlBox = false;
		((Control)this).Controls.Add((Control)(object)tableLayoutPanel1);
		((Form)this).FormBorderStyle = (FormBorderStyle)3;
		((Form)this).KeyPreview = true;
		((Form)this).Margin = new Padding(4);
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)this).Name = "TilePrefetcher";
		((Control)this).Padding = new Padding(5);
		((Form)this).ShowIcon = false;
		((Form)this).ShowInTaskbar = false;
		((Form)this).StartPosition = (FormStartPosition)4;
		((Control)this).Text = "GMap.NET - esc to cancel fetching";
		((Form)this).FormClosed += new FormClosedEventHandler(Prefetch_FormClosed);
		((Control)this).PreviewKeyDown += new PreviewKeyDownEventHandler(Prefetch_PreviewKeyDown);
		((Control)tableLayoutPanel1).ResumeLayout(false);
		((Control)tableLayoutPanel1).PerformLayout();
		((Control)this).ResumeLayout(false);
	}
}

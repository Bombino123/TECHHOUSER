using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GMap.NET.MapProviders;
using GMap.NET.Projections;

namespace GMap.NET.Internals;

internal sealed class Core : IDisposable
{
	internal PointLatLng _position;

	private GPoint _positionPixel;

	internal GPoint RenderOffset;

	internal GPoint CenterTileXYLocation;

	private GPoint _centerTileXYLocationLast;

	private GPoint _dragPoint;

	internal GPoint CompensationOffset;

	internal GPoint MouseDown;

	internal GPoint MouseCurrent;

	internal GPoint MouseLastZoom;

	internal GPoint touchCurrent;

	public MouseWheelZoomType MouseWheelZoomType;

	public bool MouseWheelZoomEnabled = true;

	public PointLatLng? LastLocationInBounds;

	public bool VirtualSizeEnabled;

	private GSize _sizeOfMapArea;

	private GSize _minOfTiles;

	private GSize _maxOfTiles;

	internal GRect TileRect;

	internal GRect TileRectBearing;

	internal float Bearing;

	public bool IsRotated;

	internal bool FillEmptyTiles = true;

	public TileMatrix Matrix = new TileMatrix();

	internal List<DrawTile> TileDrawingList = new List<DrawTile>();

	internal FastReaderWriterLock TileDrawingListLock = new FastReaderWriterLock();

	private static readonly int GThreadPoolSize = 4;

	private DateTime _lastTileLoadStart = DateTime.Now;

	private DateTime _lastTileLoadEnd = DateTime.Now;

	internal volatile bool IsStarted;

	private int _zoom;

	internal double ScaleX = 1.0;

	internal double ScaleY = 1.0;

	internal int MaxZoom = 2;

	internal int MinZoom = 2;

	internal int Width;

	internal int Height;

	internal int PxRes100M;

	internal int PxRes1000M;

	internal int PxRes10Km;

	internal int PxRes100Km;

	internal int PxRes1000Km;

	internal int PxRes5000Km;

	public bool IsDragging;

	private GMapProvider _provider;

	internal bool ZoomToArea = true;

	public bool PolygonsEnabled = true;

	public bool RoutesEnabled = true;

	public bool MarkersEnabled = true;

	public bool CanDragMap = true;

	public int RetryLoadTile;

	public int LevelsKeepInMemory = 5;

	public RenderMode RenderMode;

	private readonly List<Thread> _gThreadPool = new List<Thread>();

	internal string SystemType;

	internal static int Instances;

	private BackgroundWorker _invalidator;

	internal readonly object InvalidationLock = new object();

	internal DateTime LastInvalidation = DateTime.Now;

	internal int VWidth = 800;

	internal int VHeight = 400;

	public bool MouseWheelZooming;

	private bool _raiseEmptyTileError;

	internal Dictionary<LoadTask, Exception> FailedLoads = new Dictionary<LoadTask, Exception>(new LoadTaskComparer());

	internal static readonly int WaitForTileLoadThreadTimeout = 300000;

	private volatile int _okZoom;

	private volatile int _skipOverZoom;

	private static readonly BlockingCollection<LoadTask> TileLoadQueue4 = new BlockingCollection<LoadTask>(new ConcurrentStack<LoadTask>());

	private static List<Task> _tileLoadQueue4Tasks;

	private static int _loadWaitCount;

	public AutoResetEvent Refresh = new AutoResetEvent(initialState: false);

	public bool UpdatingBounds;

	public int Zoom
	{
		get
		{
			return _zoom;
		}
		set
		{
			if (_zoom == value || IsDragging)
			{
				return;
			}
			_zoom = value;
			_minOfTiles = Provider.Projection.GetTileMatrixMinXY(value);
			_maxOfTiles = Provider.Projection.GetTileMatrixMaxXY(value);
			_positionPixel = Provider.Projection.FromLatLngToPixel(Position, value);
			if (IsStarted)
			{
				CancelAsyncTasks();
				Matrix.ClearLevelsBelove(_zoom - LevelsKeepInMemory);
				Matrix.ClearLevelsAbove(_zoom + LevelsKeepInMemory);
				lock (FailedLoads)
				{
					FailedLoads.Clear();
					_raiseEmptyTileError = true;
				}
				GoToCurrentPositionOnZoom();
				UpdateBounds();
				if (this.OnMapZoomChanged != null)
				{
					this.OnMapZoomChanged();
				}
			}
		}
	}

	public GPoint PositionPixel => _positionPixel;

	public PointLatLng Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
			_positionPixel = Provider.Projection.FromLatLngToPixel(value, Zoom);
			if (IsStarted)
			{
				if (!IsDragging)
				{
					GoToCurrentPosition();
				}
				if (this.OnCurrentPositionChanged != null)
				{
					this.OnCurrentPositionChanged(_position);
				}
			}
		}
	}

	public GMapProvider Provider
	{
		get
		{
			return _provider;
		}
		set
		{
			if (_provider != null && _provider.Equals(value))
			{
				return;
			}
			bool flag = _provider == null || _provider.Projection != value.Projection;
			_provider = value;
			if (!_provider.IsInitialized)
			{
				_provider.IsInitialized = true;
				_provider.OnInitialized();
			}
			if (_provider.Projection != null && flag)
			{
				TileRect = new GRect(GPoint.Empty, Provider.Projection.TileSize);
				TileRectBearing = TileRect;
				if (IsRotated)
				{
					TileRectBearing.Inflate(1L, 1L);
				}
				_minOfTiles = Provider.Projection.GetTileMatrixMinXY(Zoom);
				_maxOfTiles = Provider.Projection.GetTileMatrixMaxXY(Zoom);
				_positionPixel = Provider.Projection.FromLatLngToPixel(Position, Zoom);
			}
			if (IsStarted)
			{
				CancelAsyncTasks();
				if (flag)
				{
					OnMapSizeChanged(Width, Height);
				}
				ReloadMap();
				if (MinZoom < _provider.MinZoom)
				{
					MinZoom = _provider.MinZoom;
				}
				ZoomToArea = true;
				if (_provider.Area.HasValue && !_provider.Area.Value.Contains(Position))
				{
					SetZoomToFitRect(_provider.Area.Value);
					ZoomToArea = false;
				}
				if (this.OnMapTypeChanged != null)
				{
					this.OnMapTypeChanged(value);
				}
			}
		}
	}

	public RectLatLng ViewArea
	{
		get
		{
			if (Provider.Projection != null)
			{
				PointLatLng pointLatLng = FromLocalToLatLng(0L, 0L);
				PointLatLng pointLatLng2 = FromLocalToLatLng(Width, Height);
				return RectLatLng.FromLTRB(pointLatLng.Lng, pointLatLng.Lat, pointLatLng2.Lng, pointLatLng2.Lat);
			}
			return RectLatLng.Empty;
		}
	}

	public event PositionChanged OnCurrentPositionChanged;

	public event TileLoadComplete OnTileLoadComplete;

	public event TileLoadStart OnTileLoadStart;

	public event EmptyTileError OnEmptyTileError;

	public event MapDrag OnMapDrag;

	public event MapZoomChanged OnMapZoomChanged;

	public event MapTypeChanged OnMapTypeChanged;

	public Core()
	{
		Provider = EmptyProvider.Instance;
	}

	public bool SetZoomToFitRect(RectLatLng rect)
	{
		int num = GetMaxZoomToFitRect(rect);
		if (num > 0)
		{
			PointLatLng position = new PointLatLng(rect.Lat - rect.HeightLat / 2.0, rect.Lng + rect.WidthLng / 2.0);
			Position = position;
			if (num > MaxZoom)
			{
				num = MaxZoom;
			}
			if (Zoom != num)
			{
				Zoom = num;
			}
			return true;
		}
		return false;
	}

	public BackgroundWorker OnMapOpen()
	{
		if (!IsStarted)
		{
			int num = Interlocked.Increment(ref Instances);
			IsStarted = true;
			if (num == 1)
			{
				GMaps.Instance.NoMapInstances = false;
			}
			GoToCurrentPosition();
			_invalidator = new BackgroundWorker();
			_invalidator.WorkerSupportsCancellation = true;
			_invalidator.WorkerReportsProgress = true;
			_invalidator.DoWork += InvalidatorWatch;
			_invalidator.RunWorkerAsync();
		}
		return _invalidator;
	}

	public void OnMapClose()
	{
		Dispose();
	}

	private void InvalidatorWatch(object sender, DoWorkEventArgs e)
	{
		BackgroundWorker backgroundWorker = sender as BackgroundWorker;
		TimeSpan timeSpan = TimeSpan.FromMilliseconds(111.0);
		int millisecondsTimeout = (int)timeSpan.TotalMilliseconds;
		bool flag = false;
		while (Refresh != null)
		{
			if ((!flag && Refresh.WaitOne()) || !Refresh.WaitOne(millisecondsTimeout, exitContext: false))
			{
			}
			if (backgroundWorker.CancellationPending)
			{
				break;
			}
			DateTime now = DateTime.Now;
			TimeSpan timeSpan2;
			lock (InvalidationLock)
			{
				timeSpan2 = now - LastInvalidation;
			}
			if (timeSpan2 > timeSpan)
			{
				lock (InvalidationLock)
				{
					LastInvalidation = now;
				}
				flag = false;
				backgroundWorker.ReportProgress(1);
			}
			else
			{
				flag = true;
			}
		}
	}

	public void UpdateCenterTileXYLocation()
	{
		PointLatLng p = FromLocalToLatLng(Width / 2, Height / 2);
		GPoint p2 = Provider.Projection.FromLatLngToPixel(p, Zoom);
		CenterTileXYLocation = Provider.Projection.FromPixelToTileXY(p2);
	}

	public void OnMapSizeChanged(int width, int height)
	{
		Width = width;
		Height = height;
		if (IsRotated)
		{
			int num = (int)Math.Round(Math.Sqrt(Width * Width + Height * Height) / (double)Provider.Projection.TileSize.Width, MidpointRounding.AwayFromZero);
			_sizeOfMapArea.Width = 1 + num / 2;
			_sizeOfMapArea.Height = 1 + num / 2;
		}
		else
		{
			_sizeOfMapArea.Width = 1 + Width / Provider.Projection.TileSize.Width / 2;
			_sizeOfMapArea.Height = 1 + Height / Provider.Projection.TileSize.Height / 2;
		}
		if (IsStarted)
		{
			UpdateBounds();
			GoToCurrentPosition();
		}
	}

	public PointLatLng FromLocalToLatLng(long x, long y)
	{
		GPoint p = new GPoint(x, y);
		p.OffsetNegative(RenderOffset);
		p.Offset(CompensationOffset);
		return Provider.Projection.FromPixelToLatLng(p, Zoom);
	}

	public GPoint FromLatLngToLocal(PointLatLng latlng)
	{
		GPoint result = Provider.Projection.FromLatLngToPixel(latlng, Zoom);
		result.Offset(RenderOffset);
		result.OffsetNegative(CompensationOffset);
		return result;
	}

	public int GetMaxZoomToFitRect(RectLatLng rect)
	{
		int num = MinZoom;
		if (rect.HeightLat == 0.0 || rect.WidthLng == 0.0)
		{
			num = MaxZoom / 2;
		}
		else
		{
			for (int i = num; i <= MaxZoom; i++)
			{
				GPoint gPoint = Provider.Projection.FromLatLngToPixel(rect.LocationTopLeft, i);
				GPoint gPoint2 = Provider.Projection.FromLatLngToPixel(rect.LocationRightBottom, i);
				if (gPoint2.X - gPoint.X > Width + 10 || gPoint2.Y - gPoint.Y > Height + 10)
				{
					break;
				}
				num = i;
			}
		}
		return num;
	}

	public void BeginDrag(GPoint pt)
	{
		_dragPoint.X = pt.X - RenderOffset.X;
		_dragPoint.Y = pt.Y - RenderOffset.Y;
		IsDragging = true;
	}

	public void EndDrag()
	{
		IsDragging = false;
		MouseDown = GPoint.Empty;
		Refresh.Set();
	}

	public void ReloadMap()
	{
		if (IsStarted)
		{
			_okZoom = 0;
			_skipOverZoom = 0;
			CancelAsyncTasks();
			Matrix.ClearAllLevels();
			lock (FailedLoads)
			{
				FailedLoads.Clear();
				_raiseEmptyTileError = true;
			}
			Refresh.Set();
			UpdateBounds();
			return;
		}
		throw new Exception("Please, do not call ReloadMap before form is loaded, it's useless");
	}

	public void GoToCurrentPosition()
	{
		CompensationOffset = _positionPixel;
		RenderOffset = GPoint.Empty;
		_dragPoint = GPoint.Empty;
		GPoint pt = new GPoint(Width / 2, Height / 2);
		Drag(pt);
	}

	internal void GoToCurrentPositionOnZoom()
	{
		CompensationOffset = _positionPixel;
		RenderOffset = GPoint.Empty;
		_dragPoint = GPoint.Empty;
		if (MouseWheelZooming)
		{
			if (MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter)
			{
				GPoint gPoint = new GPoint(-(_positionPixel.X - Width / 2), -(_positionPixel.Y - Height / 2));
				gPoint.Offset(CompensationOffset);
				RenderOffset.X = gPoint.X - _dragPoint.X;
				RenderOffset.Y = gPoint.Y - _dragPoint.Y;
			}
			else
			{
				RenderOffset.X = -_positionPixel.X - _dragPoint.X;
				RenderOffset.Y = -_positionPixel.Y - _dragPoint.Y;
				RenderOffset.Offset(MouseLastZoom);
				RenderOffset.Offset(CompensationOffset);
			}
		}
		else
		{
			MouseLastZoom = GPoint.Empty;
			GPoint gPoint2 = new GPoint(-(_positionPixel.X - Width / 2), -(_positionPixel.Y - Height / 2));
			gPoint2.Offset(CompensationOffset);
			RenderOffset.X = gPoint2.X - _dragPoint.X;
			RenderOffset.Y = gPoint2.Y - _dragPoint.Y;
		}
		UpdateCenterTileXYLocation();
	}

	public void DragOffset(GPoint offset)
	{
		RenderOffset.Offset(offset);
		UpdateCenterTileXYLocation();
		if (CenterTileXYLocation != _centerTileXYLocationLast)
		{
			_centerTileXYLocationLast = CenterTileXYLocation;
			UpdateBounds();
		}
		LastLocationInBounds = Position;
		IsDragging = true;
		Position = FromLocalToLatLng(Width / 2, Height / 2);
		IsDragging = false;
		if (this.OnMapDrag != null)
		{
			this.OnMapDrag();
		}
	}

	public void Drag(GPoint pt)
	{
		RenderOffset.X = pt.X - _dragPoint.X;
		RenderOffset.Y = pt.Y - _dragPoint.Y;
		UpdateCenterTileXYLocation();
		if (CenterTileXYLocation != _centerTileXYLocationLast)
		{
			_centerTileXYLocationLast = CenterTileXYLocation;
			UpdateBounds();
		}
		if (IsDragging)
		{
			LastLocationInBounds = Position;
			Position = FromLocalToLatLng(Width / 2, Height / 2);
			if (this.OnMapDrag != null)
			{
				this.OnMapDrag();
			}
		}
	}

	public void CancelAsyncTasks()
	{
		_ = IsStarted;
	}

	private void AddLoadTask(LoadTask t)
	{
		if (_tileLoadQueue4Tasks == null)
		{
			lock (TileLoadQueue4)
			{
				if (_tileLoadQueue4Tasks == null)
				{
					_tileLoadQueue4Tasks = new List<Task>();
					while (_tileLoadQueue4Tasks.Count < GThreadPoolSize)
					{
						_tileLoadQueue4Tasks.Add(Task.Factory.StartNew(delegate
						{
							string text = "ProcessLoadTask[" + Thread.CurrentThread.ManagedThreadId + "]";
							Thread.CurrentThread.Name = text;
							do
							{
								if (TileLoadQueue4.Count == 0 && Interlocked.Increment(ref _loadWaitCount) >= GThreadPoolSize)
								{
									Interlocked.Exchange(ref _loadWaitCount, 0);
									OnLoadComplete(text);
								}
								ProcessLoadTask(TileLoadQueue4.Take(), text);
							}
							while (!TileLoadQueue4.IsAddingCompleted);
						}, TaskCreationOptions.LongRunning));
					}
				}
			}
		}
		TileLoadQueue4.Add(t);
	}

	private static void ProcessLoadTask(LoadTask task, string ctid)
	{
		try
		{
			if (task.Core.Matrix == null || task.Core.Matrix.GetTileWithReadLock(task.Zoom, task.Pos).NotEmpty)
			{
				return;
			}
			Tile tile = new Tile(task.Zoom, task.Pos);
			GMapProvider[] overlays = task.Core._provider.Overlays;
			foreach (GMapProvider gMapProvider in overlays)
			{
				int num = 0;
				do
				{
					PureImage pureImage = null;
					Exception result = null;
					if (task.Zoom >= task.Core._provider.MinZoom && (!task.Core._provider.MaxZoom.HasValue || task.Zoom <= task.Core._provider.MaxZoom) && (task.Core._skipOverZoom == 0 || task.Zoom <= task.Core._skipOverZoom))
					{
						pureImage = ((!gMapProvider.InvertedAxisY) ? GMaps.Instance.GetImageFrom(gMapProvider, task.Pos, task.Zoom, out result) : GMaps.Instance.GetImageFrom(gMapProvider, new GPoint(task.Pos.X, task.Core._maxOfTiles.Height - task.Pos.Y), task.Zoom, out result));
					}
					if (pureImage != null && result == null)
					{
						if (task.Core._okZoom < task.Zoom)
						{
							task.Core._okZoom = task.Zoom;
							task.Core._skipOverZoom = 0;
						}
					}
					else if (result != null && task.Core._skipOverZoom != task.Core._okZoom && task.Zoom > task.Core._okZoom && result.Message.Contains("(404) Not Found"))
					{
						task.Core._skipOverZoom = task.Core._okZoom;
					}
					if (pureImage == null && task.Core._okZoom > 0 && task.Core.FillEmptyTiles && task.Core.Provider.Projection is MercatorProjection)
					{
						int num2 = ((task.Zoom <= task.Core._okZoom) ? 1 : (task.Zoom - task.Core._okZoom));
						long num3 = 0L;
						GPoint pos = GPoint.Empty;
						while (pureImage == null && num2 < task.Zoom)
						{
							num3 = (long)Math.Pow(2.0, num2);
							pos = new GPoint(task.Pos.X / num3, task.Pos.Y / num3);
							pureImage = GMaps.Instance.GetImageFrom(gMapProvider, pos, task.Zoom - num2++, out result);
						}
						if (pureImage != null)
						{
							long xoff = Math.Abs(task.Pos.X - pos.X * num3);
							long yoff = Math.Abs(task.Pos.Y - pos.Y * num3);
							pureImage.IsParent = true;
							pureImage.Ix = num3;
							pureImage.Xoff = xoff;
							pureImage.Yoff = yoff;
						}
					}
					if (pureImage != null)
					{
						tile.AddOverlay(pureImage);
						break;
					}
					if (result != null && task.Core.FailedLoads != null)
					{
						lock (task.Core.FailedLoads)
						{
							if (!task.Core.FailedLoads.ContainsKey(task))
							{
								task.Core.FailedLoads.Add(task, result);
								if (task.Core.OnEmptyTileError != null && !task.Core._raiseEmptyTileError)
								{
									task.Core._raiseEmptyTileError = true;
									task.Core.OnEmptyTileError(task.Zoom, task.Pos);
								}
							}
						}
					}
					if (task.Core.RetryLoadTile > 0)
					{
						Thread.Sleep(1111);
					}
				}
				while (++num < task.Core.RetryLoadTile);
			}
			if (tile.HasAnyOverlays && task.Core.IsStarted)
			{
				task.Core.Matrix.SetTile(tile);
			}
			else
			{
				tile.Dispose();
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			if (task.Core.Refresh != null)
			{
				task.Core.Refresh.Set();
			}
		}
	}

	private void OnLoadComplete(string ctid)
	{
		_lastTileLoadEnd = DateTime.Now;
		long elapsedMilliseconds = (long)(_lastTileLoadEnd - _lastTileLoadStart).TotalMilliseconds;
		if (IsStarted)
		{
			GMaps.Instance.MemoryCache.RemoveOverload();
			TileDrawingListLock.AcquireReaderLock();
			try
			{
				Matrix.ClearLevelAndPointsNotIn(Zoom, TileDrawingList);
			}
			finally
			{
				TileDrawingListLock.ReleaseReaderLock();
			}
		}
		UpdateGroundResolution();
		if (this.OnTileLoadComplete != null)
		{
			this.OnTileLoadComplete(elapsedMilliseconds);
		}
	}

	private void UpdateBounds()
	{
		if (!IsStarted || Provider.Equals(EmptyProvider.Instance))
		{
			return;
		}
		UpdatingBounds = true;
		TileDrawingListLock.AcquireWriterLock();
		try
		{
			TileDrawingList.Clear();
			long num = (int)Math.Floor((double)(-_sizeOfMapArea.Width) * ScaleX);
			for (long num2 = (int)Math.Ceiling((double)_sizeOfMapArea.Width * ScaleX); num <= num2; num++)
			{
				long num3 = (int)Math.Floor((double)(-_sizeOfMapArea.Height) * ScaleY);
				for (long num4 = (int)Math.Ceiling((double)_sizeOfMapArea.Height * ScaleY); num3 <= num4; num3++)
				{
					GPoint centerTileXYLocation = CenterTileXYLocation;
					centerTileXYLocation.X += num;
					centerTileXYLocation.Y += num3;
					if (centerTileXYLocation.X >= _minOfTiles.Width && centerTileXYLocation.Y >= _minOfTiles.Height && centerTileXYLocation.X <= _maxOfTiles.Width && centerTileXYLocation.Y <= _maxOfTiles.Height)
					{
						DrawTile drawTile = default(DrawTile);
						drawTile.PosXY = centerTileXYLocation;
						drawTile.PosPixel = new GPoint(centerTileXYLocation.X * TileRect.Width, centerTileXYLocation.Y * TileRect.Height);
						drawTile.DistanceSqr = (CenterTileXYLocation.X - centerTileXYLocation.X) * (CenterTileXYLocation.X - centerTileXYLocation.X) + (CenterTileXYLocation.Y - centerTileXYLocation.Y) * (CenterTileXYLocation.Y - centerTileXYLocation.Y);
						DrawTile item = drawTile;
						if (!TileDrawingList.Contains(item))
						{
							TileDrawingList.Add(item);
						}
					}
				}
			}
			if (GMaps.Instance.ShuffleTilesOnLoad)
			{
				Stuff.Shuffle(TileDrawingList);
			}
			else
			{
				TileDrawingList.Sort();
			}
		}
		finally
		{
			TileDrawingListLock.ReleaseWriterLock();
		}
		Interlocked.Exchange(ref _loadWaitCount, 0);
		TileDrawingListLock.AcquireReaderLock();
		try
		{
			foreach (DrawTile tileDrawing in TileDrawingList)
			{
				LoadTask t = new LoadTask(tileDrawing.PosXY, Zoom, this);
				AddLoadTask(t);
			}
		}
		finally
		{
			TileDrawingListLock.ReleaseReaderLock();
		}
		_lastTileLoadStart = DateTime.Now;
		UpdatingBounds = false;
		if (this.OnTileLoadStart != null)
		{
			this.OnTileLoadStart();
		}
	}

	private void UpdateGroundResolution()
	{
		double groundResolution = Provider.Projection.GetGroundResolution(Zoom, Position.Lat);
		PxRes100M = (int)(100.0 / groundResolution);
		PxRes1000M = (int)(1000.0 / groundResolution);
		PxRes10Km = (int)(10000.0 / groundResolution);
		PxRes100Km = (int)(100000.0 / groundResolution);
		PxRes1000Km = (int)(1000000.0 / groundResolution);
		PxRes5000Km = (int)(5000000.0 / groundResolution);
	}

	~Core()
	{
		Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		if (!IsStarted)
		{
			return;
		}
		if (_invalidator != null)
		{
			_invalidator.CancelAsync();
			_invalidator.DoWork -= InvalidatorWatch;
			_invalidator.Dispose();
			_invalidator = null;
		}
		if (Refresh != null)
		{
			Refresh.Set();
			Refresh.Close();
			Refresh = null;
		}
		int num = Interlocked.Decrement(ref Instances);
		CancelAsyncTasks();
		IsStarted = false;
		if (Matrix != null)
		{
			Matrix.Dispose();
			Matrix = null;
		}
		if (FailedLoads != null)
		{
			lock (FailedLoads)
			{
				FailedLoads.Clear();
				_raiseEmptyTileError = false;
			}
			FailedLoads = null;
		}
		TileDrawingListLock.AcquireWriterLock();
		try
		{
			TileDrawingList.Clear();
		}
		finally
		{
			TileDrawingListLock.ReleaseWriterLock();
		}
		if (TileDrawingListLock != null)
		{
			TileDrawingListLock.Dispose();
			TileDrawingListLock = null;
			TileDrawingList = null;
		}
		if (num == 0)
		{
			GMaps.Instance.NoMapInstances = true;
			GMaps.Instance.WaitForCache.Set();
			if (disposing)
			{
				GMaps.Instance.MemoryCache.Clear();
			}
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

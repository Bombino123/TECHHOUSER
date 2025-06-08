using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.ObjectModel;
using GMap.NET.Projections;

namespace GMap.NET.WindowsForms;

public class GMapControl : UserControl, Interface
{
	[CompilerGenerated]
	private ExceptionThrown m_OnExceptionThrown;

	public readonly ObservableCollectionThreadSafe<GMapOverlay> Overlays = new ObservableCollectionThreadSafe<GMapOverlay>();

	public string EmptyTileText = "We are sorry, but we don't\nhave imagery at this zoom\nlevel for this region.";

	public Pen EmptyTileBorders = new Pen(Brushes.White, 1f);

	public bool ShowCenter = true;

	public Pen ScalePen = new Pen(Color.Black, 3f);

	public Pen ScalePenBorder = new Pen(Color.WhiteSmoke, 6f);

	public Pen CenterPen = new Pen(Brushes.Red, 1f);

	public Pen SelectionPen = new Pen(Brushes.Blue, 2f);

	private Brush _selectedAreaFill = (Brush)new SolidBrush(Color.FromArgb(33, Color.RoyalBlue));

	private Color _selectedAreaFillColor = Color.FromArgb(33, Color.RoyalBlue);

	private HelperLineOptions _helperLineOption;

	public Pen HelperLinePen = new Pen(Color.Blue, 1f);

	private bool _renderHelperLine;

	private Brush _emptyTileBrush = (Brush)new SolidBrush(Color.Navy);

	private Color _emptyTileColor = Color.Navy;

	public bool MapScaleInfoEnabled;

	public MapScaleInfoPosition MapScaleInfoPosition;

	public bool FillEmptyTiles = true;

	public bool DisableAltForSelection;

	[Category("GMap.NET")]
	public MouseButtons DragButton = (MouseButtons)2097152;

	private bool _showTileGridLines;

	private RectLatLng _selectedArea;

	public RectLatLng? BoundsOfMap;

	public bool ForceDoubleBuffer;

	private readonly bool MobileMode;

	public bool HoldInvalidation;

	private bool _grayScale;

	private bool _negative;

	private ColorMatrix _colorMatrix;

	internal readonly Core Core = new Core();

	internal readonly Font CopyrightFont = new Font(FontFamily.GenericSansSerif, 7f, (FontStyle)0);

	internal readonly Font MissingDataFont = new Font(FontFamily.GenericSansSerif, 11f, (FontStyle)1);

	private Font ScaleFont = new Font(FontFamily.GenericSansSerif, 7f, (FontStyle)0);

	internal readonly StringFormat CenterFormat = new StringFormat();

	internal readonly StringFormat BottomFormat = new StringFormat();

	private readonly ImageAttributes _tileFlipXYAttributes = new ImageAttributes();

	private double _zoomReal;

	private Bitmap _backBuffer;

	private Graphics _gxOff;

	private RectLatLng? _lazySetZoomToFitRect;

	private bool _lazyEvents = true;

	public static readonly bool IsDesignerHosted;

	private PointLatLng _selectionStart;

	private PointLatLng _selectionEnd;

	private float? _mapRenderTransform;

	public Color EmptyMapBackground = Color.WhiteSmoke;

	private readonly Matrix _rotationMatrix = new Matrix();

	private readonly Matrix _rotationMatrixInvert = new Matrix();

	private bool _isSelected;

	private Cursor _cursorBefore = Cursors.Default;

	public Size DragSize = SystemInformation.DragSize;

	public bool DisableFocusOnMouseEnter;

	private bool _mouseIn;

	public bool InvertedMouseWheelZooming;

	public bool IgnoreMarkerOnMouseWheel;

	private bool _isDragging;

	private bool _isMouseOverMarker;

	internal int OverObjectCount;

	private bool _isMouseOverRoute;

	private bool _isMouseOverPolygon;

	private static readonly BinaryFormatter BinaryFormatter;

	[Category("GMap.NET")]
	[Description("maximum zoom level of map")]
	public int MaxZoom
	{
		get
		{
			return Core.MaxZoom;
		}
		set
		{
			Core.MaxZoom = value;
		}
	}

	[Category("GMap.NET")]
	[Description("minimum zoom level of map")]
	public int MinZoom
	{
		get
		{
			return Core.MinZoom;
		}
		set
		{
			Core.MinZoom = value;
		}
	}

	[Category("GMap.NET")]
	[Description("map zooming type for mouse wheel")]
	public MouseWheelZoomType MouseWheelZoomType
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Core.MouseWheelZoomType;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			Core.MouseWheelZoomType = value;
		}
	}

	[Category("GMap.NET")]
	[Description("enable map zoom on mouse wheel")]
	public bool MouseWheelZoomEnabled
	{
		get
		{
			return Core.MouseWheelZoomEnabled;
		}
		set
		{
			Core.MouseWheelZoomEnabled = value;
		}
	}

	[Category("GMap.NET")]
	[Description("background color od the selected area")]
	public Color SelectedAreaFillColor
	{
		get
		{
			return _selectedAreaFillColor;
		}
		set
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			if (_selectedAreaFillColor != value)
			{
				_selectedAreaFillColor = value;
				if (_selectedAreaFill != null)
				{
					_selectedAreaFill.Dispose();
					_selectedAreaFill = null;
				}
				_selectedAreaFill = (Brush)new SolidBrush(_selectedAreaFillColor);
			}
		}
	}

	[Browsable(false)]
	public HelperLineOptions HelperLineOption
	{
		get
		{
			return _helperLineOption;
		}
		set
		{
			_helperLineOption = value;
			_renderHelperLine = _helperLineOption == HelperLineOptions.ShowAlways;
			if (Core.IsStarted)
			{
				Invalidate();
			}
		}
	}

	[Category("GMap.NET")]
	[Description("background color of the empty tile")]
	public Color EmptyTileColor
	{
		get
		{
			return _emptyTileColor;
		}
		set
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			if (_emptyTileColor != value)
			{
				_emptyTileColor = value;
				if (_emptyTileBrush != null)
				{
					_emptyTileBrush.Dispose();
					_emptyTileBrush = null;
				}
				_emptyTileBrush = (Brush)new SolidBrush(_emptyTileColor);
			}
		}
	}

	[Browsable(false)]
	public int RetryLoadTile
	{
		get
		{
			return Core.RetryLoadTile;
		}
		set
		{
			Core.RetryLoadTile = value;
		}
	}

	[Browsable(false)]
	public int LevelsKeepInMemory
	{
		get
		{
			return Core.LevelsKeepInMemory;
		}
		set
		{
			Core.LevelsKeepInMemory = value;
		}
	}

	[Category("GMap.NET")]
	[Description("shows tile gridlines")]
	public bool ShowTileGridLines
	{
		get
		{
			return _showTileGridLines;
		}
		set
		{
			_showTileGridLines = value;
			Invalidate();
		}
	}

	[Browsable(false)]
	public RectLatLng SelectedArea
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _selectedArea;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_selectedArea = value;
			if (Core.IsStarted)
			{
				Invalidate();
			}
		}
	}

	[Category("GMap.NET")]
	public bool GrayScaleMode
	{
		get
		{
			return _grayScale;
		}
		set
		{
			_grayScale = value;
			ColorMatrix = (value ? ColorMatrixs.GrayScale : null);
		}
	}

	[Category("GMap.NET")]
	public bool NegativeMode
	{
		get
		{
			return _negative;
		}
		set
		{
			_negative = value;
			ColorMatrix = (value ? ColorMatrixs.Negative : null);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ColorMatrix ColorMatrix
	{
		get
		{
			return _colorMatrix;
		}
		set
		{
			_colorMatrix = value;
			if (GMapProvider.TileImageProxy != null && GMapProvider.TileImageProxy is GMapImageProxy)
			{
				(GMapProvider.TileImageProxy as GMapImageProxy).ColorMatrix = value;
				if (Core.IsStarted)
				{
					ReloadMap();
				}
			}
		}
	}

	[Browsable(false)]
	public bool IsRotated => Core.IsRotated;

	[Category("GMap.NET")]
	public float Bearing
	{
		get
		{
			return Core.Bearing;
		}
		set
		{
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			if (Core.Bearing == value)
			{
				return;
			}
			bool num = Core.Bearing == 0f;
			Core.Bearing = value;
			UpdateRotationMatrix();
			if (value != 0f && value % 360f != 0f)
			{
				Core.IsRotated = true;
				if (((GRect)(ref Core.TileRectBearing)).Size == ((GRect)(ref Core.TileRect)).Size)
				{
					Core.TileRectBearing = Core.TileRect;
					((GRect)(ref Core.TileRectBearing)).Inflate(1L, 1L);
				}
			}
			else
			{
				Core.IsRotated = false;
				Core.TileRectBearing = Core.TileRect;
			}
			if (num)
			{
				Core.OnMapSizeChanged(((Control)this).Width, ((Control)this).Height);
			}
			if (!HoldInvalidation && Core.IsStarted)
			{
				ForceUpdateOverlays();
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool VirtualSizeEnabled
	{
		get
		{
			return Core.VirtualSizeEnabled;
		}
		set
		{
			Core.VirtualSizeEnabled = value;
		}
	}

	[Category("GMap.NET")]
	[Description("map scale type")]
	public ScaleModes ScaleMode { get; set; }

	[Category("GMap.NET")]
	[DefaultValue(0)]
	public double Zoom
	{
		get
		{
			return _zoomReal;
		}
		set
		{
			if (_zoomReal != value)
			{
				if (value > (double)MaxZoom)
				{
					_zoomReal = MaxZoom;
				}
				else if (value < (double)MinZoom)
				{
					_zoomReal = MinZoom;
				}
				else
				{
					_zoomReal = value;
				}
				double num = value % 1.0;
				if (ScaleMode == ScaleModes.Fractional && num != 0.0)
				{
					float value2 = (float)Math.Pow(2.0, num);
					_mapRenderTransform = value2;
					ZoomStep = Convert.ToInt32(value - num);
				}
				else
				{
					_mapRenderTransform = null;
					ZoomStep = (int)Math.Floor(value);
				}
				if (Core.IsStarted && !IsDragging)
				{
					ForceUpdateOverlays();
				}
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	internal int ZoomStep
	{
		get
		{
			return Core.Zoom;
		}
		set
		{
			if (value > MaxZoom)
			{
				Core.Zoom = MaxZoom;
			}
			else if (value < MinZoom)
			{
				Core.Zoom = MinZoom;
			}
			else
			{
				Core.Zoom = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public PointLatLng Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Core.Position;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Core.Position = value;
			if (Core.IsStarted)
			{
				ForceUpdateOverlays();
			}
		}
	}

	[Browsable(false)]
	public GPoint PositionPixel => Core.PositionPixel;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public string CacheLocation
	{
		get
		{
			return CacheLocator.Location;
		}
		set
		{
			CacheLocator.Location = value;
		}
	}

	[Browsable(false)]
	public bool IsDragging => _isDragging;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool IsMouseOverMarker
	{
		get
		{
			return _isMouseOverMarker;
		}
		internal set
		{
			_isMouseOverMarker = value;
			OverObjectCount += (value ? 1 : (-1));
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool IsMouseOverRoute
	{
		get
		{
			return _isMouseOverRoute;
		}
		internal set
		{
			_isMouseOverRoute = value;
			OverObjectCount += (value ? 1 : (-1));
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool IsMouseOverPolygon
	{
		get
		{
			return _isMouseOverPolygon;
		}
		internal set
		{
			_isMouseOverPolygon = value;
			OverObjectCount += (value ? 1 : (-1));
		}
	}

	[Browsable(false)]
	public RectLatLng ViewArea
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			if (!IsRotated)
			{
				return Core.ViewArea;
			}
			if (Core.Provider.Projection != null)
			{
				PointLatLng val = FromLocalToLatLng(0, 0);
				PointLatLng val2 = FromLocalToLatLng(((Control)this).Width, ((Control)this).Height);
				return RectLatLng.FromLTRB(((PointLatLng)(ref val)).Lng, ((PointLatLng)(ref val)).Lat, ((PointLatLng)(ref val2)).Lng, ((PointLatLng)(ref val2)).Lat);
			}
			return RectLatLng.Empty;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public GMapProvider MapProvider
	{
		get
		{
			return Core.Provider;
		}
		set
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			if (Core.Provider != null && ((object)Core.Provider).Equals((object?)value))
			{
				return;
			}
			RectLatLng val = SelectedArea;
			if (val != RectLatLng.Empty)
			{
				Position = new PointLatLng(((RectLatLng)(ref val)).Lat - ((RectLatLng)(ref val)).HeightLat / 2.0, ((RectLatLng)(ref val)).Lng + ((RectLatLng)(ref val)).WidthLng / 2.0);
			}
			else
			{
				val = ViewArea;
			}
			Core.Provider = value;
			if (!Core.IsStarted)
			{
				return;
			}
			if (Core.ZoomToArea)
			{
				if (val != RectLatLng.Empty && val != ViewArea)
				{
					int maxZoomToFitRect = Core.GetMaxZoomToFitRect(val);
					if (maxZoomToFitRect > 0 && Zoom != (double)maxZoomToFitRect)
					{
						Zoom = maxZoomToFitRect;
					}
				}
			}
			else
			{
				ForceUpdateOverlays();
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public RoutingProvider RoutingProvider
	{
		get
		{
			GMapProvider mapProvider = MapProvider;
			RoutingProvider val = (RoutingProvider)(object)((mapProvider is RoutingProvider) ? mapProvider : null);
			if (val == null)
			{
				val = (RoutingProvider)(object)GMapProviders.OpenStreetMap;
			}
			return val;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public DirectionsProvider DirectionsProvider
	{
		get
		{
			GMapProvider mapProvider = MapProvider;
			DirectionsProvider val = (DirectionsProvider)(object)((mapProvider is DirectionsProvider) ? mapProvider : null);
			if (val == null)
			{
				OpenStreetMapProvider openStreetMap = GMapProviders.OpenStreetMap;
				val = (DirectionsProvider)(object)((openStreetMap is DirectionsProvider) ? openStreetMap : null);
			}
			return val;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public GeocodingProvider GeocodingProvider
	{
		get
		{
			GMapProvider mapProvider = MapProvider;
			GeocodingProvider val = (GeocodingProvider)(object)((mapProvider is GeocodingProvider) ? mapProvider : null);
			if (val == null)
			{
				val = (GeocodingProvider)(object)GMapProviders.OpenStreetMap;
			}
			return val;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public RoadsProvider RoadsProvider
	{
		get
		{
			GMapProvider mapProvider = MapProvider;
			RoadsProvider val = (RoadsProvider)(object)((mapProvider is RoadsProvider) ? mapProvider : null);
			if (val == null)
			{
				val = (RoadsProvider)(object)GMapProviders.GoogleMap;
			}
			return val;
		}
	}

	[Category("GMap.NET")]
	public bool RoutesEnabled
	{
		get
		{
			return Core.RoutesEnabled;
		}
		set
		{
			Core.RoutesEnabled = value;
		}
	}

	[Category("GMap.NET")]
	public bool PolygonsEnabled
	{
		get
		{
			return Core.PolygonsEnabled;
		}
		set
		{
			Core.PolygonsEnabled = value;
		}
	}

	[Category("GMap.NET")]
	public bool MarkersEnabled
	{
		get
		{
			return Core.MarkersEnabled;
		}
		set
		{
			Core.MarkersEnabled = value;
		}
	}

	[Category("GMap.NET")]
	public bool CanDragMap
	{
		get
		{
			return Core.CanDragMap;
		}
		set
		{
			Core.CanDragMap = value;
		}
	}

	[Browsable(false)]
	public RenderMode RenderMode
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Core.RenderMode;
		}
		internal set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			Core.RenderMode = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public GMaps Manager => GMaps.Instance;

	public event MapClick OnMapClick;

	public event MapDoubleClick OnMapDoubleClick;

	public event MarkerClick OnMarkerClick;

	public event MarkerDoubleClick OnMarkerDoubleClick;

	public event PolygonClick OnPolygonClick;

	public event PolygonDoubleClick OnPolygonDoubleClick;

	public event RouteClick OnRouteClick;

	public event RouteDoubleClick OnRouteDoubleClick;

	public event RouteEnter OnRouteEnter;

	public event RouteLeave OnRouteLeave;

	public event SelectionChange OnSelectionChange;

	public event MarkerEnter OnMarkerEnter;

	public event MarkerLeave OnMarkerLeave;

	public event PolygonEnter OnPolygonEnter;

	public event PolygonLeave OnPolygonLeave;

	public event ExceptionThrown OnExceptionThrown
	{
		[CompilerGenerated]
		add
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			ExceptionThrown val = this.m_OnExceptionThrown;
			ExceptionThrown val2;
			do
			{
				val2 = val;
				ExceptionThrown value2 = (ExceptionThrown)Delegate.Combine((Delegate?)(object)val2, (Delegate?)(object)value);
				val = Interlocked.CompareExchange(ref this.m_OnExceptionThrown, value2, val2);
			}
			while (val != val2);
		}
		[CompilerGenerated]
		remove
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			ExceptionThrown val = this.m_OnExceptionThrown;
			ExceptionThrown val2;
			do
			{
				val2 = val;
				ExceptionThrown value2 = (ExceptionThrown)Delegate.Remove((Delegate?)(object)val2, (Delegate?)(object)value);
				val = Interlocked.CompareExchange(ref this.m_OnExceptionThrown, value2, val2);
			}
			while (val != val2);
		}
	}

	public event PositionChanged OnPositionChanged
	{
		add
		{
			Core.OnCurrentPositionChanged += value;
		}
		remove
		{
			Core.OnCurrentPositionChanged -= value;
		}
	}

	public event TileLoadComplete OnTileLoadComplete
	{
		add
		{
			Core.OnTileLoadComplete += value;
		}
		remove
		{
			Core.OnTileLoadComplete -= value;
		}
	}

	public event TileLoadStart OnTileLoadStart
	{
		add
		{
			Core.OnTileLoadStart += value;
		}
		remove
		{
			Core.OnTileLoadStart -= value;
		}
	}

	public event MapDrag OnMapDrag
	{
		add
		{
			Core.OnMapDrag += value;
		}
		remove
		{
			Core.OnMapDrag -= value;
		}
	}

	public event MapZoomChanged OnMapZoomChanged
	{
		add
		{
			Core.OnMapZoomChanged += value;
		}
		remove
		{
			Core.OnMapZoomChanged -= value;
		}
	}

	public event MapTypeChanged OnMapTypeChanged
	{
		add
		{
			Core.OnMapTypeChanged += value;
		}
		remove
		{
			Core.OnMapTypeChanged -= value;
		}
	}

	public event EmptyTileError OnEmptyTileError
	{
		add
		{
			Core.OnEmptyTileError += value;
		}
		remove
		{
			Core.OnEmptyTileError -= value;
		}
	}

	[Category("GMap.NET")]
	[Description("Import From Kmz")]
	public bool ImportFromKmz(string file)
	{
		try
		{
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	[Category("GMap.NET")]
	[Description("Export From Kmz")]
	public bool ExportFromKmz(string file)
	{
		try
		{
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		((Control)this).OnKeyDown(e);
		if (HelperLineOption == HelperLineOptions.ShowOnModifierKey)
		{
			_renderHelperLine = (int)e.Modifiers == 65536 || (int)e.Modifiers == 262144;
			if (_renderHelperLine)
			{
				Invalidate();
			}
		}
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		((Control)this).OnKeyUp(e);
		if (HelperLineOption == HelperLineOptions.ShowOnModifierKey)
		{
			_renderHelperLine = (int)e.Modifiers == 65536 || (int)e.Modifiers == 262144;
			if (!_renderHelperLine)
			{
				Invalidate();
			}
		}
	}

	public override void Refresh()
	{
		HoldInvalidation = false;
		lock (Core.InvalidationLock)
		{
			Core.LastInvalidation = DateTime.Now;
		}
		((Control)this).Refresh();
	}

	public void Invalidate()
	{
		if (Core.Refresh != null)
		{
			Core.Refresh.Set();
		}
	}

	public GMapControl()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		if (!IsDesignerHosted)
		{
			((Control)this).SetStyle((ControlStyles)131072, true);
			((Control)this).SetStyle((ControlStyles)8192, true);
			((Control)this).SetStyle((ControlStyles)2, true);
			((Control)this).SetStyle((ControlStyles)4, true);
			((Control)this).ResizeRedraw = true;
			_tileFlipXYAttributes.SetWrapMode((WrapMode)3);
			GrayScaleMode = GrayScaleMode;
			NegativeMode = NegativeMode;
			Core.SystemType = "WindowsForms";
			RenderMode = (RenderMode)0;
			CenterFormat.Alignment = (StringAlignment)1;
			CenterFormat.LineAlignment = (StringAlignment)1;
			BottomFormat.Alignment = (StringAlignment)1;
			BottomFormat.LineAlignment = (StringAlignment)2;
			if (GMaps.Instance.IsRunningOnMono)
			{
				MouseWheelZoomType = (MouseWheelZoomType)1;
			}
			Overlays.CollectionChanged += Overlays_CollectionChanged;
		}
	}

	static GMapControl()
	{
		IsDesignerHosted = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
		BinaryFormatter = new BinaryFormatter();
		if (!IsDesignerHosted)
		{
			GMapImageProxy.Enable();
			GMaps.Instance.SQLitePing();
		}
	}

	private void Overlays_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems == null)
		{
			return;
		}
		foreach (GMapOverlay newItem in e.NewItems)
		{
			if (newItem != null)
			{
				newItem.Control = this;
			}
		}
		if (Core.IsStarted && !HoldInvalidation)
		{
			Invalidate();
		}
	}

	private void InvalidatorEngage(object sender, ProgressChangedEventArgs e)
	{
		((Control)this).Invalidate();
	}

	internal void ForceUpdateOverlays()
	{
		try
		{
			HoldInvalidation = true;
			foreach (GMapOverlay overlay in Overlays)
			{
				if (overlay.IsVisibile)
				{
					overlay.ForceUpdate();
				}
			}
		}
		finally
		{
			((Control)this).Refresh();
		}
	}

	public void UpdateMarkerLocalPosition(GMapMarker marker)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		GPoint val = FromLatLngToLocal(marker.Position);
		if (!MobileMode)
		{
			((GPoint)(ref val)).OffsetNegative(Core.RenderOffset);
		}
		marker.LocalPosition = new Point((int)(((GPoint)(ref val)).X + marker.Offset.X), (int)(((GPoint)(ref val)).Y + marker.Offset.Y));
	}

	public void UpdateRouteLocalPosition(GMapRoute route)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		route.LocalPoints.Clear();
		for (int i = 0; i < ((MapRoute)route).Points.Count; i++)
		{
			GPoint item = FromLatLngToLocal(((MapRoute)route).Points[i]);
			if (!MobileMode)
			{
				((GPoint)(ref item)).OffsetNegative(Core.RenderOffset);
			}
			route.LocalPoints.Add(item);
		}
		route.UpdateGraphicsPath();
	}

	public void UpdatePolygonLocalPosition(GMapPolygon polygon)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		polygon.LocalPoints.Clear();
		for (int i = 0; i < ((MapRoute)polygon).Points.Count; i++)
		{
			GPoint item = FromLatLngToLocal(((MapRoute)polygon).Points[i]);
			if (!MobileMode)
			{
				((GPoint)(ref item)).OffsetNegative(Core.RenderOffset);
			}
			polygon.LocalPoints.Add(item);
		}
		polygon.UpdateGraphicsPath();
	}

	public bool SetZoomToFitRect(RectLatLng rect)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (_lazyEvents)
		{
			_lazySetZoomToFitRect = rect;
		}
		else
		{
			int num = Core.GetMaxZoomToFitRect(rect);
			if (num > 0)
			{
				PointLatLng position = default(PointLatLng);
				((PointLatLng)(ref position))._002Ector(((RectLatLng)(ref rect)).Lat - ((RectLatLng)(ref rect)).HeightLat / 2.0, ((RectLatLng)(ref rect)).Lng + ((RectLatLng)(ref rect)).WidthLng / 2.0);
				Position = position;
				if (num > MaxZoom)
				{
					num = MaxZoom;
				}
				if ((int)Zoom != num)
				{
					Zoom = num;
				}
				return true;
			}
		}
		return false;
	}

	public bool ZoomAndCenterMarkers(string overlayId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		RectLatLng? rectOfAllMarkers = GetRectOfAllMarkers(overlayId);
		if (rectOfAllMarkers.HasValue)
		{
			return SetZoomToFitRect(rectOfAllMarkers.Value);
		}
		return false;
	}

	public bool ZoomAndCenterRoutes(string overlayId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		RectLatLng? rectOfAllRoutes = GetRectOfAllRoutes(overlayId);
		if (rectOfAllRoutes.HasValue)
		{
			return SetZoomToFitRect(rectOfAllRoutes.Value);
		}
		return false;
	}

	public bool ZoomAndCenterRoute(MapRoute route)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		RectLatLng? rectOfRoute = GetRectOfRoute(route);
		if (rectOfRoute.HasValue)
		{
			return SetZoomToFitRect(rectOfRoute.Value);
		}
		return false;
	}

	public RectLatLng? GetRectOfAllMarkers(string overlayId)
	{
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		RectLatLng? result = null;
		double num = double.MaxValue;
		double num2 = double.MinValue;
		double num3 = double.MinValue;
		double num4 = double.MaxValue;
		foreach (GMapOverlay overlay in Overlays)
		{
			if (((overlayId != null || !overlay.IsZoomSignificant) && !(overlay.Id == overlayId)) || !overlay.IsVisibile || overlay.Markers.Count <= 0)
			{
				continue;
			}
			foreach (GMapMarker marker in overlay.Markers)
			{
				if (marker.IsVisible)
				{
					PointLatLng position = marker.Position;
					if (((PointLatLng)(ref position)).Lng < num)
					{
						position = marker.Position;
						num = ((PointLatLng)(ref position)).Lng;
					}
					position = marker.Position;
					if (((PointLatLng)(ref position)).Lat > num2)
					{
						position = marker.Position;
						num2 = ((PointLatLng)(ref position)).Lat;
					}
					position = marker.Position;
					if (((PointLatLng)(ref position)).Lng > num3)
					{
						position = marker.Position;
						num3 = ((PointLatLng)(ref position)).Lng;
					}
					position = marker.Position;
					if (((PointLatLng)(ref position)).Lat < num4)
					{
						position = marker.Position;
						num4 = ((PointLatLng)(ref position)).Lat;
					}
				}
			}
		}
		if (num != double.MaxValue && num3 != double.MinValue && num2 != double.MinValue && num4 != double.MaxValue)
		{
			result = RectLatLng.FromLTRB(num, num2, num3, num4);
		}
		return result;
	}

	public RectLatLng? GetRectOfAllRoutes(string overlayId)
	{
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		RectLatLng? result = null;
		double num = double.MaxValue;
		double num2 = double.MinValue;
		double num3 = double.MinValue;
		double num4 = double.MaxValue;
		foreach (GMapOverlay overlay in Overlays)
		{
			if (((overlayId != null || !overlay.IsZoomSignificant) && !(overlay.Id == overlayId)) || !overlay.IsVisibile || overlay.Routes.Count <= 0)
			{
				continue;
			}
			foreach (GMapRoute route in overlay.Routes)
			{
				if (!route.IsVisible || !((MapRoute)route).From.HasValue || !((MapRoute)route).To.HasValue)
				{
					continue;
				}
				foreach (PointLatLng point in ((MapRoute)route).Points)
				{
					PointLatLng current3 = point;
					if (((PointLatLng)(ref current3)).Lng < num)
					{
						num = ((PointLatLng)(ref current3)).Lng;
					}
					if (((PointLatLng)(ref current3)).Lat > num2)
					{
						num2 = ((PointLatLng)(ref current3)).Lat;
					}
					if (((PointLatLng)(ref current3)).Lng > num3)
					{
						num3 = ((PointLatLng)(ref current3)).Lng;
					}
					if (((PointLatLng)(ref current3)).Lat < num4)
					{
						num4 = ((PointLatLng)(ref current3)).Lat;
					}
				}
			}
		}
		if (num != double.MaxValue && num3 != double.MinValue && num2 != double.MinValue && num4 != double.MaxValue)
		{
			result = RectLatLng.FromLTRB(num, num2, num3, num4);
		}
		return result;
	}

	public RectLatLng? GetRectOfRoute(MapRoute route)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		RectLatLng? result = null;
		double num = double.MaxValue;
		double num2 = double.MinValue;
		double num3 = double.MinValue;
		double num4 = double.MaxValue;
		if (route.From.HasValue && route.To.HasValue)
		{
			foreach (PointLatLng point in route.Points)
			{
				PointLatLng current = point;
				if (((PointLatLng)(ref current)).Lng < num)
				{
					num = ((PointLatLng)(ref current)).Lng;
				}
				if (((PointLatLng)(ref current)).Lat > num2)
				{
					num2 = ((PointLatLng)(ref current)).Lat;
				}
				if (((PointLatLng)(ref current)).Lng > num3)
				{
					num3 = ((PointLatLng)(ref current)).Lng;
				}
				if (((PointLatLng)(ref current)).Lat < num4)
				{
					num4 = ((PointLatLng)(ref current)).Lat;
				}
			}
			result = RectLatLng.FromLTRB(num, num2, num3, num4);
		}
		return result;
	}

	public Image ToImage()
	{
		Image val = null;
		bool forceDoubleBuffer = ForceDoubleBuffer;
		try
		{
			UpdateBackBuffer();
			if (!forceDoubleBuffer)
			{
				ForceDoubleBuffer = true;
			}
			((Control)this).Refresh();
			Application.DoEvents();
			using MemoryStream memoryStream = new MemoryStream();
			object obj = ((Image)_backBuffer).Clone();
			Bitmap val2 = (Bitmap)((obj is Bitmap) ? obj : null);
			try
			{
				((Image)val2).Save((Stream)memoryStream, ImageFormat.Png);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			return Image.FromStream((Stream)memoryStream);
		}
		catch (Exception)
		{
			throw;
		}
		finally
		{
			if (!forceDoubleBuffer)
			{
				ForceDoubleBuffer = false;
				ClearBackBuffer();
			}
		}
	}

	public void Offset(int x, int y)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (((Control)this).IsHandleCreated)
		{
			if (IsRotated)
			{
				Point[] array = new Point[1]
				{
					new Point(x, y)
				};
				_rotationMatrixInvert.TransformVectors(array);
				x = array[0].X;
				y = array[0].Y;
			}
			Core.DragOffset(new GPoint((long)x, (long)y));
			ForceUpdateOverlays();
		}
	}

	public double GetBearing(PointLatLng startPoint, PointLatLng endPoint)
	{
		double num = radians(((PointLatLng)(ref startPoint)).Lat);
		double num2 = radians(((PointLatLng)(ref startPoint)).Lng);
		double num3 = radians(((PointLatLng)(ref endPoint)).Lat);
		double num4 = radians(((PointLatLng)(ref endPoint)).Lng) - num2;
		double x = Math.Log(Math.Tan(num3 / 2.0 + Math.PI / 4.0) / Math.Tan(num / 2.0 + Math.PI / 4.0));
		if (Math.Abs(num4) > Math.PI)
		{
			num4 = ((!(num4 > 0.0)) ? (Math.PI * 2.0 + num4) : (0.0 - (Math.PI * 2.0 - num4)));
		}
		return Math.Round((degrees(Math.Atan2(num4, x)) + 360.0) % 360.0, 2);
	}

	public bool IsPointInBoundary(List<PointLatLng> points, string lat, string lng)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		GMapOverlay gMapOverlay = new GMapOverlay();
		GMapPolygon gMapPolygon = new GMapPolygon(points, "routePloygon")
		{
			Fill = (Brush)new SolidBrush(Color.FromArgb(50, Color.Red)),
			Stroke = new Pen(Color.Red, 1f)
		};
		gMapOverlay.Polygons.Add(gMapPolygon);
		PointLatLng p = default(PointLatLng);
		((PointLatLng)(ref p))._002Ector(double.Parse(lat), double.Parse(lng));
		return gMapPolygon.IsInside(p);
	}

	private double radians(double n)
	{
		return n * (Math.PI / 180.0);
	}

	private double degrees(double n)
	{
		return n * (180.0 / Math.PI);
	}

	protected override void OnLoad(EventArgs e)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			((UserControl)this).OnLoad(e);
			if (IsDesignerHosted)
			{
				return;
			}
			if (_lazyEvents)
			{
				_lazyEvents = false;
				if (_lazySetZoomToFitRect.HasValue)
				{
					SetZoomToFitRect(_lazySetZoomToFitRect.Value);
					_lazySetZoomToFitRect = null;
				}
			}
			Core.OnMapOpen().ProgressChanged += InvalidatorEngage;
			ForceUpdateOverlays();
		}
		catch (Exception ex)
		{
			if (this.OnExceptionThrown != null)
			{
				this.OnExceptionThrown.Invoke(ex);
				return;
			}
			throw;
		}
	}

	protected override void OnCreateControl()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		try
		{
			((UserControl)this).OnCreateControl();
			if (IsDesignerHosted)
			{
				return;
			}
			Form parentForm = ((ContainerControl)this).ParentForm;
			if (parentForm != null)
			{
				while (((ContainerControl)parentForm).ParentForm != null)
				{
					parentForm = ((ContainerControl)parentForm).ParentForm;
				}
				if (parentForm != null)
				{
					parentForm.FormClosing += new FormClosingEventHandler(ParentForm_FormClosing);
				}
			}
		}
		catch (Exception ex)
		{
			if (this.OnExceptionThrown != null)
			{
				this.OnExceptionThrown.Invoke(ex);
				return;
			}
			throw;
		}
	}

	private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)e.CloseReason == 1 || (int)e.CloseReason == 4)
		{
			Manager.CancelTileCaching();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Core.OnMapClose();
			Overlays.CollectionChanged -= Overlays_CollectionChanged;
			foreach (GMapOverlay overlay in Overlays)
			{
				overlay.Dispose();
			}
			Overlays.Clear();
			ScaleFont.Dispose();
			ScalePen.Dispose();
			CenterFormat.Dispose();
			CenterPen.Dispose();
			BottomFormat.Dispose();
			CopyrightFont.Dispose();
			EmptyTileBorders.Dispose();
			_emptyTileBrush.Dispose();
			_selectedAreaFill.Dispose();
			SelectionPen.Dispose();
			ClearBackBuffer();
		}
		((ContainerControl)this).Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		try
		{
			if (ForceDoubleBuffer)
			{
				if (_gxOff != null)
				{
					DrawGraphics(_gxOff);
					e.Graphics.DrawImage((Image)(object)_backBuffer, 0, 0);
				}
			}
			else
			{
				DrawGraphics(e.Graphics);
			}
			((Control)this).OnPaint(e);
		}
		catch (Exception ex)
		{
			if (this.OnExceptionThrown != null)
			{
				this.OnExceptionThrown.Invoke(ex);
				return;
			}
			throw;
		}
	}

	private void DrawGraphics(Graphics g)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		g.Clear(EmptyMapBackground);
		if (_mapRenderTransform.HasValue)
		{
			if (!MobileMode)
			{
				GPoint val;
				GPoint val2 = (val = new GPoint((long)(((Control)this).Width / 2), (long)(((Control)this).Height / 2)));
				((GPoint)(ref val)).OffsetNegative(Core.RenderOffset);
				GPoint val3 = val2;
				((GPoint)(ref val3)).OffsetNegative(val);
				g.ScaleTransform(_mapRenderTransform.Value, _mapRenderTransform.Value, (MatrixOrder)1);
				g.TranslateTransform((float)((GPoint)(ref val3)).X, (float)((GPoint)(ref val3)).Y, (MatrixOrder)1);
				DrawMap(g);
				g.ResetTransform();
				g.TranslateTransform((float)((GPoint)(ref val3)).X, (float)((GPoint)(ref val3)).Y, (MatrixOrder)1);
			}
			else
			{
				DrawMap(g);
				g.ResetTransform();
			}
			OnPaintOverlays(g);
		}
		else if (IsRotated)
		{
			g.TextRenderingHint = (TextRenderingHint)4;
			g.SmoothingMode = (SmoothingMode)4;
			g.TranslateTransform((float)((double)Core.Width / 2.0), (float)((double)Core.Height / 2.0));
			g.RotateTransform(0f - Bearing);
			g.TranslateTransform((float)((double)(-Core.Width) / 2.0), (float)((double)(-Core.Height) / 2.0));
			g.TranslateTransform((float)((GPoint)(ref Core.RenderOffset)).X, (float)((GPoint)(ref Core.RenderOffset)).Y);
			DrawMap(g);
			g.ResetTransform();
			g.TranslateTransform((float)((GPoint)(ref Core.RenderOffset)).X, (float)((GPoint)(ref Core.RenderOffset)).Y);
			OnPaintOverlays(g);
		}
		else
		{
			if (!MobileMode)
			{
				g.TranslateTransform((float)((GPoint)(ref Core.RenderOffset)).X, (float)((GPoint)(ref Core.RenderOffset)).Y);
			}
			DrawMap(g);
			OnPaintOverlays(g);
		}
	}

	private void DrawMap(Graphics g)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_07dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0800: Unknown result type (might be due to invalid IL or missing references)
		//IL_0801: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		if (Core.UpdatingBounds || MapProvider == EmptyProvider.Instance || MapProvider == null)
		{
			return;
		}
		Core.TileDrawingListLock.AcquireReaderLock();
		Core.Matrix.EnterReadLock();
		try
		{
			LoadTask key = default(LoadTask);
			foreach (DrawTile tileDrawing in Core.TileDrawingList)
			{
				((GRect)(ref Core.TileRect)).Location = tileDrawing.PosPixel;
				if (ForceDoubleBuffer && MobileMode)
				{
					((GRect)(ref Core.TileRect)).Offset(Core.RenderOffset);
				}
				((GRect)(ref Core.TileRect)).OffsetNegative(Core.CompensationOffset);
				bool flag = false;
				Tile tileWithNoLock = Core.Matrix.GetTileWithNoLock(Core.Zoom, tileDrawing.PosXY);
				if (tileWithNoLock.NotEmpty)
				{
					foreach (GMapImage overlay in ((Tile)(ref tileWithNoLock)).Overlays)
					{
						if (overlay == null || overlay.Img == null)
						{
							continue;
						}
						if (!flag)
						{
							flag = true;
						}
						if (!((PureImage)overlay).IsParent)
						{
							if (!_mapRenderTransform.HasValue && !IsRotated)
							{
								g.DrawImage(overlay.Img, (float)((GRect)(ref Core.TileRect)).X, (float)((GRect)(ref Core.TileRect)).Y, (float)((GRect)(ref Core.TileRect)).Width, (float)((GRect)(ref Core.TileRect)).Height);
							}
							else
							{
								g.DrawImage(overlay.Img, new Rectangle((int)((GRect)(ref Core.TileRect)).X, (int)((GRect)(ref Core.TileRect)).Y, (int)((GRect)(ref Core.TileRect)).Width, (int)((GRect)(ref Core.TileRect)).Height), 0f, 0f, (float)((GRect)(ref Core.TileRect)).Width, (float)((GRect)(ref Core.TileRect)).Height, (GraphicsUnit)2, _tileFlipXYAttributes);
							}
						}
						else
						{
							RectangleF rectangleF = new RectangleF(((PureImage)overlay).Xoff * (overlay.Img.Width / ((PureImage)overlay).Ix), ((PureImage)overlay).Yoff * (overlay.Img.Height / ((PureImage)overlay).Ix), overlay.Img.Width / ((PureImage)overlay).Ix, overlay.Img.Height / ((PureImage)overlay).Ix);
							Rectangle rectangle = new Rectangle((int)((GRect)(ref Core.TileRect)).X, (int)((GRect)(ref Core.TileRect)).Y, (int)((GRect)(ref Core.TileRect)).Width, (int)((GRect)(ref Core.TileRect)).Height);
							g.DrawImage(overlay.Img, rectangle, rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height, (GraphicsUnit)2, _tileFlipXYAttributes);
						}
					}
				}
				else if (FillEmptyTiles && MapProvider.Projection is MercatorProjection)
				{
					int num = 1;
					Tile val = Tile.Empty;
					long num2 = 0L;
					GPoint val2;
					while (!val.NotEmpty && num < Core.Zoom && num <= LevelsKeepInMemory)
					{
						num2 = (long)Math.Pow(2.0, num);
						TileMatrix matrix = Core.Matrix;
						int num3 = Core.Zoom - num++;
						val2 = tileDrawing.PosXY;
						long num4 = (int)(((GPoint)(ref val2)).X / num2);
						val2 = tileDrawing.PosXY;
						val = matrix.GetTileWithNoLock(num3, new GPoint(num4, (long)(int)(((GPoint)(ref val2)).Y / num2)));
					}
					if (val.NotEmpty)
					{
						val2 = tileDrawing.PosXY;
						long x = ((GPoint)(ref val2)).X;
						val2 = ((Tile)(ref val)).Pos;
						long num5 = Math.Abs(x - ((GPoint)(ref val2)).X * num2);
						val2 = tileDrawing.PosXY;
						long y = ((GPoint)(ref val2)).Y;
						val2 = ((Tile)(ref val)).Pos;
						long num6 = Math.Abs(y - ((GPoint)(ref val2)).Y * num2);
						foreach (GMapImage overlay2 in ((Tile)(ref val)).Overlays)
						{
							if (overlay2 != null && overlay2.Img != null && !((PureImage)overlay2).IsParent)
							{
								if (!flag)
								{
									flag = true;
								}
								RectangleF rectangleF2 = new RectangleF(num5 * (overlay2.Img.Width / num2), num6 * (overlay2.Img.Height / num2), overlay2.Img.Width / num2, overlay2.Img.Height / num2);
								Rectangle rectangle2 = new Rectangle((int)((GRect)(ref Core.TileRect)).X, (int)((GRect)(ref Core.TileRect)).Y, (int)((GRect)(ref Core.TileRect)).Width, (int)((GRect)(ref Core.TileRect)).Height);
								g.DrawImage(overlay2.Img, rectangle2, rectangleF2.X, rectangleF2.Y, rectangleF2.Width, rectangleF2.Height, (GraphicsUnit)2, _tileFlipXYAttributes);
								g.FillRectangle(_selectedAreaFill, rectangle2);
							}
						}
					}
				}
				if (!flag)
				{
					lock (Core.FailedLoads)
					{
						((LoadTask)(ref key))._002Ector(tileDrawing.PosXY, Core.Zoom, (Core)null);
						if (Core.FailedLoads.ContainsKey(key))
						{
							Exception ex = Core.FailedLoads[key];
							g.FillRectangle(_emptyTileBrush, new RectangleF(((GRect)(ref Core.TileRect)).X, ((GRect)(ref Core.TileRect)).Y, ((GRect)(ref Core.TileRect)).Width, ((GRect)(ref Core.TileRect)).Height));
							g.DrawString("Exception: " + ex.Message, MissingDataFont, Brushes.Red, new RectangleF(((GRect)(ref Core.TileRect)).X + 11, ((GRect)(ref Core.TileRect)).Y + 11, ((GRect)(ref Core.TileRect)).Width - 11, ((GRect)(ref Core.TileRect)).Height - 11));
							g.DrawString(EmptyTileText, MissingDataFont, Brushes.Blue, new RectangleF(((GRect)(ref Core.TileRect)).X, ((GRect)(ref Core.TileRect)).Y, ((GRect)(ref Core.TileRect)).Width, ((GRect)(ref Core.TileRect)).Height), CenterFormat);
							g.DrawRectangle(EmptyTileBorders, (int)((GRect)(ref Core.TileRect)).X, (int)((GRect)(ref Core.TileRect)).Y, (int)((GRect)(ref Core.TileRect)).Width, (int)((GRect)(ref Core.TileRect)).Height);
						}
					}
				}
				if (ShowTileGridLines)
				{
					g.DrawRectangle(EmptyTileBorders, (int)((GRect)(ref Core.TileRect)).X, (int)((GRect)(ref Core.TileRect)).Y, (int)((GRect)(ref Core.TileRect)).Width, (int)((GRect)(ref Core.TileRect)).Height);
					string obj = ((tileDrawing.PosXY == Core.CenterTileXYLocation) ? "CENTER: " : "TILE: ");
					DrawTile val3 = tileDrawing;
					g.DrawString(obj + ((object)(DrawTile)(ref val3)).ToString(), MissingDataFont, Brushes.Red, new RectangleF(((GRect)(ref Core.TileRect)).X, ((GRect)(ref Core.TileRect)).Y, ((GRect)(ref Core.TileRect)).Width, ((GRect)(ref Core.TileRect)).Height), CenterFormat);
				}
			}
		}
		finally
		{
			Core.Matrix.LeaveReadLock();
			Core.TileDrawingListLock.ReleaseReaderLock();
		}
	}

	protected virtual void OnPaintOverlays(Graphics g)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			g.SmoothingMode = (SmoothingMode)2;
			foreach (GMapOverlay overlay in Overlays)
			{
				if (overlay.IsVisibile)
				{
					overlay.OnRender(g);
				}
			}
			foreach (GMapOverlay overlay2 in Overlays)
			{
				if (overlay2.IsVisibile)
				{
					overlay2.OnRenderToolTips(g);
				}
			}
			if (!MobileMode)
			{
				g.ResetTransform();
			}
			RectLatLng selectedArea = SelectedArea;
			if (!((RectLatLng)(ref selectedArea)).IsEmpty)
			{
				selectedArea = SelectedArea;
				GPoint val = FromLatLngToLocal(((RectLatLng)(ref selectedArea)).LocationTopLeft);
				selectedArea = SelectedArea;
				GPoint val2 = FromLatLngToLocal(((RectLatLng)(ref selectedArea)).LocationRightBottom);
				long x = ((GPoint)(ref val)).X;
				long y = ((GPoint)(ref val)).Y;
				long x2 = ((GPoint)(ref val2)).X;
				long y2 = ((GPoint)(ref val2)).Y;
				g.DrawRectangle(SelectionPen, (float)x, (float)y, (float)(x2 - x), (float)(y2 - y));
				g.FillRectangle(_selectedAreaFill, (float)x, (float)y, (float)(x2 - x), (float)(y2 - y));
			}
			if (_renderHelperLine)
			{
				Point point = ((Control)this).PointToClient(Control.MousePosition);
				g.DrawLine(HelperLinePen, point.X, 0, point.X, ((Control)this).Height);
				g.DrawLine(HelperLinePen, 0, point.Y, ((Control)this).Width, point.Y);
			}
			if (ShowCenter)
			{
				g.DrawLine(CenterPen, ((Control)this).Width / 2 - 5, ((Control)this).Height / 2, ((Control)this).Width / 2 + 5, ((Control)this).Height / 2);
				g.DrawLine(CenterPen, ((Control)this).Width / 2, ((Control)this).Height / 2 - 5, ((Control)this).Width / 2, ((Control)this).Height / 2 + 5);
			}
			if (!string.IsNullOrEmpty(Core.Provider.Copyright))
			{
				g.DrawString(Core.Provider.Copyright, CopyrightFont, Brushes.Navy, 3f, (float)(((Control)this).Height - CopyrightFont.Height - 5));
			}
			if (MapScaleInfoEnabled)
			{
				int num = ((MapScaleInfoPosition == MapScaleInfoPosition.Top) ? 10 : (((Control)this).Bottom - 30));
				int num2 = 10;
				int bottom = num + 7;
				if (((Control)this).Width > Core.PxRes5000Km)
				{
					DrawScale(g, num, num2 + Core.PxRes5000Km, bottom, num2, "5000 km");
				}
				if (((Control)this).Width > Core.PxRes1000Km)
				{
					DrawScale(g, num, num2 + Core.PxRes1000Km, bottom, num2, "1000 km");
				}
				if (((Control)this).Width > Core.PxRes100Km && Zoom > 2.0)
				{
					DrawScale(g, num, num2 + Core.PxRes100Km, bottom, num2, "100 km");
				}
				if (((Control)this).Width > Core.PxRes10Km && Zoom > 5.0)
				{
					DrawScale(g, num, num2 + Core.PxRes10Km, bottom, num2, "10 km");
				}
				if (((Control)this).Width > Core.PxRes1000M && Zoom >= 10.0)
				{
					DrawScale(g, num, num2 + Core.PxRes1000M, bottom, num2, "1000 m");
				}
				if (((Control)this).Width > Core.PxRes100M && Zoom > 11.0)
				{
					DrawScale(g, num, num2 + Core.PxRes100M, bottom, num2, "100 m");
				}
			}
		}
		catch (Exception ex)
		{
			if (this.OnExceptionThrown != null)
			{
				this.OnExceptionThrown.Invoke(ex);
				return;
			}
			throw;
		}
	}

	private void DrawScale(Graphics g, int top, int right, int bottom, int left, string caption)
	{
		g.DrawLine(ScalePenBorder, left, top, left, bottom);
		g.DrawLine(ScalePenBorder, left, bottom, right, bottom);
		g.DrawLine(ScalePenBorder, right, bottom, right, top);
		g.DrawLine(ScalePen, left, top, left, bottom);
		g.DrawLine(ScalePen, left, bottom, right, bottom);
		g.DrawLine(ScalePen, right, bottom, right, top);
		g.DrawString(caption, ScaleFont, Brushes.Black, (float)(right + 3), (float)(top - 5));
	}

	private void UpdateRotationMatrix()
	{
		PointF pointF = new PointF(Core.Width / 2, Core.Height / 2);
		_rotationMatrix.Reset();
		_rotationMatrix.RotateAt(0f - Bearing, pointF);
		_rotationMatrixInvert.Reset();
		_rotationMatrixInvert.RotateAt(0f - Bearing, pointF);
		_rotationMatrixInvert.Invert();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		((Control)this).OnSizeChanged(e);
		if (((Control)this).Width == 0 || ((Control)this).Height == 0 || (((Control)this).Width == Core.Width && ((Control)this).Height == Core.Height) || IsDesignerHosted)
		{
			return;
		}
		if (ForceDoubleBuffer)
		{
			UpdateBackBuffer();
		}
		if (VirtualSizeEnabled)
		{
			Core.OnMapSizeChanged(Core.VWidth, Core.VHeight);
		}
		else
		{
			Core.OnMapSizeChanged(((Control)this).Width, ((Control)this).Height);
		}
		if (((Control)this).Visible && ((Control)this).IsHandleCreated && Core.IsStarted)
		{
			if (IsRotated)
			{
				UpdateRotationMatrix();
			}
			ForceUpdateOverlays();
		}
	}

	private void UpdateBackBuffer()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		ClearBackBuffer();
		_backBuffer = new Bitmap(((Control)this).Width, ((Control)this).Height);
		_gxOff = Graphics.FromImage((Image)(object)_backBuffer);
	}

	private void ClearBackBuffer()
	{
		if (_backBuffer != null)
		{
			((Image)_backBuffer).Dispose();
			_backBuffer = null;
		}
		if (_gxOff != null)
		{
			_gxOff.Dispose();
			_gxOff = null;
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		((UserControl)this).OnMouseDown(e);
		if (!IsMouseOverMarker)
		{
			if (e.Button == DragButton && CanDragMap)
			{
				Core.MouseDown = ApplyRotationInversion(e.X, e.Y);
				Invalidate();
			}
			else if (!_isSelected)
			{
				_isSelected = true;
				SelectedArea = RectLatLng.Empty;
				_selectionEnd = PointLatLng.Empty;
				_selectionStart = FromLocalToLatLng(e.X, e.Y);
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Invalid comparison between Unknown and I4
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).OnMouseUp(e);
		if (_isSelected)
		{
			_isSelected = false;
		}
		RectLatLng val;
		if (Core.IsDragging)
		{
			if (_isDragging)
			{
				_isDragging = false;
				((Control)this).Cursor = _cursorBefore;
				_cursorBefore = null;
			}
			Core.EndDrag();
			if (BoundsOfMap.HasValue)
			{
				val = BoundsOfMap.Value;
				if (!((RectLatLng)(ref val)).Contains(Position) && Core.LastLocationInBounds.HasValue)
				{
					Position = Core.LastLocationInBounds.Value;
				}
			}
			return;
		}
		if (e.Button == DragButton)
		{
			Core.MouseDown = GPoint.Empty;
		}
		if (!((PointLatLng)(ref _selectionEnd)).IsEmpty && !((PointLatLng)(ref _selectionStart)).IsEmpty)
		{
			bool zoomToFit = false;
			val = SelectedArea;
			if (!((RectLatLng)(ref val)).IsEmpty && (int)Control.ModifierKeys == 65536)
			{
				zoomToFit = SetZoomToFitRect(SelectedArea);
			}
			this.OnSelectionChange?.Invoke(SelectedArea, zoomToFit);
		}
		else
		{
			Invalidate();
		}
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).OnMouseClick(e);
		if (Core.IsDragging)
		{
			return;
		}
		bool flag = false;
		GPoint val = default(GPoint);
		GPoint val2 = default(GPoint);
		for (int num = Overlays.Count - 1; num >= 0; num--)
		{
			GMapOverlay gMapOverlay = Overlays[num];
			if (gMapOverlay != null && gMapOverlay.IsVisibile && gMapOverlay.IsHitTestVisible)
			{
				foreach (GMapMarker marker in gMapOverlay.Markers)
				{
					if (marker.IsVisible && marker.IsHitTestVisible)
					{
						((GPoint)(ref val))._002Ector((long)e.X, (long)e.Y);
						if (!MobileMode)
						{
							((GPoint)(ref val)).OffsetNegative(Core.RenderOffset);
						}
						if (marker.LocalArea.Contains((int)((GPoint)(ref val)).X, (int)((GPoint)(ref val)).Y))
						{
							this.OnMarkerClick?.Invoke(marker, e);
							flag = true;
							break;
						}
					}
				}
				foreach (GMapRoute route in gMapOverlay.Routes)
				{
					if (route.IsVisible && route.IsHitTestVisible)
					{
						((GPoint)(ref val2))._002Ector((long)e.X, (long)e.Y);
						if (!MobileMode)
						{
							((GPoint)(ref val2)).OffsetNegative(Core.RenderOffset);
						}
						if (route.IsInside((int)((GPoint)(ref val2)).X, (int)((GPoint)(ref val2)).Y))
						{
							this.OnRouteClick?.Invoke(route, e);
							flag = true;
							break;
						}
					}
				}
				foreach (GMapPolygon polygon in gMapOverlay.Polygons)
				{
					if (polygon.IsVisible && polygon.IsHitTestVisible && polygon.IsInside(FromLocalToLatLng(e.X, e.Y)))
					{
						this.OnPolygonClick?.Invoke(polygon, e);
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag && Core.MouseDown != GPoint.Empty)
		{
			this.OnMapClick?.Invoke(FromLocalToLatLng(e.X, e.Y), e);
		}
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).OnMouseDoubleClick(e);
		if (Core.IsDragging)
		{
			return;
		}
		bool flag = false;
		GPoint val = default(GPoint);
		GPoint val2 = default(GPoint);
		for (int num = Overlays.Count - 1; num >= 0; num--)
		{
			GMapOverlay gMapOverlay = Overlays[num];
			if (gMapOverlay != null && gMapOverlay.IsVisibile && gMapOverlay.IsHitTestVisible)
			{
				foreach (GMapMarker marker in gMapOverlay.Markers)
				{
					if (marker.IsVisible && marker.IsHitTestVisible)
					{
						((GPoint)(ref val))._002Ector((long)e.X, (long)e.Y);
						if (!MobileMode)
						{
							((GPoint)(ref val)).OffsetNegative(Core.RenderOffset);
						}
						if (marker.LocalArea.Contains((int)((GPoint)(ref val)).X, (int)((GPoint)(ref val)).Y))
						{
							this.OnMarkerDoubleClick?.Invoke(marker, e);
							flag = true;
							break;
						}
					}
				}
				foreach (GMapRoute route in gMapOverlay.Routes)
				{
					if (route.IsVisible && route.IsHitTestVisible)
					{
						((GPoint)(ref val2))._002Ector((long)e.X, (long)e.Y);
						if (!MobileMode)
						{
							((GPoint)(ref val2)).OffsetNegative(Core.RenderOffset);
						}
						if (route.IsInside((int)((GPoint)(ref val2)).X, (int)((GPoint)(ref val2)).Y))
						{
							this.OnRouteDoubleClick?.Invoke(route, e);
							flag = true;
							break;
						}
					}
				}
				foreach (GMapPolygon polygon in gMapOverlay.Polygons)
				{
					if (polygon.IsVisible && polygon.IsHitTestVisible && polygon.IsInside(FromLocalToLatLng(e.X, e.Y)))
					{
						this.OnPolygonDoubleClick?.Invoke(polygon, e);
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag && Core.MouseDown != GPoint.Empty)
		{
			this.OnMapDoubleClick?.Invoke(FromLocalToLatLng(e.X, e.Y), e);
		}
	}

	private GPoint ApplyRotationInversion(int x, int y)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		GPoint result = default(GPoint);
		((GPoint)(ref result))._002Ector((long)x, (long)y);
		if (IsRotated)
		{
			Point[] array = new Point[1]
			{
				new Point(x, y)
			};
			_rotationMatrixInvert.TransformPoints(array);
			Point point = array[0];
			((GPoint)(ref result)).X = point.X;
			((GPoint)(ref result)).Y = point.Y;
		}
		return result;
	}

	private GPoint ApplyRotation(int x, int y)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		GPoint result = default(GPoint);
		((GPoint)(ref result))._002Ector((long)x, (long)y);
		if (IsRotated)
		{
			Point[] array = new Point[1]
			{
				new Point(x, y)
			};
			_rotationMatrix.TransformPoints(array);
			Point point = array[0];
			((GPoint)(ref result)).X = point.X;
			((GPoint)(ref result)).Y = point.Y;
		}
		return result;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Invalid comparison between Unknown and I4
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Invalid comparison between Unknown and I4
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).OnMouseMove(e);
		if (!Core.IsDragging && !((GPoint)(ref Core.MouseDown)).IsEmpty)
		{
			GPoint val = ApplyRotationInversion(e.X, e.Y);
			if (Math.Abs(((GPoint)(ref val)).X - ((GPoint)(ref Core.MouseDown)).X) * 2 >= DragSize.Width || Math.Abs(((GPoint)(ref val)).Y - ((GPoint)(ref Core.MouseDown)).Y) * 2 >= DragSize.Height)
			{
				Core.BeginDrag(Core.MouseDown);
			}
		}
		if (Core.IsDragging)
		{
			if (!_isDragging)
			{
				_isDragging = true;
				_cursorBefore = ((Control)this).Cursor;
				((Control)this).Cursor = Cursors.SizeAll;
			}
			if (BoundsOfMap.HasValue)
			{
				RectLatLng value = BoundsOfMap.Value;
				if (!((RectLatLng)(ref value)).Contains(Position))
				{
					return;
				}
			}
			Core.MouseCurrent = ApplyRotationInversion(e.X, e.Y);
			Core.Drag(Core.MouseCurrent);
			if (MobileMode || IsRotated)
			{
				ForceUpdateOverlays();
			}
			((Control)this).Invalidate();
			return;
		}
		if (_isSelected && !((PointLatLng)(ref _selectionStart)).IsEmpty && ((int)Control.ModifierKeys == 262144 || (int)Control.ModifierKeys == 65536 || DisableAltForSelection))
		{
			_selectionEnd = FromLocalToLatLng(e.X, e.Y);
			PointLatLng selectionStart = _selectionStart;
			PointLatLng selectionEnd = _selectionEnd;
			double num = Math.Min(((PointLatLng)(ref selectionStart)).Lng, ((PointLatLng)(ref selectionEnd)).Lng);
			double num2 = Math.Max(((PointLatLng)(ref selectionStart)).Lat, ((PointLatLng)(ref selectionEnd)).Lat);
			double num3 = Math.Max(((PointLatLng)(ref selectionStart)).Lng, ((PointLatLng)(ref selectionEnd)).Lng);
			double num4 = Math.Min(((PointLatLng)(ref selectionStart)).Lat, ((PointLatLng)(ref selectionEnd)).Lat);
			SelectedArea = new RectLatLng(num2, num, num3 - num, num2 - num4);
		}
		else if (((GPoint)(ref Core.MouseDown)).IsEmpty)
		{
			GPoint val2 = default(GPoint);
			GPoint val3 = default(GPoint);
			GPoint val4 = default(GPoint);
			for (int num5 = Overlays.Count - 1; num5 >= 0; num5--)
			{
				GMapOverlay gMapOverlay = Overlays[num5];
				if (gMapOverlay != null && gMapOverlay.IsVisibile && gMapOverlay.IsHitTestVisible)
				{
					foreach (GMapMarker marker in gMapOverlay.Markers)
					{
						if (!marker.IsVisible || !marker.IsHitTestVisible)
						{
							continue;
						}
						((GPoint)(ref val2))._002Ector((long)e.X, (long)e.Y);
						if (!MobileMode)
						{
							((GPoint)(ref val2)).OffsetNegative(Core.RenderOffset);
						}
						if (marker.LocalArea.Contains((int)((GPoint)(ref val2)).X, (int)((GPoint)(ref val2)).Y))
						{
							if (!marker.IsMouseOver)
							{
								SetCursorHandOnEnter();
								marker.IsMouseOver = true;
								IsMouseOverMarker = true;
								this.OnMarkerEnter?.Invoke(marker);
								Invalidate();
							}
						}
						else if (marker.IsMouseOver)
						{
							marker.IsMouseOver = false;
							IsMouseOverMarker = false;
							RestoreCursorOnLeave();
							this.OnMarkerLeave?.Invoke(marker);
							Invalidate();
						}
					}
					foreach (GMapRoute route in gMapOverlay.Routes)
					{
						if (!route.IsVisible || !route.IsHitTestVisible)
						{
							continue;
						}
						((GPoint)(ref val3))._002Ector((long)e.X, (long)e.Y);
						if (!MobileMode)
						{
							((GPoint)(ref val3)).OffsetNegative(Core.RenderOffset);
						}
						if (route.IsInside((int)((GPoint)(ref val3)).X, (int)((GPoint)(ref val3)).Y))
						{
							if (!route.IsMouseOver)
							{
								SetCursorHandOnEnter();
								route.IsMouseOver = true;
								IsMouseOverRoute = true;
								this.OnRouteEnter?.Invoke(route);
								Invalidate();
							}
						}
						else if (route.IsMouseOver)
						{
							route.IsMouseOver = false;
							IsMouseOverRoute = false;
							RestoreCursorOnLeave();
							this.OnRouteLeave?.Invoke(route);
							Invalidate();
						}
					}
					foreach (GMapPolygon polygon in gMapOverlay.Polygons)
					{
						if (!polygon.IsVisible || !polygon.IsHitTestVisible)
						{
							continue;
						}
						((GPoint)(ref val4))._002Ector((long)e.X, (long)e.Y);
						if (!MobileMode)
						{
							((GPoint)(ref val4)).OffsetNegative(Core.RenderOffset);
						}
						if (polygon.IsInsideLocal((int)((GPoint)(ref val4)).X, (int)((GPoint)(ref val4)).Y))
						{
							if (!polygon.IsMouseOver)
							{
								SetCursorHandOnEnter();
								polygon.IsMouseOver = true;
								IsMouseOverPolygon = true;
								this.OnPolygonEnter?.Invoke(polygon);
								Invalidate();
							}
						}
						else if (polygon.IsMouseOver)
						{
							polygon.IsMouseOver = false;
							IsMouseOverPolygon = false;
							RestoreCursorOnLeave();
							this.OnPolygonLeave?.Invoke(polygon);
							Invalidate();
						}
					}
				}
			}
		}
		if (_renderHelperLine)
		{
			((Control)this).Invalidate();
		}
	}

	internal void RestoreCursorOnLeave()
	{
		if (OverObjectCount <= 0 && _cursorBefore != (Cursor)null)
		{
			OverObjectCount = 0;
			((Control)this).Cursor = _cursorBefore;
			_cursorBefore = null;
		}
	}

	internal void SetCursorHandOnEnter()
	{
		if (OverObjectCount <= 0 && ((Control)this).Cursor != Cursors.Hand)
		{
			OverObjectCount = 0;
			_cursorBefore = ((Control)this).Cursor;
			((Control)this).Cursor = Cursors.Hand;
		}
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
		if (!DisableFocusOnMouseEnter)
		{
			((Control)this).Focus();
		}
		_mouseIn = true;
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		_mouseIn = false;
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Invalid comparison between Unknown and I4
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Invalid comparison between Unknown and I4
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		((ScrollableControl)this).OnMouseWheel(e);
		if (!MouseWheelZoomEnabled || !_mouseIn || (IsMouseOverMarker && !IgnoreMarkerOnMouseWheel) || Core.IsDragging)
		{
			return;
		}
		if (((GPoint)(ref Core.MouseLastZoom)).X != e.X && ((GPoint)(ref Core.MouseLastZoom)).Y != e.Y)
		{
			if ((int)MouseWheelZoomType == 0)
			{
				Core._position = FromLocalToLatLng(e.X, e.Y);
			}
			else if ((int)MouseWheelZoomType == 2)
			{
				Core._position = FromLocalToLatLng(((Control)this).Width / 2, ((Control)this).Height / 2);
			}
			else if ((int)MouseWheelZoomType == 1)
			{
				Core._position = FromLocalToLatLng(e.X, e.Y);
			}
			((GPoint)(ref Core.MouseLastZoom)).X = e.X;
			((GPoint)(ref Core.MouseLastZoom)).Y = e.Y;
		}
		if ((int)MouseWheelZoomType != 1 && !GMaps.Instance.IsRunningOnMono)
		{
			Point point = ((Control)this).PointToScreen(new Point(((Control)this).Width / 2, ((Control)this).Height / 2));
			Stuff.SetCursorPos(point.X, point.Y);
		}
		Core.MouseWheelZooming = true;
		if (e.Delta > 0)
		{
			if (!InvertedMouseWheelZooming)
			{
				Zoom = (int)Zoom + 1;
			}
			else
			{
				Zoom = (int)(Zoom + 0.99) - 1;
			}
		}
		else if (e.Delta < 0)
		{
			if (!InvertedMouseWheelZooming)
			{
				Zoom = (int)(Zoom + 0.99) - 1;
			}
			else
			{
				Zoom = (int)Zoom + 1;
			}
		}
		Core.MouseWheelZooming = false;
	}

	public void ReloadMap()
	{
		Core.ReloadMap();
	}

	public GeoCoderStatusCode SetPositionByKeywords(string keys)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		GeoCoderStatusCode val = (GeoCoderStatusCode)0;
		GMapProvider mapProvider = MapProvider;
		GeocodingProvider val2 = (GeocodingProvider)(object)((mapProvider is GeocodingProvider) ? mapProvider : null);
		if (val2 == null)
		{
			val2 = (GeocodingProvider)(object)GMapProviders.OpenStreetMap;
		}
		if (val2 != null)
		{
			PointLatLng? point = val2.GetPoint(keys.Replace("#", "%23"), ref val);
			if ((int)val == 1 && point.HasValue)
			{
				Position = point.Value;
			}
		}
		return val;
	}

	public GeoCoderStatusCode GetPositionByKeywords(string keys, out PointLatLng point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		point = default(PointLatLng);
		GeoCoderStatusCode val = (GeoCoderStatusCode)0;
		GMapProvider mapProvider = MapProvider;
		GeocodingProvider val2 = (GeocodingProvider)(object)((mapProvider is GeocodingProvider) ? mapProvider : null);
		if (val2 == null)
		{
			val2 = (GeocodingProvider)(object)GMapProviders.OpenStreetMap;
		}
		if (val2 != null)
		{
			PointLatLng? point2 = val2.GetPoint(keys.Replace("#", "%23"), ref val);
			if ((int)val == 1 && point2.HasValue)
			{
				point = point2.Value;
			}
		}
		return val;
	}

	public PointLatLng FromLocalToLatLng(int x, int y)
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		if (_mapRenderTransform.HasValue)
		{
			x = (int)((float)((GPoint)(ref Core.RenderOffset)).X + (float)(x - ((GPoint)(ref Core.RenderOffset)).X) / _mapRenderTransform.Value);
			y = (int)((float)((GPoint)(ref Core.RenderOffset)).Y + (float)(y - ((GPoint)(ref Core.RenderOffset)).Y) / _mapRenderTransform.Value);
		}
		if (IsRotated)
		{
			Point[] array = new Point[1]
			{
				new Point(x, y)
			};
			_rotationMatrixInvert.TransformPoints(array);
			Point point = array[0];
			if (VirtualSizeEnabled)
			{
				point.X += (((Control)this).Width - Core.VWidth) / 2;
				point.Y += (((Control)this).Height - Core.VHeight) / 2;
			}
			x = point.X;
			y = point.Y;
		}
		return Core.FromLocalToLatLng((long)x, (long)y);
	}

	public GPoint FromLatLngToLocal(PointLatLng point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		GPoint result = Core.FromLatLngToLocal(point);
		if (_mapRenderTransform.HasValue)
		{
			((GPoint)(ref result)).X = (int)((float)((GPoint)(ref Core.RenderOffset)).X + (float)(((GPoint)(ref Core.RenderOffset)).X - ((GPoint)(ref result)).X) * (0f - _mapRenderTransform.Value));
			((GPoint)(ref result)).Y = (int)((float)((GPoint)(ref Core.RenderOffset)).Y + (float)(((GPoint)(ref Core.RenderOffset)).Y - ((GPoint)(ref result)).Y) * (0f - _mapRenderTransform.Value));
		}
		if (IsRotated)
		{
			Point[] array = new Point[1]
			{
				new Point((int)((GPoint)(ref result)).X, (int)((GPoint)(ref result)).Y)
			};
			_rotationMatrix.TransformPoints(array);
			Point point2 = array[0];
			if (VirtualSizeEnabled)
			{
				point2.X += (((Control)this).Width - Core.VWidth) / 2;
				point2.Y += (((Control)this).Height - Core.VHeight) / 2;
			}
			((GPoint)(ref result)).X = point2.X;
			((GPoint)(ref result)).Y = point2.Y;
		}
		return result;
	}

	public bool ShowExportDialog()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		FileDialog val = (FileDialog)new SaveFileDialog();
		try
		{
			val.CheckPathExists = true;
			val.CheckFileExists = false;
			val.AddExtension = true;
			val.DefaultExt = "gmdb";
			val.ValidateNames = true;
			val.Title = "GMap.NET: Export map to db, if file exsist only new data will be added";
			val.FileName = "DataExp";
			val.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			val.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
			val.FilterIndex = 1;
			val.RestoreDirectory = true;
			if ((int)((CommonDialog)val).ShowDialog() == 1)
			{
				bool num = GMaps.Instance.ExportToGMDB(val.FileName);
				if (num)
				{
					MessageBox.Show("Complete!", "GMap.NET", (MessageBoxButtons)0, (MessageBoxIcon)64);
				}
				else
				{
					MessageBox.Show("Failed!", "GMap.NET", (MessageBoxButtons)0, (MessageBoxIcon)48);
				}
				return num;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return false;
	}

	public bool ShowImportDialog()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		FileDialog val = (FileDialog)new OpenFileDialog();
		try
		{
			val.CheckPathExists = true;
			val.CheckFileExists = false;
			val.AddExtension = true;
			val.DefaultExt = "gmdb";
			val.ValidateNames = true;
			val.Title = "GMap.NET: Import to db, only new data will be added";
			val.FileName = "DataImport";
			val.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			val.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
			val.FilterIndex = 1;
			val.RestoreDirectory = true;
			if ((int)((CommonDialog)val).ShowDialog() == 1)
			{
				bool num = GMaps.Instance.ImportFromGMDB(val.FileName);
				if (num)
				{
					MessageBox.Show("Complete!", "GMap.NET", (MessageBoxButtons)0, (MessageBoxIcon)64);
					ReloadMap();
				}
				else
				{
					MessageBox.Show("Failed!", "GMap.NET", (MessageBoxButtons)0, (MessageBoxIcon)48);
				}
				return num;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return false;
	}

	public void SerializeOverlays(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		GMapOverlay[] array = new GMapOverlay[Overlays.Count];
		Overlays.CopyTo(array, 0);
		BinaryFormatter.Serialize(stream, array);
	}

	public void DeserializeOverlays(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		GMapOverlay[] array = BinaryFormatter.Deserialize(stream) as GMapOverlay[];
		foreach (GMapOverlay gMapOverlay in array)
		{
			gMapOverlay.Control = this;
			Overlays.Add(gMapOverlay);
		}
		ForceUpdateOverlays();
	}
}

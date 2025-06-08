using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using GMap.NET.WindowsForms.Properties;

namespace GMap.NET.WindowsForms.Markers;

[Serializable]
public class GMarkerGoogle : GMapMarker, ISerializable, IDeserializationCallback
{
	private Bitmap _bitmap;

	private Bitmap _bitmapShadow;

	private static Bitmap _arrowShadow;

	private static Bitmap _msmarkerShadow;

	private static Bitmap _shadowSmall;

	private static Bitmap _pushpinShadow;

	public readonly GMarkerGoogleType Type;

	private static readonly Dictionary<string, Bitmap> IconCache = new Dictionary<string, Bitmap>();

	public Bitmap Bitmap
	{
		get
		{
			return _bitmap;
		}
		set
		{
			_bitmap = value;
		}
	}

	public GMarkerGoogle(PointLatLng p, GMarkerGoogleType type)
		: base(p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Type = type;
		if (type != 0)
		{
			LoadBitmap();
		}
	}

	private void LoadBitmap()
	{
		Bitmap = GetIcon(Type.ToString());
		base.Size = new Size(((Image)Bitmap).Width, ((Image)Bitmap).Height);
		switch (Type)
		{
		case GMarkerGoogleType.arrow:
			base.Offset = new Point(-11, -base.Size.Height);
			if (_arrowShadow == null)
			{
				_arrowShadow = Resources.arrowshadow;
			}
			_bitmapShadow = _arrowShadow;
			break;
		case GMarkerGoogleType.blue:
		case GMarkerGoogleType.blue_dot:
		case GMarkerGoogleType.green:
		case GMarkerGoogleType.green_dot:
		case GMarkerGoogleType.yellow:
		case GMarkerGoogleType.yellow_dot:
		case GMarkerGoogleType.lightblue:
		case GMarkerGoogleType.lightblue_dot:
		case GMarkerGoogleType.orange:
		case GMarkerGoogleType.orange_dot:
		case GMarkerGoogleType.pink:
		case GMarkerGoogleType.pink_dot:
		case GMarkerGoogleType.purple:
		case GMarkerGoogleType.purple_dot:
		case GMarkerGoogleType.red:
		case GMarkerGoogleType.red_dot:
			base.Offset = new Point(-base.Size.Width / 2 + 1, -base.Size.Height + 1);
			if (_msmarkerShadow == null)
			{
				_msmarkerShadow = Resources.msmarker_shadow;
			}
			_bitmapShadow = _msmarkerShadow;
			break;
		case GMarkerGoogleType.blue_small:
		case GMarkerGoogleType.brown_small:
		case GMarkerGoogleType.gray_small:
		case GMarkerGoogleType.green_small:
		case GMarkerGoogleType.yellow_small:
		case GMarkerGoogleType.orange_small:
		case GMarkerGoogleType.purple_small:
		case GMarkerGoogleType.red_small:
		case GMarkerGoogleType.black_small:
		case GMarkerGoogleType.white_small:
			base.Offset = new Point(-base.Size.Width / 2, -base.Size.Height + 1);
			if (_shadowSmall == null)
			{
				_shadowSmall = Resources.shadow_small;
			}
			_bitmapShadow = _shadowSmall;
			break;
		case GMarkerGoogleType.green_big_go:
		case GMarkerGoogleType.yellow_big_pause:
		case GMarkerGoogleType.red_big_stop:
			base.Offset = new Point(-base.Size.Width / 2, -base.Size.Height + 1);
			if (_msmarkerShadow == null)
			{
				_msmarkerShadow = Resources.msmarker_shadow;
			}
			_bitmapShadow = _msmarkerShadow;
			break;
		case GMarkerGoogleType.blue_pushpin:
		case GMarkerGoogleType.green_pushpin:
		case GMarkerGoogleType.yellow_pushpin:
		case GMarkerGoogleType.lightblue_pushpin:
		case GMarkerGoogleType.pink_pushpin:
		case GMarkerGoogleType.purple_pushpin:
		case GMarkerGoogleType.red_pushpin:
			base.Offset = new Point(-9, -base.Size.Height + 1);
			if (_pushpinShadow == null)
			{
				_pushpinShadow = Resources.pushpin_shadow;
			}
			_bitmapShadow = _pushpinShadow;
			break;
		}
	}

	public GMarkerGoogle(PointLatLng p, Bitmap bitmap)
		: base(p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Bitmap = bitmap;
		base.Size = new Size(((Image)bitmap).Width, ((Image)bitmap).Height);
		base.Offset = new Point(-base.Size.Width / 2, -base.Size.Height);
	}

	internal static Bitmap GetIcon(string name)
	{
		if (!IconCache.TryGetValue(name, out var value))
		{
			object? @object = Resources.ResourceManager.GetObject(name, Resources.Culture);
			value = (Bitmap)((@object is Bitmap) ? @object : null);
			IconCache.Add(name, value);
		}
		return value;
	}

	public override void OnRender(Graphics g)
	{
		lock (Bitmap)
		{
			if (_bitmapShadow != null)
			{
				g.DrawImage((Image)(object)_bitmapShadow, base.LocalPosition.X, base.LocalPosition.Y, ((Image)_bitmapShadow).Width, ((Image)_bitmapShadow).Height);
			}
			g.DrawImage((Image)(object)Bitmap, base.LocalPosition.X, base.LocalPosition.Y, base.Size.Width, base.Size.Height);
		}
	}

	public override void Dispose()
	{
		if (Bitmap != null && !IconCache.ContainsValue(Bitmap))
		{
			((Image)Bitmap).Dispose();
			Bitmap = null;
		}
		base.Dispose();
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("type", Type);
		GetObjectData(info, context);
	}

	protected GMarkerGoogle(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		Type = Extensions.GetStruct<GMarkerGoogleType>(info, "type", GMarkerGoogleType.none);
	}

	public void OnDeserialization(object sender)
	{
		if (Type != 0)
		{
			LoadBitmap();
		}
	}
}

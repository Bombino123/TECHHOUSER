using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using GMap.NET.MapProviders;

namespace GMap.NET;

[Serializable]
public class MapRoute : ISerializable, IDeserializationCallback
{
	public readonly List<PointLatLng> Points = new List<PointLatLng>();

	public string Name;

	public object Tag;

	public string Duration;

	public List<string> Instructions = new List<string>();

	private PointLatLng[] deserializedPoints;

	public RouteStatusCode Status { get; set; }

	public string ErrorMessage { get; set; }

	public int ErrorCode { get; set; }

	public string WarningMessage { get; set; }

	public PointLatLng? From
	{
		get
		{
			if (Points.Count > 0)
			{
				return Points[0];
			}
			return null;
		}
	}

	public PointLatLng? To
	{
		get
		{
			if (Points.Count > 1)
			{
				return Points[Points.Count - 1];
			}
			return null;
		}
	}

	public double Distance
	{
		get
		{
			double num = 0.0;
			if (From.HasValue && To.HasValue)
			{
				for (int i = 1; i < Points.Count; i++)
				{
					num += GMapProviders.EmptyProvider.Projection.GetDistance(Points[i - 1], Points[i]);
				}
			}
			return Math.Round(num, 4);
		}
	}

	public MapRoute(string name)
	{
		Name = name;
	}

	public MapRoute(IEnumerable<PointLatLng> points, string name)
	{
		Points.AddRange(points);
		Name = name;
	}

	public MapRoute(MapRoute route)
	{
		if (route != null)
		{
			FieldInfo[] fields = route.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				fieldInfo.SetValue(this, fieldInfo.GetValue(route));
			}
		}
	}

	public double? DistanceTo(PointLatLng point)
	{
		if (Points.Count >= 2)
		{
			double num = DistanceToLinealRoute(Points[0], Points[1], point);
			for (int i = 2; i < Points.Count; i++)
			{
				double num2 = DistanceToLinealRoute(Points[i - 1], Points[i], point);
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}
		return null;
	}

	public static double DistanceToLinealRoute(PointLatLng start, PointLatLng to, PointLatLng point)
	{
		double num = (start.Lat - to.Lat) / (start.Lng - to.Lng);
		double num2 = 0.0 - (num * start.Lng - start.Lat);
		double lat = num * point.Lng + num2;
		double lng = (point.Lat - num2) / num;
		double distance = GMapProviders.EmptyProvider.Projection.GetDistance(new PointLatLng(point.Lat, lng), point);
		double distance2 = GMapProviders.EmptyProvider.Projection.GetDistance(new PointLatLng(lat, point.Lng), point);
		return ((distance <= distance2) ? distance : distance2) * 1000.0;
	}

	public void Clear()
	{
		Points.Clear();
		Tag = null;
		Name = null;
	}

	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Name", Name);
		info.AddValue("Tag", Tag);
		info.AddValue("Points", Points.ToArray());
	}

	protected MapRoute(SerializationInfo info, StreamingContext context)
	{
		Name = info.GetString("Name");
		Tag = Extensions.GetValue<object>(info, "Tag", null);
		deserializedPoints = Extensions.GetValue<PointLatLng[]>(info, "Points");
		Points = new List<PointLatLng>();
	}

	public virtual void OnDeserialization(object sender)
	{
		Points.AddRange(deserializedPoints);
		Points.TrimExcess();
	}
}

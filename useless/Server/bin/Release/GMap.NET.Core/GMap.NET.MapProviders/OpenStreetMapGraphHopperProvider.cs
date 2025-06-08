using System;
using System.Collections.Generic;
using System.Globalization;
using GMap.NET.Entity;
using GMap.NET.Internals;
using Newtonsoft.Json;

namespace GMap.NET.MapProviders;

public class OpenStreetMapGraphHopperProvider : OpenStreetMapProviderBase
{
	public static readonly OpenStreetMapGraphHopperProvider Instance;

	public string ApiKey = string.Empty;

	private GMapProvider[] _overlays;

	private static readonly string TravelTypeFoot;

	private static readonly string TravelTypeMotorCar;

	private static readonly string RoutingUrlFormat;

	private static readonly string ReverseGeocoderUrlFormat;

	private static readonly string GeocoderUrlFormat;

	public override Guid Id { get; } = new Guid("FAACDE73-4B90-5AE6-BB4A-ADE4F3545559");


	public override string Name { get; } = "OpenStreetMapGraphHopper";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					OpenStreetMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private OpenStreetMapGraphHopperProvider()
	{
		RefererUrl = "http://openseamap.org/";
	}

	static OpenStreetMapGraphHopperProvider()
	{
		TravelTypeFoot = "foot";
		TravelTypeMotorCar = "car";
		RoutingUrlFormat = "https://graphhopper.com/api/1/route?point={0},{1}&point={2},{3}&vehicle={4}&type=json";
		ReverseGeocoderUrlFormat = "https://graphhopper.com/api/1/geocode?point={0},{1}&locale=en&reverse=true";
		GeocoderUrlFormat = "https://graphhopper.com/api/1/geocode?q={0}&locale=en";
		Instance = new OpenStreetMapGraphHopperProvider();
	}

	public override MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int zoom)
	{
		return GetRoute(MakeRoutingUrl(start, end, walkingMode ? TravelTypeFoot : TravelTypeMotorCar));
	}

	public override MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom)
	{
		throw new NotImplementedException("use GetRoute(PointLatLng start, PointLatLng end...");
	}

	private string MakeRoutingUrl(PointLatLng start, PointLatLng end, string travelType)
	{
		return string.Format(CultureInfo.InvariantCulture, RoutingUrlFormat, start.Lat, start.Lng, end.Lat, end.Lng, travelType);
	}

	private MapRoute GetRoute(string url)
	{
		MapRoute mapRoute = null;
		OpenStreetMapGraphHopperRouteEntity openStreetMapGraphHopperRouteEntity = null;
		try
		{
			string text = (GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.RouteCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp((!string.IsNullOrEmpty(ApiKey)) ? (url + "&key=" + ApiKey) : url);
				if (!string.IsNullOrEmpty(text))
				{
					openStreetMapGraphHopperRouteEntity = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperRouteEntity>(text);
					if (GMaps.Instance.UseRouteCache && openStreetMapGraphHopperRouteEntity != null && openStreetMapGraphHopperRouteEntity.paths != null && openStreetMapGraphHopperRouteEntity.paths.Count > 0)
					{
						Cache.Instance.SaveContent(url, CacheType.RouteCache, text);
					}
				}
			}
			else
			{
				openStreetMapGraphHopperRouteEntity = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperRouteEntity>(text);
			}
			if (!string.IsNullOrEmpty(text))
			{
				mapRoute = new MapRoute("Route");
				if (openStreetMapGraphHopperRouteEntity != null && openStreetMapGraphHopperRouteEntity.paths != null && openStreetMapGraphHopperRouteEntity.paths.Count > 0)
				{
					mapRoute.Status = RouteStatusCode.OK;
					mapRoute.Duration = openStreetMapGraphHopperRouteEntity.paths[0].time.ToString();
					List<PointLatLng> list = new List<PointLatLng>();
					PureProjection.PolylineDecode(list, openStreetMapGraphHopperRouteEntity.paths[0].points);
					mapRoute.Points.AddRange(list);
					foreach (OpenStreetMapGraphHopperRouteEntity.Instruction instruction in openStreetMapGraphHopperRouteEntity.paths[0].instructions)
					{
						mapRoute.Instructions.Add(instruction.text);
					}
				}
			}
		}
		catch (Exception)
		{
			mapRoute = null;
		}
		return mapRoute;
	}

	public new GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
	{
		return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords), out pointList);
	}

	public new PointLatLng? GetPoint(string keywords, out GeoCoderStatusCode status)
	{
		status = GetPoints(keywords, out var pointList);
		if (pointList == null || pointList.Count <= 0)
		{
			return null;
		}
		return pointList[0];
	}

	public new GeoCoderStatusCode GetPoints(Placemark placemark, out List<PointLatLng> pointList)
	{
		throw new NotImplementedException("use GetPoint");
	}

	public new PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
	{
		throw new NotImplementedException("use GetPoint");
	}

	public new GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
	{
		placemarkList = GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location), out var status);
		return status;
	}

	public new Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
	{
		status = GetPlacemarks(location, out var placemarkList);
		if (placemarkList == null || placemarkList.Count <= 0)
		{
			return null;
		}
		return placemarkList[0];
	}

	private string MakeGeocoderUrl(string keywords)
	{
		return string.Format(GeocoderUrlFormat, keywords.Replace(' ', '+'));
	}

	private string MakeReverseGeocoderUrl(PointLatLng pt)
	{
		return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, pt.Lat, pt.Lng);
	}

	private GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
	{
		GeoCoderStatusCode result = GeoCoderStatusCode.UNKNOWN_ERROR;
		pointList = null;
		OpenStreetMapGraphHopperGeocodeEntity openStreetMapGraphHopperGeocodeEntity = null;
		try
		{
			string text = (GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(url, CacheType.GeocoderCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp((!string.IsNullOrEmpty(ApiKey)) ? (url + "&key=" + ApiKey) : url);
				if (!string.IsNullOrEmpty(text))
				{
					openStreetMapGraphHopperGeocodeEntity = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperGeocodeEntity>(text);
					if (GMaps.Instance.UseRouteCache && openStreetMapGraphHopperGeocodeEntity != null && openStreetMapGraphHopperGeocodeEntity.hits != null && openStreetMapGraphHopperGeocodeEntity.hits.Count > 0)
					{
						Cache.Instance.SaveContent(url, CacheType.GeocoderCache, text);
					}
				}
			}
			else
			{
				openStreetMapGraphHopperGeocodeEntity = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperGeocodeEntity>(text);
			}
			if (!string.IsNullOrEmpty(text))
			{
				pointList = new List<PointLatLng>();
				foreach (OpenStreetMapGraphHopperGeocodeEntity.Hit hit in openStreetMapGraphHopperGeocodeEntity.hits)
				{
					pointList.Add(new PointLatLng(hit.point.lat, hit.point.lng));
				}
				result = GeoCoderStatusCode.OK;
			}
		}
		catch (Exception)
		{
			result = GeoCoderStatusCode.EXCEPTION_IN_CODE;
		}
		return result;
	}

	private List<Placemark> GetPlacemarkFromReverseGeocoderUrl(string url, out GeoCoderStatusCode status)
	{
		status = GeoCoderStatusCode.UNKNOWN_ERROR;
		List<Placemark> list = null;
		OpenStreetMapGraphHopperGeocodeEntity openStreetMapGraphHopperGeocodeEntity = null;
		try
		{
			string text = (GMaps.Instance.UsePlacemarkCache ? Cache.Instance.GetContent(url, CacheType.PlacemarkCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp((!string.IsNullOrEmpty(ApiKey)) ? (url + "&key=" + ApiKey) : url);
				if (!string.IsNullOrEmpty(text))
				{
					openStreetMapGraphHopperGeocodeEntity = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperGeocodeEntity>(text);
					if (GMaps.Instance.UsePlacemarkCache && openStreetMapGraphHopperGeocodeEntity != null && openStreetMapGraphHopperGeocodeEntity.hits != null && openStreetMapGraphHopperGeocodeEntity.hits.Count > 0)
					{
						Cache.Instance.SaveContent(url, CacheType.PlacemarkCache, text);
					}
				}
			}
			else
			{
				openStreetMapGraphHopperGeocodeEntity = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperGeocodeEntity>(text);
			}
			if (!string.IsNullOrEmpty(text))
			{
				list = new List<Placemark>();
				foreach (OpenStreetMapGraphHopperGeocodeEntity.Hit hit in openStreetMapGraphHopperGeocodeEntity.hits)
				{
					Placemark placemark = new Placemark(hit.name);
					Placemark placemark2 = default(Placemark);
					placemark2.PlacemarkId = hit.osm_id;
					placemark2.Address = hit.name;
					placemark2.CountryName = hit.country;
					placemark2.CountryNameCode = hit.countrycode;
					placemark2.PostalCodeNumber = hit.postcode;
					placemark2.AdministrativeAreaName = hit.state;
					placemark2.SubAdministrativeAreaName = hit.city;
					placemark2.LocalityName = null;
					placemark2.ThoroughfareName = null;
					placemark = placemark2;
					list.Add(placemark);
				}
				status = GeoCoderStatusCode.OK;
			}
		}
		catch (Exception)
		{
			list = null;
			status = GeoCoderStatusCode.EXCEPTION_IN_CODE;
		}
		return list;
	}
}

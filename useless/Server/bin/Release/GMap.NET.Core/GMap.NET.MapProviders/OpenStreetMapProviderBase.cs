using System;
using System.Collections.Generic;
using System.Globalization;
using GMap.NET.Entity;
using GMap.NET.Internals;
using GMap.NET.Projections;
using Newtonsoft.Json;

namespace GMap.NET.MapProviders;

public abstract class OpenStreetMapProviderBase : GMapProvider, RoutingProvider, GeocodingProvider
{
	public readonly string ServerLetters = "abc";

	public int MinExpectedRank;

	private static readonly string RoutingUrlFormat = "http://router.project-osrm.org/route/v1/driving/{1},{0};{3},{2}";

	private static readonly string TravelTypeFoot = "foot";

	private static readonly string TravelTypeMotorCar = "motorcar";

	private static readonly string WalkingStr = "Walking";

	private static readonly string DrivingStr = "Driving";

	private static readonly string ReverseGeocoderUrlFormat = "https://nominatim.openstreetmap.org/reverse?format=json&lat={0}&lon={1}&zoom=18&addressdetails=1";

	private static readonly string GeocoderUrlFormat = "https://nominatim.openstreetmap.org/search?q={0}&format=json";

	private static readonly string GeocoderDetailedUrlFormat = "https://nominatim.openstreetmap.org/search?street={0}&city={1}&county={2}&state={3}&country={4}&postalcode={5}&format=json";

	public override Guid Id
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override string Name
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override PureProjection Projection => MercatorProjection.Instance;

	public override GMapProvider[] Overlays
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public OpenStreetMapProviderBase()
	{
		MaxZoom = null;
		Copyright = $"© OpenStreetMap - Map data ©{DateTime.Today.Year} OpenStreetMap";
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}

	public virtual MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int zoom)
	{
		return GetRoute(MakeRoutingUrl(start, end, walkingMode ? TravelTypeFoot : TravelTypeMotorCar));
	}

	public virtual MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom)
	{
		return GetRoute(MakeRoutingUrl(start, end, walkingMode ? TravelTypeFoot : TravelTypeMotorCar));
	}

	private string MakeRoutingUrl(PointLatLng start, PointLatLng end, string travelType)
	{
		return string.Format(CultureInfo.InvariantCulture, RoutingUrlFormat, start.Lat, start.Lng, end.Lat, end.Lng, travelType);
	}

	private string MakeRoutingUrl(string start, string end, string travelType)
	{
		return string.Format(CultureInfo.InvariantCulture, RoutingUrlFormat, start, end, travelType);
	}

	private MapRoute GetRoute(string url)
	{
		MapRoute mapRoute = null;
		OpenStreetMapRouteEntity openStreetMapRouteEntity = null;
		try
		{
			string text = (GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.RouteCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp(url);
				if (!string.IsNullOrEmpty(text))
				{
					openStreetMapRouteEntity = JsonConvert.DeserializeObject<OpenStreetMapRouteEntity>(text);
					if (GMaps.Instance.UseRouteCache && openStreetMapRouteEntity != null && openStreetMapRouteEntity.routes != null && openStreetMapRouteEntity.routes.Count > 0)
					{
						Cache.Instance.SaveContent(url, CacheType.RouteCache, text);
					}
				}
			}
			else
			{
				openStreetMapRouteEntity = JsonConvert.DeserializeObject<OpenStreetMapRouteEntity>(text);
			}
			if (!string.IsNullOrEmpty(text))
			{
				mapRoute = new MapRoute("Route");
				if (openStreetMapRouteEntity != null && openStreetMapRouteEntity.routes != null && openStreetMapRouteEntity.routes.Count > 0)
				{
					mapRoute.Status = RouteStatusCode.OK;
					mapRoute.Duration = openStreetMapRouteEntity.routes[0].duration.ToString();
					List<PointLatLng> list = new List<PointLatLng>();
					PureProjection.PolylineDecode(list, openStreetMapRouteEntity.routes[0].geometry);
					mapRoute.Points.AddRange(list);
				}
			}
		}
		catch (Exception)
		{
			mapRoute = null;
		}
		return mapRoute;
	}

	public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
	{
		return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords), out pointList);
	}

	public PointLatLng? GetPoint(string keywords, out GeoCoderStatusCode status)
	{
		status = GetPoints(keywords, out var pointList);
		if (pointList == null || pointList.Count <= 0)
		{
			return null;
		}
		return pointList[0];
	}

	public GeoCoderStatusCode GetPoints(Placemark placemark, out List<PointLatLng> pointList)
	{
		return GetLatLngFromGeocoderUrl(MakeDetailedGeocoderUrl(placemark), out pointList);
	}

	public PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
	{
		status = GetPoints(placemark, out var pointList);
		if (pointList == null || pointList.Count <= 0)
		{
			return null;
		}
		return pointList[0];
	}

	public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
	{
		placemarkList = GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location), out var status);
		return status;
	}

	public Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
	{
		List<Placemark> placemarkFromReverseGeocoderUrl = GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location), out status);
		if (placemarkFromReverseGeocoderUrl == null || placemarkFromReverseGeocoderUrl.Count <= 0)
		{
			return null;
		}
		return placemarkFromReverseGeocoderUrl[0];
	}

	private string MakeGeocoderUrl(string keywords)
	{
		return string.Format(GeocoderUrlFormat, keywords.Replace(' ', '+'));
	}

	private string MakeDetailedGeocoderUrl(Placemark placemark)
	{
		string text = string.Join(" ", placemark.HouseNo, placemark.ThoroughfareName).Trim();
		return string.Format(GeocoderDetailedUrlFormat, text.Replace(' ', '+'), placemark.LocalityName.Replace(' ', '+'), placemark.SubAdministrativeAreaName.Replace(' ', '+'), placemark.AdministrativeAreaName.Replace(' ', '+'), placemark.CountryName.Replace(' ', '+'), placemark.PostalCodeNumber.Replace(' ', '+'));
	}

	private string MakeReverseGeocoderUrl(PointLatLng pt)
	{
		return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, pt.Lat, pt.Lng);
	}

	private GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
	{
		GeoCoderStatusCode result = GeoCoderStatusCode.UNKNOWN_ERROR;
		pointList = null;
		List<OpenStreetMapGeocodeEntity> list = null;
		try
		{
			string text = (GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(url, CacheType.GeocoderCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp(url);
				if (!string.IsNullOrEmpty(text))
				{
					list = JsonConvert.DeserializeObject<List<OpenStreetMapGeocodeEntity>>(text);
					if (GMaps.Instance.UseGeocoderCache && list != null && list.Count > 0)
					{
						Cache.Instance.SaveContent(url, CacheType.GeocoderCache, text);
					}
				}
			}
			else
			{
				list = JsonConvert.DeserializeObject<List<OpenStreetMapGeocodeEntity>>(text);
			}
			if (!string.IsNullOrEmpty(text))
			{
				pointList = new List<PointLatLng>();
				foreach (OpenStreetMapGeocodeEntity item in list)
				{
					pointList.Add(new PointLatLng
					{
						Lat = item.lat,
						Lng = item.lon
					});
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
		OpenStreetMapGeocodeEntity openStreetMapGeocodeEntity = null;
		try
		{
			string text = (GMaps.Instance.UsePlacemarkCache ? Cache.Instance.GetContent(url, CacheType.PlacemarkCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp(url);
				if (!string.IsNullOrEmpty(text))
				{
					openStreetMapGeocodeEntity = JsonConvert.DeserializeObject<OpenStreetMapGeocodeEntity>(text);
					if (GMaps.Instance.UsePlacemarkCache && openStreetMapGeocodeEntity != null)
					{
						Cache.Instance.SaveContent(url, CacheType.PlacemarkCache, text);
					}
				}
			}
			else
			{
				openStreetMapGeocodeEntity = JsonConvert.DeserializeObject<OpenStreetMapGeocodeEntity>(text);
			}
			if (!string.IsNullOrEmpty(text))
			{
				list = new List<Placemark>();
				Placemark placemark = new Placemark(openStreetMapGeocodeEntity.display_name);
				Placemark placemark2 = default(Placemark);
				placemark2.PlacemarkId = openStreetMapGeocodeEntity.place_id;
				placemark2.Address = openStreetMapGeocodeEntity.address.ToString();
				placemark2.CountryName = openStreetMapGeocodeEntity.address.country;
				placemark2.CountryNameCode = openStreetMapGeocodeEntity.address.country_code;
				placemark2.PostalCodeNumber = openStreetMapGeocodeEntity.address.postcode;
				placemark2.AdministrativeAreaName = openStreetMapGeocodeEntity.address.state;
				placemark2.SubAdministrativeAreaName = openStreetMapGeocodeEntity.address.city;
				placemark2.LocalityName = openStreetMapGeocodeEntity.address.suburb;
				placemark2.ThoroughfareName = openStreetMapGeocodeEntity.address.road;
				placemark = placemark2;
				list.Add(placemark);
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

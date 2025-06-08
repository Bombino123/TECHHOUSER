using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GMap.NET.Internals;
using GMap.NET.Projections;
using Newtonsoft.Json;

namespace GMap.NET.MapProviders;

public abstract class GoogleMapProviderBase : GMapProvider, RoutingProvider, GeocodingProvider, DirectionsProvider, RoadsProvider
{
	public readonly string ServerAPIs = Stuff.GString("9gERyvblybF8iMuCt/LD6w==");

	public readonly string Server = Stuff.GString("gosr2U13BoS+bXaIxt6XWg==");

	public readonly string ServerChina = Stuff.GString("gosr2U13BoTEJoJJuO25gQ==");

	public readonly string ServerKorea = Stuff.GString("8ZVBOEsBinzi+zmP7y7pPA==");

	public readonly string ServerKoreaKr = Stuff.GString("gosr2U13BoQyz1gkC4QLfg==");

	public string SecureWord = "Galileo";

	public string ApiKey = string.Empty;

	private GMapProvider[] _overlays;

	public bool TryCorrectVersion = true;

	private static bool _init;

	private static readonly string Sec1 = "&s=";

	private static readonly string RouteUrlFormatPointLatLng = "https://maps.{6}/maps/api/directions/json?origin={2},{3}&destination={4},{5}&mode=driving";

	private static readonly string RouteUrlFormatStr = "http://maps.{4}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}";

	private static readonly string WalkingStr = "&mra=ls&dirflg=w";

	private static readonly string RouteWithoutHighwaysStr = "&mra=ls&dirflg=dh";

	private static readonly string RouteStr = "&mra=ls&dirflg=d";

	private static readonly string ReverseGeocoderUrlFormat = "https://maps.{0}/maps/api/geocode/json?latlng={1},{2}&language={3}&sensor=false";

	private static readonly string GeocoderUrlFormat = "https://maps.{0}/maps/api/geocode/json?address={1}&language={2}&sensor=false";

	private static readonly string DirectionUrlFormatStr = "https://maps.{7}/maps/api/directions/json?origin={0}&destination={1}&sensor={2}&language={3}{4}{5}{6}";

	private static readonly string DirectionUrlFormatPoint = "https://maps.{9}/maps/api/directions/json?origin={0},{1}&destination={2},{3}&sensor={4}&language={5}{6}{7}{8}";

	private static readonly string DirectionUrlFormatWaypoint = "https://maps.{8}/maps/api/directions/json?origin={0},{1}&waypoints={2}&destination={9},{10}&sensor={3}&language={4}{5}{6}{7}";

	private static readonly string DirectionUrlFormatWaypointStr = "https://maps.{7}/maps/api/directions/json?origin={0}&waypoints={1}&destination={8}&sensor={2}&language={3}{4}{5}{6}";

	private static readonly string RoadsUrlFormatStr = "https://roads.{2}/v1/snapToRoads?interpolate={0}&path={1}";

	private byte[] _privateKeyBytes;

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
			if (_overlays == null)
			{
				_overlays = new GMapProvider[1] { this };
			}
			return _overlays;
		}
	}

	public string ClientId { get; private set; } = string.Empty;


	public GoogleMapProviderBase()
	{
		MaxZoom = null;
		RefererUrl = $"https://maps.{Server}/";
		Copyright = string.Format("©{0} Google - Map data ©{0} Tele Atlas, Imagery ©{0} TerraMetrics", DateTime.Today.Year);
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}

	public override void OnInitialized()
	{
		if (_init || !TryCorrectVersion)
		{
			return;
		}
		string url = $"https://maps.{ServerAPIs}/maps/api/js?client=google-maps-lite&amp;libraries=search&amp;language=en&amp;region=";
		try
		{
			string text = (GMaps.Instance.UseUrlCache ? Cache.Instance.GetContent(url, CacheType.UrlCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp(url);
				if (!string.IsNullOrEmpty(text) && GMaps.Instance.UseUrlCache)
				{
					Cache.Instance.SaveContent(url, CacheType.UrlCache, text);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				Match match = new Regex($"https?://mts?\\d.{Server}/maps/vt\\?lyrs=m@(\\d*)", RegexOptions.IgnoreCase).Match(text);
				if (match.Success)
				{
					GroupCollection groups = match.Groups;
					if (groups.Count > 0)
					{
						string version = $"m@{groups[1].Value}";
						_ = GMapProviders.GoogleMap.Version;
						GMapProviders.GoogleMap.Version = version;
						GMapProviders.GoogleChinaMap.Version = version;
						string version2 = $"h@{groups[1].Value}";
						_ = GMapProviders.GoogleHybridMap.Version;
						GMapProviders.GoogleHybridMap.Version = version2;
						GMapProviders.GoogleChinaHybridMap.Version = version2;
					}
				}
				match = new Regex($"https?://khms?\\d.{Server}/kh\\?v=(\\d*)", RegexOptions.IgnoreCase).Match(text);
				if (match.Success)
				{
					GroupCollection groups2 = match.Groups;
					if (groups2.Count > 0)
					{
						string value = groups2[1].Value;
						_ = GMapProviders.GoogleSatelliteMap.Version;
						GMapProviders.GoogleSatelliteMap.Version = value;
						GMapProviders.GoogleKoreaSatelliteMap.Version = value;
						GMapProviders.GoogleChinaSatelliteMap.Version = "s@" + value;
					}
				}
				match = new Regex($"https?://mts?\\d.{Server}/maps/vt\\?lyrs=t@(\\d*),r@(\\d*)", RegexOptions.IgnoreCase).Match(text);
				if (match.Success)
				{
					GroupCollection groups3 = match.Groups;
					if (groups3.Count > 1)
					{
						string version3 = $"t@{groups3[1].Value},r@{groups3[2].Value}";
						_ = GMapProviders.GoogleTerrainMap.Version;
						GMapProviders.GoogleTerrainMap.Version = version3;
						GMapProviders.GoogleChinaTerrainMap.Version = version3;
					}
				}
			}
			_init = true;
		}
		catch (Exception)
		{
		}
	}

	internal void GetSecureWords(GPoint pos, out string sec1, out string sec2)
	{
		sec1 = string.Empty;
		sec2 = string.Empty;
		int length = (int)(pos.X * 3 + pos.Y) % 8;
		sec2 = SecureWord.Substring(0, length);
		if (pos.Y >= 10000 && pos.Y < 100000)
		{
			sec1 = Sec1;
		}
	}

	public virtual MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int zoom)
	{
		return GetRoute(MakeRouteUrl(start, end, GMapProvider.LanguageStr, avoidHighways, walkingMode));
	}

	public virtual MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom)
	{
		return GetRoute(MakeRouteUrl(start, end, GMapProvider.LanguageStr, avoidHighways, walkingMode));
	}

	private string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool walkingMode)
	{
		string text = (walkingMode ? WalkingStr : (avoidHighways ? RouteWithoutHighwaysStr : RouteStr));
		return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointLatLng, language, text, start.Lat, start.Lng, end.Lat, end.Lng, ServerAPIs);
	}

	private string MakeRouteUrl(string start, string end, string language, bool avoidHighways, bool walkingMode)
	{
		string text = (walkingMode ? WalkingStr : (avoidHighways ? RouteWithoutHighwaysStr : RouteStr));
		return string.Format(RouteUrlFormatStr, language, text, start.Replace(' ', '+'), end.Replace(' ', '+'), Server);
	}

	private MapRoute GetRoute(string url)
	{
		MapRoute mapRoute = null;
		StrucRute strucRute = null;
		try
		{
			string text = (GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.RouteCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp((!string.IsNullOrEmpty(ClientId)) ? GetSignedUri(url) : ((!string.IsNullOrEmpty(ApiKey)) ? (url + "&key=" + ApiKey) : url));
				if (!string.IsNullOrEmpty(text))
				{
					strucRute = JsonConvert.DeserializeObject<StrucRute>(text);
					if (GMaps.Instance.UseRouteCache && strucRute != null && strucRute.status == RouteStatusCode.OK)
					{
						Cache.Instance.SaveContent(url, CacheType.RouteCache, text);
					}
				}
			}
			else
			{
				strucRute = JsonConvert.DeserializeObject<StrucRute>(text);
			}
			if (strucRute != null)
			{
				mapRoute = ((strucRute.error != null || strucRute.routes == null || strucRute.routes.Count <= 0) ? new MapRoute("Route") : new MapRoute(strucRute.routes[0].summary));
				if (strucRute.error == null)
				{
					mapRoute.Status = strucRute.status;
					if (strucRute.routes != null && strucRute.routes.Count > 0 && strucRute.routes.Count > 0 && strucRute.routes[0].overview_polyline != null && strucRute.routes[0].overview_polyline.points != null)
					{
						List<PointLatLng> list = new List<PointLatLng>();
						PureProjection.PolylineDecode(list, strucRute.routes[0].overview_polyline.points);
						mapRoute.Points.Clear();
						mapRoute.Points.AddRange(list);
						mapRoute.Duration = strucRute.routes[0].legs[0].duration.text;
					}
				}
				else
				{
					mapRoute.ErrorCode = strucRute.error.code;
					mapRoute.ErrorMessage = strucRute.error.message;
					if (Enum.TryParse<RouteStatusCode>(strucRute.error.status, ignoreCase: false, out var result))
					{
						mapRoute.Status = result;
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

	public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
	{
		return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords, GMapProvider.LanguageStr), out pointList);
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
		throw new NotImplementedException("use GetPoints(string keywords...");
	}

	public PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
	{
		throw new NotImplementedException("use GetPoint(string keywords...");
	}

	public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
	{
		return GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location, GMapProvider.LanguageStr), out placemarkList);
	}

	public Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
	{
		status = GetPlacemarks(location, out var placemarkList);
		if (placemarkList == null || placemarkList.Count <= 0)
		{
			return null;
		}
		return placemarkList[0];
	}

	private string MakeGeocoderUrl(string keywords, string language)
	{
		return string.Format(CultureInfo.InvariantCulture, GeocoderUrlFormat, ServerAPIs, Uri.EscapeDataString(keywords).Replace(' ', '+'), language);
	}

	private string MakeReverseGeocoderUrl(PointLatLng pt, string language)
	{
		return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, ServerAPIs, pt.Lat, pt.Lng, language);
	}

	private GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
	{
		GeoCoderStatusCode result = GeoCoderStatusCode.UNKNOWN_ERROR;
		pointList = null;
		try
		{
			string text = (GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(url, CacheType.GeocoderCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			bool flag = false;
			if (string.IsNullOrEmpty(text))
			{
				string text2 = url;
				if (!string.IsNullOrEmpty(ClientId))
				{
					text2 = GetSignedUri(url);
				}
				else if (!string.IsNullOrEmpty(ApiKey))
				{
					text2 = text2 + "&key=" + ApiKey;
				}
				text = GetContentUsingHttp(text2);
				if (!string.IsNullOrEmpty(text))
				{
					flag = true;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				StrucGeocode strucGeocode = JsonConvert.DeserializeObject<StrucGeocode>(text);
				if (strucGeocode != null)
				{
					result = strucGeocode.status;
					if (strucGeocode.status == GeoCoderStatusCode.OK)
					{
						if (flag && GMaps.Instance.UseGeocoderCache)
						{
							Cache.Instance.SaveContent(url, CacheType.GeocoderCache, text);
						}
						pointList = new List<PointLatLng>();
						if (strucGeocode.results != null && strucGeocode.results.Count > 0)
						{
							for (int i = 0; i < strucGeocode.results.Count; i++)
							{
								pointList.Add(new PointLatLng(strucGeocode.results[i].geometry.location.lat, strucGeocode.results[i].geometry.location.lng));
							}
						}
					}
				}
			}
		}
		catch (Exception)
		{
			result = GeoCoderStatusCode.EXCEPTION_IN_CODE;
		}
		return result;
	}

	private GeoCoderStatusCode GetPlacemarkFromReverseGeocoderUrl(string url, out List<Placemark> placemarkList)
	{
		GeoCoderStatusCode result = GeoCoderStatusCode.UNKNOWN_ERROR;
		placemarkList = null;
		try
		{
			string text = (GMaps.Instance.UsePlacemarkCache ? Cache.Instance.GetContent(url, CacheType.PlacemarkCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			bool flag = false;
			if (string.IsNullOrEmpty(text))
			{
				string text2 = url;
				if (!string.IsNullOrEmpty(ClientId))
				{
					text2 = GetSignedUri(url);
				}
				else if (!string.IsNullOrEmpty(ApiKey))
				{
					text2 = text2 + "&key=" + ApiKey;
				}
				text = GetContentUsingHttp(text2);
				if (!string.IsNullOrEmpty(text))
				{
					flag = true;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				StrucGeocode strucGeocode = JsonConvert.DeserializeObject<StrucGeocode>(text);
				if (strucGeocode != null)
				{
					result = strucGeocode.status;
					if (strucGeocode.status == GeoCoderStatusCode.OK)
					{
						if (flag && GMaps.Instance.UseGeocoderCache)
						{
							Cache.Instance.SaveContent(url, CacheType.GeocoderCache, text);
						}
						placemarkList = new List<Placemark>();
						if (strucGeocode.results != null && strucGeocode.results.Count > 0)
						{
							for (int i = 0; i < strucGeocode.results.Count; i++)
							{
								Placemark item = new Placemark(strucGeocode.results[i].formatted_address);
								_ = strucGeocode.results[i].types;
								if (strucGeocode.results[i].address_components == null || strucGeocode.results[i].address_components.Count <= 0)
								{
									continue;
								}
								for (int j = 0; j < strucGeocode.results[i].address_components.Count; j++)
								{
									if (strucGeocode.results[i].address_components[j].types != null && strucGeocode.results[i].address_components[j].types.Count > 0)
									{
										switch (strucGeocode.results[i].address_components[j].types[0])
										{
										case "street_number":
											item.StreetNumber = strucGeocode.results[i].address_components[j].long_name;
											break;
										case "street_address":
											item.StreetAddress = strucGeocode.results[i].address_components[j].long_name;
											break;
										case "route":
											item.ThoroughfareName = strucGeocode.results[i].address_components[j].long_name;
											break;
										case "postal_code":
											item.PostalCodeNumber = strucGeocode.results[i].address_components[j].long_name;
											break;
										case "country":
											item.CountryName = strucGeocode.results[i].address_components[j].long_name;
											break;
										case "locality":
											item.LocalityName = strucGeocode.results[i].address_components[j].long_name;
											break;
										case "administrative_area_level_2":
											item.DistrictName = strucGeocode.results[i].address_components[j].long_name;
											break;
										case "administrative_area_level_1":
											item.AdministrativeAreaName = strucGeocode.results[i].address_components[j].long_name;
											break;
										case "administrative_area_level_3":
											item.SubAdministrativeAreaName = strucGeocode.results[i].address_components[j].long_name;
											break;
										case "neighborhood":
											item.Neighborhood = strucGeocode.results[i].address_components[j].long_name;
											break;
										}
									}
								}
								placemarkList.Add(item);
							}
						}
					}
				}
			}
		}
		catch (Exception)
		{
			result = GeoCoderStatusCode.EXCEPTION_IN_CODE;
			placemarkList = null;
		}
		return result;
	}

	public DirectionsStatusCode GetDirections(out GDirections direction, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		return GetDirectionsUrl(MakeDirectionsUrl(start, end, GMapProvider.LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
	}

	public DirectionsStatusCode GetDirections(out GDirections direction, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		return GetDirectionsUrl(MakeDirectionsUrl(start, end, GMapProvider.LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
	}

	public IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		throw new NotImplementedException();
	}

	public DirectionsStatusCode GetDirections(out GDirections direction, PointLatLng start, IEnumerable<PointLatLng> wayPoints, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		return GetDirectionsUrl(MakeDirectionsUrl(start, wayPoints, end, GMapProvider.LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
	}

	public DirectionsStatusCode GetDirections(out GDirections direction, string start, IEnumerable<string> wayPoints, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		return GetDirectionsUrl(MakeDirectionsUrl(start, wayPoints, end, GMapProvider.LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
	}

	private string MakeDirectionsUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		string text = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty);
		string text2 = "&units=" + (metric ? "metric" : "imperial");
		string text3 = "&mode=" + (walkingMode ? "walking" : "driving");
		return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatPoint, start.Lat, start.Lng, end.Lat, end.Lng, sensor.ToString().ToLower(), language, text, text2, text3, ServerAPIs);
	}

	private string MakeDirectionsUrl(string start, string end, string language, bool avoidHighways, bool walkingMode, bool avoidTolls, bool sensor, bool metric)
	{
		string text = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty);
		string text2 = "&units=" + (metric ? "metric" : "imperial");
		string text3 = "&mode=" + (walkingMode ? "walking" : "driving");
		return string.Format(DirectionUrlFormatStr, start.Replace(' ', '+'), end.Replace(' ', '+'), sensor.ToString().ToLower(), language, text, text2, text3, ServerAPIs);
	}

	private string MakeDirectionsUrl(PointLatLng start, IEnumerable<PointLatLng> wayPoints, PointLatLng end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		string text = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty);
		string text2 = "&units=" + (metric ? "metric" : "imperial");
		string text3 = "&mode=" + (walkingMode ? "walking" : "driving");
		string text4 = string.Empty;
		int num = 0;
		foreach (PointLatLng wayPoint in wayPoints)
		{
			text4 += string.Format(CultureInfo.InvariantCulture, (num++ == 0) ? "{0},{1}" : "|{0},{1}", wayPoint.Lat, wayPoint.Lng);
		}
		return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatWaypoint, start.Lat, start.Lng, text4, sensor.ToString().ToLower(), language, text, text2, text3, ServerAPIs, end.Lat, end.Lng);
	}

	private string MakeDirectionsUrl(string start, IEnumerable<string> wayPoints, string end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		string text = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty);
		string text2 = "&units=" + (metric ? "metric" : "imperial");
		string text3 = "&mode=" + (walkingMode ? "walking" : "driving");
		string text4 = string.Empty;
		int num = 0;
		foreach (string wayPoint in wayPoints)
		{
			text4 += string.Format(CultureInfo.InvariantCulture, (num++ == 0) ? "{0}" : "|{0}", wayPoint.Replace(' ', '+'));
		}
		return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatWaypointStr, start.Replace(' ', '+'), text4, sensor.ToString().ToLower(), language, text, text2, text3, ServerAPIs, end.Replace(' ', '+'));
	}

	private DirectionsStatusCode GetDirectionsUrl(string url, out GDirections direction)
	{
		DirectionsStatusCode directionsStatusCode = DirectionsStatusCode.UNKNOWN_ERROR;
		direction = null;
		try
		{
			string text = (GMaps.Instance.UseDirectionsCache ? Cache.Instance.GetContent(url, CacheType.DirectionsCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			bool flag = false;
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp((!string.IsNullOrEmpty(ClientId)) ? GetSignedUri(url) : ((!string.IsNullOrEmpty(ApiKey)) ? (url + "&key=" + ApiKey) : url));
				if (!string.IsNullOrEmpty(text))
				{
					flag = true;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				StrucDirection strucDirection = JsonConvert.DeserializeObject<StrucDirection>(text);
				if (strucDirection != null)
				{
					if (GMaps.Instance.UseDirectionsCache && flag)
					{
						Cache.Instance.SaveContent(url, CacheType.DirectionsCache, text);
					}
					directionsStatusCode = strucDirection.status;
					if (directionsStatusCode == DirectionsStatusCode.UNKNOWN_ERROR)
					{
						direction = new GDirections();
						if (strucDirection.routes != null && strucDirection.routes.Count > 0)
						{
							direction.Summary = strucDirection.routes[0].summary;
							if (strucDirection.routes[0].copyrights != null)
							{
								direction.Copyrights = strucDirection.routes[0].copyrights;
							}
							if (strucDirection.routes[0].overview_polyline != null && strucDirection.routes[0].overview_polyline.points != null)
							{
								direction.Route = new List<PointLatLng>();
								PureProjection.PolylineDecode(direction.Route, strucDirection.routes[0].overview_polyline.points);
							}
							if (strucDirection.routes[0].legs != null && strucDirection.routes[0].legs.Count > 0)
							{
								direction.Duration = string.Join(", ", strucDirection.routes[0].legs.Select((Leg x) => x.duration.text));
								direction.DurationValue = (uint)strucDirection.routes[0].legs.Sum((Leg x) => x.duration.value);
								direction.Distance = string.Join(", ", from x in strucDirection.routes[0].legs
									where x.distance != null
									select x.distance.text);
								direction.DistanceValue = (uint)strucDirection.routes[0].legs.Sum((Leg x) => x.distance?.value ?? 0);
								if (strucDirection.routes[0].legs[0].start_location != null)
								{
									direction.StartLocation.Lat = strucDirection.routes[0].legs[0].start_location.lat;
									direction.StartLocation.Lng = strucDirection.routes[0].legs[0].start_location.lng;
								}
								if (strucDirection.routes[0].legs[strucDirection.routes[0].legs.Count - 1].end_location != null)
								{
									direction.EndLocation.Lat = strucDirection.routes[0].legs[strucDirection.routes[0].legs.Count - 1].end_location.lat;
									direction.EndLocation.Lng = strucDirection.routes[0].legs[strucDirection.routes[0].legs.Count - 1].end_location.lng;
								}
								if (strucDirection.routes[0].legs[0].start_address != null)
								{
									direction.StartAddress = strucDirection.routes[0].legs[0].start_address;
								}
								if (strucDirection.routes[0].legs[strucDirection.routes[0].legs.Count - 1].end_address != null)
								{
									direction.EndAddress = strucDirection.routes[0].legs[strucDirection.routes[0].legs.Count - 1].end_address;
								}
								direction.Steps = new List<GDirectionStep>();
								foreach (Leg leg in strucDirection.routes[0].legs)
								{
									for (int i = 0; i < leg.steps.Count; i++)
									{
										GDirectionStep item = default(GDirectionStep);
										item.TravelMode = leg.steps[i].travel_mode;
										item.Duration = leg.steps[i].duration.text;
										item.Distance = leg.steps[i].distance.text;
										item.HtmlInstructions = leg.steps[i].html_instructions;
										if (leg.steps[i].start_location != null)
										{
											item.StartLocation.Lat = leg.steps[i].start_location.lat;
											item.StartLocation.Lng = leg.steps[i].start_location.lng;
										}
										if (leg.steps[i].end_location != null)
										{
											item.EndLocation.Lat = leg.steps[i].end_location.lat;
											item.EndLocation.Lng = leg.steps[i].end_location.lng;
										}
										if (leg.steps[i].polyline != null && leg.steps[i].polyline.points != null)
										{
											item.Points = new List<PointLatLng>();
											PureProjection.PolylineDecode(item.Points, leg.steps[i].polyline.points);
										}
										direction.Steps.Add(item);
									}
								}
							}
						}
					}
				}
			}
		}
		catch (Exception)
		{
			direction = null;
			directionsStatusCode = DirectionsStatusCode.EXCEPTION_IN_CODE;
		}
		return directionsStatusCode;
	}

	public virtual MapRoute GetRoadsRoute(List<PointLatLng> points, bool interpolate)
	{
		return GetRoadsRoute(MakeRoadsUrl(points, interpolate.ToString()));
	}

	public virtual MapRoute GetRoadsRoute(string points, bool interpolate)
	{
		return GetRoadsRoute(MakeRoadsUrl(points, interpolate.ToString()));
	}

	private string MakeRoadsUrl(List<PointLatLng> points, string interpolate)
	{
		string text = "";
		foreach (PointLatLng point in points)
		{
			text += string.Format("{2}{0},{1}", point.Lat, point.Lng, (text == "") ? "" : "|");
		}
		return string.Format(RoadsUrlFormatStr, interpolate, text, ServerAPIs);
	}

	private string MakeRoadsUrl(string points, string interpolate)
	{
		return string.Format(RoadsUrlFormatStr, interpolate, points, Server);
	}

	private MapRoute GetRoadsRoute(string url)
	{
		MapRoute mapRoute = null;
		StrucRoads strucRoads = null;
		try
		{
			string text = (GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.RouteCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp((!string.IsNullOrEmpty(ClientId)) ? GetSignedUri(url) : ((!string.IsNullOrEmpty(ApiKey)) ? (url + "&key=" + ApiKey) : url));
				if (!string.IsNullOrEmpty(text))
				{
					strucRoads = JsonConvert.DeserializeObject<StrucRoads>(text);
					if (GMaps.Instance.UseRouteCache && strucRoads != null && strucRoads.error == null && strucRoads.snappedPoints != null && strucRoads.snappedPoints.Count > 0)
					{
						Cache.Instance.SaveContent(url, CacheType.RouteCache, text);
					}
				}
			}
			else
			{
				strucRoads = JsonConvert.DeserializeObject<StrucRoads>(text);
			}
			if (strucRoads != null)
			{
				mapRoute = new MapRoute("Route");
				mapRoute.WarningMessage = strucRoads.warningMessage;
				if (strucRoads.error == null)
				{
					if (strucRoads.snappedPoints != null && strucRoads.snappedPoints.Count > 0)
					{
						mapRoute.Points.Clear();
						foreach (StrucRoads.SnappedPoint snappedPoint in strucRoads.snappedPoints)
						{
							mapRoute.Points.Add(new PointLatLng(snappedPoint.location.latitude, snappedPoint.location.longitude));
						}
						mapRoute.Status = RouteStatusCode.OK;
					}
				}
				else
				{
					mapRoute.ErrorCode = strucRoads.error.code;
					mapRoute.ErrorMessage = strucRoads.error.message;
					if (Enum.TryParse<RouteStatusCode>(strucRoads.error.status, ignoreCase: false, out var result))
					{
						mapRoute.Status = result;
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

	public void SetEnterpriseCredentials(string clientId, string privateKey)
	{
		privateKey = privateKey.Replace("-", "+").Replace("_", "/");
		_privateKeyBytes = Convert.FromBase64String(privateKey);
		ClientId = clientId;
	}

	private string GetSignedUri(Uri uri)
	{
		UriBuilder uriBuilder = new UriBuilder(uri);
		uriBuilder.Query = uriBuilder.Query.Substring(1) + "&client=" + ClientId;
		uri = uriBuilder.Uri;
		string signature = GetSignature(uri);
		return uri.Scheme + "://" + uri.Host + uri.LocalPath + uri.Query + "&signature=" + signature;
	}

	private string GetSignedUri(string url)
	{
		return GetSignedUri(new Uri(url));
	}

	private string GetSignature(Uri uri)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(uri.LocalPath + uri.Query);
		return Convert.ToBase64String(new HMACSHA1(_privateKeyBytes).ComputeHash(bytes)).Replace("+", "-").Replace("/", "_");
	}
}

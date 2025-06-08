using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using GMap.NET.Internals;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class BingMapProviderBase : GMapProvider, RoutingProvider, GeocodingProvider
{
	public string Version = "4810";

	public string ClientKey = string.Empty;

	internal string SessionId = string.Empty;

	public bool ForceSessionIdOnTileAccess;

	public bool DisableDynamicTileUrlFormat;

	private GMapProvider[] _overlays;

	public bool TryCorrectVersion = true;

	public bool TryGetDefaultKey = true;

	private static bool _init;

	private static readonly string RouteUrlFormatPointLatLng = "http://dev.virtualearth.net/REST/V1/Routes/{0}?o=xml&wp.0={1},{2}&wp.1={3},{4}{5}&optmz=distance&rpo=Points&key={6}";

	private static readonly string RouteUrlFormatPointQueries = "http://dev.virtualearth.net/REST/V1/Routes/{0}?o=xml&wp.0={1}&wp.1={2}{3}&optmz=distance&rpo=Points&key={4}";

	private static readonly string RouteUrlFormatListPointLatLng = "http://dev.virtualearth.net/REST/V1/Routes/{0}?o=xml{1}{2}&optmz=distance&rpo=Points&key={3}";

	private static readonly string GeocoderUrlFormat = "http://dev.virtualearth.net/REST/v1/Locations?{0}&o=xml&key={1}";

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

	public BingMapProviderBase()
	{
		MaxZoom = null;
		RefererUrl = "http://www.bing.com/maps/";
		Copyright = string.Format("©{0} Microsoft Corporation, ©{0} NAVTEQ, ©{0} Image courtesy of NASA", DateTime.Today.Year);
	}

	internal string TileXYToQuadKey(long tileX, long tileY, int levelOfDetail)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int num = levelOfDetail; num > 0; num--)
		{
			char c = '0';
			int num2 = 1 << num - 1;
			if ((tileX & num2) != 0L)
			{
				c = (char)(c + 1);
			}
			if ((tileY & num2) != 0L)
			{
				c = (char)(c + 1);
				c = (char)(c + 1);
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	internal void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int levelOfDetail)
	{
		tileX = (tileY = 0);
		levelOfDetail = quadKey.Length;
		for (int num = levelOfDetail; num > 0; num--)
		{
			int num2 = 1 << num - 1;
			switch (quadKey[levelOfDetail - num])
			{
			case '1':
				tileX |= num2;
				break;
			case '2':
				tileY |= num2;
				break;
			case '3':
				tileX |= num2;
				tileY |= num2;
				break;
			default:
				throw new ArgumentException("Invalid QuadKey digit sequence.");
			case '0':
				break;
			}
		}
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}

	public override void OnInitialized()
	{
		if (_init)
		{
			return;
		}
		try
		{
			string text = ClientKey;
			if (TryGetDefaultKey && string.IsNullOrEmpty(ClientKey))
			{
				text = Stuff.GString("Jq7FrGTyaYqcrvv9ugBKv4OVSKnmzpigqZtdvtcDdgZexmOZ2RugOexFSmVzTAhOWiHrdhFoNCoySnNF3MyyIOo5u2Y9rj88");
			}
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = (GMaps.Instance.UseUrlCache ? Cache.Instance.GetContent("BingLoggingServiceV1" + text, CacheType.UrlCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
				if (string.IsNullOrEmpty(text2))
				{
					text2 = GetContentUsingHttp($"http://dev.virtualearth.net/webservices/v1/LoggingService/LoggingService.svc/Log?entry=0&fmt=1&type=3&group=MapControl&name=AJAX&mkt=en-us&auth={text}&jsonp=microsoftMapsNetworkCallback");
					if (!string.IsNullOrEmpty(text2) && text2.Contains("ValidCredentials") && GMaps.Instance.UseUrlCache)
					{
						Cache.Instance.SaveContent("BingLoggingServiceV1" + text, CacheType.UrlCache, text2);
					}
				}
				if (!string.IsNullOrEmpty(text2) && text2.Contains("sessionId") && text2.Contains("ValidCredentials"))
				{
					SessionId = text2.Split(new char[1] { ',' })[0].Split(new char[1] { ':' })[1].Replace("\"", string.Empty).Replace(" ", string.Empty);
				}
			}
			if (TryCorrectVersion && DisableDynamicTileUrlFormat)
			{
				string url = "http://www.bing.com/maps";
				string text3 = (GMaps.Instance.UseUrlCache ? Cache.Instance.GetContent(url, CacheType.UrlCache, TimeSpan.FromDays(GMapProvider.TTLCache)) : string.Empty);
				if (string.IsNullOrEmpty(text3))
				{
					text3 = GetContentUsingHttp(url);
					if (!string.IsNullOrEmpty(text3) && GMaps.Instance.UseUrlCache)
					{
						Cache.Instance.SaveContent(url, CacheType.UrlCache, text3);
					}
				}
				if (!string.IsNullOrEmpty(text3))
				{
					Match match = new Regex("tilegeneration:(\\d*)", RegexOptions.IgnoreCase).Match(text3);
					if (match.Success)
					{
						GroupCollection groups = match.Groups;
						if (groups.Count == 2)
						{
							string value = groups[1].Value;
							string version = GMapProviders.BingMap.Version;
							if (value != version)
							{
								GMapProviders.BingMap.Version = value;
								GMapProviders.BingSatelliteMap.Version = value;
								GMapProviders.BingHybridMap.Version = value;
								GMapProviders.BingOSMap.Version = value;
							}
						}
					}
				}
			}
			_init = true;
		}
		catch (Exception)
		{
		}
	}

	protected override bool CheckTileImageHttpResponse(WebResponse response)
	{
		bool flag = base.CheckTileImageHttpResponse(response);
		if (flag)
		{
			string text = response.Headers.Get("X-VE-Tile-Info");
			if (text != null)
			{
				return !text.Equals("no-tile");
			}
		}
		return flag;
	}

	internal string GetTileUrl(string imageryType)
	{
		string result = string.Empty;
		if (!string.IsNullOrEmpty(SessionId))
		{
			try
			{
				string url = "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/" + imageryType + "?output=xml&key=" + SessionId;
				string text = (GMaps.Instance.UseUrlCache ? Cache.Instance.GetContent("GetTileUrl" + imageryType, CacheType.UrlCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
				bool flag = false;
				if (string.IsNullOrEmpty(text))
				{
					text = GetContentUsingHttp(url);
					flag = true;
				}
				if (!string.IsNullOrEmpty(text))
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(text);
					XmlNode xmlNode = xmlDocument["Response"];
					if (string.Compare(xmlNode["StatusCode"].InnerText, "200", ignoreCase: true) == 0)
					{
						xmlNode = xmlNode["ResourceSets"]["ResourceSet"]["Resources"];
						foreach (XmlNode childNode in xmlNode.ChildNodes)
						{
							XmlNode xmlNode2 = childNode["ImageUrl"];
							if (xmlNode2 != null && !string.IsNullOrEmpty(xmlNode2.InnerText))
							{
								if (flag && GMaps.Instance.UseUrlCache)
								{
									Cache.Instance.SaveContent("GetTileUrl" + imageryType, CacheType.UrlCache, text);
								}
								string text2 = xmlNode2.InnerText;
								if (text2.Contains("{key}") || text2.Contains("{token}"))
								{
									text2.Replace("{key}", SessionId).Replace("{token}", SessionId);
								}
								else if (ForceSessionIdOnTileAccess)
								{
									text2 = text2 + "&key=" + SessionId;
								}
								result = text2;
								break;
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}
		return result;
	}

	public MapRoute GetRoute(List<PointLatLng> list, bool avoidHighways, bool walkingMode, int zoom)
	{
		MapRoute result = null;
		string tooltipHtml;
		int numLevel;
		int zoomFactor;
		List<PointLatLng> routePoints = GetRoutePoints(MakeRouteUrl(list, GMapProvider.LanguageStr, avoidHighways, walkingMode), zoom, out tooltipHtml, out numLevel, out zoomFactor);
		if (routePoints != null)
		{
			result = new MapRoute(routePoints, tooltipHtml);
		}
		return result;
	}

	public MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int zoom)
	{
		MapRoute result = null;
		string tooltipHtml;
		int numLevel;
		int zoomFactor;
		List<PointLatLng> routePoints = GetRoutePoints(MakeRouteUrl(start, end, GMapProvider.LanguageStr, avoidHighways, walkingMode), zoom, out tooltipHtml, out numLevel, out zoomFactor);
		if (routePoints != null)
		{
			result = new MapRoute(routePoints, tooltipHtml);
		}
		return result;
	}

	public MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom)
	{
		MapRoute result = null;
		string tooltipHtml;
		int numLevel;
		int zoomFactor;
		List<PointLatLng> routePoints = GetRoutePoints(MakeRouteUrl(start, end, GMapProvider.LanguageStr, avoidHighways, walkingMode), zoom, out tooltipHtml, out numLevel, out zoomFactor);
		if (routePoints != null)
		{
			result = new MapRoute(routePoints, tooltipHtml);
		}
		return result;
	}

	private string MakeRouteUrl(List<PointLatLng> list, string languageStr, bool avoidHighways, bool walkingMode)
	{
		string text = (avoidHighways ? "&avoid=highways" : string.Empty);
		string text2 = (walkingMode ? "Walking" : "Driving");
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			PointLatLng pointLatLng = list[i];
			stringBuilder.Append($"&wp.{i}={pointLatLng.Lat},{pointLatLng.Lng}");
		}
		return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatListPointLatLng, text2, stringBuilder.ToString(), text, ClientKey);
	}

	private string MakeRouteUrl(string start, string end, string language, bool avoidHighways, bool walkingMode)
	{
		string text = (avoidHighways ? "&avoid=highways" : string.Empty);
		string text2 = (walkingMode ? "Walking" : "Driving");
		return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointQueries, text2, start, end, text, ClientKey);
	}

	private string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool walkingMode)
	{
		string text = (avoidHighways ? "&avoid=highways" : string.Empty);
		string text2 = (walkingMode ? "Walking" : "Driving");
		return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointLatLng, text2, start.Lat, start.Lng, end.Lat, end.Lng, text, ClientKey);
	}

	private List<PointLatLng> GetRoutePoints(string url, int zoom, out string tooltipHtml, out int numLevel, out int zoomFactor)
	{
		List<PointLatLng> list = null;
		tooltipHtml = string.Empty;
		numLevel = -1;
		zoomFactor = -1;
		try
		{
			string text = (GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.RouteCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp(url);
				if (!string.IsNullOrEmpty(text) && GMaps.Instance.UseRouteCache)
				{
					Cache.Instance.SaveContent(url, CacheType.RouteCache, text);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				int num = text.IndexOf("<RoutePath><Line>") + 17;
				if (num >= 17)
				{
					int num2 = text.IndexOf("</Line></RoutePath>", num + 1);
					if (num2 > 0)
					{
						int num3 = num2 - num;
						if (num3 > 0)
						{
							tooltipHtml = text.Substring(num, num3);
						}
					}
				}
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(text);
				XmlNode xmlNode = xmlDocument["Response"];
				switch (xmlNode["StatusCode"].InnerText)
				{
				case "200":
				{
					xmlNode = xmlNode["ResourceSets"]["ResourceSet"]["Resources"]["Route"]["RoutePath"]["Line"];
					XmlNodeList childNodes = xmlNode.ChildNodes;
					if (childNodes.Count <= 0)
					{
						break;
					}
					list = new List<PointLatLng>();
					foreach (XmlNode item in childNodes)
					{
						XmlNode xmlNode2 = item["Latitude"];
						XmlNode xmlNode3 = item["Longitude"];
						list.Add(new PointLatLng(double.Parse(xmlNode2.InnerText, CultureInfo.InvariantCulture), double.Parse(xmlNode3.InnerText, CultureInfo.InvariantCulture)));
					}
					break;
				}
				case "400":
					throw new Exception("Bad Request, The request contained an error.");
				case "401":
					throw new Exception("Unauthorized, Access was denied. You may have entered your credentials incorrectly, or you might not have access to the requested resource or operation.");
				case "403":
					throw new Exception("Forbidden, The request is for something forbidden. Authorization will not help.");
				case "404":
					throw new Exception("Not Found, The requested resource was not found.");
				case "500":
					throw new Exception("Internal Server Error, Your request could not be completed because there was a problem with the service.");
				case "501":
					throw new Exception("Service Unavailable, There's a problem with the service right now. Please try again later.");
				}
			}
		}
		catch (Exception)
		{
			list = null;
		}
		return list;
	}

	public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
	{
		return GetLatLngFromGeocoderUrl(MakeGeocoderUrl("q=" + Uri.EscapeDataString(keywords)), out pointList);
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
		return GetLatLngFromGeocoderUrl(MakeGeocoderDetailedUrl(placemark), out pointList);
	}

	public PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
	{
		status = GetLatLngFromGeocoderUrl(MakeGeocoderDetailedUrl(placemark), out var pointList);
		if (pointList == null || pointList.Count <= 0)
		{
			return null;
		}
		return pointList[0];
	}

	private string MakeGeocoderDetailedUrl(Placemark placemark)
	{
		string input = string.Empty;
		if (!AddFieldIfNotEmpty(ref input, "countryRegion", placemark.CountryNameCode))
		{
			AddFieldIfNotEmpty(ref input, "countryRegion", placemark.CountryName);
		}
		AddFieldIfNotEmpty(ref input, "adminDistrict", placemark.DistrictName);
		AddFieldIfNotEmpty(ref input, "locality", placemark.LocalityName);
		AddFieldIfNotEmpty(ref input, "postalCode", placemark.PostalCodeNumber);
		if (!string.IsNullOrEmpty(placemark.HouseNo))
		{
			AddFieldIfNotEmpty(ref input, "addressLine", placemark.ThoroughfareName + " " + placemark.HouseNo);
		}
		else
		{
			AddFieldIfNotEmpty(ref input, "addressLine", placemark.ThoroughfareName);
		}
		return MakeGeocoderUrl(input);
	}

	private bool AddFieldIfNotEmpty(ref string input, string fieldName, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			if (string.IsNullOrEmpty(input))
			{
				input = string.Empty;
			}
			else
			{
				input += "&";
			}
			input = input + fieldName + "=" + value;
			return true;
		}
		return false;
	}

	public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
	{
		throw new NotImplementedException();
	}

	public Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
	{
		throw new NotImplementedException();
	}

	private string MakeGeocoderUrl(string keywords)
	{
		return string.Format(CultureInfo.InvariantCulture, GeocoderUrlFormat, keywords, ClientKey);
	}

	private GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
	{
		pointList = null;
		GeoCoderStatusCode result;
		try
		{
			string text = (GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(url, CacheType.GeocoderCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			bool flag = false;
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp(url);
				if (!string.IsNullOrEmpty(text))
				{
					flag = true;
				}
			}
			result = GeoCoderStatusCode.UNKNOWN_ERROR;
			if (!string.IsNullOrEmpty(text) && text.StartsWith("<?xml") && text.Contains("<Response"))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(text);
				XmlNode xmlNode = xmlDocument["Response"];
				switch (xmlNode["StatusCode"].InnerText)
				{
				case "200":
					pointList = new List<PointLatLng>();
					xmlNode = xmlNode["ResourceSets"]["ResourceSet"]["Resources"];
					foreach (XmlNode childNode in xmlNode.ChildNodes)
					{
						XmlNode xmlNode2 = childNode["Point"]["Latitude"];
						XmlNode xmlNode3 = childNode["Point"]["Longitude"];
						pointList.Add(new PointLatLng(double.Parse(xmlNode2.InnerText, CultureInfo.InvariantCulture), double.Parse(xmlNode3.InnerText, CultureInfo.InvariantCulture)));
					}
					if (pointList.Count > 0)
					{
						result = GeoCoderStatusCode.OK;
						if (flag && GMaps.Instance.UseGeocoderCache)
						{
							Cache.Instance.SaveContent(url, CacheType.GeocoderCache, text);
						}
					}
					else
					{
						result = GeoCoderStatusCode.ZERO_RESULTS;
					}
					break;
				case "400":
					result = GeoCoderStatusCode.INVALID_REQUEST;
					break;
				case "401":
					result = GeoCoderStatusCode.REQUEST_DENIED;
					break;
				case "403":
					result = GeoCoderStatusCode.INVALID_REQUEST;
					break;
				case "404":
					result = GeoCoderStatusCode.ZERO_RESULTS;
					break;
				case "500":
					result = GeoCoderStatusCode.ERROR;
					break;
				case "501":
					result = GeoCoderStatusCode.UNKNOWN_ERROR;
					break;
				default:
					result = GeoCoderStatusCode.UNKNOWN_ERROR;
					break;
				}
			}
		}
		catch (Exception)
		{
			result = GeoCoderStatusCode.EXCEPTION_IN_CODE;
		}
		return result;
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using GMap.NET.Internals;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class CloudMadeMapProviderBase : GMapProvider, RoutingProvider, DirectionsProvider
{
	public readonly string ServerLetters = "abc";

	public readonly string DoubleResolutionString = "@2x";

	public bool DoubleResolution = true;

	public string Key;

	public int StyleID;

	public string Version = "0.3";

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat = "http://routes.cloudmade.com/{0}/api/{1}/{2},{3},{4},{5}/{6}.gpx?lang={7}&units={8}";

	private static readonly string TravelTypeFoot = "foot";

	private static readonly string TravelTypeMotorCar = "car";

	private static readonly string WalkingStr = "Walking";

	private static readonly string DrivingStr = "Driving";

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

	public CloudMadeMapProviderBase()
	{
		MaxZoom = null;
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}

	public MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int zoom)
	{
		List<PointLatLng> routePoints = GetRoutePoints(MakeRoutingUrl(start, end, walkingMode ? TravelTypeFoot : TravelTypeMotorCar, GMapProvider.LanguageStr, "km"));
		if (routePoints == null)
		{
			return null;
		}
		return new MapRoute(routePoints, walkingMode ? WalkingStr : DrivingStr);
	}

	public MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom)
	{
		throw new NotImplementedException();
	}

	private string MakeRoutingUrl(PointLatLng start, PointLatLng end, string travelType, string language, string units)
	{
		return string.Format(CultureInfo.InvariantCulture, UrlFormat, Key, Version, start.Lat, start.Lng, end.Lat, end.Lng, travelType, language, units);
	}

	private List<PointLatLng> GetRoutePoints(string url)
	{
		List<PointLatLng> list = null;
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
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(text);
				XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManager.AddNamespace("sm", "http://www.topografix.com/GPX/1/1");
				XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/sm:gpx/sm:wpt", xmlNamespaceManager);
				if (xmlNodeList != null && xmlNodeList.Count > 0)
				{
					list = new List<PointLatLng>();
					foreach (XmlNode item in xmlNodeList)
					{
						double lat = double.Parse(item.Attributes["lat"].InnerText, CultureInfo.InvariantCulture);
						double lng = double.Parse(item.Attributes["lon"].InnerText, CultureInfo.InvariantCulture);
						list.Add(new PointLatLng(lat, lng));
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return list;
	}

	public DirectionsStatusCode GetDirections(out GDirections direction, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		return GetDirectionsUrl(MakeRoutingUrl(start, end, walkingMode ? TravelTypeFoot : TravelTypeMotorCar, GMapProvider.LanguageStr, metric ? "km" : "miles"), out direction);
	}

	public DirectionsStatusCode GetDirections(out GDirections direction, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public DirectionsStatusCode GetDirections(out GDirections direction, string start, IEnumerable<string> wayPoints, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
	{
		throw new NotImplementedException();
	}

	private DirectionsStatusCode GetDirectionsUrl(string url, out GDirections direction)
	{
		DirectionsStatusCode result = DirectionsStatusCode.UNKNOWN_ERROR;
		direction = null;
		try
		{
			string text = (GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.DirectionsCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp(url);
				if (!string.IsNullOrEmpty(text) && GMaps.Instance.UseRouteCache)
				{
					Cache.Instance.SaveContent(url, CacheType.DirectionsCache, text);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(text);
				XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManager.AddNamespace("sm", "http://www.topografix.com/GPX/1/1");
				XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/sm:gpx/sm:wpt", xmlNamespaceManager);
				if (xmlNodeList != null && xmlNodeList.Count > 0)
				{
					result = DirectionsStatusCode.UNKNOWN_ERROR;
					direction = new GDirections();
					direction.Route = new List<PointLatLng>();
					foreach (XmlNode item2 in xmlNodeList)
					{
						double lat = double.Parse(item2.Attributes["lat"].InnerText, CultureInfo.InvariantCulture);
						double lng = double.Parse(item2.Attributes["lon"].InnerText, CultureInfo.InvariantCulture);
						direction.Route.Add(new PointLatLng(lat, lng));
					}
					if (direction.Route.Count > 0)
					{
						direction.StartLocation = direction.Route[0];
						direction.EndLocation = direction.Route[direction.Route.Count - 1];
					}
					XmlNode xmlNode = xmlDocument.SelectSingleNode("/sm:gpx/sm:metadata/sm:copyright/sm:license", xmlNamespaceManager);
					if (xmlNode != null)
					{
						direction.Copyrights = xmlNode.InnerText;
					}
					xmlNode = xmlDocument.SelectSingleNode("/sm:gpx/sm:extensions/sm:distance", xmlNamespaceManager);
					if (xmlNode != null)
					{
						direction.Distance = xmlNode.InnerText + "m";
					}
					xmlNode = xmlDocument.SelectSingleNode("/sm:gpx/sm:extensions/sm:time", xmlNamespaceManager);
					if (xmlNode != null)
					{
						direction.Duration = xmlNode.InnerText + "s";
					}
					xmlNode = xmlDocument.SelectSingleNode("/sm:gpx/sm:extensions/sm:start", xmlNamespaceManager);
					if (xmlNode != null)
					{
						direction.StartAddress = xmlNode.InnerText;
					}
					xmlNode = xmlDocument.SelectSingleNode("/sm:gpx/sm:extensions/sm:end", xmlNamespaceManager);
					if (xmlNode != null)
					{
						direction.EndAddress = xmlNode.InnerText;
					}
					xmlNodeList = xmlDocument.SelectNodes("/sm:gpx/sm:rte/sm:rtept", xmlNamespaceManager);
					if (xmlNodeList != null && xmlNodeList.Count > 0)
					{
						direction.Steps = new List<GDirectionStep>();
						foreach (XmlNode item3 in xmlNodeList)
						{
							GDirectionStep item = default(GDirectionStep);
							double lat2 = double.Parse(item3.Attributes["lat"].InnerText, CultureInfo.InvariantCulture);
							double lng2 = double.Parse(item3.Attributes["lon"].InnerText, CultureInfo.InvariantCulture);
							item.StartLocation = new PointLatLng(lat2, lng2);
							XmlNode xmlNode2 = item3.SelectSingleNode("sm:desc", xmlNamespaceManager);
							if (xmlNode2 != null)
							{
								item.HtmlInstructions = xmlNode2.InnerText;
							}
							xmlNode2 = item3.SelectSingleNode("sm:extensions/sm:distance-text", xmlNamespaceManager);
							if (xmlNode2 != null)
							{
								item.Distance = xmlNode2.InnerText;
							}
							xmlNode2 = item3.SelectSingleNode("sm:extensions/sm:time", xmlNamespaceManager);
							if (xmlNode2 != null)
							{
								item.Duration = xmlNode2.InnerText + "s";
							}
							direction.Steps.Add(item);
						}
					}
				}
			}
		}
		catch (Exception)
		{
			result = DirectionsStatusCode.EXCEPTION_IN_CODE;
			direction = null;
		}
		return result;
	}
}

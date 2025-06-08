using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using GMap.NET.Internals;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class YahooMapProviderBase : GMapProvider, GeocodingProvider
{
	public string AppId = string.Empty;

	public int MinExpectedQuality = 39;

	private GMapProvider[] _overlays;

	private static readonly string ReverseGeocoderUrlFormat = "http://where.yahooapis.com/geocode?q={0},{1}&appid={2}&flags=G&gflags=QRL{3}";

	private static readonly string GeocoderUrlFormat = "http://where.yahooapis.com/geocode?q={0}&appid={1}&flags=CG&gflags=QL{2}";

	private static readonly string GeocoderDetailedUrlFormat = "http://where.yahooapis.com/geocode?country={0}&state={1}&county={2}&city={3}&neighborhood={4}&postal={5}&street={6}&house={7}&appid={8}&flags=CG&gflags=QL{9}";

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

	public YahooMapProviderBase()
	{
		RefererUrl = "http://maps.yahoo.com/";
		Copyright = $"© Yahoo! Inc. - Map data & Imagery ©{DateTime.Today.Year} NAVTEQ";
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
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
		return GetLatLngFromGeocoderUrl(MakeGeocoderDetailedUrl(placemark), out pointList);
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
		return GetPlacemarksFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location), out placemarkList);
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

	private string MakeGeocoderUrl(string keywords)
	{
		return string.Format(CultureInfo.InvariantCulture, GeocoderUrlFormat, keywords.Replace(' ', '+'), AppId, (!string.IsNullOrEmpty(GMapProvider.LanguageStr)) ? ("&locale=" + GMapProvider.LanguageStr) : "");
	}

	private string MakeGeocoderDetailedUrl(Placemark placemark)
	{
		return string.Format(GeocoderDetailedUrlFormat, PrepareUrlString(placemark.CountryName), PrepareUrlString(placemark.AdministrativeAreaName), PrepareUrlString(placemark.SubAdministrativeAreaName), PrepareUrlString(placemark.LocalityName), PrepareUrlString(placemark.DistrictName), PrepareUrlString(placemark.PostalCodeNumber), PrepareUrlString(placemark.ThoroughfareName), PrepareUrlString(placemark.HouseNo), AppId, (!string.IsNullOrEmpty(GMapProvider.LanguageStr)) ? ("&locale=" + GMapProvider.LanguageStr) : string.Empty);
	}

	private string MakeReverseGeocoderUrl(PointLatLng pt)
	{
		return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, pt.Lat, pt.Lng, AppId, (!string.IsNullOrEmpty(GMapProvider.LanguageStr)) ? ("&locale=" + GMapProvider.LanguageStr) : "");
	}

	private string PrepareUrlString(string str)
	{
		if (str == null)
		{
			return string.Empty;
		}
		return str.Replace(' ', '+');
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
				text = GetContentUsingHttp(url);
				if (!string.IsNullOrEmpty(text))
				{
					flag = true;
				}
			}
			if (!string.IsNullOrEmpty(text) && text.StartsWith("<?xml") && text.Contains("<Result"))
			{
				if (flag && GMaps.Instance.UseGeocoderCache)
				{
					Cache.Instance.SaveContent(url, CacheType.GeocoderCache, text);
				}
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(text);
				XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/ResultSet/Result");
				if (xmlNodeList != null)
				{
					pointList = new List<PointLatLng>();
					foreach (XmlNode item in xmlNodeList)
					{
						XmlNode xmlNode2 = item.SelectSingleNode("quality");
						if (xmlNode2 == null || int.Parse(xmlNode2.InnerText) < MinExpectedQuality)
						{
							continue;
						}
						xmlNode2 = item.SelectSingleNode("latitude");
						if (xmlNode2 != null)
						{
							double lat = double.Parse(xmlNode2.InnerText, CultureInfo.InvariantCulture);
							xmlNode2 = item.SelectSingleNode("longitude");
							if (xmlNode2 != null)
							{
								double lng = double.Parse(xmlNode2.InnerText, CultureInfo.InvariantCulture);
								pointList.Add(new PointLatLng(lat, lng));
							}
						}
					}
					result = GeoCoderStatusCode.OK;
				}
			}
		}
		catch (Exception)
		{
			result = GeoCoderStatusCode.EXCEPTION_IN_CODE;
		}
		return result;
	}

	private GeoCoderStatusCode GetPlacemarksFromReverseGeocoderUrl(string url, out List<Placemark> placemarkList)
	{
		GeoCoderStatusCode result = GeoCoderStatusCode.UNKNOWN_ERROR;
		placemarkList = null;
		try
		{
			string text = (GMaps.Instance.UsePlacemarkCache ? Cache.Instance.GetContent(url, CacheType.PlacemarkCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty);
			bool flag = false;
			if (string.IsNullOrEmpty(text))
			{
				text = GetContentUsingHttp(url);
				if (!string.IsNullOrEmpty(text))
				{
					flag = true;
				}
			}
			if (!string.IsNullOrEmpty(text) && text.StartsWith("<?xml") && text.Contains("<ResultSet"))
			{
				if (flag && GMaps.Instance.UsePlacemarkCache)
				{
					Cache.Instance.SaveContent(url, CacheType.PlacemarkCache, text);
				}
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(text);
				XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/ResultSet/Result");
				if (xmlNodeList != null)
				{
					placemarkList = new List<Placemark>();
					foreach (XmlNode item2 in xmlNodeList)
					{
						XmlNode xmlNode2 = item2.SelectSingleNode("name");
						if (xmlNode2 != null)
						{
							Placemark item = new Placemark(xmlNode2.InnerText);
							xmlNode2 = item2.SelectSingleNode("level0");
							if (xmlNode2 != null)
							{
								item.CountryName = xmlNode2.InnerText;
							}
							xmlNode2 = item2.SelectSingleNode("level0code");
							if (xmlNode2 != null)
							{
								item.CountryNameCode = xmlNode2.InnerText;
							}
							xmlNode2 = item2.SelectSingleNode("postal");
							if (xmlNode2 != null)
							{
								item.PostalCodeNumber = xmlNode2.InnerText;
							}
							xmlNode2 = item2.SelectSingleNode("level1");
							if (xmlNode2 != null)
							{
								item.AdministrativeAreaName = xmlNode2.InnerText;
							}
							xmlNode2 = item2.SelectSingleNode("level2");
							if (xmlNode2 != null)
							{
								item.SubAdministrativeAreaName = xmlNode2.InnerText;
							}
							xmlNode2 = item2.SelectSingleNode("level3");
							if (xmlNode2 != null)
							{
								item.LocalityName = xmlNode2.InnerText;
							}
							xmlNode2 = item2.SelectSingleNode("level4");
							if (xmlNode2 != null)
							{
								item.DistrictName = xmlNode2.InnerText;
							}
							xmlNode2 = item2.SelectSingleNode("street");
							if (xmlNode2 != null)
							{
								item.ThoroughfareName = xmlNode2.InnerText;
							}
							xmlNode2 = item2.SelectSingleNode("house");
							if (xmlNode2 != null)
							{
								item.HouseNo = xmlNode2.InnerText;
							}
							xmlNode2 = item2.SelectSingleNode("radius");
							if (xmlNode2 != null)
							{
								item.Accuracy = int.Parse(xmlNode2.InnerText);
							}
							placemarkList.Add(item);
						}
					}
					result = GeoCoderStatusCode.OK;
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

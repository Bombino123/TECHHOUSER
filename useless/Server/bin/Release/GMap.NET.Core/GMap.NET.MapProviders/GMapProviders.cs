using System;
using System.Collections.Generic;
using System.Reflection;

namespace GMap.NET.MapProviders;

public class GMapProviders
{
	public static readonly EmptyProvider EmptyProvider;

	public static readonly OpenCycleMapProvider OpenCycleMap;

	public static readonly OpenCycleLandscapeMapProvider OpenCycleLandscapeMap;

	public static readonly OpenCycleTransportMapProvider OpenCycleTransportMap;

	public static readonly OpenStreetMapProvider OpenStreetMap;

	public static readonly OpenStreetMapGraphHopperProvider OpenStreetMapGraphHopper;

	public static readonly OpenStreet4UMapProvider OpenStreet4UMap;

	public static readonly OpenStreetMapQuestProvider OpenStreetMapQuest;

	public static readonly OpenStreetMapQuestSatelliteProvider OpenStreetMapQuestSatellite;

	public static readonly OpenStreetMapQuestHybridProvider OpenStreetMapQuestHybrid;

	public static readonly OpenSeaMapHybridProvider OpenSeaMapHybrid;

	public static readonly WikiMapiaMapProvider WikiMapiaMap;

	public static readonly BingMapProvider BingMap;

	public static readonly BingSatelliteMapProvider BingSatelliteMap;

	public static readonly BingHybridMapProvider BingHybridMap;

	public static readonly BingOSMapProvider BingOSMap;

	public static readonly YahooMapProvider YahooMap;

	public static readonly YahooSatelliteMapProvider YahooSatelliteMap;

	public static readonly YahooHybridMapProvider YahooHybridMap;

	public static readonly GoogleMapProvider GoogleMap;

	public static readonly GoogleSatelliteMapProvider GoogleSatelliteMap;

	public static readonly GoogleHybridMapProvider GoogleHybridMap;

	public static readonly GoogleTerrainMapProvider GoogleTerrainMap;

	public static readonly GoogleChinaMapProvider GoogleChinaMap;

	public static readonly GoogleChinaSatelliteMapProvider GoogleChinaSatelliteMap;

	public static readonly GoogleChinaHybridMapProvider GoogleChinaHybridMap;

	public static readonly GoogleChinaTerrainMapProvider GoogleChinaTerrainMap;

	public static readonly GoogleKoreaMapProvider GoogleKoreaMap;

	public static readonly GoogleKoreaSatelliteMapProvider GoogleKoreaSatelliteMap;

	public static readonly GoogleKoreaHybridMapProvider GoogleKoreaHybridMap;

	public static readonly NearMapProvider NearMap;

	public static readonly NearSatelliteMapProvider NearSatelliteMap;

	public static readonly NearHybridMapProvider NearHybridMap;

	public static readonly HereMapProvider HereMap;

	public static readonly HereSatelliteMapProvider HereSatelliteMap;

	public static readonly HereHybridMapProvider HereHybridMap;

	public static readonly HereTerrainMapProvider HereTerrainMap;

	public static readonly YandexMapProvider YandexMap;

	public static readonly YandexSatelliteMapProvider YandexSatelliteMap;

	public static readonly YandexHybridMapProvider YandexHybridMap;

	public static readonly LithuaniaMapProvider LithuaniaMap;

	public static readonly LithuaniaReliefMapProvider LithuaniaReliefMap;

	public static readonly Lithuania3dMapProvider Lithuania3dMap;

	public static readonly LithuaniaOrtoFotoMapProvider LithuaniaOrtoFotoMap;

	public static readonly LithuaniaOrtoFotoOldMapProvider LithuaniaOrtoFotoOldMap;

	public static readonly LithuaniaHybridMapProvider LithuaniaHybridMap;

	public static readonly LithuaniaHybridOldMapProvider LithuaniaHybridOldMap;

	public static readonly LithuaniaTOP50 LithuaniaTOP50Map;

	public static readonly LatviaMapProvider LatviaMap;

	public static readonly MapBenderWMSProvider MapBenderWMSdemoMap;

	public static readonly TurkeyMapProvider TurkeyMap;

	public static readonly CloudMadeMapProvider CloudMadeMap;

	public static readonly SpainMapProvider SpainMap;

	public static readonly CzechMapProviderOld CzechOldMap;

	public static readonly CzechSatelliteMapProviderOld CzechSatelliteOldMap;

	public static readonly CzechHybridMapProviderOld CzechHybridOldMap;

	public static readonly CzechTuristMapProviderOld CzechTuristOldMap;

	public static readonly CzechHistoryMapProviderOld CzechHistoryOldMap;

	public static readonly CzechMapProvider CzechMap;

	public static readonly CzechSatelliteMapProvider CzechSatelliteMap;

	public static readonly CzechHybridMapProvider CzechHybridMap;

	public static readonly CzechTuristMapProvider CzechTuristMap;

	public static readonly CzechTuristWinterMapProvider CzechTuristWinterMap;

	public static readonly CzechHistoryMapProvider CzechHistoryMap;

	public static readonly CzechGeographicMapProvider CzechGeographicMap;

	public static readonly ArcGIS_Imagery_World_2D_MapProvider ArcGIS_Imagery_World_2D_Map;

	public static readonly ArcGIS_ShadedRelief_World_2D_MapProvider ArcGIS_ShadedRelief_World_2D_Map;

	public static readonly ArcGIS_StreetMap_World_2D_MapProvider ArcGIS_StreetMap_World_2D_Map;

	public static readonly ArcGIS_Topo_US_2D_MapProvider ArcGIS_Topo_US_2D_Map;

	public static readonly ArcGIS_World_Physical_MapProvider ArcGIS_World_Physical_Map;

	public static readonly ArcGIS_World_Shaded_Relief_MapProvider ArcGIS_World_Shaded_Relief_Map;

	public static readonly ArcGIS_World_Street_MapProvider ArcGIS_World_Street_Map;

	public static readonly ArcGIS_World_Terrain_Base_MapProvider ArcGIS_World_Terrain_Base_Map;

	public static readonly ArcGIS_World_Topo_MapProvider ArcGIS_World_Topo_Map;

	public static readonly ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_Map;

	public static readonly SwissTopoProvider SwissMap;

	public static readonly SwedenMapProvider SwedenMap;

	public static readonly SwedenMapProviderAlt SwedenMapAlternative;

	public static readonly UMPMapProvider UMPMap;

	public static readonly CustomMapProvider CustomMap;

	private static Dictionary<Guid, GMapProvider> Hash;

	private static Dictionary<int, GMapProvider> DbHash;

	public static List<GMapProvider> List { get; }

	static GMapProviders()
	{
		EmptyProvider = EmptyProvider.Instance;
		OpenCycleMap = OpenCycleMapProvider.Instance;
		OpenCycleLandscapeMap = OpenCycleLandscapeMapProvider.Instance;
		OpenCycleTransportMap = OpenCycleTransportMapProvider.Instance;
		OpenStreetMap = OpenStreetMapProvider.Instance;
		OpenStreetMapGraphHopper = OpenStreetMapGraphHopperProvider.Instance;
		OpenStreet4UMap = OpenStreet4UMapProvider.Instance;
		OpenStreetMapQuest = OpenStreetMapQuestProvider.Instance;
		OpenStreetMapQuestSatellite = OpenStreetMapQuestSatelliteProvider.Instance;
		OpenStreetMapQuestHybrid = OpenStreetMapQuestHybridProvider.Instance;
		OpenSeaMapHybrid = OpenSeaMapHybridProvider.Instance;
		WikiMapiaMap = WikiMapiaMapProvider.Instance;
		BingMap = BingMapProvider.Instance;
		BingSatelliteMap = BingSatelliteMapProvider.Instance;
		BingHybridMap = BingHybridMapProvider.Instance;
		BingOSMap = BingOSMapProvider.Instance;
		YahooMap = YahooMapProvider.Instance;
		YahooSatelliteMap = YahooSatelliteMapProvider.Instance;
		YahooHybridMap = YahooHybridMapProvider.Instance;
		GoogleMap = GoogleMapProvider.Instance;
		GoogleSatelliteMap = GoogleSatelliteMapProvider.Instance;
		GoogleHybridMap = GoogleHybridMapProvider.Instance;
		GoogleTerrainMap = GoogleTerrainMapProvider.Instance;
		GoogleChinaMap = GoogleChinaMapProvider.Instance;
		GoogleChinaSatelliteMap = GoogleChinaSatelliteMapProvider.Instance;
		GoogleChinaHybridMap = GoogleChinaHybridMapProvider.Instance;
		GoogleChinaTerrainMap = GoogleChinaTerrainMapProvider.Instance;
		GoogleKoreaMap = GoogleKoreaMapProvider.Instance;
		GoogleKoreaSatelliteMap = GoogleKoreaSatelliteMapProvider.Instance;
		GoogleKoreaHybridMap = GoogleKoreaHybridMapProvider.Instance;
		NearMap = NearMapProvider.Instance;
		NearSatelliteMap = NearSatelliteMapProvider.Instance;
		NearHybridMap = NearHybridMapProvider.Instance;
		HereMap = HereMapProvider.Instance;
		HereSatelliteMap = HereSatelliteMapProvider.Instance;
		HereHybridMap = HereHybridMapProvider.Instance;
		HereTerrainMap = HereTerrainMapProvider.Instance;
		YandexMap = YandexMapProvider.Instance;
		YandexSatelliteMap = YandexSatelliteMapProvider.Instance;
		YandexHybridMap = YandexHybridMapProvider.Instance;
		LithuaniaMap = LithuaniaMapProvider.Instance;
		LithuaniaReliefMap = LithuaniaReliefMapProvider.Instance;
		Lithuania3dMap = Lithuania3dMapProvider.Instance;
		LithuaniaOrtoFotoMap = LithuaniaOrtoFotoMapProvider.Instance;
		LithuaniaOrtoFotoOldMap = LithuaniaOrtoFotoOldMapProvider.Instance;
		LithuaniaHybridMap = LithuaniaHybridMapProvider.Instance;
		LithuaniaHybridOldMap = LithuaniaHybridOldMapProvider.Instance;
		LithuaniaTOP50Map = LithuaniaTOP50.Instance;
		LatviaMap = LatviaMapProvider.Instance;
		MapBenderWMSdemoMap = MapBenderWMSProvider.Instance;
		TurkeyMap = TurkeyMapProvider.Instance;
		CloudMadeMap = CloudMadeMapProvider.Instance;
		SpainMap = SpainMapProvider.Instance;
		CzechOldMap = CzechMapProviderOld.Instance;
		CzechSatelliteOldMap = CzechSatelliteMapProviderOld.Instance;
		CzechHybridOldMap = CzechHybridMapProviderOld.Instance;
		CzechTuristOldMap = CzechTuristMapProviderOld.Instance;
		CzechHistoryOldMap = CzechHistoryMapProviderOld.Instance;
		CzechMap = CzechMapProvider.Instance;
		CzechSatelliteMap = CzechSatelliteMapProvider.Instance;
		CzechHybridMap = CzechHybridMapProvider.Instance;
		CzechTuristMap = CzechTuristMapProvider.Instance;
		CzechTuristWinterMap = CzechTuristWinterMapProvider.Instance;
		CzechHistoryMap = CzechHistoryMapProvider.Instance;
		CzechGeographicMap = CzechGeographicMapProvider.Instance;
		ArcGIS_Imagery_World_2D_Map = ArcGIS_Imagery_World_2D_MapProvider.Instance;
		ArcGIS_ShadedRelief_World_2D_Map = ArcGIS_ShadedRelief_World_2D_MapProvider.Instance;
		ArcGIS_StreetMap_World_2D_Map = ArcGIS_StreetMap_World_2D_MapProvider.Instance;
		ArcGIS_Topo_US_2D_Map = ArcGIS_Topo_US_2D_MapProvider.Instance;
		ArcGIS_World_Physical_Map = ArcGIS_World_Physical_MapProvider.Instance;
		ArcGIS_World_Shaded_Relief_Map = ArcGIS_World_Shaded_Relief_MapProvider.Instance;
		ArcGIS_World_Street_Map = ArcGIS_World_Street_MapProvider.Instance;
		ArcGIS_World_Terrain_Base_Map = ArcGIS_World_Terrain_Base_MapProvider.Instance;
		ArcGIS_World_Topo_Map = ArcGIS_World_Topo_MapProvider.Instance;
		ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_Map = ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider.Instance;
		SwissMap = SwissTopoProvider.Instance;
		SwedenMap = SwedenMapProvider.Instance;
		SwedenMapAlternative = SwedenMapProviderAlt.Instance;
		UMPMap = UMPMapProvider.Instance;
		CustomMap = CustomMapProvider.Instance;
		List = new List<GMapProvider>();
		FieldInfo[] fields = typeof(GMapProviders).GetFields();
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].GetValue(null) is GMapProvider item)
			{
				List.Add(item);
			}
		}
		Hash = new Dictionary<Guid, GMapProvider>();
		foreach (GMapProvider item2 in List)
		{
			Hash.Add(item2.Id, item2);
		}
		DbHash = new Dictionary<int, GMapProvider>();
		foreach (GMapProvider item3 in List)
		{
			DbHash.Add(item3.DbId, item3);
		}
	}

	private GMapProviders()
	{
	}

	public static GMapProvider TryGetProvider(Guid id)
	{
		if (Hash.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public static GMapProvider TryGetProvider(int dbId)
	{
		if (DbHash.TryGetValue(dbId, out var value))
		{
			return value;
		}
		return null;
	}

	public static GMapProvider TryGetProvider(string providerName)
	{
		if (List.Exists((GMapProvider x) => x.Name == providerName))
		{
			return List.Find((GMapProvider x) => x.Name == providerName);
		}
		return null;
	}
}

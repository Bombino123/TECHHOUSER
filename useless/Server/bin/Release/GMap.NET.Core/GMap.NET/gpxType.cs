using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace GMap.NET;

[Serializable]
[GeneratedCode("xsd", "4.0.30319.1")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
[XmlRoot("gpx", Namespace = "http://www.topografix.com/GPX/1/1", IsNullable = false)]
public class gpxType
{
	private metadataType metadataField;

	private wptType[] wptField;

	private rteType[] rteField;

	private trkType[] trkField;

	private extensionsType extensionsField;

	private string versionField;

	private string creatorField;

	public metadataType metadata
	{
		get
		{
			return metadataField;
		}
		set
		{
			metadataField = value;
		}
	}

	[XmlElement("wpt")]
	public wptType[] wpt
	{
		get
		{
			return wptField;
		}
		set
		{
			wptField = value;
		}
	}

	[XmlElement("rte")]
	public rteType[] rte
	{
		get
		{
			return rteField;
		}
		set
		{
			rteField = value;
		}
	}

	[XmlElement("trk")]
	public trkType[] trk
	{
		get
		{
			return trkField;
		}
		set
		{
			trkField = value;
		}
	}

	public extensionsType extensions
	{
		get
		{
			return extensionsField;
		}
		set
		{
			extensionsField = value;
		}
	}

	[XmlAttribute]
	public string version
	{
		get
		{
			return versionField;
		}
		set
		{
			versionField = value;
		}
	}

	[XmlAttribute]
	public string creator
	{
		get
		{
			return creatorField;
		}
		set
		{
			creatorField = value;
		}
	}

	public gpxType()
	{
		versionField = "1.1";
	}
}

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
public class wptType
{
	private decimal eleField;

	private bool eleFieldSpecified;

	private DateTime timeField;

	private bool timeFieldSpecified;

	private decimal magvarField;

	private bool magvarFieldSpecified;

	private decimal geoidheightField;

	private bool geoidheightFieldSpecified;

	private string nameField;

	private string cmtField;

	private string descField;

	private string srcField;

	private linkType[] linkField;

	private string symField;

	private string typeField;

	private fixType fixField;

	private bool fixFieldSpecified;

	private string satField;

	private decimal hdopField;

	private bool hdopFieldSpecified;

	private decimal vdopField;

	private bool vdopFieldSpecified;

	private decimal pdopField;

	private bool pdopFieldSpecified;

	private decimal ageofdgpsdataField;

	private bool ageofdgpsdataFieldSpecified;

	private string dgpsidField;

	private extensionsType extensionsField;

	private decimal latField;

	private decimal lonField;

	public decimal ele
	{
		get
		{
			return eleField;
		}
		set
		{
			eleField = value;
		}
	}

	[XmlIgnore]
	public bool eleSpecified
	{
		get
		{
			return eleFieldSpecified;
		}
		set
		{
			eleFieldSpecified = value;
		}
	}

	public DateTime time
	{
		get
		{
			return timeField;
		}
		set
		{
			timeField = value;
		}
	}

	[XmlIgnore]
	public bool timeSpecified
	{
		get
		{
			return timeFieldSpecified;
		}
		set
		{
			timeFieldSpecified = value;
		}
	}

	public decimal magvar
	{
		get
		{
			return magvarField;
		}
		set
		{
			magvarField = value;
		}
	}

	[XmlIgnore]
	public bool magvarSpecified
	{
		get
		{
			return magvarFieldSpecified;
		}
		set
		{
			magvarFieldSpecified = value;
		}
	}

	public decimal geoidheight
	{
		get
		{
			return geoidheightField;
		}
		set
		{
			geoidheightField = value;
		}
	}

	[XmlIgnore]
	public bool geoidheightSpecified
	{
		get
		{
			return geoidheightFieldSpecified;
		}
		set
		{
			geoidheightFieldSpecified = value;
		}
	}

	public string name
	{
		get
		{
			return nameField;
		}
		set
		{
			nameField = value;
		}
	}

	public string cmt
	{
		get
		{
			return cmtField;
		}
		set
		{
			cmtField = value;
		}
	}

	public string desc
	{
		get
		{
			return descField;
		}
		set
		{
			descField = value;
		}
	}

	public string src
	{
		get
		{
			return srcField;
		}
		set
		{
			srcField = value;
		}
	}

	[XmlElement("link")]
	public linkType[] link
	{
		get
		{
			return linkField;
		}
		set
		{
			linkField = value;
		}
	}

	public string sym
	{
		get
		{
			return symField;
		}
		set
		{
			symField = value;
		}
	}

	public string type
	{
		get
		{
			return typeField;
		}
		set
		{
			typeField = value;
		}
	}

	public fixType fix
	{
		get
		{
			return fixField;
		}
		set
		{
			fixField = value;
		}
	}

	[XmlIgnore]
	public bool fixSpecified
	{
		get
		{
			return fixFieldSpecified;
		}
		set
		{
			fixFieldSpecified = value;
		}
	}

	[XmlElement(DataType = "nonNegativeInteger")]
	public string sat
	{
		get
		{
			return satField;
		}
		set
		{
			satField = value;
		}
	}

	public decimal hdop
	{
		get
		{
			return hdopField;
		}
		set
		{
			hdopField = value;
		}
	}

	[XmlIgnore]
	public bool hdopSpecified
	{
		get
		{
			return hdopFieldSpecified;
		}
		set
		{
			hdopFieldSpecified = value;
		}
	}

	public decimal vdop
	{
		get
		{
			return vdopField;
		}
		set
		{
			vdopField = value;
		}
	}

	[XmlIgnore]
	public bool vdopSpecified
	{
		get
		{
			return vdopFieldSpecified;
		}
		set
		{
			vdopFieldSpecified = value;
		}
	}

	public decimal pdop
	{
		get
		{
			return pdopField;
		}
		set
		{
			pdopField = value;
		}
	}

	[XmlIgnore]
	public bool pdopSpecified
	{
		get
		{
			return pdopFieldSpecified;
		}
		set
		{
			pdopFieldSpecified = value;
		}
	}

	public decimal ageofdgpsdata
	{
		get
		{
			return ageofdgpsdataField;
		}
		set
		{
			ageofdgpsdataField = value;
		}
	}

	[XmlIgnore]
	public bool ageofdgpsdataSpecified
	{
		get
		{
			return ageofdgpsdataFieldSpecified;
		}
		set
		{
			ageofdgpsdataFieldSpecified = value;
		}
	}

	[XmlElement(DataType = "integer")]
	public string dgpsid
	{
		get
		{
			return dgpsidField;
		}
		set
		{
			dgpsidField = value;
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
	public decimal lat
	{
		get
		{
			return latField;
		}
		set
		{
			latField = value;
		}
	}

	[XmlAttribute]
	public decimal lon
	{
		get
		{
			return lonField;
		}
		set
		{
			lonField = value;
		}
	}
}

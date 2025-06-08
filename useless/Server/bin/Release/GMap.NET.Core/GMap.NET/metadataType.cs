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
public class metadataType
{
	private string nameField;

	private string descField;

	private personType authorField;

	private copyrightType copyrightField;

	private linkType[] linkField;

	private DateTime timeField;

	private bool timeFieldSpecified;

	private string keywordsField;

	private boundsType boundsField;

	private extensionsType extensionsField;

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

	public personType author
	{
		get
		{
			return authorField;
		}
		set
		{
			authorField = value;
		}
	}

	public copyrightType copyright
	{
		get
		{
			return copyrightField;
		}
		set
		{
			copyrightField = value;
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

	public string keywords
	{
		get
		{
			return keywordsField;
		}
		set
		{
			keywordsField = value;
		}
	}

	public boundsType bounds
	{
		get
		{
			return boundsField;
		}
		set
		{
			boundsField = value;
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
}

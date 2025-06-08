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
public class rteType
{
	private string nameField;

	private string cmtField;

	private string descField;

	private string srcField;

	private linkType[] linkField;

	private string numberField;

	private string typeField;

	private extensionsType extensionsField;

	private wptType[] rteptField;

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

	[XmlElement(DataType = "nonNegativeInteger")]
	public string number
	{
		get
		{
			return numberField;
		}
		set
		{
			numberField = value;
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

	[XmlElement("rtept")]
	public wptType[] rtept
	{
		get
		{
			return rteptField;
		}
		set
		{
			rteptField = value;
		}
	}
}

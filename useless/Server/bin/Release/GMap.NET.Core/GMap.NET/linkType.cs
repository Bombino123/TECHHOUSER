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
public class linkType
{
	private string textField;

	private string typeField;

	private string hrefField;

	public string text
	{
		get
		{
			return textField;
		}
		set
		{
			textField = value;
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

	[XmlAttribute(DataType = "anyURI")]
	public string href
	{
		get
		{
			return hrefField;
		}
		set
		{
			hrefField = value;
		}
	}
}

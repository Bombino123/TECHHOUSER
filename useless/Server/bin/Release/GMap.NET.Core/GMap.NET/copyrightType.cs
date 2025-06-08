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
public class copyrightType
{
	private string yearField;

	private string licenseField;

	private string authorField;

	[XmlElement(DataType = "gYear")]
	public string year
	{
		get
		{
			return yearField;
		}
		set
		{
			yearField = value;
		}
	}

	[XmlElement(DataType = "anyURI")]
	public string license
	{
		get
		{
			return licenseField;
		}
		set
		{
			licenseField = value;
		}
	}

	[XmlAttribute]
	public string author
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
}

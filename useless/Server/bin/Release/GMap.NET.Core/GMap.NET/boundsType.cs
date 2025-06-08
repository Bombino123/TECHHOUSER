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
public class boundsType
{
	private decimal minlatField;

	private decimal minlonField;

	private decimal maxlatField;

	private decimal maxlonField;

	[XmlAttribute]
	public decimal minlat
	{
		get
		{
			return minlatField;
		}
		set
		{
			minlatField = value;
		}
	}

	[XmlAttribute]
	public decimal minlon
	{
		get
		{
			return minlonField;
		}
		set
		{
			minlonField = value;
		}
	}

	[XmlAttribute]
	public decimal maxlat
	{
		get
		{
			return maxlatField;
		}
		set
		{
			maxlatField = value;
		}
	}

	[XmlAttribute]
	public decimal maxlon
	{
		get
		{
			return maxlonField;
		}
		set
		{
			maxlonField = value;
		}
	}
}

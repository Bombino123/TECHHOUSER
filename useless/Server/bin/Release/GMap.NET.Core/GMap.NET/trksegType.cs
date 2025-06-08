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
public class trksegType
{
	private wptType[] trkptField;

	private extensionsType extensionsField;

	[XmlElement("trkpt")]
	public wptType[] trkpt
	{
		get
		{
			return trkptField;
		}
		set
		{
			trkptField = value;
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

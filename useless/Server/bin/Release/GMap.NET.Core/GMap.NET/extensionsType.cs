using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

namespace GMap.NET;

[Serializable]
[GeneratedCode("xsd", "4.0.30319.1")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
public class extensionsType
{
	private XmlElement[] anyField;

	[XmlAnyElement]
	public XmlElement[] Any
	{
		get
		{
			return anyField;
		}
		set
		{
			anyField = value;
		}
	}
}

using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace GMap.NET;

public enum FixType
{
	Unknown,
	XyD,
	XyzD
}
[Serializable]
[GeneratedCode("xsd", "4.0.30319.1")]
[XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
public enum fixType
{
	none,
	[XmlEnum("2d")]
	Item2d,
	[XmlEnum("3d")]
	Item3d,
	dgps,
	pps
}

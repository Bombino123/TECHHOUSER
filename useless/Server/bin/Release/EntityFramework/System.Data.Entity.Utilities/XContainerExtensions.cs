using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace System.Data.Entity.Utilities;

internal static class XContainerExtensions
{
	public static XElement GetOrAddElement(this XContainer container, XName name)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		XElement val = container.Element(name);
		if (val == null)
		{
			val = new XElement(name);
			container.Add((object)val);
		}
		return val;
	}

	public static IEnumerable<XElement> Descendants(this XContainer container, IEnumerable<XName> name)
	{
		return name.SelectMany((Func<XName, IEnumerable<XElement>>)container.Descendants);
	}

	public static IEnumerable<XElement> Elements(this XContainer container, IEnumerable<XName> name)
	{
		return name.SelectMany((Func<XName, IEnumerable<XElement>>)container.Elements);
	}

	public static IEnumerable<XElement> Descendants<T>(this IEnumerable<T> source, IEnumerable<XName> name) where T : XContainer
	{
		return name.SelectMany((XName n) => source.SelectMany((T c) => ((XContainer)c).Descendants(n)));
	}
}

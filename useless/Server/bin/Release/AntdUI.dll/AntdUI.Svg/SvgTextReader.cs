using System.IO;
using System.Xml;

namespace AntdUI.Svg;

internal sealed class SvgTextReader : XmlTextReader
{
	public SvgTextReader(Stream stream)
		: base(stream)
	{
		base.EntityHandling = EntityHandling.ExpandEntities;
	}

	public SvgTextReader(TextReader reader)
		: base(reader)
	{
		base.EntityHandling = EntityHandling.ExpandEntities;
	}
}

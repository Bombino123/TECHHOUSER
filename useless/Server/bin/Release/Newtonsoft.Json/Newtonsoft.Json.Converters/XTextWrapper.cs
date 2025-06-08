using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

internal class XTextWrapper : XObjectWrapper
{
	private XText Text => (XText)base.WrappedNode;

	public override string? Value
	{
		get
		{
			return Text.Value;
		}
		set
		{
			Text.Value = value;
		}
	}

	public override IXmlNode? ParentNode
	{
		get
		{
			if (((XObject)Text).Parent == null)
			{
				return null;
			}
			return XContainerWrapper.WrapNode((XObject)(object)((XObject)Text).Parent);
		}
	}

	public XTextWrapper(XText text)
		: base((XObject?)(object)text)
	{
	}
}

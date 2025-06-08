using System.Xml;
using System.Xml.XPath;

namespace System.Data.Entity.Core.Mapping;

internal sealed class LineInfo : IXmlLineInfo
{
	private readonly bool m_hasLineInfo;

	private readonly int m_lineNumber;

	private readonly int m_linePosition;

	internal static readonly LineInfo Empty = new LineInfo();

	public int LineNumber => m_lineNumber;

	public int LinePosition => m_linePosition;

	internal LineInfo(XPathNavigator nav)
		: this((IXmlLineInfo)nav)
	{
	}

	internal LineInfo(IXmlLineInfo lineInfo)
	{
		m_hasLineInfo = lineInfo.HasLineInfo();
		m_lineNumber = lineInfo.LineNumber;
		m_linePosition = lineInfo.LinePosition;
	}

	private LineInfo()
	{
		m_hasLineInfo = false;
		m_lineNumber = 0;
		m_linePosition = 0;
	}

	public bool HasLineInfo()
	{
		return m_hasLineInfo;
	}
}

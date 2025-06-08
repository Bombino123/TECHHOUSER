namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class CellLabel
{
	private readonly int m_startLineNumber;

	private readonly int m_startLinePosition;

	private readonly string m_sourceLocation;

	internal int StartLineNumber => m_startLineNumber;

	internal int StartLinePosition => m_startLinePosition;

	internal string SourceLocation => m_sourceLocation;

	internal CellLabel(CellLabel source)
	{
		m_startLineNumber = source.m_startLineNumber;
		m_startLinePosition = source.m_startLinePosition;
		m_sourceLocation = source.m_sourceLocation;
	}

	internal CellLabel(MappingFragment fragmentInfo)
		: this(fragmentInfo.StartLineNumber, fragmentInfo.StartLinePosition, fragmentInfo.SourceLocation)
	{
	}

	internal CellLabel(int startLineNumber, int startLinePosition, string sourceLocation)
	{
		m_startLineNumber = startLineNumber;
		m_startLinePosition = startLinePosition;
		m_sourceLocation = sourceLocation;
	}
}

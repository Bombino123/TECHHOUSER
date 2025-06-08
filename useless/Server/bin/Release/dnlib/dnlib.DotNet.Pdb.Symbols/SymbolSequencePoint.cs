using System.Diagnostics;
using System.Text;

namespace dnlib.DotNet.Pdb.Symbols;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
public struct SymbolSequencePoint
{
	public int Offset;

	public SymbolDocument Document;

	public int Line;

	public int Column;

	public int EndLine;

	public int EndColumn;

	private readonly string GetDebuggerString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Line == 16707566 && EndLine == 16707566)
		{
			stringBuilder.Append("<hidden>");
		}
		else
		{
			stringBuilder.Append("(");
			stringBuilder.Append(Line);
			stringBuilder.Append(",");
			stringBuilder.Append(Column);
			stringBuilder.Append(")-(");
			stringBuilder.Append(EndLine);
			stringBuilder.Append(",");
			stringBuilder.Append(EndColumn);
			stringBuilder.Append(")");
		}
		stringBuilder.Append(": ");
		stringBuilder.Append(Document.URL);
		return stringBuilder.ToString();
	}
}

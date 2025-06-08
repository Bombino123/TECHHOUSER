using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("({StartLine}, {StartColumn}) - ({EndLine}, {EndColumn}) {Document.Url}")]
[ComVisible(true)]
public sealed class SequencePoint
{
	public PdbDocument Document { get; set; }

	public int StartLine { get; set; }

	public int StartColumn { get; set; }

	public int EndLine { get; set; }

	public int EndColumn { get; set; }

	public SequencePoint Clone()
	{
		return new SequencePoint
		{
			Document = Document,
			StartLine = StartLine,
			StartColumn = StartColumn,
			EndLine = EndLine,
			EndColumn = EndColumn
		};
	}
}

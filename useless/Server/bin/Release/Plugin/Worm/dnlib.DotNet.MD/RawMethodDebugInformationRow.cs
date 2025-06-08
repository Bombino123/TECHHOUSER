using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawMethodDebugInformationRow
{
	public readonly uint Document;

	public readonly uint SequencePoints;

	public uint this[int index] => index switch
	{
		0 => Document, 
		1 => SequencePoints, 
		_ => 0u, 
	};

	public RawMethodDebugInformationRow(uint Document, uint SequencePoints)
	{
		this.Document = Document;
		this.SequencePoints = SequencePoints;
	}
}

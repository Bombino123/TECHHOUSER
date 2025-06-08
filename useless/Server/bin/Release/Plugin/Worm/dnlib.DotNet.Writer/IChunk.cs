using System.Runtime.InteropServices;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public interface IChunk
{
	FileOffset FileOffset { get; }

	RVA RVA { get; }

	void SetOffset(FileOffset offset, RVA rva);

	uint GetFileLength();

	uint GetVirtualSize();

	uint CalculateAlignment();

	void WriteTo(DataWriter writer);
}

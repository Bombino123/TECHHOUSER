using dnlib.PE;

namespace dnlib.DotNet.Writer;

internal interface IReuseChunk : IChunk
{
	bool CanReuse(RVA origRva, uint origSize);
}

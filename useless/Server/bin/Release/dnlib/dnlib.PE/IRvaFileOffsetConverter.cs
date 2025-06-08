using dnlib.IO;

namespace dnlib.PE;

public interface IRvaFileOffsetConverter
{
	RVA ToRVA(FileOffset offset);

	FileOffset ToFileOffset(RVA rva);
}

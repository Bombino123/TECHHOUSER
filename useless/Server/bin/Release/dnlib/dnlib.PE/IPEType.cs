using dnlib.IO;

namespace dnlib.PE;

internal interface IPEType
{
	RVA ToRVA(PEInfo peInfo, FileOffset offset);

	FileOffset ToFileOffset(PEInfo peInfo, RVA rva);
}

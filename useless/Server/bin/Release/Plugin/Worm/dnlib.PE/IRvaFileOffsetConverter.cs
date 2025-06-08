using System.Runtime.InteropServices;
using dnlib.IO;

namespace dnlib.PE;

[ComVisible(true)]
public interface IRvaFileOffsetConverter
{
	RVA ToRVA(FileOffset offset);

	FileOffset ToFileOffset(RVA rva);
}

using System;
using System.Runtime.InteropServices;

namespace dnlib.PE;

[ComVisible(true)]
public interface IInternalPEImage : IPEImage, IRvaFileOffsetConverter, IDisposable
{
	bool IsMemoryMappedIO { get; }

	void UnsafeDisableMemoryMappedIO();
}

using System;

namespace dnlib.PE;

public interface IInternalPEImage : IPEImage, IRvaFileOffsetConverter, IDisposable
{
	bool IsMemoryMappedIO { get; }

	void UnsafeDisableMemoryMappedIO();
}

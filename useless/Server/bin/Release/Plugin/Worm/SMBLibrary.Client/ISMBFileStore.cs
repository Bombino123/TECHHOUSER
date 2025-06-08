using System.Runtime.InteropServices;

namespace SMBLibrary.Client;

[ComVisible(true)]
public interface ISMBFileStore : INTFileStore
{
	uint MaxReadSize { get; }

	uint MaxWriteSize { get; }

	NTStatus Disconnect();
}

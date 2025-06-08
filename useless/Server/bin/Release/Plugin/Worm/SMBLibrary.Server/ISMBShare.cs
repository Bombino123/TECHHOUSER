using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public interface ISMBShare
{
	string Name { get; }

	INTFileStore FileStore { get; }
}

using System.IO;
using System.Threading.Tasks;

namespace System.Net.Http;

internal class StreamToStreamCopy
{
	public extern StreamToStreamCopy(Stream source, Stream destination, int bufferSize, bool disposeSource);

	public extern Task StartAsync();
}

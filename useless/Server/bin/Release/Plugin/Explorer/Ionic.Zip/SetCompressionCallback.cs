using System.Runtime.InteropServices;
using Ionic.Zlib;

namespace Ionic.Zip;

[ComVisible(true)]
public delegate CompressionLevel SetCompressionCallback(string localFileName, string fileNameInArchive);

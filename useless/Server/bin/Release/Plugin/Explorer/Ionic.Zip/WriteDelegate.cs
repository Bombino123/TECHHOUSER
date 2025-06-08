using System.IO;
using System.Runtime.InteropServices;

namespace Ionic.Zip;

[ComVisible(true)]
public delegate void WriteDelegate(string entryName, Stream stream);

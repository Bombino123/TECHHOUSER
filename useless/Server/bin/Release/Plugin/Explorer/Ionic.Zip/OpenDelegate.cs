using System.IO;
using System.Runtime.InteropServices;

namespace Ionic.Zip;

[ComVisible(true)]
public delegate Stream OpenDelegate(string entryName);

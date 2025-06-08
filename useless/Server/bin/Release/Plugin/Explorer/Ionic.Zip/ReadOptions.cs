using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ionic.Zip;

[ComVisible(true)]
public class ReadOptions
{
	public EventHandler<ReadProgressEventArgs> ReadProgress { get; set; }

	public TextWriter StatusMessageWriter { get; set; }

	public Encoding Encoding { get; set; }
}

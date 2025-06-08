using System.IO;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Resources;

[ComVisible(true)]
public sealed class ResourceBinaryWriter : BinaryWriter
{
	public int FormatVersion { get; internal set; }

	public ResourceReaderType ReaderType { get; internal set; }

	internal ResourceBinaryWriter(Stream stream)
		: base(stream)
	{
	}

	public new void Write7BitEncodedInt(int value)
	{
		base.Write7BitEncodedInt(value);
	}
}

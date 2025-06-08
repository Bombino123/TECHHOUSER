using System.Runtime.InteropServices;

namespace dnlib.DotNet.Resources;

[ComVisible(true)]
public enum SerializationFormat
{
	BinaryFormatter = 1,
	TypeConverterByteArray,
	TypeConverterString,
	ActivatorStream
}

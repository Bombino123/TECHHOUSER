using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public enum PdbFileKind
{
	WindowsPDB,
	PortablePDB,
	EmbeddedPortablePDB
}

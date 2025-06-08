using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public interface INDRStructure
{
	void Read(NDRParser parser);

	void Write(NDRWriter writer);
}

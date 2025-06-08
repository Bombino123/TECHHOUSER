using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public abstract class WorkstationInfoLevel : INDRStructure
{
	public abstract uint Level { get; }

	public abstract void Read(NDRParser parser);

	public abstract void Write(NDRWriter writer);
}

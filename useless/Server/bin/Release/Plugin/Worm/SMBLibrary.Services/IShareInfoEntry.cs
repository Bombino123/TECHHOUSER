using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public interface IShareInfoEntry : INDRStructure
{
	uint Level { get; }
}

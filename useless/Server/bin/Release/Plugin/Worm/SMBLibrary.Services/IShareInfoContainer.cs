using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public interface IShareInfoContainer : INDRStructure
{
	uint Level { get; }
}

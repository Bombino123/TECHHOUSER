using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IHasCustomDebugInformation
{
	int HasCustomDebugInformationTag { get; }

	IList<PdbCustomDebugInfo> CustomDebugInfos { get; }

	bool HasCustomDebugInfos { get; }
}

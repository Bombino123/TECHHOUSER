using System.Collections.Generic;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

public interface IHasCustomDebugInformation
{
	int HasCustomDebugInformationTag { get; }

	IList<PdbCustomDebugInfo> CustomDebugInfos { get; }

	bool HasCustomDebugInfos { get; }
}

using System.Collections.Generic;
using dnlib.DotNet;

namespace dnlib.Utils;

internal sealed class ParameterList_CollectionDebugView : CollectionDebugView<Parameter>
{
	public ParameterList_CollectionDebugView(ParameterList list)
		: base((ICollection<Parameter>)list)
	{
	}
}

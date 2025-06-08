using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.Utils;

internal sealed class LocalList_CollectionDebugView : CollectionDebugView<Local>
{
	public LocalList_CollectionDebugView(LocalList list)
		: base((ICollection<Local>)list)
	{
	}
}

using System.Runtime.InteropServices;

namespace System.Collections.Generic;

[ComVisible(true)]
public interface IReadOnlyCollection<T> : IEnumerable<T>, IEnumerable
{
	int Count { get; }
}

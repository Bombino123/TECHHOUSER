using System.Runtime.InteropServices;

namespace System.Collections.Generic;

[ComVisible(true)]
public interface IReadOnlyList<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	T this[int index] { get; }
}

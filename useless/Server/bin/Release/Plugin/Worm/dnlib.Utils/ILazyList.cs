using System.Collections;
using System.Collections.Generic;

namespace dnlib.Utils;

internal interface ILazyList<TValue> : IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable
{
}

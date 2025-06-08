using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Internal;

internal interface IDbEnumerator<out T> : IEnumerator<T>, IDisposable, IEnumerator, IDbAsyncEnumerator<T>, IDbAsyncEnumerator
{
	new T Current { get; }
}

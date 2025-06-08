using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Net.Http.Headers;

public sealed class HttpHeaderValueCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : class
{
	[CompilerGenerated]
	private sealed class _003CGetEnumerator_003Ed__21 : IEnumerator<T>, IDisposable, IEnumerator
	{
		extern T IEnumerator<T>.Current
		{
			[DebuggerHidden]
			get;
		}

		extern object IEnumerator.Current
		{
			[DebuggerHidden]
			get;
		}

		[DebuggerHidden]
		extern void IDisposable.Dispose();

		private extern bool MoveNext();

		[DebuggerHidden]
		extern void IEnumerator.Reset();

		private extern _003CGetEnumerator_003Ed__21();
	}

	public extern int Count { get; }

	public extern bool IsReadOnly { get; }

	internal extern bool IsSpecialValueSet { get; }

	internal extern HttpHeaderValueCollection(string headerName, HttpHeaders store);

	internal extern HttpHeaderValueCollection(string headerName, HttpHeaders store, Action<HttpHeaderValueCollection<T>, T> validator);

	internal extern HttpHeaderValueCollection(string headerName, HttpHeaders store, T specialValue);

	internal extern HttpHeaderValueCollection(string headerName, HttpHeaders store, T specialValue, Action<HttpHeaderValueCollection<T>, T> validator);

	public extern void Add(T item);

	public extern void ParseAdd(string input);

	public extern bool TryParseAdd(string input);

	public extern void Clear();

	public extern bool Contains(T item);

	public extern void CopyTo(T[] array, int arrayIndex);

	public extern bool Remove(T item);

	[IteratorStateMachine(typeof(HttpHeaderValueCollection<>._003CGetEnumerator_003Ed__21))]
	public extern IEnumerator<T> GetEnumerator();

	extern IEnumerator IEnumerable.GetEnumerator();

	public override extern string ToString();

	internal extern string GetHeaderStringWithoutSpecial();

	internal extern void SetSpecialValue();

	internal extern void RemoveSpecialValue();
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Net.Http.Headers;

public abstract class HttpHeaders : IEnumerable<KeyValuePair<string, IEnumerable<string>>>, IEnumerable
{
	[CompilerGenerated]
	private sealed class _003CGetHeaderStrings_003Ed__15 : IEnumerable<KeyValuePair<string, string>>, IEnumerable, IEnumerator<KeyValuePair<string, string>>, IDisposable, IEnumerator
	{
		extern KeyValuePair<string, string> IEnumerator<KeyValuePair<string, string>>.Current
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

		[DebuggerHidden]
		extern IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator();

		[DebuggerHidden]
		extern IEnumerator IEnumerable.GetEnumerator();

		private extern _003CGetHeaderStrings_003Ed__15();
	}

	[CompilerGenerated]
	private sealed class _003CGetEnumerator_003Ed__20 : IEnumerator<KeyValuePair<string, IEnumerable<string>>>, IDisposable, IEnumerator
	{
		extern KeyValuePair<string, IEnumerable<string>> IEnumerator<KeyValuePair<string, IEnumerable<string>>>.Current
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

		private extern _003CGetEnumerator_003Ed__20();
	}

	protected extern HttpHeaders();

	public extern void Add(string name, string value);

	public extern void Add(string name, IEnumerable<string> values);

	public extern bool TryAddWithoutValidation(string name, string value);

	public extern bool TryAddWithoutValidation(string name, IEnumerable<string> values);

	public extern void Clear();

	public extern bool Remove(string name);

	public extern IEnumerable<string> GetValues(string name);

	public extern bool TryGetValues(string name, out IEnumerable<string> values);

	public extern bool Contains(string name);

	public override extern string ToString();

	[IteratorStateMachine(typeof(_003CGetHeaderStrings_003Ed__15))]
	internal extern IEnumerable<KeyValuePair<string, string>> GetHeaderStrings();

	internal extern string GetHeaderString(string headerName);

	internal extern string GetHeaderString(string headerName, object exclude);

	[IteratorStateMachine(typeof(_003CGetEnumerator_003Ed__20))]
	public extern IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator();

	extern IEnumerator IEnumerable.GetEnumerator();

	internal extern void SetConfiguration(Dictionary<string, HttpHeaderParser> parserStore, HashSet<string> invalidHeaders);

	internal extern void AddParsedValue(string name, object value);

	internal extern void SetParsedValue(string name, object value);

	internal extern void SetOrRemoveParsedValue(string name, object value);

	internal extern bool RemoveParsedValue(string name, object value);

	internal extern bool ContainsParsedValue(string name, object value);

	internal virtual extern void AddHeaders(HttpHeaders sourceHeaders);

	internal extern bool TryParseAndAddValue(string name, string value);

	internal extern object GetParsedValues(string name);
}

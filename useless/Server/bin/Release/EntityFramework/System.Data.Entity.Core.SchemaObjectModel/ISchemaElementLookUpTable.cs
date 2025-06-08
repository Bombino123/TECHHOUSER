using System.Collections.Generic;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal interface ISchemaElementLookUpTable<T> where T : SchemaElement
{
	int Count { get; }

	T this[string key] { get; }

	bool ContainsKey(string key);

	IEnumerator<T> GetEnumerator();

	T LookUpEquivalentKey(string key);
}

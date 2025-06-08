using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class SchemaElementLookUpTable<T> : IEnumerable<T>, IEnumerable, ISchemaElementLookUpTable<T> where T : SchemaElement
{
	private Dictionary<string, T> _keyToType;

	private readonly List<string> _keysInDefOrder = new List<string>();

	public int Count => KeyToType.Count;

	public T this[string key] => KeyToType[KeyFromName(key)];

	private Dictionary<string, T> KeyToType
	{
		get
		{
			if (_keyToType == null)
			{
				_keyToType = new Dictionary<string, T>(StringComparer.Ordinal);
			}
			return _keyToType;
		}
	}

	public bool ContainsKey(string key)
	{
		return KeyToType.ContainsKey(KeyFromName(key));
	}

	public T LookUpEquivalentKey(string key)
	{
		key = KeyFromName(key);
		if (KeyToType.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public T GetElementAt(int index)
	{
		return KeyToType[_keysInDefOrder[index]];
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new SchemaElementLookUpTableEnumerator<T, T>(KeyToType, _keysInDefOrder);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new SchemaElementLookUpTableEnumerator<T, T>(KeyToType, _keysInDefOrder);
	}

	public IEnumerator<S> GetFilteredEnumerator<S>() where S : T
	{
		return new SchemaElementLookUpTableEnumerator<S, T>(KeyToType, _keysInDefOrder);
	}

	public AddErrorKind TryAdd(T type)
	{
		if (string.IsNullOrEmpty(type.Identity))
		{
			return AddErrorKind.MissingNameError;
		}
		string text = KeyFromElement(type);
		if (KeyToType.TryGetValue(text, out var _))
		{
			return AddErrorKind.DuplicateNameError;
		}
		KeyToType.Add(text, type);
		_keysInDefOrder.Add(text);
		return AddErrorKind.Succeeded;
	}

	public void Add(T type, bool doNotAddErrorForEmptyName, Func<object, string> duplicateKeyErrorFormat)
	{
		switch (TryAdd(type))
		{
		case AddErrorKind.MissingNameError:
			if (!doNotAddErrorForEmptyName)
			{
				type.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, Strings.MissingName);
			}
			break;
		case AddErrorKind.DuplicateNameError:
			type.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, duplicateKeyErrorFormat(type.FQName));
			break;
		}
	}

	private static string KeyFromElement(T type)
	{
		return KeyFromName(type.Identity);
	}

	private static string KeyFromName(string unnormalizedKey)
	{
		return unnormalizedKey;
	}
}

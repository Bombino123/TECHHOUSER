using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Internal;

internal class WrappedEntityKey
{
	private readonly IEnumerable<KeyValuePair<string, object>> _keyValuePairs;

	private readonly EntityKey _key;

	public bool HasNullValues => _key == null;

	public EntityKey EntityKey => _key;

	public IEnumerable<KeyValuePair<string, object>> KeyValuePairs => _keyValuePairs;

	public WrappedEntityKey(EntitySet entitySet, string entitySetName, object[] keyValues, string keyValuesParamName)
	{
		if (keyValues == null)
		{
			keyValues = new object[1];
		}
		List<string> list = entitySet.ElementType.KeyMembers.Select((EdmMember m) => m.Name).ToList();
		if (list.Count != keyValues.Length)
		{
			throw new ArgumentException(Strings.DbSet_WrongNumberOfKeyValuesPassed, keyValuesParamName);
		}
		_keyValuePairs = list.Zip(keyValues, (string name, object value) => new KeyValuePair<string, object>(name, value));
		if (keyValues.All((object v) => v != null))
		{
			_key = new EntityKey(entitySetName, KeyValuePairs);
		}
	}
}

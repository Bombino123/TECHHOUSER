using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
[DataContract]
public class EntityKeyMember
{
	private string _keyName;

	private object _keyValue;

	[DataMember]
	public string Key
	{
		get
		{
			return _keyName;
		}
		set
		{
			Check.NotNull(value, "value");
			ValidateWritable(_keyName);
			_keyName = value;
		}
	}

	[DataMember]
	public object Value
	{
		get
		{
			return _keyValue;
		}
		set
		{
			Check.NotNull(value, "value");
			ValidateWritable(_keyValue);
			_keyValue = value;
		}
	}

	public EntityKeyMember()
	{
	}

	public EntityKeyMember(string keyName, object keyValue)
	{
		Check.NotNull(keyName, "keyName");
		Check.NotNull(keyValue, "keyValue");
		_keyName = keyName;
		_keyValue = keyValue;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "[{0}, {1}]", new object[2] { _keyName, _keyValue });
	}

	private static void ValidateWritable(object instance)
	{
		if (instance != null)
		{
			throw new InvalidOperationException(Strings.EntityKey_CannotChangeKey);
		}
	}
}

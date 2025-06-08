using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace System.Data.Entity.Core;

[Serializable]
[DebuggerDisplay("{ConcatKeyValue()}")]
[DataContract(IsReference = true)]
public sealed class EntityKey : IEquatable<EntityKey>
{
	private class KeyValueReader : IEnumerable<KeyValuePair<string, object>>, IEnumerable
	{
		private readonly IEnumerable<EntityKeyMember> _enumerator;

		public KeyValueReader(IEnumerable<EntityKeyMember> enumerator)
		{
			_enumerator = enumerator;
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			foreach (EntityKeyMember item in _enumerator)
			{
				if (item != null)
				{
					yield return new KeyValuePair<string, object>(item.Key, item.Value);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private string _entitySetName;

	private string _entityContainerName;

	private object _singletonKeyValue;

	private object[] _compositeKeyValues;

	private string[] _keyNames;

	private readonly bool _isLocked;

	[NonSerialized]
	private bool _containsByteArray;

	[NonSerialized]
	private EntityKeyMember[] _deserializedMembers;

	[NonSerialized]
	private int _hashCode;

	private static readonly EntityKey _noEntitySetKey = new EntityKey("NoEntitySetKey.NoEntitySetKey");

	private static readonly EntityKey _entityNotValidKey = new EntityKey("EntityNotValidKey.EntityNotValidKey");

	private static readonly ConcurrentDictionary<string, string> NameLookup = new ConcurrentDictionary<string, string>();

	public static EntityKey NoEntitySetKey => _noEntitySetKey;

	public static EntityKey EntityNotValidKey => _entityNotValidKey;

	[DataMember]
	public string EntitySetName
	{
		get
		{
			return _entitySetName;
		}
		set
		{
			ValidateWritable(_entitySetName);
			_entitySetName = LookupSingletonName(value);
		}
	}

	[DataMember]
	public string EntityContainerName
	{
		get
		{
			return _entityContainerName;
		}
		set
		{
			ValidateWritable(_entityContainerName);
			_entityContainerName = LookupSingletonName(value);
		}
	}

	[DataMember]
	public EntityKeyMember[] EntityKeyValues
	{
		get
		{
			if (!IsTemporary)
			{
				EntityKeyMember[] array;
				if (_singletonKeyValue != null)
				{
					array = new EntityKeyMember[1]
					{
						new EntityKeyMember(_keyNames[0], _singletonKeyValue)
					};
				}
				else
				{
					array = new EntityKeyMember[_compositeKeyValues.Length];
					for (int i = 0; i < _compositeKeyValues.Length; i++)
					{
						array[i] = new EntityKeyMember(_keyNames[i], _compositeKeyValues[i]);
					}
				}
				return array;
			}
			return null;
		}
		set
		{
			ValidateWritable(_keyNames);
			if (value != null && !InitializeKeyValues(new KeyValueReader(value), allowNullKeys: true, tokenizeStrings: true))
			{
				_deserializedMembers = value;
			}
		}
	}

	public bool IsTemporary
	{
		get
		{
			if (SingletonKeyValue == null)
			{
				return CompositeKeyValues == null;
			}
			return false;
		}
	}

	private object SingletonKeyValue
	{
		get
		{
			if (RequiresDeserialization)
			{
				DeserializeMembers();
			}
			return _singletonKeyValue;
		}
	}

	private object[] CompositeKeyValues
	{
		get
		{
			if (RequiresDeserialization)
			{
				DeserializeMembers();
			}
			return _compositeKeyValues;
		}
	}

	private bool RequiresDeserialization => _deserializedMembers != null;

	public EntityKey()
	{
	}

	public EntityKey(string qualifiedEntitySetName, IEnumerable<KeyValuePair<string, object>> entityKeyValues)
	{
		Check.NotEmpty(qualifiedEntitySetName, "qualifiedEntitySetName");
		Check.NotNull(entityKeyValues, "entityKeyValues");
		InitializeEntitySetName(qualifiedEntitySetName);
		InitializeKeyValues(entityKeyValues);
		_isLocked = true;
	}

	public EntityKey(string qualifiedEntitySetName, IEnumerable<EntityKeyMember> entityKeyValues)
	{
		Check.NotEmpty(qualifiedEntitySetName, "qualifiedEntitySetName");
		Check.NotNull(entityKeyValues, "entityKeyValues");
		InitializeEntitySetName(qualifiedEntitySetName);
		InitializeKeyValues(new KeyValueReader(entityKeyValues));
		_isLocked = true;
	}

	public EntityKey(string qualifiedEntitySetName, string keyName, object keyValue)
	{
		Check.NotEmpty(qualifiedEntitySetName, "qualifiedEntitySetName");
		Check.NotEmpty(keyName, "keyName");
		Check.NotNull(keyValue, "keyValue");
		InitializeEntitySetName(qualifiedEntitySetName);
		ValidateName(keyName);
		_keyNames = new string[1] { keyName };
		_singletonKeyValue = keyValue;
		_isLocked = true;
	}

	internal EntityKey(EntitySet entitySet, IExtendedDataRecord record)
	{
		_entitySetName = entitySet.Name;
		_entityContainerName = entitySet.EntityContainer.Name;
		InitializeKeyValues(entitySet, record);
		_isLocked = true;
	}

	internal EntityKey(string qualifiedEntitySetName)
	{
		InitializeEntitySetName(qualifiedEntitySetName);
		_isLocked = true;
	}

	internal EntityKey(EntitySetBase entitySet)
	{
		_entitySetName = entitySet.Name;
		_entityContainerName = entitySet.EntityContainer.Name;
		_isLocked = true;
	}

	internal EntityKey(EntitySetBase entitySet, object singletonKeyValue)
	{
		_singletonKeyValue = singletonKeyValue;
		_entitySetName = entitySet.Name;
		_entityContainerName = entitySet.EntityContainer.Name;
		_keyNames = entitySet.ElementType.KeyMemberNames;
		_isLocked = true;
	}

	internal EntityKey(EntitySetBase entitySet, object[] compositeKeyValues)
	{
		_compositeKeyValues = compositeKeyValues;
		_entitySetName = entitySet.Name;
		_entityContainerName = entitySet.EntityContainer.Name;
		_keyNames = entitySet.ElementType.KeyMemberNames;
		_isLocked = true;
	}

	public EntitySet GetEntitySet(MetadataWorkspace metadataWorkspace)
	{
		Check.NotNull(metadataWorkspace, "metadataWorkspace");
		if (string.IsNullOrEmpty(_entityContainerName) || string.IsNullOrEmpty(_entitySetName))
		{
			throw new InvalidOperationException(Strings.EntityKey_MissingEntitySetName);
		}
		return metadataWorkspace.GetEntityContainer(_entityContainerName, DataSpace.CSpace).GetEntitySetByName(_entitySetName, ignoreCase: false);
	}

	public override bool Equals(object obj)
	{
		return InternalEquals(this, obj as EntityKey, compareEntitySets: true);
	}

	public bool Equals(EntityKey other)
	{
		return InternalEquals(this, other, compareEntitySets: true);
	}

	public override int GetHashCode()
	{
		int num = _hashCode;
		if (num == 0)
		{
			_containsByteArray = false;
			if (RequiresDeserialization)
			{
				DeserializeMembers();
			}
			if (_entitySetName != null)
			{
				num = _entitySetName.GetHashCode();
			}
			if (_entityContainerName != null)
			{
				num ^= _entityContainerName.GetHashCode();
			}
			if (_singletonKeyValue != null)
			{
				num = AddHashValue(num, _singletonKeyValue);
			}
			else if (_compositeKeyValues != null)
			{
				int i = 0;
				for (int num2 = _compositeKeyValues.Length; i < num2; i++)
				{
					num = AddHashValue(num, _compositeKeyValues[i]);
				}
			}
			else
			{
				num = base.GetHashCode();
			}
			if (_isLocked || (!string.IsNullOrEmpty(_entitySetName) && !string.IsNullOrEmpty(_entityContainerName) && (_singletonKeyValue != null || _compositeKeyValues != null)))
			{
				_hashCode = num;
			}
		}
		return num;
	}

	private int AddHashValue(int hashCode, object keyValue)
	{
		if (keyValue is byte[] bytes)
		{
			hashCode ^= ByValueEqualityComparer.ComputeBinaryHashCode(bytes);
			_containsByteArray = true;
			return hashCode;
		}
		return hashCode ^ keyValue.GetHashCode();
	}

	public static bool operator ==(EntityKey key1, EntityKey key2)
	{
		return InternalEquals(key1, key2, compareEntitySets: true);
	}

	public static bool operator !=(EntityKey key1, EntityKey key2)
	{
		return !InternalEquals(key1, key2, compareEntitySets: true);
	}

	internal static bool InternalEquals(EntityKey key1, EntityKey key2, bool compareEntitySets)
	{
		if ((object)key1 == key2)
		{
			return true;
		}
		if ((object)key1 == null || (object)key2 == null)
		{
			return false;
		}
		if ((object)NoEntitySetKey == key1 || (object)EntityNotValidKey == key1 || (object)NoEntitySetKey == key2 || (object)EntityNotValidKey == key2)
		{
			return false;
		}
		if ((key1.GetHashCode() != key2.GetHashCode() && compareEntitySets) || key1._containsByteArray != key2._containsByteArray)
		{
			return false;
		}
		if (key1._singletonKeyValue != null)
		{
			if (key1._containsByteArray)
			{
				if (key2._singletonKeyValue == null)
				{
					return false;
				}
				if (!ByValueEqualityComparer.CompareBinaryValues((byte[])key1._singletonKeyValue, (byte[])key2._singletonKeyValue))
				{
					return false;
				}
			}
			else if (!key1._singletonKeyValue.Equals(key2._singletonKeyValue))
			{
				return false;
			}
			if (!string.Equals(key1._keyNames[0], key2._keyNames[0]))
			{
				return false;
			}
		}
		else
		{
			if (key1._compositeKeyValues == null || key2._compositeKeyValues == null || key1._compositeKeyValues.Length != key2._compositeKeyValues.Length)
			{
				return false;
			}
			if (key1._containsByteArray)
			{
				if (!CompositeValuesWithBinaryEqual(key1, key2))
				{
					return false;
				}
			}
			else if (!CompositeValuesEqual(key1, key2))
			{
				return false;
			}
		}
		if (compareEntitySets && (!string.Equals(key1._entitySetName, key2._entitySetName) || !string.Equals(key1._entityContainerName, key2._entityContainerName)))
		{
			return false;
		}
		return true;
	}

	internal static bool CompositeValuesWithBinaryEqual(EntityKey key1, EntityKey key2)
	{
		for (int i = 0; i < key1._compositeKeyValues.Length; i++)
		{
			if (key1._keyNames[i].Equals(key2._keyNames[i]))
			{
				if (!ByValueEqualityComparer.Default.Equals(key1._compositeKeyValues[i], key2._compositeKeyValues[i]))
				{
					return false;
				}
			}
			else if (!ValuesWithBinaryEqual(key1._keyNames[i], key1._compositeKeyValues[i], key2))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ValuesWithBinaryEqual(string keyName, object keyValue, EntityKey key2)
	{
		for (int i = 0; i < key2._keyNames.Length; i++)
		{
			if (string.Equals(keyName, key2._keyNames[i]))
			{
				return ByValueEqualityComparer.Default.Equals(keyValue, key2._compositeKeyValues[i]);
			}
		}
		return false;
	}

	private static bool CompositeValuesEqual(EntityKey key1, EntityKey key2)
	{
		for (int i = 0; i < key1._compositeKeyValues.Length; i++)
		{
			if (key1._keyNames[i].Equals(key2._keyNames[i]))
			{
				if (!object.Equals(key1._compositeKeyValues[i], key2._compositeKeyValues[i]))
				{
					return false;
				}
			}
			else if (!ValuesEqual(key1._keyNames[i], key1._compositeKeyValues[i], key2))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ValuesEqual(string keyName, object keyValue, EntityKey key2)
	{
		for (int i = 0; i < key2._keyNames.Length; i++)
		{
			if (string.Equals(keyName, key2._keyNames[i]))
			{
				return object.Equals(keyValue, key2._compositeKeyValues[i]);
			}
		}
		return false;
	}

	internal KeyValuePair<string, DbExpression>[] GetKeyValueExpressions(EntitySet entitySet)
	{
		int num = 0;
		if (!IsTemporary)
		{
			num = ((_singletonKeyValue != null) ? 1 : _compositeKeyValues.Length);
		}
		if (((EntitySetBase)entitySet).ElementType.KeyMembers.Count != num)
		{
			throw new ArgumentException(Strings.EntityKey_EntitySetDoesNotMatch(TypeHelpers.GetFullName(entitySet.EntityContainer.Name, entitySet.Name)), "entitySet");
		}
		KeyValuePair<string, DbExpression>[] array;
		if (_singletonKeyValue != null)
		{
			EdmMember edmMember = ((EntitySetBase)entitySet).ElementType.KeyMembers[0];
			array = new KeyValuePair<string, DbExpression>[1] { Helper.GetModelTypeUsage(edmMember).Constant(_singletonKeyValue).As(edmMember.Name) };
		}
		else
		{
			array = new KeyValuePair<string, DbExpression>[_compositeKeyValues.Length];
			for (int i = 0; i < _compositeKeyValues.Length; i++)
			{
				EdmMember edmMember2 = ((EntitySetBase)entitySet).ElementType.KeyMembers[i];
				array[i] = Helper.GetModelTypeUsage(edmMember2).Constant(_compositeKeyValues[i]).As(edmMember2.Name);
			}
		}
		return array;
	}

	internal string ConcatKeyValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("EntitySet=").Append(_entitySetName);
		if (!IsTemporary)
		{
			EntityKeyMember[] entityKeyValues = EntityKeyValues;
			foreach (EntityKeyMember entityKeyMember in entityKeyValues)
			{
				stringBuilder.Append(';');
				stringBuilder.Append(entityKeyMember.Key).Append("=").Append(entityKeyMember.Value);
			}
		}
		return stringBuilder.ToString();
	}

	internal object FindValueByName(string keyName)
	{
		if (SingletonKeyValue != null)
		{
			return _singletonKeyValue;
		}
		object[] compositeKeyValues = CompositeKeyValues;
		for (int i = 0; i < compositeKeyValues.Length; i++)
		{
			if (keyName == _keyNames[i])
			{
				return compositeKeyValues[i];
			}
		}
		throw new ArgumentOutOfRangeException("keyName");
	}

	internal void InitializeEntitySetName(string qualifiedEntitySetName)
	{
		string[] array = qualifiedEntitySetName.Split(new char[1] { '.' });
		if (array.Length != 2 || string.IsNullOrWhiteSpace(array[0]) || string.IsNullOrWhiteSpace(array[1]))
		{
			throw new ArgumentException(Strings.EntityKey_InvalidQualifiedEntitySetName, "qualifiedEntitySetName");
		}
		_entityContainerName = array[0];
		_entitySetName = array[1];
		ValidateName(_entityContainerName);
		ValidateName(_entitySetName);
	}

	private static void ValidateName(string name)
	{
		if (!name.IsValidUndottedName())
		{
			throw new ArgumentException(Strings.EntityKey_InvalidName(name));
		}
	}

	internal bool InitializeKeyValues(IEnumerable<KeyValuePair<string, object>> entityKeyValues, bool allowNullKeys = false, bool tokenizeStrings = false)
	{
		int num = entityKeyValues.Count();
		if (num == 1)
		{
			_keyNames = new string[1];
			KeyValuePair<string, object> keyValuePair = entityKeyValues.Single();
			InitializeKeyValue(keyValuePair, 0, tokenizeStrings);
			_singletonKeyValue = keyValuePair.Value;
		}
		else if (num > 1)
		{
			_keyNames = new string[num];
			_compositeKeyValues = new object[num];
			int num2 = 0;
			foreach (KeyValuePair<string, object> entityKeyValue in entityKeyValues)
			{
				InitializeKeyValue(entityKeyValue, num2, tokenizeStrings);
				_compositeKeyValues[num2] = entityKeyValue.Value;
				num2++;
			}
		}
		else if (!allowNullKeys)
		{
			throw new ArgumentException(Strings.EntityKey_EntityKeyMustHaveValues, "entityKeyValues");
		}
		return num > 0;
	}

	private void InitializeKeyValue(KeyValuePair<string, object> keyValuePair, int i, bool tokenizeStrings)
	{
		if (EntityUtil.IsNull(keyValuePair.Value) || string.IsNullOrWhiteSpace(keyValuePair.Key))
		{
			throw new ArgumentException(Strings.EntityKey_NoNullsAllowedInKeyValuePairs, "entityKeyValues");
		}
		ValidateName(keyValuePair.Key);
		_keyNames[i] = (tokenizeStrings ? LookupSingletonName(keyValuePair.Key) : keyValuePair.Key);
	}

	private void InitializeKeyValues(EntitySet entitySet, IExtendedDataRecord record)
	{
		int count = entitySet.ElementType.KeyMembers.Count;
		_keyNames = entitySet.ElementType.KeyMemberNames;
		EntityType entityType = (EntityType)record.DataRecordInfo.RecordType.EdmType;
		if (count == 1)
		{
			_singletonKeyValue = record[entityType.KeyMembers[0].Name];
			if (EntityUtil.IsNull(_singletonKeyValue))
			{
				throw new ArgumentException(Strings.EntityKey_NoNullsAllowedInKeyValuePairs, "record");
			}
			return;
		}
		_compositeKeyValues = new object[count];
		for (int i = 0; i < count; i++)
		{
			_compositeKeyValues[i] = record[entityType.KeyMembers[i].Name];
			if (EntityUtil.IsNull(_compositeKeyValues[i]))
			{
				throw new ArgumentException(Strings.EntityKey_NoNullsAllowedInKeyValuePairs, "record");
			}
		}
	}

	internal void ValidateEntityKey(MetadataWorkspace workspace, EntitySet entitySet)
	{
		ValidateEntityKey(workspace, entitySet, isArgumentException: false, null);
	}

	internal void ValidateEntityKey(MetadataWorkspace workspace, EntitySet entitySet, bool isArgumentException, string argumentName)
	{
		if (entitySet == null)
		{
			return;
		}
		ReadOnlyMetadataCollection<EdmMember> keyMembers = ((EntitySetBase)entitySet).ElementType.KeyMembers;
		if (_singletonKeyValue != null)
		{
			if (keyMembers.Count != 1)
			{
				if (isArgumentException)
				{
					throw new ArgumentException(Strings.EntityKey_IncorrectNumberOfKeyValuePairs(entitySet.ElementType.FullName, keyMembers.Count, 1), argumentName);
				}
				throw new InvalidOperationException(Strings.EntityKey_IncorrectNumberOfKeyValuePairs(entitySet.ElementType.FullName, keyMembers.Count, 1));
			}
			ValidateTypeOfKeyValue(workspace, keyMembers[0], _singletonKeyValue, isArgumentException, argumentName);
			if (_keyNames[0] != keyMembers[0].Name)
			{
				if (isArgumentException)
				{
					throw new ArgumentException(Strings.EntityKey_MissingKeyValue(keyMembers[0].Name, entitySet.ElementType.FullName), argumentName);
				}
				throw new InvalidOperationException(Strings.EntityKey_MissingKeyValue(keyMembers[0].Name, entitySet.ElementType.FullName));
			}
		}
		else
		{
			if (_compositeKeyValues == null)
			{
				return;
			}
			if (keyMembers.Count != _compositeKeyValues.Length)
			{
				if (isArgumentException)
				{
					throw new ArgumentException(Strings.EntityKey_IncorrectNumberOfKeyValuePairs(entitySet.ElementType.FullName, keyMembers.Count, _compositeKeyValues.Length), argumentName);
				}
				throw new InvalidOperationException(Strings.EntityKey_IncorrectNumberOfKeyValuePairs(entitySet.ElementType.FullName, keyMembers.Count, _compositeKeyValues.Length));
			}
			for (int i = 0; i < _compositeKeyValues.Length; i++)
			{
				EdmMember edmMember = ((EntitySetBase)entitySet).ElementType.KeyMembers[i];
				bool flag = false;
				for (int j = 0; j < _compositeKeyValues.Length; j++)
				{
					if (edmMember.Name == _keyNames[j])
					{
						ValidateTypeOfKeyValue(workspace, edmMember, _compositeKeyValues[j], isArgumentException, argumentName);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					if (isArgumentException)
					{
						throw new ArgumentException(Strings.EntityKey_MissingKeyValue(edmMember.Name, entitySet.ElementType.FullName), argumentName);
					}
					throw new InvalidOperationException(Strings.EntityKey_MissingKeyValue(edmMember.Name, entitySet.ElementType.FullName));
				}
			}
		}
	}

	private static void ValidateTypeOfKeyValue(MetadataWorkspace workspace, EdmMember keyMember, object keyValue, bool isArgumentException, string argumentName)
	{
		EdmType edmType = keyMember.TypeUsage.EdmType;
		if (Helper.IsPrimitiveType(edmType))
		{
			Type clrEquivalentType = ((PrimitiveType)edmType).ClrEquivalentType;
			if (clrEquivalentType != keyValue.GetType())
			{
				if (isArgumentException)
				{
					throw new ArgumentException(Strings.EntityKey_IncorrectValueType(keyMember.Name, clrEquivalentType.FullName, keyValue.GetType().FullName), argumentName);
				}
				throw new InvalidOperationException(Strings.EntityKey_IncorrectValueType(keyMember.Name, clrEquivalentType.FullName, keyValue.GetType().FullName));
			}
			return;
		}
		if (workspace.TryGetObjectSpaceType((EnumType)edmType, out var objectSpaceType))
		{
			Type clrType = objectSpaceType.ClrType;
			if (clrType != keyValue.GetType())
			{
				if (isArgumentException)
				{
					throw new ArgumentException(Strings.EntityKey_IncorrectValueType(keyMember.Name, clrType.FullName, keyValue.GetType().FullName), argumentName);
				}
				throw new InvalidOperationException(Strings.EntityKey_IncorrectValueType(keyMember.Name, clrType.FullName, keyValue.GetType().FullName));
			}
			return;
		}
		if (isArgumentException)
		{
			throw new ArgumentException(Strings.EntityKey_NoCorrespondingOSpaceTypeForEnumKeyMember(keyMember.Name, edmType.FullName), argumentName);
		}
		throw new InvalidOperationException(Strings.EntityKey_NoCorrespondingOSpaceTypeForEnumKeyMember(keyMember.Name, edmType.FullName));
	}

	[Conditional("DEBUG")]
	private void AssertCorrectState(EntitySetBase entitySetBase, bool isTemporary)
	{
		_ = (EntitySet)entitySetBase;
		if (_singletonKeyValue != null)
		{
			return;
		}
		if (_compositeKeyValues != null)
		{
			for (int i = 0; i < _compositeKeyValues.Length; i++)
			{
			}
		}
		else
		{
			_ = IsTemporary;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[OnDeserializing]
	public void OnDeserializing(StreamingContext context)
	{
		if (RequiresDeserialization)
		{
			DeserializeMembers();
		}
	}

	[OnDeserialized]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	public void OnDeserialized(StreamingContext context)
	{
		_entitySetName = LookupSingletonName(_entitySetName);
		_entityContainerName = LookupSingletonName(_entityContainerName);
		if (_keyNames != null)
		{
			for (int i = 0; i < _keyNames.Length; i++)
			{
				_keyNames[i] = LookupSingletonName(_keyNames[i]);
			}
		}
	}

	internal static string LookupSingletonName(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return NameLookup.GetOrAdd(name, (string n) => n);
		}
		return null;
	}

	private void ValidateWritable(object instance)
	{
		if (_isLocked || instance != null)
		{
			throw new InvalidOperationException(Strings.EntityKey_CannotChangeKey);
		}
	}

	private void DeserializeMembers()
	{
		if (InitializeKeyValues(new KeyValueReader(_deserializedMembers), allowNullKeys: true, tokenizeStrings: true))
		{
			_deserializedMembers = null;
		}
	}
}

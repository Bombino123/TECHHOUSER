using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.Objects;

internal sealed class MaterializedDataRecord : DbDataRecord, IExtendedDataRecord, IDataRecord, ICustomTypeDescriptor
{
	private class FilterCache
	{
		public Attribute[] Attributes;

		public PropertyDescriptorCollection FilteredProperties;

		public bool IsValid(Attribute[] other)
		{
			if (other == null || Attributes == null)
			{
				return false;
			}
			if (Attributes.Length != other.Length)
			{
				return false;
			}
			for (int i = 0; i < other.Length; i++)
			{
				if (!Attributes[i].Match(other[i]))
				{
					return false;
				}
			}
			return true;
		}
	}

	private FieldNameLookup _fieldNameLookup;

	private DataRecordInfo _recordInfo;

	private readonly MetadataWorkspace _workspace;

	private readonly TypeUsage _edmUsage;

	private readonly object[] _values;

	private PropertyDescriptorCollection _propertyDescriptors;

	private FilterCache _filterCache;

	private Dictionary<object, AttributeCollection> _attrCache;

	public DataRecordInfo DataRecordInfo
	{
		get
		{
			if (_recordInfo == null)
			{
				if (_workspace == null)
				{
					_recordInfo = new DataRecordInfo(_edmUsage);
				}
				else
				{
					_recordInfo = new DataRecordInfo(_workspace.GetOSpaceTypeUsage(_edmUsage));
				}
			}
			return _recordInfo;
		}
	}

	public override int FieldCount => _values.Length;

	public override object this[int ordinal] => GetValue(ordinal);

	public override object this[string name] => GetValue(GetOrdinal(name));

	internal MaterializedDataRecord(MetadataWorkspace workspace, TypeUsage edmUsage, object[] values)
	{
		_workspace = workspace;
		_edmUsage = edmUsage;
		_values = values;
	}

	public override bool GetBoolean(int ordinal)
	{
		return (bool)_values[ordinal];
	}

	public override byte GetByte(int ordinal)
	{
		return (byte)_values[ordinal];
	}

	public override long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
	{
		int num = 0;
		byte[] array = (byte[])_values[ordinal];
		num = array.Length;
		if (fieldOffset > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("fieldOffset", Strings.ADP_InvalidSourceBufferIndex(num.ToString(CultureInfo.InvariantCulture), fieldOffset.ToString(CultureInfo.InvariantCulture)));
		}
		int num2 = (int)fieldOffset;
		if (buffer == null)
		{
			return num;
		}
		try
		{
			if (num2 < num)
			{
				num = ((num2 + length <= num) ? length : (num - num2));
			}
			Array.Copy(array, num2, buffer, bufferOffset, num);
		}
		catch (Exception e)
		{
			if (e.IsCatchableExceptionType())
			{
				num = array.Length;
				if (length < 0)
				{
					throw new IndexOutOfRangeException(Strings.ADP_InvalidDataLength(((long)length).ToString(CultureInfo.InvariantCulture)));
				}
				if (bufferOffset < 0 || bufferOffset >= buffer.Length)
				{
					throw new ArgumentOutOfRangeException("bufferOffset", Strings.ADP_InvalidDestinationBufferIndex(length.ToString(CultureInfo.InvariantCulture), bufferOffset.ToString(CultureInfo.InvariantCulture)));
				}
				if (fieldOffset < 0 || fieldOffset >= num)
				{
					throw new ArgumentOutOfRangeException("fieldOffset", Strings.ADP_InvalidSourceBufferIndex(length.ToString(CultureInfo.InvariantCulture), fieldOffset.ToString(CultureInfo.InvariantCulture)));
				}
				if (num + bufferOffset > buffer.Length)
				{
					throw new IndexOutOfRangeException(Strings.ADP_InvalidBufferSizeOrIndex(num.ToString(CultureInfo.InvariantCulture), bufferOffset.ToString(CultureInfo.InvariantCulture)));
				}
			}
			throw;
		}
		return num;
	}

	public override char GetChar(int ordinal)
	{
		return ((string)GetValue(ordinal))[0];
	}

	public override long GetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
	{
		int num = 0;
		string text = (string)_values[ordinal];
		num = text.Length;
		if (fieldOffset > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("fieldOffset", Strings.ADP_InvalidSourceBufferIndex(num.ToString(CultureInfo.InvariantCulture), fieldOffset.ToString(CultureInfo.InvariantCulture)));
		}
		int num2 = (int)fieldOffset;
		if (buffer == null)
		{
			return num;
		}
		try
		{
			if (num2 < num)
			{
				num = ((num2 + length <= num) ? length : (num - num2));
			}
			text.CopyTo(num2, buffer, bufferOffset, num);
		}
		catch (Exception e)
		{
			if (e.IsCatchableExceptionType())
			{
				num = text.Length;
				if (length < 0)
				{
					throw new IndexOutOfRangeException(Strings.ADP_InvalidDataLength(((long)length).ToString(CultureInfo.InvariantCulture)));
				}
				if (bufferOffset < 0 || bufferOffset >= buffer.Length)
				{
					throw new ArgumentOutOfRangeException("bufferOffset", Strings.ADP_InvalidDestinationBufferIndex(buffer.Length.ToString(CultureInfo.InvariantCulture), bufferOffset.ToString(CultureInfo.InvariantCulture)));
				}
				if (fieldOffset < 0 || fieldOffset >= num)
				{
					throw new ArgumentOutOfRangeException("fieldOffset", Strings.ADP_InvalidSourceBufferIndex(num.ToString(CultureInfo.InvariantCulture), fieldOffset.ToString(CultureInfo.InvariantCulture)));
				}
				if (num + bufferOffset > buffer.Length)
				{
					throw new IndexOutOfRangeException(Strings.ADP_InvalidBufferSizeOrIndex(num.ToString(CultureInfo.InvariantCulture), bufferOffset.ToString(CultureInfo.InvariantCulture)));
				}
			}
			throw;
		}
		return num;
	}

	public DbDataRecord GetDataRecord(int ordinal)
	{
		return (DbDataRecord)_values[ordinal];
	}

	public DbDataReader GetDataReader(int i)
	{
		return GetDbDataReader(i);
	}

	public override string GetDataTypeName(int ordinal)
	{
		return GetMember(ordinal).TypeUsage.EdmType.Name;
	}

	public override DateTime GetDateTime(int ordinal)
	{
		return (DateTime)_values[ordinal];
	}

	public override decimal GetDecimal(int ordinal)
	{
		return (decimal)_values[ordinal];
	}

	public override double GetDouble(int ordinal)
	{
		return (double)_values[ordinal];
	}

	public override Type GetFieldType(int ordinal)
	{
		return GetMember(ordinal).TypeUsage.EdmType.ClrType ?? typeof(object);
	}

	public override float GetFloat(int ordinal)
	{
		return (float)_values[ordinal];
	}

	public override Guid GetGuid(int ordinal)
	{
		return (Guid)_values[ordinal];
	}

	public override short GetInt16(int ordinal)
	{
		return (short)_values[ordinal];
	}

	public override int GetInt32(int ordinal)
	{
		return (int)_values[ordinal];
	}

	public override long GetInt64(int ordinal)
	{
		return (long)_values[ordinal];
	}

	public override string GetName(int ordinal)
	{
		return GetMember(ordinal).Name;
	}

	public override int GetOrdinal(string name)
	{
		if (_fieldNameLookup == null)
		{
			_fieldNameLookup = new FieldNameLookup(this);
		}
		return _fieldNameLookup.GetOrdinal(name);
	}

	public override string GetString(int ordinal)
	{
		return (string)_values[ordinal];
	}

	public override object GetValue(int ordinal)
	{
		return _values[ordinal];
	}

	public override int GetValues(object[] values)
	{
		Check.NotNull(values, "values");
		int num = Math.Min(values.Length, FieldCount);
		for (int i = 0; i < num; i++)
		{
			values[i] = _values[i];
		}
		return num;
	}

	private EdmMember GetMember(int ordinal)
	{
		return DataRecordInfo.FieldMetadata[ordinal].FieldType;
	}

	public override bool IsDBNull(int ordinal)
	{
		return DBNull.Value == _values[ordinal];
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return TypeDescriptor.GetAttributes(this, noCustomTypeDesc: true);
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return null;
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return null;
	}

	private PropertyDescriptorCollection InitializePropertyDescriptors()
	{
		if (_values == null)
		{
			return null;
		}
		if (_propertyDescriptors == null && _values.Length != 0)
		{
			_propertyDescriptors = CreatePropertyDescriptorCollection(DataRecordInfo.RecordType.EdmType as StructuralType, typeof(MaterializedDataRecord), isReadOnly: true);
		}
		return _propertyDescriptors;
	}

	internal static PropertyDescriptorCollection CreatePropertyDescriptorCollection(StructuralType structuralType, Type componentType, bool isReadOnly)
	{
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		if (structuralType != null)
		{
			foreach (EdmMember member in structuralType.Members)
			{
				if (member.BuiltInTypeKind == BuiltInTypeKind.EdmProperty)
				{
					EdmProperty property = (EdmProperty)member;
					FieldDescriptor item = new FieldDescriptor(componentType, isReadOnly, property);
					list.Add(item);
				}
			}
		}
		return new PropertyDescriptorCollection(list.ToArray());
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return ((ICustomTypeDescriptor)this).GetProperties((Attribute[]?)null);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		bool flag = attributes != null && attributes.Length != 0;
		PropertyDescriptorCollection propertyDescriptorCollection = InitializePropertyDescriptors();
		if (propertyDescriptorCollection == null)
		{
			return propertyDescriptorCollection;
		}
		FilterCache filterCache = _filterCache;
		if (flag && filterCache != null && filterCache.IsValid(attributes))
		{
			return filterCache.FilteredProperties;
		}
		if (!flag && propertyDescriptorCollection != null)
		{
			return propertyDescriptorCollection;
		}
		if (_attrCache == null && attributes != null && attributes.Length != 0)
		{
			_attrCache = new Dictionary<object, AttributeCollection>();
			foreach (FieldDescriptor propertyDescriptor2 in _propertyDescriptors)
			{
				object[] customAttributes = propertyDescriptor2.GetValue(this).GetType().GetCustomAttributes(inherit: false);
				Attribute[] array = new Attribute[customAttributes.Length];
				customAttributes.CopyTo(array, 0);
				_attrCache.Add(propertyDescriptor2, new AttributeCollection(array));
			}
		}
		propertyDescriptorCollection = new PropertyDescriptorCollection(null);
		foreach (PropertyDescriptor propertyDescriptor3 in _propertyDescriptors)
		{
			if (_attrCache[propertyDescriptor3].Matches(attributes))
			{
				propertyDescriptorCollection.Add(propertyDescriptor3);
			}
		}
		if (flag)
		{
			filterCache = new FilterCache();
			filterCache.Attributes = attributes;
			filterCache.FilteredProperties = propertyDescriptorCollection;
			_filterCache = filterCache;
		}
		return propertyDescriptorCollection;
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core;

internal sealed class FieldNameLookup
{
	private readonly Dictionary<string, int> _fieldNameLookup = new Dictionary<string, int>();

	private readonly string[] _fieldNames;

	public FieldNameLookup(ReadOnlyCollection<string> columnNames)
	{
		int count = columnNames.Count;
		_fieldNames = new string[count];
		for (int i = 0; i < count; i++)
		{
			_fieldNames[i] = columnNames[i];
		}
		GenerateLookup();
	}

	public FieldNameLookup(IDataRecord reader)
	{
		int fieldCount = reader.FieldCount;
		_fieldNames = new string[fieldCount];
		for (int i = 0; i < fieldCount; i++)
		{
			_fieldNames[i] = reader.GetName(i);
		}
		GenerateLookup();
	}

	public int GetOrdinal(string fieldName)
	{
		Check.NotNull(fieldName, "fieldName");
		int num = IndexOf(fieldName);
		if (num == -1)
		{
			throw new IndexOutOfRangeException(fieldName);
		}
		return num;
	}

	private int IndexOf(string fieldName)
	{
		if (!_fieldNameLookup.TryGetValue(fieldName, out var value))
		{
			value = LinearIndexOf(fieldName, CompareOptions.IgnoreCase);
			if (value == -1)
			{
				value = LinearIndexOf(fieldName, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);
			}
		}
		return value;
	}

	private int LinearIndexOf(string fieldName, CompareOptions compareOptions)
	{
		for (int i = 0; i < _fieldNames.Length; i++)
		{
			if (CultureInfo.InvariantCulture.CompareInfo.Compare(fieldName, _fieldNames[i], compareOptions) == 0)
			{
				_fieldNameLookup[fieldName] = i;
				return i;
			}
		}
		return -1;
	}

	private void GenerateLookup()
	{
		int num = _fieldNames.Length - 1;
		while (0 <= num)
		{
			_fieldNameLookup[_fieldNames[num]] = num;
			num--;
		}
	}
}

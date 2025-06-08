using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Internal;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure;

public class DbPropertyValues
{
	private readonly InternalPropertyValues _internalValues;

	public IEnumerable<string> PropertyNames => _internalValues.PropertyNames;

	public object this[string propertyName]
	{
		get
		{
			Check.NotEmpty(propertyName, "propertyName");
			object obj = _internalValues[propertyName];
			if (obj is InternalPropertyValues internalValues)
			{
				obj = new DbPropertyValues(internalValues);
			}
			return obj;
		}
		set
		{
			Check.NotEmpty(propertyName, "propertyName");
			_internalValues[propertyName] = value;
		}
	}

	internal InternalPropertyValues InternalPropertyValues => _internalValues;

	internal DbPropertyValues(InternalPropertyValues internalValues)
	{
		_internalValues = internalValues;
	}

	public object ToObject()
	{
		return _internalValues.ToObject();
	}

	public void SetValues(object obj)
	{
		Check.NotNull(obj, "obj");
		_internalValues.SetValues(obj);
	}

	public DbPropertyValues Clone()
	{
		return new DbPropertyValues(_internalValues.Clone());
	}

	public void SetValues(DbPropertyValues propertyValues)
	{
		Check.NotNull(propertyValues, "propertyValues");
		_internalValues.SetValues(propertyValues._internalValues);
	}

	public TValue GetValue<TValue>(string propertyName)
	{
		return (TValue)this[propertyName];
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq.Expressions;

namespace System.Data.Entity.Internal;

internal abstract class InternalPropertyValues
{
	private static readonly ConcurrentDictionary<Type, Func<object>> _nonEntityFactories = new ConcurrentDictionary<Type, Func<object>>();

	private readonly InternalContext _internalContext;

	private readonly Type _type;

	private readonly bool _isEntityValues;

	public abstract ISet<string> PropertyNames { get; }

	public object this[string propertyName]
	{
		get
		{
			return GetItem(propertyName).Value;
		}
		set
		{
			if (value is DbPropertyValues dbPropertyValues)
			{
				value = dbPropertyValues.InternalPropertyValues;
			}
			IPropertyValuesItem item = GetItem(propertyName);
			if (!(item.Value is InternalPropertyValues internalPropertyValues))
			{
				SetValue(item, value);
				return;
			}
			if (!(value is InternalPropertyValues values))
			{
				throw Error.DbPropertyValues_AttemptToSetNonValuesOnComplexProperty();
			}
			internalPropertyValues.SetValues(values);
		}
	}

	public Type ObjectType => _type;

	public InternalContext InternalContext => _internalContext;

	public bool IsEntityValues => _isEntityValues;

	protected InternalPropertyValues(InternalContext internalContext, Type type, bool isEntityValues)
	{
		_internalContext = internalContext;
		_type = type;
		_isEntityValues = isEntityValues;
	}

	protected abstract IPropertyValuesItem GetItemImpl(string propertyName);

	public object ToObject()
	{
		object obj = CreateObject();
		IDictionary<string, Action<object, object>> propertySetters = DbHelpers.GetPropertySetters(_type);
		foreach (string propertyName in PropertyNames)
		{
			object obj2 = GetItem(propertyName).Value;
			if (obj2 is InternalPropertyValues internalPropertyValues)
			{
				obj2 = internalPropertyValues.ToObject();
			}
			if (propertySetters.TryGetValue(propertyName, out var value))
			{
				value(obj, obj2);
			}
		}
		return obj;
	}

	private object CreateObject()
	{
		if (_isEntityValues)
		{
			return _internalContext.CreateObject(_type);
		}
		if (!_nonEntityFactories.TryGetValue(_type, out var value))
		{
			value = Expression.Lambda<Func<object>>((Expression)Expression.New(_type.GetDeclaredConstructor()), (ParameterExpression[]?)null).Compile();
			_nonEntityFactories.TryAdd(_type, value);
		}
		return value();
	}

	public void SetValues(object value)
	{
		IDictionary<string, Func<object, object>> propertyGetters = DbHelpers.GetPropertyGetters(value.GetType());
		foreach (string propertyName in PropertyNames)
		{
			if (propertyGetters.TryGetValue(propertyName, out var value2))
			{
				object obj = value2(value);
				IPropertyValuesItem item = GetItem(propertyName);
				if (obj == null && item.IsComplex)
				{
					throw Error.DbPropertyValues_ComplexObjectCannotBeNull(propertyName, _type.Name);
				}
				if (!(item.Value is InternalPropertyValues internalPropertyValues))
				{
					SetValue(item, obj);
				}
				else
				{
					internalPropertyValues.SetValues(obj);
				}
			}
		}
	}

	public InternalPropertyValues Clone()
	{
		return new ClonedPropertyValues(this);
	}

	public void SetValues(InternalPropertyValues values)
	{
		if (!_type.IsAssignableFrom(values.ObjectType))
		{
			throw Error.DbPropertyValues_AttemptToSetValuesFromWrongType(values.ObjectType.Name, _type.Name);
		}
		foreach (string propertyName in PropertyNames)
		{
			IPropertyValuesItem item = values.GetItem(propertyName);
			if (item.Value == null && item.IsComplex)
			{
				throw Error.DbPropertyValues_NestedPropertyValuesNull(propertyName, _type.Name);
			}
			this[propertyName] = item.Value;
		}
	}

	public IPropertyValuesItem GetItem(string propertyName)
	{
		if (!PropertyNames.Contains(propertyName))
		{
			throw Error.DbPropertyValues_PropertyDoesNotExist(propertyName, _type.Name);
		}
		return GetItemImpl(propertyName);
	}

	private void SetValue(IPropertyValuesItem item, object newValue)
	{
		if (!DbHelpers.PropertyValuesEqual(item.Value, newValue))
		{
			if (item.Value == null && item.IsComplex)
			{
				throw Error.DbPropertyValues_NestedPropertyValuesNull(item.Name, _type.Name);
			}
			if (newValue != null && !item.Type.IsAssignableFrom(newValue.GetType()))
			{
				throw Error.DbPropertyValues_WrongTypeForAssignment(newValue.GetType().Name, item.Name, item.Type.Name, _type.Name);
			}
			item.Value = newValue;
		}
	}
}

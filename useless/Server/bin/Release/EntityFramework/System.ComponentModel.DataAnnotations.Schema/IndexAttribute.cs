using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Utilities;
using System.Runtime.CompilerServices;

namespace System.ComponentModel.DataAnnotations.Schema;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class IndexAttribute : Attribute
{
	private string _name;

	private int _order = -1;

	private bool? _isClustered;

	private bool? _isUnique;

	public virtual string Name
	{
		get
		{
			return _name;
		}
		internal set
		{
			_name = value;
		}
	}

	public virtual int Order
	{
		get
		{
			return _order;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_order = value;
		}
	}

	public virtual bool IsClustered
	{
		get
		{
			if (_isClustered.HasValue)
			{
				return _isClustered.Value;
			}
			return false;
		}
		set
		{
			_isClustered = value;
		}
	}

	public virtual bool IsClusteredConfigured => _isClustered.HasValue;

	public virtual bool IsUnique
	{
		get
		{
			if (_isUnique.HasValue)
			{
				return _isUnique.Value;
			}
			return false;
		}
		set
		{
			_isUnique = value;
		}
	}

	public virtual bool IsUniqueConfigured => _isUnique.HasValue;

	public override object TypeId => RuntimeHelpers.GetHashCode(this);

	public IndexAttribute()
	{
	}

	public IndexAttribute(string name)
	{
		Check.NotEmpty(name, "name");
		_name = name;
	}

	public IndexAttribute(string name, int order)
	{
		Check.NotEmpty(name, "name");
		if (order < 0)
		{
			throw new ArgumentOutOfRangeException("order");
		}
		_name = name;
		_order = order;
	}

	internal IndexAttribute(string name, bool? isClustered, bool? isUnique)
	{
		_name = name;
		_isClustered = isClustered;
		_isUnique = isUnique;
	}

	internal IndexAttribute(string name, int order, bool? isClustered, bool? isUnique)
	{
		_name = name;
		_order = order;
		_isClustered = isClustered;
		_isUnique = isUnique;
	}

	protected virtual bool Equals(IndexAttribute other)
	{
		if (_name == other._name && _order == other._order && _isClustered.Equals(other._isClustered))
		{
			return _isUnique.Equals(other._isUnique);
		}
		return false;
	}

	public override string ToString()
	{
		return IndexAnnotationSerializer.SerializeIndexAttribute(this);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((IndexAttribute)obj);
	}

	public override int GetHashCode()
	{
		return (((((((base.GetHashCode() * 397) ^ ((_name != null) ? _name.GetHashCode() : 0)) * 397) ^ _order) * 397) ^ _isClustered.GetHashCode()) * 397) ^ _isUnique.GetHashCode();
	}
}

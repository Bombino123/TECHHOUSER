using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Data.Entity.ModelConfiguration.Utilities;

internal class PropertyPath : IEnumerable<PropertyInfo>, IEnumerable
{
	private static readonly PropertyPath _empty = new PropertyPath();

	private readonly List<PropertyInfo> _components = new List<PropertyInfo>();

	public int Count => _components.Count;

	public static PropertyPath Empty => _empty;

	public PropertyInfo this[int index] => _components[index];

	public PropertyPath(IEnumerable<PropertyInfo> components)
	{
		_components.AddRange(components);
	}

	public PropertyPath(PropertyInfo component)
	{
		_components.Add(component);
	}

	private PropertyPath()
	{
	}

	public override string ToString()
	{
		StringBuilder propertyPathName = new StringBuilder();
		_components.Each(delegate(PropertyInfo pi)
		{
			propertyPathName.Append(pi.Name);
			propertyPathName.Append('.');
		});
		return propertyPathName.ToString(0, propertyPathName.Length - 1);
	}

	public bool Equals(PropertyPath other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return _components.SequenceEqual(other._components, (PropertyInfo p1, PropertyInfo p2) => p1.IsSameAs(p2));
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
		if (obj.GetType() != typeof(PropertyPath))
		{
			return false;
		}
		return Equals((PropertyPath)obj);
	}

	public override int GetHashCode()
	{
		return _components.Aggregate(0, (int t, PropertyInfo n) => t ^ (n.DeclaringType.GetHashCode() * n.Name.GetHashCode() * 397));
	}

	public static bool operator ==(PropertyPath left, PropertyPath right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(PropertyPath left, PropertyPath right)
	{
		return !object.Equals(left, right);
	}

	IEnumerator<PropertyInfo> IEnumerable<PropertyInfo>.GetEnumerator()
	{
		return _components.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _components.GetEnumerator();
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;

namespace System.Data.Entity.ModelConfiguration.Utilities;

internal class EdmPropertyPath : IEnumerable<EdmProperty>, IEnumerable
{
	private static readonly EdmPropertyPath _empty = new EdmPropertyPath();

	private readonly List<EdmProperty> _components = new List<EdmProperty>();

	public static EdmPropertyPath Empty => _empty;

	public EdmPropertyPath(IEnumerable<EdmProperty> components)
	{
		_components.AddRange(components);
	}

	public EdmPropertyPath(EdmProperty component)
	{
		_components.Add(component);
	}

	private EdmPropertyPath()
	{
	}

	public override string ToString()
	{
		StringBuilder propertyPathName = new StringBuilder();
		_components.Each(delegate(EdmProperty pi)
		{
			propertyPathName.Append(pi.Name);
			propertyPathName.Append('.');
		});
		return propertyPathName.ToString(0, propertyPathName.Length - 1);
	}

	public bool Equals(EdmPropertyPath other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return _components.SequenceEqual(other._components, (EdmProperty p1, EdmProperty p2) => p1 == p2);
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
		if (obj.GetType() != typeof(EdmPropertyPath))
		{
			return false;
		}
		return Equals((EdmPropertyPath)obj);
	}

	public override int GetHashCode()
	{
		return _components.Aggregate(0, (int t, EdmProperty n) => t + n.GetHashCode());
	}

	public static bool operator ==(EdmPropertyPath left, EdmPropertyPath right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(EdmPropertyPath left, EdmPropertyPath right)
	{
		return !object.Equals(left, right);
	}

	IEnumerator<EdmProperty> IEnumerable<EdmProperty>.GetEnumerator()
	{
		return _components.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _components.GetEnumerator();
	}
}

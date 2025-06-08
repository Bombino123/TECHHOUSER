using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Utilities;

internal class TypeFinder
{
	private readonly Assembly _assembly;

	public TypeFinder(Assembly assembly)
	{
		_assembly = assembly;
	}

	public Type FindType(Type baseType, string typeName, Func<IEnumerable<Type>, IEnumerable<Type>> filter, Func<string, Exception> noType = null, Func<string, IEnumerable<Type>, Exception> multipleTypes = null, Func<string, string, Exception> noTypeWithName = null, Func<string, string, Exception> multipleTypesWithName = null)
	{
		bool flag = !string.IsNullOrWhiteSpace(typeName);
		Type type = null;
		if (flag)
		{
			type = _assembly.GetType(typeName);
		}
		if (type == null)
		{
			string name = _assembly.GetName().Name;
			IEnumerable<Type> enumerable = from t in _assembly.GetAccessibleTypes()
				where baseType.IsAssignableFrom(t)
				select t;
			if (flag)
			{
				enumerable = enumerable.Where((Type t) => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase)).ToList();
				if (enumerable.Count() > 1)
				{
					enumerable = enumerable.Where((Type t) => string.Equals(t.Name, typeName, StringComparison.Ordinal)).ToList();
				}
				if (!enumerable.Any())
				{
					if (noTypeWithName != null)
					{
						throw noTypeWithName(typeName, name);
					}
					return null;
				}
				if (enumerable.Count() > 1)
				{
					if (multipleTypesWithName != null)
					{
						throw multipleTypesWithName(typeName, name);
					}
					return null;
				}
			}
			else
			{
				enumerable = filter(enumerable);
				if (!enumerable.Any())
				{
					if (noType != null)
					{
						throw noType(name);
					}
					return null;
				}
				if (enumerable.Count() > 1)
				{
					if (multipleTypes != null)
					{
						throw multipleTypes(name, enumerable);
					}
					return null;
				}
			}
			type = enumerable.Single();
		}
		return type;
	}
}

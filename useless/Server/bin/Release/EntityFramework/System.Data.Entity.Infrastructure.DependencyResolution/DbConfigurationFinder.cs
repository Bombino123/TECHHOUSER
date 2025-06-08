using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class DbConfigurationFinder
{
	public virtual Type TryFindConfigurationType(Type contextType, IEnumerable<Type> typesToSearch = null)
	{
		return TryFindConfigurationType(contextType.Assembly(), contextType, typesToSearch);
	}

	public virtual Type TryFindConfigurationType(Assembly assemblyHint, Type contextTypeHint, IEnumerable<Type> typesToSearch = null)
	{
		if (contextTypeHint != null)
		{
			Type type = (from a in contextTypeHint.GetCustomAttributes<DbConfigurationTypeAttribute>(inherit: true)
				select a.ConfigurationType).FirstOrDefault();
			if (type != null)
			{
				if (!typeof(DbConfiguration).IsAssignableFrom(type))
				{
					throw new InvalidOperationException(Strings.CreateInstance_BadDbConfigurationType(type.ToString(), typeof(DbConfiguration).ToString()));
				}
				return type;
			}
		}
		List<Type> list = (typesToSearch ?? assemblyHint.GetAccessibleTypes()).Where((Type t) => t.IsSubclassOf(typeof(DbConfiguration)) && !t.IsAbstract() && !t.IsGenericType()).ToList();
		if (list.Count > 1)
		{
			throw new InvalidOperationException(Strings.MultipleConfigsInAssembly(list.First().Assembly(), typeof(DbConfiguration).Name));
		}
		return list.FirstOrDefault();
	}

	public virtual Type TryFindContextType(Assembly assemblyHint, Type contextTypeHint, IEnumerable<Type> typesToSearch = null)
	{
		if (contextTypeHint != null)
		{
			return contextTypeHint;
		}
		List<Type> list = (typesToSearch ?? assemblyHint.GetAccessibleTypes()).Where((Type t) => t.IsSubclassOf(typeof(DbContext)) && !t.IsAbstract() && !t.IsGenericType() && t.GetCustomAttributes<DbConfigurationTypeAttribute>(inherit: true).Any()).ToList();
		if (list.Count != 1)
		{
			return null;
		}
		return list[0];
	}
}

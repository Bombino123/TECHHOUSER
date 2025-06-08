using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Utilities;

internal static class AssemblyExtensions
{
	public static string GetInformationalVersion(this Assembly assembly)
	{
		return assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;
	}

	public static IEnumerable<Type> GetAccessibleTypes(this Assembly assembly)
	{
		try
		{
			return assembly.DefinedTypes.Select((TypeInfo t) => t.AsType());
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where((Type t) => t != null);
		}
	}
}

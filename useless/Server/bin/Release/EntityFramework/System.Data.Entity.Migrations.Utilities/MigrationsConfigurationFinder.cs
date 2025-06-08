using System.Collections.Generic;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace System.Data.Entity.Migrations.Utilities;

internal class MigrationsConfigurationFinder
{
	private readonly TypeFinder _typeFinder;

	public MigrationsConfigurationFinder()
	{
	}

	public MigrationsConfigurationFinder(TypeFinder typeFinder)
	{
		_typeFinder = typeFinder;
	}

	public virtual DbMigrationsConfiguration FindMigrationsConfiguration(Type contextType, string configurationTypeName, Func<string, Exception> noType = null, Func<string, IEnumerable<Type>, Exception> multipleTypes = null, Func<string, string, Exception> noTypeWithName = null, Func<string, string, Exception> multipleTypesWithName = null)
	{
		Type type = _typeFinder.FindType((contextType == null) ? typeof(DbMigrationsConfiguration) : typeof(DbMigrationsConfiguration<>).MakeGenericType(contextType), configurationTypeName, (IEnumerable<Type> types) => types.Where((Type t) => t.GetPublicConstructor() != null && !t.IsAbstract() && !t.IsGenericType()).ToList(), noType, multipleTypes, noTypeWithName, multipleTypesWithName);
		try
		{
			return (type == null) ? null : type.CreateInstance<DbMigrationsConfiguration>(Strings.CreateInstance_BadMigrationsConfigurationType, (string s) => new MigrationsException(s));
		}
		catch (TargetInvocationException ex)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			throw ex.InnerException;
		}
	}
}

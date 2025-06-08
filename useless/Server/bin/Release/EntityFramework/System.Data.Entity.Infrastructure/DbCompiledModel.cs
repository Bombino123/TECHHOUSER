using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Infrastructure;

public class DbCompiledModel
{
	private static readonly ConcurrentDictionary<Type, Func<EntityConnection, ObjectContext>> _contextConstructors = new ConcurrentDictionary<Type, Func<EntityConnection, ObjectContext>>();

	private static readonly Func<EntityConnection, ObjectContext> _objectContextConstructor = (EntityConnection c) => new ObjectContext(c);

	private readonly ICachedMetadataWorkspace _workspace;

	private readonly DbModelBuilder _cachedModelBuilder;

	private readonly string _defaultSchema;

	internal virtual DbModelBuilder CachedModelBuilder => _cachedModelBuilder;

	internal virtual DbProviderInfo ProviderInfo => _workspace.ProviderInfo;

	internal string DefaultSchema => _defaultSchema;

	internal DbCompiledModel()
	{
	}

	internal DbCompiledModel(CodeFirstCachedMetadataWorkspace workspace, DbModelBuilder cachedModelBuilder)
	{
		_workspace = workspace;
		_cachedModelBuilder = cachedModelBuilder;
		_defaultSchema = cachedModelBuilder.ModelConfiguration.DefaultSchema;
	}

	internal DbCompiledModel(CodeFirstCachedMetadataWorkspace workspace, string defaultSchema)
	{
		_workspace = workspace;
		_defaultSchema = defaultSchema;
	}

	public TContext CreateObjectContext<TContext>(DbConnection existingConnection) where TContext : ObjectContext
	{
		Check.NotNull(existingConnection, "existingConnection");
		EntityConnection arg = new EntityConnection(_workspace.GetMetadataWorkspace(existingConnection), existingConnection);
		TContext val = (TContext)GetConstructorDelegate<TContext>()(arg);
		val.ContextOwnsConnection = true;
		if (string.IsNullOrEmpty(val.DefaultContainerName))
		{
			val.DefaultContainerName = _workspace.DefaultContainerName;
		}
		foreach (Assembly assembly in _workspace.Assemblies)
		{
			val.MetadataWorkspace.LoadFromAssembly(assembly);
		}
		return val;
	}

	internal static Func<EntityConnection, ObjectContext> GetConstructorDelegate<TContext>() where TContext : ObjectContext
	{
		if (typeof(TContext) == typeof(ObjectContext))
		{
			return _objectContextConstructor;
		}
		if (!_contextConstructors.TryGetValue(typeof(TContext), out var value))
		{
			ConstructorInfo declaredConstructor = typeof(TContext).GetDeclaredConstructor((ConstructorInfo c) => c.IsPublic, new Type[1] { typeof(EntityConnection) }, new Type[1] { typeof(DbConnection) }, new Type[1] { typeof(IDbConnection) }, new Type[1] { typeof(IDisposable) }, new Type[1] { typeof(Component) }, new Type[1] { typeof(MarshalByRefObject) }, new Type[1] { typeof(object) });
			if (declaredConstructor == null)
			{
				throw Error.DbModelBuilder_MissingRequiredCtor(typeof(TContext).Name);
			}
			ParameterExpression parameterExpression = Expression.Parameter(typeof(EntityConnection), "connection");
			value = Expression.Lambda<Func<EntityConnection, ObjectContext>>(Expression.New(declaredConstructor, parameterExpression), new ParameterExpression[1] { parameterExpression }).Compile();
			_contextConstructors.TryAdd(typeof(TContext), value);
		}
		return value;
	}
}

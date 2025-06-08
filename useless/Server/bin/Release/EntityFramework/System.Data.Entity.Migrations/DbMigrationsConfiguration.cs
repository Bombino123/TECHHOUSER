using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.IO;
using System.Reflection;

namespace System.Data.Entity.Migrations;

public class DbMigrationsConfiguration
{
	public const string DefaultMigrationsDirectory = "Migrations";

	private readonly Dictionary<string, MigrationSqlGenerator> _sqlGenerators = new Dictionary<string, MigrationSqlGenerator>();

	private readonly Dictionary<string, Func<DbConnection, string, HistoryContext>> _historyContextFactories = new Dictionary<string, Func<DbConnection, string, HistoryContext>>();

	private MigrationCodeGenerator _codeGenerator;

	private Type _contextType;

	private Assembly _migrationsAssembly;

	private EdmModelDiffer _modelDiffer = new EdmModelDiffer();

	private DbConnectionInfo _connectionInfo;

	private string _migrationsDirectory = "Migrations";

	private readonly Lazy<IDbDependencyResolver> _resolver;

	private string _contextKey;

	private int? _commandTimeout;

	public bool AutomaticMigrationsEnabled { get; set; }

	public string ContextKey
	{
		get
		{
			return _contextKey;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_contextKey = value;
		}
	}

	public bool AutomaticMigrationDataLossAllowed { get; set; }

	public Type ContextType
	{
		get
		{
			return _contextType;
		}
		set
		{
			Check.NotNull(value, "value");
			if (!typeof(DbContext).IsAssignableFrom(value))
			{
				throw new ArgumentException(Strings.DbMigrationsConfiguration_ContextType(value.Name));
			}
			_contextType = value;
			DbConfigurationManager.Instance.EnsureLoadedForContext(_contextType);
		}
	}

	public string MigrationsNamespace { get; set; }

	public string MigrationsDirectory
	{
		get
		{
			return _migrationsDirectory;
		}
		set
		{
			Check.NotEmpty(value, "value");
			if (Path.IsPathRooted(value))
			{
				throw new MigrationsException(Strings.DbMigrationsConfiguration_RootedPath(value));
			}
			_migrationsDirectory = value;
		}
	}

	public MigrationCodeGenerator CodeGenerator
	{
		get
		{
			return _codeGenerator;
		}
		set
		{
			Check.NotNull(value, "value");
			_codeGenerator = value;
		}
	}

	public Assembly MigrationsAssembly
	{
		get
		{
			return _migrationsAssembly;
		}
		set
		{
			Check.NotNull(value, "value");
			_migrationsAssembly = value;
		}
	}

	public DbConnectionInfo TargetDatabase
	{
		get
		{
			return _connectionInfo;
		}
		set
		{
			Check.NotNull(value, "value");
			_connectionInfo = value;
		}
	}

	public int? CommandTimeout
	{
		get
		{
			return _commandTimeout;
		}
		set
		{
			if (value.HasValue && value < 0)
			{
				throw new ArgumentException(Strings.ObjectContext_InvalidCommandTimeout);
			}
			_commandTimeout = value;
		}
	}

	internal EdmModelDiffer ModelDiffer
	{
		get
		{
			return _modelDiffer;
		}
		set
		{
			_modelDiffer = value;
		}
	}

	public DbMigrationsConfiguration()
		: this(new Lazy<IDbDependencyResolver>(() => DbConfiguration.DependencyResolver))
	{
		CodeGenerator = new CSharpMigrationCodeGenerator();
		ContextKey = GetType().ToString();
	}

	internal DbMigrationsConfiguration(Lazy<IDbDependencyResolver> resolver)
	{
		_resolver = resolver;
	}

	public void SetSqlGenerator(string providerInvariantName, MigrationSqlGenerator migrationSqlGenerator)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(migrationSqlGenerator, "migrationSqlGenerator");
		_sqlGenerators[providerInvariantName] = migrationSqlGenerator;
	}

	public MigrationSqlGenerator GetSqlGenerator(string providerInvariantName)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		if (!_sqlGenerators.TryGetValue(providerInvariantName, out var value))
		{
			return (_resolver.Value.GetService<Func<MigrationSqlGenerator>>(providerInvariantName) ?? throw Error.NoSqlGeneratorForProvider(providerInvariantName))();
		}
		return value;
	}

	public void SetHistoryContextFactory(string providerInvariantName, Func<DbConnection, string, HistoryContext> factory)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(factory, "factory");
		_historyContextFactories[providerInvariantName] = factory;
	}

	public Func<DbConnection, string, HistoryContext> GetHistoryContextFactory(string providerInvariantName)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		if (!_historyContextFactories.TryGetValue(providerInvariantName, out var value))
		{
			return _resolver.Value.GetService<Func<DbConnection, string, HistoryContext>>(providerInvariantName) ?? _resolver.Value.GetService<Func<DbConnection, string, HistoryContext>>();
		}
		return value;
	}

	internal virtual void OnSeed(DbContext context)
	{
	}
}
public class DbMigrationsConfiguration<TContext> : DbMigrationsConfiguration where TContext : DbContext
{
	static DbMigrationsConfiguration()
	{
		DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
	}

	public DbMigrationsConfiguration()
	{
		base.ContextType = typeof(TContext);
		base.MigrationsAssembly = GetType().Assembly();
		base.MigrationsNamespace = GetType().Namespace;
	}

	protected virtual void Seed(TContext context)
	{
		Check.NotNull(context, "context");
	}

	internal override void OnSeed(DbContext context)
	{
		Seed((TContext)context);
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected new object MemberwiseClone()
	{
		return base.MemberwiseClone();
	}
}

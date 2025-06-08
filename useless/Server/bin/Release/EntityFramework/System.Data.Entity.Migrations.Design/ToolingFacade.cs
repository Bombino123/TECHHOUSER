using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;

namespace System.Data.Entity.Migrations.Design;

[Obsolete("Use System.Data.Entity.Infrastructure.Design.Executor instead.")]
public class ToolingFacade : IDisposable
{
	private class ToolLogger : MigrationsLogger
	{
		private readonly ToolingFacade _facade;

		public ToolLogger(ToolingFacade facade)
		{
			_facade = facade;
		}

		public override void Info(string message)
		{
			if (_facade.LogInfoDelegate != null)
			{
				_facade.LogInfoDelegate(message);
			}
		}

		public override void Warning(string message)
		{
			if (_facade.LogWarningDelegate != null)
			{
				_facade.LogWarningDelegate(message);
			}
		}

		public override void Verbose(string sql)
		{
			if (_facade.LogVerboseDelegate != null)
			{
				_facade.LogVerboseDelegate(sql);
			}
		}
	}

	[Serializable]
	private abstract class BaseRunner
	{
		public string MigrationsAssemblyName { get; set; }

		public string ContextAssemblyName { get; set; }

		public string ConfigurationTypeName { get; set; }

		public DbConnectionInfo ConnectionStringInfo { get; set; }

		public ToolLogger Log { get; set; }

		public void Run()
		{
			try
			{
				RunCore();
			}
			catch (Exception ex)
			{
				AppDomain.CurrentDomain.SetData("error", ex.Message);
				AppDomain.CurrentDomain.SetData("typeName", ex.GetType().FullName);
				AppDomain.CurrentDomain.SetData("stackTrace", ex.ToString());
			}
		}

		protected abstract void RunCore();

		protected MigratorBase GetMigrator()
		{
			return DecorateMigrator(new DbMigrator(GetConfiguration()));
		}

		protected DbMigrationsConfiguration GetConfiguration()
		{
			DbMigrationsConfiguration dbMigrationsConfiguration = FindConfiguration();
			OverrideConfiguration(dbMigrationsConfiguration);
			return dbMigrationsConfiguration;
		}

		protected virtual void OverrideConfiguration(DbMigrationsConfiguration configuration)
		{
			if (ConnectionStringInfo != null)
			{
				configuration.TargetDatabase = ConnectionStringInfo;
			}
		}

		private MigratorBase DecorateMigrator(DbMigrator migrator)
		{
			return new MigratorLoggingDecorator(migrator, Log);
		}

		private DbMigrationsConfiguration FindConfiguration()
		{
			return new MigrationsConfigurationFinder(new TypeFinder(LoadMigrationsAssembly())).FindMigrationsConfiguration(null, ConfigurationTypeName, Error.AssemblyMigrator_NoConfiguration, (string assembly, IEnumerable<Type> types) => Error.AssemblyMigrator_MultipleConfigurations(assembly), Error.AssemblyMigrator_NoConfigurationWithName, Error.AssemblyMigrator_MultipleConfigurationsWithName);
		}

		protected Assembly LoadMigrationsAssembly()
		{
			return LoadAssembly(MigrationsAssemblyName);
		}

		protected Assembly LoadContextAssembly()
		{
			return LoadAssembly(ContextAssemblyName);
		}

		private static Assembly LoadAssembly(string name)
		{
			try
			{
				return Assembly.Load(name);
			}
			catch (FileNotFoundException ex)
			{
				throw new MigrationsException(Strings.ToolingFacade_AssemblyNotFound(ex.FileName), ex);
			}
		}
	}

	[Serializable]
	private class GetDatabaseMigrationsRunner : BaseRunner
	{
		protected override void RunCore()
		{
			IEnumerable<string> databaseMigrations = GetMigrator().GetDatabaseMigrations();
			AppDomain.CurrentDomain.SetData("result", databaseMigrations);
		}
	}

	[Serializable]
	private class GetPendingMigrationsRunner : BaseRunner
	{
		protected override void RunCore()
		{
			IEnumerable<string> pendingMigrations = GetMigrator().GetPendingMigrations();
			AppDomain.CurrentDomain.SetData("result", pendingMigrations);
		}
	}

	[Serializable]
	private class UpdateRunner : BaseRunner
	{
		public string TargetMigration { get; set; }

		public bool Force { get; set; }

		protected override void RunCore()
		{
			GetMigrator().Update(TargetMigration);
		}

		protected override void OverrideConfiguration(DbMigrationsConfiguration configuration)
		{
			base.OverrideConfiguration(configuration);
			if (Force)
			{
				configuration.AutomaticMigrationDataLossAllowed = true;
			}
		}
	}

	[Serializable]
	private class ScriptUpdateRunner : BaseRunner
	{
		public string SourceMigration { get; set; }

		public string TargetMigration { get; set; }

		public bool Force { get; set; }

		protected override void RunCore()
		{
			string data = new MigratorScriptingDecorator(GetMigrator()).ScriptUpdate(SourceMigration, TargetMigration);
			AppDomain.CurrentDomain.SetData("result", data);
		}

		protected override void OverrideConfiguration(DbMigrationsConfiguration configuration)
		{
			base.OverrideConfiguration(configuration);
			if (Force)
			{
				configuration.AutomaticMigrationDataLossAllowed = true;
			}
		}
	}

	[Serializable]
	private class ScaffoldRunner : BaseRunner
	{
		public string MigrationName { get; set; }

		public string Language { get; set; }

		public string RootNamespace { get; set; }

		public bool IgnoreChanges { get; set; }

		protected override void RunCore()
		{
			DbMigrationsConfiguration configuration = GetConfiguration();
			MigrationScaffolder migrationScaffolder = new MigrationScaffolder(configuration);
			string text = configuration.MigrationsNamespace;
			if (Language == "vb" && !string.IsNullOrWhiteSpace(RootNamespace))
			{
				if (RootNamespace.EqualsIgnoreCase(text))
				{
					text = null;
				}
				else
				{
					if (text == null || !text.StartsWith(RootNamespace + ".", StringComparison.OrdinalIgnoreCase))
					{
						throw Error.MigrationsNamespaceNotUnderRootNamespace(text, RootNamespace);
					}
					text = text.Substring(RootNamespace.Length + 1);
				}
			}
			migrationScaffolder.Namespace = text;
			ScaffoldedMigration data = Scaffold(migrationScaffolder);
			AppDomain.CurrentDomain.SetData("result", data);
		}

		protected virtual ScaffoldedMigration Scaffold(MigrationScaffolder scaffolder)
		{
			return scaffolder.Scaffold(MigrationName, IgnoreChanges);
		}

		protected override void OverrideConfiguration(DbMigrationsConfiguration configuration)
		{
			base.OverrideConfiguration(configuration);
			if (Language == "vb" && configuration.CodeGenerator is CSharpMigrationCodeGenerator)
			{
				configuration.CodeGenerator = new VisualBasicMigrationCodeGenerator();
			}
		}
	}

	[Serializable]
	private class InitialCreateScaffoldRunner : ScaffoldRunner
	{
		protected override ScaffoldedMigration Scaffold(MigrationScaffolder scaffolder)
		{
			return scaffolder.ScaffoldInitialCreate();
		}
	}

	[Serializable]
	private class GetContextTypesRunner : BaseRunner
	{
		protected override void RunCore()
		{
			List<string> data = (from t in LoadContextAssembly().GetAccessibleTypes()
				where !t.IsAbstract && !t.IsGenericType && typeof(DbContext).IsAssignableFrom(t)
				select t.FullName).ToList();
			AppDomain.CurrentDomain.SetData("result", data);
		}
	}

	[Serializable]
	private class GetContextTypeRunner : BaseRunner
	{
		public string ContextTypeName { get; set; }

		protected override void RunCore()
		{
			Type type = new TypeFinder(LoadContextAssembly()).FindType(typeof(DbContext), ContextTypeName, (IEnumerable<Type> types) => types.Where((Type t) => !typeof(HistoryContext).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType), Error.EnableMigrations_NoContext, delegate(string assembly, IEnumerable<Type> types)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(Strings.EnableMigrations_MultipleContexts(assembly));
				foreach (Type type2 in types)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(Strings.EnableMigrationsForContext(type2.FullName));
				}
				return new MigrationsException(stringBuilder.ToString());
			}, Error.EnableMigrations_NoContextWithName, Error.EnableMigrations_MultipleContextsWithName);
			AppDomain.CurrentDomain.SetData("result", type.FullName);
		}
	}

	private readonly string _migrationsAssemblyName;

	private readonly string _contextAssemblyName;

	private readonly string _configurationTypeName;

	private readonly string _configurationFile;

	private readonly DbConnectionInfo _connectionStringInfo;

	private AppDomain _appDomain;

	public Action<string> LogInfoDelegate { get; set; }

	public Action<string> LogWarningDelegate { get; set; }

	public Action<string> LogVerboseDelegate { get; set; }

	public ToolingFacade(string migrationsAssemblyName, string contextAssemblyName, string configurationTypeName, string workingDirectory, string configurationFilePath, string dataDirectory, DbConnectionInfo connectionStringInfo)
	{
		Check.NotEmpty(migrationsAssemblyName, "migrationsAssemblyName");
		_migrationsAssemblyName = migrationsAssemblyName;
		_contextAssemblyName = contextAssemblyName;
		_configurationTypeName = configurationTypeName;
		_connectionStringInfo = connectionStringInfo;
		AppDomainSetup appDomainSetup = new AppDomainSetup
		{
			ShadowCopyFiles = "true"
		};
		if (!string.IsNullOrWhiteSpace(workingDirectory))
		{
			appDomainSetup.ApplicationBase = workingDirectory;
		}
		_configurationFile = new ConfigurationFileUpdater().Update(configurationFilePath);
		appDomainSetup.ConfigurationFile = _configurationFile;
		string text = "MigrationsToolingFacade" + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
		_appDomain = AppDomain.CreateDomain(text, (Evidence)null, appDomainSetup);
		if (!string.IsNullOrWhiteSpace(dataDirectory))
		{
			_appDomain.SetData("DataDirectory", dataDirectory);
		}
	}

	internal ToolingFacade()
	{
	}

	~ToolingFacade()
	{
		Dispose(disposing: false);
	}

	public IEnumerable<string> GetContextTypes()
	{
		GetContextTypesRunner runner = new GetContextTypesRunner();
		ConfigureRunner(runner);
		Run(runner);
		return (IEnumerable<string>)_appDomain.GetData("result");
	}

	public string GetContextType(string contextTypeName)
	{
		GetContextTypeRunner runner = new GetContextTypeRunner
		{
			ContextTypeName = contextTypeName
		};
		ConfigureRunner(runner);
		Run(runner);
		return (string)_appDomain.GetData("result");
	}

	public virtual IEnumerable<string> GetDatabaseMigrations()
	{
		GetDatabaseMigrationsRunner runner = new GetDatabaseMigrationsRunner();
		ConfigureRunner(runner);
		Run(runner);
		return (IEnumerable<string>)_appDomain.GetData("result");
	}

	public virtual IEnumerable<string> GetPendingMigrations()
	{
		GetPendingMigrationsRunner runner = new GetPendingMigrationsRunner();
		ConfigureRunner(runner);
		Run(runner);
		return (IEnumerable<string>)_appDomain.GetData("result");
	}

	public void Update(string targetMigration, bool force)
	{
		UpdateRunner runner = new UpdateRunner
		{
			TargetMigration = targetMigration,
			Force = force
		};
		ConfigureRunner(runner);
		Run(runner);
	}

	public string ScriptUpdate(string sourceMigration, string targetMigration, bool force)
	{
		ScriptUpdateRunner runner = new ScriptUpdateRunner
		{
			SourceMigration = sourceMigration,
			TargetMigration = targetMigration,
			Force = force
		};
		ConfigureRunner(runner);
		Run(runner);
		return (string)_appDomain.GetData("result");
	}

	public virtual ScaffoldedMigration Scaffold(string migrationName, string language, string rootNamespace, bool ignoreChanges)
	{
		ScaffoldRunner runner = new ScaffoldRunner
		{
			MigrationName = migrationName,
			Language = language,
			RootNamespace = rootNamespace,
			IgnoreChanges = ignoreChanges
		};
		ConfigureRunner(runner);
		Run(runner);
		return (ScaffoldedMigration)_appDomain.GetData("result");
	}

	public ScaffoldedMigration ScaffoldInitialCreate(string language, string rootNamespace)
	{
		InitialCreateScaffoldRunner runner = new InitialCreateScaffoldRunner
		{
			Language = language,
			RootNamespace = rootNamespace
		};
		ConfigureRunner(runner);
		Run(runner);
		return (ScaffoldedMigration)_appDomain.GetData("result");
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && _appDomain != null)
		{
			AppDomain.Unload(_appDomain);
			_appDomain = null;
		}
		if (_configurationFile != null)
		{
			File.Delete(_configurationFile);
		}
	}

	private void ConfigureRunner(BaseRunner runner)
	{
		runner.MigrationsAssemblyName = _migrationsAssemblyName;
		runner.ContextAssemblyName = _contextAssemblyName;
		runner.ConfigurationTypeName = _configurationTypeName;
		runner.ConnectionStringInfo = _connectionStringInfo;
		runner.Log = new ToolLogger(this);
	}

	private void Run(BaseRunner runner)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		_appDomain.SetData("error", null);
		_appDomain.SetData("typeName", null);
		_appDomain.SetData("stackTrace", null);
		_appDomain.DoCallBack(new CrossAppDomainDelegate(runner.Run));
		string text = (string)_appDomain.GetData("error");
		if (text != null)
		{
			string innerType = (string)_appDomain.GetData("typeName");
			string innerStackTrace = (string)_appDomain.GetData("stackTrace");
			throw new ToolingException(text, innerType, innerStackTrace);
		}
	}
}

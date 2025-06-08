using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Data.Entity.Infrastructure.Design;

public class Executor : MarshalByRefObject
{
	public class GetContextType : OperationBase
	{
		public GetContextType(Executor executor, object resultHandler, IDictionary args)
			: base(resultHandler)
		{
			Check.NotNull(executor, "executor");
			Check.NotNull(resultHandler, "resultHandler");
			Check.NotNull(args, "args");
			string contextTypeName = (string)args["contextTypeName"];
			string contextAssemblyName = (string)args["contextAssemblyName"];
			Execute(() => executor.GetContextTypeInternal(contextTypeName, contextAssemblyName));
		}
	}

	internal class GetProviderServices : OperationBase
	{
		public GetProviderServices(Executor executor, object handler, string invariantName, IDictionary<string, object> anonymousArguments)
			: base(handler)
		{
			Check.NotNull(executor, "executor");
			Check.NotEmpty(invariantName, "invariantName");
			Execute(() => executor.GetProviderServicesInternal(invariantName));
		}
	}

	public class ScaffoldInitialCreate : OperationBase
	{
		public ScaffoldInitialCreate(Executor executor, object resultHandler, IDictionary args)
			: base(resultHandler)
		{
			Check.NotNull(executor, "executor");
			Check.NotNull(resultHandler, "resultHandler");
			Check.NotNull(args, "args");
			string connectionStringName = (string)args["connectionStringName"];
			string connectionString = (string)args["connectionString"];
			string connectionProviderName = (string)args["connectionProviderName"];
			string contextTypeName = (string)args["contextTypeName"];
			string contextAssemblyName = (string)args["contextAssemblyName"];
			string migrationsNamespace = (string)args["migrationsNamespace"];
			bool auto = (bool)args["auto"];
			string migrationsDir = (string)args["migrationsDir"];
			Execute(() => executor.ScaffoldInitialCreateInternal(OperationBase.CreateConnectionInfo(connectionStringName, connectionString, connectionProviderName), contextTypeName, contextAssemblyName, migrationsNamespace, auto, migrationsDir));
		}
	}

	public class Scaffold : OperationBase
	{
		public Scaffold(Executor executor, object resultHandler, IDictionary args)
			: base(resultHandler)
		{
			Check.NotNull(executor, "executor");
			Check.NotNull(resultHandler, "resultHandler");
			Check.NotNull(args, "args");
			string name = (string)args["name"];
			string connectionStringName = (string)args["connectionStringName"];
			string connectionString = (string)args["connectionString"];
			string connectionProviderName = (string)args["connectionProviderName"];
			string migrationsConfigurationName = (string)args["migrationsConfigurationName"];
			bool ignoreChanges = (bool)args["ignoreChanges"];
			Execute(() => executor.ScaffoldInternal(name, OperationBase.CreateConnectionInfo(connectionStringName, connectionString, connectionProviderName), migrationsConfigurationName, ignoreChanges));
		}
	}

	public class GetDatabaseMigrations : OperationBase
	{
		public GetDatabaseMigrations(Executor executor, object resultHandler, IDictionary args)
			: base(resultHandler)
		{
			Check.NotNull(executor, "executor");
			Check.NotNull(resultHandler, "resultHandler");
			Check.NotNull(args, "args");
			string connectionStringName = (string)args["connectionStringName"];
			string connectionString = (string)args["connectionString"];
			string connectionProviderName = (string)args["connectionProviderName"];
			string migrationsConfigurationName = (string)args["migrationsConfigurationName"];
			Execute(() => executor.GetDatabaseMigrationsInternal(OperationBase.CreateConnectionInfo(connectionStringName, connectionString, connectionProviderName), migrationsConfigurationName));
		}
	}

	public class ScriptUpdate : OperationBase
	{
		public ScriptUpdate(Executor executor, object resultHandler, IDictionary args)
			: base(resultHandler)
		{
			Check.NotNull(executor, "executor");
			Check.NotNull(resultHandler, "resultHandler");
			Check.NotNull(args, "args");
			string sourceMigration = (string)args["sourceMigration"];
			string targetMigration = (string)args["targetMigration"];
			bool force = (bool)args["force"];
			string connectionStringName = (string)args["connectionStringName"];
			string connectionString = (string)args["connectionString"];
			string connectionProviderName = (string)args["connectionProviderName"];
			string migrationsConfigurationName = (string)args["migrationsConfigurationName"];
			Execute(() => executor.ScriptUpdateInternal(sourceMigration, targetMigration, force, OperationBase.CreateConnectionInfo(connectionStringName, connectionString, connectionProviderName), migrationsConfigurationName));
		}
	}

	public class Update : OperationBase
	{
		public Update(Executor executor, object resultHandler, IDictionary args)
			: base(resultHandler)
		{
			Check.NotNull(executor, "executor");
			Check.NotNull(resultHandler, "resultHandler");
			Check.NotNull(args, "args");
			string targetMigration = (string)args["targetMigration"];
			bool force = (bool)args["force"];
			string connectionStringName = (string)args["connectionStringName"];
			string connectionString = (string)args["connectionString"];
			string connectionProviderName = (string)args["connectionProviderName"];
			string migrationsConfigurationName = (string)args["migrationsConfigurationName"];
			Execute(delegate
			{
				executor.UpdateInternal(targetMigration, force, OperationBase.CreateConnectionInfo(connectionStringName, connectionString, connectionProviderName), migrationsConfigurationName);
			});
		}
	}

	public abstract class OperationBase : MarshalByRefObject
	{
		private readonly WrappedResultHandler _handler;

		protected OperationBase(object handler)
		{
			Check.NotNull(handler, "handler");
			_handler = new WrappedResultHandler(handler);
		}

		protected static DbConnectionInfo CreateConnectionInfo(string connectionStringName, string connectionString, string connectionProviderName)
		{
			if (!string.IsNullOrWhiteSpace(connectionStringName))
			{
				return new DbConnectionInfo(connectionStringName);
			}
			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				return new DbConnectionInfo(connectionString, connectionProviderName);
			}
			return null;
		}

		protected virtual void Execute(Action action)
		{
			Check.NotNull(action, "action");
			try
			{
				action();
			}
			catch (Exception ex)
			{
				if (!_handler.SetError(ex.GetType().FullName, ex.Message, ex.ToString()))
				{
					throw;
				}
			}
		}

		protected virtual void Execute<T>(Func<T> action)
		{
			Check.NotNull(action, "action");
			Execute(delegate
			{
				_handler.SetResult(action());
			});
		}

		protected virtual void Execute<T>(Func<IEnumerable<T>> action)
		{
			Check.NotNull(action, "action");
			Execute(delegate
			{
				_handler.SetResult(action().ToArray());
			});
		}
	}

	private class ToolLogger : MigrationsLogger
	{
		private readonly Reporter _reporter;

		public ToolLogger(Reporter reporter)
		{
			_reporter = reporter;
		}

		public override void Info(string message)
		{
			_reporter.WriteInformation(message);
		}

		public override void Warning(string message)
		{
			_reporter.WriteWarning(message);
		}

		public override void Verbose(string sql)
		{
			_reporter.WriteVerbose(sql);
		}
	}

	private readonly Assembly _assembly;

	private readonly Reporter _reporter;

	private readonly string _language;

	private readonly string _rootNamespace;

	public Executor(string assemblyFile, IDictionary<string, object> anonymousArguments)
	{
		Check.NotEmpty(assemblyFile, "assemblyFile");
		_reporter = new Reporter(new WrappedReportHandler(anonymousArguments?["reportHandler"]));
		_language = (string)anonymousArguments?["language"];
		_rootNamespace = (string)anonymousArguments?["rootNamespace"];
		_assembly = Assembly.Load(AssemblyName.GetAssemblyName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyFile)));
	}

	private Assembly LoadAssembly(string assemblyName)
	{
		if (string.IsNullOrEmpty(assemblyName))
		{
			return null;
		}
		try
		{
			return Assembly.Load(assemblyName);
		}
		catch (FileNotFoundException ex)
		{
			throw new MigrationsException(Strings.ToolingFacade_AssemblyNotFound(ex.FileName), ex);
		}
	}

	private string GetContextTypeInternal(string contextTypeName, string contextAssemblyName)
	{
		return new TypeFinder(LoadAssembly(contextAssemblyName) ?? _assembly).FindType(typeof(DbContext), contextTypeName, (IEnumerable<Type> types) => types.Where((Type t) => !typeof(HistoryContext).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType), Error.EnableMigrations_NoContext, delegate(string assembly, IEnumerable<Type> types)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Strings.EnableMigrations_MultipleContexts(assembly));
			foreach (Type type in types)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(Strings.EnableMigrationsForContext(type.FullName));
			}
			return new MigrationsException(stringBuilder.ToString());
		}, Error.EnableMigrations_NoContextWithName, Error.EnableMigrations_MultipleContextsWithName).FullName;
	}

	internal virtual string GetProviderServicesInternal(string invariantName)
	{
		DbConfiguration.LoadConfiguration(_assembly);
		IDbDependencyResolver dependencyResolver = DbConfiguration.DependencyResolver;
		DbProviderServices dbProviderServices = null;
		try
		{
			dbProviderServices = dependencyResolver.GetService<DbProviderServices>(invariantName);
		}
		catch
		{
		}
		return dbProviderServices?.GetType().AssemblyQualifiedName;
	}

	private void OverrideConfiguration(DbMigrationsConfiguration configuration, DbConnectionInfo connectionInfo, bool force = false)
	{
		if (connectionInfo != null)
		{
			configuration.TargetDatabase = connectionInfo;
		}
		if (string.Equals(_language, "VB", StringComparison.OrdinalIgnoreCase) && configuration.CodeGenerator is CSharpMigrationCodeGenerator)
		{
			configuration.CodeGenerator = new VisualBasicMigrationCodeGenerator();
		}
		if (force)
		{
			configuration.AutomaticMigrationDataLossAllowed = true;
		}
	}

	private MigrationScaffolder CreateMigrationScaffolder(DbMigrationsConfiguration configuration)
	{
		MigrationScaffolder migrationScaffolder = new MigrationScaffolder(configuration);
		string text = configuration.MigrationsNamespace;
		if (string.Equals(_language, "VB", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_rootNamespace))
		{
			if (_rootNamespace.EqualsIgnoreCase(text))
			{
				text = null;
			}
			else
			{
				if (text == null || !text.StartsWith(_rootNamespace + ".", StringComparison.OrdinalIgnoreCase))
				{
					throw Error.MigrationsNamespaceNotUnderRootNamespace(text, _rootNamespace);
				}
				text = text.Substring(_rootNamespace.Length + 1);
			}
		}
		migrationScaffolder.Namespace = text;
		return migrationScaffolder;
	}

	private static IDictionary ToHashtable(ScaffoldedMigration result)
	{
		if (result != null)
		{
			return new Hashtable
			{
				["MigrationId"] = result.MigrationId,
				["UserCode"] = result.UserCode,
				["DesignerCode"] = result.DesignerCode,
				["Language"] = result.Language,
				["Directory"] = result.Directory,
				["Resources"] = result.Resources,
				["IsRescaffold"] = result.IsRescaffold
			};
		}
		return null;
	}

	internal virtual IDictionary ScaffoldInitialCreateInternal(DbConnectionInfo connectionInfo, string contextTypeName, string contextAssemblyName, string migrationsNamespace, bool auto, string migrationsDir)
	{
		Assembly assembly = LoadAssembly(contextAssemblyName) ?? _assembly;
		DbMigrationsConfiguration configuration = new DbMigrationsConfiguration
		{
			ContextType = assembly.GetType(contextTypeName, throwOnError: true),
			MigrationsAssembly = _assembly,
			MigrationsNamespace = migrationsNamespace,
			AutomaticMigrationsEnabled = auto,
			MigrationsDirectory = migrationsDir
		};
		OverrideConfiguration(configuration, connectionInfo);
		return ToHashtable(CreateMigrationScaffolder(configuration).ScaffoldInitialCreate());
	}

	private DbMigrationsConfiguration GetMigrationsConfiguration(string migrationsConfigurationName)
	{
		return new MigrationsConfigurationFinder(new TypeFinder(_assembly)).FindMigrationsConfiguration(null, migrationsConfigurationName, Error.AssemblyMigrator_NoConfiguration, (string assembly, IEnumerable<Type> types) => Error.AssemblyMigrator_MultipleConfigurations(assembly), Error.AssemblyMigrator_NoConfigurationWithName, Error.AssemblyMigrator_MultipleConfigurationsWithName);
	}

	internal virtual IDictionary ScaffoldInternal(string name, DbConnectionInfo connectionInfo, string migrationsConfigurationName, bool ignoreChanges)
	{
		DbMigrationsConfiguration migrationsConfiguration = GetMigrationsConfiguration(migrationsConfigurationName);
		OverrideConfiguration(migrationsConfiguration, connectionInfo);
		return ToHashtable(CreateMigrationScaffolder(migrationsConfiguration).Scaffold(name, ignoreChanges));
	}

	internal IEnumerable<string> GetDatabaseMigrationsInternal(DbConnectionInfo connectionInfo, string migrationsConfigurationName)
	{
		DbMigrationsConfiguration migrationsConfiguration = GetMigrationsConfiguration(migrationsConfigurationName);
		OverrideConfiguration(migrationsConfiguration, connectionInfo);
		return CreateMigrator(migrationsConfiguration).GetDatabaseMigrations();
	}

	internal string ScriptUpdateInternal(string sourceMigration, string targetMigration, bool force, DbConnectionInfo connectionInfo, string migrationsConfigurationName)
	{
		DbMigrationsConfiguration migrationsConfiguration = GetMigrationsConfiguration(migrationsConfigurationName);
		OverrideConfiguration(migrationsConfiguration, connectionInfo, force);
		return new MigratorScriptingDecorator(CreateMigrator(migrationsConfiguration)).ScriptUpdate(sourceMigration, targetMigration);
	}

	internal void UpdateInternal(string targetMigration, bool force, DbConnectionInfo connectionInfo, string migrationsConfigurationName)
	{
		DbMigrationsConfiguration migrationsConfiguration = GetMigrationsConfiguration(migrationsConfigurationName);
		OverrideConfiguration(migrationsConfiguration, connectionInfo, force);
		CreateMigrator(migrationsConfiguration).Update(targetMigration);
	}

	private MigratorBase CreateMigrator(DbMigrationsConfiguration configuration)
	{
		return new MigratorLoggingDecorator(new DbMigrator(configuration), new ToolLogger(_reporter));
	}
}

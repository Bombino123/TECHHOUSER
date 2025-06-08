using System.Collections.Generic;
using System.Data.Entity.SqlServer.Resources;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.SqlServer;

internal class SqlTypesAssemblyLoader
{
	private const string AssemblyNameTemplate = "Microsoft.SqlServer.Types, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

	private static readonly SqlTypesAssemblyLoader _instance = new SqlTypesAssemblyLoader();

	private readonly IEnumerable<string> _preferredSqlTypesAssemblies;

	private readonly Lazy<SqlTypesAssembly> _latestVersion;

	public static SqlTypesAssemblyLoader DefaultInstance => _instance;

	public SqlTypesAssemblyLoader(IEnumerable<string> assemblyNames = null)
	{
		if (assemblyNames != null)
		{
			_preferredSqlTypesAssemblies = assemblyNames;
		}
		else
		{
			List<string> list = new List<string>
			{
				GenerateSqlServerTypesAssemblyName(11),
				GenerateSqlServerTypesAssemblyName(10)
			};
			for (int num = 20; num > 11; num--)
			{
				list.Add(GenerateSqlServerTypesAssemblyName(num));
			}
			_preferredSqlTypesAssemblies = list.ToList();
		}
		_latestVersion = new Lazy<SqlTypesAssembly>(BindToLatest, isThreadSafe: true);
	}

	private static string GenerateSqlServerTypesAssemblyName(int version)
	{
		return string.Format(CultureInfo.InvariantCulture, "Microsoft.SqlServer.Types, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91", new object[1] { version });
	}

	public SqlTypesAssemblyLoader(SqlTypesAssembly assembly)
	{
		_latestVersion = new Lazy<SqlTypesAssembly>(() => assembly, isThreadSafe: true);
	}

	public virtual SqlTypesAssembly TryGetSqlTypesAssembly()
	{
		return _latestVersion.Value;
	}

	public virtual SqlTypesAssembly GetSqlTypesAssembly()
	{
		return _latestVersion.Value ?? throw new InvalidOperationException(Strings.SqlProvider_SqlTypesAssemblyNotFound);
	}

	public virtual bool TryGetSqlTypesAssembly(Assembly assembly, out SqlTypesAssembly sqlAssembly)
	{
		if (IsKnownAssembly(assembly))
		{
			sqlAssembly = new SqlTypesAssembly(assembly);
			return true;
		}
		sqlAssembly = null;
		return false;
	}

	private SqlTypesAssembly BindToLatest()
	{
		Assembly assembly = null;
		IEnumerable<string> enumerable;
		if (SqlProviderServices.SqlServerTypesAssemblyName == null)
		{
			enumerable = _preferredSqlTypesAssemblies;
		}
		else
		{
			IEnumerable<string> enumerable2 = new string[1] { SqlProviderServices.SqlServerTypesAssemblyName };
			enumerable = enumerable2;
		}
		foreach (string item in enumerable)
		{
			AssemblyName assemblyRef = new AssemblyName(item);
			try
			{
				assembly = Assembly.Load(assemblyRef);
			}
			catch (FileNotFoundException)
			{
				continue;
			}
			catch (FileLoadException)
			{
				continue;
			}
			break;
		}
		if (assembly != null)
		{
			return new SqlTypesAssembly(assembly);
		}
		return null;
	}

	private bool IsKnownAssembly(Assembly assembly)
	{
		foreach (string preferredSqlTypesAssembly in _preferredSqlTypesAssemblies)
		{
			if (AssemblyNamesMatch(assembly.FullName, new AssemblyName(preferredSqlTypesAssembly)))
			{
				return true;
			}
		}
		return false;
	}

	private static bool AssemblyNamesMatch(string infoRowProviderAssemblyName, AssemblyName targetAssemblyName)
	{
		if (string.IsNullOrWhiteSpace(infoRowProviderAssemblyName))
		{
			return false;
		}
		AssemblyName assemblyName;
		try
		{
			assemblyName = new AssemblyName(infoRowProviderAssemblyName);
		}
		catch (Exception)
		{
			return false;
		}
		if (!string.Equals(targetAssemblyName.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (targetAssemblyName.Version == null || assemblyName.Version == null)
		{
			return false;
		}
		if (targetAssemblyName.Version.Major != assemblyName.Version.Major || targetAssemblyName.Version.Minor != assemblyName.Version.Minor)
		{
			return false;
		}
		return targetAssemblyName.GetPublicKeyToken()?.SequenceEqual(assemblyName.GetPublicKeyToken()) ?? false;
	}
}

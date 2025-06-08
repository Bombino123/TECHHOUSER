using System.Collections;
using System.Configuration;
using System.Data.Entity.Internal.ConfigFile;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Internal;

internal class InitializerConfig
{
	private const string ConfigKeyKey = "DatabaseInitializerForType";

	private const string DisabledSpecialValue = "Disabled";

	private readonly EntityFrameworkSection _entityFrameworkSettings;

	private readonly KeyValueConfigurationCollection _appSettings;

	public InitializerConfig()
	{
	}

	public InitializerConfig(EntityFrameworkSection entityFrameworkSettings, KeyValueConfigurationCollection appSettings)
	{
		_entityFrameworkSettings = entityFrameworkSettings;
		_appSettings = appSettings;
	}

	private static object TryGetInitializer(Type requiredContextType, string contextTypeName, string initializerTypeName, bool isDisabled, Func<object[]> initializerArgs, Func<object, object, string> exceptionMessage)
	{
		try
		{
			if (Type.GetType(contextTypeName, throwOnError: true) == requiredContextType)
			{
				if (isDisabled)
				{
					return Activator.CreateInstance(typeof(NullDatabaseInitializer<>).MakeGenericType(requiredContextType));
				}
				return Activator.CreateInstance(Type.GetType(initializerTypeName, throwOnError: true), initializerArgs());
			}
		}
		catch (Exception innerException)
		{
			string arg = (isDisabled ? "Disabled" : initializerTypeName);
			throw new InvalidOperationException(exceptionMessage(arg, contextTypeName), innerException);
		}
		return null;
	}

	public virtual object TryGetInitializer(Type contextType)
	{
		return TryGetInitializerFromEntityFrameworkSection(contextType) ?? TryGetInitializerFromLegacyConfig(contextType);
	}

	private object TryGetInitializerFromEntityFrameworkSection(Type contextType)
	{
		return (from e in ((IEnumerable)_entityFrameworkSettings.Contexts).OfType<ContextElement>()
			where e.IsDatabaseInitializationDisabled || !string.IsNullOrWhiteSpace(e.DatabaseInitializer.InitializerTypeName)
			select TryGetInitializer(contextType, e.ContextTypeName, e.DatabaseInitializer.InitializerTypeName ?? string.Empty, e.IsDatabaseInitializationDisabled, () => e.DatabaseInitializer.Parameters.GetTypedParameterValues(), Strings.Database_InitializeFromConfigFailed)).FirstOrDefault((object i) => i != null);
	}

	private object TryGetInitializerFromLegacyConfig(Type contextType)
	{
		foreach (string item in _appSettings.AllKeys.Where((string k) => k.StartsWith("DatabaseInitializerForType", StringComparison.OrdinalIgnoreCase)))
		{
			string text = item.Remove(0, "DatabaseInitializerForType".Length).Trim();
			string text2 = (_appSettings[item].Value ?? string.Empty).Trim();
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new InvalidOperationException(Strings.Database_BadLegacyInitializerEntry(item, text2));
			}
			object obj = TryGetInitializer(contextType, text, text2, text2.Length == 0 || text2.Equals("Disabled", StringComparison.OrdinalIgnoreCase), () => new object[0], Strings.Database_InitializeFromLegacyConfigFailed);
			if (obj != null)
			{
				return obj;
			}
		}
		return null;
	}
}

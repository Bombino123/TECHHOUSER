using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal.ConfigFile;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Internal;

internal class AppConfig
{
	public const string EFSectionName = "entityFramework";

	private static readonly AppConfig _defaultInstance = new AppConfig();

	private readonly KeyValueConfigurationCollection _appSettings;

	private readonly ConnectionStringSettingsCollection _connectionStrings;

	private readonly EntityFrameworkSection _entityFrameworkSettings;

	private readonly Lazy<IDbConnectionFactory> _defaultConnectionFactory;

	private readonly Lazy<IDbConnectionFactory> _defaultDefaultConnectionFactory = new Lazy<IDbConnectionFactory>(() => (IDbConnectionFactory)null, isThreadSafe: true);

	private readonly ProviderServicesFactory _providerServicesFactory;

	private readonly Lazy<IList<NamedDbProviderService>> _providerServices;

	public static AppConfig DefaultInstance => _defaultInstance;

	public virtual ContextConfig ContextConfigs => new ContextConfig(_entityFrameworkSettings);

	public virtual InitializerConfig Initializers => new InitializerConfig(_entityFrameworkSettings, _appSettings);

	public virtual string ConfigurationTypeName => _entityFrameworkSettings.ConfigurationTypeName;

	public virtual IList<NamedDbProviderService> DbProviderServices => _providerServices.Value;

	public virtual IEnumerable<IDbInterceptor> Interceptors => _entityFrameworkSettings.Interceptors.Interceptors;

	public virtual QueryCacheConfig QueryCache => new QueryCacheConfig(_entityFrameworkSettings);

	public AppConfig(Configuration configuration)
		: this(configuration.ConnectionStrings.ConnectionStrings, configuration.AppSettings.Settings, (EntityFrameworkSection)(object)configuration.GetSection("entityFramework"))
	{
	}

	public AppConfig(ConnectionStringSettingsCollection connectionStrings)
		: this(connectionStrings, null, null)
	{
	}

	private AppConfig()
		: this(ConfigurationManager.ConnectionStrings, Convert(ConfigurationManager.AppSettings), (EntityFrameworkSection)ConfigurationManager.GetSection("entityFramework"))
	{
	}

	internal AppConfig(ConnectionStringSettingsCollection connectionStrings, KeyValueConfigurationCollection appSettings, EntityFrameworkSection entityFrameworkSettings, ProviderServicesFactory providerServicesFactory = null)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		_connectionStrings = connectionStrings;
		_appSettings = (KeyValueConfigurationCollection)(((object)appSettings) ?? ((object)new KeyValueConfigurationCollection()));
		_entityFrameworkSettings = entityFrameworkSettings ?? new EntityFrameworkSection();
		_providerServicesFactory = providerServicesFactory ?? new ProviderServicesFactory();
		_providerServices = new Lazy<IList<NamedDbProviderService>>(() => (from e in ((IEnumerable)_entityFrameworkSettings.Providers).OfType<ProviderElement>()
			select new NamedDbProviderService(e.InvariantName, _providerServicesFactory.GetInstance(e.ProviderTypeName, e.InvariantName))).ToList());
		if (((ConfigurationElement)_entityFrameworkSettings.DefaultConnectionFactory).ElementInformation.IsPresent)
		{
			_defaultConnectionFactory = new Lazy<IDbConnectionFactory>(delegate
			{
				DefaultConnectionFactoryElement defaultConnectionFactory = _entityFrameworkSettings.DefaultConnectionFactory;
				try
				{
					Type factoryType = defaultConnectionFactory.GetFactoryType();
					object[] typedParameterValues = defaultConnectionFactory.Parameters.GetTypedParameterValues();
					return (IDbConnectionFactory)Activator.CreateInstance(factoryType, typedParameterValues);
				}
				catch (Exception innerException)
				{
					throw new InvalidOperationException(Strings.SetConnectionFactoryFromConfigFailed(defaultConnectionFactory.FactoryTypeName), innerException);
				}
			}, isThreadSafe: true);
		}
		else
		{
			_defaultConnectionFactory = _defaultDefaultConnectionFactory;
		}
	}

	public virtual IDbConnectionFactory TryGetDefaultConnectionFactory()
	{
		return _defaultConnectionFactory.Value;
	}

	public ConnectionStringSettings GetConnectionString(string name)
	{
		return _connectionStrings[name];
	}

	private static KeyValueConfigurationCollection Convert(NameValueCollection collection)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		KeyValueConfigurationCollection val = new KeyValueConfigurationCollection();
		string[] allKeys = collection.AllKeys;
		foreach (string text in allKeys)
		{
			val.Add(text, ConfigurationManager.AppSettings[text]);
		}
		return val;
	}
}

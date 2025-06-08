using System.Collections;
using System.Collections.Concurrent;
using System.Data.Entity.Internal.ConfigFile;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Internal;

internal class ContextConfig
{
	private readonly EntityFrameworkSection _entityFrameworkSettings;

	private readonly ConcurrentDictionary<Type, int?> _commandTimeouts = new ConcurrentDictionary<Type, int?>();

	public ContextConfig()
	{
	}

	public ContextConfig(EntityFrameworkSection entityFrameworkSettings)
	{
		_entityFrameworkSettings = entityFrameworkSettings;
	}

	public virtual int? TryGetCommandTimeout(Type contextType)
	{
		return _commandTimeouts.GetOrAdd(contextType, (Type requiredContextType) => (from e in ((IEnumerable)_entityFrameworkSettings.Contexts).OfType<ContextElement>()
			where e.CommandTimeout.HasValue
			select TryGetCommandTimeout(contextType, e.ContextTypeName, e.CommandTimeout.Value)).FirstOrDefault((int? i) => i.HasValue));
	}

	private static int? TryGetCommandTimeout(Type requiredContextType, string contextTypeName, int commandTimeout)
	{
		try
		{
			if (Type.GetType(contextTypeName, throwOnError: true) == requiredContextType)
			{
				return commandTimeout;
			}
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException(Strings.Database_InitializationException, innerException);
		}
		return null;
	}
}

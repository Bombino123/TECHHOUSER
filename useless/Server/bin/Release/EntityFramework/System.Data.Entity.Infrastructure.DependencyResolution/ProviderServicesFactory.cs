using System.Data.Entity.Core.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class ProviderServicesFactory
{
	public virtual DbProviderServices TryGetInstance(string providerTypeName)
	{
		Type type = Type.GetType(providerTypeName, throwOnError: false);
		if (!(type == null))
		{
			return GetInstance(type);
		}
		return null;
	}

	public virtual DbProviderServices GetInstance(string providerTypeName, string providerInvariantName)
	{
		Type? type = Type.GetType(providerTypeName, throwOnError: false);
		if (type == null)
		{
			throw new InvalidOperationException(Strings.EF6Providers_ProviderTypeMissing(providerTypeName, providerInvariantName));
		}
		return GetInstance(type);
	}

	private static DbProviderServices GetInstance(Type providerType)
	{
		object obj = ((object)providerType.GetStaticProperty("Instance")) ?? ((object)providerType.GetField("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
		if ((MemberInfo?)obj == null)
		{
			throw new InvalidOperationException(Strings.EF6Providers_InstanceMissing(providerType.AssemblyQualifiedName));
		}
		return (((MemberInfo)obj).GetValue() as DbProviderServices) ?? throw new InvalidOperationException(Strings.EF6Providers_NotDbProviderServices(providerType.AssemblyQualifiedName));
	}
}

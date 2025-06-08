using System.ComponentModel;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

public class DbConfigurationLoadedEventArgs : EventArgs
{
	private readonly InternalConfiguration _internalConfiguration;

	public IDbDependencyResolver DependencyResolver => _internalConfiguration.ResolverSnapshot;

	internal DbConfigurationLoadedEventArgs(InternalConfiguration configuration)
	{
		_internalConfiguration = configuration;
	}

	public void AddDependencyResolver(IDbDependencyResolver resolver, bool overrideConfigFile)
	{
		Check.NotNull(resolver, "resolver");
		_internalConfiguration.CheckNotLocked("AddDependencyResolver");
		_internalConfiguration.AddDependencyResolver(resolver, overrideConfigFile);
	}

	public void AddDefaultResolver(IDbDependencyResolver resolver)
	{
		Check.NotNull(resolver, "resolver");
		_internalConfiguration.CheckNotLocked("AddDefaultResolver");
		_internalConfiguration.AddDefaultResolver(resolver);
	}

	public void ReplaceService<TService>(Func<TService, object, TService> serviceInterceptor)
	{
		Check.NotNull(serviceInterceptor, "serviceInterceptor");
		AddDependencyResolver(new WrappingDependencyResolver<TService>(DependencyResolver, serviceInterceptor), overrideConfigFile: true);
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
}

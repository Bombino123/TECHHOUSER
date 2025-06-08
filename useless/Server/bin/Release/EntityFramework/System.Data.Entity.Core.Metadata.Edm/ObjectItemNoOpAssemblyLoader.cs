using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class ObjectItemNoOpAssemblyLoader : ObjectItemAssemblyLoader
{
	internal ObjectItemNoOpAssemblyLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
		: base(assembly, new MutableAssemblyCacheEntry(), sessionData)
	{
	}

	internal override void Load()
	{
		if (!base.SessionData.KnownAssemblies.Contains(base.SourceAssembly, base.SessionData.ObjectItemAssemblyLoaderFactory, base.SessionData.EdmItemCollection))
		{
			AddToKnownAssemblies();
		}
	}

	protected override void AddToAssembliesLoaded()
	{
		throw new NotImplementedException();
	}

	protected override void LoadTypesFromAssembly()
	{
		throw new NotImplementedException();
	}
}

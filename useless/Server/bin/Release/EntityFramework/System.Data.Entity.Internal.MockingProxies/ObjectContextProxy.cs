using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Reflection;

namespace System.Data.Entity.Internal.MockingProxies;

internal class ObjectContextProxy : IDisposable
{
	private readonly ObjectContext _objectContext;

	private ObjectItemCollection _objectItemCollection;

	public virtual EntityConnectionProxy Connection => new EntityConnectionProxy((EntityConnection)_objectContext.Connection);

	public virtual string DefaultContainerName
	{
		get
		{
			return _objectContext.DefaultContainerName;
		}
		set
		{
			_objectContext.DefaultContainerName = value;
		}
	}

	protected ObjectContextProxy()
	{
	}

	public ObjectContextProxy(ObjectContext objectContext)
	{
		_objectContext = objectContext;
	}

	public static implicit operator ObjectContext(ObjectContextProxy proxy)
	{
		return proxy?._objectContext;
	}

	public virtual void Dispose()
	{
		_objectContext.Dispose();
	}

	public virtual IEnumerable<GlobalItem> GetObjectItemCollection()
	{
		return _objectItemCollection = (ObjectItemCollection)_objectContext.MetadataWorkspace.GetItemCollection(DataSpace.OSpace);
	}

	public virtual Type GetClrType(StructuralType item)
	{
		return _objectItemCollection.GetClrType(item);
	}

	public virtual Type GetClrType(EnumType item)
	{
		return _objectItemCollection.GetClrType(item);
	}

	public virtual void LoadFromAssembly(Assembly assembly)
	{
		_objectContext.MetadataWorkspace.LoadFromAssembly(assembly);
	}

	public virtual ObjectContextProxy CreateNew(EntityConnectionProxy entityConnection)
	{
		return new ObjectContextProxy(new ObjectContext(entityConnection));
	}

	public virtual void CopyContextOptions(ObjectContextProxy source)
	{
		_objectContext.ContextOptions.LazyLoadingEnabled = source._objectContext.ContextOptions.LazyLoadingEnabled;
		_objectContext.ContextOptions.ProxyCreationEnabled = source._objectContext.ContextOptions.ProxyCreationEnabled;
		_objectContext.ContextOptions.UseCSharpNullComparisonBehavior = source._objectContext.ContextOptions.UseCSharpNullComparisonBehavior;
		_objectContext.ContextOptions.UseConsistentNullReferenceBehavior = source._objectContext.ContextOptions.UseConsistentNullReferenceBehavior;
		_objectContext.ContextOptions.UseLegacyPreserveChangesBehavior = source._objectContext.ContextOptions.UseLegacyPreserveChangesBehavior;
		_objectContext.CommandTimeout = source._objectContext.CommandTimeout;
		_objectContext.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions = source._objectContext.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions;
		_objectContext.InterceptionContext = source._objectContext.InterceptionContext.WithObjectContext(_objectContext);
	}
}

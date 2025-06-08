using System.Data.Common;
using System.Data.Entity.Core.Mapping.Update.Internal;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.EntityClient.Internal;

internal class EntityAdapter : IEntityAdapter
{
	private bool _acceptChangesDuringUpdate = true;

	private EntityConnection _connection;

	private readonly ObjectContext _context;

	private readonly Func<EntityAdapter, UpdateTranslator> _updateTranslatorFactory;

	public ObjectContext Context => _context;

	DbConnection IEntityAdapter.Connection
	{
		get
		{
			return Connection;
		}
		set
		{
			Connection = (EntityConnection)value;
		}
	}

	public EntityConnection Connection
	{
		get
		{
			return _connection;
		}
		set
		{
			_connection = value;
		}
	}

	public bool AcceptChangesDuringUpdate
	{
		get
		{
			return _acceptChangesDuringUpdate;
		}
		set
		{
			_acceptChangesDuringUpdate = value;
		}
	}

	public int? CommandTimeout { get; set; }

	public EntityAdapter(ObjectContext context)
		: this(context, (EntityAdapter a) => new UpdateTranslator(a))
	{
	}

	protected EntityAdapter(ObjectContext context, Func<EntityAdapter, UpdateTranslator> updateTranslatorFactory)
	{
		_context = context;
		_updateTranslatorFactory = updateTranslatorFactory;
	}

	public int Update()
	{
		return Update(0, (UpdateTranslator ut) => ut.Update());
	}

	public Task<int> UpdateAsync(CancellationToken cancellationToken)
	{
		return Update<Task<int>>(Task.FromResult(0), (UpdateTranslator ut) => ut.UpdateAsync(cancellationToken));
	}

	private T Update<T>(T noChangesResult, Func<UpdateTranslator, T> updateFunction)
	{
		if (!IsStateManagerDirty(_context.ObjectStateManager))
		{
			return noChangesResult;
		}
		if (_connection == null)
		{
			throw Error.EntityClient_NoConnectionForAdapter();
		}
		if (_connection.StoreProviderFactory == null || _connection.StoreConnection == null)
		{
			throw Error.EntityClient_NoStoreConnectionForUpdate();
		}
		if (ConnectionState.Open != _connection.State)
		{
			throw Error.EntityClient_ClosedConnectionForUpdate();
		}
		UpdateTranslator arg = _updateTranslatorFactory(this);
		return updateFunction(arg);
	}

	private static bool IsStateManagerDirty(ObjectStateManager entityCache)
	{
		return entityCache.HasChanges();
	}
}

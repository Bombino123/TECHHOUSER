using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Internal;

internal sealed class CommandTracer : ICancelableDbCommandInterceptor, IDbInterceptor, IDbCommandTreeInterceptor, ICancelableEntityConnectionInterceptor, IDisposable
{
	private readonly List<DbCommand> _commands = new List<DbCommand>();

	private readonly List<DbCommandTree> _commandTrees = new List<DbCommandTree>();

	private readonly DbContext _context;

	private readonly DbDispatchers _dispatchers;

	public IEnumerable<DbCommand> DbCommands => _commands;

	public IEnumerable<DbCommandTree> CommandTrees => _commandTrees;

	public CommandTracer(DbContext context)
		: this(context, DbInterception.Dispatch)
	{
	}

	internal CommandTracer(DbContext context, DbDispatchers dispatchers)
	{
		_context = context;
		_dispatchers = dispatchers;
		_dispatchers.AddInterceptor(this);
	}

	public bool CommandExecuting(DbCommand command, DbInterceptionContext interceptionContext)
	{
		if (interceptionContext.DbContexts.Contains(_context, object.ReferenceEquals))
		{
			_commands.Add(command);
			return false;
		}
		return true;
	}

	public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
	{
		if (interceptionContext.DbContexts.Contains(_context, object.ReferenceEquals))
		{
			_commandTrees.Add(interceptionContext.Result);
		}
	}

	public bool ConnectionOpening(EntityConnection connection, DbInterceptionContext interceptionContext)
	{
		return !interceptionContext.DbContexts.Contains(_context, object.ReferenceEquals);
	}

	void IDisposable.Dispose()
	{
		_dispatchers.RemoveInterceptor(this);
	}
}

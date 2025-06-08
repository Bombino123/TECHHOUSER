using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql;

public sealed class ParseResult
{
	private readonly DbCommandTree _commandTree;

	private readonly ReadOnlyCollection<FunctionDefinition> _functionDefs;

	public DbCommandTree CommandTree => _commandTree;

	public ReadOnlyCollection<FunctionDefinition> FunctionDefinitions => _functionDefs;

	internal ParseResult(DbCommandTree commandTree, List<FunctionDefinition> functionDefs)
	{
		_commandTree = commandTree;
		_functionDefs = new ReadOnlyCollection<FunctionDefinition>(functionDefs);
	}
}

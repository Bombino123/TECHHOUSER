using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal sealed class ProviderCommandInfo
{
	private readonly DbCommandTree _commandTree;

	internal DbCommandTree CommandTree => _commandTree;

	internal ProviderCommandInfo(DbCommandTree commandTree)
	{
		_commandTree = commandTree;
	}
}

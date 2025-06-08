using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Migrations.Model;

public class HistoryOperation : MigrationOperation
{
	private readonly IList<DbModificationCommandTree> _commandTrees;

	public IList<DbModificationCommandTree> CommandTrees => _commandTrees;

	public override bool IsDestructiveChange => false;

	public HistoryOperation(IList<DbModificationCommandTree> commandTrees, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotNull(commandTrees, "commandTrees");
		if (!commandTrees.Any())
		{
			throw new ArgumentException(Strings.CollectionEmpty("commandTrees", "HistoryOperation"));
		}
		_commandTrees = commandTrees;
	}
}

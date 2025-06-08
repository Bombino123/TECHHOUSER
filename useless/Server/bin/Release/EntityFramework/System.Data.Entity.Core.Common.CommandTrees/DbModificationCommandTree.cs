using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class DbModificationCommandTree : DbCommandTree
{
	private readonly DbExpressionBinding _target;

	private ReadOnlyCollection<DbParameterReferenceExpression> _parameters;

	public DbExpressionBinding Target => _target;

	internal abstract bool HasReader { get; }

	internal DbModificationCommandTree()
	{
	}

	internal DbModificationCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target)
		: base(metadata, dataSpace)
	{
		_target = target;
	}

	internal override IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters()
	{
		if (_parameters == null)
		{
			_parameters = ParameterRetriever.GetParameters(this);
		}
		return _parameters.Select((DbParameterReferenceExpression p) => new KeyValuePair<string, TypeUsage>(p.ParameterName, p.ResultType));
	}

	internal override void DumpStructure(ExpressionDumper dumper)
	{
		if (Target != null)
		{
			dumper.Dump(Target, "Target");
		}
	}
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbQueryCommandTree : DbCommandTree
{
	private readonly DbExpression _query;

	private ReadOnlyCollection<DbParameterReferenceExpression> _parameters;

	public DbExpression Query => _query;

	public override DbCommandTreeKind CommandTreeKind => DbCommandTreeKind.Query;

	public DbQueryCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query, bool validate, bool useDatabaseNullSemantics, bool disableFilterOverProjectionSimplificationForCustomFunctions)
		: base(metadata, dataSpace, useDatabaseNullSemantics, disableFilterOverProjectionSimplificationForCustomFunctions)
	{
		Check.NotNull(query, "query");
		if (validate)
		{
			DbExpressionValidator dbExpressionValidator = new DbExpressionValidator(metadata, dataSpace);
			dbExpressionValidator.ValidateExpression(query, "query");
			_parameters = new ReadOnlyCollection<DbParameterReferenceExpression>(dbExpressionValidator.Parameters.Select((KeyValuePair<string, DbParameterReferenceExpression> paramInfo) => paramInfo.Value).ToList());
		}
		_query = query;
	}

	public DbQueryCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query, bool validate, bool useDatabaseNullSemantics)
		: this(metadata, dataSpace, query, validate, useDatabaseNullSemantics, disableFilterOverProjectionSimplificationForCustomFunctions: false)
	{
	}

	public DbQueryCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query, bool validate)
		: this(metadata, dataSpace, query, validate, useDatabaseNullSemantics: true, disableFilterOverProjectionSimplificationForCustomFunctions: false)
	{
	}

	public DbQueryCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query)
		: this(metadata, dataSpace, query, validate: true, useDatabaseNullSemantics: true, disableFilterOverProjectionSimplificationForCustomFunctions: false)
	{
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
		if (Query != null)
		{
			dumper.Dump(Query, "Query");
		}
	}

	internal override string PrintTree(ExpressionPrinter printer)
	{
		return printer.Print(this);
	}

	internal static DbQueryCommandTree FromValidExpression(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query, bool useDatabaseNullSemantics, bool disableFilterOverProjectionSimplificationForCustomFunctions)
	{
		return new DbQueryCommandTree(metadata, dataSpace, query, validate: false, useDatabaseNullSemantics, disableFilterOverProjectionSimplificationForCustomFunctions);
	}
}

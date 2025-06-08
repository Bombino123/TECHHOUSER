using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class ProviderCommandInfoUtils
{
	internal static ProviderCommandInfo Create(Command command, Node node)
	{
		PhysicalProjectOp physicalProjectOp = node.Op as PhysicalProjectOp;
		PlanCompiler.Assert(physicalProjectOp != null, "Expected root Op to be a physical Project");
		DbCommandTree dbCommandTree = CTreeGenerator.Generate(command, node);
		DbQueryCommandTree obj = dbCommandTree as DbQueryCommandTree;
		PlanCompiler.Assert(obj != null, "null query command tree");
		CollectionType edmType = TypeHelpers.GetEdmType<CollectionType>(obj.Query.ResultType);
		PlanCompiler.Assert(TypeSemantics.IsRowType(edmType.TypeUsage), "command rowtype is not a record");
		BuildOutputVarMap(physicalProjectOp, edmType.TypeUsage);
		return new ProviderCommandInfo(dbCommandTree);
	}

	private static Dictionary<Var, EdmProperty> BuildOutputVarMap(PhysicalProjectOp projectOp, TypeUsage outputType)
	{
		Dictionary<Var, EdmProperty> dictionary = new Dictionary<Var, EdmProperty>();
		PlanCompiler.Assert(TypeSemantics.IsRowType(outputType), "PhysicalProjectOp result type is not a RowType?");
		IEnumerator<EdmProperty> enumerator = TypeHelpers.GetEdmType<RowType>(outputType).Properties.GetEnumerator();
		IEnumerator<Var> enumerator2 = projectOp.Outputs.GetEnumerator();
		while (true)
		{
			bool num = enumerator.MoveNext();
			bool flag = enumerator2.MoveNext();
			if (num != flag)
			{
				throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 1, null);
			}
			if (!num)
			{
				break;
			}
			dictionary[enumerator2.Current] = enumerator.Current;
		}
		return dictionary;
	}
}

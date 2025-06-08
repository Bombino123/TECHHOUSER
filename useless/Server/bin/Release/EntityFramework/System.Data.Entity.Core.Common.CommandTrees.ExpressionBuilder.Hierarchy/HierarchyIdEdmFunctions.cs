using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Hierarchy;

public static class HierarchyIdEdmFunctions
{
	public static DbFunctionExpression HierarchyIdParse(DbExpression input)
	{
		Check.NotNull(input, "input");
		return EdmFunctions.InvokeCanonicalFunction("HierarchyIdParse", input);
	}

	public static DbFunctionExpression HierarchyIdGetRoot()
	{
		return EdmFunctions.InvokeCanonicalFunction("HierarchyIdGetRoot");
	}

	public static DbFunctionExpression GetAncestor(this DbExpression hierarchyIdValue, DbExpression n)
	{
		Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
		Check.NotNull(n, "n");
		return EdmFunctions.InvokeCanonicalFunction("GetAncestor", hierarchyIdValue, n);
	}

	public static DbFunctionExpression GetDescendant(this DbExpression hierarchyIdValue, DbExpression child1, DbExpression child2)
	{
		Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
		Check.NotNull(child1, "child1");
		Check.NotNull(child2, "child2");
		return EdmFunctions.InvokeCanonicalFunction("GetDescendant", hierarchyIdValue, child1, child2);
	}

	public static DbFunctionExpression GetLevel(this DbExpression hierarchyIdValue)
	{
		Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
		return EdmFunctions.InvokeCanonicalFunction("GetLevel", hierarchyIdValue);
	}

	public static DbFunctionExpression IsDescendantOf(this DbExpression hierarchyIdValue, DbExpression parent)
	{
		Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
		Check.NotNull(parent, "parent");
		return EdmFunctions.InvokeCanonicalFunction("IsDescendantOf", hierarchyIdValue, parent);
	}

	public static DbFunctionExpression GetReparentedValue(this DbExpression hierarchyIdValue, DbExpression oldRoot, DbExpression newRoot)
	{
		Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
		Check.NotNull(oldRoot, "oldRoot");
		Check.NotNull(newRoot, "newRoot");
		return EdmFunctions.InvokeCanonicalFunction("GetReparentedValue", hierarchyIdValue, oldRoot, newRoot);
	}
}

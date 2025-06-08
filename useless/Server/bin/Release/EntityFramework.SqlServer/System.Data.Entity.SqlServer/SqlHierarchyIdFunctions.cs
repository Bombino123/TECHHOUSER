using System.Data.Entity.Hierarchy;
using System.Data.Entity.SqlServer.Resources;

namespace System.Data.Entity.SqlServer;

public static class SqlHierarchyIdFunctions
{
	[DbFunction("SqlServer", "GetAncestor")]
	public static HierarchyId GetAncestor(HierarchyId hierarchyIdValue, int n)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "GetDescendant")]
	public static HierarchyId GetDescendant(HierarchyId hierarchyIdValue, HierarchyId child1, HierarchyId child2)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "GetLevel")]
	public static short GetLevel(HierarchyId hierarchyIdValue)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "GetRoot")]
	public static HierarchyId GetRoot()
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "IsDescendantOf")]
	public static bool IsDescendantOf(HierarchyId hierarchyIdValue, HierarchyId parent)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "GetReparentedValue")]
	public static HierarchyId GetReparentedValue(HierarchyId hierarchyIdValue, HierarchyId oldRoot, HierarchyId newRoot)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}

	[DbFunction("SqlServer", "Parse")]
	public static HierarchyId Parse(string input)
	{
		throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
	}
}

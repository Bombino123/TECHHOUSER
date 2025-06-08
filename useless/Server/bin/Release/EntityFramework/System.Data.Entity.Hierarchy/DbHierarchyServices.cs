namespace System.Data.Entity.Hierarchy;

[Serializable]
public abstract class DbHierarchyServices
{
	public abstract HierarchyId GetAncestor(int n);

	public abstract HierarchyId GetDescendant(HierarchyId child1, HierarchyId child2);

	public abstract short GetLevel();

	public static HierarchyId GetRoot()
	{
		return new HierarchyId("/");
	}

	public abstract bool IsDescendantOf(HierarchyId parent);

	public abstract HierarchyId GetReparentedValue(HierarchyId oldRoot, HierarchyId newRoot);

	public static HierarchyId Parse(string input)
	{
		return new HierarchyId(input);
	}
}

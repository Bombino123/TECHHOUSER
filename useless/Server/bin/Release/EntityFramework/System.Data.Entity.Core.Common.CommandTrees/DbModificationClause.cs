using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Common.Utils;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class DbModificationClause
{
	internal DbModificationClause()
	{
	}

	internal abstract void DumpStructure(ExpressionDumper dumper);

	internal abstract TreeNode Print(DbExpressionVisitor<TreeNode> visitor);
}

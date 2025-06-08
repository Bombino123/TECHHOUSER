using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Common.Utils;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbSetClause : DbModificationClause
{
	private readonly DbExpression _prop;

	private readonly DbExpression _val;

	public DbExpression Property => _prop;

	public DbExpression Value => _val;

	internal DbSetClause(DbExpression targetProperty, DbExpression sourceValue)
	{
		_prop = targetProperty;
		_val = sourceValue;
	}

	internal override void DumpStructure(ExpressionDumper dumper)
	{
		dumper.Begin("DbSetClause");
		if (Property != null)
		{
			dumper.Dump(Property, "Property");
		}
		if (Value != null)
		{
			dumper.Dump(Value, "Value");
		}
		dumper.End("DbSetClause");
	}

	internal override TreeNode Print(DbExpressionVisitor<TreeNode> visitor)
	{
		TreeNode treeNode = new TreeNode("DbSetClause");
		if (Property != null)
		{
			treeNode.Children.Add(new TreeNode("Property", Property.Accept(visitor)));
		}
		if (Value != null)
		{
			treeNode.Children.Add(new TreeNode("Value", Value.Accept(visitor)));
		}
		return treeNode;
	}
}

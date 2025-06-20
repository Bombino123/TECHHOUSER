namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class BuiltInExpr : Node
{
	internal readonly BuiltInKind Kind;

	internal readonly string Name;

	internal readonly int ArgCount;

	internal readonly Node Arg1;

	internal readonly Node Arg2;

	internal readonly Node Arg3;

	internal readonly Node Arg4;

	private BuiltInExpr(BuiltInKind kind, string name)
	{
		Kind = kind;
		Name = name.ToUpperInvariant();
	}

	internal BuiltInExpr(BuiltInKind kind, string name, Node arg1)
		: this(kind, name)
	{
		ArgCount = 1;
		Arg1 = arg1;
	}

	internal BuiltInExpr(BuiltInKind kind, string name, Node arg1, Node arg2)
		: this(kind, name)
	{
		ArgCount = 2;
		Arg1 = arg1;
		Arg2 = arg2;
	}

	internal BuiltInExpr(BuiltInKind kind, string name, Node arg1, Node arg2, Node arg3)
		: this(kind, name)
	{
		ArgCount = 3;
		Arg1 = arg1;
		Arg2 = arg2;
		Arg3 = arg3;
	}

	internal BuiltInExpr(BuiltInKind kind, string name, Node arg1, Node arg2, Node arg3, Node arg4)
		: this(kind, name)
	{
		ArgCount = 4;
		Arg1 = arg1;
		Arg2 = arg2;
		Arg3 = arg3;
		Arg4 = arg4;
	}
}

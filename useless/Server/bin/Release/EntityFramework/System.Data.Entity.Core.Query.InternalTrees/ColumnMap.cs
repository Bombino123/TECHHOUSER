using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class ColumnMap
{
	private TypeUsage _type;

	private string _name;

	internal const string DefaultColumnName = "Value";

	internal TypeUsage Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	internal string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	internal bool IsNamed => _name != null;

	internal ColumnMap(TypeUsage type, string name)
	{
		_type = type;
		_name = name;
	}

	[DebuggerNonUserCode]
	internal abstract void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg);

	[DebuggerNonUserCode]
	internal abstract TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg);
}

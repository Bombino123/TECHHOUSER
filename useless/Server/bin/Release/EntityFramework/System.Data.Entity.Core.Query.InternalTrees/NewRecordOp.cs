using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class NewRecordOp : ScalarOp
{
	private readonly List<EdmProperty> m_fields;

	internal static readonly NewRecordOp Pattern = new NewRecordOp();

	internal List<EdmProperty> Properties => m_fields;

	internal NewRecordOp(TypeUsage type)
		: base(OpType.NewRecord, type)
	{
		m_fields = new List<EdmProperty>(TypeHelpers.GetEdmType<RowType>(type).Properties);
	}

	internal NewRecordOp(TypeUsage type, List<EdmProperty> fields)
		: base(OpType.NewRecord, type)
	{
		m_fields = fields;
	}

	private NewRecordOp()
		: base(OpType.NewRecord)
	{
	}

	internal bool GetFieldPosition(EdmProperty field, out int fieldPosition)
	{
		fieldPosition = 0;
		for (int i = 0; i < m_fields.Count; i++)
		{
			if (m_fields[i] == field)
			{
				fieldPosition = i;
				return true;
			}
		}
		return false;
	}

	[DebuggerNonUserCode]
	internal override void Accept(BasicOpVisitor v, Node n)
	{
		v.Visit(this, n);
	}

	[DebuggerNonUserCode]
	internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
	{
		return v.Visit(this, n);
	}
}

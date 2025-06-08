using System.Collections.Generic;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class SingleStreamNestOp : NestBaseOp
{
	private readonly VarVec m_keys;

	private readonly Var m_discriminator;

	private readonly List<SortKey> m_postfixSortKeys;

	internal override int Arity => 1;

	internal Var Discriminator => m_discriminator;

	internal List<SortKey> PostfixSortKeys => m_postfixSortKeys;

	internal VarVec Keys => m_keys;

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

	internal SingleStreamNestOp(VarVec keys, List<SortKey> prefixSortKeys, List<SortKey> postfixSortKeys, VarVec outputVars, List<CollectionInfo> collectionInfoList, Var discriminatorVar)
		: base(OpType.SingleStreamNest, prefixSortKeys, outputVars, collectionInfoList)
	{
		m_keys = keys;
		m_postfixSortKeys = postfixSortKeys;
		m_discriminator = discriminatorVar;
	}
}

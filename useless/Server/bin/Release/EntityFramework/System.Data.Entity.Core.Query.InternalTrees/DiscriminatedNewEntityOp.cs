using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class DiscriminatedNewEntityOp : NewEntityBaseOp
{
	private readonly ExplicitDiscriminatorMap m_discriminatorMap;

	internal static readonly DiscriminatedNewEntityOp Pattern = new DiscriminatedNewEntityOp();

	internal ExplicitDiscriminatorMap DiscriminatorMap => m_discriminatorMap;

	internal DiscriminatedNewEntityOp(TypeUsage type, ExplicitDiscriminatorMap discriminatorMap, EntitySet entitySet, List<RelProperty> relProperties)
		: base(OpType.DiscriminatedNewEntity, type, scoped: true, entitySet, relProperties)
	{
		m_discriminatorMap = discriminatorMap;
	}

	private DiscriminatedNewEntityOp()
		: base(OpType.DiscriminatedNewEntity)
	{
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

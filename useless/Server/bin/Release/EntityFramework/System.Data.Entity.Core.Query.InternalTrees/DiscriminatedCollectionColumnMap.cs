using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class DiscriminatedCollectionColumnMap : CollectionColumnMap
{
	private readonly SimpleColumnMap m_discriminator;

	private readonly object m_discriminatorValue;

	internal SimpleColumnMap Discriminator => m_discriminator;

	internal object DiscriminatorValue => m_discriminatorValue;

	internal DiscriminatedCollectionColumnMap(TypeUsage type, string name, ColumnMap elementMap, SimpleColumnMap[] keys, SimpleColumnMap[] foreignKeys, SimpleColumnMap discriminator, object discriminatorValue)
		: base(type, name, elementMap, keys, foreignKeys)
	{
		m_discriminator = discriminator;
		m_discriminatorValue = discriminatorValue;
	}

	[DebuggerNonUserCode]
	internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
	{
		visitor.Visit(this, arg);
	}

	[DebuggerNonUserCode]
	internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
	{
		return visitor.Visit(this, arg);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "M{{{0}}}", new object[1] { base.Element });
	}
}

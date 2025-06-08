namespace System.Data.Entity.Core.Metadata.Edm;

internal class SafeLinkCollection<TParent, TChild> : ReadOnlyMetadataCollection<TChild> where TParent : class where TChild : MetadataItem
{
	public SafeLinkCollection(TParent parent, Func<TChild, SafeLink<TParent>> getLink, MetadataCollection<TChild> children)
		: base((MetadataCollection<TChild>)SafeLink<TParent>.BindChildren(parent, getLink, children))
	{
	}
}

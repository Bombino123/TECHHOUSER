using System.Collections.Generic;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class SafeLink<TParent> where TParent : class
{
	private TParent _value;

	public TParent Value => _value;

	internal static IEnumerable<TChild> BindChildren<TChild>(TParent parent, Func<TChild, SafeLink<TParent>> getLink, IEnumerable<TChild> children)
	{
		foreach (TChild child in children)
		{
			BindChild(parent, getLink, child);
		}
		return children;
	}

	internal static TChild BindChild<TChild>(TParent parent, Func<TChild, SafeLink<TParent>> getLink, TChild child)
	{
		getLink(child)._value = parent;
		return child;
	}
}

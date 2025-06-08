namespace dnlib.DotNet;

public abstract class ModifierSig : NonLeafSig
{
	private readonly ITypeDefOrRef modifier;

	public ITypeDefOrRef Modifier => modifier;

	protected ModifierSig(ITypeDefOrRef modifier, TypeSig nextSig)
		: base(nextSig)
	{
		this.modifier = modifier;
	}
}

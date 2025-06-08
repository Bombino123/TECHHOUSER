using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class GenericSig : LeafSig
{
	private readonly bool isTypeVar;

	private readonly uint number;

	private readonly ITypeOrMethodDef genericParamProvider;

	public bool HasOwner => genericParamProvider != null;

	public bool HasOwnerType => OwnerType != null;

	public bool HasOwnerMethod => OwnerMethod != null;

	public TypeDef OwnerType => genericParamProvider as TypeDef;

	public MethodDef OwnerMethod => genericParamProvider as MethodDef;

	public uint Number => number;

	public GenericParam GenericParam
	{
		get
		{
			ITypeOrMethodDef typeOrMethodDef = genericParamProvider;
			if (typeOrMethodDef == null)
			{
				return null;
			}
			IList<GenericParam> genericParameters = typeOrMethodDef.GenericParameters;
			int count = genericParameters.Count;
			for (int i = 0; i < count; i++)
			{
				GenericParam genericParam = genericParameters[i];
				if (genericParam.Number == number)
				{
					return genericParam;
				}
			}
			return null;
		}
	}

	public bool IsMethodVar => !isTypeVar;

	public bool IsTypeVar => isTypeVar;

	protected GenericSig(bool isTypeVar, uint number)
		: this(isTypeVar, number, null)
	{
	}

	protected GenericSig(bool isTypeVar, uint number, ITypeOrMethodDef genericParamProvider)
	{
		this.isTypeVar = isTypeVar;
		this.number = number;
		this.genericParamProvider = genericParamProvider;
	}
}

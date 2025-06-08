using dnlib.Utils;

namespace dnlib.DotNet;

public class MethodDefUser : MethodDef
{
	public MethodDefUser()
	{
		paramDefs = new LazyList<ParamDef>(this);
		genericParameters = new LazyList<GenericParam>(this);
		parameterList = new ParameterList(this, null);
		semAttrs = 0 | MethodDef.SEMATTRS_INITD;
	}

	public MethodDefUser(UTF8String name)
		: this(name, null, MethodImplAttributes.IL, MethodAttributes.PrivateScope)
	{
	}

	public MethodDefUser(UTF8String name, MethodSig methodSig)
		: this(name, methodSig, MethodImplAttributes.IL, MethodAttributes.PrivateScope)
	{
	}

	public MethodDefUser(UTF8String name, MethodSig methodSig, MethodAttributes flags)
		: this(name, methodSig, MethodImplAttributes.IL, flags)
	{
	}

	public MethodDefUser(UTF8String name, MethodSig methodSig, MethodImplAttributes implFlags)
		: this(name, methodSig, implFlags, MethodAttributes.PrivateScope)
	{
	}

	public MethodDefUser(UTF8String name, MethodSig methodSig, MethodImplAttributes implFlags, MethodAttributes flags)
	{
		base.name = name;
		signature = methodSig;
		paramDefs = new LazyList<ParamDef>(this);
		genericParameters = new LazyList<GenericParam>(this);
		implAttributes = (int)implFlags;
		attributes = (int)flags;
		parameterList = new ParameterList(this, null);
		semAttrs = 0 | MethodDef.SEMATTRS_INITD;
	}
}

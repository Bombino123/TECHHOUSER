using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class ImportMapper
{
	public virtual ITypeDefOrRef Map(ITypeDefOrRef source)
	{
		return null;
	}

	public virtual IField Map(FieldDef source)
	{
		return null;
	}

	public virtual IMethod Map(MethodDef source)
	{
		return null;
	}

	public virtual MemberRef Map(MemberRef source)
	{
		return null;
	}

	public virtual ITypeDefOrRef Map(Type source)
	{
		return null;
	}
}

using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class GenericParamConstraintUser : GenericParamConstraint
{
	public GenericParamConstraintUser()
	{
	}

	public GenericParamConstraintUser(ITypeDefOrRef constraint)
	{
		base.constraint = constraint;
	}
}

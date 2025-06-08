namespace dnlib.DotNet;

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

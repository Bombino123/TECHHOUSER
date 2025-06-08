namespace dnlib.DotNet;

public struct MethodOverride
{
	public IMethodDefOrRef MethodBody;

	public IMethodDefOrRef MethodDeclaration;

	public MethodOverride(IMethodDefOrRef methodBody, IMethodDefOrRef methodDeclaration)
	{
		MethodBody = methodBody;
		MethodDeclaration = methodDeclaration;
	}
}

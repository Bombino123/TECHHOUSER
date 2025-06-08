using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
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

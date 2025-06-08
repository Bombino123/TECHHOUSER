using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class TypeSpecUser : TypeSpec
{
	public TypeSpecUser()
	{
	}

	public TypeSpecUser(TypeSig typeSig)
	{
		base.typeSig = typeSig;
		extraData = null;
		typeSigAndExtraData_isInitialized = true;
	}
}

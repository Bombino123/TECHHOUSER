namespace dnlib.DotNet;

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

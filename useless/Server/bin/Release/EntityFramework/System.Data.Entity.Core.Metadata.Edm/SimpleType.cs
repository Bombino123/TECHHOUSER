namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class SimpleType : EdmType
{
	internal SimpleType()
	{
	}

	internal SimpleType(string name, string namespaceName, DataSpace dataSpace)
		: base(name, namespaceName, dataSpace)
	{
	}
}

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class ClrEnumType : EnumType
{
	private readonly Type _type;

	private readonly string _cspaceTypeName;

	internal override Type ClrType => _type;

	internal string CSpaceTypeName => _cspaceTypeName;

	internal ClrEnumType(Type clrType, string cspaceNamespaceName, string cspaceTypeName)
		: base(clrType)
	{
		_type = clrType;
		_cspaceTypeName = cspaceNamespaceName + "." + cspaceTypeName;
	}
}

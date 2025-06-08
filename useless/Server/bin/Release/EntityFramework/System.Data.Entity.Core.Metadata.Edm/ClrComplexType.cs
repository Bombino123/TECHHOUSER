using System.Data.Entity.Utilities;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class ClrComplexType : ComplexType
{
	private readonly Type _type;

	private Func<object> _constructor;

	private readonly string _cspaceTypeName;

	internal Func<object> Constructor
	{
		get
		{
			return _constructor;
		}
		set
		{
			Interlocked.CompareExchange(ref _constructor, value, null);
		}
	}

	internal override Type ClrType => _type;

	internal string CSpaceTypeName => _cspaceTypeName;

	internal ClrComplexType(Type clrType, string cspaceNamespaceName, string cspaceTypeName)
		: base(Check.NotNull(clrType, "clrType").Name, clrType.NestingNamespace() ?? string.Empty, DataSpace.OSpace)
	{
		_type = clrType;
		_cspaceTypeName = cspaceNamespaceName + "." + cspaceTypeName;
		base.Abstract = clrType.IsAbstract();
	}

	internal static ClrComplexType CreateReadonlyClrComplexType(Type clrType, string cspaceNamespaceName, string cspaceTypeName)
	{
		ClrComplexType clrComplexType = new ClrComplexType(clrType, cspaceNamespaceName, cspaceTypeName);
		clrComplexType.SetReadOnly();
		return clrComplexType;
	}
}

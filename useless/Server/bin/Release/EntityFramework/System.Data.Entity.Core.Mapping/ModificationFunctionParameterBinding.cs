using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.Mapping;

public sealed class ModificationFunctionParameterBinding : MappingItem
{
	private readonly FunctionParameter _parameter;

	private readonly ModificationFunctionMemberPath _memberPath;

	private readonly bool _isCurrent;

	public FunctionParameter Parameter => _parameter;

	public ModificationFunctionMemberPath MemberPath => _memberPath;

	public bool IsCurrent => _isCurrent;

	public ModificationFunctionParameterBinding(FunctionParameter parameter, ModificationFunctionMemberPath memberPath, bool isCurrent)
	{
		Check.NotNull(parameter, "parameter");
		Check.NotNull(memberPath, "memberPath");
		_parameter = parameter;
		_memberPath = memberPath;
		_isCurrent = isCurrent;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "@{0}->{1}{2}", new object[3]
		{
			Parameter,
			IsCurrent ? "+" : "-",
			MemberPath
		});
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(_memberPath);
		base.SetReadOnly();
	}
}

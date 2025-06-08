using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawGenericParamConstraintRow
{
	public readonly uint Owner;

	public readonly uint Constraint;

	public uint this[int index] => index switch
	{
		0 => Owner, 
		1 => Constraint, 
		_ => 0u, 
	};

	public RawGenericParamConstraintRow(uint Owner, uint Constraint)
	{
		this.Owner = Owner;
		this.Constraint = Constraint;
	}
}

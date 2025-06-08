using System;

namespace dnlib.DotNet;

[Flags]
public enum GenericParamAttributes : ushort
{
	VarianceMask = 3,
	NonVariant = 0,
	Covariant = 1,
	Contravariant = 2,
	SpecialConstraintMask = 0x1C,
	NoSpecialConstraint = 0,
	ReferenceTypeConstraint = 4,
	NotNullableValueTypeConstraint = 8,
	DefaultConstructorConstraint = 0x10
}

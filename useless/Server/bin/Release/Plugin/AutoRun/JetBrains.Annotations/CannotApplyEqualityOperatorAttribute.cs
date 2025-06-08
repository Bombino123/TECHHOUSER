using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
internal sealed class CannotApplyEqualityOperatorAttribute : Attribute
{
}

using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class PureAttribute : Attribute
{
}

using System;

namespace JetBrains.Annotations;

[Obsolete("Use [ContractAnnotation('=> halt')] instead")]
[AttributeUsage(AttributeTargets.Method)]
internal sealed class TerminatesProgramAttribute : Attribute
{
}

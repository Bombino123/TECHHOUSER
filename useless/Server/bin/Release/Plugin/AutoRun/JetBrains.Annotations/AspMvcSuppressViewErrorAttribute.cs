using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class AspMvcSuppressViewErrorAttribute : Attribute
{
}

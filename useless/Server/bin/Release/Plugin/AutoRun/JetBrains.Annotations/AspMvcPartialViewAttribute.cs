using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
internal sealed class AspMvcPartialViewAttribute : Attribute
{
}

using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
internal sealed class AspMvcActionSelectorAttribute : Attribute
{
}

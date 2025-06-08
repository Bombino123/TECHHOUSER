using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
internal sealed class HtmlAttributeValueAttribute : Attribute
{
	[NotNull]
	public string Name { get; private set; }

	public HtmlAttributeValueAttribute([NotNull] string name)
	{
		Name = name;
	}
}

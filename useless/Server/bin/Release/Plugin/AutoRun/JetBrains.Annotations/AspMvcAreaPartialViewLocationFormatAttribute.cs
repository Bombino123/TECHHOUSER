using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
internal sealed class AspMvcAreaPartialViewLocationFormatAttribute : Attribute
{
	[NotNull]
	public string Format { get; private set; }

	public AspMvcAreaPartialViewLocationFormatAttribute([NotNull] string format)
	{
		Format = format;
	}
}

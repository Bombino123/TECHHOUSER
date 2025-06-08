using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.All)]
internal sealed class LocalizationRequiredAttribute : Attribute
{
	public bool Required { get; private set; }

	public LocalizationRequiredAttribute()
		: this(required: true)
	{
	}

	public LocalizationRequiredAttribute(bool required)
	{
		Required = required;
	}
}

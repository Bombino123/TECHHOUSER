using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
{
	[CanBeNull]
	public string ParameterName { get; private set; }

	public NotifyPropertyChangedInvocatorAttribute()
	{
	}

	public NotifyPropertyChangedInvocatorAttribute([NotNull] string parameterName)
	{
		ParameterName = parameterName;
	}
}

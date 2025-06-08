using System;

namespace JetBrains.Annotations;

[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
internal sealed class PublicAPIAttribute : Attribute
{
	[CanBeNull]
	public string Comment { get; private set; }

	public PublicAPIAttribute()
	{
	}

	public PublicAPIAttribute([NotNull] string comment)
	{
		Comment = comment;
	}
}

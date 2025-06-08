using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class AssociationTypeExtensions
{
	private const string IsIndependentAnnotation = "IsIndependent";

	private const string IsPrincipalConfiguredAnnotation = "IsPrincipalConfigured";

	public static void MarkIndependent(this AssociationType associationType)
	{
		associationType.GetMetadataProperties().SetAnnotation("IsIndependent", true);
	}

	public static bool IsIndependent(this AssociationType associationType)
	{
		object annotation = associationType.Annotations.GetAnnotation("IsIndependent");
		if (annotation != null)
		{
			return (bool)annotation;
		}
		return false;
	}

	public static void MarkPrincipalConfigured(this AssociationType associationType)
	{
		associationType.GetMetadataProperties().SetAnnotation("IsPrincipalConfigured", true);
	}

	public static bool IsPrincipalConfigured(this AssociationType associationType)
	{
		object annotation = associationType.Annotations.GetAnnotation("IsPrincipalConfigured");
		if (annotation != null)
		{
			return (bool)annotation;
		}
		return false;
	}

	public static AssociationEndMember GetOtherEnd(this AssociationType associationType, AssociationEndMember associationEnd)
	{
		if (associationEnd != associationType.SourceEnd)
		{
			return associationType.SourceEnd;
		}
		return associationType.TargetEnd;
	}

	public static object GetConfiguration(this AssociationType associationType)
	{
		return associationType.Annotations.GetConfiguration();
	}

	public static void SetConfiguration(this AssociationType associationType, object configuration)
	{
		associationType.GetMetadataProperties().SetConfiguration(configuration);
	}

	public static bool IsRequiredToMany(this AssociationType associationType)
	{
		if (associationType.SourceEnd.IsRequired())
		{
			return associationType.TargetEnd.IsMany();
		}
		return false;
	}

	public static bool IsRequiredToRequired(this AssociationType associationType)
	{
		if (associationType.SourceEnd.IsRequired())
		{
			return associationType.TargetEnd.IsRequired();
		}
		return false;
	}

	public static bool IsManyToRequired(this AssociationType associationType)
	{
		if (associationType.SourceEnd.IsMany())
		{
			return associationType.TargetEnd.IsRequired();
		}
		return false;
	}

	public static bool IsManyToMany(this AssociationType associationType)
	{
		if (associationType.SourceEnd.IsMany())
		{
			return associationType.TargetEnd.IsMany();
		}
		return false;
	}

	public static bool IsOneToOne(this AssociationType associationType)
	{
		if (!associationType.SourceEnd.IsMany())
		{
			return !associationType.TargetEnd.IsMany();
		}
		return false;
	}

	public static bool IsSelfReferencing(this AssociationType associationType)
	{
		AssociationEndMember sourceEnd = associationType.SourceEnd;
		AssociationEndMember targetEnd = associationType.TargetEnd;
		return sourceEnd.GetEntityType().GetRootType() == targetEnd.GetEntityType().GetRootType();
	}

	public static bool IsRequiredToNonRequired(this AssociationType associationType)
	{
		if (!associationType.SourceEnd.IsRequired() || associationType.TargetEnd.IsRequired())
		{
			if (associationType.TargetEnd.IsRequired())
			{
				return !associationType.SourceEnd.IsRequired();
			}
			return false;
		}
		return true;
	}

	public static bool TryGuessPrincipalAndDependentEnds(this AssociationType associationType, out AssociationEndMember principalEnd, out AssociationEndMember dependentEnd)
	{
		principalEnd = (dependentEnd = null);
		AssociationEndMember sourceEnd = associationType.SourceEnd;
		AssociationEndMember targetEnd = associationType.TargetEnd;
		if (sourceEnd.RelationshipMultiplicity != targetEnd.RelationshipMultiplicity)
		{
			principalEnd = ((sourceEnd.IsRequired() || (sourceEnd.IsOptional() && targetEnd.IsMany())) ? sourceEnd : targetEnd);
			dependentEnd = ((principalEnd == sourceEnd) ? targetEnd : sourceEnd);
		}
		return principalEnd != null;
	}
}

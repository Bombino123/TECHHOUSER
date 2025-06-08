using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class ForeignKeyBuilderExtensions
{
	private const string IsTypeConstraint = "IsTypeConstraint";

	private const string IsSplitConstraint = "IsSplitConstraint";

	private const string AssociationType = "AssociationType";

	private const string PreferredNameAnnotation = "PreferredName";

	public static string GetPreferredName(this ForeignKeyBuilder fk)
	{
		return (string)fk.Annotations.GetAnnotation("PreferredName");
	}

	public static void SetPreferredName(this ForeignKeyBuilder fk, string name)
	{
		fk.GetMetadataProperties().SetAnnotation("PreferredName", name);
	}

	public static bool GetIsTypeConstraint(this ForeignKeyBuilder fk)
	{
		object annotation = fk.Annotations.GetAnnotation("IsTypeConstraint");
		if (annotation != null)
		{
			return (bool)annotation;
		}
		return false;
	}

	public static void SetIsTypeConstraint(this ForeignKeyBuilder fk)
	{
		fk.GetMetadataProperties().SetAnnotation("IsTypeConstraint", true);
	}

	public static void SetIsSplitConstraint(this ForeignKeyBuilder fk)
	{
		fk.GetMetadataProperties().SetAnnotation("IsSplitConstraint", true);
	}

	public static AssociationType GetAssociationType(this ForeignKeyBuilder fk)
	{
		return fk.Annotations.GetAnnotation("AssociationType") as AssociationType;
	}

	public static void SetAssociationType(this ForeignKeyBuilder fk, AssociationType associationType)
	{
		fk.GetMetadataProperties().SetAnnotation("AssociationType", associationType);
	}
}

using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

public sealed class AssociationSet : RelationshipSet
{
	private readonly ReadOnlyMetadataCollection<AssociationSetEnd> _associationSetEnds = new ReadOnlyMetadataCollection<AssociationSetEnd>(new MetadataCollection<AssociationSetEnd>());

	public new AssociationType ElementType => (AssociationType)base.ElementType;

	[MetadataProperty(BuiltInTypeKind.AssociationSetEnd, true)]
	public ReadOnlyMetadataCollection<AssociationSetEnd> AssociationSetEnds => _associationSetEnds;

	internal EntitySet SourceSet
	{
		get
		{
			return AssociationSetEnds.FirstOrDefault()?.EntitySet;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			AssociationSetEnd associationSetEnd = new AssociationSetEnd(value, this, ElementType.SourceEnd);
			if (AssociationSetEnds.Count == 0)
			{
				AddAssociationSetEnd(associationSetEnd);
			}
			else
			{
				AssociationSetEnds.Source[0] = associationSetEnd;
			}
		}
	}

	internal EntitySet TargetSet
	{
		get
		{
			return AssociationSetEnds.ElementAtOrDefault(1)?.EntitySet;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			AssociationSetEnd associationSetEnd = new AssociationSetEnd(value, this, ElementType.TargetEnd);
			if (AssociationSetEnds.Count == 1)
			{
				AddAssociationSetEnd(associationSetEnd);
			}
			else
			{
				AssociationSetEnds.Source[1] = associationSetEnd;
			}
		}
	}

	internal AssociationEndMember SourceEnd
	{
		get
		{
			AssociationSetEnd associationSetEnd = AssociationSetEnds.FirstOrDefault();
			if (associationSetEnd == null)
			{
				return null;
			}
			return ElementType.KeyMembers.OfType<AssociationEndMember>().SingleOrDefault((AssociationEndMember e) => e.Name == associationSetEnd.Name);
		}
	}

	internal AssociationEndMember TargetEnd
	{
		get
		{
			AssociationSetEnd associationSetEnd = AssociationSetEnds.ElementAtOrDefault(1);
			if (associationSetEnd == null)
			{
				return null;
			}
			return ElementType.KeyMembers.OfType<AssociationEndMember>().SingleOrDefault((AssociationEndMember e) => e.Name == associationSetEnd.Name);
		}
	}

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.AssociationSet;

	internal AssociationSet(string name, AssociationType associationType)
		: base(name, null, null, null, associationType)
	{
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
			AssociationSetEnds.Source.SetReadOnly();
		}
	}

	internal void AddAssociationSetEnd(AssociationSetEnd associationSetEnd)
	{
		AssociationSetEnds.Source.Add(associationSetEnd);
	}

	public static AssociationSet Create(string name, AssociationType type, EntitySet sourceSet, EntitySet targetSet, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(type, "type");
		if (!CheckEntitySetAgainstEndMember(sourceSet, type.SourceEnd) || !CheckEntitySetAgainstEndMember(targetSet, type.TargetEnd))
		{
			throw new ArgumentException(Strings.AssociationSet_EndEntityTypeMismatch);
		}
		AssociationSet associationSet = new AssociationSet(name, type);
		if (sourceSet != null)
		{
			associationSet.SourceSet = sourceSet;
		}
		if (targetSet != null)
		{
			associationSet.TargetSet = targetSet;
		}
		if (metadataProperties != null)
		{
			associationSet.AddMetadataProperties(metadataProperties);
		}
		associationSet.SetReadOnly();
		return associationSet;
	}

	private static bool CheckEntitySetAgainstEndMember(EntitySet entitySet, AssociationEndMember endMember)
	{
		if (entitySet != null || endMember != null)
		{
			if (entitySet != null && endMember != null)
			{
				return entitySet.ElementType == endMember.GetEntityType();
			}
			return false;
		}
		return true;
	}
}

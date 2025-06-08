using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public class AssociationType : RelationshipType
{
	internal volatile int Index = -1;

	private readonly ReadOnlyMetadataCollection<ReferentialConstraint> _referentialConstraints;

	private FilteredReadOnlyMetadataCollection<AssociationEndMember, EdmMember> _associationEndMembers;

	private bool _isForeignKey;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.AssociationType;

	public ReadOnlyMetadataCollection<AssociationEndMember> AssociationEndMembers
	{
		get
		{
			if (_associationEndMembers == null)
			{
				Interlocked.CompareExchange(ref _associationEndMembers, new FilteredReadOnlyMetadataCollection<AssociationEndMember, EdmMember>(KeyMembers, Helper.IsAssociationEndMember), null);
			}
			return _associationEndMembers;
		}
	}

	public ReferentialConstraint Constraint
	{
		get
		{
			return ReferentialConstraints.SingleOrDefault();
		}
		set
		{
			Check.NotNull(value, "value");
			Util.ThrowIfReadOnly(this);
			ReferentialConstraint constraint = Constraint;
			if (constraint != null)
			{
				ReferentialConstraints.Source.Remove(constraint);
			}
			AddReferentialConstraint(value);
			_isForeignKey = true;
		}
	}

	internal AssociationEndMember SourceEnd
	{
		get
		{
			return KeyMembers.FirstOrDefault() as AssociationEndMember;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			if (KeyMembers.Count == 0)
			{
				AddKeyMember(value);
			}
			else
			{
				SetKeyMember(0, value);
			}
		}
	}

	internal AssociationEndMember TargetEnd
	{
		get
		{
			return KeyMembers.ElementAtOrDefault(1) as AssociationEndMember;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			if (KeyMembers.Count == 1)
			{
				AddKeyMember(value);
			}
			else
			{
				SetKeyMember(1, value);
			}
		}
	}

	[MetadataProperty(BuiltInTypeKind.ReferentialConstraint, true)]
	public ReadOnlyMetadataCollection<ReferentialConstraint> ReferentialConstraints => _referentialConstraints;

	[MetadataProperty(PrimitiveTypeKind.Boolean, false)]
	public bool IsForeignKey => _isForeignKey;

	internal AssociationType(string name, string namespaceName, bool foreignKey, DataSpace dataSpace)
		: base(name, namespaceName, dataSpace)
	{
		_referentialConstraints = new ReadOnlyMetadataCollection<ReferentialConstraint>(new MetadataCollection<ReferentialConstraint>());
		_isForeignKey = foreignKey;
	}

	private void SetKeyMember(int index, AssociationEndMember member)
	{
		EdmMember value = KeyMembers.Source[index];
		int num = base.Members.IndexOf(value);
		if (num >= 0)
		{
			base.Members.Source[num] = member;
		}
		KeyMembers.Source[index] = member;
	}

	internal override void ValidateMemberForAdd(EdmMember member)
	{
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
			ReferentialConstraints.Source.SetReadOnly();
		}
	}

	internal void AddReferentialConstraint(ReferentialConstraint referentialConstraint)
	{
		ReferentialConstraints.Source.Add(referentialConstraint);
	}

	public static AssociationType Create(string name, string namespaceName, bool foreignKey, DataSpace dataSpace, AssociationEndMember sourceEnd, AssociationEndMember targetEnd, ReferentialConstraint constraint, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(namespaceName, "namespaceName");
		AssociationType associationType = new AssociationType(name, namespaceName, foreignKey, dataSpace);
		if (sourceEnd != null)
		{
			associationType.SourceEnd = sourceEnd;
		}
		if (targetEnd != null)
		{
			associationType.TargetEnd = targetEnd;
		}
		if (constraint != null)
		{
			associationType.AddReferentialConstraint(constraint);
		}
		if (metadataProperties != null)
		{
			associationType.AddMetadataProperties(metadataProperties);
		}
		associationType.SetReadOnly();
		return associationType;
	}
}

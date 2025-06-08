using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

public class AssociationTypeMapping : TypeMapping
{
	private readonly AssociationSetMapping _associationSetMapping;

	private MappingFragment _mappingFragment;

	private readonly AssociationType m_relation;

	public AssociationSetMapping AssociationSetMapping => _associationSetMapping;

	internal override EntitySetBaseMapping SetMapping => AssociationSetMapping;

	public AssociationType AssociationType => m_relation;

	public MappingFragment MappingFragment
	{
		get
		{
			return _mappingFragment;
		}
		internal set
		{
			_mappingFragment = value;
		}
	}

	internal override ReadOnlyCollection<MappingFragment> MappingFragments
	{
		get
		{
			if (_mappingFragment != null)
			{
				return new ReadOnlyCollection<MappingFragment>(new MappingFragment[1] { _mappingFragment });
			}
			return new ReadOnlyCollection<MappingFragment>(new MappingFragment[0]);
		}
	}

	internal override ReadOnlyCollection<EntityTypeBase> Types => new ReadOnlyCollection<EntityTypeBase>(new AssociationType[1] { m_relation });

	internal override ReadOnlyCollection<EntityTypeBase> IsOfTypes => new ReadOnlyCollection<EntityTypeBase>(new List<EntityTypeBase>());

	public AssociationTypeMapping(AssociationSetMapping associationSetMapping)
	{
		Check.NotNull(associationSetMapping, "associationSetMapping");
		_associationSetMapping = associationSetMapping;
		m_relation = associationSetMapping.AssociationSet.ElementType;
	}

	internal AssociationTypeMapping(AssociationType relation, AssociationSetMapping associationSetMapping)
	{
		_associationSetMapping = associationSetMapping;
		m_relation = relation;
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(_mappingFragment);
		base.SetReadOnly();
	}
}

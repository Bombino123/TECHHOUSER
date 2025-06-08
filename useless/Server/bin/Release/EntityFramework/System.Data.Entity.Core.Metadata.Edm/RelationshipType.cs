using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class RelationshipType : EntityTypeBase
{
	private ReadOnlyMetadataCollection<RelationshipEndMember> _relationshipEndMembers;

	public ReadOnlyMetadataCollection<RelationshipEndMember> RelationshipEndMembers
	{
		get
		{
			if (_relationshipEndMembers == null)
			{
				FilteredReadOnlyMetadataCollection<RelationshipEndMember, EdmMember> value = new FilteredReadOnlyMetadataCollection<RelationshipEndMember, EdmMember>(base.Members, Helper.IsRelationshipEndMember);
				Interlocked.CompareExchange(ref _relationshipEndMembers, value, null);
			}
			return _relationshipEndMembers;
		}
	}

	internal RelationshipType(string name, string namespaceName, DataSpace dataSpace)
		: base(name, namespaceName, dataSpace)
	{
	}
}

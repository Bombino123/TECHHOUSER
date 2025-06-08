using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Objects.DataClasses;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EdmRelationshipAttribute : Attribute
{
	private readonly string _relationshipNamespaceName;

	private readonly string _relationshipName;

	private readonly string _role1Name;

	private readonly string _role2Name;

	private readonly RelationshipMultiplicity _role1Multiplicity;

	private readonly RelationshipMultiplicity _role2Multiplicity;

	private readonly Type _role1Type;

	private readonly Type _role2Type;

	private readonly bool _isForeignKey;

	public string RelationshipNamespaceName => _relationshipNamespaceName;

	public string RelationshipName => _relationshipName;

	public string Role1Name => _role1Name;

	public RelationshipMultiplicity Role1Multiplicity => _role1Multiplicity;

	public Type Role1Type => _role1Type;

	public string Role2Name => _role2Name;

	public RelationshipMultiplicity Role2Multiplicity => _role2Multiplicity;

	public Type Role2Type => _role2Type;

	public bool IsForeignKey => _isForeignKey;

	public EdmRelationshipAttribute(string relationshipNamespaceName, string relationshipName, string role1Name, RelationshipMultiplicity role1Multiplicity, Type role1Type, string role2Name, RelationshipMultiplicity role2Multiplicity, Type role2Type)
	{
		_relationshipNamespaceName = relationshipNamespaceName;
		_relationshipName = relationshipName;
		_role1Name = role1Name;
		_role1Multiplicity = role1Multiplicity;
		_role1Type = role1Type;
		_role2Name = role2Name;
		_role2Multiplicity = role2Multiplicity;
		_role2Type = role2Type;
	}

	public EdmRelationshipAttribute(string relationshipNamespaceName, string relationshipName, string role1Name, RelationshipMultiplicity role1Multiplicity, Type role1Type, string role2Name, RelationshipMultiplicity role2Multiplicity, Type role2Type, bool isForeignKey)
	{
		_relationshipNamespaceName = relationshipNamespaceName;
		_relationshipName = relationshipName;
		_role1Name = role1Name;
		_role1Multiplicity = role1Multiplicity;
		_role1Type = role1Type;
		_role2Name = role2Name;
		_role2Multiplicity = role2Multiplicity;
		_role2Type = role2Type;
		_isForeignKey = isForeignKey;
	}
}

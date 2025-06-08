namespace System.Data.Entity.Core.Objects.DataClasses;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EdmRelationshipNavigationPropertyAttribute : EdmPropertyAttribute
{
	private readonly string _relationshipNamespaceName;

	private readonly string _relationshipName;

	private readonly string _targetRoleName;

	public string RelationshipNamespaceName => _relationshipNamespaceName;

	public string RelationshipName => _relationshipName;

	public string TargetRoleName => _targetRoleName;

	public EdmRelationshipNavigationPropertyAttribute(string relationshipNamespaceName, string relationshipName, string targetRoleName)
	{
		_relationshipNamespaceName = relationshipNamespaceName;
		_relationshipName = relationshipName;
		_targetRoleName = targetRoleName;
	}
}

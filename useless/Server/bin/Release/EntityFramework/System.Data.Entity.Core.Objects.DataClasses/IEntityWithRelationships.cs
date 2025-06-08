namespace System.Data.Entity.Core.Objects.DataClasses;

public interface IEntityWithRelationships
{
	RelationshipManager RelationshipManager { get; }
}
